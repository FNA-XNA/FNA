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
 * [5] https://www.shawnhargreaves.com/blog/setdataoptions-nooverwrite-versus-discard.html
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

			public MTLPixelFormat Format;
			public IntPtr SamplerHandle;
			public TextureAddressMode WrapS;
			public TextureAddressMode WrapT;
			public TextureAddressMode WrapR;
			public TextureFilter Filter;
			public float Anisotropy;
			public int MaxMipmapLevel;
			public float LODBias;

			public IntPtr CPUHandle = IntPtr.Zero;

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
				Format = XNAToMTL.TextureFormat[(int) format];
				HasMipmaps = levelCount > 1;
				IsPrivate = isPrivate;

				if (!IsPrivate)
				{
					// For Managed and Shared, these are the same.
					CPUHandle = Handle;
				}

				WrapS = TextureAddressMode.Wrap;
				WrapT = TextureAddressMode.Wrap;
				WrapR = TextureAddressMode.Wrap;
				Filter = TextureFilter.Linear;
				Anisotropy = 4.0f;
				MaxMipmapLevel = 0;
				LODBias = 0.0f;
			}

			/* FIXME: Could we create a cache of CPU-accessible
			 * textures instead of creating new ones all the time?
			 * -caleb
			 */
			public void MakeCPUTexture(IntPtr device)
			{
				IntPtr texDesc = mtlMakeTexture2DDescriptor(
					Format,
					(ulong) Width,
					(ulong) Height,
					HasMipmaps
				);
				CPUHandle = mtlNewTextureWithDescriptor(
					device,
					texDesc
				);
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

			public int MultiSampleCount
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

			public MetalRenderbuffer(
				IntPtr mtlDevice,
				int width,
				int height,
				bool isDepthStencil,
				SurfaceFormat format,
				DepthFormat depthFormat,
				int multiSampleCount
			) {
				Width = width;
				Height = height;
				IsDepthStencil = isDepthStencil;
				Format = format;
				DepthFormat = depthFormat;
				MultiSampleCount = multiSampleCount;

				// Generate the texture
				MTLPixelFormat pixelFormat = (
					isDepthStencil ?
					XNAToMTL.DepthFormat[(int) depthFormat] :
					XNAToMTL.TextureFormat[(int) format]
				);
				IntPtr desc = mtlMakeTexture2DDescriptor(
					pixelFormat,
					(ulong) Width,
					(ulong) Height,
					false
				);
				mtlSetStorageMode(
					desc,
					MTLResourceStorageMode.Private
				);
				mtlSetTextureUsage(
					desc,
					MTLTextureUsage.RenderTarget
				);
				if (multiSampleCount > 1)
				{
					mtlSetTextureType(
						desc,
						MTLTextureType.Multisample2D
					);
					mtlSetTextureSampleCount(
						desc,
						multiSampleCount
					);
				}
				Handle = mtlNewTextureWithDescriptor(
					mtlDevice,
					desc
				);
			}
		}

		#endregion

		#region Metal Buffer Container Class

		private class MetalBuffer : IGLBuffer
		{
			public IntPtr Handle
			{
				get
				{
					return internalBuffers[frame];
				}
			}

			public IntPtr Contents
			{
				get
				{
					return mtlGetBufferContentsPtr(Handle);
				}
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
			private IntPtr[] internalBuffers;
			private int internalBufferSize = 0;
			private int prevDataLength = 0;
			private int frame = 0;
			private int copiesNeeded = 0;
			private bool dynamic = false;
			private bool variableDataSize = false;

			public MetalBuffer(
				MetalDevice device,
				IntPtr bufferSize,
				bool dynamic,
				bool variableDataSize
			) {
				this.device = device;
				this.mtlDevice = device.device;
				BufferSize = bufferSize;
				this.dynamic = dynamic;
				this.variableDataSize = variableDataSize;

				/* Since dynamic buffers will likely be overwritten
				 * in a single frame, allocate more space up-front.
				 *
				 * Note that in the case of a dynamic buffer set with
				 * SetDataOptions.None (e.g. immediate mode SpriteBatch),
				 * the bigger this number, the faster the performance
				 * since there's more batching between CPU stalls.
				 *
				 * -caleb
				 */
				internalBufferSize = (
					(int) bufferSize *
					(dynamic ? 8 : 1)
				);
				internalBuffers = new IntPtr[device.backingBufferCount];
				for (int i = 0; i < internalBuffers.Length; i += 1)
				{
					CreateBackingBuffer(i);
				}
			}

			private void CreateBackingBuffer(int f)
			{
				IntPtr oldBuffer = internalBuffers[f];
				IntPtr newBuffer = mtlNewBufferWithLength(
					mtlDevice,
					(uint) internalBufferSize
				);
				internalBuffers[f] = newBuffer;
				if (oldBuffer != IntPtr.Zero)
				{
					// Copy over data from old buffer
					memcpy(
						mtlGetBufferContentsPtr(newBuffer),
						mtlGetBufferContentsPtr(oldBuffer),
						(IntPtr) mtlGetBufferLength(oldBuffer)
					);

					// Free the old buffer
					ObjCRelease(oldBuffer);
				}
			}

			public void SetData(
				int offsetInBytes,
				IntPtr data,
				int dataLength,
				SetDataOptions options
			) {
				int len = variableDataSize ? dataLength : (int) BufferSize;
				if (options == SetDataOptions.Discard)
				{
					HandleOverwrite(dynamic, len);
				}
				else if (options == SetDataOptions.None)
				{
					HandleOverwrite(false, len);
				}

				// Copy the data into the buffer
				memcpy(
					Contents + InternalOffset + offsetInBytes,
					data,
					(IntPtr) dataLength
				);

				// Set flags for the next SetData or EndOfFrame call
				if (!dynamic)
				{
					copiesNeeded = device.backingBufferCount - 1;
				}
				prevDataLength = len;
			}

			private void HandleOverwrite(bool shouldExpand, int dataLength)
			{
				InternalOffset += prevDataLength;

				int sizeNeeded = InternalOffset + dataLength;
				if (sizeNeeded > (int) mtlGetBufferLength(Handle))
				{
					/* We can't stall if we're on a background thread.
					 * Let's just expand the buffer instead. It'll use
					 * a bit more memory, but that's better than crashing!
					 * -caleb
					 */
					shouldExpand |= !device.OnMainThread();

					if (shouldExpand)
					{
						if (sizeNeeded >= internalBufferSize)
						{
							// Increase capacity when we're out of room
							FNALoggerEXT.LogWarn("We need more space! Increasing internal buffer size!");
							internalBufferSize = Math.Max(
								internalBufferSize * 2,
								internalBufferSize + dataLength
							);
						}
						CreateBackingBuffer(frame);
					}
					else
					{
						// Stall until we can rewrite this buffer
						mtlEndEncoding(device.renderCommandEncoder);
						device.renderCommandEncoder = IntPtr.Zero;

						mtlCommitCommandBuffer(device.commandBuffer);
						mtlCommandBufferWaitUntilCompleted(device.commandBuffer);

						device.commandBuffer = mtlMakeCommandBuffer(device.queue);
						device.needNewRenderPass = true;
						device.UpdateRenderPass();
						InternalOffset = 0;
					}
				}
			}

			public void EndOfFrame()
			{
				int lastFrame = frame;
				InternalOffset = 0;
				frame = (frame + 1) % device.backingBufferCount;
				prevDataLength = 0;

				if (copiesNeeded > 0)
				{
					// Copy the last frame's contents to the new one
					FNALoggerEXT.LogInfo("Copy " + Handle);
					int dstLen = (int) mtlGetBufferLength(Handle);
					if (dstLen < internalBufferSize)
					{
						CreateBackingBuffer(frame);
					}
					memcpy(
						mtlGetBufferContentsPtr(Handle),
						mtlGetBufferContentsPtr(internalBuffers[lastFrame]),
						(IntPtr) internalBufferSize
					);
					copiesNeeded -= 1;
				}
			}

			public void Dispose()
			{
				for (int i = 0; i < internalBuffers.Length; i += 1)
				{
					ObjCRelease(internalBuffers[i]);
					internalBuffers[i] = IntPtr.Zero;
				}
				internalBuffers = null;
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
		private bool[] textureNeedsUpdate;
		private bool[] samplerNeedsUpdate;

		#endregion

		#region Buffer Binding Cache Variables

		private List<MetalBuffer> Buffers = new List<MetalBuffer>();
		private IntPtr ldEffect = IntPtr.Zero;
		private IntPtr ldTechnique = IntPtr.Zero;
		private uint ldPass = 0;

		private VertexDeclaration userVertexDeclaration = null;
		private MetalBuffer userVertexBuffer = null;
		private MetalBuffer userIndexBuffer = null;

		#endregion

		#region Render Target Cache Variables

		private enum AttachmentType
		{
			None,
			Backbuffer,
			Renderbuffer,
			Texture2D,
			TextureCubeMapPositiveX,
			TextureCubeMapNegativeX,
			TextureCubeMapPositiveY,
			TextureCubeMapNegativeY,
			TextureCubeMapPositiveZ,
			TextureCubeMapNegativeZ
		}

		private readonly IntPtr[] currentAttachments;
		private readonly AttachmentType[] currentAttachmentTypes;
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

		private bool needNewRenderPass = false;
		private bool shouldClearColor = false;
		private bool shouldClearDepth = false;
		private bool shouldClearStencil = false;

		private readonly int backingBufferCount = 2;
		private Queue<IntPtr> submittedCommandBuffers;

		private int mainThreadID;
		private string platform;

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

		private Dictionary<int, IntPtr> PipelineStateCache =
			new Dictionary<int, IntPtr>();

		private Dictionary<StateHash, IntPtr> DepthStencilStateCache =
			new Dictionary<StateHash, IntPtr>();

		private Dictionary<StateHash, IntPtr> SamplerStateCache =
			new Dictionary<StateHash, IntPtr>();

		#endregion

		#region Private Render Pipeline State Variables

		private BlendState blendState;
		private DepthStencilState depthStencilState;

		private IntPtr ldDepthStencilState = IntPtr.Zero;
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

		private bool renderTargetBound = false;
		private bool effectApplied = false;

		private IntPtr currentVertexShader = IntPtr.Zero;
		private IntPtr currentFragmentShader = IntPtr.Zero;
		private IntPtr currentVertUniformBuffer = IntPtr.Zero;
		private IntPtr currentFragUniformBuffer = IntPtr.Zero;
		private int currentVertUniformOffset = 0;
		private int currentFragUniformOffset = 0;

		private IntPtr prevEffect = IntPtr.Zero;
		private IntPtr prevVertexShader = IntPtr.Zero;
		private IntPtr prevFragmentShader = IntPtr.Zero;
		private IntPtr prevVertUniformBuffer = IntPtr.Zero;
		private IntPtr prevFragUniformBuffer = IntPtr.Zero;
		private int prevVertUniformOffset = 0;
		private int prevFragUniformOffset = 0;

		#endregion

		#region Private Graphics Object Disposal Queues

		private Queue<IGLTexture> GCTextures = new Queue<IGLTexture>();
		private Queue<IGLRenderbuffer> GCDepthBuffers = new Queue<IGLRenderbuffer>();
		private Queue<IGLBuffer> GCVertexBuffers = new Queue<IGLBuffer>();
		private Queue<IGLBuffer> GCIndexBuffers = new Queue<IGLBuffer>();
		private Queue<IGLEffect> GCEffects = new Queue<IGLEffect>();
		private Queue<IGLQuery> GCQueries = new Queue<IGLQuery>();

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
			IntPtr metalView,
			string platform
		) {
			device = MTLCreateSystemDefaultDevice();
			queue = mtlNewCommandQueue(device);

			// Get the CAMetalLayer for this view
			layer = mtlGetLayer(metalView);
			mtlSetLayerDevice(layer, device);
			mtlSetLayerFramebufferOnly(layer, true);

			// Log GLDevice info
			FNALoggerEXT.LogInfo("IGLDevice: MetalDevice");
			FNALoggerEXT.LogInfo("Device Name: " + mtlGetDeviceName(device));
			FNALoggerEXT.LogInfo("MojoShader Profile: metal");

			/* FIXME: This environment variable still says "OPENGL".
			 * Should we introduce a METAL equivalent or just use GL?
			 * For backwards compatibility I'm thinking the latter.
			 * Its naming can be chalked up to "legacy reasons".
			 * -caleb
			 */
			// Some users might want pixely upscaling...
			backbufferScaleMode = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_BACKBUFFER_SCALE_NEAREST"
			) == "1" ? MTLSamplerMinMagFilter.Nearest : MTLSamplerMinMagFilter.Linear;

			// Set device properties
			this.platform = platform;
			SupportsS3tc = platform.Equals("Mac OS X");
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
			textureNeedsUpdate = new bool[MaxTextureSlots];
			samplerNeedsUpdate = new bool[MaxTextureSlots];

			// Initialize attachments array
			currentAttachments = new IntPtr[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];
			currentColorFormats = new MTLPixelFormat[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];
			currentAttachmentTypes = new AttachmentType[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				currentAttachments[i] = IntPtr.Zero;
				currentColorFormats[i] = MTLPixelFormat.Invalid;
				currentAttachmentTypes[i] = AttachmentType.None;
			}

			// Initialize vertex buffer cache
			ldVertexBuffers = new IntPtr[MAX_BOUND_VERTEX_BUFFERS];
			ldVertexBufferOffsets = new int[MAX_BOUND_VERTEX_BUFFERS];

			// Initialize submitted command buffer synchronization queue
			submittedCommandBuffers = new Queue<IntPtr>(backingBufferCount);

			// Store the main thread ID
			mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

			// Create and setup the faux-backbuffer
			InitializeFauxBackbuffer(presentationParameters);

			// Begin the autorelease pool
			pool = StartAutoreleasePool();

			// Create the inaugural command buffer!
			commandBuffer = mtlMakeCommandBuffer(queue);
		}

		#endregion

		#region Dispose Method

		public void Dispose()
		{
			if (renderCommandEncoder != IntPtr.Zero)
			{
				mtlEndEncoding(renderCommandEncoder);
			}

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

			// Release sampler states
			foreach (IntPtr ss in SamplerStateCache.Values)
			{
				ObjCRelease(ss);
			}
			SamplerStateCache.Clear();
			SamplerStateCache = null;

			// Dispose the backbuffer
			(Backbuffer as MetalBackbuffer).Dispose();
		}

		#endregion

		#region GetDrawableSize Methods

		/* FIXME: This should be replaced by its SDL2
		 * equivalent if/when this patch gets merged:
		 * https://bugzilla.libsdl.org/show_bug.cgi?id=4796
		 * -caleb
		 */
		public static void FNA_Metal_GetDrawableSize(
			IntPtr view,
			out int w,
			out int h
		) {
			IntPtr l = mtlGetLayer(view);
			GetDrawableSize(l, out w, out h);
		}

		private static void GetDrawableSize(
			IntPtr layer,
			out int w,
			out int h
		) {
			CGSize size = mtlGetDrawableSize(layer);
			w = (int) size.width;
			h = (int) size.height;
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
				GetDrawableSize(
					layer,
					out dstW,
					out dstH
				);
			}

			// "Blit" the backbuffer to the drawable
			CopyTextureRegion(
				colorBuffer,
				new Rectangle(srcX, srcY, srcW, srcH),
				mtlGetTextureFromDrawable(drawable),
				new Rectangle(dstX, dstY, dstW, dstH)
			);

			// Submit the command buffer for presentation
			mtlPresentDrawable(commandBuffer, drawable);
			mtlCommitCommandBuffer(commandBuffer);

			// Put it in the queue so we can track it
			submittedCommandBuffers.Enqueue(commandBuffer);
			ObjCRetain(commandBuffer);

			// Release allocations from this frame
			DrainAutoreleasePool(pool);

			// Wait until we can submit another command buffer
			if (submittedCommandBuffers.Count >= backingBufferCount)
			{
				IntPtr cmdbuf = submittedCommandBuffers.Dequeue();
				mtlCommandBufferWaitUntilCompleted(cmdbuf);
				ObjCRelease(cmdbuf);
			}

			// Clear out all the deleted resources
			while (GCTextures.Count > 0)
			{
				DeleteTexture(GCTextures.Dequeue());
			}
			while (GCDepthBuffers.Count > 0)
			{
				DeleteRenderbuffer(GCDepthBuffers.Dequeue());
			}
			while (GCVertexBuffers.Count > 0)
			{
				DeleteBuffer(GCVertexBuffers.Dequeue());
			}
			while (GCIndexBuffers.Count > 0)
			{
				DeleteBuffer(GCIndexBuffers.Dequeue());
			}
			while (GCEffects.Count > 0)
			{
				DeleteEffect(GCEffects.Dequeue());
			}
			while (GCQueries.Count > 0)
			{
				DeleteQuery(GCQueries.Dequeue());
			}

			// The cycle begins anew...
			pool = StartAutoreleasePool();
			commandBuffer = mtlMakeCommandBuffer(queue);
			renderCommandEncoder = IntPtr.Zero;

			// Reset all buffers
			for (int i = 0; i < Buffers.Count; i += 1)
			{
				Buffers[i].EndOfFrame();
			}
			MojoShader.MOJOSHADER_mtlEndFrame();

			// Go back to using the faux-backbuffer
			ResetAttachments();
			BindBackbuffer();
		}

		private void CopyTextureRegion(
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
				// FIXME: OpenGL lets this slide, but what does XNA do here?
				throw new InvalidOperationException(
					"sourceRectangle and destinationRectangle must have non-zero width and height!"
				);
			}

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
				GetDrawableSize(
					layer,
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
				GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
				memcpy(
					mtlGetBufferContentsPtr(fauxBackbufferDrawBuffer),
					handle.AddrOfPinnedObject(),
					(IntPtr) (16 * sizeof(float))
				);
				handle.Free();
			}

			mtlSetVertexBuffer(
				rce,
				fauxBackbufferDrawBuffer,
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
				fauxBackbufferDrawBuffer,
				16 * sizeof(float),
				1,
				0,
				0
			);

			mtlEndEncoding(rce);
		}

		#endregion

		#region Render Command Encoder Methods

		private void UpdateRenderPass()
		{
			if (!needNewRenderPass)
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

			// Bind color attachments
			for (ulong i = 0; i < (ulong) currentAttachments.Length; i += 1)
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

			// Clear depth
			if (currentDepthFormat != DepthFormat.None)
			{
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
				}
				else
				{
					mtlSetAttachmentLoadAction(
						depthAttachment,
						MTLLoadAction.Load
					);
				}
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
			// FIXME: Need to test this with multiple bound render targets. -caleb
			currentAttachmentWidth = mtlGetTextureWidth(currentAttachments[0]);
			currentAttachmentHeight = mtlGetTextureHeight(currentAttachments[0]);

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

			// Reset the bindings
			for (int i = 0; i < MaxTextureSlots; i += 1)
			{
				if (Textures[i] != MetalTexture.NullTexture)
				{
					textureNeedsUpdate[i] = true;
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
					(ulong) stencilRef
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
			if (renderCommandEncoder != null && !needNewRenderPass)
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
			GCEffects.Enqueue(effect);
		}

		public void AddDisposeIndexBuffer(IGLBuffer buffer)
		{
			GCIndexBuffers.Enqueue(buffer);
		}

		public void AddDisposeQuery(IGLQuery query)
		{
			GCQueries.Enqueue(query);
		}

		public void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			GCDepthBuffers.Enqueue(renderbuffer);
		}

		public void AddDisposeTexture(IGLTexture texture)
		{
			GCTextures.Enqueue(texture);
		}

		public void AddDisposeVertexBuffer(IGLBuffer buffer)
		{
			GCVertexBuffers.Enqueue(buffer);
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
			ulong totalIndexOffset = (ulong) (
				(startIndex * XNAToMTL.IndexSize[(int) indices.IndexElementSize]) +
				(indices.buffer as MetalBuffer).InternalOffset
			);
			mtlDrawIndexedPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				XNAToMTL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToMTL.IndexType[(int) indices.IndexElementSize],
				(indices.buffer as MetalBuffer).Handle,
				totalIndexOffset,
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
			// Bind user vertex buffer
			ulong numIndices = XNAToMTL.PrimitiveVerts(
				primitiveType,
				primitiveCount
			);
			BindUserVertexBuffer(
				vertexData,
				(int) numIndices
			);

			// Bind user index buffer
			int indexSize = XNAToMTL.IndexSize[(int) indexElementSize];
			int len = (int) numIndices * indexSize;
			if (userIndexBuffer == null)
			{
				userIndexBuffer = new MetalBuffer(
					this,
					(IntPtr) len,
					true,
					true
				);
				Buffers.Add(userIndexBuffer);
			}
			userIndexBuffer.SetData(
				0,
				indexData,
				len,
				SetDataOptions.Discard
			);
			ulong totalIndexOffset = (ulong) (
				(indexOffset * indexSize) +
				userIndexBuffer.InternalOffset
			);

			// Draw!
			mtlDrawIndexedPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				numIndices,
				XNAToMTL.IndexType[(int) indexElementSize],
				userIndexBuffer.Handle,
				totalIndexOffset,
				1,
				vertexOffset,
				0
			);
		}

		public void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		) {
			ulong numVerts = XNAToMTL.PrimitiveVerts(
				primitiveType,
				primitiveCount
			);
			BindUserVertexBuffer(
				vertexData,
				(int) numVerts
			);
			mtlDrawPrimitives(
				renderCommandEncoder,
				XNAToMTL.Primitive[(int) primitiveType],
				(ulong) vertexOffset,
				numVerts
			);
		}

		private void BindUserVertexBuffer(
			IntPtr vertexData,
			int vertexCount
		) {
			UpdateRenderPass();

			int len = vertexCount * userVertexDeclaration.VertexStride;
			if (userVertexBuffer == null)
			{
				userVertexBuffer = new MetalBuffer(
					this,
					(IntPtr) len,
					true,
					true
				);
				Buffers.Add(userVertexBuffer);
			}
			userVertexBuffer.SetData(
				0,
				vertexData,
				len,
				SetDataOptions.Discard
			);

			int offset = userVertexBuffer.InternalOffset;
			IntPtr handle = userVertexBuffer.Handle;
			if (ldVertexBuffers[0] != handle)
			{
				mtlSetVertexBuffer(
					renderCommandEncoder,
					handle,
					(ulong) offset,
					(ulong) 0
				);
				ldVertexBuffers[0] = handle;
				ldVertexBufferOffsets[0] = offset;
			}
			else if (ldVertexBufferOffsets[0] != offset)
			{
				mtlSetVertexBufferOffset(
					renderCommandEncoder,
					(ulong) offset,
					(ulong) 0
				);
				ldVertexBufferOffsets[0] = offset;
			}

			BindResources();
		}

		#endregion

		#region State Management Methods

		public void SetPresentationInterval(PresentInterval interval)
		{
			if (platform.Equals("iOS") || platform.Equals("tvOS"))
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
				textureNeedsUpdate[index] = true;
				samplerNeedsUpdate[index] = true;
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

			// Update the texture info
			tex.WrapS = sampler.AddressU;
			tex.WrapT = sampler.AddressV;
			tex.WrapR = sampler.AddressW;
			tex.Filter = sampler.Filter;
			tex.Anisotropy = sampler.MaxAnisotropy;
			tex.MaxMipmapLevel = sampler.MaxMipLevel;
			tex.LODBias = sampler.MipMapLevelOfDetailBias;
			tex.SamplerHandle = FetchSamplerState(sampler);
			if (tex.SamplerHandle != Textures[index].SamplerHandle)
			{
				samplerNeedsUpdate[index] = true;
			}

			// Bind the correct texture
			if (tex != Textures[index])
			{
				Textures[index] = tex;
				textureNeedsUpdate[index] = true;
			}
		}

		public void SetBlendState(BlendState blendState)
		{
			this.blendState = blendState;
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			this.depthStencilState = depthStencilState;
		}

		#endregion

		#region State Creation/Retrieval Methods

		/* A pipeline state is defined by these things:
		 *
		 * Vertex Shader
		 * Fragment Shader
		 * Vertex Descriptor
		 * Color Attachment Formats (0-4)
		 * Depth-Stencil Attachment Format
		 * Blend State
		 * Depth Stencil State
		 * 
		 * -caleb
		 */

		private IntPtr FetchRenderPipeline()
		{
			// Can we just reuse an existing pipeline?
			// FIXME: This hash could definitely be improved. -caleb
			int hash = unchecked(
				(int) currentVertexShader +
				(int) currentFragmentShader +
				(int) currentVertexDescriptor +
				(int) currentColorFormats[0] +
				(int) currentColorFormats[1] +
				(int) currentColorFormats[2] +
				(int) currentColorFormats[3] +
				(int) currentDepthFormat +
				PipelineCache.GetBlendHash(blendState).GetHashCode() +
				(int) FetchDepthStencilState()
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
				currentVertexShader
			);
			IntPtr fragHandle = MojoShader.MOJOSHADER_mtlGetFunctionHandle(
				currentFragmentShader
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

				/* FIXME: So how exactly do we factor in
				* COLORWRITEENABLE for buffer 0? Do we just assume that
				* the default is just buffer 0, and all other calls
				* update the other write masks?
				*/
				if (i == 0)
				{
					mtlSetAttachmentWriteMask(
						colorAttachment,
						(ulong) blendState.ColorWriteChannels
					);
				}
				else if (i == 1)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 1),
						(ulong) blendState.ColorWriteChannels1
					);
				}
				else if (i == 2)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 2),
						(ulong) blendState.ColorWriteChannels2
					);
				}
				else if (i == 3)
				{
					mtlSetAttachmentWriteMask(
						mtlGetColorAttachment(pipelineDesc, 3),
						(ulong) blendState.ColorWriteChannels3
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
			ObjCRelease(pipelineDesc);
			ObjCRelease(vertHandle);
			ObjCRelease(fragHandle);

			// Return the pipeline!
			return pipelineState;
		}

		private IntPtr FetchDepthStencilState()
		{
			// Don't apply a depth state if none was requested.
			if (	currentDepthFormat == DepthFormat.None ||
				depthStencilState.Name == "DepthStencilState.None")
			{
				return IntPtr.Zero;
			}

			// Can we just reuse an existing state?
			StateHash hash = PipelineCache.GetDepthStencilHash(
				depthStencilState
			);
			IntPtr state = IntPtr.Zero;
			if (DepthStencilStateCache.TryGetValue(hash, out state))
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
			DepthStencilStateCache[hash] = state;

			// Clean up
			ObjCRelease(dsDesc);
			ObjCRelease(back);
			ObjCRelease(front);

			// Return the state!
			return state;
		}

		private IntPtr FetchSamplerState(SamplerState samplerState)
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
			ObjCRelease(samplerDesc);

			// Return the sampler state!
			return state;
		}

		private long GetVertexDeclarationHash(VertexDeclaration declaration)
		{
			long hash = 0;
			for (int i = 0; i < declaration.elements.Length; i += 1)
			{
				VertexElement e = declaration.elements[i];
				hash += unchecked(e.UsageIndex +
					(int) e.VertexElementFormat +
					(int) e.VertexElementUsage);
			}
			hash += declaration.VertexStride + (int) currentVertexShader;
			return hash;
		}

		private IntPtr FetchVertexDescriptor(
			VertexBufferBinding[] bindings,
			int numBindings
		) {
			// Get the binding hash value
			long hash = 0;
			for (int i = 0; i < numBindings; i += 1)
			{
				VertexBufferBinding binding = bindings[i];
				hash += binding.VertexOffset +
					binding.InstanceFrequency +
					GetVertexDeclarationHash(
						binding.VertexBuffer.VertexDeclaration
					);
			}

			// Can we just reuse an existing descriptor?
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

		private IntPtr FetchVertexDescriptor(
			VertexDeclaration vertexDeclaration,
			int vertexOffset
		) {
			// Get the binding hash value
			long hash = GetVertexDeclarationHash(vertexDeclaration);

			// Can we just reuse an existing descriptor?
			IntPtr descriptor;
			if (VertexDescriptorCache.TryGetValue(hash, out descriptor))
			{
				// The value is already cached!
				return descriptor;
			}

			// We have to make a new vertex descriptor...
			descriptor = mtlMakeVertexDescriptor();
			ObjCRetain(descriptor); // Make sure this doesn't get drained

			// Describe vertex attributes
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
				backingBufferCount
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
					out currentVertexShader,
					out currentFragmentShader,
					out currentVertUniformBuffer,
					out currentFragUniformBuffer,
					out currentVertUniformOffset,
					out currentFragUniformOffset
				);
				currentEffect = IntPtr.Zero;
				currentTechnique = IntPtr.Zero;
				currentPass = 0;
				currentVertexShader = IntPtr.Zero;
				currentFragmentShader = IntPtr.Zero;
				currentVertUniformBuffer = IntPtr.Zero;
				currentFragUniformBuffer = IntPtr.Zero;
				currentVertUniformOffset = 0;
				currentFragUniformOffset = 0;
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
				backingBufferCount
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
						out currentFragUniformBuffer,
						out currentVertUniformOffset,
						out currentFragUniformOffset
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
					out currentFragUniformBuffer,
					out currentVertUniformOffset,
					out currentFragUniformOffset
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
					out currentFragUniformBuffer,
					out currentVertUniformOffset,
					out currentFragUniformOffset
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
				out currentFragUniformBuffer,
				out currentVertUniformOffset,
				out currentFragUniformOffset
			);
			currentEffect = mtlEffectData;
			currentTechnique = technique;
			currentPass = pass;
		}

		public void BeginPassRestore(IGLEffect effect, IntPtr stateChanges)
		{
			// Store the current data
			// FIXME: This is super inelegant... -caleb
			prevEffect = currentEffect;
			prevVertexShader = currentVertexShader;
			prevFragmentShader = currentFragmentShader;
			prevVertUniformBuffer = currentVertUniformBuffer;
			prevFragUniformBuffer = currentFragUniformBuffer;
			prevVertUniformOffset = currentVertUniformOffset;
			prevFragUniformOffset = currentFragUniformOffset;

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
				out currentVertexShader,
				out currentFragmentShader,
				out currentVertUniformBuffer,
				out currentFragUniformBuffer,
				out currentVertUniformOffset,
				out currentFragUniformOffset
			);
			currentEffect = mtlEffectData;
			effectApplied = true;
		}

		public void EndPassRestore(IGLEffect effect)
		{
			IntPtr mtlEffectData = (effect as MetalEffect).MTLEffectData;
			MojoShader.MOJOSHADER_mtlEffectEndPass(mtlEffectData);
			MojoShader.MOJOSHADER_mtlEffectEnd(
				mtlEffectData,
				out currentVertexShader,
				out currentFragmentShader,
				out currentVertUniformBuffer,
				out currentFragUniformBuffer,
				out currentVertUniformOffset,
				out currentFragUniformOffset
			);
			effectApplied = true;

			// Restore the old data
			// FIXME: This is super inelegant... -caleb
			currentVertexShader = prevVertexShader;
			currentFragmentShader = prevFragmentShader;
			currentVertUniformBuffer = prevVertUniformBuffer;
			currentFragUniformBuffer = prevFragUniformBuffer;
			currentVertUniformOffset = prevVertUniformOffset;
			currentFragUniformOffset = prevFragUniformOffset;
			currentEffect = prevEffect;
		}

		#endregion

		#region ApplyVertexAttributes Methods

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
						(ulong) i
					);
					textureNeedsUpdate[i] = false;
				}
				if (samplerNeedsUpdate[i])
				{
					mtlSetFragmentSamplerState(
						renderCommandEncoder,
						Textures[i].SamplerHandle,
						(ulong) i
					);
					samplerNeedsUpdate[i] = false;
				}
			}

			// Bind the uniform buffers
			const int UNIFORM_REG = 16; // In MojoShader output it's always 16
			if (currentVertUniformBuffer != ldVertUniformBuffer)
			{
				mtlSetVertexBuffer(
					renderCommandEncoder,
					currentVertUniformBuffer,
					(ulong) currentVertUniformOffset,
					UNIFORM_REG
				);
				ldVertUniformBuffer = currentVertUniformBuffer;
				ldVertUniformOffset = currentVertUniformOffset;
			}
			else if (currentVertUniformOffset != ldVertUniformOffset)
			{
				mtlSetVertexBufferOffset(
					renderCommandEncoder,
					(ulong) currentVertUniformOffset,
					UNIFORM_REG
				);
				ldVertUniformOffset = currentVertUniformOffset;
			}

			if (currentFragUniformBuffer != ldFragUniformBuffer)
			{
				mtlSetFragmentBuffer(
					renderCommandEncoder,
					currentFragUniformBuffer,
					(ulong) currentFragUniformOffset,
					UNIFORM_REG
				);
				ldFragUniformBuffer = currentFragUniformBuffer;
				ldFragUniformOffset = currentFragUniformOffset;
			}
			else if (currentFragUniformOffset != ldFragUniformOffset)
			{
				mtlSetFragmentBufferOffset(
					renderCommandEncoder,
					(ulong) currentFragUniformOffset,
					UNIFORM_REG
				);
				ldFragUniformOffset = currentFragUniformOffset;
			}

			// Bind the depth-stencil state
			IntPtr depthStencilState = FetchDepthStencilState();
			if (depthStencilState != ldDepthStencilState)
			{
				if (depthStencilState != IntPtr.Zero)
				{
					mtlSetDepthStencilState(
						renderCommandEncoder,
						depthStencilState
					);
				}
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
			}

			// Prepare for rendering
			UpdateRenderPass();
			for (int i = 0; i < bindings.Length; i += 1)
			{
				VertexBuffer vertexBuffer = bindings[i].VertexBuffer;
				if (vertexBuffer != null)
				{
					int offset = (
						bindings[i].VertexOffset +
						(vertexBuffer.buffer as MetalBuffer).InternalOffset
					);
					IntPtr handle = (vertexBuffer.buffer as MetalBuffer).Handle;

					if (ldVertexBuffers[i] != handle)
					{
						mtlSetVertexBuffer(
							renderCommandEncoder,
							handle,
							(ulong) offset,
							(ulong) i
						);
						ldVertexBuffers[i] = handle;
						ldVertexBufferOffsets[i] = offset;
					}
					else if (ldVertexBufferOffsets[i] != offset)
					{
						mtlSetVertexBufferOffset(
							renderCommandEncoder,
							(ulong) offset,
							(ulong) i
						);
						ldVertexBufferOffsets[i] = offset;
					}
				}
			}
			BindResources();
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
			if (	currentEffect != ldEffect ||
				currentTechnique != ldTechnique ||
				currentPass != ldPass ||
				effectApplied	)
			{
				// Translate the declaration into a descriptor
				currentVertexDescriptor = FetchVertexDescriptor(
					vertexDeclaration,
					vertexOffset
				);
			}

			userVertexDeclaration = vertexDeclaration;
			// The rest happens in DrawUser[Indexed]Primitives.
		}

		#endregion

		#region GenBuffers Methods

		public IGLBuffer GenIndexBuffer(
			bool dynamic,
			int indexCount,
			IndexElementSize indexElementSize
		) {
			int elementSize = XNAToMTL.IndexSize[(int) indexElementSize];
			IntPtr size = (IntPtr) (indexCount * elementSize);
			MetalBuffer newbuf = new MetalBuffer(
				this,
				size,
				dynamic,
				false
			);
			Buffers.Add(newbuf);
			return newbuf;
		}

		public IGLBuffer GenVertexBuffer(
			bool dynamic,
			int vertexCount,
			int vertexStride
		) {
			IntPtr size = (IntPtr) (vertexCount * vertexStride);
			MetalBuffer newbuf = new MetalBuffer(
				this,
				size,
				dynamic,
				false
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
			int multiSampleCount
		) {
			return new MetalRenderbuffer(
				device,
				width,
				height,
				false,
				format,
				DepthFormat.None, // ignored!
				multiSampleCount
			);
		}

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		) {
			return new MetalRenderbuffer(
				device,
				width,
				height,
				true,
				0, // ignored!
				format,
				multiSampleCount
			);
		}

		private void DeleteRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			IntPtr handle = (renderbuffer as MetalRenderbuffer).Handle;

			// Check color attachments
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (handle == currentAttachments[i])
				{
					currentAttachments[i] = IntPtr.Zero;
				}
			}

			// Check depth-stencil attachment
			if (handle == currentDepthStencilBuffer)
			{
				currentDepthStencilBuffer = IntPtr.Zero;
			}

			// Finally.
			ObjCRelease(handle);
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
				(ulong) width,
				(ulong) height,
				levelCount > 1
			);

			// Override Metal's automatic mipmap level calculation
			mtlSetMipmapLevelCount(texDesc, levelCount);

			if (isRenderTarget)
			{
				mtlSetStorageMode(
					texDesc,
					MTLResourceStorageMode.Private
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
			IntPtr handle = (texture as MetalTexture).Handle;
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (handle == currentAttachments[i])
				{
					currentAttachments[i] = IntPtr.Zero;
				}
			}
			mtlSetPurgeableState(handle, MTLPurgeableState.Empty);
			ObjCRelease(handle);
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
			ulong bytesPerRow = (ulong) (w * Texture.GetFormatSize(format));
			if (	format == SurfaceFormat.Dxt1 ||
				format == SurfaceFormat.Dxt3 ||
				format == SurfaceFormat.Dxt5	)
			{
				bytesPerRow /= (ulong) (Texture.GetFormatSize(format) / 4);
			}

			// Create a CPU-accessible texture, if needed
			MetalTexture tex = texture as MetalTexture;
			if (tex.IsPrivate && tex.CPUHandle == IntPtr.Zero)
			{
				tex.MakeCPUTexture(device);
			}

			// Write the data
			MTLRegion region = new MTLRegion(
				new MTLOrigin((ulong) x, (ulong) y, 0),
				new MTLSize((ulong) w, (ulong) h, 1)
			);
			mtlReplaceRegion(
				(texture as MetalTexture).CPUHandle,
				region,
				(ulong) level,
				data,
				bytesPerRow
			);

			if (tex.IsPrivate)
			{
				// End the render pass
				if (renderCommandEncoder != IntPtr.Zero)
				{
					mtlEndEncoding(renderCommandEncoder);
					renderCommandEncoder = IntPtr.Zero;
				}

				// Blit the texture to the GPU-private texture
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				MTLOrigin origin = new MTLOrigin(0, 0, 0);
				mtlBlitTextureToTexture(
					blit,
					tex.CPUHandle,
					0,
					(ulong) level,
					origin,
					new MTLSize(
						(ulong) tex.Width,
						(ulong) tex.Height,
						1
					),
					tex.Handle,
					0,
					(ulong) level,
					origin
				);

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				mtlCommitCommandBuffer(commandBuffer);
				mtlCommandBufferWaitUntilCompleted(commandBuffer);
				commandBuffer = mtlMakeCommandBuffer(queue);
				needNewRenderPass = true;
			}
		}

		public void SetTextureDataYUV(Texture2D[] textures, IntPtr ptr)
		{
			for (int i = 0; i < 3; i += 1)
			{
				Texture2D tex = textures[i];
				MTLRegion region = new MTLRegion(
					new MTLOrigin(0, 0, 0),
					new MTLSize((ulong) tex.Width, (ulong) tex.Height, 1)
				);
				mtlReplaceRegion(
					(tex.texture as MetalTexture).Handle,
					region,
					0,
					ptr,
					(ulong) tex.Width
				);
				ptr += tex.Width * tex.Height;
			}
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
			if (	format == SurfaceFormat.Dxt1 ||
				format == SurfaceFormat.Dxt3 ||
				format == SurfaceFormat.Dxt5	)
			{
				throw new NotImplementedException("GetData, CompressedTexture");
			}

			MetalTexture tex = texture as MetalTexture;
			if (tex.IsPrivate)
			{
				// Create a CPU-accessible texture, if needed
				if (tex.CPUHandle == IntPtr.Zero)
				{
					tex.MakeCPUTexture(device);
				}

				// End the render pass
				if (renderCommandEncoder != IntPtr.Zero)
				{
					mtlEndEncoding(renderCommandEncoder);
					renderCommandEncoder = IntPtr.Zero;
				}

				// Blit the texture to the CPU-accessible texture
				IntPtr blit = mtlMakeBlitCommandEncoder(commandBuffer);
				MTLOrigin origin = new MTLOrigin(0, 0, 0);
				mtlBlitTextureToTexture(
					blit,
					tex.Handle,
					0,
					(ulong) level,
					origin,
					new MTLSize(
						(ulong) tex.Width,
						(ulong) tex.Height,
						1
					),
					tex.CPUHandle,
					0,
					(ulong) level,
					origin
				);

				// "Managed" resources require explicit synchronization
				if (platform.Equals("Mac OS X"))
				{
					mtlSynchronizeResource(blit, tex.CPUHandle);
				}

				// Submit the blit command to the GPU and wait...
				mtlEndEncoding(blit);
				mtlCommitCommandBuffer(commandBuffer);
				mtlCommandBufferWaitUntilCompleted(commandBuffer);
				commandBuffer = mtlMakeCommandBuffer(queue);
				needNewRenderPass = true;
			}

			MTLRegion region = new MTLRegion(
				new MTLOrigin((ulong) subX, (ulong) subY, 0),
				new MTLSize((ulong) subW, (ulong) subH, 1)
			);
			mtlGetTextureBytes(
				tex.CPUHandle,
				data,
				(ulong) (subW * Texture.GetFormatSize(format)),
				region,
				(ulong) level
			);
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
			// FIXME!
			// if ((target.RenderTarget as IRenderTarget).MultiSampleCount > 0)
			// {
			// 	// Set up the texture framebuffer
			// 	int width, height;
			// 	if (target.RenderTarget is RenderTarget2D)
			// 	{
			// 		Texture2D target2D = (target.RenderTarget as Texture2D);
			// 		width = target2D.Width;
			// 		height = target2D.Height;
			// 		glNamedFramebufferTexture(
			// 			resolveFramebufferDraw,
			// 			GLenum.GL_COLOR_ATTACHMENT0,
			// 			(target.RenderTarget.texture as OpenGLTexture).Handle,
			// 			0
			// 		);
			// 	}
			// 	else
			// 	{
			// 		TextureCube targetCube = (target.RenderTarget as TextureCube);
			// 		width = targetCube.Size;
			// 		height = targetCube.Size;
			// 		glNamedFramebufferTextureLayer(
			// 			resolveFramebufferDraw,
			// 			GLenum.GL_COLOR_ATTACHMENT0,
			// 			(target.RenderTarget.texture as OpenGLTexture).Handle,
			// 			0,
			// 			(int) target.CubeMapFace
			// 		);
			// 	}

			// 	// Set up the renderbuffer framebuffer
			// 	glNamedFramebufferRenderbuffer(
			// 		resolveFramebufferRead,
			// 		GLenum.GL_COLOR_ATTACHMENT0,
			// 		GLenum.GL_RENDERBUFFER,
			// 		((target.RenderTarget as IRenderTarget).ColorBuffer as OpenGLRenderbuffer).Handle
			// 	);

			// 	// Blit!
			// 	if (scissorTestEnable)
			// 	{
			// 		glDisable(GLenum.GL_SCISSOR_TEST);
			// 	}
			// 	glBlitNamedFramebuffer(
			// 		resolveFramebufferRead,
			// 		resolveFramebufferDraw,
			// 		0, 0, width, height,
			// 		0, 0, width, height,
			// 		GLenum.GL_COLOR_BUFFER_BIT,
			// 		GLenum.GL_LINEAR
			// 	);
			// 	/* Invalidate the MSAA buffer */
			// 	glInvalidateNamedFramebufferData(
			// 		resolveFramebufferRead,
			// 		attachments.Length + 2,
			// 		drawBuffersArray
			// 	);
			// 	if (scissorTestEnable)
			// 	{
			// 		glEnable(GLenum.GL_SCISSOR_TEST);
			// 	}
			// }

			//// If the target has mipmaps, regenerate them now
			//if (target.RenderTarget.LevelCount > 1)
			//{
				//glGenerateTextureMipmap((target.RenderTarget.texture as OpenGLTexture).Handle);
			//}
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
				renderTargetBound = false;
				BindBackbuffer();
				return;
			}
			renderTargetBound = true;

			// Update color buffers
			int i;
			for (i = 0; i < renderTargets.Length; i += 1)
			{
				IGLRenderbuffer colorBuffer = (renderTargets[i].RenderTarget as IRenderTarget).ColorBuffer;
				if (colorBuffer != null)
				{
					currentAttachments[i] = (colorBuffer as MetalRenderbuffer).Handle;
					currentAttachmentTypes[i] = AttachmentType.Renderbuffer;
					currentColorFormats[i] = XNAToMTL.TextureFormat[
						(int) (colorBuffer as MetalRenderbuffer).Format
					];
				}
				else
				{
					currentAttachments[i] = (renderTargets[i].RenderTarget.texture as MetalTexture).Handle;
					currentColorFormats[i] = (renderTargets[i].RenderTarget.texture as MetalTexture).Format;
					if (renderTargets[i].RenderTarget is RenderTarget2D)
					{
						currentAttachmentTypes[i] = AttachmentType.Texture2D;
					}
					else
					{
						currentAttachmentTypes[i] = AttachmentType.TextureCubeMapPositiveX + (int) renderTargets[i].CubeMapFace;
					}
				}
			}

			// Update depth stencil state
			IntPtr handle = IntPtr.Zero;
			if (renderbuffer != null)
			{
				handle = (renderbuffer as MetalRenderbuffer).Handle;
			}
			if (handle != currentDepthStencilBuffer)
			{
				currentDepthFormat = depthFormat;
				currentDepthStencilBuffer = handle;
			}
		}

		private void ResetAttachments()
		{
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				currentAttachments[i] = IntPtr.Zero;
				currentAttachmentTypes[i] = AttachmentType.None;
				currentColorFormats[i] = MTLPixelFormat.Invalid;
			}
			currentDepthStencilBuffer = IntPtr.Zero;
			currentDepthFormat = DepthFormat.None;
		}

		private void BindBackbuffer()
		{
			MetalBackbuffer bb = (Backbuffer as MetalBackbuffer);
			currentAttachments[0] = bb.ColorBuffer;
			currentColorFormats[0] = bb.PixelFormat;
			currentAttachmentTypes[0] = AttachmentType.Backbuffer;
			currentDepthStencilBuffer = bb.DepthStencilBuffer;
			currentDepthFormat = bb.DepthFormat;
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

				// Release the existing color buffer, if applicable
				if (ColorBuffer != IntPtr.Zero)
				{
					ObjCRelease(ColorBuffer);
					ColorBuffer = IntPtr.Zero;
				}

				// Release the depth/stencil buffer, if applicable
				if (DepthStencilBuffer != IntPtr.Zero)
				{
					ObjCRelease(DepthStencilBuffer);
					DepthStencilBuffer = IntPtr.Zero;
				}

				// Update color buffer to the new resolution.
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
				mtlDevice.BindBackbuffer();
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

			/* Create a combined vertex/index buffer
			 * for rendering the faux-backbuffer.
			 */
			fauxBackbufferDrawBuffer = mtlNewBufferWithLength(
				device,
				(16 * sizeof(float)) + (6 * sizeof(ushort))
			);

			ushort[] indices = new ushort[]
			{
				0, 1, 3,
				1, 2, 3
			};
			GCHandle indicesPinned = GCHandle.Alloc(indices, GCHandleType.Pinned);
			memcpy(
				mtlGetBufferContentsPtr(fauxBackbufferDrawBuffer) + (16 * sizeof(float)),
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

			ObjCRelease(nsShaderSource);
			ObjCRelease(nsVertexShader);
			ObjCRelease(nsFragmentShader);

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
			ObjCRelease(vertexFunc);
			ObjCRelease(fragFunc);
		}

		#endregion

		#region Threading Helper Method

		private bool OnMainThread()
		{
			return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadID;
		}

		#endregion
	}
}