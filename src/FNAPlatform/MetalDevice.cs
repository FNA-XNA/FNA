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
 * [3] https://computergraphics.stackexchange.com/questions/5556/how-are-mipmap-levels-computed-in-metal
 * [4] https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-setresourceminlod#remarks
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

		#region Metal Renderbuffer Container Class

		private class MetalRenderbuffer : IGLRenderbuffer
		{
			public IntPtr Handle
			{
				get;
				private set;
			}

			public SurfaceFormat Format
			{
				get;
				private set;
			}

			public DepthFormat DepthFormat
			{
				get;
				private set;
			}

			public bool IsDepthStencil
			{
				get;
				private set;
			}

			public MetalRenderbuffer(
				IntPtr handle,
				bool isDepthStencil,
				SurfaceFormat format,
				DepthFormat depthFormat
			) {
				Handle = handle;
				IsDepthStencil = isDepthStencil;
				Format = format;
				DepthFormat = depthFormat;
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

		private Color blendColor = Color.Transparent;
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
					SetEncoderBlendColor();
				}
			}
		}

		// FIXME: This feature is unsupported in Metal! Workarounds...?
		private int multisampleMask = -1; // AKA 0xFFFFFFFF
		public int MultiSampleMask
		{
			get
			{
				return multisampleMask;
			}
			set
			{
				multisampleMask = value;
			}
		}

		#endregion

		#region Depth State Variables

		private bool zEnable = false;
		private bool zWriteEnable = false;
		private CompareFunction depthFunc = CompareFunction.Less;

		#endregion

		#region Stencil State Variables

		private int stencilRef = 0;
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
					SetEncoderStencilReferenceValue();
				}
			}
		}

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

		private Rectangle scissorRectangle = new Rectangle();
		private Rectangle viewport = new Rectangle();
		private float depthRangeMin = 0.0f;
		private float depthRangeMax = 1.0f;

		#endregion

		#region Sampler State Variables

		private MetalTexture[] Textures;

		#endregion

		#region Buffer Binding Cache Variables

		private VertexDeclaration ldVertexDeclaration = null;
		private IntPtr ldPointer = IntPtr.Zero;
		private IntPtr ldEffect = IntPtr.Zero;
		private IntPtr ldTechnique = IntPtr.Zero;
		private uint ldPass = 0;

		#endregion

		#region Render Target Cache Variables

		private readonly IntPtr[] currentAttachments;
		private readonly MTLPixelFormat[] currentColorFormats;
		private DepthFormat currentDepthFormat;

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
		private IntPtr renderCommandEncoder;	// MTLRenderCommandEncoder*

		private IntPtr currentDepthStencilBuffer; // MTLTexture*
		private IntPtr currentVertexDescriptor;	// MTLVertexDescriptor*

		private ulong currentAttachmentWidth;
		private ulong currentAttachmentHeight;

		private bool renderPassDirty = false;
		private bool shouldClearColor = false;
		private bool shouldClearDepth = false;
		private bool shouldClearStencil = false;

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

		#region Private State Object Caches

		private Dictionary<long, IntPtr> VertexDescriptorCache =
			new Dictionary<long, IntPtr>();

		private Dictionary<PipelineState, IntPtr> PipelineStateCache =
			new Dictionary<PipelineState, IntPtr>();

		private Dictionary<DepthStencilState, IntPtr> DepthStencilStateCache =
			new Dictionary<DepthStencilState, IntPtr>();

		private Dictionary<SamplerState, IntPtr> SamplerStateCache =
			new Dictionary<SamplerState, IntPtr>();

		#endregion

		#region Private Render Pipeline State Variables

		private BlendState blendState;
		private DepthStencilState depthStencilState;

		#endregion

		#region Private Vertex Attribute Cache
		#endregion

		#region Private MojoShader Interop

		private IntPtr currentEffect = IntPtr.Zero;
		private IntPtr currentTechnique = IntPtr.Zero;
		private uint currentPass = 0;

		private bool renderTargetBound = false;

		private bool effectApplied = false;

		private IntPtr currentVertexShader = IntPtr.Zero;
		private IntPtr currentFragmentShader = IntPtr.Zero;
		private IntPtr currentVertUniformBuffer = IntPtr.Zero;
		private IntPtr currentFragUniformBuffer = IntPtr.Zero;

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
			IntPtr metalView
		) {
			device = MTLCreateSystemDefaultDevice();
			queue = mtlNewCommandQueue(device);
			commandBuffer = mtlMakeCommandBuffer(queue);

			// Get the CAMetalLayer for this view
			layer = SDL.SDL_Metal_GetLayer(metalView);
			mtlSetLayerDevice(layer, device);
			mtlSetLayerFramebufferOnly(layer, false);

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

			// Initialize attachments array
			currentAttachments = new IntPtr[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];
			currentColorFormats = new MTLPixelFormat[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				currentAttachments[i] = IntPtr.Zero;
				currentColorFormats[i] = MTLPixelFormat.Invalid;
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
			DrainAutoreleasePool(pool);

			// Release vertex descriptors
			foreach (IntPtr vdesc in VertexDescriptorCache.Values)
			{
				ObjCRelease(vdesc);
			}
			VertexDescriptorCache.Clear();
			VertexDescriptorCache = null;

			// Release depth stencil states
			foreach (IntPtr ds in DepthStencilStateCache.Values)
			{
				ObjCRelease(ds);
			}
			DepthStencilStateCache.Clear();
			DepthStencilStateCache = null;

			// Release pipeline states
			foreach (IntPtr pso in PipelineStateCache.Values)
			{
				ObjCRelease(pso);
			}
			PipelineStateCache.Clear();
			PipelineStateCache = null;

			// Dispose the backbuffer
			(Backbuffer as MetalBackbuffer).Dispose();

			// FIXME: "release" all retained objects
			// FIXME: null-ify variables
		}

		#endregion

		#region Window Backbuffer Reset Method

		public void ResetBackbuffer(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter
		) {
			Backbuffer.ResetFramebuffer(
				presentationParameters
			);
		}

		#endregion

		#region Window SwapBuffers Method

		public void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			/* Sometimes clearing a render target is the last draw
			 * operation. In that situation, we perform one final
			 * render pass before blitting the faux-backbuffer.
			 * -caleb
			 */
			UpdateRenderPass();

			// Finish the render pass, if there is one
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlEndEncoding(renderCommandEncoder);
				renderCommandEncoder = IntPtr.Zero;
			}

			// Get the next drawable
			IntPtr drawable = mtlNextDrawable(layer);

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

				IntPtr resolveTexture = mtlNewTextureWithDescriptor(
					device,
					resolveTextureDesc
				);
				IntPtr resolveRenderPass = mtlMakeRenderPassDescriptor();
				IntPtr colorAttachment = mtlGetColorAttachment(
					resolveRenderPass,
					0
				);

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

				IntPtr rce = mtlMakeRenderCommandEncoder(
					commandBuffer,
					resolveRenderPass
				);
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
				SDL.SDL_Metal_GetDrawableSize(
					overrideWindowHandle,
					out dstW,
					out dstH
				);
			}

			// "Blit" the backbuffer to the drawable
			CopyTextureRegion(
				colorBuffer,
				new Rectangle(srcX, srcY, srcW, srcH),
				mtlGetTextureFromDrawable(drawable),
				new Rectangle(dstX, dstY, dstW, dstH),
				overrideWindowHandle
			);

			// Submit the command buffer for presentation
			mtlPresentDrawable(commandBuffer, drawable);
			mtlCommitCommandBuffer(commandBuffer);

			// Release allocations from this frame
			DrainAutoreleasePool(pool);

			// The cycle begins anew...
			pool = StartAutoreleasePool();
			commandBuffer = mtlMakeCommandBuffer(queue);
			renderPassDirty = true;
			renderCommandEncoder = IntPtr.Zero;

			// Go back to using the faux-backbuffer
			ResetAttachments();
		}

		private void ResetAttachments()
		{
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				currentAttachments[i] = IntPtr.Zero;
			}

			MetalBackbuffer bb = (Backbuffer as MetalBackbuffer);
			currentAttachments[0] = bb.ColorBuffer;
			currentColorFormats[0] = bb.PixelFormat;
			currentDepthStencilBuffer = bb.DepthStencilBuffer;
			currentDepthFormat = bb.DepthFormat;
		}

		private void CopyTextureRegion(
			IntPtr srcTex,
			Rectangle srcRect,
			IntPtr dstTex,
			Rectangle dstRect,
			IntPtr windowHandle
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
					new MTLOrigin(
						(ulong) srcRect.X,
						(ulong) srcRect.Y,
						0
					),
					new MTLSize(
						(ulong) srcRect.Width,
						(ulong) srcRect.Height,
						1
					),
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
					SDL2.SDL.SDL_Metal_GetDrawableSize(
						windowHandle,
						out dw,
						out dh
					);
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

		#region Render Command Encoder Methods

		private void UpdateRenderPass()
		{
			if (!renderPassDirty)
			{
				// Nothing to do
				return;
			}

			// Wrap up rendering with the old encoder
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlEndEncoding(renderCommandEncoder);
			}

			// Generate the descriptor
			IntPtr passDesc = mtlMakeRenderPassDescriptor();

			// Clear color
			// FIXME: How does this work for multiple bound render targets?
			IntPtr colorAttachment = mtlGetColorAttachment(passDesc, 0);
			mtlSetAttachmentTexture(
				colorAttachment,
				currentAttachments[0]
			);
			if (shouldClearColor)
			{
				mtlSetAttachmentLoadAction(
					colorAttachment,
					MTLLoadAction.Clear
				);
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
				mtlSetAttachmentLoadAction(
					colorAttachment,
					MTLLoadAction.Load
				);
			}

			// Clear depth
			IntPtr depthAttachment = mtlGetDepthAttachment(passDesc);
			mtlSetAttachmentTexture(
				depthAttachment,
				currentDepthStencilBuffer
			);
			if (shouldClearDepth)
			{
				mtlSetAttachmentLoadAction(
					depthAttachment,
					MTLLoadAction.Clear
				);
				mtlSetDepthAttachmentClearDepth(
					depthAttachment,
					clearDepth
				);
				shouldClearDepth = false;
			}
			else
			{
				mtlSetAttachmentLoadAction(
					depthAttachment,
					MTLLoadAction.Load
				);
			}

			// Clear stencil
			if (currentDepthFormat == DepthFormat.Depth24Stencil8)
			{
				IntPtr stencilAttachment = mtlGetStencilAttachment(passDesc);
				mtlSetAttachmentTexture(
					stencilAttachment,
					currentDepthStencilBuffer
				);
				if (shouldClearStencil)
				{
					mtlSetAttachmentLoadAction(
						stencilAttachment,
						MTLLoadAction.Clear
					);
					mtlSetStencilAttachmentClearStencil(
						stencilAttachment,
						clearStencil
					);
					shouldClearStencil = false;
				}
				else
				{
					mtlSetAttachmentLoadAction(
						stencilAttachment,
						MTLLoadAction.Load
					);
				}
			}

			// Get attachment size
			// FIXME: Is this right...?
			currentAttachmentWidth = mtlGetTextureWidth(currentAttachments[0]);
			currentAttachmentHeight = mtlGetTextureHeight(currentAttachments[0]);

			// Make a new encoder
			renderCommandEncoder = mtlMakeRenderCommandEncoder(
				commandBuffer,
				passDesc
			);

			SetEncoderViewport();
			SetEncoderScissorRect();
			SetEncoderBlendColor();
			SetEncoderStencilReferenceValue();
			SetEncoderCullMode();
			SetEncoderFillMode();
			SetEncoderDepthBias();

			// Reset the flag
			renderPassDirty = false;
		}

		private void SetEncoderStencilReferenceValue()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlSetStencilReferenceValue(
					renderCommandEncoder,
					(ulong) stencilRef
				);
			}
		}

		private void SetEncoderBlendColor()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlSetBlendColor(
					renderCommandEncoder,
					blendColor.R / 255f,
					blendColor.G / 255f,
					blendColor.B / 255f,
					blendColor.A / 255f
				);
			}
		}

		private void SetEncoderViewport()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
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
		}

		private void SetEncoderCullMode()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlSetCullMode(
					renderCommandEncoder,
					XNAToMTL.CullingEnabled[(int) cullFrontFace]
				);
			}
		}

		private void SetEncoderFillMode()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlSetTriangleFillMode(
					renderCommandEncoder,
					XNAToMTL.FillMode[(int) fillMode]
				);
			}
		}

		private void SetEncoderDepthBias()
		{
			if (renderCommandEncoder != null)
			{
				mtlSetDepthBias(
					renderCommandEncoder,
					depthBias,
					slopeScaleDepthBias,
					0.0f // no clamp
				);
			}
		}

		private void SetEncoderScissorRect()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				if (!scissorTestEnable)
				{
					// Set to the default scissor rect
					mtlSetScissorRect(
						renderCommandEncoder,
						0,
						0,
						currentAttachmentWidth,
						currentAttachmentHeight
					);
				}
				else
				{
					mtlSetScissorRect(
						renderCommandEncoder,
						(uint) scissorRectangle.X,
						(uint) scissorRectangle.Y,
						(uint) scissorRectangle.Width,
						(uint) scissorRectangle.Height
					);
				}
			}
		}

		#endregion

		#region Metal Object Disposal Wrappers

		public void AddDisposeEffect(IGLEffect effect)
		{
			DeleteEffect(effect);
		}

		public void AddDisposeIndexBuffer(IGLBuffer buffer)
		{
			DeleteIndexBuffer(buffer);
		}

		public void AddDisposeQuery(IGLQuery query)
		{
			DeleteQuery(query);
		}

		public void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			DeleteRenderbuffer(renderbuffer);
		}

		public void AddDisposeTexture(IGLTexture texture)
		{
			DeleteTexture(texture);
		}

		public void AddDisposeVertexBuffer(IGLBuffer buffer)
		{
			DeleteVertexBuffer(buffer);
		}

		#endregion

		#region String Marker Method

		public void SetStringMarker(string text)
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlInsertDebugSignpost(renderCommandEncoder, text);
			}
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
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToMTL.IndexType[(int) indices.IndexElementSize],
				(indices.buffer as MetalBuffer).Handle,
				(ulong) minVertexIndex,
				(ulong) instanceCount,
				baseVertex,
				0
			);
		}

		public void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		) {
			mtlDrawPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				(ulong) vertexStart,
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount)
			);
		}

		public void DrawUserIndexedPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int numVertices,
			IntPtr indexData,
			int indexOffset,
			IndexElementSize indexElementSize,
			int primitiveCount
		) {
			FNALoggerEXT.LogError("Client-side arrays are not allowed in Metal.");
			throw new NotSupportedException();
		}

		public void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		) {
			FNALoggerEXT.LogError("Client-side arrays are not allowed in Metal.");
			throw new NotSupportedException();
		}

		#endregion

		#region State Management Methods

		public void SetPresentationInterval(PresentInterval interval)
		{
			string platform = SDL.SDL_GetPlatform();
			if (platform == "iOS" || platform == "tvOS")
			{
				FNALoggerEXT.LogWarn(
					"Cannot set presentation interval on iOS/tvOS! " +
					"Only vsync is supported."
				);
				return;
			}

			// macOS-only options
			if (interval == PresentInterval.Default || interval == PresentInterval.One)
			{
				mtlSetDisplaySyncEnabled(layer, true);
			}
			else if (interval == PresentInterval.Immediate)
			{
				mtlSetDisplaySyncEnabled(layer, false);
			}
			else if (interval == PresentInterval.Two)
			{
				/* There is no support for present-every-other-frame
				 * in Metal. We *could* work around this, but do
				 * any games actually use this mode...?
				 * -caleb
				 */
				mtlSetDisplaySyncEnabled(layer, true);
			}
			else
			{
				throw new NotSupportedException("Unrecognized PresentInterval!");
			}
		}

		public void SetViewport(Viewport vp)
		{
			if (	vp.Bounds != viewport ||
				vp.MinDepth != depthRangeMin ||
				vp.MaxDepth != depthRangeMax	)
			{
				viewport = vp.Bounds;
				depthRangeMin = vp.MinDepth;
				depthRangeMax = vp.MaxDepth;
				SetEncoderViewport(); // Dynamic state!
			}
		}

		public void SetScissorRect(Rectangle scissorRect)
		{
			if (scissorRectangle != scissorRect)
			{
				scissorRectangle = scissorRect;
				SetEncoderScissorRect(); // Dynamic state!
			}
		}

		public void ApplyRasterizerState(RasterizerState rasterizerState)
		{
			if (rasterizerState.ScissorTestEnable != scissorTestEnable)
			{
				scissorTestEnable = rasterizerState.ScissorTestEnable;
				SetEncoderScissorRect(); // Dynamic state!
			}

			if (rasterizerState.CullMode != cullFrontFace)
			{
				cullFrontFace = rasterizerState.CullMode;
				SetEncoderCullMode(); // Dynamic state!
			}

			if (rasterizerState.FillMode != fillMode)
			{
				fillMode = rasterizerState.FillMode;
				SetEncoderFillMode(); // Dynamic state!
			}

			float realDepthBias = rasterizerState.DepthBias * XNAToMTL.DepthBiasScale[
				renderTargetBound ?
					(int) currentDepthFormat :
					(int) Backbuffer.DepthFormat
			];
			if (	realDepthBias != depthBias ||
				rasterizerState.SlopeScaleDepthBias != slopeScaleDepthBias	)
			{
				depthBias = realDepthBias;
				slopeScaleDepthBias = rasterizerState.SlopeScaleDepthBias;
				SetEncoderDepthBias(); // Dynamic state!
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

			// Update the texture info
			tex.WrapS = sampler.AddressU;
			tex.WrapT = sampler.AddressV;
			tex.WrapR = sampler.AddressW;
			tex.Filter = sampler.Filter;
			tex.Anisotropy = sampler.MaxAnisotropy;
			tex.MaxMipmapLevel = sampler.MaxMipLevel;
			tex.LODBias = sampler.MipMapLevelOfDetailBias;
			tex.SamplerHandle = FetchSamplerState(sampler);
		}

		public void SetBlendState(BlendState blendState)
		{
			this.blendState = blendState;
			SetEncoderBlendColor(); // Dynamic state!
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			this.depthStencilState = depthStencilState;
			SetEncoderStencilReferenceValue(); // Dynamic state!
		}

		#endregion

		#region PipelineState Helper Struct

		private struct PipelineState
		{
			public IntPtr vertexFunction;
			public IntPtr fragmentFunction;
			public IntPtr vertexDescriptor;
			public MTLPixelFormat[] colorFormats;
			public MTLPixelFormat depthFormat;
			public BlendState blend;
			public DepthStencilState depthStencil;
		}

		#endregion

		#region State Creation/Retrieval Methods

		private IntPtr FetchRenderPipeline()
		{
			PipelineState state = new PipelineState
			{
				vertexFunction = MojoShader.MOJOSHADER_mtlGetFunctionHandle(
					currentVertexShader
				),
				fragmentFunction = MojoShader.MOJOSHADER_mtlGetFunctionHandle(
					currentFragmentShader
				),
				vertexDescriptor = currentVertexDescriptor,
				colorFormats = currentColorFormats,
				depthFormat = XNAToMTL.DepthFormat[(int) currentDepthFormat],
				blend = blendState,
				depthStencil = depthStencilState
			};

			// Can we just reuse an existing pipeline?
			IntPtr pipeline = IntPtr.Zero;
			if (PipelineStateCache.TryGetValue(state, out pipeline))
			{
				// We have this state already cached!
				return pipeline;
			}

			// We'll have to make a new pipeline...
			IntPtr pipelineDesc = mtlNewRenderPipelineDescriptor();
			mtlSetPipelineVertexFunction(
				pipelineDesc,
				state.vertexFunction
			);
			mtlSetPipelineFragmentFunction(
				pipelineDesc,
				state.fragmentFunction
			);
			mtlSetPipelineVertexDescriptor(
				pipelineDesc,
				state.vertexDescriptor
			);
			mtlSetDepthAttachmentPixelFormat(
				pipelineDesc,
				XNAToMTL.DepthFormat[(int) currentDepthFormat]
			);
			if (currentDepthFormat == DepthFormat.Depth24Stencil8)
			{
				mtlSetStencilAttachmentPixelFormat(
					pipelineDesc,
					XNAToMTL.DepthFormat[(int) currentDepthFormat]
				);
			}

			// Apply the blend state
			bool alphaBlendEnable = !(
				blendState.ColorSourceBlend == Blend.One &&
				blendState.ColorDestinationBlend == Blend.Zero &&
				blendState.AlphaSourceBlend == Blend.One &&
				blendState.AlphaDestinationBlend == Blend.Zero
			);
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (currentAttachments[i] == IntPtr.Zero)
				{
					// There's no attachment bound at this index.
					continue;
				}

				IntPtr colorAttachment = mtlGetColorAttachment(
					pipelineDesc,
					(ulong) i
				);
				mtlSetAttachmentPixelFormat(
					colorAttachment,
					currentColorFormats[i]
				);
				mtlSetAttachmentBlendingEnabled(
					colorAttachment,
					alphaBlendEnable
				);
				mtlSetAttachmentSourceRGBBlendFactor(
					colorAttachment,
					XNAToMTL.BlendMode[
						(int) blendState.ColorSourceBlend
					]
				);
				mtlSetAttachmentDestinationRGBBlendFactor(
					colorAttachment,
					XNAToMTL.BlendMode[
						(int) blendState.ColorDestinationBlend
					]
				);
				mtlSetAttachmentSourceAlphaBlendFactor(
					colorAttachment,
					XNAToMTL.BlendMode[
						(int) blendState.AlphaSourceBlend
					]
				);
				mtlSetAttachmentDestinationAlphaBlendFactor(
					colorAttachment,
					XNAToMTL.BlendMode[
						(int) blendState.AlphaDestinationBlend
					]
				);
				mtlSetAttachmentRGBBlendOperation(
					colorAttachment,
					XNAToMTL.BlendOperation[
						(int) blendState.ColorBlendFunction
					]
				);
				mtlSetAttachmentAlphaBlendOperation(
					colorAttachment,
					XNAToMTL.BlendOperation[
						(int) blendState.AlphaBlendFunction
					]
				);
				mtlSetAttachmentWriteMask(
					colorAttachment,
					(ulong) blendState.ColorWriteChannels
				);
				/* FIXME: So how exactly do we factor in
				* COLORWRITEENABLE for buffer 0? Do we just assume that
				* the default is just buffer 0, and all other calls
				* update the other write masks?
				*/
				mtlSetAttachmentWriteMask(
					mtlGetColorAttachment(pipelineDesc, 1),
					(ulong) blendState.ColorWriteChannels1
				);
				mtlSetAttachmentWriteMask(
					mtlGetColorAttachment(pipelineDesc, 2),
					(ulong) blendState.ColorWriteChannels2
				);
				mtlSetAttachmentWriteMask(
					mtlGetColorAttachment(pipelineDesc, 3),
					(ulong) blendState.ColorWriteChannels3
				);
			}

			// Bake the render pipeline!
			IntPtr pipelineState = mtlNewRenderPipelineStateWithDescriptor(
				device,
				pipelineDesc
			);
			PipelineStateCache[state] = pipelineState;

			// Clean up
			ObjCRelease(pipelineDesc);

			// Return the pipeline!
			return pipelineState;
		}

		private IntPtr FetchDepthStencilState()
		{
			// Can we just reuse an existing state?
			IntPtr state = IntPtr.Zero;
			if (DepthStencilStateCache.TryGetValue(depthStencilState, out state))
			{
				// This state has already been cached!
				return state;
			}

			// We have to make a new DepthStencilState...
			IntPtr dsDesc = mtlNewDepthStencilDescriptor();
			mtlSetDepthCompareFunction(
				dsDesc,
				XNAToMTL.CompareFunc[(int) depthStencilState.DepthBufferFunction]
			);
			mtlSetDepthWriteEnabled(
				dsDesc,
				depthStencilState.DepthBufferWriteEnable
			);

			// Create stencil descriptors
			IntPtr front = IntPtr.Zero;
			IntPtr back = IntPtr.Zero;

			if (depthStencilState.StencilEnable)
			{
				front = mtlNewStencilDescriptor();
				mtlSetStencilFailureOperation(
					front,
					XNAToMTL.StencilOp[(int) depthStencilState.StencilFail]
				);
				mtlSetDepthFailureOperation(
					front,
					XNAToMTL.StencilOp[(int) depthStencilState.StencilDepthBufferFail]
				);
				mtlSetDepthStencilPassOperation(
					front,
					XNAToMTL.StencilOp[(int) depthStencilState.StencilPass]
				);
				mtlSetStencilCompareFunction(
					front,
					XNAToMTL.CompareFunc[(int) depthStencilState.StencilFunction]
				);
				mtlSetStencilReadMask(
					front,
					depthStencilState.StencilMask
				);
				mtlSetStencilWriteMask(
					front,
					depthStencilState.StencilWriteMask
				);

				if (!depthStencilState.TwoSidedStencilMode)
				{
					back = front;
				}
			}

			if (front != back)
			{
				back = mtlNewStencilDescriptor();
				mtlSetStencilFailureOperation(
					back,
					XNAToMTL.StencilOp[(int) depthStencilState.CounterClockwiseStencilFail]
				);
				mtlSetDepthFailureOperation(
					back,
					XNAToMTL.StencilOp[(int) depthStencilState.CounterClockwiseStencilDepthBufferFail]
				);
				mtlSetDepthStencilPassOperation(
					back,
					XNAToMTL.StencilOp[(int) depthStencilState.CounterClockwiseStencilPass]
				);
				mtlSetStencilCompareFunction(
					back,
					XNAToMTL.CompareFunc[(int) depthStencilState.CounterClockwiseStencilFunction]
				);
				mtlSetStencilReadMask(
					back,
					depthStencilState.StencilMask
				);
				mtlSetStencilWriteMask(
					back,
					depthStencilState.StencilWriteMask
				);
			}

			mtlSetFrontFaceStencil(
				dsDesc,
				front
			);
			mtlSetBackFaceStencil(
				dsDesc,
				back
			);

			// Bake the state!
			state = mtlNewDepthStencilStateWithDescriptor(
				device,
				dsDesc
			);
			DepthStencilStateCache[depthStencilState] = state;

			// Clean up
			ObjCRelease(dsDesc);
			ObjCRelease(back);
			ObjCRelease(front);

			// Return the state!
			return state;
		}

		private IntPtr FetchSamplerState(SamplerState samplerState)
		{
			IntPtr state = IntPtr.Zero;
			if (SamplerStateCache.TryGetValue(samplerState, out state))
			{
				// The value is already cached!
				return state;
			}

			// We have to make a new sampler state...
			IntPtr samplerDesc = mtlNewSamplerDescriptor();

			mtlSetSampler_sAddressMode(
				samplerDesc,
				XNAToMTL.Wrap[(int) samplerState.AddressU]
			);
			mtlSetSampler_tAddressMode(
				samplerDesc,
				XNAToMTL.Wrap[(int) samplerState.AddressV]
			);
			mtlSetSampler_rAddressMode(
				samplerDesc,
				XNAToMTL.Wrap[(int) samplerState.AddressW]
			);
			mtlSetSamplerMagFilter(
				samplerDesc,
				XNAToMTL.MagFilter[(int) samplerState.Filter]
			);
			mtlSetSamplerMinFilter(
				samplerDesc,
				XNAToMTL.MinFilter[(int) samplerState.Filter]
			);
			if (samplerState.MaxMipLevel > 0)
			{
				mtlSetSamplerMipFilter(
					samplerDesc,
					XNAToMTL.MipFilter[(int) samplerState.Filter]
				);
			}
			mtlSetSamplerLodMinClamp(
				samplerDesc,
				samplerState.MaxMipLevel
			);

			// Anisotropy must be in the range [1, 16]
			ulong scaledAnisotropy = 1 + (ulong) (samplerState.MaxAnisotropy * 15);
			mtlSetSamplerMaxAnisotropy(
				samplerDesc,
				(samplerState.Filter == TextureFilter.Anisotropic) ?
					scaledAnisotropy :
					1
			);

			/* FIXME:
			 * The only way to set lod bias in metal is via the MSL
			 * bias() function in a shader. So we can't do:
			 *
			 * 	mtlSetSamplerLodBias(
			 *		samplerDesc,
			 *		samplerState.MipMapLevelOfDetailBias
			 *	);
			 *
			 * What should we do instead?
			 * -caleb
			 */

			// Bake the sampler state!
			state = mtlNewSamplerStateWithDescriptor(
				device,
				samplerDesc
			);
			SamplerStateCache[samplerState] = state;

			// Clean up
			ObjCRelease(samplerDesc);

			// Return the sampler state!
			return state;
		}

		private IntPtr FetchVertexDescriptor(
			VertexBufferBinding[] bindings,
			int numBindings
		) {
			// Get the binding hash value
			long hash = 0;
			for (int i = 0; i < numBindings; i += 1)
			{
				hash += (long) bindings[i].GetHashCode();
			}

			// Try to get the descriptor from the cache
			IntPtr descriptor;
			if (VertexDescriptorCache.TryGetValue(hash, out descriptor))
			{
				// The value is already cached!
				return descriptor;
			}

			// We have to make a new vertex descriptor...
			descriptor = mtlMakeVertexDescriptor();
			ObjCRetain(descriptor); // Make sure this doesn't get drained

			for (int i = numBindings - 1; i >= 0; i -= 1)
			{
				// Describe vertex attributes
				VertexDeclaration vertexDeclaration = bindings[i].VertexBuffer.VertexDeclaration;
				for (int j = 0; j < vertexDeclaration.elements.Length; j += 1)
				{
					VertexElement element = vertexDeclaration.elements[j];

					int attribLoc = MojoShader.MOJOSHADER_mtlGetVertexAttribLocation(
						currentVertexShader,
						XNAToMTL.VertexAttribUsage[(int) element.VertexElementUsage],
						element.UsageIndex
					);
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
			VertexDescriptorCache[hash] = descriptor;
			return descriptor;
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

		private void DeleteEffect(IGLEffect effect)
		{
			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			if (mtlEffectData == currentEffect)
			{
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectEnd(
					currentEffect,
					out currentVertexShader,
					out currentFragmentShader,
					out currentVertUniformBuffer,
					out currentFragUniformBuffer
				);
				currentEffect = IntPtr.Zero;
				currentTechnique = IntPtr.Zero;
				currentPass = 0;
				currentVertexShader = IntPtr.Zero;
				currentFragmentShader = IntPtr.Zero;
				currentVertUniformBuffer = IntPtr.Zero;
				currentFragUniformBuffer = IntPtr.Zero;
			}
			MojoShader.MOJOSHADER_mtlDeleteEffect(mtlEffectData);
			MojoShader.MOJOSHADER_freeEffect(effect.EffectData);
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
						out currentVertUniformBuffer,
						out currentFragUniformBuffer
					);
					return;
				}
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectBeginPass(
					currentEffect,
					pass,
					out currentVertexShader,
					out currentFragmentShader,
					out currentVertUniformBuffer,
					out currentFragUniformBuffer
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
					out currentVertUniformBuffer,
					out currentFragUniformBuffer
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
				out currentVertUniformBuffer,
				out currentFragUniformBuffer
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

		private void BindResources()
		{
			// Bind textures and their sampler states
			for (int i = 0; i < Textures.Length; i += 1)
			{
				mtlSetFragmentTexture(
					renderCommandEncoder,
					Textures[i].Handle,
					(ulong) i
				);
				mtlSetFragmentSamplerState(
					renderCommandEncoder,
					Textures[i].SamplerHandle,
					(ulong) i
				);
			}

			// Bind the uniform buffers
			if (currentVertUniformBuffer != IntPtr.Zero)
			{
				mtlSetVertexBuffer(
					renderCommandEncoder,
					currentVertUniformBuffer,
					0,
					16 // In MojoShader output it's always 16 for some reason
				);
			}
			if (currentFragUniformBuffer != IntPtr.Zero)
			{
				mtlSetFragmentBuffer(
					renderCommandEncoder,
					currentFragUniformBuffer,
					0,
					16 // In MojoShader output it's always 16 for some reason
				);
			}

			// Bind the depth-stencil state
			if (currentDepthStencilBuffer != IntPtr.Zero)
			{
				IntPtr depthStencilState = FetchDepthStencilState();
				mtlSetDepthStencilState(
					renderCommandEncoder,
					depthStencilState
				);
			}

			// Finally, bind the pipeline state
			IntPtr pipelineState = FetchRenderPipeline();
			mtlSetRenderPipelineState(
				renderCommandEncoder,
				pipelineState
			);
		}

		// FIXME: Update to handle overlapping attributes
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
				currentVertexDescriptor = FetchVertexDescriptor(
					bindings,
					numBindings
				);

				ldEffect = currentEffect;
				ldTechnique = currentTechnique;
				ldPass = currentPass;
				effectApplied = false;
				ldVertexDeclaration = null;
				ldPointer = IntPtr.Zero;
			}

			// Prepare for rendering
			UpdateRenderPass();
			for (int i = 0; i < bindings.Length; i += 1)
			{
				VertexBuffer vertexBuffer = bindings[i].VertexBuffer;
				if (vertexBuffer != null)
				{
					mtlSetVertexBuffer(
						renderCommandEncoder,
						(vertexBuffer.buffer as MetalBuffer).Handle,
						(ulong) bindings[i].VertexOffset,
						(ulong) i
					);
				}
			}
			BindResources();
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
			FNALoggerEXT.LogError("Client-side arrays are not allowed in Metal.");
			throw new NotSupportedException();
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

		#region Renderbuffer Methods

		public IGLRenderbuffer GenRenderbuffer(int width, int height, SurfaceFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		public IGLRenderbuffer GenRenderbuffer(int width, int height, DepthFormat format, int multiSampleCount)
		{
			throw new NotImplementedException();
		}

		private void DeleteRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			// uint handle = (renderbuffer as OpenGLRenderbuffer).Handle;

			// // Check color attachments
			// for (int i = 0; i < currentAttachments.Length; i += 1)
			// {
			// 	if (handle == currentAttachments[i])
			// 	{
			// 		// Force an attachment update, this no longer exists!
			// 		currentAttachments[i] = uint.MaxValue;
			// 	}
			// }

			// // Check depth/stencil attachment
			// if (handle == currentRenderbuffer)
			// {
			// 	// Force a renderbuffer update, this no longer exists!
			// 	currentRenderbuffer = uint.MaxValue;
			// }

			// // Finally.
			// glDeleteRenderbuffers(1, ref handle);
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
				memset(
					(buffer as MetalBuffer).Contents,
					(IntPtr) 0,
					buffer.BufferSize
				);
			}

			IntPtr dst = IntPtr.Add(
				(buffer as MetalBuffer).Contents,
				offsetInBytes
			);
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

		#region DeleteBuffer Methods

		private void DeleteVertexBuffer(IGLBuffer buffer)
		{
			// uint handle = (buffer as OpenGLBuffer).Handle;
			// if (handle == currentVertexBuffer)
			// {
			// 	glBindBuffer(GLenum.GL_ARRAY_BUFFER, 0);
			// 	currentVertexBuffer = 0;
			// }
			// for (int i = 0; i < attributes.Length; i += 1)
			// {
			// 	if (handle == attributes[i].CurrentBuffer)
			// 	{
			// 		// Force the next vertex attrib update!
			// 		attributes[i].CurrentBuffer = uint.MaxValue;
			// 	}
			// }
			// glDeleteBuffers(1, ref handle);
		}

		private void DeleteIndexBuffer(IGLBuffer buffer)
		{
			// uint handle = (buffer as OpenGLBuffer).Handle;
			// if (handle == currentIndexBuffer)
			// {
			// 	glBindBuffer(GLenum.GL_ELEMENT_ARRAY_BUFFER, 0);
			// 	currentIndexBuffer = 0;
			// }
			// glDeleteBuffers(1, ref handle);
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

		#region DeleteTexture Method

		private void DeleteTexture(IGLTexture texture)
		{
			// IntPtr handle = (texture as MetalTexture).Handle;
			// for (int i = 0; i < currentAttachments.Length; i += 1)
			// {
			// 	if (handle == currentAttachments[i].Handle)
			// 	{
			// 		// Force an attachment update -- this no longer exists!
			// 		currentAttachments[i] = MetalTexture.NullTexture;
			// 	}
			// }
			// ObjCRelease(handle);

			// uint handle = (texture as OpenGLTexture).Handle;
			// for (int i = 0; i < currentAttachments.Length; i += 1)
			// {
			// 	if (handle == currentAttachments[i])
			// 	{
			// 		// Force an attachment update, this no longer exists!
			// 		currentAttachments[i] = uint.MaxValue;
			// 	}
			// }
			// glDeleteTextures(1, ref handle);
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

		public void SetTextureDataYUV(Texture2D[] textures, IntPtr ptr)
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

		private void DeleteQuery(IGLQuery query)
		{
			throw new NotImplementedException();
			// uint handle = (query as OpenGLQuery).Handle;
			// glDeleteQueries(
			// 	1,
			// 	ref handle
			// );
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

			public static readonly MTLPixelFormat[] DepthFormat = new MTLPixelFormat[]
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
				return (
					element.VertexElementUsage == VertexElementUsage.Color ||
					element.VertexElementFormat == VertexElementFormat.NormalizedShort2 ||
					element.VertexElementFormat == VertexElementFormat.NormalizedShort4
				);
			}

			public static readonly MTLIndexType[] IndexType = new MTLIndexType[]
			{
				MTLIndexType.UInt16,	// IndexElementSize.SixteenBits
				MTLIndexType.UInt32	// IndexElementSize.ThirtyTwoBits
			};

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

			public static readonly MTLCompareFunction[] CompareFunc = new MTLCompareFunction[]
			{
				MTLCompareFunction.Always,	// CompareFunction.Always
				MTLCompareFunction.Never,	// CompareFunction.Never
				MTLCompareFunction.Less,	// CompareFunction.Less
				MTLCompareFunction.LessEqual,	// CompareFunction.LessEqual
				MTLCompareFunction.Equal,	// CompareFunction.Equal
				MTLCompareFunction.GreaterEqual,// CompareFunction.GreaterEqual
				MTLCompareFunction.Greater,	// CompareFunction.Greater
				MTLCompareFunction.NotEqual	// CompareFunction.NotEqual
			};

			public static readonly MTLStencilOperation[] StencilOp = new MTLStencilOperation[]
			{
				MTLStencilOperation.Keep,		// StencilOperation.Keep
				MTLStencilOperation.Zero,		// StencilOperation.Zero
				MTLStencilOperation.Replace,		// StencilOperation.Replace
				MTLStencilOperation.IncrementWrap,	// StencilOperation.Increment
				MTLStencilOperation.DecrementWrap,	// StencilOperation.Decrement
				MTLStencilOperation.IncrementClamp,	// StencilOperation.IncrementSaturation
				MTLStencilOperation.DecrementClamp,	// StencilOperation.DecrementSaturation
				MTLStencilOperation.Invert		// StencilOperation.Invert
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
				MTLCullMode.Front,		// CullMode.CullClockwiseFace
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

			public static readonly MTLSamplerMipFilter[] MipFilter = new MTLSamplerMipFilter[]
			{
				MTLSamplerMipFilter.Linear,	// TextureFilter.Linear
				MTLSamplerMipFilter.Nearest,	// TextureFilter.Point
				MTLSamplerMipFilter.Linear,	// TextureFilter.Anisotropic
				MTLSamplerMipFilter.Nearest,	// TextureFilter.LinearMipPoint
				MTLSamplerMipFilter.Linear,	// TextureFilter.PointMipLinear
				MTLSamplerMipFilter.Linear,	// TextureFilter.MinLinearMagPointMipLinear
				MTLSamplerMipFilter.Nearest,	// TextureFilter.MinLinearMagPointMipPoint
				MTLSamplerMipFilter.Linear,	// TextureFilter.MinPointMagLinearMipLinear
				MTLSamplerMipFilter.Nearest	// TextureFilter.MinPointMagLinearMipPoint
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

			public static ulong PrimitiveVerts(PrimitiveType primitiveType, int primitiveCount)
			{
				switch (primitiveType)
				{
					case PrimitiveType.TriangleList:
						return (ulong) (primitiveCount * 3);
					case PrimitiveType.TriangleStrip:
						return (ulong) (primitiveCount + 2);
					case PrimitiveType.LineList:
						return (ulong) (primitiveCount * 2);
					case PrimitiveType.LineStrip:
						return (ulong) (primitiveCount + 1);
					case PrimitiveType.PointListEXT:
						return (ulong) primitiveCount;
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

			public MTLPixelFormat PixelFormat
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

			public IntPtr ColorBuffer = IntPtr.Zero;
			public IntPtr DepthStencilBuffer = IntPtr.Zero;
			
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

				PixelFormat = mtlGetLayerPixelFormat(mtlDevice.layer);

				// Generate the color buffer
				IntPtr colorBufferDesc = mtlMakeTexture2DDescriptor(
					PixelFormat,
					(uint) Width,
					(uint) Height,
					false
				);
				mtlSetTextureUsage(
					colorBufferDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);

				if (multiSampleCount > 0)
				{
					mtlSetStorageMode(
						colorBufferDesc,
						MTLResourceStorageMode.Private
					);
					mtlSetTextureType(
						colorBufferDesc,
						MTLTextureType.Multisample2D
					);
					mtlSetTextureSampleCount(
						colorBufferDesc,
						multiSampleCount
					);
				}

				ColorBuffer = mtlNewTextureWithDescriptor(
					device.device,
					colorBufferDesc
				);

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
						XNAToMTL.DepthFormat[(int) depthFormat],
						(uint) Width,
						(uint) Height,
						false
					);
					mtlSetStorageMode(
						depthStencilBufferDesc,
						MTLResourceStorageMode.Private
					);
					mtlSetTextureUsage(
						depthStencilBufferDesc,
						MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
					);
					DepthStencilBuffer = mtlNewTextureWithDescriptor(
						device.device,
						depthStencilBufferDesc
					);
				}
			}

			public void Dispose()
			{
				ObjCRelease(ColorBuffer);
				ColorBuffer = IntPtr.Zero;

				ObjCRelease(DepthStencilBuffer);
				DepthStencilBuffer = IntPtr.Zero;
			}

			public void ResetFramebuffer(
				PresentationParameters presentationParameters
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
				mtlSetTextureUsage(
					colorBufferDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);
				if (MultiSampleCount > 0)
				{
					mtlSetStorageMode(
						colorBufferDesc,
						MTLResourceStorageMode.Private
					);
					mtlSetTextureType(
						colorBufferDesc,
						MTLTextureType.Multisample2D
					);
					mtlSetTextureSampleCount(
						colorBufferDesc,
						MultiSampleCount
					);
				}
				ColorBuffer = mtlNewTextureWithDescriptor(
					mtlDevice.device,
					colorBufferDesc
				);

				// Update the depth/stencil buffer, if applicable
				if (DepthFormat != DepthFormat.None)
				{
					IntPtr depthStencilBufferDesc = mtlMakeTexture2DDescriptor(
						XNAToMTL.DepthFormat[(int) DepthFormat],
						(uint) Width,
						(uint) Height,
						false
					);
					mtlSetStorageMode(
						depthStencilBufferDesc,
						MTLResourceStorageMode.Private
					);
					mtlSetTextureUsage(
						depthStencilBufferDesc,
						MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
					);
					DepthStencilBuffer = mtlNewTextureWithDescriptor(
						mtlDevice.device,
						depthStencilBufferDesc
					);
				}

				// This is the default render target
				mtlDevice.ResetAttachments();
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
			// FIXME: Combine this and the index buffer into one MTLBuffer
			fauxBackbufferVertexBuffer = mtlNewBufferWithLength(
				device,
				16 * sizeof(float)
			);

			// Create and fill the index buffer
			fauxBackbufferIndexBuffer = mtlNewBufferWithLength(
				device,
				6 * sizeof(ushort)
			);

			ushort[] indices = new ushort[]
			{
				0, 1, 3,
				1, 2, 3
			};
			GCHandle indicesPinned = GCHandle.Alloc(indices, GCHandleType.Pinned);
			memcpy(
				mtlGetBufferContentsPtr(fauxBackbufferIndexBuffer),
				indicesPinned.AddrOfPinnedObject(),
				(IntPtr) (6 * sizeof(ushort))
			);
			indicesPinned.Free();

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
				fragmentShader(
					VertexOut in [[stage_in]],
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
			ObjCRelease(samplerDescriptor);

			// Create a render pipeline for rendering the backbuffer
			IntPtr pipelineDesc = mtlNewRenderPipelineDescriptor();
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
			ObjCRelease(pipelineDesc);
		}

		#endregion
	}
}