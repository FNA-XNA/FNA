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

		#region Texture Descriptor Container Class

		// FIXME: We could probably remove this class
		private class TextureDescriptor
		{
			private IntPtr handle;
			private IntPtr mtlDevice;

			public MTLTextureType TextureType
			{
				set
				{
					mtlSetTextureType(handle, value);
				}
			}

			public MTLPixelFormat PixelFormat
			{
				set
				{
					mtlSetTexturePixelFormat(handle, value);
				}
			}

			public int Width
			{
				set
				{
					mtlSetTextureWidth(handle, value);
				}
			}

			public int Height
			{
				set
				{
					mtlSetTextureHeight(handle, value);
				}
			}

			public int SampleCount
			{
				set
				{
					mtlSetTextureSampleCount(handle, value);
				}
			}

			public MTLTextureUsage Usage
			{
				set
				{
					mtlSetTextureUsage(handle, value);
				}
			}

			public MTLResourceStorageMode StorageMode
			{
				set
				{
					mtlSetStorageMode(handle, value);
				}
			}

			public TextureDescriptor(IntPtr mtlDevice)
			{
				this.mtlDevice = mtlDevice;
				handle = mtlMakeTexture2DDescriptor(
					MTLPixelFormat.RGBA8Unorm,
					1,
					1,
					false
				);
			}

			public void Reset()
			{
				// Return to default values
				TextureType = MTLTextureType.Texture2D;
				PixelFormat = MTLPixelFormat.RGBA8Unorm;
				Width = 1;
				Height = 1;
				SampleCount = 1;
				Usage = MTLTextureUsage.ShaderRead;
				if (SDL.SDL_GetPlatform().Equals("Mac OS X"))
				{
					// macOS does not support Shared texture storage
					StorageMode = MTLResourceStorageMode.Managed;
				}
				else
				{
					StorageMode = MTLResourceStorageMode.Shared;
				}
			}

			public IntPtr GenTexture()
			{
				IntPtr tex = mtlNewTextureWithDescriptor(mtlDevice, handle);

				// Make sure ObjC doesn't autorelease our texture
				ObjCRetain(tex);

				return tex;
			}
		}

		private TextureDescriptor textureDescriptor;

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
					mtlSetBlendColor(
						renderCommandEncoder,
						blendColor.R / 255f,
						blendColor.G / 255f,
						blendColor.B / 255f,
						blendColor.A / 255f
					);
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
					mtlSetStencilReferenceValue(
						renderCommandEncoder,
						(uint) stencilRef
					);
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

		private Vector4 currentClearColor = new Vector4(0, 0, 0, 0);
		private float currentClearDepth = 1.0f;
		private int currentClearStencil = 0;

		#endregion

		#region Private Metal State Variables

		private IntPtr layer;			// CAMetalLayer*
		private IntPtr device;			// MTLDevice*
		private IntPtr queue;			// MTLCommandQueue*
		private IntPtr commandBuffer;		// MTLCommandBuffer*
		private IntPtr renderCommandEncoder;	// MTLRenderCommandEncoder*
		private IntPtr currentDrawable;		// CAMetalDrawable*

		private IntPtr currentColorBuffer;	// MTLTexture*
		private IntPtr currentDepthStencilBuffer; // MTLTexture*

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

			// Reuse the same descriptor for each generated texture
			textureDescriptor = new TextureDescriptor(device);

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

		public void SwapBuffers(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
		{
			// We're done rendering the frame!
			mtlEndEncoding(renderCommandEncoder);

			// Do one final pass to set the MSAA resolve texture, if applicable
			IntPtr colorBuffer = (Backbuffer as MetalBackbuffer).ColorBuffer;
			if (Backbuffer.MultiSampleCount > 0)
			{
				// Generate temp texture for resolving the actual backbuffer
				textureDescriptor.PixelFormat = mtlGetLayerPixelFormat(layer);
				textureDescriptor.Width = Backbuffer.Width;
				textureDescriptor.Height = Backbuffer.Height;
				textureDescriptor.Usage = MTLTextureUsage.RenderTarget;
				colorBuffer = textureDescriptor.GenTexture();
				textureDescriptor.Reset();

				// Describe the pass
				IntPtr resolvePass = mtlMakeRenderPassDescriptor();
				IntPtr colorAttachment = mtlGetColorAttachment(resolvePass, 0);
				mtlSetAttachmentTexture(
					colorAttachment,
					(Backbuffer as MetalBackbuffer).ColorBuffer
				);
				mtlSetAttachmentResolveTexture(
					colorAttachment,
					colorBuffer
				);
				mtlSetAttachmentStoreAction(
					colorAttachment,
					MTLStoreAction.MultisampleResolve
				);

				// Perform the pass
				renderCommandEncoder = mtlMakeRenderCommandEncoder(
					commandBuffer,
					resolvePass
				);
				mtlEndEncoding(renderCommandEncoder);
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

			if (srcW == dstW && srcH == dstH)
			{
				// Blit the faux-backbuffer to the real backbuffer
				// FIXME: What should we do with depth/stencil?
				MTLOrigin srcOrigin = new MTLOrigin((ulong) srcX, (ulong) srcY, 0);
				MTLOrigin dstOrigin = new MTLOrigin((ulong) dstX, (ulong) dstY, 0);
				MTLSize rectSize = new MTLSize((ulong) srcW, (ulong) srcH, 1);
				IntPtr blitEncoder = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					blitEncoder,
					colorBuffer,
					0,
					0,
					srcOrigin,
					rectSize,
					mtlGetTextureFromDrawable(currentDrawable),
					0,
					0,
					dstOrigin
				);
				mtlEndEncoding(blitEncoder);
			}
			else
			{
				/* Metal doesn't have a way to blit to a destination rect,
				 * so we get to render it ourselves instead. Yayyy...
				 * -caleb
				 */

				// FIXME: What should we do about depth/stencil?

				IntPtr backbufferRenderPass = mtlMakeRenderPassDescriptor();
				mtlSetAttachmentTexture(
					mtlGetColorAttachment(backbufferRenderPass, 0),
					mtlGetTextureFromDrawable(currentDrawable)
				);
				renderCommandEncoder = mtlMakeRenderCommandEncoder(
					commandBuffer,
					backbufferRenderPass
				);

				mtlSetRenderPipelineState(
					renderCommandEncoder,
					fauxBackbufferRenderPipeline
				);

				// Update cached vertex buffer if needed
				Rectangle dstBounds = new Rectangle(dstX, dstY, dstW, dstH);
				if (fauxBackbufferDestBounds != dstBounds)
				{
					fauxBackbufferDestBounds = dstBounds;

					// Scale the coordinates to (-1, 1)
					float sx = -1 + (dstX / (float) Backbuffer.Width);
					float sy = -1 + (dstY / (float) Backbuffer.Height);
					float sw = (dstW / (float) Backbuffer.Width) * 2;
					float sh = (dstH / (float) Backbuffer.Height) * 2;

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
					renderCommandEncoder,
					fauxBackbufferVertexBuffer,
					0,
					0
				);

				mtlSetFragmentTexture(
					renderCommandEncoder,
					colorBuffer,
					0
				);

				mtlSetFragmentSamplerState(
					renderCommandEncoder,
					fauxBackbufferSamplerState,
					0
				);

				mtlDrawIndexedPrimitives(
					renderCommandEncoder,
					MTLPrimitiveType.Triangle,
					6,
					MTLIndexType.UInt16,
					fauxBackbufferIndexBuffer,
					0,
					0,
					0,
					0
				);

				mtlEndEncoding(renderCommandEncoder);
			}

			mtlPresentDrawable(commandBuffer, currentDrawable);
			mtlCommitCommandBuffer(commandBuffer);

			// Release allocations from this frame
			DrainAutoreleasePool(pool);

			// The cycle begins anew...
			pool = StartAutoreleasePool();
			commandBuffer = mtlMakeCommandBuffer(queue);
			currentDrawable = mtlNextDrawable(layer);

			IntPtr renderPass = mtlMakeRenderPassDescriptor();
			// FIXME: Set render pass info (e.g. render targets)
			renderCommandEncoder = mtlMakeRenderCommandEncoder(commandBuffer, renderPass);
			ReinitializeRenderCommandEncoder();
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
				
				mtlSetViewport(
					renderCommandEncoder,
					viewport.X,
					viewport.Y,
					viewport.Width,
					viewport.Height,
					(double) depthRangeMin,
					(double) depthRangeMax
				);
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
				mtlSetScissorRect(
					renderCommandEncoder,
					(uint) scissorRectangle.X,
					(uint) scissorRectangle.Y,
					(uint) scissorRectangle.Width,
					(uint) scissorRectangle.Height
				);
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
			throw new NotImplementedException();
		}
		
		public IGLEffect CloneEffect(IGLEffect effect)
		{
			throw new NotImplementedException();
		}

		public void ApplyEffect(IGLEffect effect, IntPtr technique, uint pass, IntPtr stateChanges)
		{
			throw new NotImplementedException();
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

		public IGLBuffer GenIndexBuffer(bool dynamic, int indexCount, IndexElementSize indexElementSize)
		{
			throw new NotImplementedException();
		}

		public IGLRenderbuffer GenRenderbuffer(int width, int height, SurfaceFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		public IGLRenderbuffer GenRenderbuffer(int width, int height, DepthFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		public IGLBuffer GenVertexBuffer(bool dynamic, int vertexCount, int vertexStride)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region SetBufferData Methods

		public void SetIndexBufferData(IGLBuffer buffer, int offsetInBytes, IntPtr data, int dataLength, SetDataOptions options)
		{
			throw new NotImplementedException();
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
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlEndEncoding(renderCommandEncoder);
			}

			IntPtr pass = mtlMakeRenderPassDescriptor();

			bool clearTarget = (options & ClearOptions.Target) == ClearOptions.Target;
			bool clearDepth = (options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer;
			bool clearStencil = (options & ClearOptions.Stencil) == ClearOptions.Stencil;

			if (clearTarget)
			{
				IntPtr colorAttachment = mtlGetColorAttachment(pass, 0);
				mtlSetAttachmentTexture(
					colorAttachment,
					currentColorBuffer
				);
				mtlSetAttachmentLoadAction(
					colorAttachment,
					MTLLoadAction.Clear
				);
				mtlSetColorAttachmentClearColor(
					colorAttachment,
					color.X,
					color.Y,
					color.Z,
					color.W
				);
				currentClearColor = color;
			}
			if (clearDepth)
			{
				IntPtr depthAttachment = mtlGetDepthAttachment(pass);
				mtlSetAttachmentTexture(
					depthAttachment,
					currentDepthStencilBuffer
				);
				mtlSetAttachmentLoadAction(
					depthAttachment,
					MTLLoadAction.Clear
				);
				mtlSetDepthAttachmentClearDepth(
					depthAttachment,
					depth
				);
				currentClearDepth = depth;
			}
			if (clearStencil)
			{
				IntPtr stencilAttachment = mtlGetStencilAttachment(pass);
					mtlSetAttachmentTexture(
					stencilAttachment,
					currentDepthStencilBuffer
				);
				mtlSetAttachmentLoadAction(
					stencilAttachment,
					MTLLoadAction.Clear
				);
				mtlSetStencilAttachmentClearStencil(
					stencilAttachment,
					stencil
				);
				currentClearStencil = stencil;
			}

			renderCommandEncoder = mtlMakeRenderCommandEncoder(commandBuffer, pass);
			ReinitializeRenderCommandEncoder();
		}

		#endregion

		#region RenderCommandEncoder Initialization Method

		private void ReinitializeRenderCommandEncoder()
		{
			mtlSetViewport(
				renderCommandEncoder,
				viewport.X,
				viewport.Y,
				viewport.Width,
				viewport.Height,
				depthRangeMin,
				depthRangeMax
			);

			mtlSetScissorRect(
				renderCommandEncoder,
				(uint) scissorRectangle.X,
				(uint) scissorRectangle.Y,
				(uint) scissorRectangle.Width,
				(uint) scissorRectangle.Height
			);

			// FIXME: Set any other renderCommandEncoder properties here
		}

		#endregion

		#region SetRenderTargets Method

		public void SetRenderTargets(RenderTargetBinding[] renderTargets, IGLRenderbuffer renderbuffer, DepthFormat depthFormat)
		{
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
				/* FIXME: DepthFloat32 is the only cross-platform depth format
				 * in Metal. Maybe we should check for feature set support so
				 * that we could use Depth24UnormStencil8 and Depth16Unorm.
				 */

				MTLPixelFormat.Invalid,			// NOPE
				MTLPixelFormat.Depth32Float,		// DepthFormat.Depth16
				MTLPixelFormat.Depth32Float,		// DepthFormat.Depth24
				MTLPixelFormat.Depth32Float_Stencil8	// DepthFormat.Depth24Stencil8
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
				mtlDevice.textureDescriptor.PixelFormat = mtlGetLayerPixelFormat(mtlDevice.layer);
				mtlDevice.textureDescriptor.Width = Width;
				mtlDevice.textureDescriptor.Height = Height;
				mtlDevice.textureDescriptor.Usage = MTLTextureUsage.RenderTarget;
				if (multiSampleCount > 0)
				{
					mtlDevice.textureDescriptor.StorageMode = MTLResourceStorageMode.Private;
					mtlDevice.textureDescriptor.TextureType = MTLTextureType.Multisample2D;
					mtlDevice.textureDescriptor.SampleCount = multiSampleCount;
				}
				ColorBuffer = mtlDevice.textureDescriptor.GenTexture();

				if (depthFormat == DepthFormat.None)
				{
					// Don't bother creating a depth/stencil buffer.
					DepthStencilBuffer = IntPtr.Zero;
				}
				else
				{
					// Create the depth/stencil buffer
					mtlDevice.textureDescriptor.PixelFormat = XNAToMTL.DepthStorage[(int) depthFormat];
					mtlDevice.textureDescriptor.StorageMode = MTLResourceStorageMode.Private;
					DepthStencilBuffer = mtlDevice.textureDescriptor.GenTexture();
				}

				// Clean up
				mtlDevice.textureDescriptor.Reset();

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
				mtlDevice.textureDescriptor.PixelFormat = mtlGetLayerPixelFormat(mtlDevice.layer);
				mtlDevice.textureDescriptor.Width = Width;
				mtlDevice.textureDescriptor.Height = Height;
				mtlDevice.textureDescriptor.Usage = MTLTextureUsage.RenderTarget;
				if (MultiSampleCount > 0)
				{
					mtlDevice.textureDescriptor.StorageMode = MTLResourceStorageMode.Private;
					mtlDevice.textureDescriptor.TextureType = MTLTextureType.Multisample2D;
					mtlDevice.textureDescriptor.SampleCount = MultiSampleCount;
				}
				ColorBuffer = mtlDevice.textureDescriptor.GenTexture();

				// Update the depth/stencil buffer, if applicable
				if (DepthFormat != DepthFormat.None)
				{
					mtlDevice.textureDescriptor.PixelFormat = XNAToMTL.DepthStorage[(int) DepthFormat];
					mtlDevice.textureDescriptor.StorageMode = MTLResourceStorageMode.Private;
					DepthStencilBuffer = mtlDevice.textureDescriptor.GenTexture();
				}

				// Clean up
				mtlDevice.textureDescriptor.Reset();

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
			// FIXME: Could we replace this with an XNA SpriteEffect or something?
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