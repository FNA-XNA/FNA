#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
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

			public bool IsPrivate
			{
				get;
				private set;
			}

			public SurfaceFormat Format;
			public TextureAddressMode WrapS;
			public TextureAddressMode WrapT;
			public TextureAddressMode WrapR;
			public TextureFilter Filter;
			public float Anisotropy;
			public int MaxMipmapLevel;
			public float LODBias;

			public MetalTexture(
				IntPtr handle,
				int width,
				int height,
				SurfaceFormat format,
				int levelCount,
				bool isPrivate
			) {
				Handle = handle;
				Width = width;
				Height = height;
				Format = format;
				HasMipmaps = levelCount > 1;
				IsPrivate = isPrivate;

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

			public void Dispose()
			{
				objc_release(Handle);
				Handle = IntPtr.Zero;
			}
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

			public IntPtr MultiSampleHandle
			{
				get;
				private set;
			}

			public MTLPixelFormat PixelFormat
			{
				get;
				private set;
			}

			public int MultiSampleCount
			{
				get;
				private set;
			}

			public MetalRenderbuffer(
				IntPtr handle,
				MTLPixelFormat pixelFormat,
				int multiSampleCount,
				IntPtr multiSampleHandle
			) {
				Handle = handle;
				PixelFormat = pixelFormat;
				MultiSampleCount = multiSampleCount;
				MultiSampleHandle = multiSampleHandle;
			}

			public void Dispose()
			{
				if (MultiSampleHandle == IntPtr.Zero)
				{
					objc_release(Handle);
					Handle = IntPtr.Zero;
				}
				else
				{
					objc_release(MultiSampleHandle);
					MultiSampleHandle = IntPtr.Zero;

					/* Don't release the regular Handle since
					 * it's owned by the associated IGLTexture.
					 */
					Handle = IntPtr.Zero;
				}
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

			public int InternalOffset
			{
				get;
				private set;
			}

			private MetalDevice device;
			private IntPtr mtlDevice = IntPtr.Zero;
			private int internalBufferSize = 0;
			private int prevDataLength = 0;
			private int prevInternalOffset;
			private BufferUsage usage;
			private bool boundThisFrame;

			public MetalBuffer(
				MetalDevice device,
				bool dynamic,
				BufferUsage usage,
				IntPtr bufferSize
			) {
				this.device = device;
				this.mtlDevice = device.device;
				this.usage = usage;

				BufferSize = bufferSize;
				internalBufferSize = (int) bufferSize;

				CreateBackingBuffer(-1);
			}

			private void CreateBackingBuffer(int prevSize)
			{
				IntPtr oldBuffer = Handle;
				IntPtr oldContents = Contents;

				Handle = mtlNewBufferWithLength(
					mtlDevice,
					internalBufferSize,
					usage == BufferUsage.WriteOnly ?
						MTLResourceOptions.CPUCacheModeWriteCombined :
						MTLResourceOptions.CPUCacheModeDefaultCache
				);
				Contents = mtlGetBufferContentsPtr(Handle);

				// Copy over data from old buffer
				if (oldBuffer != IntPtr.Zero)
				{
					SDL.SDL_memcpy(
						Contents,
						oldContents,
						(IntPtr) prevSize
					);
					objc_release(oldBuffer);
				}
			}

			public void SetData(
				int offsetInBytes,
				IntPtr data,
				int dataLength,
				SetDataOptions options
			) {
				if (options == SetDataOptions.None && boundThisFrame)
				{
					device.Stall();
					boundThisFrame = true;
				}
				else if (options == SetDataOptions.Discard && boundThisFrame)
				{
					InternalOffset += (int) BufferSize;
					if (InternalOffset + dataLength > internalBufferSize)
					{
						// Expand!
						int prevSize = internalBufferSize;
						internalBufferSize *= 2;
						CreateBackingBuffer(prevSize);
					}
				}

				// Copy previous contents, if needed
				if (prevInternalOffset != InternalOffset && dataLength < (int) BufferSize)
				{
					SDL.SDL_memcpy(
						Contents + InternalOffset,
						Contents + prevInternalOffset,
						BufferSize
					);
				}

				// Copy the data into the buffer
				SDL.SDL_memcpy(
					Contents + InternalOffset + offsetInBytes,
					data,
					(IntPtr) dataLength
				);

				prevInternalOffset = InternalOffset;
				prevDataLength = (int) BufferSize;
			}

			public void SetData(
				int offsetInBytes,
				IntPtr data,
				int dataLength
			) {
				InternalOffset += prevDataLength;
				if (InternalOffset + dataLength > internalBufferSize)
				{
					// Expand!
					int prevSize = internalBufferSize;
					internalBufferSize = Math.Max(
						internalBufferSize * 2,
						internalBufferSize + dataLength
					);
					CreateBackingBuffer(prevSize);
				}

				// Copy the data into the buffer
				SDL.SDL_memcpy(
					Contents + InternalOffset,
					data + offsetInBytes,
					(IntPtr) dataLength
				);

				prevDataLength = dataLength;
			}

			public void Bound()
			{
				boundThisFrame = true;
			}

			public void Reset()
			{
				InternalOffset = 0;
				boundThisFrame = false;
				prevDataLength = 0;
			}

			public void Dispose()
			{
				objc_release(Handle);
				Handle = IntPtr.Zero;
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

			public void Dispose()
			{
				objc_release(Handle);
				Handle = IntPtr.Zero;
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
				// FIXME: Metal does not support multisample masks. Workarounds...?
			}
		}

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

		/* Used for resetting scissor rectangle */
		private int currentAttachmentWidth;
		private int currentAttachmentHeight;

		#endregion

		#region Sampler State Variables

		private MetalTexture[] Textures;
		private IntPtr[] Samplers;
		private bool[] textureNeedsUpdate;
		private bool[] samplerNeedsUpdate;

		#endregion

		#region Depth Stencil State Variables

		private DepthStencilState depthStencilState;

		private IntPtr defaultDepthStencilState;	// MTLDepthStencilState*
		private IntPtr ldDepthStencilState;		// MTLDepthStencilState*

		private MTLPixelFormat D16Format;
		private MTLPixelFormat D24Format;
		private MTLPixelFormat D24S8Format;

		#endregion

		#region Buffer Binding Cache Variables

		private List<MetalBuffer> Buffers = new List<MetalBuffer>();

		private MetalBuffer userVertexBuffer = null;
		private MetalBuffer userIndexBuffer = null;
		private int userVertexStride = 0;

		// Some vertex declarations may have overlapping attributes :/
		private bool[,] attrUse = new bool[(int) MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TOTAL, 16];

		#endregion

		#region Render Target Cache Variables

		private readonly IntPtr[] currentAttachments;
		private readonly MTLPixelFormat[] currentColorFormats;
		private readonly IntPtr[] currentMSAttachments;
		private readonly CubeMapFace[] currentAttachmentSlices;
		private IntPtr currentDepthStencilBuffer;
		private DepthFormat currentDepthFormat;
		private int currentSampleCount;

		#endregion

		#region Clear Cache Variables

		private Vector4 clearColor = new Vector4(0, 0, 0, 0);
		private float clearDepth = 1.0f;
		private int clearStencil = 0;

		private bool shouldClearColor = false;
		private bool shouldClearDepth = false;
		private bool shouldClearStencil = false;

		#endregion

		#region Private Metal State Variables

		private IntPtr view;				// SDL_MetalView*
		private IntPtr layer;				// CAMetalLayer*
		private IntPtr device;				// MTLDevice*
		private IntPtr queue;				// MTLCommandQueue*
		private IntPtr commandBuffer;			// MTLCommandBuffer*
		private IntPtr renderCommandEncoder;		// MTLRenderCommandEncoder*
		private IntPtr currentVertexDescriptor;		// MTLVertexDescriptor*
		private IntPtr currentVisibilityBuffer;		// MTLBuffer*

		private bool needNewRenderPass;

		#endregion

		#region Operating System Variables

		private bool isMac;

		#endregion

		#region Frame Tracking Variables

		/* FIXME:
		 * In theory, double- or even triple-buffering could
		 * significantly help performance by reducing CPU idle
		 * time. The trade-off is that buffer synchronization
		 * becomes much more complicated and error-prone.
		 *
		 * I've attempted a few implementations of multi-
		 * buffering, but they all had serious issues and
		 * typically performed worse than single buffering.
		 *
		 * I'm leaving these variables here in case any brave
		 * souls want to attempt a multi-buffer implementation.
		 * This could be a huge win for performance, but it'll
		 * take someone smarter than me to figure this out. ;)
		 *
		 * -caleb
		 */
		private const int MAX_FRAMES_IN_FLIGHT = 1;
		private Queue<IntPtr> committedCommandBuffers = new Queue<IntPtr>();

		private bool frameInProgress = false;

		#endregion

		#region Objective-C Memory Management Variables

		private IntPtr pool; // NSAutoreleasePool*

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
		private IntPtr fauxBackbufferDrawBuffer;
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
			get
			{
				return true;
			}
		}

		public bool SupportsNoOverwrite
		{
			get
			{
				return true;
			}
		}

		public int MaxTextureSlots
		{
			get
			{
				return 16;
			}
		}

		public int MaxMultiSampleCount
		{
			get;
			private set;
		}

		private bool supportsOcclusionQueries;

		#endregion

		#region Private State Object Caches

		private Dictionary<ulong, IntPtr> VertexDescriptorCache =
			new Dictionary<ulong, IntPtr>();

		private Dictionary<PipelineHash, IntPtr> PipelineStateCache =
			new Dictionary<PipelineHash, IntPtr>();

		private Dictionary<StateHash, IntPtr> DepthStencilStateCache =
			new Dictionary<StateHash, IntPtr>();

		private Dictionary<StateHash, IntPtr> SamplerStateCache =
			new Dictionary<StateHash, IntPtr>();

		private List<MetalTexture> transientTextures =
			new List<MetalTexture>();

		#endregion

		#region Private Render Pipeline State Variables

		private BlendState blendState;
		private IntPtr ldPipelineState = IntPtr.Zero;

		#endregion

		#region Private Buffer Binding Cache

		private const int MAX_BOUND_VERTEX_BUFFERS = 16;

		private IntPtr ldVertUniformBuffer = IntPtr.Zero;
		private IntPtr ldFragUniformBuffer = IntPtr.Zero;
		private int ldVertUniformOffset = 0;
		private int ldFragUniformOffset = 0;

		private IntPtr[] ldVertexBuffers;
		private int[] ldVertexBufferOffsets;

		#endregion

		#region Private MojoShader Interop

		private IntPtr currentEffect = IntPtr.Zero;
		private IntPtr currentTechnique = IntPtr.Zero;
		private uint currentPass = 0;

		private IntPtr prevEffect = IntPtr.Zero;

		private MojoShader.MOJOSHADER_mtlShaderState shaderState = new MojoShader.MOJOSHADER_mtlShaderState();
		private MojoShader.MOJOSHADER_mtlShaderState prevShaderState;

		#endregion

		#region Public Constructor

		public MetalDevice(PresentationParameters presentationParameters)
		{
			device = MTLCreateSystemDefaultDevice();
			queue = mtlNewCommandQueue(device);

			// Create the Metal view
			view = SDL.SDL_Metal_CreateView(
				presentationParameters.DeviceWindowHandle
			);

			// Get the layer from the view
			layer = mtlGetLayer(view);

			// Set up the CAMetalLayer
			mtlSetLayerDevice(layer, device);
			mtlSetLayerFramebufferOnly(layer, true);
			mtlSetLayerMagnificationFilter(layer, UTF8ToNSString("nearest"));

			// Log GLDevice info
			FNALoggerEXT.LogInfo("IGLDevice: MetalDevice");
			FNALoggerEXT.LogInfo("Device Name: " + mtlGetDeviceName(device));
			FNALoggerEXT.LogInfo("MojoShader Profile: metal");

			// Some users might want pixely upscaling...
			backbufferScaleMode = Environment.GetEnvironmentVariable(
				"FNA_GRAPHICS_BACKBUFFER_SCALE_NEAREST"
			) == "1" ? MTLSamplerMinMagFilter.Nearest : MTLSamplerMinMagFilter.Linear;

			// Set device properties
			isMac = SDL.SDL_GetPlatform().Equals("Mac OS X");
			SupportsS3tc = SupportsDxt1 = isMac;
			MaxMultiSampleCount = mtlSupportsSampleCount(device, 8) ? 8 : 4;
			supportsOcclusionQueries = isMac || HasModernAppleGPU();

			// Determine supported depth formats
			D16Format = MTLPixelFormat.Depth32Float;
			D24Format = MTLPixelFormat.Depth32Float;
			D24S8Format = MTLPixelFormat.Depth32Float_Stencil8;

			if (isMac)
			{
				bool supportsD24S8 = mtlSupportsDepth24Stencil8(device);
				if (supportsD24S8)
				{
					D24S8Format = MTLPixelFormat.Depth24Unorm_Stencil8;

					// Gross, but at least it's a unorm format! -caleb
					D24Format = MTLPixelFormat.Depth24Unorm_Stencil8;
					D16Format = MTLPixelFormat.Depth24Unorm_Stencil8;
				}

				// Depth16Unorm requires macOS 10.12+
				if (OperatingSystemAtLeast(10, 12, 0))
				{
					D16Format = MTLPixelFormat.Depth16Unorm;
				}
			}
			else
			{
				// Depth16Unorm requires iOS 13+
				if (OperatingSystemAtLeast(13, 0, 0))
				{
					D16Format = MTLPixelFormat.Depth16Unorm;
				}
			}

			// Add fallbacks for missing texture formats on macOS
			if (isMac)
			{
				XNAToMTL.TextureFormat[(int) SurfaceFormat.Bgr565]
					= MTLPixelFormat.BGRA8Unorm;
				XNAToMTL.TextureFormat[(int) SurfaceFormat.Bgra5551]
					= MTLPixelFormat.BGRA8Unorm;
				XNAToMTL.TextureFormat[(int) SurfaceFormat.Bgra4444]
					= MTLPixelFormat.BGRA8Unorm;
			}

			// Initialize texture and sampler collections
			Textures = new MetalTexture[MaxTextureSlots];
			Samplers = new IntPtr[MaxTextureSlots];
			for (int i = 0; i < MaxTextureSlots; i += 1)
			{
				Textures[i] = MetalTexture.NullTexture;
				Samplers[i] = IntPtr.Zero;
			}
			textureNeedsUpdate = new bool[MaxTextureSlots];
			samplerNeedsUpdate = new bool[MaxTextureSlots];

			// Initialize attachment arrays
			int numAttachments = GraphicsDevice.MAX_RENDERTARGET_BINDINGS;
			currentAttachments = new IntPtr[numAttachments];
			currentColorFormats = new MTLPixelFormat[numAttachments];
			currentMSAttachments = new IntPtr[numAttachments];
			currentAttachmentSlices = new CubeMapFace[numAttachments];

			// Initialize vertex buffer cache
			ldVertexBuffers = new IntPtr[MAX_BOUND_VERTEX_BUFFERS];
			ldVertexBufferOffsets = new int[MAX_BOUND_VERTEX_BUFFERS];

			// Create a default depth stencil state
			IntPtr defDS = mtlNewDepthStencilDescriptor();
			defaultDepthStencilState = mtlNewDepthStencilStateWithDescriptor(device, defDS);
			objc_release(defDS);

			// Create and setup the faux-backbuffer
			InitializeFauxBackbuffer(presentationParameters);
		}

		#endregion

		#region Dispose Method

		public void Dispose()
		{
			// Stop rendering
			EndPass();

			// Release vertex descriptors
			foreach (IntPtr vdesc in VertexDescriptorCache.Values)
			{
				objc_release(vdesc);
			}
			VertexDescriptorCache.Clear();
			VertexDescriptorCache = null;

			// Release depth stencil states
			foreach (IntPtr ds in DepthStencilStateCache.Values)
			{
				objc_release(ds);
			}
			DepthStencilStateCache.Clear();
			DepthStencilStateCache = null;

			// Release pipeline states
			foreach (IntPtr pso in PipelineStateCache.Values)
			{
				objc_release(pso);
			}
			PipelineStateCache.Clear();
			PipelineStateCache = null;

			// Release sampler states
			foreach (IntPtr ss in SamplerStateCache.Values)
			{
				objc_release(ss);
			}
			SamplerStateCache.Clear();
			SamplerStateCache = null;

			// Release transient textures
			foreach (MetalTexture tex in transientTextures)
			{
				objc_release(tex.Handle);
			}
			transientTextures.Clear();
			transientTextures = null;

			// Dispose the backbuffer
			(Backbuffer as MetalBackbuffer).Dispose();

			// Destroy the view
			SDL.SDL_Metal_DestroyView(view);
		}

		#endregion

		#region GetDrawableSize Methods

		public static void GetDrawableSize(
			IntPtr layer,
			out int w,
			out int h
		) {
			CGSize size = mtlGetDrawableSize(layer);
			w = (int) size.width;
			h = (int) size.height;
		}

		public static void GetDrawableSizeFromView(
			IntPtr view,
			out int w,
			out int h
		) {
			GetDrawableSize(mtlGetLayer(view), out w, out h);
		}

		#endregion

		#region Window Backbuffer Reset Method

		public void ResetBackbuffer(PresentationParameters presentationParameters)
		{
			Backbuffer.ResetFramebuffer(presentationParameters);
		}

		#endregion

		#region BeginFrame Method

		public void BeginFrame()
		{
			if (frameInProgress) return;

			// Wait for command buffers to complete...
			while (committedCommandBuffers.Count >= MAX_FRAMES_IN_FLIGHT)
			{
				IntPtr cmdbuf = committedCommandBuffers.Dequeue();
				mtlCommandBufferWaitUntilCompleted(cmdbuf);
				objc_release(cmdbuf);
			}

			// The cycle begins anew!
			frameInProgress = true;
			pool = objc_autoreleasePoolPush();
			commandBuffer = mtlMakeCommandBuffer(queue);
		}

		#endregion

		#region Window SwapBuffers Method

		public void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			/* Just in case Present() is called
			 * before any rendering happens...
			 */
			BeginFrame();

			// Bind the backbuffer and finalize rendering
			SetRenderTargets(null, null, DepthFormat.None);
			EndPass();

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
				GetDrawableSize(
					layer,
					out dstW,
					out dstH
				);
			}

			// Get the next drawable
			IntPtr drawable = mtlNextDrawable(layer);

			// "Blit" the backbuffer to the drawable
			BlitFramebuffer(
				currentAttachments[0],
				new Rectangle(srcX, srcY, srcW, srcH),
				mtlGetTextureFromDrawable(drawable),
				new Rectangle(dstX, dstY, dstW, dstH)
			);

			// Commit the command buffer for presentation
			mtlPresentDrawable(commandBuffer, drawable);
			mtlCommitCommandBuffer(commandBuffer);

			// Enqueue the command buffer for tracking
			objc_retain(commandBuffer);
			committedCommandBuffers.Enqueue(commandBuffer);
			commandBuffer = IntPtr.Zero;

			// Release allocations from the past frame
			objc_autoreleasePoolPop(pool);

			// Reset buffers
			for (int i = 0; i < Buffers.Count; i += 1)
			{
				Buffers[i].Reset();
			}
			MojoShader.MOJOSHADER_mtlEndFrame();

			// We're done here.
			frameInProgress = false;
		}

		private void BlitFramebuffer(
			IntPtr srcTex,
			Rectangle srcRect,
			IntPtr dstTex,
			Rectangle dstRect
		) {
			if (	srcRect.Width == 0 ||
				srcRect.Height == 0 ||
				dstRect.Width == 0 ||
				dstRect.Height == 0	)
			{
				// Enjoy that bright red window!
				return;
			}

			// Update cached vertex buffer if needed
			if (fauxBackbufferDestBounds != dstRect || fauxBackbufferSizeChanged)
			{
				fauxBackbufferDestBounds = dstRect;
				fauxBackbufferSizeChanged = false;

				// Scale the coordinates to (-1, 1)
				int dw, dh;
				GetDrawableSize(layer, out dw, out dh);
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
				GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
				SDL.SDL_memcpy(
					mtlGetBufferContentsPtr(fauxBackbufferDrawBuffer),
					handle.AddrOfPinnedObject(),
					(IntPtr) (16 * sizeof(float))
				);
				handle.Free();
			}

			// Render the source texture to the destination texture
			IntPtr backbufferRenderPass = mtlMakeRenderPassDescriptor();
			mtlSetAttachmentTexture(
				mtlGetColorAttachment(backbufferRenderPass, 0),
				dstTex
			);
			IntPtr rce = mtlMakeRenderCommandEncoder(
				commandBuffer,
				backbufferRenderPass
			);
			mtlSetRenderPipelineState(rce, fauxBackbufferRenderPipeline);
			mtlSetVertexBuffer(rce, fauxBackbufferDrawBuffer, 0, 0);
			mtlSetFragmentTexture(rce, srcTex, 0);
			mtlSetFragmentSamplerState(rce, fauxBackbufferSamplerState, 0);
			mtlDrawIndexedPrimitives(
				rce,
				MTLPrimitiveType.Triangle,
				6,
				MTLIndexType.UInt16,
				fauxBackbufferDrawBuffer,
				16 * sizeof(float),
				1
			);
			mtlEndEncoding(rce);
		}

		#endregion

		#region Render Command Encoder Methods

		private void EndPass()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlEndEncoding(renderCommandEncoder);
				renderCommandEncoder = IntPtr.Zero;
			}
		}

		private void UpdateRenderPass()
		{
			if (!needNewRenderPass) return;

			/* Normally the frame begins in BeginDraw(),
			 * but some games perform drawing outside
			 * of the Draw method (e.g. initializing
			 * render targets in LoadContent). This call
			 * ensures that we catch any unexpected draws.
			 * -caleb
			 */
			BeginFrame();

			// Wrap up rendering with the old encoder
			EndPass();

			// Generate the descriptor
			IntPtr passDesc = mtlMakeRenderPassDescriptor();

			// Bind color attachments
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (currentAttachments[i] == IntPtr.Zero)
				{
					continue;
				}

				IntPtr colorAttachment = mtlGetColorAttachment(passDesc, i);
				mtlSetAttachmentTexture(
					colorAttachment,
					currentAttachments[i]
				);
				mtlSetAttachmentSlice(
					colorAttachment,
					(int) currentAttachmentSlices[i]
				);

				// Multisample?
				if (currentSampleCount > 0)
				{
					mtlSetAttachmentTexture(
						colorAttachment,
						currentMSAttachments[i]
					);
					mtlSetAttachmentSlice(
						colorAttachment,
						0
					);
					mtlSetAttachmentResolveTexture(
						colorAttachment,
						currentAttachments[i]
					);
					mtlSetAttachmentStoreAction(
						colorAttachment,
						MTLStoreAction.MultisampleResolve
					);
					mtlSetAttachmentResolveSlice(
						colorAttachment,
						(int) currentAttachmentSlices[i]
					);
				}

				// Clear color
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
				}
				else
				{
					mtlSetAttachmentLoadAction(
						colorAttachment,
						MTLLoadAction.Load
					);
				}
			}

			// Bind depth attachment
			if (currentDepthFormat != DepthFormat.None)
			{
				IntPtr depthAttachment = mtlGetDepthAttachment(passDesc);
				mtlSetAttachmentTexture(
					depthAttachment,
					currentDepthStencilBuffer
				);
				mtlSetAttachmentStoreAction(
					depthAttachment,
					MTLStoreAction.Store
				);

				// Clear?
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
				}
				else
				{
					mtlSetAttachmentLoadAction(
						depthAttachment,
						MTLLoadAction.Load
					);
				}
			}

			// Bind stencil buffer
			if (currentDepthFormat == DepthFormat.Depth24Stencil8)
			{
				IntPtr stencilAttachment = mtlGetStencilAttachment(passDesc);
				mtlSetAttachmentTexture(
					stencilAttachment,
					currentDepthStencilBuffer
				);
				mtlSetAttachmentStoreAction(
					stencilAttachment,
					MTLStoreAction.Store
				);

				// Clear?
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
			currentAttachmentWidth = (int) mtlGetTextureWidth(
				currentAttachments[0]
			);
			currentAttachmentHeight = (int) mtlGetTextureHeight(
				currentAttachments[0]
			);

			// Attach the visibility buffer, if needed
			if (currentVisibilityBuffer != IntPtr.Zero)
			{
				mtlSetVisibilityResultBuffer(
					passDesc,
					currentVisibilityBuffer
				);
			}

			// Make a new encoder
			renderCommandEncoder = mtlMakeRenderCommandEncoder(
				commandBuffer,
				passDesc
			);

			// Reset the flags
			needNewRenderPass = false;
			shouldClearColor = false;
			shouldClearDepth = false;
			shouldClearStencil = false;

			// Apply the dynamic state
			SetEncoderViewport();
			SetEncoderScissorRect();
			SetEncoderBlendColor();
			SetEncoderStencilReferenceValue();
			SetEncoderCullMode();
			SetEncoderFillMode();
			SetEncoderDepthBias();

			// Start visibility buffer counting
			if (currentVisibilityBuffer != IntPtr.Zero)
			{
				mtlSetVisibilityResultMode(
					renderCommandEncoder,
					MTLVisibilityResultMode.Counting,
					0
				);
			}

			// Reset the bindings
			for (int i = 0; i < MaxTextureSlots; i += 1)
			{
				if (Textures[i] != MetalTexture.NullTexture)
				{
					textureNeedsUpdate[i] = true;
				}
				if (Samplers[i] != IntPtr.Zero)
				{
					samplerNeedsUpdate[i] = true;
				}
			}
			ldDepthStencilState = IntPtr.Zero;
			ldFragUniformBuffer = IntPtr.Zero;
			ldFragUniformOffset = 0;
			ldVertUniformBuffer = IntPtr.Zero;
			ldVertUniformOffset = 0;
			ldPipelineState = IntPtr.Zero;
			for (int i = 0; i < MAX_BOUND_VERTEX_BUFFERS; i += 1)
			{
				ldVertexBuffers[i] = IntPtr.Zero;
				ldVertexBufferOffsets[i] = 0;
			}
		}

		private void SetEncoderStencilReferenceValue()
		{
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
			{
				mtlSetStencilReferenceValue(
					renderCommandEncoder,
					(uint) stencilRef
				);
			}
		}

		private void SetEncoderBlendColor()
		{
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
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
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
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
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
			{
				mtlSetCullMode(
					renderCommandEncoder,
					XNAToMTL.CullingEnabled[(int) cullFrontFace]
				);
			}
		}

		private void SetEncoderFillMode()
		{
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
			{
				mtlSetTriangleFillMode(
					renderCommandEncoder,
					XNAToMTL.FillMode[(int) fillMode]
				);
			}
		}

		private void SetEncoderDepthBias()
		{
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
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
			if (renderCommandEncoder != IntPtr.Zero && !needNewRenderPass)
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
						scissorRectangle.X,
						scissorRectangle.Y,
						scissorRectangle.Width,
						scissorRectangle.Height
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
			DeleteBuffer(buffer);
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
			DeleteBuffer(buffer);
		}

		#endregion

		#region Pipeline Stall Method

		private void Stall()
		{
			EndPass();
			mtlCommitCommandBuffer(commandBuffer);
			mtlCommandBufferWaitUntilCompleted(commandBuffer);

			commandBuffer = mtlMakeCommandBuffer(queue);
			needNewRenderPass = true;
			committedCommandBuffers.Clear();

			for (int i = 0; i < Buffers.Count; i += 1)
			{
				Buffers[i].Reset();
			}
		}

		#endregion

		#region String Marker Method

		public void SetStringMarker(string text)
		{
#if DEBUG
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlInsertDebugSignpost(renderCommandEncoder, text);
			}
#endif
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
			IGLBuffer indices,
			IndexElementSize indexElementSize
		) {
			MetalBuffer indexBuffer = indices as MetalBuffer;
			indexBuffer.Bound();
			int totalIndexOffset = (
				(startIndex * XNAToMTL.IndexSize[(int) indexElementSize]) +
				indexBuffer.InternalOffset
			);
			mtlDrawIndexedPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToMTL.IndexType[(int) indexElementSize],
				indexBuffer.Handle,
				totalIndexOffset,
				1
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
			IGLBuffer indices,
			IndexElementSize indexElementSize
		) {
			MetalBuffer indexBuffer = indices as MetalBuffer;
			indexBuffer.Bound();
			int totalIndexOffset = (
				(startIndex * XNAToMTL.IndexSize[(int) indexElementSize]) +
				indexBuffer.InternalOffset
			);
			mtlDrawIndexedPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToMTL.IndexType[(int) indexElementSize],
				indexBuffer.Handle,
				totalIndexOffset,
				instanceCount
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
				vertexStart,
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount)
			);
		}

		private void BindUserVertexBuffer(
			IntPtr vertexData,
			int vertexCount,
			int vertexOffset
		) {
			// Update the buffer contents
			int len = vertexCount * userVertexStride;
			if (userVertexBuffer == null)
			{
				userVertexBuffer = new MetalBuffer(
					this,
					true,
					BufferUsage.WriteOnly,
					(IntPtr) len
				);
				Buffers.Add(userVertexBuffer);
			}
			userVertexBuffer.SetData(
				vertexOffset * userVertexStride,
				vertexData,
				len
			);

			// Bind the buffer
			int offset = userVertexBuffer.InternalOffset;
			IntPtr handle = userVertexBuffer.Handle;
			if (ldVertexBuffers[0] != handle)
			{
				mtlSetVertexBuffer(
					renderCommandEncoder,
					handle,
					offset,
					0
				);
				ldVertexBuffers[0] = handle;
				ldVertexBufferOffsets[0] = offset;
			}
			else if (ldVertexBufferOffsets[0] != offset)
			{
				mtlSetVertexBufferOffset(
					renderCommandEncoder,
					offset,
					0
				);
				ldVertexBufferOffsets[0] = offset;
			}
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
			// Bind the vertex buffer
			BindUserVertexBuffer(
				vertexData,
				numVertices,
				vertexOffset
			);

			// Prepare the index buffer
			int numIndices = XNAToMTL.PrimitiveVerts(
				primitiveType,
				primitiveCount
			);
			int indexSize = XNAToMTL.IndexSize[(int) indexElementSize];
			int len = (int) numIndices * indexSize;
			if (userIndexBuffer == null)
			{
				userIndexBuffer = new MetalBuffer(
					this,
					true,
					BufferUsage.WriteOnly,
					(IntPtr) len
				);
				Buffers.Add(userIndexBuffer);
			}
			userIndexBuffer.SetData(
				indexOffset * indexSize,
				indexData,
				len
			);

			// Draw!
			mtlDrawIndexedPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				numIndices,
				XNAToMTL.IndexType[(int) indexElementSize],
				userIndexBuffer.Handle,
				userIndexBuffer.InternalOffset,
				1
			);
		}

		public void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		) {
			// Bind the vertex buffer
			int numVerts = XNAToMTL.PrimitiveVerts(
				primitiveType,
				primitiveCount
			);
			BindUserVertexBuffer(
				vertexData,
				numVerts,
				vertexOffset
			);

			// Draw!
			mtlDrawPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				0,
				numVerts
			);
		}

		#endregion

		#region State Management Methods

		public void SetPresentationInterval(PresentInterval interval)
		{
			// Toggling vsync is only supported on macOS 10.13+
			if (!RespondsToSelector(layer, selDisplaySyncEnabled))
			{
				FNALoggerEXT.LogWarn(
					"Cannot set presentation interval! " +
					"Only vsync is supported."
				);
				return;
			}

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
				/* FIXME:
				 * There is no built-in support for
				 * present-every-other-frame in Metal.
				 * We could work around this, but do
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

			float realDepthBias = rasterizerState.DepthBias;
			realDepthBias *= XNAToMTL.DepthBiasScale(
				GetDepthFormat(currentDepthFormat)
			);
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
				// FIXME: Metal does not support toggling MSAA. Workarounds...?
			}
		}

		public void VerifySampler(int index, Texture texture, SamplerState sampler)
		{
			if (texture == null)
			{
				if (Textures[index] != MetalTexture.NullTexture)
				{
					Textures[index] = MetalTexture.NullTexture;
					textureNeedsUpdate[index] = true;
				}
				if (Samplers[index] == IntPtr.Zero)
				{
					/* Some shaders require non-null samplers
					 * even if they aren't actually used.
					 * -caleb
					 */
					Samplers[index] = FetchSamplerState(sampler, false);
					samplerNeedsUpdate[index] = true;
				}
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
				textureNeedsUpdate[index] = true;
			}

			// Update the texture sampler info
			tex.WrapS = sampler.AddressU;
			tex.WrapT = sampler.AddressV;
			tex.WrapR = sampler.AddressW;
			tex.Filter = sampler.Filter;
			tex.Anisotropy = sampler.MaxAnisotropy;
			tex.MaxMipmapLevel = sampler.MaxMipLevel;
			tex.LODBias = sampler.MipMapLevelOfDetailBias;

			// Update the sampler state, if needed
			IntPtr ss = FetchSamplerState(sampler, tex.HasMipmaps);
			if (ss != Samplers[index])
			{
				Samplers[index] = ss;
				samplerNeedsUpdate[index] = true;
			}
		}

		public void SetBlendState(BlendState blendState)
		{
			this.blendState = blendState;
			BlendFactor = blendState.BlendFactor; // Dynamic state!
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			this.depthStencilState = depthStencilState;
			ReferenceStencil = depthStencilState.ReferenceStencil; // Dynamic state!
		}

		#endregion

		#region State Creation/Retrieval Methods

		private struct PipelineHash : IEquatable<PipelineHash>
		{
			readonly ulong a;
			readonly ulong b;
			readonly ulong c;
			readonly ulong d;

			public PipelineHash(
				ulong vertexShader,
				ulong fragmentShader,
				ulong vertexDescriptor,
				MTLPixelFormat[] formats,
				DepthFormat depthFormat,
				int sampleCount,
				BlendState blendState
			) {
				this.a = vertexShader;
				this.b = fragmentShader;
				this.c = vertexDescriptor;

				unchecked
				{
					this.d = (
						((ulong) blendState.GetHashCode() << 32) |
						((ulong) sampleCount << 22) |
						((ulong) depthFormat << 20) |
						((ulong) HashFormat(formats[3]) << 15) |
						((ulong) HashFormat(formats[2]) << 10) |
						((ulong) HashFormat(formats[1]) << 5) |
						((ulong) HashFormat(formats[0]))
					);
				}
			}

			private static uint HashFormat(MTLPixelFormat format)
			{
				switch (format)
				{
					case MTLPixelFormat.Invalid:
						return 0;
					case MTLPixelFormat.R16Float:
						return 1;
					case MTLPixelFormat.R32Float:
						return 2;
					case MTLPixelFormat.RG16Float:
						return 3;
					case MTLPixelFormat.RG16Snorm:
						return 4;
					case MTLPixelFormat.RG16Unorm:
						return 5;
					case MTLPixelFormat.RG32Float:
						return 6;
					case MTLPixelFormat.RG8Snorm:
						return 7;
					case MTLPixelFormat.RGB10A2Unorm:
						return 8;
					case MTLPixelFormat.RGBA16Float:
						return 9;
					case MTLPixelFormat.RGBA16Unorm:
						return 10;
					case MTLPixelFormat.RGBA32Float:
						return 11;
					case MTLPixelFormat.RGBA8Unorm:
						return 12;
					case MTLPixelFormat.A8Unorm:
						return 13;
					case MTLPixelFormat.ABGR4Unorm:
						return 14;
					case MTLPixelFormat.B5G6R5Unorm:
						return 15;
					case MTLPixelFormat.BC1_RGBA:
						return 16;
					case MTLPixelFormat.BC2_RGBA:
						return 17;
					case MTLPixelFormat.BC3_RGBA:
						return 18;
					case MTLPixelFormat.BGR5A1Unorm:
						return 19;
					case MTLPixelFormat.BGRA8Unorm:
						return 20;
				}

				throw new NotSupportedException();
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int i1 = (int) (a ^ (a >> 32));
					int i2 = (int) (b ^ (b >> 32));
					int i3 = (int) (c ^ (c >> 32));
					int i4 = (int) (d ^ (d >> 32));
					return i1 + i2 + i3 + i4;
				}
			}

			public bool Equals(PipelineHash other)
			{
				return (
					a == other.a &&
					b == other.b &&
					c == other.c &&
					d == other.d
				);
			}

			public override bool Equals(object obj)
			{
				if (obj == null || obj.GetType() != GetType())
				{
					return false;
				}

				PipelineHash hash = (PipelineHash) obj;
				return (
					a == hash.a &&
					b == hash.b &&
					c == hash.c &&
					d == hash.d
				);
			}
		}

		private IntPtr FetchRenderPipeline()
		{
			// Can we just reuse an existing pipeline?
			PipelineHash hash = new PipelineHash(
				(ulong) shaderState.vertexShader,
				(ulong) shaderState.fragmentShader,
				(ulong) currentVertexDescriptor,
				currentColorFormats,
				currentDepthFormat,
				currentSampleCount,
				blendState
			);
			IntPtr pipeline = IntPtr.Zero;
			if (PipelineStateCache.TryGetValue(hash, out pipeline))
			{
				// We have this state already cached!
				return pipeline;
			}

			// We have to make a new pipeline...
			IntPtr pipelineDesc = mtlNewRenderPipelineDescriptor();
			IntPtr vertHandle = MojoShader.MOJOSHADER_mtlGetFunctionHandle(
				shaderState.vertexShader
			);
			IntPtr fragHandle = MojoShader.MOJOSHADER_mtlGetFunctionHandle(
				shaderState.fragmentShader
			);
			mtlSetPipelineVertexFunction(
				pipelineDesc,
				vertHandle
			);
			mtlSetPipelineFragmentFunction(
				pipelineDesc,
				fragHandle
			);
			mtlSetPipelineVertexDescriptor(
				pipelineDesc,
				currentVertexDescriptor
			);
			mtlSetDepthAttachmentPixelFormat(
				pipelineDesc,
				GetDepthFormat(currentDepthFormat)
			);
			if (currentDepthFormat == DepthFormat.Depth24Stencil8)
			{
				mtlSetStencilAttachmentPixelFormat(
					pipelineDesc,
					GetDepthFormat(currentDepthFormat)
				);
			}
			mtlSetPipelineSampleCount(
				pipelineDesc,
				Math.Max(1, currentSampleCount)
			);

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
					i
				);
				mtlSetAttachmentPixelFormat(
					colorAttachment,
					currentColorFormats[i]
				);
				mtlSetAttachmentBlendingEnabled(
					colorAttachment,
					alphaBlendEnable
				);
				if (alphaBlendEnable)
				{
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
				}

				/* FIXME: So how exactly do we factor in
				 * COLORWRITEENABLE for buffer 0? Do we just assume that
				 * the default is just buffer 0, and all other calls
				 * update the other write masks?
				 */
				if (i == 0)
				{
					mtlSetAttachmentWriteMask(
						colorAttachment,
						XNAToMTL.ColorWriteMask(blendState.ColorWriteChannels)
					);
				}
				else if (i == 1)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 1),
						XNAToMTL.ColorWriteMask(blendState.ColorWriteChannels1)
					);
				}
				else if (i == 2)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 2),
						XNAToMTL.ColorWriteMask(blendState.ColorWriteChannels2)
					);
				}
				else if (i == 3)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 3),
						XNAToMTL.ColorWriteMask(blendState.ColorWriteChannels3)
					);
				}
			}

			// Bake the render pipeline!
			IntPtr pipelineState = mtlNewRenderPipelineStateWithDescriptor(
				device,
				pipelineDesc
			);
			PipelineStateCache[hash] = pipelineState;

			// Clean up
			objc_release(pipelineDesc);
			objc_release(vertHandle);
			objc_release(fragHandle);

			// Return the pipeline!
			return pipelineState;
		}

		private IntPtr FetchDepthStencilState()
		{
			/* Just use the default depth-stencil state
			 * if depth and stencil testing are disabled,
			 * or if there is no bound depth attachment.
			 * This wards off Metal validation errors.
			 * -caleb
			 */
			bool zEnable = depthStencilState.DepthBufferEnable;
			bool sEnable = depthStencilState.StencilEnable;
			bool zFormat = (currentDepthFormat != DepthFormat.None);
			if ((!zEnable && !sEnable) || (!zFormat))
			{
				return defaultDepthStencilState;
			}

			// Can we just reuse an existing state?
			StateHash hash = PipelineCache.GetDepthStencilHash(depthStencilState);
			IntPtr state = IntPtr.Zero;
			if (DepthStencilStateCache.TryGetValue(hash, out state))
			{
				// This state has already been cached!
				return state;
			}

			// We have to make a new DepthStencilState...
			IntPtr dsDesc = mtlNewDepthStencilDescriptor();
			if (zEnable)
			{
				mtlSetDepthCompareFunction(
					dsDesc,
					XNAToMTL.CompareFunc[(int) depthStencilState.DepthBufferFunction]
				);
				mtlSetDepthWriteEnabled(
					dsDesc,
					depthStencilState.DepthBufferWriteEnable
				);
			}

			// Create stencil descriptors
			IntPtr front = IntPtr.Zero;
			IntPtr back = IntPtr.Zero;

			if (sEnable)
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
					(uint) depthStencilState.StencilMask
				);
				mtlSetStencilWriteMask(
					front,
					(uint) depthStencilState.StencilWriteMask
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
					(uint) depthStencilState.StencilMask
				);
				mtlSetStencilWriteMask(
					back,
					(uint) depthStencilState.StencilWriteMask
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
			DepthStencilStateCache[hash] = state;

			// Clean up
			objc_release(dsDesc);

			// Return the state!
			return state;
		}

		private IntPtr FetchSamplerState(SamplerState samplerState, bool hasMipmaps)
		{
			// Can we just reuse an existing state?
			StateHash hash = PipelineCache.GetSamplerHash(samplerState);
			IntPtr state = IntPtr.Zero;
			if (SamplerStateCache.TryGetValue(hash, out state))
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
			if (hasMipmaps)
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
			mtlSetSamplerMaxAnisotropy(
				samplerDesc,
				(samplerState.Filter == TextureFilter.Anisotropic) ?
					Math.Max(1, samplerState.MaxAnisotropy) :
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
			 *
			 * -caleb
			 */

			// Bake the sampler state!
			state = mtlNewSamplerStateWithDescriptor(
				device,
				samplerDesc
			);
			SamplerStateCache[hash] = state;

			// Clean up
			objc_release(samplerDesc);

			// Return the sampler state!
			return state;
		}

		private IntPtr FetchVertexDescriptor(
			VertexBufferBinding[] bindings,
			int numBindings
		) {
			// Can we just reuse an existing descriptor?
			ulong hash = PipelineCache.GetVertexBindingHash(
				bindings,
				numBindings,
				(ulong) shaderState.vertexShader
			);
			IntPtr descriptor;
			if (VertexDescriptorCache.TryGetValue(hash, out descriptor))
			{
				// The value is already cached!
				return descriptor;
			}

			// We have to make a new vertex descriptor...
			descriptor = mtlMakeVertexDescriptor();
			objc_retain(descriptor);

			/* There's this weird case where you can have overlapping
			 * vertex usage/index combinations. It seems like the first
			 * attrib gets priority, so whenever a duplicate attribute
			 * exists, give it the next available index. If that fails, we
			 * have to crash :/
			 * -flibit
			 */
			Array.Clear(attrUse, 0, attrUse.Length);
			for (int i = 0; i < numBindings; i += 1)
			{
				// Describe vertex attributes
				VertexDeclaration vertexDeclaration = bindings[i].VertexBuffer.VertexDeclaration;
				foreach (VertexElement element in vertexDeclaration.elements)
				{
					int usage = (int) element.VertexElementUsage;
					int index = element.UsageIndex;
					if (attrUse[usage, index])
					{
						index = -1;
						for (int j = 0; j < 16; j += 1)
						{
							if (!attrUse[usage, j])
							{
								index = j;
								break;
							}
						}
						if (index < 0)
						{
							throw new InvalidOperationException("Vertex usage collision!");
						}
					}
					attrUse[usage, index] = true;
					int attribLoc = MojoShader.MOJOSHADER_mtlGetVertexAttribLocation(
						shaderState.vertexShader,
						XNAToMTL.VertexAttribUsage[usage],
						index
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
				if (bindings[i].InstanceFrequency > 0)
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

		private IntPtr FetchVertexDescriptor(
			VertexDeclaration vertexDeclaration,
			int vertexOffset
		) {
			// Can we just reuse an existing descriptor?
			ulong hash = PipelineCache.GetVertexDeclarationHash(
				vertexDeclaration,
				(ulong) shaderState.vertexShader
			);
			IntPtr descriptor;
			if (VertexDescriptorCache.TryGetValue(hash, out descriptor))
			{
				// The value is already cached!
				return descriptor;
			}

			// We have to make a new vertex descriptor...
			descriptor = mtlMakeVertexDescriptor();
			objc_retain(descriptor);

			/* There's this weird case where you can have overlapping
			 * vertex usage/index combinations. It seems like the first
			 * attrib gets priority, so whenever a duplicate attribute
			 * exists, give it the next available index. If that fails, we
			 * have to crash :/
			 * -flibit
			 */
			Array.Clear(attrUse, 0, attrUse.Length);
			foreach (VertexElement element in vertexDeclaration.elements)
			{
				int usage = (int) element.VertexElementUsage;
				int index = element.UsageIndex;
				if (attrUse[usage, index])
				{
					index = -1;
					for (int j = 0; j < 16; j += 1)
					{
						if (!attrUse[usage, j])
						{
							index = j;
							break;
						}
					}
					if (index < 0)
					{
						throw new InvalidOperationException("Vertex usage collision!");
					}
				}
				attrUse[usage, index] = true;
				int attribLoc = MojoShader.MOJOSHADER_mtlGetVertexAttribLocation(
					shaderState.vertexShader,
					XNAToMTL.VertexAttribUsage[usage],
					index
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
					0
				);
			}

			// Describe vertex buffer layout
			IntPtr layout = mtlGetVertexBufferLayoutDescriptor(
				descriptor,
				0
			);
			mtlSetVertexBufferLayoutStride(
				layout,
				vertexDeclaration.VertexStride
			);

			VertexDescriptorCache[hash] = descriptor;
			return descriptor;
		}

		private IntPtr FetchTransientTexture(MetalTexture fromTexture)
		{
			// Can we just reuse an existing texture?
			for (int i = 0; i < transientTextures.Count; i += 1)
			{
				MetalTexture tex = transientTextures[i];
				if (	tex.Format == fromTexture.Format &&
					tex.Width == fromTexture.Width &&
					tex.Height == fromTexture.Height &&
					tex.HasMipmaps == fromTexture.HasMipmaps	)
				{
					mtlSetPurgeableState(
						tex.Handle,
						MTLPurgeableState.NonVolatile
					);
					return tex.Handle;
				}
			}

			// We have to make a new texture...
			IntPtr texDesc = mtlMakeTexture2DDescriptor(
				XNAToMTL.TextureFormat[(int) fromTexture.Format],
				fromTexture.Width,
				fromTexture.Height,
				fromTexture.HasMipmaps
			);
			MetalTexture ret = new MetalTexture(
				mtlNewTextureWithDescriptor(device, texDesc),
				fromTexture.Width,
				fromTexture.Height,
				fromTexture.Format,
				fromTexture.HasMipmaps ? 2 : 0,
				false
			);
			transientTextures.Add(ret);
			return ret.Handle;
		}

		#endregion

		#region DepthFormat Conversion Method

		private MTLPixelFormat GetDepthFormat(DepthFormat format)
		{
			switch (format)
			{
				case DepthFormat.Depth16:		return D16Format;
				case DepthFormat.Depth24:		return D24Format;
				case DepthFormat.Depth24Stencil8:	return D24S8Format;
				default:				return MTLPixelFormat.Invalid;
			}
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

			mtlEffect = MojoShader.MOJOSHADER_mtlCompileEffect(
				effect,
				device,
				MAX_FRAMES_IN_FLIGHT
			);
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
					ref shaderState
				);
				currentEffect = IntPtr.Zero;
				currentTechnique = IntPtr.Zero;
				currentPass = 0;
				shaderState = new MojoShader.MOJOSHADER_mtlShaderState();
			}
			MojoShader.MOJOSHADER_mtlDeleteEffect(mtlEffectData);
			MojoShader.MOJOSHADER_freeEffect(effect.EffectData);
		}

		public IGLEffect CloneEffect(IGLEffect cloneSource)
		{
			IntPtr effect = IntPtr.Zero;
			IntPtr mtlEffect = IntPtr.Zero;

			effect = MojoShader.MOJOSHADER_cloneEffect(cloneSource.EffectData);
			mtlEffect = MojoShader.MOJOSHADER_mtlCompileEffect(
				effect,
				device,
				1
			);
			if (mtlEffect == IntPtr.Zero)
			{
				throw new InvalidOperationException(
					MojoShader.MOJOSHADER_mtlGetError()
				);
			}

			return new MetalEffect(effect, mtlEffect);
		}

		public void ApplyEffect(
			IGLEffect effect,
			IntPtr technique,
			uint pass,
			IntPtr stateChanges
		) {
			/* If a frame isn't already in progress,
			 * wait until one begins to avoid overwriting
			 * the previous frame's uniform buffers.
			 */
			BeginFrame();

			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			if (mtlEffectData == currentEffect)
			{
				if (technique == currentTechnique && pass == currentPass)
				{
					MojoShader.MOJOSHADER_mtlEffectCommitChanges(
						currentEffect,
						ref shaderState
					);
					return;
				}
				MojoShader.MOJOSHADER_mtlEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_mtlEffectBeginPass(
					currentEffect,
					pass,
					ref shaderState
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
					ref shaderState
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
				ref shaderState
			);
			currentEffect = mtlEffectData;
			currentTechnique = technique;
			currentPass = pass;
		}

		public void BeginPassRestore(IGLEffect effect, IntPtr stateChanges)
		{
			/* If a frame isn't already in progress,
			 * wait until one begins to avoid overwriting
			 * the previous frame's uniform buffers.
			 */
			BeginFrame();

			// Store the current data
			prevEffect = currentEffect;
			prevShaderState = shaderState;

			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			uint whatever;
			MojoShader.MOJOSHADER_mtlEffectBegin(
				mtlEffectData,
				out whatever,
				1,
				stateChanges
			);
			MojoShader.MOJOSHADER_mtlEffectBeginPass(
				mtlEffectData,
				0,
				ref shaderState
			);
			currentEffect = mtlEffectData;
		}

		public void EndPassRestore(IGLEffect effect)
		{
			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			MojoShader.MOJOSHADER_mtlEffectEndPass(mtlEffectData);
			MojoShader.MOJOSHADER_mtlEffectEnd(
				mtlEffectData,
				ref shaderState
			);

			// Restore the old data
			shaderState = prevShaderState;
			currentEffect = prevEffect;
		}

		#endregion

		#region Resource Binding Method

		private void BindResources()
		{
			// Bind textures and their sampler states
			for (int i = 0; i < Textures.Length; i += 1)
			{
				if (textureNeedsUpdate[i])
				{
					mtlSetFragmentTexture(
						renderCommandEncoder,
						Textures[i].Handle,
						i
					);
					textureNeedsUpdate[i] = false;
				}
				if (samplerNeedsUpdate[i])
				{
					mtlSetFragmentSamplerState(
						renderCommandEncoder,
						Samplers[i],
						i
					);
					samplerNeedsUpdate[i] = false;
				}
			}

			// Bind the uniform buffers
			const int UNIFORM_REG = 16; // In MojoShader output it's always 16

			IntPtr vUniform = shaderState.vertexUniformBuffer;
			int vOff = shaderState.vertexUniformOffset;
			if (vUniform != ldVertUniformBuffer)
			{
				mtlSetVertexBuffer(
					renderCommandEncoder,
					vUniform,
					vOff,
					UNIFORM_REG
				);
				ldVertUniformBuffer = vUniform;
				ldVertUniformOffset = vOff;
			}
			else if (vOff != ldVertUniformOffset)
			{
				mtlSetVertexBufferOffset(
					renderCommandEncoder,
					vOff,
					UNIFORM_REG
				);
				ldVertUniformOffset = vOff;
			}

			IntPtr fUniform = shaderState.fragmentUniformBuffer;
			int fOff = shaderState.fragmentUniformOffset;
			if (fUniform != ldFragUniformBuffer)
			{
				mtlSetFragmentBuffer(
					renderCommandEncoder,
					fUniform,
					fOff,
					UNIFORM_REG
				);
				ldFragUniformBuffer = fUniform;
				ldFragUniformOffset = fOff;
			}
			else if (fOff != ldFragUniformOffset)
			{
				mtlSetFragmentBufferOffset(
					renderCommandEncoder,
					fOff,
					UNIFORM_REG
				);
				ldFragUniformOffset = fOff;
			}

			// Bind the depth-stencil state
			IntPtr depthStencilState = FetchDepthStencilState();
			if (depthStencilState != ldDepthStencilState)
			{
				mtlSetDepthStencilState(
					renderCommandEncoder,
					depthStencilState
				);
				ldDepthStencilState = depthStencilState;
			}

			// Finally, bind the pipeline state
			IntPtr pipelineState = FetchRenderPipeline();
			if (pipelineState != ldPipelineState)
			{
				mtlSetRenderPipelineState(
					renderCommandEncoder,
					pipelineState
				);
				ldPipelineState = pipelineState;
			}
		}

		#endregion

		#region ApplyVertexAttributes Methods

		public void ApplyVertexAttributes(
			VertexBufferBinding[] bindings,
			int numBindings,
			bool bindingsUpdated,
			int baseVertex
		) {
			// Translate the bindings array into a descriptor
			currentVertexDescriptor = FetchVertexDescriptor(
				bindings,
				numBindings
			);

			// Prepare for rendering
			UpdateRenderPass();
			BindResources();

			// Bind the vertex buffers
			for (int i = 0; i < bindings.Length; i += 1)
			{
				VertexBuffer vertexBuffer = bindings[i].VertexBuffer;
				if (vertexBuffer != null)
				{
					int stride = bindings[i].VertexBuffer.VertexDeclaration.VertexStride;
					int offset = (
						((bindings[i].VertexOffset + baseVertex) * stride) +
						(vertexBuffer.buffer as MetalBuffer).InternalOffset
					);

					IntPtr handle = (vertexBuffer.buffer as MetalBuffer).Handle;
					(vertexBuffer.buffer as MetalBuffer).Bound();
					if (ldVertexBuffers[i] != handle)
					{
						mtlSetVertexBuffer(
							renderCommandEncoder,
							handle,
							offset,
							i
						);
						ldVertexBuffers[i] = handle;
						ldVertexBufferOffsets[i] = offset;
					}
					else if (ldVertexBufferOffsets[i] != offset)
					{
						mtlSetVertexBufferOffset(
							renderCommandEncoder,
							offset,
							i
						);
						ldVertexBufferOffsets[i] = offset;
					}
				}
			}
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
			// Translate the declaration into a descriptor
			currentVertexDescriptor = FetchVertexDescriptor(
				vertexDeclaration,
				vertexOffset
			);
			userVertexStride = vertexDeclaration.VertexStride;

			// Prepare for rendering
			UpdateRenderPass();
			BindResources();

			// The rest happens in DrawUser[Indexed]Primitives.
		}

		#endregion

		#region GenBuffers Methods

		public IGLBuffer GenIndexBuffer(
			bool dynamic,
			BufferUsage usage,
			int indexCount,
			IndexElementSize indexElementSize
		) {
			int elementSize = XNAToMTL.IndexSize[(int) indexElementSize];
			MetalBuffer newbuf = new MetalBuffer(
				this,
				dynamic,
				usage,
				(IntPtr) (indexCount * elementSize)
			);
			Buffers.Add(newbuf);
			return newbuf;
		}

		public IGLBuffer GenVertexBuffer(
			bool dynamic,
			BufferUsage usage,
			int vertexCount,
			int vertexStride
		) {
			MetalBuffer newbuf = new MetalBuffer(
				this,
				dynamic,
				usage,
				(IntPtr) (vertexCount * vertexStride)
			);
			Buffers.Add(newbuf);
			return newbuf;
		}

		#endregion

		#region Renderbuffer Methods

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount,
			IGLTexture texture
		) {
			MTLPixelFormat pixelFormat = XNAToMTL.TextureFormat[(int) format];
			int sampleCount = GetCompatibleSampleCount(multiSampleCount);

			// Generate a multisample texture
			IntPtr desc = mtlMakeTexture2DDescriptor(
				pixelFormat,
				width,
				height,
				false
			);
			mtlSetStorageMode(
				desc,
				MTLStorageMode.Private
			);
			mtlSetTextureUsage(
				desc,
				MTLTextureUsage.RenderTarget
			);
			mtlSetTextureType(
				desc,
				MTLTextureType.Multisample2D
			);
			mtlSetTextureSampleCount(
				desc,
				sampleCount
			);
			IntPtr multisampleTexture = mtlNewTextureWithDescriptor(
				device,
				desc
			);

			// We're done!
			return new MetalRenderbuffer(
				(texture as MetalTexture).Handle,
				pixelFormat,
				sampleCount,
				multisampleTexture
			);
		}

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		) {
			MTLPixelFormat pixelFormat = GetDepthFormat(format);
			int sampleCount = GetCompatibleSampleCount(multiSampleCount);

			// Generate a depth texture
			IntPtr desc = mtlMakeTexture2DDescriptor(
				pixelFormat,
				width,
				height,
				false
			);
			mtlSetStorageMode(
				desc,
				MTLStorageMode.Private
			);
			mtlSetTextureUsage(
				desc,
				MTLTextureUsage.RenderTarget
			);
			if (multiSampleCount > 0)
			{
				mtlSetTextureType(
					desc,
					MTLTextureType.Multisample2D
				);
				mtlSetTextureSampleCount(
					desc,
					sampleCount
				);
			}
			IntPtr handle = mtlNewTextureWithDescriptor(
				device,
				desc
			);

			// We're done!
			return new MetalRenderbuffer(
				handle,
				pixelFormat,
				sampleCount,
				IntPtr.Zero
			);
		}

		private void DeleteRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			MetalRenderbuffer rb = renderbuffer as MetalRenderbuffer;
			bool isDepthStencil = rb.MultiSampleHandle == IntPtr.Zero;

			if (isDepthStencil)
			{
				if (rb.Handle == currentDepthStencilBuffer)
				{
					currentDepthStencilBuffer = IntPtr.Zero;
				}
			}
			else
			{
				for (int i = 0; i < currentAttachments.Length; i += 1)
				{
					if (rb.MultiSampleHandle == currentMSAttachments[i])
					{
						currentMSAttachments[i] = IntPtr.Zero;
					}
				}
			}

			rb.Dispose();
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
			(buffer as MetalBuffer).SetData(
				offsetInBytes,
				data,
				dataLength,
				options
			);
		}

		public void SetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			(buffer as MetalBuffer).SetData(
				offsetInBytes,
				data,
				dataLength,
				options
			);
		}

		#endregion

		#region GetBufferData Methods

		public void GetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
			SDL.SDL_memcpy(
				data + (startIndex * elementSizeInBytes),
				(buffer as MetalBuffer).Contents + offsetInBytes,
				(IntPtr) (elementCount * elementSizeInBytes)
			);
		}

		public void GetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride
		) {
			IntPtr cpy;
			bool useStagingBuffer = elementSizeInBytes < vertexStride;
			if (useStagingBuffer)
			{
				cpy = Marshal.AllocHGlobal(elementCount * vertexStride);
			}
			else
			{
				cpy = data + (startIndex * elementSizeInBytes);
			}

			SDL.SDL_memcpy(
				cpy,
				(buffer as MetalBuffer).Contents + offsetInBytes,
				(IntPtr) (elementCount * vertexStride)
			);

			if (useStagingBuffer)
			{
				IntPtr src = cpy;
				IntPtr dst = data + (startIndex * elementSizeInBytes);
				for (int i = 0; i < elementCount; i += 1)
				{
					SDL.SDL_memcpy(dst, src, (IntPtr) elementSizeInBytes);
					dst += elementSizeInBytes;
					src += vertexStride;
				}
				Marshal.FreeHGlobal(cpy);
			}
		}

		#endregion

		#region DeleteBuffer Methods

		private void DeleteBuffer(IGLBuffer buffer)
		{
			Buffers.Remove(buffer as MetalBuffer);
			(buffer as MetalBuffer).Dispose();
		}

		#endregion

		#region CreateTexture Methods

		public IGLTexture CreateTexture2D(
			SurfaceFormat format,
			int width,
			int height,
			int levelCount,
			bool isRenderTarget
		) {
			IntPtr texDesc = mtlMakeTexture2DDescriptor(
				XNAToMTL.TextureFormat[(int) format],
				width,
				height,
				levelCount > 1
			);

			if (isRenderTarget)
			{
				mtlSetStorageMode(
					texDesc,
					MTLStorageMode.Private
				);
				mtlSetTextureUsage(
					texDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);
			}

			return new MetalTexture(
				mtlNewTextureWithDescriptor(device, texDesc),
				width,
				height,
				format,
				levelCount,
				isRenderTarget
			);
		}

		public IGLTexture CreateTexture3D(
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount
		) {
			IntPtr texDesc = mtlMakeTexture2DDescriptor(
				XNAToMTL.TextureFormat[(int) format],
				width,
				height,
				levelCount > 1
			);

			// Make it 3D!
			mtlSetTextureDepth(texDesc, depth);
			mtlSetTextureType(texDesc, MTLTextureType.Texture3D);

			return new MetalTexture(
				mtlNewTextureWithDescriptor(device, texDesc),
				width,
				height,
				format,
				levelCount,
				false
			);
		}

		public IGLTexture CreateTextureCube(
			SurfaceFormat format,
			int size,
			int levelCount,
			bool isRenderTarget
		) {
			IntPtr texDesc = mtlMakeTextureCubeDescriptor(
				XNAToMTL.TextureFormat[(int) format],
				size,
				levelCount > 1
			);

			if (isRenderTarget)
			{
				mtlSetStorageMode(
					texDesc,
					MTLStorageMode.Private
				);
				mtlSetTextureUsage(
					texDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);
			}

			return new MetalTexture(
				mtlNewTextureWithDescriptor(device, texDesc),
				size,
				size,
				format,
				levelCount,
				isRenderTarget
			);
		}

		#endregion

		#region DeleteTexture Method

		private void DeleteTexture(IGLTexture texture)
		{
			MetalTexture tex = texture as MetalTexture;
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (tex.Handle == currentAttachments[i])
				{
					currentAttachments[i] = IntPtr.Zero;
				}
			}
			for (int i = 0; i < Textures.Length; i += 1)
			{
				if (tex.Handle == Textures[i].Handle)
				{
					Textures[i] = MetalTexture.NullTexture;
					textureNeedsUpdate[i] = true;
				}
			}
			tex.Dispose();
		}

		#endregion

		#region Texture Data Helper Methods

		private int BytesPerRow(int width, SurfaceFormat format)
		{
			int blocksPerRow = width;

			if (	format == SurfaceFormat.Dxt1 ||
				format == SurfaceFormat.Dxt3 ||
				format == SurfaceFormat.Dxt5	)
			{
				blocksPerRow = (width + 3) / 4;
			}

			return blocksPerRow * Texture.GetFormatSize(format);
		}

		private int BytesPerImage(int width, int height, SurfaceFormat format)
		{
			int blocksPerRow = width;
			int blocksPerColumn = height;
			int formatSize = Texture.GetFormatSize(format);

			if (	format == SurfaceFormat.Dxt1 ||
				format == SurfaceFormat.Dxt3 ||
				format == SurfaceFormat.Dxt5	)
			{
				blocksPerRow = (width + 3) / 4;
				blocksPerColumn = (height + 3) / 4;
			}

			return blocksPerRow * blocksPerColumn * formatSize;
		}

		private int GetCompatibleSampleCount(int sampleCount)
		{
			/* If the device does not support the requested
			 * multisample count, halve it until we find a
			 * value that is supported.
			 */
			while (sampleCount > 0 && !mtlSupportsSampleCount(device, sampleCount))
			{
				sampleCount = MathHelper.ClosestMSAAPower(
					sampleCount / 2
				);
			}
			return sampleCount;
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
			MetalTexture tex = texture as MetalTexture;
			IntPtr handle = tex.Handle;

			MTLOrigin origin = new MTLOrigin(x, y, 0);
			MTLSize size = new MTLSize(w, h, 1);

			if (tex.IsPrivate)
			{
				// We need an active command buffer
				BeginFrame();

				// Fetch a CPU-accessible texture
				handle = FetchTransientTexture(tex);
			}

			// Write the data
			mtlReplaceRegion(
				handle,
				new MTLRegion(origin, size),
				level,
				0,
				data,
				BytesPerRow(w, format),
				0
			);

			// Blit the temp texture to the actual texture
			if (tex.IsPrivate)
			{
				// End the render pass
				EndPass();

				// Blit!
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					blit,
					handle,
					0,
					level,
					origin,
					size,
					tex.Handle,
					0,
					level,
					origin
				);

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				Stall();

				// We're done with the temp texture
				mtlSetPurgeableState(
					handle,
					MTLPurgeableState.Empty
				);
			}
		}

		public void SetTextureDataYUV(Texture2D[] textures, IntPtr ptr)
		{
			for (int i = 0; i < 3; i += 1)
			{
				Texture2D tex = textures[i];
				MTLRegion region = new MTLRegion(
					MTLOrigin.Zero,
					new MTLSize(tex.Width, tex.Height, 1)
				);
				mtlReplaceRegion(
					(tex.texture as MetalTexture).Handle,
					region,
					0,
					0,
					ptr,
					tex.Width,
					0
				);
				ptr += tex.Width * tex.Height;
			}
		}

		public void SetTextureData3D(
			IGLTexture texture,
			SurfaceFormat format,
			int level,
			int left,
			int top,
			int right,
			int bottom,
			int front,
			int back,
			IntPtr data,
			int dataLength
		) {
			int w = right - left;
			int h = bottom - top;
			int d = back - front;

			MTLRegion region = new MTLRegion(
				new MTLOrigin(left, top, front),
				new MTLSize(w, h, d)
			);
			mtlReplaceRegion(
				(texture as MetalTexture).Handle,
				region,
				level,
				0,
				data,
				BytesPerRow(w, format),
				BytesPerImage(w, h, format)
			);
		}

		public void SetTextureDataCube(
			IGLTexture texture,
			SurfaceFormat format,
			int xOffset,
			int yOffset,
			int width,
			int height,
			CubeMapFace cubeMapFace,
			int level,
			IntPtr data,
			int dataLength
		) {
			MetalTexture tex = texture as MetalTexture;
			IntPtr handle = tex.Handle;

			MTLOrigin origin = new MTLOrigin(xOffset, yOffset, 0);
			MTLSize size = new MTLSize(width, height, 1);
			int slice = (int) cubeMapFace;

			if (tex.IsPrivate)
			{
				// We need an active command buffer
				BeginFrame();

				// Fetch a CPU-accessible texture
				handle = FetchTransientTexture(tex);

				// Transient textures have no slices
				slice = 0;
			}

			// Write the data
			mtlReplaceRegion(
				handle,
				new MTLRegion(origin, size),
				level,
				slice,
				data,
				BytesPerRow(width, format),
				0
			);

			// Blit the temp texture to the actual texture
			if (tex.IsPrivate)
			{
				// End the render pass
				EndPass();

				// Blit!
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					blit,
					handle,
					slice,
					level,
					origin,
					size,
					tex.Handle,
					slice,
					level,
					origin
				);

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				Stall();

				// We're done with the temp texture
				mtlSetPurgeableState(
					handle,
					MTLPurgeableState.Empty
				);
			}
		}

		#endregion

		#region GetTextureData Methods

		public void GetTextureData2D(
			IGLTexture texture,
			SurfaceFormat format,
			int width,
			int height,
			int level,
			int subX,
			int subY,
			int subW,
			int subH,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
			MetalTexture tex = texture as MetalTexture;
			IntPtr handle = tex.Handle;

			MTLSize size = new MTLSize(subW, subH, 1);
			MTLOrigin origin = new MTLOrigin(subX, subY, 0);

			if (tex.IsPrivate)
			{
				// We need an active command buffer
				BeginFrame();

				// Fetch a CPU-accessible texture
				handle = FetchTransientTexture(tex);

				// End the render pass
				EndPass();

				// Blit the actual texture to a CPU-accessible texture
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					blit,
					tex.Handle,
					0,
					level,
					origin,
					size,
					handle,
					0,
					level,
					origin
				);

				// Managed resources require explicit synchronization
				if (isMac)
				{
					mtlSynchronizeResource(blit, handle);
				}

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				Stall();
			}

			mtlGetTextureBytes(
				handle,
				data,
				BytesPerRow(subW, format),
				0,
				new MTLRegion(origin, size),
				level,
				0
			);

			if (tex.IsPrivate)
			{
				// We're done with the temp texture
				mtlSetPurgeableState(
					handle,
					MTLPurgeableState.Empty
				);
			}
		}

		public void GetTextureData3D(
			IGLTexture texture,
			SurfaceFormat format,
			int left,
			int top,
			int front,
			int right,
			int bottom,
			int back,
			int level,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
			int w = right - left;
			int h = bottom - top;
			int d = back - front;

			MTLRegion region = new MTLRegion(
				new MTLOrigin(left, top, front),
				new MTLSize(w, h, d)
			);
			mtlGetTextureBytes(
				(texture as MetalTexture).Handle,
				data,
				BytesPerRow(w, format),
				BytesPerImage(w, h, format),
				region,
				level,
				0
			);
		}

		public void GetTextureDataCube(
			IGLTexture texture,
			SurfaceFormat format,
			int size,
			CubeMapFace cubeMapFace,
			int level,
			int subX,
			int subY,
			int subW,
			int subH,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
			MetalTexture tex = texture as MetalTexture;
			IntPtr handle = tex.Handle;

			MTLSize regionSize = new MTLSize(subW, subH, 1);
			MTLOrigin origin = new MTLOrigin(subX, subY, 0);
			int slice = (int) cubeMapFace;

			if (tex.IsPrivate)
			{
				// We need an active command buffer
				BeginFrame();

				// Fetch a CPU-accessible texture
				handle = FetchTransientTexture(tex);

				// Transient textures have no slices
				slice = 0;

				// End the render pass
				EndPass();

				// Blit the actual texture to a CPU-accessible texture
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlBlitTextureToTexture(
					blit,
					tex.Handle,
					(int) cubeMapFace,
					level,
					origin,
					regionSize,
					handle,
					slice,
					level,
					origin
				);

				// Managed resources require explicit synchronization
				if (isMac)
				{
					mtlSynchronizeResource(blit, handle);
				}

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				Stall();
			}

			mtlGetTextureBytes(
				handle,
				data,
				BytesPerRow(subW, format),
				0,
				new MTLRegion(origin, regionSize),
				level,
				slice
			);

			if (tex.IsPrivate)
			{
				// We're done with the temp texture
				mtlSetPurgeableState(
					handle,
					MTLPurgeableState.Empty
				);
			}
		}

		#endregion

		#region ReadBackbuffer Method

		public void ReadBackbuffer(
			IntPtr data,
			int dataLen,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int subX,
			int subY,
			int subW,
			int subH
		) {
			/* FIXME: Right now we're expecting one of the following:
			 * - byte[]
			 * - int[]
			 * - uint[]
			 * - Color[]
			 * Anything else will freak out because we're using
			 * color backbuffers. Maybe check this out when adding
			 * support for more backbuffer types!
			 * -flibit
			 */

			if (startIndex > 0 || elementCount != (dataLen / elementSizeInBytes))
			{
				throw new NotImplementedException(
					"ReadBackbuffer startIndex/elementCount"
				);
			}

			GetTextureData2D(
				(Backbuffer as MetalBackbuffer).Texture,
				SurfaceFormat.Color,
				Backbuffer.Width,
				Backbuffer.Height,
				0,
				subX,
				subY,
				subW,
				subH,
				data,
				0,
				dataLen,
				1
			);
		}

		#endregion

		#region RenderTarget->Texture Method

		public void ResolveTarget(RenderTargetBinding target)
		{
			// The target is resolved at the end of each render pass.

			// If the target has mipmaps, regenerate them now
			if (target.RenderTarget.LevelCount > 1)
			{
				EndPass();

				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				mtlGenerateMipmapsForTexture(
					blit,
					(target.RenderTarget.texture as MetalTexture).Handle
				);
				mtlEndEncoding(blit);

				needNewRenderPass = true;
			}
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

			needNewRenderPass |= clearTarget | clearDepth | clearStencil;
		}

		#endregion

		#region SetRenderTargets Methods

		public void SetRenderTargets(
			RenderTargetBinding[] renderTargets,
			IGLRenderbuffer renderbuffer,
			DepthFormat depthFormat
		) {
			// Perform any pending clears before switching render targets
			if (shouldClearColor || shouldClearDepth || shouldClearStencil)
			{
				UpdateRenderPass();
			}

			// Force an update to the render pass
			needNewRenderPass = true;

			// Bind the correct framebuffer
			ResetAttachments();
			if (renderTargets == null)
			{
				BindBackbuffer();
				return;
			}

			// Update color buffers
			int i;
			for (i = 0; i < renderTargets.Length; i += 1)
			{
				IRenderTarget rt = renderTargets[i].RenderTarget as IRenderTarget;
				currentAttachmentSlices[i] = renderTargets[i].CubeMapFace;
				if (rt.ColorBuffer != null)
				{
					MetalRenderbuffer rb = rt.ColorBuffer as MetalRenderbuffer;
					currentAttachments[i] = rb.Handle;
					currentColorFormats[i] = rb.PixelFormat;
					currentSampleCount = rb.MultiSampleCount;
					currentMSAttachments[i] = rb.MultiSampleHandle;
				}
				else
				{
					MetalTexture tex = renderTargets[i].RenderTarget.texture as MetalTexture;
					currentAttachments[i] = tex.Handle;
					currentColorFormats[i] = XNAToMTL.TextureFormat[(int) tex.Format];
					currentSampleCount = 0;
				}
			}

			// Update depth stencil buffer
			IntPtr handle = IntPtr.Zero;
			if (renderbuffer != null)
			{
				handle = (renderbuffer as MetalRenderbuffer).Handle;
			}
			currentDepthStencilBuffer = handle;
			currentDepthFormat = (
				(currentDepthStencilBuffer == IntPtr.Zero) ?
				DepthFormat.None :
				depthFormat
			);
		}

		private void ResetAttachments()
		{
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				currentAttachments[i] = IntPtr.Zero;
				currentColorFormats[i] = MTLPixelFormat.Invalid;
				currentMSAttachments[i] = IntPtr.Zero;
				currentAttachmentSlices[i] = (CubeMapFace) 0;
			}
			currentDepthStencilBuffer = IntPtr.Zero;
			currentDepthFormat = DepthFormat.None;
			currentSampleCount = 0;
		}

		private void BindBackbuffer()
		{
			MetalBackbuffer bb = Backbuffer as MetalBackbuffer;
			currentAttachments[0] = bb.ColorBuffer;
			currentColorFormats[0] = bb.PixelFormat;
			currentDepthStencilBuffer = bb.DepthStencilBuffer;
			currentDepthFormat = bb.DepthFormat;
			currentSampleCount = bb.MultiSampleCount;
			currentMSAttachments[0] = bb.MultiSampleColorBuffer;
			currentAttachmentSlices[0] = (CubeMapFace) 0;
		}

		#endregion

		#region Query Object Methods

		public IGLQuery CreateQuery()
		{
			if (!supportsOcclusionQueries)
			{
				throw new NotSupportedException(
					"Occlusion queries are not supported on this device!"
				);
			}

			IntPtr buf = mtlNewBufferWithLength(device, sizeof(ulong), 0);
			return new MetalQuery(buf);
		}

		private void DeleteQuery(IGLQuery query)
		{
			(query as MetalQuery).Dispose();
		}

		public void QueryBegin(IGLQuery query)
		{
			// Stop the current pass
			EndPass();

			// Attach the visibility buffer to a new render pass
			currentVisibilityBuffer = (query as MetalQuery).Handle;
			needNewRenderPass = true;
		}

		public bool QueryComplete(IGLQuery query)
		{
			/* FIXME:
			 * There's no easy way to check for completion
			 * of the query. The only accurate way would be
			 * to monitor the completion of the command buffer
			 * associated with each query, but that gets tricky
			 * since we can't use completion callbacks.
			 * (Thank Objective-C and its stupid "block" lambdas.)
			 *
			 * Futhermore, I don't know how visibility queries
			 * work across command buffers, in the event of a
			 * stalled buffer overwrite or something similar.
			 *
			 * The below code is obviously wrong, but it happens
			 * to work for the Lens Flare XNA sample. Maybe it'll
			 * work for your game too?
			 *
			 * (Although if you're making a new game with FNA,
			 * you really shouldn't be using queries anyway...)
			 *
			 * -caleb
			 */
			return true;
		}

		public void QueryEnd(IGLQuery query)
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				// Stop counting.
				mtlSetVisibilityResultMode(
					renderCommandEncoder,
					MTLVisibilityResultMode.Disabled,
					0
				);
			}
			currentVisibilityBuffer = IntPtr.Zero;
		}

		public int QueryPixelCount(IGLQuery query)
		{
			IntPtr contents = mtlGetBufferContentsPtr(
				(query as MetalQuery).Handle
			);
			ulong result;
			unsafe
			{
				result = *((ulong *) contents);
			}
			return (int) result;
		}

		#endregion

		#region XNA->GL Enum Conversion Class

		private static class XNAToMTL
		{
			public static readonly MTLPixelFormat[] TextureFormat = new MTLPixelFormat[]
			{
				MTLPixelFormat.RGBA8Unorm,	// SurfaceFormat.Color
				MTLPixelFormat.B5G6R5Unorm,	// SurfaceFormat.Bgr565
				MTLPixelFormat.BGR5A1Unorm,	// SurfaceFormat.Bgra5551
				MTLPixelFormat.ABGR4Unorm,	// SurfaceFormat.Bgra4444
				MTLPixelFormat.BC1_RGBA,	// SurfaceFormat.Dxt1
				MTLPixelFormat.BC2_RGBA,	// SurfaceFormat.Dxt3
				MTLPixelFormat.BC3_RGBA,	// SurfaceFormat.Dxt5
				MTLPixelFormat.RG8Snorm,	// SurfaceFormat.NormalizedByte2
				MTLPixelFormat.RG16Snorm,	// SurfaceFormat.NormalizedByte4
				MTLPixelFormat.RGB10A2Unorm,	// SurfaceFormat.Rgba1010102
				MTLPixelFormat.RG16Unorm,	// SurfaceFormat.Rg32
				MTLPixelFormat.RGBA16Unorm,	// SurfaceFormat.Rgba64
				MTLPixelFormat.A8Unorm,		// SurfaceFormat.Alpha8
				MTLPixelFormat.R32Float,	// SurfaceFormat.Single
				MTLPixelFormat.RG32Float,	// SurfaceFormat.Vector2
				MTLPixelFormat.RGBA32Float,	// SurfaceFormat.Vector4
				MTLPixelFormat.R16Float,	// SurfaceFormat.HalfSingle
				MTLPixelFormat.RG16Float,	// SurfaceFormat.HalfVector2
				MTLPixelFormat.RGBA16Float,	// SurfaceFormat.HalfVector4
				MTLPixelFormat.RGBA16Float,	// SurfaceFormat.HdrBlendable
				MTLPixelFormat.BGRA8Unorm	// SurfaceFormat.ColorBgraEXT
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

			public static readonly MTLVertexFormat[] VertexAttribType = new MTLVertexFormat[]
			{
				MTLVertexFormat.Float,			// VertexElementFormat.Single
				MTLVertexFormat.Float2,			// VertexElementFormat.Vector2
				MTLVertexFormat.Float3,			// VertexElementFormat.Vector3
				MTLVertexFormat.Float4,			// VertexElementFormat.Vector4
				MTLVertexFormat.UChar4Normalized,	// VertexElementFormat.Color
				MTLVertexFormat.UChar4,			// VertexElementFormat.Byte4
				MTLVertexFormat.Short2,			// VertexElementFormat.Short2
				MTLVertexFormat.Short4,			// VertexElementFormat.Short4
				MTLVertexFormat.Short2Normalized,	// VertexElementFormat.NormalizedShort2
				MTLVertexFormat.Short4Normalized,	// VertexElementFormat.NormalizedShort4
				MTLVertexFormat.Half2,			// VertexElementFormat.HalfVector2
				MTLVertexFormat.Half4			// VertexElementFormat.HalfVector4
			};

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

			public static int ColorWriteMask(ColorWriteChannels channels)
			{
				if (channels == ColorWriteChannels.None)
				{
					return 0x0;
				}
				if (channels == ColorWriteChannels.All)
				{
					return 0xf;
				}

				int ret = 0;
				if ((channels & ColorWriteChannels.Red) != 0)
				{
					ret |= (0x1 << 3);
				}
				if ((channels & ColorWriteChannels.Green) != 0)
				{
					ret |= (0x1 << 2);
				}
				if ((channels & ColorWriteChannels.Blue) != 0)
				{
					ret |= (0x1 << 1);
				}
				if ((channels & ColorWriteChannels.Alpha) != 0)
				{
					ret |= (0x1 << 0);
				}
				return ret;
			}

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

			public static float DepthBiasScale(MTLPixelFormat format)
			{
				switch (format)
				{
					case MTLPixelFormat.Depth16Unorm:
						return (float) ((1 << 16) - 1);

					case MTLPixelFormat.Depth24Unorm_Stencil8:
						return (float) ((1 << 24) - 1);

					case MTLPixelFormat.Depth32Float:
					case MTLPixelFormat.Depth32Float_Stencil8:
						return (float) ((1 << 23) - 1);
				}

				return 0.0f;
			}

			public static readonly MTLCullMode[] CullingEnabled = new MTLCullMode[]
			{
				MTLCullMode.None,	// CullMode.None
				MTLCullMode.Front,	// CullMode.CullClockwiseFace
				MTLCullMode.Back	// CullMode.CullCounterClockwiseFace
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

			public MetalTexture Texture
			{
				get;
				private set;
			}

			public IntPtr ColorBuffer = IntPtr.Zero;
			public IntPtr MultiSampleColorBuffer = IntPtr.Zero;
			public IntPtr DepthStencilBuffer = IntPtr.Zero;
			
			private MetalDevice mtlDevice;

			public MetalBackbuffer(
				MetalDevice device,
				PresentationParameters presentationParameters
			) {
				mtlDevice = device;
				PixelFormat = MTLPixelFormat.RGBA8Unorm;

				/* Set these now to prevent a changed event in Create!
				 * The rest will be set and don't have checks anywhere.
				 * -flibit
				 */
				Width = presentationParameters.BackBufferWidth;
				Height = presentationParameters.BackBufferHeight;
			}

			public void Dispose()
			{
				objc_release(ColorBuffer);
				ColorBuffer = IntPtr.Zero;

				objc_release(MultiSampleColorBuffer);
				MultiSampleColorBuffer = IntPtr.Zero;

				objc_release(DepthStencilBuffer);
				DepthStencilBuffer = IntPtr.Zero;
			}

			public void ResetFramebuffer(
				PresentationParameters presentationParameters
			) {
				// Just destroy and recreate from scratch
				Dispose();
				CreateFramebuffer(presentationParameters);
			}

			public void CreateFramebuffer(
				PresentationParameters presentationParameters
			) {
				// Update the backbuffer size
				int newWidth = presentationParameters.BackBufferWidth;
				int newHeight = presentationParameters.BackBufferHeight;
				if (Width != newWidth || Height != newHeight)
				{
					mtlDevice.fauxBackbufferSizeChanged = true;
				}
				Width = newWidth;
				Height = newHeight;

				// Update other presentation parameters
				DepthFormat = presentationParameters.DepthStencilFormat;
				MultiSampleCount = mtlDevice.GetCompatibleSampleCount(
					presentationParameters.MultiSampleCount
				);

				// Update color buffer to the new resolution.
				IntPtr colorBufferDesc = mtlMakeTexture2DDescriptor(
					PixelFormat,
					Width,
					Height,
					false
				);
				mtlSetStorageMode(
					colorBufferDesc,
					MTLStorageMode.Private
				);
				mtlSetTextureUsage(
					colorBufferDesc,
					MTLTextureUsage.RenderTarget | MTLTextureUsage.ShaderRead
				);
				ColorBuffer = mtlNewTextureWithDescriptor(
					mtlDevice.device,
					colorBufferDesc
				);
				if (MultiSampleCount > 0)
				{
					mtlSetTextureType(
						colorBufferDesc,
						MTLTextureType.Multisample2D
					);
					mtlSetTextureSampleCount(
						colorBufferDesc,
						MultiSampleCount
					);
					mtlSetTextureUsage(
						colorBufferDesc,
						MTLTextureUsage.RenderTarget
					);
					MultiSampleColorBuffer = mtlNewTextureWithDescriptor(
						mtlDevice.device,
						colorBufferDesc
					);
				}

				// Update the depth/stencil buffer, if applicable
				if (DepthFormat != DepthFormat.None)
				{
					IntPtr depthStencilBufferDesc = mtlMakeTexture2DDescriptor(
						mtlDevice.GetDepthFormat(DepthFormat),
						Width,
						Height,
						false
					);
					mtlSetStorageMode(
						depthStencilBufferDesc,
						MTLStorageMode.Private
					);
					mtlSetTextureUsage(
						depthStencilBufferDesc,
						MTLTextureUsage.RenderTarget
					);
					if (MultiSampleCount > 0)
					{
						mtlSetTextureType(
							depthStencilBufferDesc,
							MTLTextureType.Multisample2D
						);
						mtlSetTextureSampleCount(
							depthStencilBufferDesc,
							MultiSampleCount
						);
					}
					DepthStencilBuffer = mtlNewTextureWithDescriptor(
						mtlDevice.device,
						depthStencilBufferDesc
					);
				}

				// Update the Texture representation
				Texture = new MetalTexture(
					ColorBuffer,
					Width,
					Height,
					SurfaceFormat.Color,
					1,
					true
				);

				// This is the default render target
				mtlDevice.SetRenderTargets(null, null, DepthFormat.None);
			}
		}

		private void InitializeFauxBackbuffer(
			PresentationParameters presentationParameters
		) {
			MetalBackbuffer mtlBackbuffer = new MetalBackbuffer(
				this,
				presentationParameters
			);
			Backbuffer = mtlBackbuffer;
			mtlBackbuffer.CreateFramebuffer(presentationParameters);

			/* Create a combined vertex/index buffer
			 * for rendering the faux-backbuffer.
			 */
			fauxBackbufferDrawBuffer = mtlNewBufferWithLength(
				device,
				(16 * sizeof(float)) + (6 * sizeof(ushort)),
				MTLResourceOptions.CPUCacheModeWriteCombined
			);

			ushort[] indices = new ushort[]
			{
				0, 1, 3,
				1, 2, 3
			};
			GCHandle indicesPinned = GCHandle.Alloc(indices, GCHandleType.Pinned);
			SDL.SDL_memcpy(
				mtlGetBufferContentsPtr(fauxBackbufferDrawBuffer) + (16 * sizeof(float)),
				indicesPinned.AddrOfPinnedObject(),
				(IntPtr) (6 * sizeof(ushort))
			);
			indicesPinned.Free();

			// Create vertex and fragment shaders for the faux-backbuffer pipeline
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
					out.position.y *= -1;
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

			IntPtr nsShaderSource = UTF8ToNSString(shaderSource);
			IntPtr nsVertexShader = UTF8ToNSString("vertexShader");
			IntPtr nsFragmentShader = UTF8ToNSString("fragmentShader");

			IntPtr library = mtlNewLibraryWithSource(
				device,
				nsShaderSource,
				IntPtr.Zero
			);
			IntPtr vertexFunc = mtlNewFunctionWithName(
				library,
				nsVertexShader
			);
			IntPtr fragFunc = mtlNewFunctionWithName(
				library,
				nsFragmentShader
			);

			objc_release(nsShaderSource);
			objc_release(nsVertexShader);
			objc_release(nsFragmentShader);

			// Create a sampler state
			IntPtr samplerDescriptor = mtlNewSamplerDescriptor();
			mtlSetSamplerMinFilter(samplerDescriptor, backbufferScaleMode);
			mtlSetSamplerMagFilter(samplerDescriptor, backbufferScaleMode);
			fauxBackbufferSamplerState = mtlNewSamplerStateWithDescriptor(
				device,
				samplerDescriptor
			);
			objc_release(samplerDescriptor);

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
			objc_release(pipelineDesc);
			objc_release(vertexFunc);
			objc_release(fragFunc);
		}

		#endregion
	}
}
