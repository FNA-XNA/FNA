#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
using SDL2;
#endregion

/* References:
 * [1] https://developer.apple.com/metal/Metal-Feature-Set-Tables.pdf
 * [2] https://developer.apple.com/documentation/metal/mtldevice/1433355-supportstexturesamplecount
 */

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class MetalDevice : IGLDevice
	{
		#region Metal Texture Container Class

		private class MetalTexture : IGLTexture
		{
			public IntPtr Handle
			{
				get;
				private set;
			}

			public bool HasMipmaps
			{
				get;
				private set;
			}

			public MetalTexture(
				IntPtr handle,
				int levelCount
			) {
				Handle = handle;
				HasMipmaps = levelCount > 1;
			}
		}

		#endregion

		#region Metal Buffer Container Class

		private class MetalBuffer : IGLBuffer
		{
			public IntPtr Handle
			{
				get;
				private set;
			}

			public IntPtr Contents
			{
				get;
				private set;
			}

			public IntPtr BufferSize
			{
				get;
				private set;
			}

			public MetalBuffer(
				IntPtr handle,
				IntPtr bufferSize
			) {
				Handle = handle;
				Contents = mtlGetBufferContentsPtr(handle);
				BufferSize = bufferSize;
			}
		}

		#endregion

		#region Metal Effect Container Class

		private class MetalEffect : IGLEffect
		{
			public IntPtr EffectData
			{
				get;
				private set;
			}

			public IntPtr MTLEffectData
			{
				get;
				private set;
			}

			public MetalEffect(IntPtr effect, IntPtr mtlEffect)
			{
				EffectData = effect;
				MTLEffectData = mtlEffect;
			}
		}

		#endregion

		#region Metal Query Container Class

		private class MetalQuery : IGLQuery
		{
			public IntPtr Handle
			{
				get;
				private set;
			}

			public MetalQuery(IntPtr handle)
			{
				Handle = handle;
			}
		}

		#endregion

		#region Blending State Variables

		public Color BlendFactor
		{
			get
			{
				return blendColor;
			}
			set
			{
				if (value != blendColor)
				{
					blendColor = value;
					if (RenderCommandEncoder != IntPtr.Zero)
					{
						mtlSetBlendColor(
							RenderCommandEncoder,
							blendColor.R / 255f,
							blendColor.G / 255f,
							blendColor.B / 255f,
							blendColor.A / 255f
						);
					}
				}
			}
		}

		// FIXME: This feature is unsupported in Metal. What should we do?
		public int MultiSampleMask
		{
			get
			{
				return multisampleMask;
			}
			set
			{
				if (value != multisampleMask)
				{
					multisampleMask = value;
				}
			}
		}

		private Color blendColor = Color.Transparent;
		private int multisampleMask = -1; // AKA 0xFFFFFFFF

		#endregion

		#region Depth State Variables
		#endregion

		#region Stencil State Variables

		public int ReferenceStencil
		{
			get
			{
				return stencilRef;
			}
			set
			{
				if (value != stencilRef)
				{
					stencilRef = value;
					if (RenderCommandEncoder != IntPtr.Zero)
					{
						mtlSetStencilReferenceValue(
							RenderCommandEncoder,
							(uint) stencilRef
						);
					}
				}
			}
		}

		private int stencilRef = 0;

		#endregion

		#region Rasterizer State Variables
		#endregion

		#region Viewport State Variables

		/* These two aren't actually empty rects by default in Metal,
		 * but we don't _really_ know the starting window size, so
		 * force apply this when the GraphicsDevice is initialized.
		 */
		private Rectangle scissorRectangle = new Rectangle(
			0,
			0,
			0,
			0
		);
		private Rectangle viewport = new Rectangle(
			0,
			0,
			0,
			0
		);
		private float depthRangeMin = 0.0f;
		private float depthRangeMax = 1.0f;

		#endregion

		#region Sampler State Variables
		#endregion

		#region Buffer Binding Cache Variables
		#endregion

		#region Render Target Cache Variables
		#endregion

		#region Clear Cache Variables

		private Vector4 clearColor = new Vector4(0, 0, 0, 0);
		private float clearDepth = 1.0f;
		private int clearStencil = 0;

		#endregion

		#region Private Metal State Variables

		private IntPtr layer;			// CAMetalLayer*
		private IntPtr device;			// MTLDevice*
		private IntPtr queue;			// MTLCommandQueue*
		private IntPtr commandBuffer;		// MTLCommandBuffer*

		private IntPtr currentDrawable;		// CAMetalDrawable*
		private IntPtr currentColorBuffer;	// MTLTexture*
		private IntPtr currentDepthStencilBuffer; // MTLTexture*

		private bool renderPassDirty = false;
		private bool shouldClearColor = false;
		private bool shouldClearDepth = false;
		private bool shouldClearStencil = false;

		#endregion

		#region Private Metal State Properties

		private IntPtr RenderCommandEncoder;	// MTLRenderCommandEncoder*
		private IntPtr GetRenderCommandEncoder()
		{
			if (renderPassDirty)
			{
				// Wrap up rendering with the old encoder
				if (RenderCommandEncoder != IntPtr.Zero)
				{
					mtlEndEncoding(RenderCommandEncoder);
				}

				// Generate the descriptor
				IntPtr renderPassDesc = mtlMakeRenderPassDescriptor();

				// Clear color
				IntPtr colorAttachment = mtlGetColorAttachment(renderPassDesc, 0);
				mtlSetAttachmentTexture(colorAttachment, currentColorBuffer);
				if (shouldClearColor)
				{
					mtlSetAttachmentLoadAction(colorAttachment, MTLLoadAction.Clear);
					mtlSetColorAttachmentClearColor(
						colorAttachment,
						clearColor.X,
						clearColor.Y,
						clearColor.Z,
						clearColor.W
					);
					shouldClearColor = false;
				}
				else
				{
					mtlSetAttachmentLoadAction(colorAttachment, MTLLoadAction.Load);
				}

				// Clear depth
				IntPtr depthAttachment = mtlGetDepthAttachment(renderPassDesc);
				mtlSetAttachmentTexture(depthAttachment, currentDepthStencilBuffer);
				if (shouldClearDepth)
				{
					mtlSetAttachmentLoadAction(depthAttachment, MTLLoadAction.Clear);
					mtlSetDepthAttachmentClearDepth(depthAttachment, clearDepth);
					shouldClearDepth = false;
				}
				else
				{
					mtlSetAttachmentLoadAction(depthAttachment, MTLLoadAction.Load);
				}

				// Clear stencil
				IntPtr stencilAttachment = mtlGetStencilAttachment(renderPassDesc);
				mtlSetAttachmentTexture(stencilAttachment, currentDepthStencilBuffer);
				if (shouldClearStencil)
				{
					mtlSetAttachmentLoadAction(stencilAttachment, MTLLoadAction.Clear);
					mtlSetStencilAttachmentClearStencil(stencilAttachment, clearStencil);
					shouldClearStencil = false;
				}
				else
				{
					mtlSetAttachmentLoadAction(stencilAttachment, MTLLoadAction.Load);
				}

				// Make a new encoder
				RenderCommandEncoder = mtlMakeRenderCommandEncoder(
					commandBuffer,
					renderPassDesc
				);

				// Initialize the new encoder
				mtlSetViewport(
					RenderCommandEncoder,
					viewport.X,
					viewport.Y,
					viewport.Width,
					viewport.Height,
					depthRangeMin,
					depthRangeMax
				);

				mtlSetScissorRect(
					RenderCommandEncoder,
					(uint) scissorRectangle.X,
					(uint) scissorRectangle.Y,
					(uint) scissorRectangle.Width,
					(uint) scissorRectangle.Height
				);

				mtlSetBlendColor(
					RenderCommandEncoder,
					blendColor.R / 255f,
					blendColor.G / 255f,
					blendColor.B / 255f,
					blendColor.A / 255f
				);

				mtlSetStencilReferenceValue(
					RenderCommandEncoder,
					(ulong) stencilRef
				);

				// Reset the flag
				renderPassDirty = false;
			}

			return RenderCommandEncoder;
		}

		#endregion

		#region Objective-C Memory Management Variables

		private IntPtr pool;			// NSAutoreleasePool*

		#endregion

		#region Faux-Backbuffer Variables

		public IGLBackbuffer Backbuffer
		{
			get;
			private set;
		}

		private MTLSamplerMinMagFilter backbufferScaleMode;

		// Cached data for rendering the faux-backbuffer
		private Rectangle fauxBackbufferDestBounds;
		private IntPtr fauxBackbufferVertexBuffer;
		private IntPtr fauxBackbufferIndexBuffer;
		private IntPtr fauxBackbufferRenderPipeline;
		private IntPtr fauxBackbufferSamplerState;
		private bool fauxBackbufferSizeChanged;

		#endregion

		#region Metal Device Capabilities

		public bool SupportsDxt1
		{
			get;
			private set;
		}

		public bool SupportsS3tc
		{
			get;
			private set;
		}

		public bool SupportsHardwareInstancing
		{
			get;
			private set;
		}

		public int MaxTextureSlots
		{
			get;
			private set;
		}

		public int MaxMultiSampleCount
		{
			get;
			private set;
		}

		#endregion

		#region Private Render Pipeline State Cache
		#endregion

		#region Private Vertex Attribute Cache
		#endregion

		#region Private MojoShader Interop

		private IntPtr currentEffect = IntPtr.Zero;
		private IntPtr currentTechnique = IntPtr.Zero;
		private uint currentPass = 0;

		private bool effectApplied = false;

		// FIXME: Do these belong here?
		private IntPtr currentVertexShader = IntPtr.Zero;
		private IntPtr currentFragmentShader = IntPtr.Zero;

		#endregion

		#region memcpy Export

		/* This is used a lot for GetData/Read calls... -flibit */
#if NETSTANDARD2_0
		private static unsafe void memcpy(IntPtr dst, IntPtr src, IntPtr len)
		{
			long size = len.ToInt64();
			Buffer.MemoryCopy(
				(void*) src,
				(void*) dst,
				size,
				size
			);
		}
#else
		[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
		private static extern void memcpy(IntPtr dst, IntPtr src, IntPtr len);
#endif

		#endregion

		#region memset Export

		// FIXME: What is the .NET Standard version of this?
		[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
		private static extern void memset(IntPtr dst, IntPtr value, IntPtr size);

		#endregion

		#region Public Constructor

		public MetalDevice(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter,
			IntPtr metalLayer
		) {
			device = MTLCreateSystemDefaultDevice();
			queue = mtlMakeCommandQueue(device);
			commandBuffer = mtlMakeCommandBuffer(queue);

			// FIXME: Replace this with SDL_MTL_GetMetalLayer() or equivalent
			layer = metalLayer;

			// Get a reference to the drawable for this frame
			currentDrawable = mtlNextDrawable(layer);

			// Log GLDevice info
			FNALoggerEXT.LogInfo("IGLDevice: MetalDevice");
			FNALoggerEXT.LogInfo("Device Name: " + mtlGetDeviceName(device));
			FNALoggerEXT.LogInfo("MojoShader Profile: metal");

			// Some users might want pixely upscaling...
			backbufferScaleMode = Environment.GetEnvironmentVariable(
				"FNA_METAL_BACKBUFFER_SCALE_NEAREST"
			) == "1" ? MTLSamplerMinMagFilter.Nearest : MTLSamplerMinMagFilter.Linear;

			// Set device properties
			SupportsS3tc = SDL.SDL_GetPlatform().Equals("Mac OS X");
			SupportsDxt1 = SupportsS3tc;
			SupportsHardwareInstancing = true;
			MaxTextureSlots = 16;
			MaxMultiSampleCount = mtlSupportsSampleCount(device, 8) ? 8 : 4;

			// Force the creation of a render pass
			renderPassDirty = true;

			// Create and setup the faux-backbuffer
			InitializeFauxBackbuffer(presentationParameters);

			// Begin the autorelease pool
			pool = StartAutoreleasePool();
		}

		#endregion

		#region Dispose Method

		public void Dispose()
		{
			// FIXME: Drain NSAutorelease pool
			// FIXME: "release" all retained objects
			// FIXME: Delete the faux back buffer
			// FIXME: null-ify variables
		}

		#endregion

		#region Window Backbuffer Reset Method

		public void ResetBackbuffer(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter,
			bool renderTargetBound
		) {
			if (Backbuffer is NullBackbuffer)
			{
				Backbuffer = new MetalBackbuffer(
					this,
					presentationParameters.BackBufferWidth,
					presentationParameters.BackBufferHeight,
					presentationParameters.DepthStencilFormat,
					presentationParameters.MultiSampleCount
				);
			}
			else
			{
				Backbuffer.ResetFramebuffer(
					presentationParameters,
					renderTargetBound
				);
			}
		}

		#endregion

		#region Window SwapBuffers Method

		public void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			// Finish the render pass
			if (GetRenderCommandEncoder() != IntPtr.Zero)
			{
				mtlEndEncoding(RenderCommandEncoder);
				RenderCommandEncoder = IntPtr.Zero;

			}

			// Perform a pass for the MSAA resolve texture, if applicable
			IntPtr colorBuffer = (Backbuffer as MetalBackbuffer).ColorBuffer;
			if (Backbuffer.MultiSampleCount > 0)
			{
				// Generate temp texture for resolving the actual backbuffer
				IntPtr resolveTextureDesc = mtlMakeTexture2DDescriptor(
					mtlGetLayerPixelFormat(layer),
					(uint) Backbuffer.Width,
					(uint) Backbuffer.Height,
					false
				);
				mtlSetTextureType(
					resolveTextureDesc,
					MTLTextureType.Texture2D
				);
				mtlSetTextureUsage(
					resolveTextureDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);

				IntPtr resolveTexture = mtlNewTextureWithDescriptor(device, resolveTextureDesc);
				IntPtr resolveRenderPass = mtlMakeRenderPassDescriptor();
				IntPtr colorAttachment = mtlGetColorAttachment(resolveRenderPass, 0);

				mtlSetAttachmentStoreAction(
					colorAttachment,
					MTLStoreAction.MultisampleResolve
				);
				mtlSetAttachmentTexture(
					colorAttachment,
					colorBuffer
				);
				mtlSetAttachmentResolveTexture(
					colorAttachment,
					resolveTexture
				);

				IntPtr rce = mtlMakeRenderCommandEncoder(commandBuffer, resolveRenderPass);
				mtlEndEncoding(rce);

				colorBuffer = resolveTexture;
			}

			// Determine the regions to present
			int srcX, srcY, srcW, srcH;
			int dstX, dstY, dstW, dstH;
			if (sourceRectangle.HasValue)
			{
				srcX = sourceRectangle.Value.X;
				srcY = sourceRectangle.Value.Y;
				srcW = sourceRectangle.Value.Width;
				srcH = sourceRectangle.Value.Height;
			}
			else
			{
				srcX = 0;
				srcY = 0;
				srcW = Backbuffer.Width;
				srcH = Backbuffer.Height;
			}
			if (destinationRectangle.HasValue)
			{
				dstX = destinationRectangle.Value.X;
				dstY = destinationRectangle.Value.Y;
				dstW = destinationRectangle.Value.Width;
				dstH = destinationRectangle.Value.Height;
			}
			else
			{
				dstX = 0;
				dstY = 0;
				MTL_GetDrawableSize(layer, out dstW, out dstH);
			}

			CopyTextureRegion(
				colorBuffer,
				new Rectangle(srcX, srcY, srcW, srcH),
				mtlGetTextureFromDrawable(currentDrawable),
				new Rectangle(dstX, dstY, dstW, dstH)
			);

			mtlPresentDrawable(commandBuffer, currentDrawable);
			mtlCommitCommandBuffer(commandBuffer);

			// Release allocations from this frame
			DrainAutoreleasePool(pool);

			// The cycle begins anew...
			pool = StartAutoreleasePool();
			commandBuffer = mtlMakeCommandBuffer(queue);
			currentDrawable = mtlNextDrawable(layer);
			renderPassDirty = true;
			RenderCommandEncoder = IntPtr.Zero;
		}

		private void CopyTextureRegion(
			IntPtr srcTex,
			Rectangle srcRect,
			IntPtr dstTex,
			Rectangle dstRect
		) {
			if (srcRect.Width == 0 || srcRect.Height == 0 || dstRect.Width == 0 || dstRect.Height == 0)
			{
				// FIXME: OpenGL lets this slide, but what does XNA do here?
				throw new InvalidOperationException(
					"sourceRectangle and destinationRectangle must have non-zero width and height!"
				);
			}

			// Can we just blit?
			if (srcRect.Width == dstRect.Width && srcRect.Height == dstRect.Height)
			{
				IntPtr bce = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					bce,
					srcTex,
					0,
					0,
					new MTLOrigin((ulong) srcRect.X, (ulong) srcRect.Y, 0),
					new MTLSize((ulong) srcRect.Width, (ulong) srcRect.Height, 1),
					dstTex,
					0,
					0,
					new MTLOrigin((ulong) dstRect.X, (ulong) dstRect.Y, 0)
				);
				mtlEndEncoding(bce);
			}
			else
			{
				/* Metal doesn't have a way to blit to a destination rect,
				 * so we get to render it ourselves instead. Yayyy...
				 * -caleb
				 */

				IntPtr backbufferRenderPass = mtlMakeRenderPassDescriptor();
				mtlSetAttachmentTexture(
					mtlGetColorAttachment(backbufferRenderPass, 0),
					dstTex
				);

				IntPtr rce = mtlMakeRenderCommandEncoder(
					commandBuffer,
					backbufferRenderPass
				);
				mtlSetRenderPipelineState(
					rce,
					fauxBackbufferRenderPipeline
				);

				// Update cached vertex buffer if needed
				if (fauxBackbufferDestBounds != dstRect || fauxBackbufferSizeChanged)
				{
					fauxBackbufferDestBounds = dstRect;
					fauxBackbufferSizeChanged = false;

					// Scale the coordinates to (-1, 1)
					int dw, dh;
					MTL_GetDrawableSize(layer, out dw, out dh);
					float sx = -1 + (dstRect.X / (float) dw);
					float sy = -1 + (dstRect.Y / (float) dh);
					float sw = (dstRect.Width / (float) dw) * 2;
					float sh = (dstRect.Height / (float) dh) * 2;

					// Update the vertex buffer contents
					float[] data = new float[]
					{
						sx, sy,			0, 0,
						sx + sw, sy,		1, 0,
						sx + sw, sy + sh,	1, 1,
						sx, sy + sh,		0, 1
					};
					memcpy(
						mtlGetBufferContentsPtr(fauxBackbufferVertexBuffer),
						Marshal.UnsafeAddrOfPinnedArrayElement(data, 0),
						(IntPtr) (16 * sizeof(float))
					);
				}

				mtlSetVertexBuffer(
					rce,
					fauxBackbufferVertexBuffer,
					0,
					0
				);

				mtlSetFragmentTexture(
					rce,
					srcTex,
					0
				);

				mtlSetFragmentSamplerState(
					rce,
					fauxBackbufferSamplerState,
					0
				);

				mtlDrawIndexedPrimitives(
					rce,
					MTLPrimitiveType.Triangle,
					6,
					MTLIndexType.UInt16,
					fauxBackbufferIndexBuffer,
					0,
					1,
					0,
					0
				);

				mtlEndEncoding(rce);
			}
		}

		#endregion

		#region Metal Object Disposal Wrappers

		public void AddDisposeEffect(IGLEffect effect)
		{
			throw new NotImplementedException();
		}

		public void AddDisposeIndexBuffer(IGLBuffer buffer)
		{
			throw new NotImplementedException();
		}

		public void AddDisposeQuery(IGLQuery query)
		{
			throw new NotImplementedException();
		}

		public void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			throw new NotImplementedException();
		}

		public void AddDisposeTexture(IGLTexture texture)
		{
			if (texture != null)
			{
				ObjCRelease((texture as MetalTexture).Handle);
			}
		}

		public void AddDisposeVertexBuffer(IGLBuffer buffer)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region String Marker Method

		public void SetStringMarker(string text)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Drawing Methods

		public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, IndexBuffer indices)
		{
			throw new NotImplementedException();
		}

		public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount, int instanceCount, IndexBuffer indices)
		{
			throw new NotImplementedException();
		}

		public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
		{
			throw new NotImplementedException();
		}

		public void DrawUserIndexedPrimitives(PrimitiveType primitiveType, IntPtr vertexData, int vertexOffset, int numVertices, IntPtr indexData, int indexOffset, IndexElementSize indexElementSize, int primitiveCount)
		{
			throw new NotImplementedException();
		}

		public void DrawUserPrimitives(PrimitiveType primitiveType, IntPtr vertexData, int vertexOffset, int primitiveCount)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region State Management Methods

		public void SetPresentationInterval(PresentInterval interval)
		{
			if (interval == PresentInterval.Default || interval == PresentInterval.One)
			{
				// FIXME: Enable vsync
			}
			else if (interval == PresentInterval.Immediate)
			{
				// FIXME: Turn off vsync
			}
			else if (interval == PresentInterval.Two)
			{
				// FIXME: Enable vsync. Only draw buffer every other frame.
			}
			else
			{
				throw new NotSupportedException("Unrecognized PresentInterval!");
			}
			
		}

		public void SetViewport(Viewport vp, bool renderTargetBound)
		{
			if (vp.Bounds != viewport
				|| vp.MinDepth != depthRangeMin
				|| vp.MaxDepth != depthRangeMax	)
			{
				viewport = vp.Bounds;
				depthRangeMin = vp.MinDepth;
				depthRangeMax = vp.MaxDepth;

				if (RenderCommandEncoder != IntPtr.Zero)
				{
					mtlSetViewport(
						RenderCommandEncoder,
						viewport.X,
						viewport.Y,
						viewport.Width,
						viewport.Height,
						(double) depthRangeMin,
						(double) depthRangeMax
					);
				}
			}

			/* Note: We don't need to flip the viewport,
			 * so we have no reason to use renderTargetBound.
			 * -caleb
			 */
		}

		public void SetScissorRect(Rectangle scissorRect, bool renderTargetBound)
		{
			if (scissorRect != scissorRectangle)
			{
				scissorRectangle = scissorRect;
				if (RenderCommandEncoder != IntPtr.Zero)
				{
					mtlSetScissorRect(
						RenderCommandEncoder,
						(uint) scissorRectangle.X,
						(uint) scissorRectangle.Y,
						(uint) scissorRectangle.Width,
						(uint) scissorRectangle.Height
					);
				}
			}

			/* Note: We don't need to flip the rectangle,
			 * so we have no reason to use renderTargetBound.
			 * -caleb
			 */
		}

		public void ApplyRasterizerState(RasterizerState rasterizerState, bool renderTargetBound)
		{
			throw new NotImplementedException();
		}

		public void VerifySampler(int index, Texture texture, SamplerState sampler)
		{
			throw new NotImplementedException();
		}

		public void SetBlendState(BlendState blendState)
		{
			throw new NotImplementedException();
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Effect Methods

		public IGLEffect CreateEffect(byte[] effectCode)
		{
			IntPtr effect = IntPtr.Zero;
			IntPtr mtlEffect = IntPtr.Zero;

			effect = MojoShader.MOJOSHADER_parseEffect(
				"metal",
				effectCode,
				(uint) effectCode.Length,
				null,
				0,
				null,
				0,
				null,
				null,
				IntPtr.Zero
			);

#if DEBUG
			unsafe
			{
				MojoShader.MOJOSHADER_effect *effectPtr = (MojoShader.MOJOSHADER_effect*) effect;
				MojoShader.MOJOSHADER_error* err = (MojoShader.MOJOSHADER_error*) effectPtr->errors;
				for (int i = 0; i < effectPtr->error_count; i += 1)
				{
					// From the SDL2# LPToUtf8StringMarshaler
					byte* endPtr = (byte*) err[i].error;
					while (*endPtr != 0)
					{
						endPtr++;
					}
					byte[] bytes = new byte[endPtr - (byte*) err[i].error];
					Marshal.Copy(err[i].error, bytes, 0, bytes.Length);

					FNALoggerEXT.LogError(
						"MOJOSHADER_parseEffect Error: " +
						System.Text.Encoding.UTF8.GetString(bytes)
					);
				}
			}
#endif
			mtlEffect = MojoShader.MOJOSHADER_mtlCompileEffect(effect, device);
			if (mtlEffect == IntPtr.Zero)
			{
				throw new InvalidOperationException(
					MojoShader.MOJOSHADER_mtlGetError()
				);
			}

			return new MetalEffect(effect, mtlEffect);
		}
		
		public IGLEffect CloneEffect(IGLEffect effect)
		{
			throw new NotImplementedException();
		}

		public void ApplyEffect(
			IGLEffect effect,
			IntPtr technique,
			uint pass,
			IntPtr stateChanges
		) {
			effectApplied = true;
			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			if (mtlEffectData == currentEffect)
			{
				if (technique == currentTechnique && pass == currentPass)
				{
					MojoShader.MOJOSHADER_mtlEffectCommitChanges(
						currentEffect,
						out currentVertexShader,
						out currentFragmentShader
					);
					return;
				}
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectBeginPass(
					currentEffect,
					pass,
					out currentVertexShader,
					out currentFragmentShader
				);
				currentTechnique = technique;
				currentPass = pass;
				return;
			}
			else if (currentEffect != IntPtr.Zero)
			{
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectEnd(
					currentEffect,
					out currentVertexShader,
					out currentFragmentShader
				);
			}
			uint whatever;
			MojoShader.MOJOSHADER_mtlEffectBegin(
				mtlEffectData,
				out whatever,
				0,
				stateChanges
			);
			MojoShader.MOJOSHADER_mtlEffectBeginPass(
				mtlEffectData,
				pass,
				out currentVertexShader,
				out currentFragmentShader
			);
			currentEffect = mtlEffectData;
			currentTechnique = technique;
			currentPass = pass;
		}

		public void BeginPassRestore(IGLEffect effect, IntPtr stateChanges)
		{
			throw new NotImplementedException();
		}

		public void EndPassRestore(IGLEffect effect)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ApplyVertexAttributes Methods

		public void ApplyVertexAttributes(VertexBufferBinding[] bindings, int numBindings, bool bindingsUpdated, int baseVertex)
		{
			throw new NotImplementedException();
		}

		public void ApplyVertexAttributes(VertexDeclaration vertexDeclaration, IntPtr ptr, int vertexOffset)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region glGenBuffers Methods

		public IGLBuffer GenIndexBuffer(
			bool dynamic,
			int indexCount,
			IndexElementSize indexElementSize
		) {
			IntPtr size = (IntPtr) (indexCount * XNAToMTL.IndexSize[(int) indexElementSize]);
			IntPtr buf = mtlNewBufferWithLength(device, (uint) size);
			return new MetalBuffer(buf, size);

			/* No need to use memset since the buffer is zero-filled by default.
			 * Additionally, we have no reason to use the dynamic flag here.
			 * -caleb
			 */
		}

		public IGLRenderbuffer GenRenderbuffer(int width, int height, SurfaceFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		public IGLRenderbuffer GenRenderbuffer(int width, int height, DepthFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		public IGLBuffer GenVertexBuffer(
			bool dynamic,
			int vertexCount,
			int vertexStride
		) {
			IntPtr size = (IntPtr) (vertexCount * vertexStride);
			IntPtr buf = mtlNewBufferWithLength(device, (uint) size);
			return new MetalBuffer(buf, size);

			/* No need to use memset since the buffer is zero-filled by default.
			 * Additionally, we have no reason to use the dynamic flag here.
			 * -caleb
			 */
		}

		#endregion

		#region SetBufferData Methods

		public void SetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			if (options == SetDataOptions.Discard)
			{
				// Zero out the memory
				memset((buffer as MetalBuffer).Contents, (IntPtr) 0, buffer.BufferSize);
			}

			IntPtr dst = IntPtr.Add((buffer as MetalBuffer).Contents, offsetInBytes);
			memcpy(dst, data, (IntPtr) dataLength);
		}

		public void SetVertexBufferData(IGLBuffer buffer, int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region GetBufferData Methods

		public void GetIndexBufferData(IGLBuffer buffer, int offsetInBytes, IntPtr data, int startIndex, int elementCount, int elementSizeInBytes)
		{
			throw new NotImplementedException();
		}

		public void GetVertexBufferData(IGLBuffer buffer, int offsetInBytes, IntPtr data, int startIndex, int elementCount, int elementSizeInBytes, int vertexStride)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region CreateTexture Methods

		public IGLTexture CreateTexture2D(SurfaceFormat format, int width, int height, int levelCount)
		{
			throw new NotImplementedException();
		}

		public IGLTexture CreateTexture3D(SurfaceFormat format, int width, int height, int depth, int levelCount)
		{
			throw new NotImplementedException();
		}

		public IGLTexture CreateTextureCube(SurfaceFormat format, int size, int levelCount)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region SetTextureData Methods

		public void SetTextureData2D(IGLTexture texture, SurfaceFormat format, int x, int y, int w, int h, int level, IntPtr data, int dataLength)
		{
			throw new NotImplementedException();
		}

		public void SetTextureData2DPointer(Texture2D texture, IntPtr ptr)
		{
			throw new NotImplementedException();
		}

		public void SetTextureData3D(IGLTexture texture, SurfaceFormat format, int level, int left, int top, int right, int bottom, int front, int back, IntPtr data, int dataLength)
		{
			throw new NotImplementedException();
		}

		public void SetTextureDataCube(IGLTexture texture, SurfaceFormat format, int xOffset, int yOffset, int width, int height, CubeMapFace cubeMapFace, int level, IntPtr data, int dataLength)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region GetTextureData Methods

		public void GetTextureData2D(IGLTexture texture, SurfaceFormat format, int width, int height, int level, int subX, int subY, int subW, int subH, IntPtr data, int startIndex, int elementCount, int elementSizeInBytes)
		{
			throw new NotImplementedException();
		}

		public void GetTextureData3D(IGLTexture texture, SurfaceFormat format, int left, int top, int front, int right, int bottom, int back, int level, IntPtr data, int startIndex, int elementCount, int elementSizeInBytes)
		{
			throw new NotImplementedException();
		}

		public void GetTextureDataCube(IGLTexture texture, SurfaceFormat format, int size, CubeMapFace cubeMapFace, int level, int subX, int subY, int subW, int subH, IntPtr data, int startIndex, int elementCount, int elementSizeInBytes)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ReadBackbuffer Method

		public void ReadBackbuffer(IntPtr data, int dataLen, int startIndex, int elementCount, int elementSizeInBytes, int subX, int subY, int subW, int subH)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region RenderTarget->Texture Method

		public void ResolveTarget(RenderTargetBinding target)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Clear Method

		public void Clear(
			ClearOptions options,
			Vector4 color,
			float depth,
			int stencil
		) {
			bool clearTarget = (options & ClearOptions.Target) == ClearOptions.Target;
			bool clearDepth = (options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer;
			bool clearStencil = (options & ClearOptions.Stencil) == ClearOptions.Stencil;

			if (clearTarget)
			{
				clearColor = color;
				shouldClearColor = true;
			}
			if (clearDepth)
			{
				this.clearDepth = depth;
				shouldClearDepth = true;
			}
			if (clearStencil)
			{
				this.clearStencil = stencil;
				shouldClearStencil = true;
			}

			renderPassDirty |= clearTarget | clearDepth | clearStencil;
		}

		#endregion

		#region SetRenderTargets Method

		public void SetRenderTargets(
			RenderTargetBinding[] renderTargets,
			IGLRenderbuffer renderbuffer,
			DepthFormat depthFormat
		) {
			throw new NotImplementedException();
		}

		#endregion

		#region Query Object Methods

		public IGLQuery CreateQuery()
		{
			throw new NotImplementedException();
		}

		public void QueryBegin(IGLQuery query)
		{
			throw new NotImplementedException();
		}

		public bool QueryComplete(IGLQuery query)
		{
			throw new NotImplementedException();
		}

		public void QueryEnd(IGLQuery query)
		{
			throw new NotImplementedException();
		}

		public int QueryPixelCount(IGLQuery query)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region XNA->GL Enum Conversion Class

		private static class XNAToMTL
		{
			public static readonly MTLPixelFormat[] TextureFormat = new MTLPixelFormat[]
			{
				MTLPixelFormat.RGBA8Unorm,		// SurfaceFormat.Color
				MTLPixelFormat.B5G6R5Unorm,		// SurfaceFormat.Bgr565
				MTLPixelFormat.BGR5A1Unorm,		// SurfaceFormat.Bgra5551
				MTLPixelFormat.ABGR4Unorm,		// SurfaceFormat.Bgra4444
				MTLPixelFormat.BC1_RGBA,		// SurfaceFormat.Dxt1
				MTLPixelFormat.BC2_RGBA,		// SurfaceFormat.Dxt3
				MTLPixelFormat.BC3_RGBA,		// SurfaceFormat.Dxt5
				MTLPixelFormat.RG8Snorm,		// SurfaceFormat.NormalizedByte2
				MTLPixelFormat.RG16Snorm,		// SurfaceFormat.NormalizedByte4
				MTLPixelFormat.RGB10A2Unorm,		// SurfaceFormat.Rgba1010102
				MTLPixelFormat.RG16Unorm,		// SurfaceFormat.Rg32
				MTLPixelFormat.RGBA16Unorm,		// SurfaceFormat.Rgba64
				MTLPixelFormat.A8Unorm,			// SurfaceFormat.Alpha8
				MTLPixelFormat.R32Float,		// SurfaceFormat.Single
				MTLPixelFormat.RG32Float,		// SurfaceFormat.Vector2
				MTLPixelFormat.RGBA32Float,		// SurfaceFormat.Vector4
				MTLPixelFormat.R16Float,		// SurfaceFormat.HalfSingle
				MTLPixelFormat.RG16Float,		// SurfaceFormat.HalfVector2
				MTLPixelFormat.RGBA16Float,		// SurfaceFormat.HalfVector4
				MTLPixelFormat.RGBA16Float,		// SurfaceFormat.HdrBlendable
				MTLPixelFormat.BGRA8Unorm		// SurfaceFormat.ColorBgraEXT
			};

			public static readonly MTLPixelFormat[] DepthStorage = new MTLPixelFormat[]
			{
				/* FIXME: Depth32Float is the only cross-platform depth format
				 * in Metal. Maybe we should check for feature set support so
				 * that we could use Depth24UnormStencil8 and Depth16Unorm.
				 */

				MTLPixelFormat.Invalid,			// NOPE
				MTLPixelFormat.Depth32Float,		// DepthFormat.Depth16
				MTLPixelFormat.Depth32Float,		// DepthFormat.Depth24
				MTLPixelFormat.Depth32Float_Stencil8	// DepthFormat.Depth24Stencil8
			};

			public static readonly int[] IndexSize = new int[]
			{
				2,	// IndexElementSize.SixteenBits
				4	// IndexElementSize.ThirtyTwoBits
			};
		}

		#endregion

		#region The Faux-Backbuffer

		private class MetalBackbuffer : IGLBackbuffer
		{
			public int Width
			{
				get;
				private set;
			}

			public int Height
			{
				get;
				private set;
			}

			public DepthFormat DepthFormat
			{
				get;
				private set;
			}

			public int MultiSampleCount
			{
				get;
				private set;
			}

			public IntPtr ColorBuffer;
			public IntPtr DepthStencilBuffer;
			
			private MetalDevice mtlDevice;

			public MetalBackbuffer(
				MetalDevice device,
				int width,
				int height,
				DepthFormat depthFormat,
				int multiSampleCount
			) {
				Width = width;
				Height = height;

				mtlDevice = device;
				DepthFormat = depthFormat;
				MultiSampleCount = multiSampleCount;

				// Generate the color buffer
				IntPtr colorBufferDesc = mtlMakeTexture2DDescriptor(
					mtlGetLayerPixelFormat(mtlDevice.layer),
					(uint) Width,
					(uint) Height,
					false
				);
				mtlSetTextureUsage(colorBufferDesc, MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead);

				if (multiSampleCount > 0)
				{
					mtlSetStorageMode(colorBufferDesc, MTLResourceStorageMode.Private);
					mtlSetTextureType(colorBufferDesc, MTLTextureType.Multisample2D);
					mtlSetTextureSampleCount(colorBufferDesc, multiSampleCount);
				}

				ColorBuffer = mtlNewTextureWithDescriptor(device.device, colorBufferDesc);

				// Create the depth-stencil buffer, if needed
				if (depthFormat == DepthFormat.None)
				{
					// Don't bother creating a depth/stencil buffer.
					DepthStencilBuffer = IntPtr.Zero;
				}
				else
				{
					// Create the depth/stencil buffer
					IntPtr depthStencilBufferDesc = mtlMakeTexture2DDescriptor(
						XNAToMTL.DepthStorage[(int) depthFormat],
						(uint) Width,
						(uint) Height,
						false
					);
					mtlSetStorageMode(depthStencilBufferDesc, MTLResourceStorageMode.Private);
					DepthStencilBuffer = mtlNewTextureWithDescriptor(device.device, depthStencilBufferDesc);
				}

				// This backbuffer is the initial render target
				mtlDevice.currentColorBuffer = ColorBuffer;
				mtlDevice.currentDepthStencilBuffer = DepthStencilBuffer;
			}

			public void Dispose()
			{
				ObjCRelease(ColorBuffer);
				ColorBuffer = IntPtr.Zero;

				ObjCRelease(DepthStencilBuffer);
				DepthStencilBuffer = IntPtr.Zero;
			}

			public void ResetFramebuffer(
				PresentationParameters presentationParameters,
				bool renderTargetBound
			) {
				Width = presentationParameters.BackBufferWidth;
				Height = presentationParameters.BackBufferHeight;
				mtlDevice.fauxBackbufferSizeChanged = true;

				DepthFormat = presentationParameters.DepthStencilFormat;
				MultiSampleCount = presentationParameters.MultiSampleCount;

				// Release the existing color buffer
				ObjCRelease(ColorBuffer);
				ColorBuffer = IntPtr.Zero;

				// Release the depth/stencil buffer, if applicable
				if (DepthStencilBuffer != IntPtr.Zero)
				{
					ObjCRelease(DepthStencilBuffer);
					DepthStencilBuffer = IntPtr.Zero;
				}

				// Update color buffer to the new resolution.
				IntPtr colorBufferDesc = mtlMakeTexture2DDescriptor(
					mtlGetLayerPixelFormat(mtlDevice.layer),
					(uint) Width,
					(uint) Height,
					false
				);
				mtlSetTextureUsage(colorBufferDesc, MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead);
				if (MultiSampleCount > 0)
				{
					mtlSetStorageMode(colorBufferDesc, MTLResourceStorageMode.Private);
					mtlSetTextureType(colorBufferDesc, MTLTextureType.Multisample2D);
					mtlSetTextureSampleCount(colorBufferDesc, MultiSampleCount);
				}
				ColorBuffer = mtlNewTextureWithDescriptor(mtlDevice.device, colorBufferDesc);

				// Update the depth/stencil buffer, if applicable
				if (DepthFormat != DepthFormat.None)
				{
					IntPtr depthStencilBufferDesc = mtlMakeTexture2DDescriptor(
						XNAToMTL.DepthStorage[(int) DepthFormat],
						(uint) Width,
						(uint) Height,
						false
					);
					mtlSetStorageMode(depthStencilBufferDesc, MTLResourceStorageMode.Private);
					DepthStencilBuffer = mtlNewTextureWithDescriptor(mtlDevice.device, depthStencilBufferDesc);
				}

				// If we don't already have a render target, treat this as the render target.
				if (!renderTargetBound)
				{
					mtlDevice.currentColorBuffer = ColorBuffer;
					mtlDevice.currentDepthStencilBuffer = DepthStencilBuffer;
				}
			}
		}

		private void InitializeFauxBackbuffer(
			PresentationParameters presentationParameters
		) {
			Backbuffer = new MetalBackbuffer(
				this,
				presentationParameters.BackBufferWidth,
				presentationParameters.BackBufferHeight,
				presentationParameters.DepthStencilFormat,
				presentationParameters.MultiSampleCount
			);

			// Create the vertex buffer for rendering the faux-backbuffer
			fauxBackbufferVertexBuffer = mtlNewBufferWithLength(
				device,
				16 * sizeof(float)
			);

			// Create and fill the index buffer
			ushort[] indices = new ushort[]
			{
				0, 1, 3,
				1, 2, 3
			};
			fauxBackbufferIndexBuffer = mtlNewBufferWithLength(
				device,
				6 * sizeof(ushort)
			);
			memcpy(
				mtlGetBufferContentsPtr(fauxBackbufferIndexBuffer),
				Marshal.UnsafeAddrOfPinnedArrayElement(indices, 0),
				(IntPtr) (6 * sizeof(ushort))
			);

			// Create vertex and fragment shaders for the faux-backbuffer pipeline
			// FIXME: Wonder if we could just compile ahead-of-time for this...
			string shaderSource =
			@"
				#include <metal_stdlib>
				using namespace metal;

				struct VertexIn {
					packed_float2 position;
					packed_float2 texCoord;
				};

				struct VertexOut {
					float4 position [[ position ]];
					float2 texCoord;
				};

				vertex VertexOut
				vertexShader(
					uint vertexID [[ vertex_id ]],
					constant VertexIn *vertexArray [[ buffer(0) ]]
				) {
					VertexOut out;
					out.position = float4(vertexArray[vertexID].position, 0.0, 1.0);
					out.texCoord = vertexArray[vertexID].texCoord;
					return out;
				}

				fragment float4
				fragmentShader(VertexOut in [[stage_in]],
					texture2d<half> colorTexture [[ texture(0) ]],
					sampler s0 [[sampler(0)]]
				) {
					const half4 colorSample = colorTexture.sample(s0, in.texCoord);
					return float4(colorSample);
				}
			";

			IntPtr library = mtlNewLibraryWithSource(
				device,
				UTF8ToNSString(shaderSource),
				IntPtr.Zero
			);
			IntPtr vertexFunc = mtlNewFunctionWithName(
				library,
				UTF8ToNSString("vertexShader")
			);
			IntPtr fragFunc = mtlNewFunctionWithName(
				library,
				UTF8ToNSString("fragmentShader")
			);

			// Create a sampler state
			IntPtr samplerDescriptor = mtlNewSamplerDescriptor();
			mtlSetSamplerMinFilter(samplerDescriptor, backbufferScaleMode);
			mtlSetSamplerMagFilter(samplerDescriptor, backbufferScaleMode);
			fauxBackbufferSamplerState = mtlNewSamplerStateWithDescriptor(
				device,
				samplerDescriptor
			);

			// Create a render pipeline for rendering the backbuffer
			IntPtr pipelineDesc = mtlMakeRenderPipelineDescriptor();
			mtlSetPipelineVertexFunction(pipelineDesc, vertexFunc);
			mtlSetPipelineFragmentFunction(pipelineDesc, fragFunc);
			mtlSetAttachmentPixelFormat(
				mtlGetColorAttachment(pipelineDesc, 0),
				mtlGetLayerPixelFormat(layer)
			);
			fauxBackbufferRenderPipeline = mtlNewRenderPipelineStateWithDescriptor(
				device,
				pipelineDesc
			);
		}

		#endregion

		#region The Faux-Faux-Backbuffer

		private class NullBackbuffer : IGLBackbuffer
		{
			public int Width
			{
				get;
				private set;
			}

			public int Height
			{
				get;
				private set;
			}

			public DepthFormat DepthFormat
			{
				get;
				private set;
			}

			public int MultiSampleCount
			{
				get
				{
					// Constant, per SDL2_GameWindow
					return 0;
				}
			}

			public NullBackbuffer(int width, int height, DepthFormat depthFormat)
			{
				Width = width;
				Height = height;
				DepthFormat = depthFormat;
			}

			public void ResetFramebuffer(
				PresentationParameters presentationParameters,
				bool renderTargetBound
			) {
				Width = presentationParameters.BackBufferWidth;
				Height = presentationParameters.BackBufferHeight;
			}
		}

		#endregion

		#region Public Static Utilities

		public static void MTL_GetDrawableSize(
			IntPtr metalLayer,
			out int width,
			out int height
		) {
			CGSize size = mtlGetDrawableSize(metalLayer);
			width = (int) size.width;
			height = (int) size.height;
		}

		#endregion
	}
}