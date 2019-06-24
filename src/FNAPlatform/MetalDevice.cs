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
using System.Collections.Generic;
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

			public IntPtr SamplerHandle;
			public TextureAddressMode WrapS;
			public TextureAddressMode WrapT;
			public TextureAddressMode WrapR;
			public TextureFilter Filter;
			public float Anisotropy;
			public int MaxMipmapLevel;
			public float LODBias;

			public MetalTexture(
				IntPtr handle,
				int levelCount
			) {
				Handle = handle;
				HasMipmaps = levelCount > 1;

				WrapS = TextureAddressMode.Wrap;
				WrapT = TextureAddressMode.Wrap;
				WrapR = TextureAddressMode.Wrap;
				Filter = TextureFilter.Linear;
				Anisotropy = 4.0f;
				MaxMipmapLevel = 0;
				LODBias = 0.0f;
			}

			private MetalTexture()
			{
				Handle = IntPtr.Zero;
			}
			public static readonly MetalTexture NullTexture = new MetalTexture();
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
					SetRCEBlendColor();
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

		private bool alphaBlendEnable = false;
		private Color blendColor = Color.Transparent;
		private BlendFunction blendOp = BlendFunction.Add;
		private BlendFunction blendOpAlpha = BlendFunction.Add;
		private Blend srcBlend = Blend.One;
		private Blend dstBlend = Blend.Zero;
		private Blend srcBlendAlpha = Blend.One;
		private Blend dstBlendAlpha = Blend.Zero;
		private ColorWriteChannels colorWriteEnable = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable1 = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable2 = ColorWriteChannels.All;
		private ColorWriteChannels colorWriteEnable3 = ColorWriteChannels.All;
		private int multisampleMask = -1; // AKA 0xFFFFFFFF

		#endregion

		#region Depth State Variables

		private bool zEnable = false;
		private bool zWriteEnable = false;
		private CompareFunction depthFunc = CompareFunction.Less;

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
					SetRCEStencilReferenceValue();
				}
			}
		}

		private bool stencilEnable = false;
		private int stencilWriteMask = -1; // AKA 0xFFFFFFFF, ugh -flibit
		private bool separateStencilEnable = false;
		private int stencilRef = 0;
		private int stencilMask = -1; // AKA 0xFFFFFFFF, ugh -flibit
		private CompareFunction stencilFunc = CompareFunction.Always;
		private StencilOperation stencilFail = StencilOperation.Keep;
		private StencilOperation stencilZFail = StencilOperation.Keep;
		private StencilOperation stencilPass = StencilOperation.Keep;
		private CompareFunction ccwStencilFunc = CompareFunction.Always;
		private StencilOperation ccwStencilFail = StencilOperation.Keep;
		private StencilOperation ccwStencilZFail = StencilOperation.Keep;
		private StencilOperation ccwStencilPass = StencilOperation.Keep;

		#endregion

		#region Rasterizer State Variables

		private bool scissorTestEnable = false;
		private CullMode cullFrontFace = CullMode.None;
		private FillMode fillMode = FillMode.Solid;
		private float depthBias = 0.0f;
		private float slopeScaleDepthBias = 0.0f;
		private bool multiSampleEnable = true;

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

		private MetalTexture[] Textures;

		#endregion

		#region Buffer Binding Cache Variables

		// ld, or LastDrawn, effect/vertex attributes
		private int ldBaseVertex = -1; // FIXME: Needed?
		private VertexDeclaration ldVertexDeclaration = null;
		private IntPtr ldPointer = IntPtr.Zero;
		private IntPtr ldEffect = IntPtr.Zero;
		private IntPtr ldTechnique = IntPtr.Zero;
		private uint ldPass = 0;

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
		private IntPtr currentVertexDescriptor;	// MTLVertexDescriptor*

		private ulong currentAttachmentWidth;
		private ulong currentAttachmentHeight;

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

				// Get attachment size
				currentAttachmentWidth = mtlGetTextureWidth(currentColorBuffer);
				currentAttachmentHeight = mtlGetTextureHeight(currentColorBuffer);

				// Make a new encoder
				RenderCommandEncoder = mtlMakeRenderCommandEncoder(
					commandBuffer,
					renderPassDesc
				);

				SetRCEViewport();
				if (scissorTestEnable)
				{
					SetRCEScissorRect();
				}
				SetRCEBlendColor();
				SetRCEStencilReferenceValue();
				SetRCECullModeAndWinding();
				SetRCEFillMode();
				SetRCEDepthBias();

				// Reset the flag
				renderPassDirty = false;
			}

			return RenderCommandEncoder;
		}

		private void SetRCEStencilReferenceValue()
		{
			if (RenderCommandEncoder != IntPtr.Zero)
			{
				mtlSetStencilReferenceValue(
					RenderCommandEncoder,
					(ulong) stencilRef
				);
			}
		}

		private void SetRCEBlendColor()
		{
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

		private void SetRCEViewport()
		{
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

		private void SetRCECullModeAndWinding()
		{
			if (RenderCommandEncoder != IntPtr.Zero)
			{
				mtlSetCullMode(
					RenderCommandEncoder,
					XNAToMTL.CullingEnabled[(int) cullFrontFace]
				);

				if (cullFrontFace != CullMode.None)
				{
					mtlSetFrontFacingWinding(
						RenderCommandEncoder,
						XNAToMTL.FrontFace[(int) cullFrontFace]
					);
				}
			}

		}

		private void SetRCEFillMode()
		{
			if (RenderCommandEncoder != IntPtr.Zero)
			{
				mtlSetTriangleFillMode(
					RenderCommandEncoder,
					XNAToMTL.FillMode[(int) fillMode]
				);
			}
		}

		private void SetRCEDepthBias()
		{
			if (RenderCommandEncoder != null)
			{
				mtlSetDepthBias(
					RenderCommandEncoder,
					depthBias,
					slopeScaleDepthBias,
					2.0f // FIXME: What should this be?
				);
			}
		}

		private void SetRCEScissorRect()
		{
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

		private IntPtr currentVertexShader = IntPtr.Zero;
		private IntPtr currentFragmentShader = IntPtr.Zero;
		private IntPtr currentVertexUniformBuffer = IntPtr.Zero;
		private IntPtr currentFragmentUniformBuffer = IntPtr.Zero;

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

			// Initialize texture collection array
			Textures = new MetalTexture[MaxTextureSlots];
			for (int i = 0; i < MaxTextureSlots; i += 1)
			{
				Textures[i] = MetalTexture.NullTexture;
			}

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

		public void DrawIndexedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			IndexBuffer indices
		) {
			DrawInstancedPrimitives(
				primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				1,
				indices
			);
		}

		public void DrawInstancedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount,
			IndexBuffer indices
		) {
			mtlDrawIndexedPrimitives(
				RenderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				(ulong) XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount),
				(indices.IndexElementSize == IndexElementSize.SixteenBits) ? MTLIndexType.UInt16 : MTLIndexType.UInt32,
				(indices.buffer as MetalBuffer).Handle,
				(ulong) minVertexIndex,
				(ulong) instanceCount,
				baseVertex,
				0
			);
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
				SetRCEViewport();
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
			}

			/* Note: We don't need to flip the rectangle,
			 * so we have no reason to use renderTargetBound.
			 * -caleb
			 */
		}

		public void ApplyRasterizerState(
			RasterizerState rasterizerState,
			bool renderTargetBound
		) {
			if (rasterizerState.ScissorTestEnable != scissorTestEnable)
			{
				scissorTestEnable = rasterizerState.ScissorTestEnable;
				if (!scissorTestEnable && RenderCommandEncoder != IntPtr.Zero)
				{
					// Set to the default scissor rect
					// FIXME: Make sure currentAttachmentWidth/Height get updated appropriately
					mtlSetScissorRect(
						RenderCommandEncoder,
						0,
						0,
						currentAttachmentWidth,
						currentAttachmentHeight
					);
				}
			}

			CullMode actualMode;
			if (renderTargetBound)
			{
				actualMode = rasterizerState.CullMode;
			}
			else
			{
				// When not rendering offscreen the faces change order.
				if (rasterizerState.CullMode == CullMode.None)
				{
					actualMode = rasterizerState.CullMode;
				}
				else
				{
					actualMode = (
						rasterizerState.CullMode == CullMode.CullClockwiseFace ?
							CullMode.CullCounterClockwiseFace :
							CullMode.CullClockwiseFace
					);
				}
			}
			if (actualMode != cullFrontFace)
			{
				cullFrontFace = actualMode;
				SetRCECullModeAndWinding();
			}

			if (rasterizerState.FillMode != fillMode)
			{
				fillMode = rasterizerState.FillMode;
				SetRCEFillMode();
			}

			float realDepthBias = rasterizerState.DepthBias * XNAToMTL.DepthBiasScale[
				(int) Backbuffer.DepthFormat // FIXME: Handle render targets
			];
			if (	realDepthBias != depthBias ||
				rasterizerState.SlopeScaleDepthBias != slopeScaleDepthBias	)
			{
				depthBias = realDepthBias;
				slopeScaleDepthBias = rasterizerState.SlopeScaleDepthBias;
				SetRCEDepthBias();
			}

			if (rasterizerState.MultiSampleAntiAlias != multiSampleEnable)
			{
				multiSampleEnable = rasterizerState.MultiSampleAntiAlias;
				// FIXME: What should we do with this...?
			}
		}

		public void VerifySampler(int index, Texture texture, SamplerState sampler)
		{
			if (texture == null)
			{
				Textures[index] = MetalTexture.NullTexture;
				return;
			}

			MetalTexture tex = texture.texture as MetalTexture;
			if (	tex == Textures[index] &&
				sampler.AddressU == tex.WrapS &&
				sampler.AddressV == tex.WrapT &&
				sampler.AddressW == tex.WrapR &&
				sampler.Filter == tex.Filter &&
				sampler.MaxAnisotropy == tex.Anisotropy &&
				sampler.MaxMipLevel == tex.MaxMipmapLevel &&
				sampler.MipMapLevelOfDetailBias == tex.LODBias	)
			{
				// Nothing's changing, forget it.
				return;
			}

			// Bind the correct texture
			if (tex != Textures[index])
			{
				Textures[index] = tex;
			}

			// Apply the sampler states
			IntPtr samplerDesc = mtlNewSamplerDescriptor();
			if (sampler.AddressU != tex.WrapS)
			{
				tex.WrapS = sampler.AddressU;
				mtlSetSampler_sAddressMode(
					samplerDesc,
					XNAToMTL.Wrap[(int) tex.WrapS]
				);
			}
			if (sampler.AddressV != tex.WrapT)
			{
				tex.WrapT = sampler.AddressV;
				mtlSetSampler_tAddressMode(
					samplerDesc,
					XNAToMTL.Wrap[(int) tex.WrapT]
				);
			}
			if (sampler.AddressW != tex.WrapR)
			{
				tex.WrapR = sampler.AddressW;
				mtlSetSampler_rAddressMode(
					samplerDesc,
					XNAToMTL.Wrap[(int) tex.WrapR]
				);
			}
			if (	sampler.Filter != tex.Filter ||
				sampler.MaxAnisotropy != tex.Anisotropy	)
			{
				tex.Filter = sampler.Filter;
				tex.Anisotropy = sampler.MaxAnisotropy;

				mtlSetSamplerMagFilter(
					samplerDesc,
					XNAToMTL.MagFilter[(int) tex.Filter]
				);

				mtlSetSamplerMinFilter(
					samplerDesc,
					XNAToMTL.MinFilter[(int) tex.Filter]
				);

				ulong scaledAnisotropy = 1 + (ulong) Math.Round(tex.Anisotropy * 15);
				mtlSetSamplerMaxAnisotropy(
					samplerDesc,
					(tex.Filter == TextureFilter.Anisotropic) ?
						scaledAnisotropy :
						1
				);
			}

			// FIXME: We'll need to create a new MTLTexture for this.
			// if (sampler.MaxMipLevel != tex.MaxMipmapLevel)
			// {
			// 	tex.MaxMipmapLevel = sampler.MaxMipLevel;
			// 	glTexParameteri(
			// 		tex.Target,
			// 		GLenum.GL_TEXTURE_BASE_LEVEL,
			// 		tex.MaxMipmapLevel
			// 	);
			// }

			if (sampler.MipMapLevelOfDetailBias != tex.LODBias)
			{
				tex.LODBias = sampler.MipMapLevelOfDetailBias;
				/* FIXME: Metal doesn't have a LODBias.
				 * It only has lodMinClamp, lodMaxClamp, and lodAverage.
				 */
			}

			// Create and store the new sampler state
			tex.SamplerHandle = mtlNewSamplerStateWithDescriptor(
				device,
				samplerDesc
			);
		}

		public void SetBlendState(BlendState blendState)
		{
			/* Store changes since the state isn't applied until
			 * we create or retrieve a render pipeline state.
			 */

			bool newEnable = (
				!(	blendState.ColorSourceBlend == Blend.One &&
					blendState.ColorDestinationBlend == Blend.Zero &&
					blendState.AlphaSourceBlend == Blend.One &&
					blendState.AlphaDestinationBlend == Blend.Zero	)
			);
			if (newEnable != alphaBlendEnable)
			{
				alphaBlendEnable = newEnable;
			}

			if (alphaBlendEnable)
			{
				if (blendState.BlendFactor != blendColor)
				{
					blendColor = blendState.BlendFactor;
					SetRCEBlendColor();
				}

				if (	blendState.ColorSourceBlend != srcBlend ||
					blendState.ColorDestinationBlend != dstBlend ||
					blendState.AlphaSourceBlend != srcBlendAlpha ||
					blendState.AlphaDestinationBlend != dstBlendAlpha	)
				{
					srcBlend = blendState.ColorSourceBlend;
					dstBlend = blendState.ColorDestinationBlend;
					srcBlendAlpha = blendState.AlphaSourceBlend;
					dstBlendAlpha = blendState.AlphaDestinationBlend;
				}

				if (	blendState.ColorBlendFunction != blendOp ||
					blendState.AlphaBlendFunction != blendOpAlpha	)
				{
					blendOp = blendState.ColorBlendFunction;
					blendOpAlpha = blendState.AlphaBlendFunction;
				}
			}

			if (blendState.ColorWriteChannels != colorWriteEnable)
			{
				colorWriteEnable = blendState.ColorWriteChannels;
			}
			if (blendState.ColorWriteChannels1 != colorWriteEnable1)
			{
				colorWriteEnable1 = blendState.ColorWriteChannels1;
			}
			if (blendState.ColorWriteChannels2 != colorWriteEnable2)
			{
				colorWriteEnable2 = blendState.ColorWriteChannels2;
			}
			if (blendState.ColorWriteChannels3 != colorWriteEnable3)
			{
				colorWriteEnable3 = blendState.ColorWriteChannels3;
			}

			if (blendState.MultiSampleMask != multisampleMask)
			{
				multisampleMask = blendState.MultiSampleMask;
				// FIXME: This doesn't do anything...
			}
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			// FIXME: Add this once we figure out depth-stencil buffers
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
						out currentFragmentShader,
						out currentVertexUniformBuffer,
						out currentFragmentUniformBuffer
					);
					return;
				}
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectBeginPass(
					currentEffect,
					pass,
					out currentVertexShader,
					out currentFragmentShader,
					out currentVertexUniformBuffer,
					out currentFragmentUniformBuffer
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
					out currentFragmentShader,
					out currentVertexUniformBuffer,
					out currentFragmentUniformBuffer
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
				out currentFragmentShader,
				out currentVertexUniformBuffer,
				out currentFragmentUniformBuffer
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

		private Dictionary<VertexBufferBinding[], IntPtr> VertexDescriptorCache =
			new Dictionary<VertexBufferBinding[], IntPtr>();

		private Dictionary<PipelineState, IntPtr> PipelineStateCache =
			new Dictionary<PipelineState, IntPtr>();

		private struct PipelineState
		{
			public MTLPixelFormat pixelFormat;
			public IntPtr vertexFunction;
			public IntPtr fragFunction;
			public IntPtr vertexDescriptor;

			// FIXME: Make use of PipelineCache
			public bool alphaBlendEnable;
			public MTLBlendFactor srcRGB;
			public MTLBlendFactor dstRGB;
			public MTLBlendFactor srcAlpha;
			public MTLBlendFactor dstAlpha;
			public MTLBlendOperation rgbOp;
			public MTLBlendOperation alphaOp;
			public ulong colorWriteEnable;
			public ulong colorWriteEnable1;
			public ulong colorWriteEnable2;
			public ulong colorWriteEnable3;
		}

		private IntPtr FetchRenderPipeline()
		{
			PipelineState state = new PipelineState();

			state.pixelFormat = mtlGetLayerPixelFormat(layer); // FIXME: This should be currentPixelFormat!
			state.vertexFunction = currentVertexShader;
			state.fragFunction = currentFragmentShader;
			state.vertexDescriptor = currentVertexDescriptor;

			state.alphaBlendEnable = alphaBlendEnable;
			state.srcRGB = XNAToMTL.BlendMode[(int) srcBlend];
			state.srcAlpha = XNAToMTL.BlendMode[(int) srcBlendAlpha];
			state.dstRGB = XNAToMTL.BlendMode[(int) dstBlend];
			state.dstAlpha = XNAToMTL.BlendMode[(int) dstBlendAlpha];
			state.rgbOp = XNAToMTL.BlendOperation[(int) blendOp];
			state.alphaOp = XNAToMTL.BlendOperation[(int) blendOpAlpha];
			state.colorWriteEnable  = (ulong) colorWriteEnable;
			state.colorWriteEnable1 = (ulong) colorWriteEnable1;
			state.colorWriteEnable2 = (ulong) colorWriteEnable2;
			state.colorWriteEnable3 = (ulong) colorWriteEnable3;

			IntPtr pipeline = IntPtr.Zero;
			if (PipelineStateCache.TryGetValue(state, out pipeline))
			{
				// We have this state already cached!
				return pipeline;
			}

			Console.WriteLine("Making new pipeline...");

			// Make a new render pipeline descriptor
			IntPtr pipelineDesc = mtlMakeRenderPipelineDescriptor();
			
			// Apply Blend State
			IntPtr colorAttachment = mtlGetColorAttachment(pipelineDesc, 0);
			Console.WriteLine("Blending enabled? " + alphaBlendEnable);
			Console.WriteLine("SRC RGB: " + state.srcRGB);
			Console.WriteLine("SRC ALPHA: " + state.srcAlpha);
			Console.WriteLine("DST RGB: " + state.dstRGB);
			Console.WriteLine("DST ALPHA: " + state.dstAlpha);
			Console.WriteLine("RBG OP: " + state.rgbOp);
			Console.WriteLine("ALPHA OP: " + state.alphaOp);
			Console.WriteLine("COLOR WRITE ENABLE 0: " + state.colorWriteEnable);
			Console.WriteLine("COLOR WRITE ENABLE 1: " + state.colorWriteEnable1);
			Console.WriteLine("COLOR WRITE ENABLE 2: " + state.colorWriteEnable2);
			Console.WriteLine("COLOR WRITE ENABLE 3: " + state.colorWriteEnable3);
			mtlSetAttachmentBlendingEnabled(
				colorAttachment,
				alphaBlendEnable
			);
			mtlSetAttachmentSourceRGBBlendFactor(
				colorAttachment,
				state.srcRGB
			);
			mtlSetAttachmentDestinationRGBBlendFactor(
				colorAttachment,
				state.dstRGB
			);
			mtlSetAttachmentSourceAlphaBlendFactor(
				colorAttachment,
				state.srcAlpha
			);
			mtlSetAttachmentDestinationAlphaBlendFactor(
				colorAttachment,
				state.dstAlpha
			);

			mtlSetAttachmentRGBBlendOperation(
				colorAttachment,
				state.rgbOp
			);
			mtlSetAttachmentAlphaBlendOperation(
				colorAttachment,
				state.alphaOp
			);

			mtlSetAttachmentWriteMask(
				colorAttachment,
				state.colorWriteEnable
			);
			/* FIXME: So how exactly do we factor in
			 * COLORWRITEENABLE for buffer 0? Do we just assume that
			 * the default is just buffer 0, and all other calls
			 * update the other write masks afterward?
			 * -flibit
			 */
			mtlSetAttachmentWriteMask(
				mtlGetColorAttachment(pipelineDesc, 1),
				state.colorWriteEnable1
			);
			mtlSetAttachmentWriteMask(
				mtlGetColorAttachment(pipelineDesc, 2),
				state.colorWriteEnable2
			);
			mtlSetAttachmentWriteMask(
				mtlGetColorAttachment(pipelineDesc, 3),
				state.colorWriteEnable3
			);

			mtlSetAttachmentPixelFormat(
				colorAttachment,
				state.pixelFormat
			);

			// Apply shaders and vertex descriptor
			mtlSetPipelineVertexFunction(pipelineDesc, state.vertexFunction);
			mtlSetPipelineFragmentFunction(pipelineDesc, state.fragFunction);
			mtlSetPipelineVertexDescriptor(pipelineDesc, state.vertexDescriptor);

			// Finalize the render pipeline
			IntPtr pipelineState = mtlNewRenderPipelineStateWithDescriptor(
				device,
				pipelineDesc
			);
			PipelineStateCache.Add(state, pipelineState);
			return pipelineState;
		}

		public void ApplyVertexAttributes(
			VertexBufferBinding[] bindings,
			int numBindings,
			bool bindingsUpdated,
			int baseVertex
		) {
			if (	bindingsUpdated ||
				currentEffect != ldEffect ||
				currentTechnique != ldTechnique ||
				currentPass != ldPass ||
				effectApplied	)
			{
				// Translate the bindings array into a descriptor
				IntPtr descriptor;
				if (VertexDescriptorCache.TryGetValue(bindings, out descriptor))
				{
					currentVertexDescriptor = descriptor;
				}
				else
				{
					Console.WriteLine("Making a new vertex descriptor");
					descriptor = mtlMakeVertexDescriptor();
					for (int i = numBindings - 1; i >= 0; i -= 1)
					{
						// Describe vertex attributes
						VertexDeclaration vertexDeclaration = bindings[i].VertexBuffer.VertexDeclaration;
						for (int j = 0; j < vertexDeclaration.elements.Length; j += 1)
						{
							VertexElement element = vertexDeclaration.elements[j];

							Console.WriteLine(XNAToMTL.VertexAttribUsage[(int) element.VertexElementUsage]);
							Console.WriteLine("USAGE INDEX: " + element.UsageIndex);

							int attribLoc = MojoShader.MOJOSHADER_mtlGetVertexAttribLocation(
								currentEffect,
								XNAToMTL.VertexAttribUsage[(int) element.VertexElementUsage],
								element.UsageIndex
							);
							Console.WriteLine(attribLoc);
							if (attribLoc == -1)
							{
								// Stream not in use!
								continue;
							}
							IntPtr attrib = mtlGetVertexAttributeDescriptor(
								descriptor,
								attribLoc
							);
							mtlSetVertexAttributeFormat(
								attrib,
								XNAToMTL.VertexAttribType[(int) element.VertexElementFormat]
							);
							mtlSetVertexAttributeOffset(
								attrib,
								element.Offset
							);
							mtlSetVertexAttributeBufferIndex(
								attrib,
								i
							);
						}
						
						// Describe vertex buffer layout
						IntPtr layout = mtlGetVertexBufferLayoutDescriptor(
							descriptor,
							i
						);
						mtlSetVertexBufferLayoutStride(
							layout,
							vertexDeclaration.VertexStride
						);
						if (bindings[i].InstanceFrequency > 1)
						{
							mtlSetVertexBufferLayoutStepFunction(
								layout,
								MTLVertexStepFunction.PerInstance
							);
							mtlSetVertexBufferLayoutStepRate(
								layout,
								bindings[i].InstanceFrequency
							);
						}

					}
					VertexDescriptorCache.Add(bindings, descriptor);
					currentVertexDescriptor = descriptor;
				}

				ldBaseVertex = baseVertex;
				ldEffect = currentEffect;
				ldTechnique = currentTechnique;
				ldPass = currentPass;
				effectApplied = false;
				ldVertexDeclaration = null;
				ldPointer = IntPtr.Zero;
			}

			// Get the latest encoder
			GetRenderCommandEncoder();

			// Update the vertex buffers
			for (int i = 0; i < bindings.Length; i += 1)
			{
				if (bindings[i].VertexBuffer != null)
				{
					mtlSetVertexBuffer(
						RenderCommandEncoder,
						(bindings[i].VertexBuffer.buffer as MetalBuffer).Handle,
						(ulong) bindings[i].VertexOffset, // FIXME: This may need to change.
						(ulong) i
					);
					//Console.WriteLine((bindings[i].VertexBuffer.buffer as MetalBuffer).Contents.ToString("X"));
				}
			}

			// Bind the texture and its sampler state
			mtlSetFragmentTexture(
				RenderCommandEncoder,
				Textures[0].Handle, // FIXME
				0 // FIXME
			);
			mtlSetFragmentSamplerState(
				RenderCommandEncoder,
				Textures[0].SamplerHandle, // FIXME
				0 // FIXME
			);

			// Bind the uniform buffers
			if (currentVertexUniformBuffer != IntPtr.Zero)
			{
				//Console.WriteLine(mtlGetBufferContentsPtr(currentVertexUniformBuffer).ToString("X"));
				mtlSetVertexBuffer(
					RenderCommandEncoder,
					currentVertexUniformBuffer,
					0,
					16 // In MojoShader output it's always 16 for some reason
				);
			}

			if (currentFragmentUniformBuffer != IntPtr.Zero)
			{
				mtlSetFragmentBuffer(
					RenderCommandEncoder,
					currentFragmentUniformBuffer,
					0,
					16 // In MojoShader output it's always 16 for some reason
				);
			}

			// Finally, set the pipeline state.
			IntPtr pipelineState = FetchRenderPipeline();
			mtlSetRenderPipelineState(
				RenderCommandEncoder,
				pipelineState
			);
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
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

		private void SetBufferData(
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

		public void SetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			SetBufferData(buffer, offsetInBytes, data, dataLength, options);
		}

		public void SetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			SetBufferData(buffer, offsetInBytes, data, dataLength, options);
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

		public IGLTexture CreateTexture2D(
			SurfaceFormat format,
			int width,
			int height,
			int levelCount
		) {
			IntPtr texDesc = mtlMakeTexture2DDescriptor(
				XNAToMTL.TextureFormat[(int) format],
				(ulong) width,
				(ulong) height,
				levelCount > 0
			);
			IntPtr tex = mtlNewTextureWithDescriptor(device, texDesc);
			return new MetalTexture(tex, levelCount);
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

		public void SetTextureData2D(
			IGLTexture texture,
			SurfaceFormat format,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength
		) {
			MTLRegion region = new MTLRegion(
				new MTLOrigin((ulong) x, (ulong) y, 0),
				new MTLSize((ulong) w, (ulong) h, 1)
			);
			mtlReplaceRegion(
				(texture as MetalTexture).Handle,
				region,
				(ulong) level,
				data,
				(ulong) (dataLength / h)
			);
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

			public static readonly MojoShader.MOJOSHADER_usage[] VertexAttribUsage = new MojoShader.MOJOSHADER_usage[]
			{
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POSITION,		// VertexElementUsage.Position
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_COLOR,		// VertexElementUsage.Color
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TEXCOORD,		// VertexElementUsage.TextureCoordinate
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_NORMAL,		// VertexElementUsage.Normal
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_BINORMAL,		// VertexElementUsage.Binormal
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TANGENT,		// VertexElementUsage.Tangent
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_BLENDINDICES,	// VertexElementUsage.BlendIndices
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_BLENDWEIGHT,	// VertexElementUsage.BlendWeight
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_DEPTH,		// VertexElementUsage.Depth
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_FOG,		// VertexElementUsage.Fog
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POINTSIZE,		// VertexElementUsage.PointSize
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_SAMPLE,		// VertexElementUsage.Sample
				MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TESSFACTOR		// VertexElementUsage.TessellateFactor
			};

			public static readonly int[] VertexAttribSize = new int[]
			{
					1,	// VertexElementFormat.Single
					2,	// VertexElementFormat.Vector2
					3,	// VertexElementFormat.Vector3
					4,	// VertexElementFormat.Vector4
					4,	// VertexElementFormat.Color
					4,	// VertexElementFormat.Byte4
					2,	// VertexElementFormat.Short2
					4,	// VertexElementFormat.Short4
					2,	// VertexElementFormat.NormalizedShort2
					4,	// VertexElementFormat.NormalizedShort4
					2,	// VertexElementFormat.HalfVector2
					4	// VertexElementFormat.HalfVector4
			};

			public static readonly MTLVertexFormat[] VertexAttribType = new MTLVertexFormat[]
			{
				MTLVertexFormat.Float,		// VertexElementFormat.Single
				MTLVertexFormat.Float2,		// VertexElementFormat.Vector2
				MTLVertexFormat.Float3,		// VertexElementFormat.Vector3
				MTLVertexFormat.Float4,		// VertexElementFormat.Vector4
				MTLVertexFormat.UChar4Normalized,	// VertexElementFormat.Color
				MTLVertexFormat.UChar4,		// VertexElementFormat.Byte4
				MTLVertexFormat.Short2,		// VertexElementFormat.Short2
				MTLVertexFormat.Short4,		// VertexElementFormat.Short4
				MTLVertexFormat.Short2Normalized,	// VertexElementFormat.NormalizedShort2
				MTLVertexFormat.Short4Normalized,	// VertexElementFormat.NormalizedShort4
				MTLVertexFormat.Half2,		// VertexElementFormat.HalfVector2
				MTLVertexFormat.Half4		// VertexElementFormat.HalfVector4
			};

			public static bool VertexAttribNormalized(VertexElement element)
			{
				return (	element.VertexElementUsage == VertexElementUsage.Color ||
						element.VertexElementFormat == VertexElementFormat.NormalizedShort2 ||
						element.VertexElementFormat == VertexElementFormat.NormalizedShort4	);
			}

			public static readonly int[] IndexSize = new int[]
			{
				2,	// IndexElementSize.SixteenBits
				4	// IndexElementSize.ThirtyTwoBits
			};

			public static readonly MTLBlendFactor[] BlendMode = new MTLBlendFactor[]
			{
				MTLBlendFactor.One,			// Blend.One
				MTLBlendFactor.Zero,			// Blend.Zero
				MTLBlendFactor.SourceColor,		// Blend.SourceColor
				MTLBlendFactor.OneMinusSourceColor,	// Blend.InverseSourceColor
				MTLBlendFactor.SourceAlpha,		// Blend.SourceAlpha
				MTLBlendFactor.OneMinusSourceAlpha,	// Blend.InverseSourceAlpha
				MTLBlendFactor.DestinationColor,	// Blend.DestinationColor
				MTLBlendFactor.OneMinusDestinationColor,// Blend.InverseDestinationColor
				MTLBlendFactor.DestinationAlpha,	// Blend.DestinationAlpha
				MTLBlendFactor.OneMinusDestinationAlpha,// Blend.InverseDestinationAlpha
				MTLBlendFactor.BlendColor,		// Blend.BlendFactor
				MTLBlendFactor.OneMinusBlendColor,	// Blend.InverseBlendFactor
				MTLBlendFactor.SourceAlphaSaturated	// Blend.SourceAlphaSaturation
			};

			public static readonly MTLBlendOperation[] BlendOperation = new MTLBlendOperation[]
			{
				MTLBlendOperation.Add,			// BlendFunction.Add
				MTLBlendOperation.Subtract,		// BlendFunction.Subtract
				MTLBlendOperation.ReverseSubtract,	// BlendFunction.ReverseSubtract
				MTLBlendOperation.Max,			// BlendFunction.Max
				MTLBlendOperation.Min			// BlendFunction.Min
			};

			public static readonly MTLWinding[] FrontFace = new MTLWinding[]
			{
				0,				// NOPE
				MTLWinding.Clockwise,		// CullMode.CullClockwiseFace
				MTLWinding.CounterClockwise	// CullMode.CullCounterClockwiseFace
			};

			public static readonly MTLTriangleFillMode[] FillMode = new MTLTriangleFillMode[]
			{
				MTLTriangleFillMode.Fill,	// FillMode.Solid
				MTLTriangleFillMode.Lines	// FillMode.WireFrame
			};

			public static readonly float[] DepthBiasScale = new float[]
			{
				0.0f,				// DepthFormat.None
				(float) ((1 << 16) - 1),	// DepthFormat.Depth16
				(float) ((1 << 24) - 1),	// DepthFormat.Depth24
				(float) ((1 << 24) - 1)		// DepthFormat.Depth24Stencil8
			};

			public static readonly MTLCullMode[] CullingEnabled = new MTLCullMode[]
			{
				MTLCullMode.None,		// CullMode.None
				MTLCullMode.Back,		// CullMode.CullClockwiseFace
				MTLCullMode.Back		// CullMode.CullCounterClockwiseFace
			};

			public static readonly MTLSamplerAddressMode[] Wrap = new MTLSamplerAddressMode[]
			{
				MTLSamplerAddressMode.Repeat,		// TextureAddressMode.Wrap
				MTLSamplerAddressMode.ClampToEdge,	// TextureAddressMode.Clamp
				MTLSamplerAddressMode.MirrorRepeat	// TextureAddressMode.Mirror
			};

			public static readonly MTLSamplerMinMagFilter[] MagFilter = new MTLSamplerMinMagFilter[]
			{
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.Linear
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.Point
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.Anisotropic
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.LinearMipPoint
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.PointMipLinear
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.MinLinearMagPointMipLinear
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.MinLinearMagPointMipPoint
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.MinPointMagLinearMipLinear
				MTLSamplerMinMagFilter.Linear	// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly int[] MinMipFilter = new int[]
			{
				(int) MTLSamplerMipFilter.Linear,	// TextureFilter.Linear
				(int) MTLSamplerMipFilter.Nearest,	// TextureFilter.Point
				(int) MTLSamplerMipFilter.Linear,	// TextureFilter.Anisotropic
				(int) MTLSamplerMipFilter.Nearest,	// TextureFilter.LinearMipPoint
				(int) MTLSamplerMipFilter.Linear,	// TextureFilter.PointMipLinear
				(int) MTLSamplerMipFilter.Linear,	// TextureFilter.MinLinearMagPointMipLinear
				(int) MTLSamplerMipFilter.Nearest,	// TextureFilter.MinLinearMagPointMipPoint
				(int) MTLSamplerMipFilter.Linear,	// TextureFilter.MinPointMagLinearMipLinear
				(int) MTLSamplerMipFilter.Nearest	// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly MTLSamplerMinMagFilter[] MinFilter = new MTLSamplerMinMagFilter[]
			{
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.Linear
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.Point
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.Anisotropic
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.LinearMipPoint
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.PointMipLinear
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.MinLinearMagPointMipLinear
				MTLSamplerMinMagFilter.Linear,	// TextureFilter.MinLinearMagPointMipPoint
				MTLSamplerMinMagFilter.Nearest,	// TextureFilter.MinPointMagLinearMipLinear
				MTLSamplerMinMagFilter.Nearest	// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly MTLPrimitiveType[] Primitive = new MTLPrimitiveType[]
			{
				MTLPrimitiveType.Triangle,	// PrimitiveType.TriangleList
				MTLPrimitiveType.TriangleStrip,	// PrimitiveType.TriangleStrip
				MTLPrimitiveType.Line,		// PrimitiveType.LineList
				MTLPrimitiveType.LineStrip,	// PrimitiveType.LineStrip
				MTLPrimitiveType.Point		// PrimitiveType.PointListEXT
			};

			public static int PrimitiveVerts(PrimitiveType primitiveType, int primitiveCount)
			{
				switch (primitiveType)
				{
					case PrimitiveType.TriangleList:
						return primitiveCount * 3;
					case PrimitiveType.TriangleStrip:
						return primitiveCount + 2;
					case PrimitiveType.LineList:
						return primitiveCount * 2;
					case PrimitiveType.LineStrip:
						return primitiveCount + 1;
					case PrimitiveType.PointListEXT:
						return primitiveCount;
				}
				throw new NotSupportedException();
			}
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