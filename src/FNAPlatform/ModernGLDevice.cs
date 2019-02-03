#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region THREADED_GL Option
// #define THREADED_GL
/* Ah, so I see you've run into some issues with threaded GL...
 *
 * This class is designed to handle rendering coming from multiple threads, but
 * if you're too wreckless with how many threads are calling the GL, this will
 * hang.
 *
 * With THREADED_GL we instead allow you to run threaded rendering using
 * multiple GL contexts. This is more flexible, but much more dangerous.
 *
 * Basically, if you have to enable this, you should feel very bad.
 * -flibit
 */
#endregion

#region DISABLE_THREADING Option
// #define DISABLE_THREADING
/* Perhaps you read the above option and thought to yourself:
 * "Wow, only an idiot would need threads for their graphics code!"
 *
 * For those of you who are particularly well-behaved with your renderer and
 * don't ever call anything on a thread at all, you can enable this define and
 * cut out a _ton_ of garbage generation that's caused by our attempt to force
 * things to the main thread.
 *
 * Enjoy the boost, you've earned it.
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class ModernGLDevice : IGLDevice
	{
		#region OpenGL Texture Container Class

		private class OpenGLTexture : IGLTexture
		{
			public uint Handle
			{
				get;
				private set;
			}

			public GLenum Target
			{
				get;
				private set;
			}

			public bool HasMipmaps
			{
				get;
				private set;
			}

			public OpenGLTexture(
				uint handle,
				GLenum target,
				int levelCount
			) {
				Handle = handle;
				Target = target;
				HasMipmaps = levelCount > 1;
			}

			// We can't set a SamplerState Texture to null, so use this.
			private OpenGLTexture()
			{
				Handle = 0;
				Target = GLenum.GL_TEXTURE_2D; // FIXME: Assumption! -flibit
			}
			public static readonly OpenGLTexture NullTexture = new OpenGLTexture();
		}

		#endregion

		#region OpenGL Renderbuffer Container Class

		private class OpenGLRenderbuffer : IGLRenderbuffer
		{
			public uint Handle
			{
				get;
				private set;
			}

			public OpenGLRenderbuffer(uint handle)
			{
				Handle = handle;
			}
		}

		#endregion

		#region OpenGL Buffer Container Class

		private class OpenGLBuffer : IGLBuffer
		{
			public uint Handle
			{
				get;
				private set;
			}

			public IntPtr BufferSize
			{
				get;
				private set;
			}

			public GLenum Dynamic
			{
				get;
				private set;
			}

			public OpenGLBuffer(
				uint handle,
				IntPtr bufferSize,
				GLenum dynamic
			) {
				Handle = handle;
				BufferSize = bufferSize;
				Dynamic = dynamic;
			}

			private OpenGLBuffer()
			{
				Handle = 0;
			}
			public static readonly OpenGLBuffer NullBuffer = new OpenGLBuffer();
		}

		#endregion

		#region OpenGL Effect Container Class

		private class OpenGLEffect : IGLEffect
		{
			public IntPtr EffectData
			{
				get;
				private set;
			}

			public IntPtr GLEffectData
			{
				get;
				private set;
			}

			public OpenGLEffect(IntPtr effect, IntPtr glEffect)
			{
				EffectData = effect;
				GLEffectData = glEffect;
			}
		}

		#endregion

		#region OpenGL Query Container Class

		private class OpenGLQuery : IGLQuery
		{
			public uint Handle
			{
				get;
				private set;
			}

			public OpenGLQuery(uint handle)
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
					glBlendColor(
						blendColor.R / 255.0f,
						blendColor.G / 255.0f,
						blendColor.B / 255.0f,
						blendColor.A / 255.0f
					);
				}
			}
		}

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
					if (value == -1)
					{
						glDisable(GLenum.GL_SAMPLE_MASK);
					}
					else
					{
						if (multisampleMask == -1)
						{
							glEnable(GLenum.GL_SAMPLE_MASK);
						}
						// FIXME: index...? -flibit
						glSampleMaski(0, (uint) value);
					}
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
					if (separateStencilEnable)
					{
						glStencilFuncSeparate(
							GLenum.GL_FRONT,
							XNAToGL.CompareFunc[(int) stencilFunc],
							stencilRef,
							stencilMask
						);
						glStencilFuncSeparate(
							GLenum.GL_BACK,
							XNAToGL.CompareFunc[(int) ccwStencilFunc],
							stencilRef,
							stencilMask
						);
					}
					else
					{
						glStencilFunc(
							XNAToGL.CompareFunc[(int) stencilFunc],
							stencilRef,
							stencilMask
						);
					}
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

		/* These two aren't actually empty rects by default in OpenGL,
		 * but we don't _really_ know the starting window size, so
		 * force apply this when the GraphicsDevice is initialized.
		 * -flibit
		 */
		private Rectangle scissorRectangle =  new Rectangle(
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

		private OpenGLTexture[] Textures;
		private TextureAddressMode[] SamplersU;
		private TextureAddressMode[] SamplersV;
		private TextureAddressMode[] SamplersW;
		private TextureFilter[] SamplersFilter;
		private float[] SamplersAnisotropy;
		private int[] SamplersMaxLevel;
		private float[] SamplersLODBias;
		private bool[] SamplersMipped;

		private uint[] Samplers;

		#endregion

		#region Buffer Binding Cache Variables

		private uint currentVertexBuffer = 0;
		private uint currentIndexBuffer = 0;

		// ld, or LastDrawn, effect/vertex attributes
		private VertexDeclaration ldVertexDeclaration = null;
		private IntPtr ldPointer = IntPtr.Zero;
		private IntPtr ldEffect = IntPtr.Zero;
		private IntPtr ldTechnique = IntPtr.Zero;
		private uint ldPass = 0;

		#endregion

		#region Render Target Cache Variables

		private uint currentReadFramebuffer = 0;
		private uint currentDrawFramebuffer = 0;
		private uint targetFramebuffer = 0;
		private uint resolveFramebufferRead = 0;
		private uint resolveFramebufferDraw = 0;
		private readonly uint[] currentAttachments;
		private readonly GLenum[] currentAttachmentTypes;
		private int currentDrawBuffers;
		private readonly IntPtr drawBuffersArray;
		private uint currentRenderbuffer;
		private DepthFormat currentDepthStencilFormat;
		private readonly uint[] attachments;
		private readonly GLenum[] attachmentTypes;

		#endregion

		#region Clear Cache Variables

		private Vector4 currentClearColor = new Vector4(0, 0, 0, 0);
		private float currentClearDepth = 1.0f;
		private int currentClearStencil = 0;

		#endregion

		#region Private OpenGL Context Variable

		private IntPtr glContext;

		#endregion

		#region Faux-Backbuffer Variables

		public IGLBackbuffer Backbuffer
		{
			get;
			private set;
		}

		private GLenum backbufferScaleMode;

		#endregion

		#region OpenGL Device Capabilities

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

		#region Private Vertex Attribute Cache

		private class VertexAttribute
		{
			public uint CurrentBuffer;
			public IntPtr CurrentPointer;
			public VertexElementFormat CurrentFormat;
			public bool CurrentNormalized;
			public int CurrentStride;
			public VertexAttribute()
			{
				CurrentBuffer = 0;
				CurrentPointer = IntPtr.Zero;
				CurrentFormat = VertexElementFormat.Single;
				CurrentNormalized = false;
				CurrentStride = 0;
			}
		}
		private VertexAttribute[] attributes;
		private bool[] attributeEnabled;
		private bool[] previousAttributeEnabled;
		private int[] attributeDivisor;
		private int[] previousAttributeDivisor;

		#endregion

		#region Private MojoShader Interop

		private string shaderProfile;
		private IntPtr shaderContext;

		private IntPtr currentEffect = IntPtr.Zero;
		private IntPtr currentTechnique = IntPtr.Zero;
		private uint currentPass = 0;

		private int flipViewport = 1;

		private bool effectApplied = false;

		private static IntPtr glGetProcAddress(IntPtr name, IntPtr d)
		{
			return SDL.SDL_GL_GetProcAddress(name);
		}
		private static MojoShader.MOJOSHADER_glGetProcAddress GLGetProcAddress = glGetProcAddress;

		#endregion

		#region Private Graphics Object Disposal Queues

		private ConcurrentQueue<IGLTexture> GCTextures = new ConcurrentQueue<IGLTexture>();
		private ConcurrentQueue<IGLRenderbuffer> GCDepthBuffers = new ConcurrentQueue<IGLRenderbuffer>();
		private ConcurrentQueue<IGLBuffer> GCVertexBuffers = new ConcurrentQueue<IGLBuffer>();
		private ConcurrentQueue<IGLBuffer> GCIndexBuffers = new ConcurrentQueue<IGLBuffer>();
		private ConcurrentQueue<IGLEffect> GCEffects = new ConcurrentQueue<IGLEffect>();
		private ConcurrentQueue<IGLQuery> GCQueries = new ConcurrentQueue<IGLQuery>();

		#endregion

		#region Private Profile-specific Variables

		private bool useCoreProfile;
		private DepthFormat windowDepthFormat;
		private uint vao;

		#endregion

		#region memcpy Export

		/* This is used a lot for GetData/Read calls... -flibit */
		[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
		private static extern void memcpy(IntPtr dst, IntPtr src, IntPtr len);

		#endregion

		#region Public Constructor

		public ModernGLDevice(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter
		) {
			// Create OpenGL context
			glContext = SDL.SDL_GL_CreateContext(
				presentationParameters.DeviceWindowHandle
			);

			// Check for a possible Core context
			int flags;
			int coreFlag = (int) SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;
			SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, out flags);
			useCoreProfile = (flags & coreFlag) == coreFlag;

			// Check the window's depth/stencil format
			int depthSize, stencilSize;
			SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, out depthSize);
			SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, out stencilSize);
			if (depthSize == 0 && stencilSize == 0)
			{
				windowDepthFormat = DepthFormat.None;
			}
			else if (depthSize == 16 && stencilSize == 0)
			{
				windowDepthFormat = DepthFormat.Depth16;
			}
			else if (depthSize == 24 && stencilSize == 0)
			{
				windowDepthFormat = DepthFormat.Depth24;
			}
			else if (depthSize == 24 && stencilSize == 8)
			{
				windowDepthFormat = DepthFormat.Depth24Stencil8;
			}
			else
			{
				throw new NotSupportedException("Unrecognized window depth/stencil format!");
			}

			// Init threaded GL crap where applicable
			InitThreadedGL(
				presentationParameters.DeviceWindowHandle
			);

			// Initialize entry points
			LoadGLEntryPoints();

			shaderProfile = MojoShader.MOJOSHADER_glBestProfile(
				GLGetProcAddress,
				IntPtr.Zero,
				null,
				null,
				IntPtr.Zero
			);
			shaderContext = MojoShader.MOJOSHADER_glCreateContext(
				shaderProfile,
				GLGetProcAddress,
				IntPtr.Zero,
				null,
				null,
				IntPtr.Zero
			);
			MojoShader.MOJOSHADER_glMakeContextCurrent(shaderContext);

			// Some users might want pixely upscaling...
			backbufferScaleMode = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_BACKBUFFER_SCALE_NEAREST"
			) == "1" ? GLenum.GL_NEAREST : GLenum.GL_LINEAR;

			// Print GL information
			FNALoggerEXT.LogInfo("IGLDevice: ModernGLDevice");
			FNALoggerEXT.LogInfo("OpenGL Device: " + glGetString(GLenum.GL_RENDERER));
			FNALoggerEXT.LogInfo("OpenGL Driver: " + glGetString(GLenum.GL_VERSION));
			FNALoggerEXT.LogInfo("OpenGL Vendor: " + glGetString(GLenum.GL_VENDOR));
			FNALoggerEXT.LogInfo("MojoShader Profile: " + shaderProfile);

			// Load the extension list, initialize extension-dependent components
			string extensions;
			if (useCoreProfile)
			{
				extensions = string.Empty;
				int numExtensions;
				glGetIntegerv(GLenum.GL_NUM_EXTENSIONS, out numExtensions);
				for (uint i = 0; i < numExtensions; i += 1)
				{
					extensions += glGetStringi(GLenum.GL_EXTENSIONS, i) + " ";
				}
			}
			else
			{
				extensions = glGetString(GLenum.GL_EXTENSIONS);
			}
			SupportsS3tc = (
				extensions.Contains("GL_EXT_texture_compression_s3tc") ||
				extensions.Contains("GL_OES_texture_compression_S3TC") ||
				extensions.Contains("GL_EXT_texture_compression_dxt3") ||
				extensions.Contains("GL_EXT_texture_compression_dxt5")
			);
			SupportsDxt1 = (
				SupportsS3tc ||
				extensions.Contains("GL_EXT_texture_compression_dxt1")
			);
			SupportsHardwareInstancing = true;

			/* Check the max multisample count, override parameters if necessary */
			int maxSamples = 0;
			glGetIntegerv(GLenum.GL_MAX_SAMPLES, out maxSamples);
			MaxMultiSampleCount = maxSamples;
			presentationParameters.MultiSampleCount = Math.Min(
				presentationParameters.MultiSampleCount,
				MaxMultiSampleCount
			);

			// Initialize the faux-backbuffer
			if (UseFauxBackbuffer(presentationParameters, adapter.CurrentDisplayMode))
			{
				Backbuffer = new OpenGLBackbuffer(
					this,
					presentationParameters.BackBufferWidth,
					presentationParameters.BackBufferHeight,
					presentationParameters.DepthStencilFormat,
					presentationParameters.MultiSampleCount
				);
			}
			else
			{
				Backbuffer = new NullBackbuffer(
					presentationParameters.BackBufferWidth,
					presentationParameters.BackBufferHeight,
					windowDepthFormat
				);
			}

			// Initialize texture collection array
			int numSamplers;
			glGetIntegerv(GLenum.GL_MAX_TEXTURE_IMAGE_UNITS, out numSamplers);
			numSamplers = Math.Min(
				numSamplers,
				GraphicsDevice.MAX_TEXTURE_SAMPLERS + GraphicsDevice.MAX_VERTEXTEXTURE_SAMPLERS
			);
			Textures = new OpenGLTexture[numSamplers];
			Samplers = new uint[numSamplers];
			SamplersU = new TextureAddressMode[numSamplers];
			SamplersV = new TextureAddressMode[numSamplers];
			SamplersW = new TextureAddressMode[numSamplers];
			SamplersFilter = new TextureFilter[numSamplers];
			SamplersAnisotropy = new float[numSamplers];
			SamplersMaxLevel = new int[numSamplers];
			SamplersLODBias = new float[numSamplers];
			SamplersMipped = new bool[numSamplers];
			GCHandle smpHandle = GCHandle.Alloc(Samplers, GCHandleType.Pinned);
			glCreateSamplers(numSamplers, smpHandle.AddrOfPinnedObject());
			smpHandle.Free();
			for (int i = 0; i < numSamplers; i += 1)
			{
				Textures[i] = OpenGLTexture.NullTexture;
				SamplersU[i] = TextureAddressMode.Wrap;
				SamplersV[i] = TextureAddressMode.Wrap;
				SamplersW[i] = TextureAddressMode.Wrap;
				SamplersFilter[i] = TextureFilter.Linear;
				SamplersAnisotropy[i] = 4.0f;
				SamplersMaxLevel[i] = 0;
				SamplersLODBias[i] = 0.0f;
				SamplersMipped[i] = false;
				glBindSampler(i, Samplers[i]);
			}
			MaxTextureSlots = numSamplers;

			// Initialize vertex attribute state arrays
			int numAttributes;
			glGetIntegerv(GLenum.GL_MAX_VERTEX_ATTRIBS, out numAttributes);
			numAttributes = Math.Min(
				numAttributes,
				GraphicsDevice.MAX_VERTEX_ATTRIBUTES
			);
			attributes = new VertexAttribute[numAttributes];
			attributeEnabled = new bool[numAttributes];
			previousAttributeEnabled = new bool[numAttributes];
			attributeDivisor = new int[numAttributes];
			previousAttributeDivisor = new int[numAttributes];
			for (int i = 0; i < numAttributes; i += 1)
			{
				attributes[i] = new VertexAttribute();
				attributeEnabled[i] = false;
				previousAttributeEnabled[i] = false;
				attributeDivisor[i] = 0;
				previousAttributeDivisor[i] = 0;
			}

			// Initialize render target FBO and state arrays
			int numAttachments;
			glGetIntegerv(GLenum.GL_MAX_DRAW_BUFFERS, out numAttachments);
			numAttachments = Math.Min(
				numAttachments,
				GraphicsDevice.MAX_RENDERTARGET_BINDINGS
			);
			attachments = new uint[numAttachments];
			attachmentTypes = new GLenum[numAttachments];
			currentAttachments = new uint[numAttachments];
			currentAttachmentTypes = new GLenum[numAttachments];
			drawBuffersArray = Marshal.AllocHGlobal(sizeof(GLenum) * numAttachments);
			unsafe
			{
				GLenum* dba = (GLenum*) drawBuffersArray;
				for (int i = 0; i < numAttachments; i += 1)
				{
					currentAttachments[i] = 0;
					currentAttachmentTypes[i] = GLenum.GL_TEXTURE_2D;
					dba[i] = GLenum.GL_COLOR_ATTACHMENT0 + i;
				}
			}
			currentDrawBuffers = 0;
			currentRenderbuffer = 0;
			currentDepthStencilFormat = DepthFormat.None;
			glCreateFramebuffers(1, out targetFramebuffer);
			glCreateFramebuffers(1, out resolveFramebufferRead);
			glCreateFramebuffers(1, out resolveFramebufferDraw);

			// Generate and bind a VAO, to shut Core up
			if (useCoreProfile)
			{
				glGenVertexArrays(1, out vao);
				glBindVertexArray(vao);
			}
		}

		#endregion

		#region Dispose Method

		public void Dispose()
		{
			if (useCoreProfile)
			{
				glBindVertexArray(0);
				glDeleteVertexArrays(1, ref vao);
			}
			glDeleteFramebuffers(1, ref resolveFramebufferRead);
			resolveFramebufferRead = 0;
			glDeleteFramebuffers(1, ref resolveFramebufferDraw);
			resolveFramebufferDraw = 0;
			glDeleteFramebuffers(1, ref targetFramebuffer);
			targetFramebuffer = 0;
			if (Backbuffer is OpenGLBackbuffer)
			{
				(Backbuffer as OpenGLBackbuffer).Dispose();
			}
			Backbuffer = null;
			Marshal.FreeHGlobal(drawBuffersArray);
			MojoShader.MOJOSHADER_glMakeContextCurrent(IntPtr.Zero);
			MojoShader.MOJOSHADER_glDestroyContext(shaderContext);

#if THREADED_GL
			SDL.SDL_GL_DeleteContext(BackgroundContext.context);
#endif
			SDL.SDL_GL_DeleteContext(glContext);
		}

		#endregion

		#region Window Backbuffer Reset Method

		public void ResetBackbuffer(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter,
			bool renderTargetBound
		) {
			if (UseFauxBackbuffer(presentationParameters, adapter.CurrentDisplayMode))
			{
				if (Backbuffer is NullBackbuffer)
				{
					Backbuffer = new OpenGLBackbuffer(
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
			else
			{
				if (Backbuffer is OpenGLBackbuffer)
				{
					(Backbuffer as OpenGLBackbuffer).Dispose();
					Backbuffer = new NullBackbuffer(
						presentationParameters.BackBufferWidth,
						presentationParameters.BackBufferHeight,
						windowDepthFormat
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
		}

		#endregion

		#region Window SwapBuffers Method

		public void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			/* Only the faux-backbuffer supports presenting
			 * specific regions given to Present().
			 * -flibit
			 */
			if (Backbuffer is OpenGLBackbuffer)
			{
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
					SDL.SDL_GL_GetDrawableSize(
						overrideWindowHandle,
						out dstW,
						out dstH
					);
				}

				if (scissorTestEnable)
				{
					glDisable(GLenum.GL_SCISSOR_TEST);
				}

				uint finalBuffer;
				if (	Backbuffer.MultiSampleCount > 0 &&
					(srcX != dstX || srcY != dstY || srcW != dstW || srcH != dstH)	)
				{
					/* We have to resolve the renderbuffer to a texture first.
					 * For whatever reason, we can't blit a multisample renderbuffer
					 * to the backbuffer. Not sure why, but oh well.
					 * -flibit
					 */
					OpenGLBackbuffer glBack = Backbuffer as OpenGLBackbuffer;
					if (glBack.Texture == 0)
					{
						glCreateTextures(GLenum.GL_TEXTURE_2D, 1, out glBack.Texture);
						glTextureStorage2D(
							glBack.Texture,
							1,
							GLenum.GL_RGBA,
							glBack.Width,
							glBack.Height
						);
					}
					glNamedFramebufferTexture(
						glBack.Handle,
						GLenum.GL_COLOR_ATTACHMENT0,
						glBack.Texture,
						0
					);
					glBlitNamedFramebuffer(
						glBack.Handle,
						resolveFramebufferDraw,
						0, 0, glBack.Width, glBack.Height,
						0, 0, glBack.Width, glBack.Height,
						GLenum.GL_COLOR_BUFFER_BIT,
						GLenum.GL_LINEAR
					);
					finalBuffer = resolveFramebufferDraw;
				}
				else
				{
					finalBuffer = (Backbuffer as OpenGLBackbuffer).Handle;
				}

				glBlitNamedFramebuffer(
					finalBuffer,
					0,
					srcX, srcY, srcW, srcH,
					dstX, dstY, dstW, dstH,
					GLenum.GL_COLOR_BUFFER_BIT,
					backbufferScaleMode
				);

				if (scissorTestEnable)
				{
					glEnable(GLenum.GL_SCISSOR_TEST);
				}

				SDL.SDL_GL_SwapWindow(
					overrideWindowHandle
				);
			}
			else
			{
				// Nothing left to do, just swap!
				SDL.SDL_GL_SwapWindow(
					overrideWindowHandle
				);
			}

#if !DISABLE_THREADING && !THREADED_GL
			RunActions();
#endif
			IGLTexture gcTexture;
			while (GCTextures.TryDequeue(out gcTexture))
			{
				DeleteTexture(gcTexture);
			}
			IGLRenderbuffer gcDepthBuffer;
			while (GCDepthBuffers.TryDequeue(out gcDepthBuffer))
			{
				DeleteRenderbuffer(gcDepthBuffer);
			}
			IGLBuffer gcBuffer;
			while (GCVertexBuffers.TryDequeue(out gcBuffer))
			{
				DeleteVertexBuffer(gcBuffer);
			}
			while (GCIndexBuffers.TryDequeue(out gcBuffer))
			{
				DeleteIndexBuffer(gcBuffer);
			}
			IGLEffect gcEffect;
			while (GCEffects.TryDequeue(out gcEffect))
			{
				DeleteEffect(gcEffect);
			}
			IGLQuery gcQuery;
			while (GCQueries.TryDequeue(out gcQuery))
			{
				DeleteQuery(gcQuery);
			}
		}

		#endregion

		#region GL Object Disposal Wrappers

		public void AddDisposeTexture(IGLTexture texture)
		{
			if (IsOnMainThread())
			{
				DeleteTexture(texture);
			}
			else
			{
				GCTextures.Enqueue(texture);
			}
		}

		public void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			if (IsOnMainThread())
			{
				DeleteRenderbuffer(renderbuffer);
			}
			else
			{
				GCDepthBuffers.Enqueue(renderbuffer);
			}
		}

		public void AddDisposeVertexBuffer(IGLBuffer buffer)
		{
			if (IsOnMainThread())
			{
				DeleteVertexBuffer(buffer);
			}
			else
			{
				GCVertexBuffers.Enqueue(buffer);
			}
		}

		public void AddDisposeIndexBuffer(IGLBuffer buffer)
		{
			if (IsOnMainThread())
			{
				DeleteIndexBuffer(buffer);
			}
			else
			{
				GCIndexBuffers.Enqueue(buffer);
			}
		}

		public void AddDisposeEffect(IGLEffect effect)
		{
			if (IsOnMainThread())
			{
				DeleteEffect(effect);
			}
			else
			{
				GCEffects.Enqueue(effect);
			}
		}

		public void AddDisposeQuery(IGLQuery query)
		{
			if (IsOnMainThread())
			{
				DeleteQuery(query);
			}
			else
			{
				GCQueries.Enqueue(query);
			}
		}

		#endregion

		#region String Marker Method

		public void SetStringMarker(string text)
		{
#if DEBUG
			IntPtr chars = Marshal.StringToHGlobalAnsi(text);
			glStringMarkerGREMEDY(text.Length, chars);
			Marshal.FreeHGlobal(chars);
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
			IndexBuffer indices
		) {
			// Bind the index buffer
			BindIndexBuffer(indices.buffer);

			// Draw!
			glDrawRangeElementsBaseVertex(
				XNAToGL.Primitive[(int) primitiveType],
				minVertexIndex,
				minVertexIndex + numVertices - 1,
				XNAToGL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToGL.IndexType[(int) indices.IndexElementSize],
				(IntPtr) (startIndex * XNAToGL.IndexSize[(int) indices.IndexElementSize]),
				baseVertex
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
			// Note that minVertexIndex and numVertices are NOT used!

			// Bind the index buffer
			BindIndexBuffer(indices.buffer);

			// Draw!
			glDrawElementsInstancedBaseVertex(
				XNAToGL.Primitive[(int) primitiveType],
				XNAToGL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToGL.IndexType[(int) indices.IndexElementSize],
				(IntPtr) (startIndex * XNAToGL.IndexSize[(int) indices.IndexElementSize]),
				instanceCount,
				baseVertex
			);
		}

		public void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		) {
			// Draw!
			glDrawArrays(
				XNAToGL.Primitive[(int) primitiveType],
				vertexStart,
				XNAToGL.PrimitiveVerts(primitiveType, primitiveCount)
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
			// Unbind current index buffer.
			BindIndexBuffer(OpenGLBuffer.NullBuffer);

			// Draw!
			glDrawRangeElements(
				XNAToGL.Primitive[(int) primitiveType],
				0,
				numVertices - 1,
				XNAToGL.PrimitiveVerts(primitiveType, primitiveCount),
				XNAToGL.IndexType[(int) indexElementSize],
				(IntPtr) (
					indexData.ToInt64() +
					(indexOffset * XNAToGL.IndexSize[(int) indexElementSize])
				)
			);
		}

		public void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		) {
			// Draw!
			glDrawArrays(
				XNAToGL.Primitive[(int) primitiveType],
				vertexOffset,
				XNAToGL.PrimitiveVerts(primitiveType, primitiveCount)
			);
		}

		#endregion

		#region State Management Methods

		public void SetViewport(Viewport vp, bool renderTargetBound)
		{
			// Flip viewport when target is not bound
			if (!renderTargetBound)
			{
				vp.Y = Backbuffer.Height - vp.Y - vp.Height;
			}

			if (vp.Bounds != viewport)
			{
				viewport = vp.Bounds;
				glViewport(
					viewport.X,
					viewport.Y,
					viewport.Width,
					viewport.Height
				);
			}

			if (vp.MinDepth != depthRangeMin || vp.MaxDepth != depthRangeMax)
			{
				depthRangeMin = vp.MinDepth;
				depthRangeMax = vp.MaxDepth;
				glDepthRange((double) depthRangeMin, (double) depthRangeMax);
			}
		}

		public void SetScissorRect(
			Rectangle scissorRect,
			bool renderTargetBound
		) {
			// Flip rectangle when target is not bound
			if (!renderTargetBound)
			{
				scissorRect.Y = Backbuffer.Height - scissorRect.Y - scissorRect.Height;
			}

			if (scissorRect != scissorRectangle)
			{
				scissorRectangle = scissorRect;
				glScissor(
					scissorRectangle.X,
					scissorRectangle.Y,
					scissorRectangle.Width,
					scissorRectangle.Height
				);
			}
		}

		public void SetBlendState(BlendState blendState)
		{
			bool newEnable = (
				!(	blendState.ColorSourceBlend == Blend.One &&
					blendState.ColorDestinationBlend == Blend.Zero &&
					blendState.AlphaSourceBlend == Blend.One &&
					blendState.AlphaDestinationBlend == Blend.Zero	)
			);
			if (newEnable != alphaBlendEnable)
			{
				alphaBlendEnable = newEnable;
				ToggleGLState(GLenum.GL_BLEND, alphaBlendEnable);
			}

			if (alphaBlendEnable)
			{
				if (blendState.BlendFactor != blendColor)
				{
					blendColor = blendState.BlendFactor;
					glBlendColor(
						blendColor.R / 255.0f,
						blendColor.G / 255.0f,
						blendColor.B / 255.0f,
						blendColor.A / 255.0f
					);
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
					glBlendFuncSeparate(
						XNAToGL.BlendMode[(int) srcBlend],
						XNAToGL.BlendMode[(int) dstBlend],
						XNAToGL.BlendMode[(int) srcBlendAlpha],
						XNAToGL.BlendMode[(int) dstBlendAlpha]
					);
				}

				if (	blendState.ColorBlendFunction != blendOp ||
					blendState.AlphaBlendFunction != blendOpAlpha	)
				{
					blendOp = blendState.ColorBlendFunction;
					blendOpAlpha = blendState.AlphaBlendFunction;
					glBlendEquationSeparate(
						XNAToGL.BlendEquation[(int) blendOp],
						XNAToGL.BlendEquation[(int) blendOpAlpha]
					);
				}
			}

			if (blendState.ColorWriteChannels != colorWriteEnable)
			{
				colorWriteEnable = blendState.ColorWriteChannels;
				glColorMask(
					(colorWriteEnable & ColorWriteChannels.Red) != 0,
					(colorWriteEnable & ColorWriteChannels.Green) != 0,
					(colorWriteEnable & ColorWriteChannels.Blue) != 0,
					(colorWriteEnable & ColorWriteChannels.Alpha) != 0
				);
			}
			/* FIXME: So how exactly do we factor in
			 * COLORWRITEENABLE for buffer 0? Do we just assume that
			 * the default is just buffer 0, and all other calls
			 * update the other write masks afterward? Or do we
			 * assume that COLORWRITEENABLE only touches 0, and the
			 * other 3 buffers are left alone unless we don't have
			 * EXT_draw_buffers2?
			 * -flibit
			 */
			if (blendState.ColorWriteChannels1 != colorWriteEnable1)
			{
				colorWriteEnable1 = blendState.ColorWriteChannels1;
				glColorMaski(
					1,
					(colorWriteEnable1 & ColorWriteChannels.Red) != 0,
					(colorWriteEnable1 & ColorWriteChannels.Green) != 0,
					(colorWriteEnable1 & ColorWriteChannels.Blue) != 0,
					(colorWriteEnable1 & ColorWriteChannels.Alpha) != 0
				);
			}
			if (blendState.ColorWriteChannels2 != colorWriteEnable2)
			{
				colorWriteEnable2 = blendState.ColorWriteChannels2;
				glColorMaski(
					2,
					(colorWriteEnable2 & ColorWriteChannels.Red) != 0,
					(colorWriteEnable2 & ColorWriteChannels.Green) != 0,
					(colorWriteEnable2 & ColorWriteChannels.Blue) != 0,
					(colorWriteEnable2 & ColorWriteChannels.Alpha) != 0
				);
			}
			if (blendState.ColorWriteChannels3 != colorWriteEnable3)
			{
				colorWriteEnable3 = blendState.ColorWriteChannels3;
				glColorMaski(
					3,
					(colorWriteEnable3 & ColorWriteChannels.Red) != 0,
					(colorWriteEnable3 & ColorWriteChannels.Green) != 0,
					(colorWriteEnable3 & ColorWriteChannels.Blue) != 0,
					(colorWriteEnable3 & ColorWriteChannels.Alpha) != 0
				);
			}

			if (blendState.MultiSampleMask != multisampleMask)
			{
				if (blendState.MultiSampleMask == -1)
				{
					glDisable(GLenum.GL_SAMPLE_MASK);
				}
				else
				{
					if (multisampleMask == -1)
					{
						glEnable(GLenum.GL_SAMPLE_MASK);
					}
					// FIXME: index...? -flibit
					glSampleMaski(0, (uint) blendState.MultiSampleMask);
				}
				multisampleMask = blendState.MultiSampleMask;
			}
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			if (depthStencilState.DepthBufferEnable != zEnable)
			{
				zEnable = depthStencilState.DepthBufferEnable;
				ToggleGLState(GLenum.GL_DEPTH_TEST, zEnable);
			}

			if (zEnable)
			{
				if (depthStencilState.DepthBufferWriteEnable != zWriteEnable)
				{
					zWriteEnable = depthStencilState.DepthBufferWriteEnable;
					glDepthMask(zWriteEnable);
				}

				if (depthStencilState.DepthBufferFunction != depthFunc)
				{
					depthFunc = depthStencilState.DepthBufferFunction;
					glDepthFunc(XNAToGL.CompareFunc[(int) depthFunc]);
				}
			}

			if (depthStencilState.StencilEnable != stencilEnable)
			{
				stencilEnable = depthStencilState.StencilEnable;
				ToggleGLState(GLenum.GL_STENCIL_TEST, stencilEnable);
			}

			if (stencilEnable)
			{
				if (depthStencilState.StencilWriteMask != stencilWriteMask)
				{
					stencilWriteMask = depthStencilState.StencilWriteMask;
					glStencilMask(stencilWriteMask);
				}

				// TODO: Can we split StencilFunc/StencilOp up nicely? -flibit
				if (	depthStencilState.TwoSidedStencilMode != separateStencilEnable ||
					depthStencilState.ReferenceStencil != stencilRef ||
					depthStencilState.StencilMask != stencilMask ||
					depthStencilState.StencilFunction != stencilFunc ||
					depthStencilState.CounterClockwiseStencilFunction != ccwStencilFunc ||
					depthStencilState.StencilFail != stencilFail ||
					depthStencilState.StencilDepthBufferFail != stencilZFail ||
					depthStencilState.StencilPass != stencilPass ||
					depthStencilState.CounterClockwiseStencilFail != ccwStencilFail ||
					depthStencilState.CounterClockwiseStencilDepthBufferFail != ccwStencilZFail ||
					depthStencilState.CounterClockwiseStencilPass != ccwStencilPass	)
				{
					separateStencilEnable = depthStencilState.TwoSidedStencilMode;
					stencilRef = depthStencilState.ReferenceStencil;
					stencilMask = depthStencilState.StencilMask;
					stencilFunc = depthStencilState.StencilFunction;
					stencilFail = depthStencilState.StencilFail;
					stencilZFail = depthStencilState.StencilDepthBufferFail;
					stencilPass = depthStencilState.StencilPass;
					if (separateStencilEnable)
					{
						ccwStencilFunc = depthStencilState.CounterClockwiseStencilFunction;
						ccwStencilFail = depthStencilState.CounterClockwiseStencilFail;
						ccwStencilZFail = depthStencilState.CounterClockwiseStencilDepthBufferFail;
						ccwStencilPass = depthStencilState.CounterClockwiseStencilPass;
						glStencilFuncSeparate(
							GLenum.GL_FRONT,
							XNAToGL.CompareFunc[(int) stencilFunc],
							stencilRef,
							stencilMask
						);
						glStencilFuncSeparate(
							GLenum.GL_BACK,
							XNAToGL.CompareFunc[(int) ccwStencilFunc],
							stencilRef,
							stencilMask
						);
						glStencilOpSeparate(
							GLenum.GL_FRONT,
							XNAToGL.GLStencilOp[(int) stencilFail],
							XNAToGL.GLStencilOp[(int) stencilZFail],
							XNAToGL.GLStencilOp[(int) stencilPass]
						);
						glStencilOpSeparate(
							GLenum.GL_BACK,
							XNAToGL.GLStencilOp[(int) ccwStencilFail],
							XNAToGL.GLStencilOp[(int) ccwStencilZFail],
							XNAToGL.GLStencilOp[(int) ccwStencilPass]
						);
					}
					else
					{
						glStencilFunc(
							XNAToGL.CompareFunc[(int) stencilFunc],
							stencilRef,
							stencilMask
						);
						glStencilOp(
							XNAToGL.GLStencilOp[(int) stencilFail],
							XNAToGL.GLStencilOp[(int) stencilZFail],
							XNAToGL.GLStencilOp[(int) stencilPass]
						);
					}
				}
			}
		}

		public void ApplyRasterizerState(
			RasterizerState rasterizerState,
			bool renderTargetBound
		) {
			if (rasterizerState.ScissorTestEnable != scissorTestEnable)
			{
				scissorTestEnable = rasterizerState.ScissorTestEnable;
				ToggleGLState(GLenum.GL_SCISSOR_TEST, scissorTestEnable);
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
				if ((actualMode == CullMode.None) != (cullFrontFace == CullMode.None))
				{
					ToggleGLState(GLenum.GL_CULL_FACE, actualMode != CullMode.None);
				}
				cullFrontFace = actualMode;
				if (cullFrontFace != CullMode.None)
				{
					glFrontFace(XNAToGL.FrontFace[(int) cullFrontFace]);
				}
			}

			if (rasterizerState.FillMode != fillMode)
			{
				fillMode = rasterizerState.FillMode;
				glPolygonMode(
					GLenum.GL_FRONT_AND_BACK,
					XNAToGL.GLFillMode[(int) fillMode]
				);
			}

			// FIXME: Floating point equality comparisons used for speed -flibit
			float realDepthBias = rasterizerState.DepthBias * XNAToGL.DepthBiasScale[
				renderTargetBound ?
					(int) currentDepthStencilFormat :
					(int) Backbuffer.DepthFormat
			];
			if (	realDepthBias != depthBias ||
				rasterizerState.SlopeScaleDepthBias != slopeScaleDepthBias	)
			{
				if (	realDepthBias == 0.0f &&
					rasterizerState.SlopeScaleDepthBias == 0.0f)
				{
					// We're changing to disabled bias, disable!
					glDisable(GLenum.GL_POLYGON_OFFSET_FILL);
				}
				else
				{
					if (depthBias == 0.0f && slopeScaleDepthBias == 0.0f)
					{
						// We're changing away from disabled bias, enable!
						glEnable(GLenum.GL_POLYGON_OFFSET_FILL);
					}
					glPolygonOffset(
						rasterizerState.SlopeScaleDepthBias,
						realDepthBias
					);
				}
				depthBias = realDepthBias;
				slopeScaleDepthBias = rasterizerState.SlopeScaleDepthBias;
			}

			/* If you're reading this, you have a user with broken MSAA!
			 * Here's the deal: On all modern drivers this should work,
			 * but there was a period of time where, for some reason,
			 * IHVs all took a nap and decided that they didn't have to
			 * respect GL_MULTISAMPLE toggles. A couple sources:
			 *
			 * https://developer.apple.com/library/content/documentation/GraphicsImaging/Conceptual/OpenGL-MacProgGuide/opengl_fsaa/opengl_fsaa.html
			 *
			 * https://www.opengl.org/discussion_boards/showthread.php/172025-glDisable(GL_MULTISAMPLE)-has-no-effect
			 *
			 * So yeah. Have em update their driver. If they're on Intel,
			 * tell them to install Linux. Yes, really.
			 * -flibit
			 */
			if (rasterizerState.MultiSampleAntiAlias != multiSampleEnable)
			{
				multiSampleEnable = rasterizerState.MultiSampleAntiAlias;
				ToggleGLState(GLenum.GL_MULTISAMPLE, multiSampleEnable);
			}
		}

		public void VerifySampler(int index, Texture texture, SamplerState sampler)
		{
			if (texture == null)
			{
				if (Textures[index] != OpenGLTexture.NullTexture)
				{
					glBindTextureUnit(index, 0);
					Textures[index] = OpenGLTexture.NullTexture;
				}
				return;
			}

			OpenGLTexture tex = texture.texture as OpenGLTexture;

			// Bind the correct texture
			if (tex != Textures[index])
			{
				if (tex.Target != Textures[index].Target)
				{
					// If we're changing targets, unbind the old texture first!
					glBindTextureUnit(index, 0);
				}
				glBindTextureUnit(index, tex.Handle);
				Textures[index] = tex;
			}

			// Apply the sampler states
			uint slot = Samplers[index];

			if (sampler.AddressU != SamplersU[index])
			{
				SamplersU[index] = sampler.AddressU;
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_WRAP_S,
					XNAToGL.Wrap[(int) sampler.AddressU]
				);
			}
			if (sampler.AddressV != SamplersV[index])
			{
				SamplersV[index] = sampler.AddressV;
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_WRAP_T,
					XNAToGL.Wrap[(int) sampler.AddressV]
				);
			}
			if (sampler.AddressW != SamplersW[index])
			{
				SamplersW[index] = sampler.AddressW;
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_WRAP_R,
					XNAToGL.Wrap[(int) sampler.AddressW]
				);
			}
			if (	sampler.Filter != SamplersFilter[index] ||
				sampler.MaxAnisotropy != SamplersAnisotropy[index] ||
				tex.HasMipmaps != SamplersMipped[index]	)
			{
				SamplersFilter[index] = sampler.Filter;
				SamplersAnisotropy[index] = sampler.MaxAnisotropy;
				SamplersMipped[index] = tex.HasMipmaps;
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_MAG_FILTER,
					XNAToGL.MagFilter[(int) sampler.Filter]
				);
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_MIN_FILTER,
					tex.HasMipmaps ?
						XNAToGL.MinMipFilter[(int) sampler.Filter] :
						XNAToGL.MinFilter[(int) sampler.Filter]
				);
				glSamplerParameterf(
					slot,
					GLenum.GL_TEXTURE_MAX_ANISOTROPY_EXT,
					(sampler.Filter == TextureFilter.Anisotropic) ?
						Math.Max(SamplersAnisotropy[index], 1.0f) :
						1.0f
				);
			}
			if (sampler.MaxMipLevel != SamplersMaxLevel[index])
			{
				SamplersMaxLevel[index] = sampler.MaxMipLevel;
				glSamplerParameteri(
					slot,
					GLenum.GL_TEXTURE_BASE_LEVEL,
					sampler.MaxMipLevel
				);
			}
			if (sampler.MipMapLevelOfDetailBias != SamplersLODBias[index])
			{
				SamplersLODBias[index] = sampler.MipMapLevelOfDetailBias;
				glSamplerParameterf(
					slot,
					GLenum.GL_TEXTURE_LOD_BIAS,
					sampler.MipMapLevelOfDetailBias
				);
			}
		}

		#endregion

		#region Effect Methods

		public IGLEffect CreateEffect(byte[] effectCode)
		{
			IntPtr effect = IntPtr.Zero;
			IntPtr glEffect = IntPtr.Zero;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			effect = MojoShader.MOJOSHADER_parseEffect(
				shaderProfile,
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

			glEffect = MojoShader.MOJOSHADER_glCompileEffect(effect);
			if (glEffect == IntPtr.Zero)
			{
				throw new InvalidOperationException(
					MojoShader.MOJOSHADER_glGetError()
				);
			}

#if !DISABLE_THREADING
			});
#endif

			return new OpenGLEffect(effect, glEffect);
		}

		private void DeleteEffect(IGLEffect effect)
		{
			IntPtr glEffectData = (effect as OpenGLEffect).GLEffectData;
			if (glEffectData == currentEffect)
			{
				MojoShader.MOJOSHADER_glEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_glEffectEnd(currentEffect);
				currentEffect = IntPtr.Zero;
				currentTechnique = IntPtr.Zero;
				currentPass = 0;
			}
			MojoShader.MOJOSHADER_glDeleteEffect(glEffectData);
			MojoShader.MOJOSHADER_freeEffect(effect.EffectData);
		}

		public IGLEffect CloneEffect(IGLEffect cloneSource)
		{
			IntPtr effect = IntPtr.Zero;
			IntPtr glEffect = IntPtr.Zero;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			effect = MojoShader.MOJOSHADER_cloneEffect(cloneSource.EffectData);
			glEffect = MojoShader.MOJOSHADER_glCompileEffect(effect);
			if (glEffect == IntPtr.Zero)
			{
				throw new InvalidOperationException(
					MojoShader.MOJOSHADER_glGetError()
				);
			}

#if !DISABLE_THREADING
			});
#endif

			return new OpenGLEffect(effect, glEffect);
		}

		public void ApplyEffect(
			IGLEffect effect,
			IntPtr technique,
			uint pass,
			IntPtr stateChanges
		) {
			effectApplied = true;
			IntPtr glEffectData = (effect as OpenGLEffect).GLEffectData;
			if (glEffectData == currentEffect)
			{
				if (technique == currentTechnique && pass == currentPass)
				{
					MojoShader.MOJOSHADER_glEffectCommitChanges(currentEffect);
					return;
				}
				MojoShader.MOJOSHADER_glEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_glEffectBeginPass(currentEffect, pass);
				currentTechnique = technique;
				currentPass = pass;
				return;
			}
			else if (currentEffect != IntPtr.Zero)
			{
				MojoShader.MOJOSHADER_glEffectEndPass(currentEffect);
				MojoShader.MOJOSHADER_glEffectEnd(currentEffect);
			}
			uint whatever;
			MojoShader.MOJOSHADER_glEffectBegin(
				glEffectData,
				out whatever,
				0,
				stateChanges
			);
			MojoShader.MOJOSHADER_glEffectBeginPass(
				glEffectData,
				pass
			);
			currentEffect = glEffectData;
			currentTechnique = technique;
			currentPass = pass;
		}

		public void BeginPassRestore(IGLEffect effect, IntPtr stateChanges)
		{
			IntPtr glEffectData = (effect as OpenGLEffect).GLEffectData;
			uint whatever;
			MojoShader.MOJOSHADER_glEffectBegin(
				glEffectData,
				out whatever,
				1,
				stateChanges
			);
			MojoShader.MOJOSHADER_glEffectBeginPass(
				glEffectData,
				0
			);
			effectApplied = true;
		}

		public void EndPassRestore(IGLEffect effect)
		{
			IntPtr glEffectData = (effect as OpenGLEffect).GLEffectData;
			MojoShader.MOJOSHADER_glEffectEndPass(glEffectData);
			MojoShader.MOJOSHADER_glEffectEnd(glEffectData);
			effectApplied = true;
		}

		#endregion

		#region glVertexAttribPointer/glVertexAttribDivisor Methods

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
				/* There's this weird case where you can have multiple vertbuffers,
				 * but they will have overlapping attributes. It seems like the
				 * first buffer gets priority, so start with the last one so the
				 * first buffer's attributes are what's bound at the end.
				 * -flibit
				 */
				for (int i = numBindings - 1; i >= 0; i -= 1)
				{
					BindVertexBuffer(bindings[i].VertexBuffer.buffer);
					VertexDeclaration vertexDeclaration = bindings[i].VertexBuffer.VertexDeclaration;
					IntPtr basePtr = (IntPtr) (
						vertexDeclaration.VertexStride *
						(bindings[i].VertexOffset)
					);
					foreach (VertexElement element in vertexDeclaration.elements)
					{
						int attribLoc = MojoShader.MOJOSHADER_glGetVertexAttribLocation(
							XNAToGL.VertexAttribUsage[(int) element.VertexElementUsage],
							element.UsageIndex
						);
						if (attribLoc == -1)
						{
							// Stream not in use!
							continue;
						}
						attributeEnabled[attribLoc] = true;
						VertexAttribute attr = attributes[attribLoc];
						uint buffer = (bindings[i].VertexBuffer.buffer as OpenGLBuffer).Handle;
						IntPtr ptr = basePtr + element.Offset;
						VertexElementFormat format = element.VertexElementFormat;
						bool normalized = XNAToGL.VertexAttribNormalized(element);
						if (	attr.CurrentBuffer != buffer ||
							attr.CurrentPointer != ptr ||
							attr.CurrentFormat != element.VertexElementFormat ||
							attr.CurrentNormalized != normalized ||
							attr.CurrentStride != vertexDeclaration.VertexStride	)
						{
							glVertexAttribPointer(
								attribLoc,
								XNAToGL.VertexAttribSize[(int) format],
								XNAToGL.VertexAttribType[(int) format],
								normalized,
								vertexDeclaration.VertexStride,
								ptr
							);
							attr.CurrentBuffer = buffer;
							attr.CurrentPointer = ptr;
							attr.CurrentFormat = format;
							attr.CurrentNormalized = normalized;
							attr.CurrentStride = vertexDeclaration.VertexStride;
						}
						if (SupportsHardwareInstancing)
						{
							attributeDivisor[attribLoc] = bindings[i].InstanceFrequency;
						}
					}
				}
				FlushGLVertexAttributes();

				ldEffect = currentEffect;
				ldTechnique = currentTechnique;
				ldPass = currentPass;
				effectApplied = false;
				ldVertexDeclaration = null;
				ldPointer = IntPtr.Zero;
			}

			MojoShader.MOJOSHADER_glProgramReady();
			MojoShader.MOJOSHADER_glProgramViewportFlip(flipViewport);
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
			BindVertexBuffer(OpenGLBuffer.NullBuffer);
			IntPtr basePtr = ptr + (vertexDeclaration.VertexStride * vertexOffset);

			if (	vertexDeclaration != ldVertexDeclaration ||
				basePtr != ldPointer ||
				currentEffect != ldEffect ||
				currentTechnique != ldTechnique ||
				currentPass != ldPass ||
				effectApplied	)
			{
				foreach (VertexElement element in vertexDeclaration.elements)
				{
					int attribLoc = MojoShader.MOJOSHADER_glGetVertexAttribLocation(
						XNAToGL.VertexAttribUsage[(int) element.VertexElementUsage],
						element.UsageIndex
					);
					if (attribLoc == -1)
					{
						// Stream not used!
						continue;
					}
					attributeEnabled[attribLoc] = true;
					VertexAttribute attr = attributes[attribLoc];
					IntPtr finalPtr = basePtr + element.Offset;
					bool normalized = XNAToGL.VertexAttribNormalized(element);
					if (	attr.CurrentBuffer != 0 ||
						attr.CurrentPointer != finalPtr ||
						attr.CurrentFormat != element.VertexElementFormat ||
						attr.CurrentNormalized != normalized ||
						attr.CurrentStride != vertexDeclaration.VertexStride	)
					{
						glVertexAttribPointer(
							attribLoc,
							XNAToGL.VertexAttribSize[(int) element.VertexElementFormat],
							XNAToGL.VertexAttribType[(int) element.VertexElementFormat],
							normalized,
							vertexDeclaration.VertexStride,
							finalPtr
						);
						attr.CurrentBuffer = 0;
						attr.CurrentPointer = finalPtr;
						attr.CurrentFormat = element.VertexElementFormat;
						attr.CurrentNormalized = normalized;
						attr.CurrentStride = vertexDeclaration.VertexStride;
					}
					attributeDivisor[attribLoc] = 0;
				}
				FlushGLVertexAttributes();

				ldVertexDeclaration = vertexDeclaration;
				ldPointer = ptr;
				ldEffect = currentEffect;
				ldTechnique = currentTechnique;
				ldPass = currentPass;
				effectApplied = false;
			}

			MojoShader.MOJOSHADER_glProgramReady();
			MojoShader.MOJOSHADER_glProgramViewportFlip(flipViewport);
		}

		private void FlushGLVertexAttributes()
		{
			for (int i = 0; i < attributes.Length; i += 1)
			{
				if (attributeEnabled[i])
				{
					attributeEnabled[i] = false;
					if (!previousAttributeEnabled[i])
					{
						glEnableVertexAttribArray(i);
						previousAttributeEnabled[i] = true;
					}
				}
				else if (previousAttributeEnabled[i])
				{
					glDisableVertexAttribArray(i);
					previousAttributeEnabled[i] = false;
				}

				int divisor = attributeDivisor[i];
				if (divisor != previousAttributeDivisor[i])
				{
					glVertexAttribDivisor(i, divisor);
					previousAttributeDivisor[i] = divisor;
				}
			}
		}

		#endregion

		#region glGenBuffers Methods

		public IGLBuffer GenVertexBuffer(
			bool dynamic,
			int vertexCount,
			int vertexStride
		) {
			OpenGLBuffer result = null;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			uint handle;
			glCreateBuffers(1, out handle);

			result = new OpenGLBuffer(
				handle,
				(IntPtr) (vertexStride * vertexCount),
				dynamic ? GLenum.GL_STREAM_DRAW : GLenum.GL_STATIC_DRAW
			);

			glNamedBufferData(
				handle,
				result.BufferSize,
				IntPtr.Zero,
				result.Dynamic
			);

#if !DISABLE_THREADING
			});
#endif

			return result;
		}

		public IGLBuffer GenIndexBuffer(
			bool dynamic,
			int indexCount,
			IndexElementSize indexElementSize
		) {
			OpenGLBuffer result = null;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			uint handle;
			glCreateBuffers(1, out handle);

			result = new OpenGLBuffer(
				handle,
				(IntPtr) (indexCount * XNAToGL.IndexSize[(int) indexElementSize]),
				dynamic ? GLenum.GL_STREAM_DRAW : GLenum.GL_STATIC_DRAW
			);

			glNamedBufferData(
				handle,
				result.BufferSize,
				IntPtr.Zero,
				result.Dynamic
			);

#if !DISABLE_THREADING
			});
#endif

			return result;
		}

		#endregion

		#region glBindBuffer Methods

		private void BindVertexBuffer(IGLBuffer buffer)
		{
			uint handle = (buffer as OpenGLBuffer).Handle;
			if (handle != currentVertexBuffer)
			{
				glBindBuffer(GLenum.GL_ARRAY_BUFFER, handle);
				currentVertexBuffer = handle;
			}
		}

		private void BindIndexBuffer(IGLBuffer buffer)
		{
			uint handle = (buffer as OpenGLBuffer).Handle;
			if (handle != currentIndexBuffer)
			{
				glBindBuffer(GLenum.GL_ELEMENT_ARRAY_BUFFER, handle);
				currentIndexBuffer = handle;
			}
		}

		#endregion

		#region glSetBufferData Methods

		public void SetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif
			uint handle = (buffer as OpenGLBuffer).Handle;

			if (options == SetDataOptions.Discard)
			{
				glNamedBufferData(
					handle,
					buffer.BufferSize,
					IntPtr.Zero,
					(buffer as OpenGLBuffer).Dynamic
				);
			}

			glNamedBufferSubData(
				handle,
				(IntPtr) offsetInBytes,
				(IntPtr) dataLength,
				data
			);

#if !DISABLE_THREADING
			});
#endif
		}

		public void SetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			uint handle = (buffer as OpenGLBuffer).Handle;

			if (options == SetDataOptions.Discard)
			{
				glNamedBufferData(
					handle,
					buffer.BufferSize,
					IntPtr.Zero,
					(buffer as OpenGLBuffer).Dynamic
				);
			}

			glNamedBufferSubData(
				handle,
				(IntPtr) offsetInBytes,
				(IntPtr) dataLength,
				data
			);

#if !DISABLE_THREADING
			});
#endif
		}

		#endregion

		#region glGetBufferData Methods

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

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif
				
			glGetNamedBufferSubData(
				(buffer as OpenGLBuffer).Handle,
				(IntPtr) offsetInBytes,
				(IntPtr) (elementCount * vertexStride),
				cpy
			);

#if !DISABLE_THREADING
			});
#endif

			if (useStagingBuffer)
			{
				IntPtr src = cpy;
				IntPtr dst = data + (startIndex * elementSizeInBytes);
				for (int i = 0; i < elementCount; i += 1)
				{
					memcpy(dst, src, (IntPtr) elementSizeInBytes);
					dst += elementSizeInBytes;
					src += vertexStride;
				}
				Marshal.FreeHGlobal(cpy);
			}
		}

		public void GetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			glGetNamedBufferSubData(
				(buffer as OpenGLBuffer).Handle,
				(IntPtr) offsetInBytes,
				(IntPtr) (elementCount * elementSizeInBytes),
				data + (startIndex * elementSizeInBytes)
			);

#if !DISABLE_THREADING
			});
#endif
		}

		#endregion

		#region glDeleteBuffers Methods

		private void DeleteVertexBuffer(IGLBuffer buffer)
		{
			uint handle = (buffer as OpenGLBuffer).Handle;
			if (handle == currentVertexBuffer)
			{
				glBindBuffer(GLenum.GL_ARRAY_BUFFER, 0);
				currentVertexBuffer = 0;
			}
			for (int i = 0; i < attributes.Length; i += 1)
			{
				if (handle == attributes[i].CurrentBuffer)
				{
					// Force the next vertex attrib update!
					attributes[i].CurrentBuffer = uint.MaxValue;
				}
			}
			glDeleteBuffers(1, ref handle);
		}

		private void DeleteIndexBuffer(IGLBuffer buffer)
		{
			uint handle = (buffer as OpenGLBuffer).Handle;
			if (handle == currentIndexBuffer)
			{
				glBindBuffer(GLenum.GL_ELEMENT_ARRAY_BUFFER, 0);
				currentIndexBuffer = 0;
			}
			glDeleteBuffers(1, ref handle);
		}

		#endregion

		#region glCreateTexture Methods

		private OpenGLTexture CreateTexture(
			GLenum target,
			int levelCount
		) {
			uint handle;
			glCreateTextures(target, 1, out handle);
			OpenGLTexture result = new OpenGLTexture(
				handle,
				target,
				levelCount
			);
			return result;
		}

		public IGLTexture CreateTexture2D(
			SurfaceFormat format,
			int width,
			int height,
			int levelCount
		) {
			OpenGLTexture result = null;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			result = CreateTexture(
				GLenum.GL_TEXTURE_2D,
				levelCount
			);

			glTextureStorage2D(
				result.Handle,
				levelCount,
				XNAToGL.TextureInternalFormat[(int) format],
				width,
				height
			);

#if !DISABLE_THREADING
			});
#endif

			return result;
		}

		public IGLTexture CreateTexture3D(
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount
		) {
			OpenGLTexture result = null;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			result = CreateTexture(
				GLenum.GL_TEXTURE_3D,
				levelCount
			);

			glTextureStorage3D(
				result.Handle,
				levelCount,
				XNAToGL.TextureInternalFormat[(int) format],
				width,
				height,
				depth
			);

#if !DISABLE_THREADING
			});
#endif

			return result;
		}

		public IGLTexture CreateTextureCube(
			SurfaceFormat format,
			int size,
			int levelCount
		) {
			OpenGLTexture result = null;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			result = CreateTexture(
				GLenum.GL_TEXTURE_CUBE_MAP,
				levelCount
			);

			glTextureStorage2D(
				result.Handle,
				levelCount,
				XNAToGL.TextureInternalFormat[(int) format],
				size,
				size
			);

#if !DISABLE_THREADING
			});
#endif

			return result;
		}

		#endregion

		#region glTexSubImage Methods

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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif
			GLenum glFormat = XNAToGL.TextureFormat[(int) format];
			if (glFormat == GLenum.GL_COMPRESSED_TEXTURE_FORMATS)
			{
				/* Note that we're using glInternalFormat, not glFormat.
				 * In this case, they should actually be the same thing,
				 * but we use glFormat somewhat differently for
				 * compressed textures.
				 * -flibit
				 */
				glCompressedTextureSubImage2D(
					(texture as OpenGLTexture).Handle,
					level,
					x,
					y,
					w,
					h,
					XNAToGL.TextureInternalFormat[(int) format],
					dataLength,
					data
				);
			}
			else
			{
				// Set pixel alignment to match texel size in bytes
				int packSize = Texture.GetPixelStoreAlignment(format);
				if (packSize != 4)
				{
					glPixelStorei(
						GLenum.GL_UNPACK_ALIGNMENT,
						packSize
					);
				}

				glTextureSubImage2D(
					(texture as OpenGLTexture).Handle,
					level,
					x,
					y,
					w,
					h,
					glFormat,
					XNAToGL.TextureDataType[(int) format],
					data
				);

				// Keep this state sane -flibit
				if (packSize != 4)
				{
					glPixelStorei(
						GLenum.GL_UNPACK_ALIGNMENT,
						4
					);
				}
			}

#if !DISABLE_THREADING
			});
#endif
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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif
			glTextureSubImage3D(
				(texture as OpenGLTexture).Handle,
				level,
				left,
				top,
				front,
				right - left,
				bottom - top,
				back - front,
				XNAToGL.TextureFormat[(int) format],
				XNAToGL.TextureDataType[(int) format],
				data
			);

#if !DISABLE_THREADING
			});
#endif
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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif
			GLenum glFormat = XNAToGL.TextureFormat[(int) format];
			if (glFormat == GLenum.GL_COMPRESSED_TEXTURE_FORMATS)
			{
				/* Note that we're using glInternalFormat, not glFormat.
				 * In this case, they should actually be the same thing,
				 * but we use glFormat somewhat differently for
				 * compressed textures.
				 * -flibit
				 */
				glCompressedTextureSubImage3D(
					(texture as OpenGLTexture).Handle,
					level,
					xOffset,
					yOffset,
					(int) cubeMapFace,
					width,
					height,
					1,
					XNAToGL.TextureInternalFormat[(int) format],
					dataLength,
					data
				);
			}
			else
			{
				glTextureSubImage3D(
					(texture as OpenGLTexture).Handle,
					level,
					xOffset,
					yOffset,
					(int) cubeMapFace,
					width,
					height,
					1,
					glFormat,
					XNAToGL.TextureDataType[(int) format],
					data
				);
			}

#if !DISABLE_THREADING
			});
#endif
		}

		public void SetTextureData2DPointer(
			Texture2D texture,
			IntPtr ptr
		) {
			// Set pixel alignment to match texel size in bytes
			int packSize = Texture.GetPixelStoreAlignment(texture.Format);
			if (packSize != 4)
			{
				glPixelStorei(
					GLenum.GL_UNPACK_ALIGNMENT,
					packSize
				);
			}
			glTextureSubImage2D(
				(texture.texture as OpenGLTexture).Handle,
				0,
				0,
				0,
				texture.Width,
				texture.Height,
				XNAToGL.TextureFormat[(int) texture.Format],
				XNAToGL.TextureDataType[(int) texture.Format],
				ptr
			);
			// Keep this state sane -flibit
			if (packSize != 4)
			{
				glPixelStorei(GLenum.GL_UNPACK_ALIGNMENT, 4);
			}
		}

		#endregion

		#region glGetTexImage Methods

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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			if (level == 0 && ReadTargetIfApplicable(
				texture,
				width,
				height,
				level,
				data,
				subX,
				subY,
				subW,
				subH
			)) {
				return;
			}

			glGetTextureSubImage(
				(texture as OpenGLTexture).Handle,
				level,
				subX,
				subY,
				0,
				subW,
				subH,
				1,
				XNAToGL.TextureFormat[(int) format],
				XNAToGL.TextureDataType[(int) format],
				elementCount * elementSizeInBytes,
				data
			);

#if !DISABLE_THREADING
			});
#endif
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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			glGetTextureSubImage(
				(texture as OpenGLTexture).Handle,
				level,
				left,
				top,
				front,
				right - left,
				bottom - top,
				back - front,
				XNAToGL.TextureFormat[(int) format],
				XNAToGL.TextureDataType[(int) format],
				elementCount * elementSizeInBytes,
				data
			);

#if !DISABLE_THREADING
			});
#endif
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
#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			glGetTextureSubImage(
				(texture as OpenGLTexture).Handle,
				level,
				subX,
				subY,
				(int) cubeMapFace,
				subW,
				subH,
				1,
				XNAToGL.TextureFormat[(int) format],
				XNAToGL.TextureDataType[(int) format],
				elementCount * elementSizeInBytes,
				data
			);

#if !DISABLE_THREADING
			});
#endif
		}

		#endregion

		#region glDeleteTexture Method

		private void DeleteTexture(IGLTexture texture)
		{
			uint handle = (texture as OpenGLTexture).Handle;
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (handle == currentAttachments[i])
				{
					// Force an attachment update, this no longer exists!
					currentAttachments[i] = uint.MaxValue;
				}
			}
			glDeleteTextures(1, ref handle);
		}

		#endregion

		#region glReadPixels Methods

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

			uint prevReadBuffer = currentReadFramebuffer;

			if (Backbuffer.MultiSampleCount > 0)
			{
				// We have to resolve the renderbuffer to a texture first.
				OpenGLBackbuffer glBack = Backbuffer as OpenGLBackbuffer;
				if (glBack.Texture == 0)
				{
					glCreateTextures(GLenum.GL_TEXTURE_2D, 1, out glBack.Texture);
					glTextureStorage2D(
						glBack.Texture,
						1,
						GLenum.GL_RGBA,
						glBack.Width,
						glBack.Height
					);
				}
				glNamedFramebufferTexture(
					glBack.Handle,
					GLenum.GL_COLOR_ATTACHMENT0,
					glBack.Texture,
					0
				);
				glBlitNamedFramebuffer(
					glBack.Handle,
					resolveFramebufferDraw,
					0, 0, glBack.Width, glBack.Height,
					0, 0, glBack.Width, glBack.Height,
					GLenum.GL_COLOR_BUFFER_BIT,
					GLenum.GL_LINEAR
				);
				BindReadFramebuffer(resolveFramebufferDraw);
			}
			else
			{
				BindReadFramebuffer(
					(Backbuffer is OpenGLBackbuffer) ?
						(Backbuffer as OpenGLBackbuffer).Handle :
						0
				);
			}

			glReadPixels(
				subX,
				subY,
				subW,
				subH,
				GLenum.GL_RGBA,
				GLenum.GL_UNSIGNED_BYTE,
				data
			);

			BindReadFramebuffer(prevReadBuffer);

			// Now we get to do a software-based flip! Yes, really! -flibit
			int pitch = subW * 4;
			IntPtr temp = Marshal.AllocHGlobal(pitch);
			for (int row = 0; row < subH / 2; row += 1)
			{
				// Top to temp, bottom to top, temp to bottom
				memcpy(temp, data + (row * pitch), (IntPtr) pitch);
				memcpy(data + (row * pitch), data + ((subH - row - 1) * pitch), (IntPtr) pitch);
				memcpy(data + ((subH - row - 1) * pitch), temp, (IntPtr) pitch);
			}
			Marshal.FreeHGlobal(temp);
		}

		/// <summary>
		/// Attempts to read the texture data directly from the FBO using glReadPixels
		/// </summary>
		/// <typeparam name="T">Texture data type</typeparam>
		/// <param name="texture">The texture to read from</param>
		/// <param name="width">The texture width</param>
		/// <param name="height">The texture height</param>
		/// <param name="level">The texture level</param>
		/// <param name="data">The texture data array</param>
		/// <param name="rect">The portion of the image to read from</param>
		/// <returns>True if we successfully read the texture data</returns>
		private bool ReadTargetIfApplicable(
			IGLTexture texture,
			int width,
			int height,
			int level,
			IntPtr data,
			int subX,
			int subY,
			int subW,
			int subH
		) {
			bool texUnbound = (	currentDrawBuffers != 1 ||
						currentAttachments[0] != (texture as OpenGLTexture).Handle	);
			if (texUnbound)
			{
				return false;
			}

			uint prevReadBuffer = currentReadFramebuffer;
			BindReadFramebuffer(targetFramebuffer);

			/* glReadPixels should be faster than reading
			 * back from the render target if we are already bound.
			 */
			glReadPixels(
				subX,
				subY,
				subW,
				subH,
				GLenum.GL_RGBA, // FIXME: Assumption!
				GLenum.GL_UNSIGNED_BYTE,
				data
			);

			BindReadFramebuffer(prevReadBuffer);
			return true;
		}

		#endregion

		#region RenderTarget->Texture Method

		public void ResolveTarget(RenderTargetBinding target)
		{
			if ((target.RenderTarget as IRenderTarget).MultiSampleCount > 0)
			{
				// Set up the texture framebuffer
				int width, height;
				if (target.RenderTarget is RenderTarget2D)
				{
					Texture2D target2D = (target.RenderTarget as Texture2D);
					width = target2D.Width;
					height = target2D.Height;
					glNamedFramebufferTexture(
						resolveFramebufferDraw,
						GLenum.GL_COLOR_ATTACHMENT0,
						(target.RenderTarget.texture as OpenGLTexture).Handle,
						0
					);
				}
				else
				{
					TextureCube targetCube = (target.RenderTarget as TextureCube);
					width = targetCube.Size;
					height = targetCube.Size;
					glNamedFramebufferTextureLayer(
						resolveFramebufferDraw,
						GLenum.GL_COLOR_ATTACHMENT0,
						(target.RenderTarget.texture as OpenGLTexture).Handle,
						0,
						(int) target.CubeMapFace
					);
				}

				// Set up the renderbuffer framebuffer
				glNamedFramebufferRenderbuffer(
					resolveFramebufferRead,
					GLenum.GL_COLOR_ATTACHMENT0,
					GLenum.GL_RENDERBUFFER,
					((target.RenderTarget as IRenderTarget).ColorBuffer as OpenGLRenderbuffer).Handle
				);

				// Blit!
				if (scissorTestEnable)
				{
					glDisable(GLenum.GL_SCISSOR_TEST);
				}
				glBlitNamedFramebuffer(
					resolveFramebufferRead,
					resolveFramebufferDraw,
					0, 0, width, height,
					0, 0, width, height,
					GLenum.GL_COLOR_BUFFER_BIT,
					GLenum.GL_LINEAR
				);
				if (scissorTestEnable)
				{
					glEnable(GLenum.GL_SCISSOR_TEST);
				}
			}

			// If the target has mipmaps, regenerate them now
			if (target.RenderTarget.LevelCount > 1)
			{
				glGenerateTextureMipmap((target.RenderTarget.texture as OpenGLTexture).Handle);
			}
		}

		#endregion

		#region Framebuffer Methods

		private void BindFramebuffer(uint handle)
		{
			if (	currentReadFramebuffer != handle &&
				currentDrawFramebuffer != handle	)
			{
				glBindFramebuffer(
					GLenum.GL_FRAMEBUFFER,
					handle
				);
				currentReadFramebuffer = handle;
				currentDrawFramebuffer = handle;
			}
			else if (currentReadFramebuffer != handle)
			{
				BindReadFramebuffer(handle);
			}
			else if (currentDrawFramebuffer != handle)
			{
				BindDrawFramebuffer(handle);
			}
		}

		private void BindReadFramebuffer(uint handle)
		{
			if (handle == currentReadFramebuffer)
			{
				return;
			}

			glBindFramebuffer(
				GLenum.GL_READ_FRAMEBUFFER,
				handle
			);

			currentReadFramebuffer = handle;
		}

		private void BindDrawFramebuffer(uint handle)
		{
			if (handle == currentDrawFramebuffer)
			{
				return;
			}

			glBindFramebuffer(
				GLenum.GL_DRAW_FRAMEBUFFER,
				handle
			);

			currentDrawFramebuffer = handle;
		}

		#endregion

		#region Renderbuffer Methods

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount
		) {
			uint handle = 0;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			glCreateRenderbuffers(1, out handle);
			if (multiSampleCount > 0)
			{
				glNamedRenderbufferStorageMultisample(
					handle,
					multiSampleCount,
					XNAToGL.TextureInternalFormat[(int) format],
					width,
					height
				);
			}
			else
			{
				glNamedRenderbufferStorage(
					handle,
					XNAToGL.TextureInternalFormat[(int) format],
					width,
					height
				);
			}

#if !DISABLE_THREADING
			});
#endif

			return new OpenGLRenderbuffer(handle);
		}

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		) {
			uint handle = 0;

#if !DISABLE_THREADING
			ForceToMainThread(() => {
#endif

			glCreateRenderbuffers(1, out handle);
			if (multiSampleCount > 0)
			{
				glNamedRenderbufferStorageMultisample(
					handle,
					multiSampleCount,
					XNAToGL.DepthStorage[(int) format],
					width,
					height
				);
			}
			else
			{
				glNamedRenderbufferStorage(
					handle,
					XNAToGL.DepthStorage[(int) format],
					width,
					height
				);
			}

#if !DISABLE_THREADING
			});
#endif

			return new OpenGLRenderbuffer(handle);
		}

		private void DeleteRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			uint handle = (renderbuffer as OpenGLRenderbuffer).Handle;

			// Check color attachments
			for (int i = 0; i < currentAttachments.Length; i += 1)
			{
				if (handle == currentAttachments[i])
				{
					// Force an attachment update, this no longer exists!
					currentAttachments[i] = uint.MaxValue;
				}
			}

			// Check depth/stencil attachment
			if (handle == currentRenderbuffer)
			{
				// Force a renderbuffer update, this no longer exists!
				currentRenderbuffer = uint.MaxValue;
			}

			// Finally.
			glDeleteRenderbuffers(1, ref handle);
		}

		#endregion

		#region glEnable/glDisable Method

		private void ToggleGLState(GLenum feature, bool enable)
		{
			if (enable)
			{
				glEnable(feature);
			}
			else
			{
				glDisable(feature);
			}
		}

		#endregion

		#region glClear Method

		public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
		{
			// glClear depends on the scissor rectangle!
			if (scissorTestEnable)
			{
				glDisable(GLenum.GL_SCISSOR_TEST);
			}

			bool clearTarget = (options & ClearOptions.Target) == ClearOptions.Target;
			bool clearDepth = (options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer;
			bool clearStencil = (options & ClearOptions.Stencil) == ClearOptions.Stencil;

			// Get the clear mask, set the clear properties if needed
			GLenum clearMask = GLenum.GL_ZERO;
			if (clearTarget)
			{
				clearMask |= GLenum.GL_COLOR_BUFFER_BIT;
				if (!color.Equals(currentClearColor))
				{
					glClearColor(
						color.X,
						color.Y,
						color.Z,
						color.W
					);
					currentClearColor = color;
				}
				// glClear depends on the color write mask!
				if (colorWriteEnable != ColorWriteChannels.All)
				{
					// FIXME: ColorWriteChannels1/2/3? -flibit
					glColorMask(true, true, true, true);
				}
			}
			if (clearDepth)
			{
				clearMask |= GLenum.GL_DEPTH_BUFFER_BIT;
				if (depth != currentClearDepth)
				{
					glClearDepth((double) depth);
					currentClearDepth = depth;
				}
				// glClear depends on the depth write mask!
				if (!zWriteEnable)
				{
					glDepthMask(true);
				}
			}
			if (clearStencil)
			{
				clearMask |= GLenum.GL_STENCIL_BUFFER_BIT;
				if (stencil != currentClearStencil)
				{
					glClearStencil(stencil);
					currentClearStencil = stencil;
				}
				// glClear depends on the stencil write mask!
				if (stencilWriteMask != -1)
				{
					// AKA 0xFFFFFFFF, ugh -flibit
					glStencilMask(-1);
				}
			}

			// CLEAR!
			glClear(clearMask);

			// Clean up after ourselves.
			if (scissorTestEnable)
			{
				glEnable(GLenum.GL_SCISSOR_TEST);
			}
			if (clearTarget && colorWriteEnable != ColorWriteChannels.All)
			{
				// FIXME: ColorWriteChannels1/2/3? -flibit
				glColorMask(
					(colorWriteEnable & ColorWriteChannels.Red) != 0,
					(colorWriteEnable & ColorWriteChannels.Blue) != 0,
					(colorWriteEnable & ColorWriteChannels.Green) != 0,
					(colorWriteEnable & ColorWriteChannels.Alpha) != 0
				);
			}
			if (clearDepth && !zWriteEnable)
			{
				glDepthMask(false);
			}
			if (clearStencil && stencilWriteMask != -1) // AKA 0xFFFFFFFF, ugh -flibit
			{
				glStencilMask(stencilWriteMask);
			}
		}

		#endregion

		#region SetRenderTargets Method

		public void SetRenderTargets(
			RenderTargetBinding[] renderTargets,
			IGLRenderbuffer renderbuffer,
			DepthFormat depthFormat
		) {
			// Bind the right framebuffer, if needed
			if (renderTargets == null)
			{
				BindFramebuffer(
					(Backbuffer is OpenGLBackbuffer) ?
						(Backbuffer as OpenGLBackbuffer).Handle :
						0
				);
				flipViewport = 1;
				return;
			}
			else
			{
				BindFramebuffer(targetFramebuffer);
				flipViewport = -1;
			}

			int i;
			for (i = 0; i < renderTargets.Length; i += 1)
			{
				IGLRenderbuffer colorBuffer = (renderTargets[i].RenderTarget as IRenderTarget).ColorBuffer;
				if (colorBuffer != null)
				{
					attachments[i] = (colorBuffer as OpenGLRenderbuffer).Handle;
					attachmentTypes[i] = GLenum.GL_RENDERBUFFER;
				}
				else
				{
					attachments[i] = (renderTargets[i].RenderTarget.texture as OpenGLTexture).Handle;
					if (renderTargets[i].RenderTarget is RenderTarget2D)
					{
						attachmentTypes[i] = GLenum.GL_TEXTURE_2D;
					}
					else
					{
						attachmentTypes[i] = GLenum.GL_TEXTURE_CUBE_MAP_POSITIVE_X + (int) renderTargets[i].CubeMapFace;
					}
				}
			}

			// Update the color attachments, DrawBuffers state
			for (i = 0; i < renderTargets.Length; i += 1)
			{
				if (attachments[i] != currentAttachments[i])
				{
					if (currentAttachments[i] != 0)
					{
						if (	attachmentTypes[i] != GLenum.GL_RENDERBUFFER &&
							currentAttachmentTypes[i] == GLenum.GL_RENDERBUFFER	)
						{
							glNamedFramebufferRenderbuffer(
								targetFramebuffer,
								GLenum.GL_COLOR_ATTACHMENT0 + i,
								GLenum.GL_RENDERBUFFER,
								0
							);
						}
						else if (	attachmentTypes[i] == GLenum.GL_RENDERBUFFER &&
								currentAttachmentTypes[i] != GLenum.GL_RENDERBUFFER	)
						{
							// FIXME: Do we use layer for unbinding cubes? -flibit
							glNamedFramebufferTexture(
								targetFramebuffer,
								GLenum.GL_COLOR_ATTACHMENT0 + i,
								0,
								0
							);
						}
					}
					if (attachmentTypes[i] == GLenum.GL_RENDERBUFFER)
					{
						glNamedFramebufferRenderbuffer(
							targetFramebuffer,
							GLenum.GL_COLOR_ATTACHMENT0 + i,
							GLenum.GL_RENDERBUFFER,
							attachments[i]
						);
					}
					else
					{
						glNamedFramebufferTexture(
							targetFramebuffer,
							GLenum.GL_COLOR_ATTACHMENT0 + i,
							attachments[i],
							0
						);
					}
					currentAttachments[i] = attachments[i];
					currentAttachmentTypes[i] = attachmentTypes[i];
				}
				else if (attachmentTypes[i] != currentAttachmentTypes[i])
				{
					// Texture cube face change!
					glNamedFramebufferTextureLayer(
						targetFramebuffer,
						GLenum.GL_COLOR_ATTACHMENT0 + i,
						attachments[i],
						0,
						(int) attachmentTypes[i] - (int) GLenum.GL_TEXTURE_CUBE_MAP_POSITIVE_X
					);
					currentAttachmentTypes[i] = attachmentTypes[i];
				}
			}
			while (i < currentAttachments.Length)
			{
				if (currentAttachments[i] != 0)
				{
					if (currentAttachmentTypes[i] == GLenum.GL_RENDERBUFFER)
					{
						glNamedFramebufferRenderbuffer(
							targetFramebuffer,
							GLenum.GL_COLOR_ATTACHMENT0 + i,
							GLenum.GL_RENDERBUFFER,
							0
						);
					}
					else
					{
						// FIXME: Do we use layer for unbinding cubes? -flibit
						glNamedFramebufferTexture(
							targetFramebuffer,
							GLenum.GL_COLOR_ATTACHMENT0 + i,
							0,
							0
						);
					}
					currentAttachments[i] = 0;
					currentAttachmentTypes[i] = GLenum.GL_TEXTURE_2D;
				}
				i += 1;
			}
			if (renderTargets.Length != currentDrawBuffers)
			{
				glNamedFramebufferDrawBuffers(
					targetFramebuffer,
					renderTargets.Length,
					drawBuffersArray
				);
				currentDrawBuffers = renderTargets.Length;
			}

			// Update the depth/stencil attachment
			/* FIXME: Notice that we do separate attach calls for the stencil.
			 * We _should_ be able to do a single attach for depthstencil, but
			 * some drivers (like Mesa) cannot into GL_DEPTH_STENCIL_ATTACHMENT.
			 * Use XNAToGL.DepthStencilAttachment when this isn't a problem.
			 * -flibit
			 */
			uint handle;
			if (renderbuffer == null)
			{
				handle = 0;
			}
			else
			{
				handle = (renderbuffer as OpenGLRenderbuffer).Handle;
			}
			if (handle != currentRenderbuffer)
			{
				if (currentDepthStencilFormat == DepthFormat.Depth24Stencil8)
				{
					glNamedFramebufferRenderbuffer(
						targetFramebuffer,
						GLenum.GL_STENCIL_ATTACHMENT,
						GLenum.GL_RENDERBUFFER,
						0
					);
				}
				currentDepthStencilFormat = depthFormat;
				glNamedFramebufferRenderbuffer(
					targetFramebuffer,
					GLenum.GL_DEPTH_ATTACHMENT,
					GLenum.GL_RENDERBUFFER,
					handle
				);
				if (currentDepthStencilFormat == DepthFormat.Depth24Stencil8)
				{
					glNamedFramebufferRenderbuffer(
						targetFramebuffer,
						GLenum.GL_STENCIL_ATTACHMENT,
						GLenum.GL_RENDERBUFFER,
						handle
					);
				}
				currentRenderbuffer = handle;
			}
		}

		#endregion

		#region Query Object Methods

		public IGLQuery CreateQuery()
		{
			uint handle;
			glGenQueries(1, out handle);
			return new OpenGLQuery(handle);
		}

		private void DeleteQuery(IGLQuery query)
		{
			uint handle = (query as OpenGLQuery).Handle;
			glDeleteQueries(
				1,
				ref handle
			);
		}

		public void QueryBegin(IGLQuery query)
		{
			glBeginQuery(
				GLenum.GL_SAMPLES_PASSED,
				(query as OpenGLQuery).Handle
			);
		}

		public void QueryEnd(IGLQuery query)
		{
			// May need to check active queries...?
			glEndQuery(
				GLenum.GL_SAMPLES_PASSED
			);
		}

		public bool QueryComplete(IGLQuery query)
		{
			uint result;
			glGetQueryObjectuiv(
				(query as OpenGLQuery).Handle,
				GLenum.GL_QUERY_RESULT_AVAILABLE,
				out result
			);
			return result != 0;
		}

		public int QueryPixelCount(IGLQuery query)
		{
			uint result;
			glGetQueryObjectuiv(
				(query as OpenGLQuery).Handle,
				GLenum.GL_QUERY_RESULT,
				out result
			);
			return (int) result;
		}

		#endregion

		#region XNA->GL Enum Conversion Class

		private static class XNAToGL
		{
			public static readonly GLenum[] TextureFormat = new GLenum[]
			{
				GLenum.GL_RGBA,				// SurfaceFormat.Color
				GLenum.GL_RGB,				// SurfaceFormat.Bgr565
				GLenum.GL_BGRA,				// SurfaceFormat.Bgra5551
				GLenum.GL_BGRA,				// SurfaceFormat.Bgra4444
				GLenum.GL_COMPRESSED_TEXTURE_FORMATS,	// SurfaceFormat.Dxt1
				GLenum.GL_COMPRESSED_TEXTURE_FORMATS,	// SurfaceFormat.Dxt3
				GLenum.GL_COMPRESSED_TEXTURE_FORMATS,	// SurfaceFormat.Dxt5
				GLenum.GL_RG,				// SurfaceFormat.NormalizedByte2
				GLenum.GL_RGBA,				// SurfaceFormat.NormalizedByte4
				GLenum.GL_RGBA,				// SurfaceFormat.Rgba1010102
				GLenum.GL_RG,				// SurfaceFormat.Rg32
				GLenum.GL_RGBA,				// SurfaceFormat.Rgba64
				GLenum.GL_LUMINANCE,			// SurfaceFormat.Alpha8
				GLenum.GL_RED,				// SurfaceFormat.Single
				GLenum.GL_RG,				// SurfaceFormat.Vector2
				GLenum.GL_RGBA,				// SurfaceFormat.Vector4
				GLenum.GL_RED,				// SurfaceFormat.HalfSingle
				GLenum.GL_RG,				// SurfaceFormat.HalfVector2
				GLenum.GL_RGBA,				// SurfaceFormat.HalfVector4
				GLenum.GL_RGBA,				// SurfaceFormat.HdrBlendable
				GLenum.GL_BGRA				// SurfaceFormat.ColorBgraEXT
			};

			public static readonly GLenum[] TextureInternalFormat = new GLenum[]
			{
				GLenum.GL_RGBA8,				// SurfaceFormat.Color
				GLenum.GL_RGB565,				// SurfaceFormat.Bgr565
				GLenum.GL_RGB5_A1,				// SurfaceFormat.Bgra5551
				GLenum.GL_RGBA4,				// SurfaceFormat.Bgra4444
				GLenum.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT,	// SurfaceFormat.Dxt1
				GLenum.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT,	// SurfaceFormat.Dxt3
				GLenum.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT,	// SurfaceFormat.Dxt5
				GLenum.GL_RG8,					// SurfaceFormat.NormalizedByte2
				GLenum.GL_RGBA8,				// SurfaceFormat.NormalizedByte4
				GLenum.GL_RGB10_A2_EXT,				// SurfaceFormat.Rgba1010102
				GLenum.GL_RG16,					// SurfaceFormat.Rg32
				GLenum.GL_RGBA16,				// SurfaceFormat.Rgba64
				GLenum.GL_LUMINANCE8,				// SurfaceFormat.Alpha8
				GLenum.GL_R32F,					// SurfaceFormat.Single
				GLenum.GL_RG32F,				// SurfaceFormat.Vector2
				GLenum.GL_RGBA32F,				// SurfaceFormat.Vector4
				GLenum.GL_R16F,					// SurfaceFormat.HalfSingle
				GLenum.GL_RG16F,				// SurfaceFormat.HalfVector2
				GLenum.GL_RGBA16F,				// SurfaceFormat.HalfVector4
				GLenum.GL_RGBA16F,				// SurfaceFormat.HdrBlendable
				GLenum.GL_RGBA8,				// SurfaceFormat.ColorBgraEXT
			};

			public static readonly GLenum[] TextureDataType = new GLenum[]
			{
				GLenum.GL_UNSIGNED_BYTE,			// SurfaceFormat.Color
				GLenum.GL_UNSIGNED_SHORT_5_6_5,			// SurfaceFormat.Bgr565
				GLenum.GL_UNSIGNED_SHORT_5_5_5_1_REV,		// SurfaceFormat.Bgra5551
				GLenum.GL_UNSIGNED_SHORT_4_4_4_4_REV,		// SurfaceFormat.Bgra4444
				GLenum.GL_ZERO,					// NOPE
				GLenum.GL_ZERO,					// NOPE
				GLenum.GL_ZERO,					// NOPE
				GLenum.GL_BYTE,					// SurfaceFormat.NormalizedByte2
				GLenum.GL_BYTE,					// SurfaceFormat.NormalizedByte4
				GLenum.GL_UNSIGNED_INT_2_10_10_10_REV,		// SurfaceFormat.Rgba1010102
				GLenum.GL_UNSIGNED_SHORT,			// SurfaceFormat.Rg32
				GLenum.GL_UNSIGNED_SHORT,			// SurfaceFormat.Rgba64
				GLenum.GL_UNSIGNED_BYTE,			// SurfaceFormat.Alpha8
				GLenum.GL_FLOAT,				// SurfaceFormat.Single
				GLenum.GL_FLOAT,				// SurfaceFormat.Vector2
				GLenum.GL_FLOAT,				// SurfaceFormat.Vector4
				GLenum.GL_HALF_FLOAT,				// SurfaceFormat.HalfSingle
				GLenum.GL_HALF_FLOAT,				// SurfaceFormat.HalfVector2
				GLenum.GL_HALF_FLOAT,				// SurfaceFormat.HalfVector4
				GLenum.GL_HALF_FLOAT,				// SurfaceFormat.HdrBlendable
				GLenum.GL_UNSIGNED_BYTE,			// SurfaceFormat.ColorBgraEXT
			};

			public static readonly GLenum[] BlendMode = new GLenum[]
			{
				GLenum.GL_ONE,				// Blend.One
				GLenum.GL_ZERO,				// Blend.Zero
				GLenum.GL_SRC_COLOR,			// Blend.SourceColor
				GLenum.GL_ONE_MINUS_SRC_COLOR,		// Blend.InverseSourceColor
				GLenum.GL_SRC_ALPHA,			// Blend.SourceAlpha
				GLenum.GL_ONE_MINUS_SRC_ALPHA,		// Blend.InverseSourceAlpha
				GLenum.GL_DST_COLOR,			// Blend.DestinationColor
				GLenum.GL_ONE_MINUS_DST_COLOR,		// Blend.InverseDestinationColor
				GLenum.GL_DST_ALPHA,			// Blend.DestinationAlpha
				GLenum.GL_ONE_MINUS_DST_ALPHA,		// Blend.InverseDestinationAlpha
				GLenum.GL_CONSTANT_COLOR,		// Blend.BlendFactor
				GLenum.GL_ONE_MINUS_CONSTANT_COLOR,	// Blend.InverseBlendFactor
				GLenum.GL_SRC_ALPHA_SATURATE		// Blend.SourceAlphaSaturation
			};

			public static readonly GLenum[] BlendEquation = new GLenum[]
			{
				GLenum.GL_FUNC_ADD,			// BlendFunction.Add
				GLenum.GL_FUNC_SUBTRACT,		// BlendFunction.Subtract
				GLenum.GL_FUNC_REVERSE_SUBTRACT,	// BlendFunction.ReverseSubtract
				GLenum.GL_MAX,				// BlendFunction.Max
				GLenum.GL_MIN				// BlendFunction.Min
			};

			public static readonly GLenum[] CompareFunc = new GLenum[]
			{
				GLenum.GL_ALWAYS,	// CompareFunction.Always
				GLenum.GL_NEVER,	// CompareFunction.Never
				GLenum.GL_LESS,		// CompareFunction.Less
				GLenum.GL_LEQUAL,	// CompareFunction.LessEqual
				GLenum.GL_EQUAL,	// CompareFunction.Equal
				GLenum.GL_GEQUAL,	// CompareFunction.GreaterEqual
				GLenum.GL_GREATER,	// CompareFunction.Greater
				GLenum.GL_NOTEQUAL	// CompareFunction.NotEqual
			};

			public static readonly GLenum[] GLStencilOp = new GLenum[]
			{
				GLenum.GL_KEEP,		// StencilOperation.Keep
				GLenum.GL_ZERO,		// StencilOperation.Zero
				GLenum.GL_REPLACE,	// StencilOperation.Replace
				GLenum.GL_INCR_WRAP,	// StencilOperation.Increment
				GLenum.GL_DECR_WRAP,	// StencilOperation.Decrement
				GLenum.GL_INCR,		// StencilOperation.IncrementSaturation
				GLenum.GL_DECR,		// StencilOperation.DecrementSaturation
				GLenum.GL_INVERT	// StencilOperation.Invert
			};

			public static readonly GLenum[] FrontFace = new GLenum[]
			{
				GLenum.GL_ZERO,	// NOPE
				GLenum.GL_CW,	// CullMode.CullClockwiseFace
				GLenum.GL_CCW	// CullMode.CullCounterClockwiseFace
			};

			public static readonly GLenum[] GLFillMode = new GLenum[]
			{
				GLenum.GL_FILL,	// FillMode.Solid
				GLenum.GL_LINE	// FillMode.WireFrame
			};

			public static readonly int[] Wrap = new int[]
			{
				(int) GLenum.GL_REPEAT,			// TextureAddressMode.Wrap
				(int) GLenum.GL_CLAMP_TO_EDGE,		// TextureAddressMode.Clamp
				(int) GLenum.GL_MIRRORED_REPEAT		// TextureAddressMode.Mirror
			};

			public static readonly int[] MagFilter = new int[]
			{
				(int) GLenum.GL_LINEAR,		// TextureFilter.Linear
				(int) GLenum.GL_NEAREST,	// TextureFilter.Point
				(int) GLenum.GL_LINEAR,		// TextureFilter.Anisotropic
				(int) GLenum.GL_LINEAR,		// TextureFilter.LinearMipPoint
				(int) GLenum.GL_NEAREST,	// TextureFilter.PointMipLinear
				(int) GLenum.GL_NEAREST,	// TextureFilter.MinLinearMagPointMipLinear
				(int) GLenum.GL_NEAREST,	// TextureFilter.MinLinearMagPointMipPoint
				(int) GLenum.GL_LINEAR,		// TextureFilter.MinPointMagLinearMipLinear
				(int) GLenum.GL_LINEAR		// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly int[] MinMipFilter = new int[]
			{
				(int) GLenum.GL_LINEAR_MIPMAP_LINEAR,	// TextureFilter.Linear
				(int) GLenum.GL_NEAREST_MIPMAP_NEAREST,	// TextureFilter.Point
				(int) GLenum.GL_LINEAR_MIPMAP_LINEAR,	// TextureFilter.Anisotropic
				(int) GLenum.GL_LINEAR_MIPMAP_NEAREST,	// TextureFilter.LinearMipPoint
				(int) GLenum.GL_NEAREST_MIPMAP_LINEAR,	// TextureFilter.PointMipLinear
				(int) GLenum.GL_LINEAR_MIPMAP_LINEAR,	// TextureFilter.MinLinearMagPointMipLinear
				(int) GLenum.GL_LINEAR_MIPMAP_NEAREST,	// TextureFilter.MinLinearMagPointMipPoint
				(int) GLenum.GL_NEAREST_MIPMAP_LINEAR,	// TextureFilter.MinPointMagLinearMipLinear
				(int) GLenum.GL_NEAREST_MIPMAP_NEAREST	// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly int[] MinFilter = new int[]
			{
				(int) GLenum.GL_LINEAR,		// TextureFilter.Linear
				(int) GLenum.GL_NEAREST,	// TextureFilter.Point
				(int) GLenum.GL_LINEAR,		// TextureFilter.Anisotropic
				(int) GLenum.GL_LINEAR,		// TextureFilter.LinearMipPoint
				(int) GLenum.GL_NEAREST,	// TextureFilter.PointMipLinear
				(int) GLenum.GL_LINEAR,		// TextureFilter.MinLinearMagPointMipLinear
				(int) GLenum.GL_LINEAR,		// TextureFilter.MinLinearMagPointMipPoint
				(int) GLenum.GL_NEAREST,	// TextureFilter.MinPointMagLinearMipLinear
				(int) GLenum.GL_NEAREST		// TextureFilter.MinPointMagLinearMipPoint
			};

			public static readonly GLenum[] DepthStencilAttachment = new GLenum[]
			{
				GLenum.GL_ZERO,				// NOPE
				GLenum.GL_DEPTH_ATTACHMENT,		// DepthFormat.Depth16
				GLenum.GL_DEPTH_ATTACHMENT,		// DepthFormat.Depth24
				GLenum.GL_DEPTH_STENCIL_ATTACHMENT	// DepthFormat.Depth24Stencil8
			};

			public static readonly GLenum[] DepthStorage = new GLenum[]
			{
				GLenum.GL_ZERO,			// NOPE
				GLenum.GL_DEPTH_COMPONENT16,	// DepthFormat.Depth16
				GLenum.GL_DEPTH_COMPONENT24,	// DepthFormat.Depth24
				GLenum.GL_DEPTH24_STENCIL8	// DepthFormat.Depth24Stencil8
			};

			public static readonly float[] DepthBiasScale = new float[]
			{
				0.0f,				// DepthFormat.None
				(float) ((1 << 16) - 1),	// DepthFormat.Depth16
				(float) ((1 << 24) - 1),	// DepthFormat.Depth24
				(float) ((1 << 24) - 1)		// DepthFormat.Depth24Stencil8
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

			public static readonly GLenum[] VertexAttribType = new GLenum[]
			{
				GLenum.GL_FLOAT,		// VertexElementFormat.Single
				GLenum.GL_FLOAT,		// VertexElementFormat.Vector2
				GLenum.GL_FLOAT,		// VertexElementFormat.Vector3
				GLenum.GL_FLOAT,		// VertexElementFormat.Vector4
				GLenum.GL_UNSIGNED_BYTE,	// VertexElementFormat.Color
				GLenum.GL_UNSIGNED_BYTE,	// VertexElementFormat.Byte4
				GLenum.GL_SHORT,		// VertexElementFormat.Short2
				GLenum.GL_SHORT,		// VertexElementFormat.Short4
				GLenum.GL_SHORT,		// VertexElementFormat.NormalizedShort2
				GLenum.GL_SHORT,		// VertexElementFormat.NormalizedShort4
				GLenum.GL_HALF_FLOAT,		// VertexElementFormat.HalfVector2
				GLenum.GL_HALF_FLOAT		// VertexElementFormat.HalfVector4
			};

			public static bool VertexAttribNormalized(VertexElement element)
			{
				return (	element.VertexElementUsage == VertexElementUsage.Color ||
						element.VertexElementFormat == VertexElementFormat.NormalizedShort2 ||
						element.VertexElementFormat == VertexElementFormat.NormalizedShort4	);
			}

			public static readonly GLenum[] IndexType = new GLenum[]
			{
				GLenum.GL_UNSIGNED_SHORT,	// IndexElementSize.SixteenBits
				GLenum.GL_UNSIGNED_INT		// IndexElementSize.ThirtyTwoBits
			};

			public static readonly int[] IndexSize = new int[]
			{
				2,	// IndexElementSize.SixteenBits
				4	// IndexElementSize.ThirtyTwoBits
			};

			public static readonly GLenum[] Primitive = new GLenum[]
			{
				GLenum.GL_TRIANGLES,		// PrimitiveType.TriangleList
				GLenum.GL_TRIANGLE_STRIP,	// PrimitiveType.TriangleStrip
				GLenum.GL_LINES,		// PrimitiveType.LineList
				GLenum.GL_LINE_STRIP,		// PrimitiveType.LineStrip
				GLenum.GL_POINTS		// PrimitiveType.PointListEXT
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

		private bool UseFauxBackbuffer(PresentationParameters presentationParameters, DisplayMode mode)
		{
			int drawX, drawY;
			SDL.SDL_GL_GetDrawableSize(
				presentationParameters.DeviceWindowHandle,
				out drawX,
				out drawY
			);
			bool displayMismatch = (	drawX != presentationParameters.BackBufferWidth ||
							drawY != presentationParameters.BackBufferHeight	);
			return displayMismatch || (presentationParameters.MultiSampleCount > 0);
		}

		private class OpenGLBackbuffer : IGLBackbuffer
		{
			public uint Handle
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

			public uint Texture;

			private uint colorAttachment;
			private uint depthStencilAttachment;
			private ModernGLDevice glDevice;

			public OpenGLBackbuffer(
				ModernGLDevice device,
				int width,
				int height,
				DepthFormat depthFormat,
				int multiSampleCount
			) {
				Width = width;
				Height = height;

				glDevice = device;
				DepthFormat = depthFormat;
				MultiSampleCount = multiSampleCount;
				Texture = 0;

				// Generate and bind the FBO.
				uint handle;
				glDevice.glCreateFramebuffers(1, out handle);
				Handle = handle;
				glDevice.BindFramebuffer(Handle);

				// Create and attach the color buffer
				glDevice.glCreateRenderbuffers(1, out colorAttachment);
				if (multiSampleCount > 0)
				{
					glDevice.glNamedRenderbufferStorageMultisample(
						colorAttachment,
						multiSampleCount,
						GLenum.GL_RGBA8,
						width,
						height
					);
				}
				else
				{
					glDevice.glNamedRenderbufferStorage(
						colorAttachment,
						GLenum.GL_RGBA8,
						width,
						height
					);
				}
				glDevice.glNamedFramebufferRenderbuffer(
					Handle,
					GLenum.GL_COLOR_ATTACHMENT0,
					GLenum.GL_RENDERBUFFER,
					colorAttachment
				);

				if (depthFormat == DepthFormat.None)
				{
					// Don't bother creating a depth/stencil buffer.
					depthStencilAttachment = 0;
					return;
				}

				// Create and attach the depth/stencil buffer
				glDevice.glCreateRenderbuffers(1, out depthStencilAttachment);
				if (multiSampleCount > 0)
				{
					glDevice.glNamedRenderbufferStorageMultisample(
						depthStencilAttachment,
						multiSampleCount,
						XNAToGL.DepthStorage[(int) depthFormat],
						width,
						height
					);
				}
				else
				{
					glDevice.glNamedRenderbufferStorage(
						depthStencilAttachment,
						XNAToGL.DepthStorage[(int) depthFormat],
						width,
						height
					);
				}
				glDevice.glNamedFramebufferRenderbuffer(
					Handle,
					GLenum.GL_DEPTH_ATTACHMENT,
					GLenum.GL_RENDERBUFFER,
					depthStencilAttachment
				);
				if (depthFormat == DepthFormat.Depth24Stencil8)
				{
					glDevice.glNamedFramebufferRenderbuffer(
						Handle,
						GLenum.GL_STENCIL_ATTACHMENT,
						GLenum.GL_RENDERBUFFER,
						depthStencilAttachment
					);
				}
			}

			public void Dispose()
			{
				uint handle = Handle;
				glDevice.BindFramebuffer(0);
				glDevice.glDeleteFramebuffers(1, ref handle);
				glDevice.glDeleteRenderbuffers(1, ref colorAttachment);
				if (depthStencilAttachment != 0)
				{
					glDevice.glDeleteRenderbuffers(1, ref depthStencilAttachment);
				}
				if (Texture != 0)
				{
					glDevice.glDeleteTextures(1, ref Texture);
				}
				glDevice = null;
				Handle = 0;
			}

			public void ResetFramebuffer(
				PresentationParameters presentationParameters,
				bool renderTargetBound
			) {
				Width = presentationParameters.BackBufferWidth;
				Height = presentationParameters.BackBufferHeight;

				DepthFormat depthFormat = presentationParameters.DepthStencilFormat;
				MultiSampleCount = presentationParameters.MultiSampleCount;
				if (Texture != 0)
				{
					glDevice.glDeleteTextures(1, ref Texture);
					Texture = 0;
				}

				// Detach color attachment
				glDevice.glNamedFramebufferRenderbuffer(
					Handle,
					GLenum.GL_COLOR_ATTACHMENT0,
					GLenum.GL_RENDERBUFFER,
					0
				);

				// Detach depth/stencil attachment, if applicable
				if (depthStencilAttachment != 0)
				{
					glDevice.glNamedFramebufferRenderbuffer(
						Handle,
						GLenum.GL_DEPTH_ATTACHMENT,
						GLenum.GL_RENDERBUFFER,
						0
					);
					if (DepthFormat == DepthFormat.Depth24Stencil8)
					{
						glDevice.glNamedFramebufferRenderbuffer(
							Handle,
							GLenum.GL_STENCIL_ATTACHMENT,
							GLenum.GL_RENDERBUFFER,
							0
						);
					}
				}

				// Update our color attachment to the new resolution.
				if (MultiSampleCount > 0)
				{
					glDevice.glNamedRenderbufferStorageMultisample(
						colorAttachment,
						MultiSampleCount,
						GLenum.GL_RGBA8,
						Width,
						Height
					);
				}
				else
				{
					glDevice.glNamedRenderbufferStorage(
						colorAttachment,
						GLenum.GL_RGBA8,
						Width,
						Height
					);
				}
				glDevice.glNamedFramebufferRenderbuffer(
					Handle,
					GLenum.GL_COLOR_ATTACHMENT0,
					GLenum.GL_RENDERBUFFER,
					colorAttachment
				);

				// Generate/Delete depth/stencil attachment, if needed
				if (depthFormat == DepthFormat.None)
				{
					if (depthStencilAttachment != 0)
					{
						glDevice.glDeleteRenderbuffers(
							1,
							ref depthStencilAttachment
						);
						depthStencilAttachment = 0;
					}
				}
				else if (depthStencilAttachment == 0)
				{
					glDevice.glCreateRenderbuffers(
						1,
						out depthStencilAttachment
					);
				}

				// Update the depth/stencil buffer, if applicable
				if (depthStencilAttachment != 0)
				{
					if (MultiSampleCount > 0)
					{
						glDevice.glNamedRenderbufferStorageMultisample(
							depthStencilAttachment,
							MultiSampleCount,
							XNAToGL.DepthStorage[(int)depthFormat],
							Width,
							Height
						);
					}
					else
					{
						glDevice.glNamedRenderbufferStorage(
							depthStencilAttachment,
							XNAToGL.DepthStorage[(int)depthFormat],
							Width,
							Height
						);
					}
					glDevice.glNamedFramebufferRenderbuffer(
						Handle,
						GLenum.GL_DEPTH_ATTACHMENT,
						GLenum.GL_RENDERBUFFER,
						depthStencilAttachment
					);
					if (depthFormat == DepthFormat.Depth24Stencil8)
					{
						glDevice.glNamedFramebufferRenderbuffer(
							Handle,
							GLenum.GL_STENCIL_ATTACHMENT,
							GLenum.GL_RENDERBUFFER,
							depthStencilAttachment
						);
					}
				}
				DepthFormat = depthFormat;
			}
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

		#region Threaded GL Nonsense

		private int mainThreadId;

		private bool IsOnMainThread()
		{
			return mainThreadId == Thread.CurrentThread.ManagedThreadId;
		}

		private void InitThreadedGL(IntPtr window)
		{
			mainThreadId = Thread.CurrentThread.ManagedThreadId;
#if THREADED_GL
			// Create a background context
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
			WindowInfo = window;
			BackgroundContext = new GL_ContextHandle()
			{
				context = SDL.SDL_GL_CreateContext(window)
			};

			// Make the foreground context current.
			SDL.SDL_GL_MakeCurrent(window, glContext);

			// We're going to need glFlush, so load this entry point.
			glFlush = (Flush) Marshal.GetDelegateForFunctionPointer(
				SDL.SDL_GL_GetProcAddress("glFlush"),
				typeof(Flush)
			);
#endif
		}

#if !DISABLE_THREADING

#if THREADED_GL
		private class GL_ContextHandle
		{
			public IntPtr context;
		}
		private GL_ContextHandle BackgroundContext;
		private IntPtr WindowInfo;
		private delegate void Flush();
		private Flush glFlush;

#else
		private System.Collections.Generic.List<Action> actions = new System.Collections.Generic.List<Action>();
		private void RunActions()
		{
			lock (actions)
			{
				foreach (Action action in actions)
				{
					action();
				}
				actions.Clear();
			}
		}
#endif

		private void ForceToMainThread(Action action)
		{
			// If we're already on the main thread, just call the action.
			if (mainThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				action();
				return;
			}

#if THREADED_GL
			lock (BackgroundContext)
			{
				// Make the context current on this thread.
				SDL.SDL_GL_MakeCurrent(WindowInfo, BackgroundContext.context);

				// Execute the action.
				action();

				// Must flush the GL calls now before we release the context.
				glFlush();

				// Free the threaded context for the next threaded call...
				SDL.SDL_GL_MakeCurrent(WindowInfo, IntPtr.Zero);
			}
#else
			ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
			lock (actions)
			{
				actions.Add(() =>
				{
					action();
					resetEvent.Set();
				});
			}
			resetEvent.Wait();
#endif
		}

#endif // !DISABLE_THREADING

		#endregion
	}
}
