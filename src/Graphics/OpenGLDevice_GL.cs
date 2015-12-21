#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
	internal partial class OpenGLDevice : IGLDevice
	{
		#region Private OpenGL Entry Points

		internal enum GLenum : int
		{
			// Hint Enum Value
			GL_DONT_CARE =				0x1100,
			// 0/1
			GL_ZERO =				0x0000,
			GL_ONE =				0x0001,
			// Types
			GL_BYTE =				0x1400,
			GL_UNSIGNED_BYTE =			0x1401,
			GL_SHORT =				0x1402,
			GL_UNSIGNED_SHORT =			0x1403,
			GL_UNSIGNED_INT =			0x1405,
			GL_FLOAT =				0x1406,
			GL_HALF_FLOAT =				0x140B,
			GL_UNSIGNED_SHORT_4_4_4_4 =		0x8033,
			GL_UNSIGNED_SHORT_5_5_5_1 =		0x8034,
			GL_UNSIGNED_INT_10_10_10_2 =		0x8036,
			GL_UNSIGNED_SHORT_5_6_5 =		0x8363,
			GL_UNSIGNED_INT_24_8 =			0x84FA,
			// Strings
			GL_VENDOR =				0x1F00,
			GL_RENDERER =				0x1F01,
			GL_VERSION =				0x1F02,
			GL_EXTENSIONS =				0x1F03,
			// Clear Mask
			GL_COLOR_BUFFER_BIT =			0x4000,
			GL_DEPTH_BUFFER_BIT =			0x0100,
			GL_STENCIL_BUFFER_BIT =			0x0400,
			// Enable Caps
			GL_SCISSOR_TEST =			0x0C11,
			GL_DEPTH_TEST =				0x0B71,
			GL_STENCIL_TEST =			0x0B90,
			// Polygons
			GL_LINE =				0x1B01,
			GL_FILL =				0x1B02,
			GL_CW =					0x0900,
			GL_CCW =				0x0901,
			GL_FRONT =				0x0404,
			GL_BACK =				0x0405,
			GL_FRONT_AND_BACK =			0x0408,
			GL_CULL_FACE =				0x0B44,
			GL_POLYGON_OFFSET_FILL =		0x8037,
			// Texture Type
			GL_TEXTURE_2D =				0x0DE1,
			GL_TEXTURE_3D =				0x806F,
			GL_TEXTURE_CUBE_MAP =			0x8513,
			GL_TEXTURE_CUBE_MAP_POSITIVE_X =	0x8515,
			// Blend Mode
			GL_BLEND =				0x0BE2,
			GL_SRC_COLOR =				0x0300,
			GL_ONE_MINUS_SRC_COLOR =		0x0301,
			GL_SRC_ALPHA =				0x0302,
			GL_ONE_MINUS_SRC_ALPHA =		0x0303,
			GL_DST_ALPHA =				0x0304,
			GL_ONE_MINUS_DST_ALPHA =		0x0305,
			GL_DST_COLOR =				0x0306,
			GL_ONE_MINUS_DST_COLOR =		0x0307,
			GL_SRC_ALPHA_SATURATE =			0x0308,
			GL_CONSTANT_COLOR =			0x8001,
			GL_ONE_MINUS_CONSTANT_COLOR =		0x8002,
			// Equations
			GL_MIN =				0x8007,
			GL_MAX =				0x8008,
			GL_FUNC_ADD =				0x8006,
			GL_FUNC_SUBTRACT =			0x800A,
			GL_FUNC_REVERSE_SUBTRACT =		0x800B,
			// Comparisons
			GL_NEVER =				0x0200,
			GL_LESS =				0x0201,
			GL_EQUAL =				0x0202,
			GL_LEQUAL =				0x0203,
			GL_GREATER =				0x0204,
			GL_NOTEQUAL =				0x0205,
			GL_GEQUAL =				0x0206,
			GL_ALWAYS =				0x0207,
			// Stencil Operations
			GL_INVERT =				0x150A,
			GL_KEEP =				0x1E00,
			GL_REPLACE =				0x1E01,
			GL_INCR =				0x1E02,
			GL_DECR =				0x1E03,
			GL_INCR_WRAP =				0x8507,
			GL_DECR_WRAP =				0x8508,
			// Wrap Modes
			GL_REPEAT =				0x2901,
			GL_CLAMP_TO_EDGE =			0x812F,
			GL_MIRRORED_REPEAT =			0x8370,
			// Filters
			GL_NEAREST =				0x2600,
			GL_LINEAR =				0x2601,
			GL_NEAREST_MIPMAP_NEAREST =		0x2700,
			GL_NEAREST_MIPMAP_LINEAR =		0x2702,
			GL_LINEAR_MIPMAP_NEAREST =		0x2701,
			GL_LINEAR_MIPMAP_LINEAR =		0x2703,
			// Attachments
			GL_COLOR_ATTACHMENT0 =			0x8CE0,
			GL_DEPTH_ATTACHMENT =			0x8D00,
			GL_STENCIL_ATTACHMENT =			0x8D20,
			GL_DEPTH_STENCIL_ATTACHMENT =		0x821A,
			// Texture Formats
			GL_RED =				0x1903,
			GL_RGB =				0x1907,
			GL_RGBA =				0x1908,
			GL_LUMINANCE =				0x1909,
			GL_LUMINANCE8 =				0x8040,
			GL_RGB8 =				0x8051,
			GL_RGBA8 =				0x8058,
			GL_RGBA4 =				0x8056,
			GL_RGB5_A1 =				0x8057,
			GL_RGB10_A2_EXT =			0x8059,
			GL_RGBA16 =				0x805B,
			GL_BGRA =				0x80E1,
			GL_DEPTH_COMPONENT16 =			0x81A5,
			GL_DEPTH_COMPONENT24 =			0x81A6,
			GL_RG =					0x8227,
			GL_RG8 =				0x822B,
			GL_RG16 =				0x822C,
			GL_R16F =				0x822D,
			GL_R32F =				0x822E,
			GL_RG16F =				0x822F,
			GL_RG32F =				0x8230,
			GL_RGBA32F =				0x8814,
			GL_RGBA16F =				0x881A,
			GL_DEPTH24_STENCIL8 =			0x88F0,
			GL_COMPRESSED_TEXTURE_FORMATS =		0x86A3,
			GL_COMPRESSED_RGBA_S3TC_DXT1_EXT =	0x83F1,
			GL_COMPRESSED_RGBA_S3TC_DXT3_EXT =	0x83F2,
			GL_COMPRESSED_RGBA_S3TC_DXT5_EXT =	0x83F3,
			// Texture Internal Formats
			GL_DEPTH_COMPONENT =			0x1902,
			GL_DEPTH_STENCIL =			0x84F9,
			// Textures
			GL_TEXTURE_WRAP_S =			0x2802,
			GL_TEXTURE_WRAP_T =			0x2803,
			GL_TEXTURE_WRAP_R =			0x8072,
			GL_TEXTURE_MAG_FILTER =			0x2800,
			GL_TEXTURE_MIN_FILTER =			0x2801,
			GL_TEXTURE_MAX_ANISOTROPY_EXT =		0x84FE,
			GL_TEXTURE_BASE_LEVEL =			0x813C,
			GL_TEXTURE_MAX_LEVEL =			0x813D,
			GL_TEXTURE_LOD_BIAS =			0x8501,
			GL_UNPACK_ALIGNMENT =			0x0CF5,
			// Multitexture
			GL_TEXTURE0 =				0x84C0,
			GL_MAX_TEXTURE_IMAGE_UNITS =		0x8872,
			GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS =	0x8B4C,
			// Buffer objects
			GL_ARRAY_BUFFER =			0x8892,
			GL_ELEMENT_ARRAY_BUFFER =		0x8893,
			GL_STREAM_DRAW =			0x88E0,
			GL_STATIC_DRAW =			0x88E4,
			GL_MAX_VERTEX_ATTRIBS =			0x8869,
			// Render targets
			GL_FRAMEBUFFER =			0x8D40,
			GL_READ_FRAMEBUFFER =			0x8CA8,
			GL_DRAW_FRAMEBUFFER =			0x8CA9,
			GL_RENDERBUFFER =			0x8D41,
			GL_MAX_DRAW_BUFFERS =			0x8824,
			// Draw Primitives
			GL_LINES =				0x0001,
			GL_LINE_STRIP =				0x0003,
			GL_TRIANGLES =				0x0004,
			GL_TRIANGLE_STRIP =			0x0005,
			// Query Objects
			GL_QUERY_RESULT =			0x8866,
			GL_QUERY_RESULT_AVAILABLE =		0x8867,
			GL_SAMPLES_PASSED =			0x8914,
			// Multisampling
			GL_MULTISAMPLE =			0x809D,
			GL_MAX_SAMPLES =			0x8D57,
			GL_SAMPLE_MASK =			0x8E51,
			// 3.2 Core Profile
			GL_NUM_EXTENSIONS =			0x821D,
			// Source Enum Values
			GL_DEBUG_SOURCE_API_ARB =		0x8246,
			GL_DEBUG_SOURCE_WINDOW_SYSTEM_ARB =	0x8247,
			GL_DEBUG_SOURCE_SHADER_COMPILER_ARB =	0x8248,
			GL_DEBUG_SOURCE_THIRD_PARTY_ARB =	0x8249,
			GL_DEBUG_SOURCE_APPLICATION_ARB =	0x824A,
			GL_DEBUG_SOURCE_OTHER_ARB =		0x824B,
			// Type Enum Values
			GL_DEBUG_TYPE_ERROR_ARB =		0x824C,
			GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR_ARB =	0x824D,
			GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR_ARB =	0x824E,
			GL_DEBUG_TYPE_PORTABILITY_ARB =		0x824F,
			GL_DEBUG_TYPE_PERFORMANCE_ARB =		0x8250,
			GL_DEBUG_TYPE_OTHER_ARB =		0x8251,
			// Severity Enum Values
			GL_DEBUG_SEVERITY_HIGH_ARB =		0x9146,
			GL_DEBUG_SEVERITY_MEDIUM_ARB =		0x9147,
			GL_DEBUG_SEVERITY_LOW_ARB =		0x9148,
			GL_DEBUG_SEVERITY_NOTIFICATION_ARB =	0x826B
		}

		// Entry Points

		/* BEGIN GET FUNCTIONS */

		private delegate IntPtr GetString(GLenum pname);
		private GetString INTERNAL_glGetString;
		private string glGetString(GLenum pname)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetString(pname));
			}
		}

		private delegate void GetIntegerv(GLenum pname, out int param);
		private GetIntegerv glGetIntegerv;

		/* END GET FUNCTIONS */

		/* BEGIN ENABLE/DISABLE FUNCTIONS */

		private delegate void Enable(GLenum cap);
		private Enable glEnable;

		private delegate void Disable(GLenum cap);
		private Disable glDisable;

		/* END ENABLE/DISABLE FUNCTIONS */

		/* BEGIN VIEWPORT/SCISSOR FUNCTIONS */

		private delegate void G_Viewport(
			int x,
			int y,
			int width,
			int height
		);
		private G_Viewport glViewport;

		private delegate void DepthRange(
			double near_val,
			double far_val
		);
		private DepthRange glDepthRange;

		private delegate void DepthRangef(
			float near_val,
			float far_val
		);
		private DepthRangef glDepthRangef;

		private delegate void Scissor(
			int x,
			int y,
			int width,
			int height
		);
		private Scissor glScissor;

		/* END VIEWPORT/SCISSOR FUNCTIONS */

		/* BEGIN BLEND STATE FUNCTIONS */

		private delegate void BlendColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private BlendColor glBlendColor;

		private delegate void BlendFuncSeparate(
			GLenum srcRGB,
			GLenum dstRGB,
			GLenum srcAlpha,
			GLenum dstAlpha
		);
		private BlendFuncSeparate glBlendFuncSeparate;

		private delegate void BlendEquationSeparate(
			GLenum modeRGB,
			GLenum modeAlpha
		);
		private BlendEquationSeparate glBlendEquationSeparate;

		private delegate void ColorMask(
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMask glColorMask;

		private delegate void ColorMaskIndexedEXT(
			uint buf,
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMaskIndexedEXT glColorMaskIndexedEXT;

		private delegate void SampleMaski(uint maskNumber, uint mask);
		private SampleMaski glSampleMaski;

		/* END BLEND STATE FUNCTIONS */

		/* BEGIN DEPTH/STENCIL STATE FUNCTIONS */

		private delegate void DepthMask(bool flag);
		private DepthMask glDepthMask;

		private delegate void DepthFunc(GLenum func);
		private DepthFunc glDepthFunc;

		private delegate void StencilMask(int mask);
		private StencilMask glStencilMask;

		private delegate void StencilFuncSeparate(
			GLenum face,
			GLenum func,
			int reference,
			int mask
		);
		private StencilFuncSeparate glStencilFuncSeparate;

		private delegate void StencilOpSeparate(
			GLenum face,
			GLenum sfail,
			GLenum dpfail,
			GLenum dppass
		);
		private StencilOpSeparate glStencilOpSeparate;

		private delegate void StencilFunc(
			GLenum fail,
			int reference,
			int mask
		);
		private StencilFunc glStencilFunc;

		private delegate void StencilOp(
			GLenum fail,
			GLenum zfail,
			GLenum zpass
		);
		private StencilOp glStencilOp;

		/* END DEPTH/STENCIL STATE FUNCTIONS */

		/* BEGIN RASTERIZER STATE FUNCTIONS */

		private delegate void FrontFace(GLenum mode);
		private FrontFace glFrontFace;

		private delegate void PolygonMode(GLenum face, GLenum mode);
		private PolygonMode glPolygonMode;

		private delegate void PolygonOffset(float factor, float units);
		private PolygonOffset glPolygonOffset;

		/* END RASTERIZER STATE FUNCTIONS */

		/* BEGIN TEXTURE FUNCTIONS */

		private delegate void GenTextures(int n, out uint textures);
		private GenTextures glGenTextures;

		private delegate void DeleteTextures(
			int n,
			ref uint textures
		);
		private DeleteTextures glDeleteTextures;

		private delegate void G_BindTexture(GLenum target, uint texture);
		private G_BindTexture glBindTexture;

		private delegate void TexImage2D(
			GLenum target,
			int level,
			int internalFormat,
			int width,
			int height,
			int border,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TexImage2D glTexImage2D;

		private delegate void TexSubImage2D(
			GLenum target,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TexSubImage2D glTexSubImage2D;

		private delegate void CompressedTexImage2D(
			GLenum target,
			int level,
			int internalFormat,
			int width,
			int height,
			int border,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTexImage2D glCompressedTexImage2D;

		private delegate void CompressedTexSubImage2D(
			GLenum target,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTexSubImage2D glCompressedTexSubImage2D;

		private delegate void TexImage3D(
			GLenum target,
			int level,
			int internalFormat,
			int width,
			int height,
			int depth,
			int border,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TexImage3D glTexImage3D;

		private delegate void TexSubImage3D(
			GLenum target,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TexSubImage3D glTexSubImage3D;

		private delegate void GetTexImage(
			GLenum target,
			int level,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private GetTexImage glGetTexImage;

		private delegate void TexParameteri(
			GLenum target,
			GLenum pname,
			int param
		);
		private TexParameteri glTexParameteri;

		private delegate void TexParameterf(
			GLenum target,
			GLenum pname,
			float param
		);
		private TexParameterf glTexParameterf;

		private delegate void ActiveTexture(GLenum texture);
		private ActiveTexture glActiveTexture;

		private delegate void PixelStorei(GLenum pname, int param);
		private PixelStorei glPixelStorei;

		/* END TEXTURE FUNCTIONS */

		/* BEGIN BUFFER FUNCTIONS */

		private delegate void GenBuffers(int n, out uint buffers);
		private GenBuffers glGenBuffers;

		private delegate void DeleteBuffers(
			int n,
			ref uint buffers
		);
		private DeleteBuffers glDeleteBuffers;

		private delegate void BindBuffer(GLenum target, uint buffer);
		private BindBuffer glBindBuffer;

		private delegate void BufferData(
			GLenum target,
			IntPtr size,
			IntPtr data,
			GLenum usage
		);
		private BufferData glBufferData;

		private delegate void BufferSubData(
			GLenum target,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private BufferSubData glBufferSubData;

		private delegate void GetBufferSubData(
			GLenum target,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private GetBufferSubData glGetBufferSubData;

		/* END BUFFER FUNCTIONS */

		/* BEGIN CLEAR FUNCTIONS */

		private delegate void ClearColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private ClearColor glClearColor;

		private delegate void ClearDepth(double depth);
		private ClearDepth glClearDepth;

		private delegate void ClearDepthf(float depth);
		private ClearDepthf glClearDepthf;

		private delegate void ClearStencil(int s);
		private ClearStencil glClearStencil;

		private delegate void G_Clear(GLenum mask);
		private G_Clear glClear;

		/* END CLEAR FUNCTIONS */

		/* BEGIN FRAMEBUFFER FUNCTIONS */

		private delegate void DrawBuffers(int n, GLenum[] bufs);
		private DrawBuffers glDrawBuffers;

		private delegate void ReadPixels(
			int x,
			int y,
			int width,
			int height,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private ReadPixels glReadPixels;

		private delegate void GenerateMipmap(GLenum target);
		private GenerateMipmap glGenerateMipmap;

		private delegate void GenFramebuffers(
			int n,
			out uint framebuffers
		);
		private GenFramebuffers glGenFramebuffers;

		private delegate void DeleteFramebuffers(
			int n,
			ref uint framebuffers
		);
		private DeleteFramebuffers glDeleteFramebuffers;

		private delegate void G_BindFramebuffer(
			GLenum target,
			uint framebuffer
		);
		private G_BindFramebuffer glBindFramebuffer;

		private delegate void FramebufferTexture2D(
			GLenum target,
			GLenum attachment,
			GLenum textarget,
			uint texture,
			int level
		);
		private FramebufferTexture2D glFramebufferTexture2D;

		private delegate void FramebufferRenderbuffer(
			GLenum target,
			GLenum attachment,
			GLenum renderbuffertarget,
			uint renderbuffer
		);
		private FramebufferRenderbuffer glFramebufferRenderbuffer;

		private delegate void BlitFramebuffer(
			int srcX0,
			int srcY0,
			int srcX1,
			int srcY1,
			int dstX0,
			int dstY0,
			int dstX1,
			int dstY1,
			GLenum mask,
			GLenum filter
		);
		private BlitFramebuffer glBlitFramebuffer;

		private delegate void GenRenderbuffers(
			int n,
			out uint renderbuffers
		);
		private GenRenderbuffers glGenRenderbuffers;

		private delegate void DeleteRenderbuffers(
			int n,
			ref uint renderbuffers
		);
		private DeleteRenderbuffers glDeleteRenderbuffers;

		private delegate void BindRenderbuffer(
			GLenum target,
			uint renderbuffer
		);
		private BindRenderbuffer glBindRenderbuffer;

		private delegate void RenderbufferStorage(
			GLenum target,
			GLenum internalformat,
			int width,
			int height
		);
		private RenderbufferStorage glRenderbufferStorage;

		private delegate void RenderbufferStorageMultisample(
			GLenum target,
			int samples,
			GLenum internalformat,
			int width,
			int height
		);
		private RenderbufferStorageMultisample glRenderbufferStorageMultisample;

		/* END FRAMEBUFFER FUNCTIONS */

		/* BEGIN VERTEX ATTRIBUTE FUNCTIONS */

		private delegate void VertexAttribPointer(
			int index,
			int size,
			GLenum type,
			bool normalized,
			int stride,
			IntPtr pointer
		);
		private VertexAttribPointer glVertexAttribPointer;

		private delegate void VertexAttribDivisor(
			int index,
			int divisor
		);
		private VertexAttribDivisor glVertexAttribDivisor;

		private delegate void EnableVertexAttribArray(int index);
		private EnableVertexAttribArray glEnableVertexAttribArray;

		private delegate void DisableVertexAttribArray(int index);
		private DisableVertexAttribArray glDisableVertexAttribArray;

		/* END VERTEX ATTRIBUTE FUNCTIONS */

		/* BEGIN DRAWING FUNCTIONS */

		private delegate void DrawElementsInstanced(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount
		);
		private DrawElementsInstanced glDrawElementsInstanced;

		private delegate void DrawRangeElements(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices
		);
		private DrawRangeElements glDrawRangeElements;

		private delegate void DrawElements(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices
		);
		private DrawElements glDrawElements;

		private delegate void DrawArrays(
			GLenum mode,
			int first,
			int count
		);
		private DrawArrays glDrawArrays;

		/* END DRAWING FUNCTIONS */

		/* BEGIN QUERY FUNCTIONS */

		private delegate void GenQueries(int n, out uint ids);
		private GenQueries glGenQueries;

		private delegate void DeleteQueries(int n, ref uint ids);
		private DeleteQueries glDeleteQueries;

		private delegate void BeginQuery(GLenum target, uint id);
		private BeginQuery glBeginQuery;

		private delegate void EndQuery(GLenum target);
		private EndQuery glEndQuery;

		private delegate void GetQueryObjectuiv(
			uint id,
			GLenum pname,
			out uint param
		);
		private GetQueryObjectuiv glGetQueryObjectuiv;

		/* END QUERY FUNCTIONS */

		/* BEGIN 3.2 CORE PROFILE FUNCTIONS */

		private delegate IntPtr GetStringi(GLenum pname, uint index);
		private GetStringi INTERNAL_glGetStringi;
		private string glGetStringi(GLenum pname, uint index)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetStringi(pname, index));
			}
		}

		private delegate void GenVertexArrays(int n, out uint arrays);
		private GenVertexArrays glGenVertexArrays;

		private delegate void DeleteVertexArrays(int n, ref uint arrays);
		private DeleteVertexArrays glDeleteVertexArrays;

		private delegate void BindVertexArray(uint array);
		private BindVertexArray glBindVertexArray;

		/* END 3.2 CORE PROFILE FUNCTIONS */

#if DEBUG
		/* BEGIN DEBUG OUTPUT FUNCTIONS */

		private delegate void DebugMessageCallback(
			DebugProc callback,
			IntPtr userParam
		);
		private DebugMessageCallback glDebugMessageCallbackARB;

		private delegate void DebugMessageControl(
			GLenum source,
			GLenum type,
			GLenum severity,
			int count,
			IntPtr ids, // const GLuint*
			bool enabled
		);
		private DebugMessageControl glDebugMessageControlARB;

		// ARB_debug_output callback
		private delegate void DebugProc(
			GLenum source,
			GLenum type,
			uint id,
			GLenum severity,
			int length,
			IntPtr message, // const GLchar*
			IntPtr userParam // const GLvoid*
		);
		private DebugProc DebugCall = DebugCallback;
		private static void DebugCallback(
			GLenum source,
			GLenum type,
			uint id,
			GLenum severity,
			int length,
			IntPtr message, // const GLchar*
			IntPtr userParam // const GLvoid*
		) {
			System.Console.WriteLine(
				"{0}\n\tSource: {1}\n\tType: {2}\n\tSeverity: {3}",
				Marshal.PtrToStringAnsi(message),
				source.ToString(),
				type.ToString(),
				severity.ToString()
			);
			if (type == GLenum.GL_DEBUG_TYPE_ERROR_ARB)
			{
				throw new Exception("ARB_debug_output found an error.");
			}
		}

		/* END DEBUG OUTPUT FUNCTIONS */

		/* BEGIN STRING MARKER FUNCTIONS */

		private delegate void StringMarkerGREMEDY(int length, byte[] chars);
		private StringMarkerGREMEDY glStringMarkerGREMEDY;

		/* END STRING MARKER FUNCTIONS */
#endif

		private void LoadGLEntryPoints()
		{
			string baseErrorString;
			if (useES2)
			{
				baseErrorString = "OpenGL ES 2.0";
			}
			else
			{
				baseErrorString = "OpenGL 2.1";
			}
			baseErrorString += " support is required!";

			/* Basic entry points. If you don't have these, you're screwed. */
			try
			{
				INTERNAL_glGetString = (GetString) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetString"),
					typeof(GetString)
				);
				glGetIntegerv = (GetIntegerv) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetIntegerv"),
					typeof(GetIntegerv)
				);
				glEnable = (Enable) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEnable"),
					typeof(Enable)
				);
				glDisable = (Disable) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDisable"),
					typeof(Disable)
				);
				glViewport = (G_Viewport) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glViewport"),
					typeof(G_Viewport)
				);
				glScissor = (Scissor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glScissor"),
					typeof(Scissor)
				);
				glBlendColor = (BlendColor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendColor"),
					typeof(BlendColor)
				);
				glBlendFuncSeparate = (BlendFuncSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendFuncSeparate"),
					typeof(BlendFuncSeparate)
				);
				glBlendEquationSeparate = (BlendEquationSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendEquationSeparate"),
					typeof(BlendEquationSeparate)
				);
				glColorMask = (ColorMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glColorMask"),
					typeof(ColorMask)
				);
				glDepthMask = (DepthMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthMask"),
					typeof(DepthMask)
				);
				glDepthFunc = (DepthFunc) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthFunc"),
					typeof(DepthFunc)
				);
				glStencilMask = (StencilMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilMask"),
					typeof(StencilMask)
				);
				glStencilFuncSeparate = (StencilFuncSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilFuncSeparate"),
					typeof(StencilFuncSeparate)
				);
				glStencilOpSeparate = (StencilOpSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilOpSeparate"),
					typeof(StencilOpSeparate)
				);
				glStencilFunc = (StencilFunc) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilFunc"),
					typeof(StencilFunc)
				);
				glStencilOp = (StencilOp) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilOp"),
					typeof(StencilOp)
				);
				glFrontFace = (FrontFace) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glFrontFace"),
					typeof(FrontFace)
				);
				glPolygonOffset = (PolygonOffset) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPolygonOffset"),
					typeof(PolygonOffset)
				);
				glGenTextures = (GenTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenTextures"),
					typeof(GenTextures)
				);
				glDeleteTextures = (DeleteTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteTextures"),
					typeof(DeleteTextures)
				);
				glBindTexture = (G_BindTexture) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindTexture"),
					typeof(G_BindTexture)
				);
				glTexImage2D = (TexImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTexImage2D"),
					typeof(TexImage2D)
				);
				glTexSubImage2D = (TexSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTexSubImage2D"),
					typeof(TexSubImage2D)
				);
				glCompressedTexImage2D = (CompressedTexImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTexImage2D"),
					typeof(CompressedTexImage2D)
				);
				glCompressedTexSubImage2D = (CompressedTexSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTexSubImage2D"),
					typeof(CompressedTexSubImage2D)
				);
				glTexParameteri = (TexParameteri) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTexParameteri"),
					typeof(TexParameteri)
				);
				glTexParameterf = (TexParameterf) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTexParameterf"),
					typeof(TexParameterf)
				);
				glActiveTexture = (ActiveTexture) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glActiveTexture"),
					typeof(ActiveTexture)
				);
				glPixelStorei = (PixelStorei) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPixelStorei"),
					typeof(PixelStorei)
				);
				glGenBuffers = (GenBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenBuffers"),
					typeof(GenBuffers)
				);
				glDeleteBuffers = (DeleteBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteBuffers"),
					typeof(DeleteBuffers)
				);
				glBindBuffer = (BindBuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindBuffer"),
					typeof(BindBuffer)
				);
				glBufferData = (BufferData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBufferData"),
					typeof(BufferData)
				);
				glBufferSubData = (BufferSubData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBufferSubData"),
					typeof(BufferSubData)
				);
				glClearColor = (ClearColor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearColor"),
					typeof(ClearColor)
				);
				glClearStencil = (ClearStencil) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearStencil"),
					typeof(ClearStencil)
				);
				glClear = (G_Clear) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClear"),
					typeof(G_Clear)
				);
				glDrawBuffers = (DrawBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawBuffers"),
					typeof(DrawBuffers)
				);
				glReadPixels = (ReadPixels) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glReadPixels"),
					typeof(ReadPixels)
				);
				glVertexAttribPointer = (VertexAttribPointer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribPointer"),
					typeof(VertexAttribPointer)
				);
				glEnableVertexAttribArray = (EnableVertexAttribArray) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEnableVertexAttribArray"),
					typeof(EnableVertexAttribArray)
				);
				glDisableVertexAttribArray = (DisableVertexAttribArray) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDisableVertexAttribArray"),
					typeof(DisableVertexAttribArray)
				);
				glDrawArrays = (DrawArrays) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawArrays"),
					typeof(DrawArrays)
				);
			}
			catch
			{
				throw new NoSuitableGraphicsDeviceException(baseErrorString);
			}

			/* DrawRangeElements is better, but some ES2 targets don't have it. */
			IntPtr ep = SDL.SDL_GL_GetProcAddress("glDrawRangeElements");
			if (ep != IntPtr.Zero)
			{
				glDrawRangeElements = (DrawRangeElements) Marshal.GetDelegateForFunctionPointer(
					ep,
					typeof(DrawRangeElements)
				);
			}
			else
			{
				ep = SDL.SDL_GL_GetProcAddress("glDrawElements");
				if (ep == IntPtr.Zero)
				{
					throw new NoSuitableGraphicsDeviceException(baseErrorString);
				}
				glDrawElements = (DrawElements) Marshal.GetDelegateForFunctionPointer(
					ep,
					typeof(DrawElements)
				);
				glDrawRangeElements = DrawRangeElementsUnchecked;
			}

			/* These functions are NOT supported in ES.
			 * NVIDIA or desktop ES might, but real scenarios where you need ES
			 * will certainly not have these.
			 * -flibit
			 */
			if (useES2)
			{
				ep = SDL.SDL_GL_GetProcAddress("glPolygonMode");
				if (ep != IntPtr.Zero)
				{
					glPolygonMode = (PolygonMode) Marshal.GetDelegateForFunctionPointer(
						ep,
						typeof(PolygonMode)
					);
				}
				else
				{
					glPolygonMode = PolygonModeESError;
				}
				ep = SDL.SDL_GL_GetProcAddress("glGetTexImage");
				if (ep != IntPtr.Zero)
				{
					glGetTexImage = (GetTexImage) Marshal.GetDelegateForFunctionPointer(
						ep,
						typeof(GetTexImage)
					);
				}
				else
				{
					glGetTexImage = GetTexImageESError;
				}
				ep = SDL.SDL_GL_GetProcAddress("glGetBufferSubData");
				if (ep != IntPtr.Zero)
				{
					glGetBufferSubData = (GetBufferSubData) Marshal.GetDelegateForFunctionPointer(
						ep,
						typeof(GetBufferSubData)
					);
				}
				else
				{
					glGetBufferSubData = GetBufferSubDataESError;
				}
			}
			else
			{
				try
				{
					glPolygonMode = (PolygonMode) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glPolygonMode"),
						typeof(PolygonMode)
					);
					glGetTexImage = (GetTexImage) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGetTexImage"),
						typeof(GetTexImage)
					);
					glGetBufferSubData = (GetBufferSubData) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGetBufferSubData"),
						typeof(GetBufferSubData)
					);
				}
				catch
				{
					throw new NoSuitableGraphicsDeviceException(baseErrorString);
				}
			}

			/* We need _some_ form of depth range, ES... */
			IntPtr drPtr = SDL.SDL_GL_GetProcAddress("glDepthRange");
			if (drPtr != IntPtr.Zero)
			{
				glDepthRange = (DepthRange) Marshal.GetDelegateForFunctionPointer(
					drPtr,
					typeof(DepthRange)
				);
			}
			else
			{
				drPtr = SDL.SDL_GL_GetProcAddress("glDepthRangef");
				if (drPtr == IntPtr.Zero)
				{
					throw new NoSuitableGraphicsDeviceException(baseErrorString);
				}
				glDepthRangef = (DepthRangef) Marshal.GetDelegateForFunctionPointer(
					drPtr,
					typeof(DepthRangef)
				);
				glDepthRange = DepthRangeFloat;
			}
			drPtr = SDL.SDL_GL_GetProcAddress("glClearDepth");
			if (drPtr != IntPtr.Zero)
			{
				glClearDepth = (ClearDepth) Marshal.GetDelegateForFunctionPointer(
					drPtr,
					typeof(ClearDepth)
				);
			}
			else
			{
				drPtr = SDL.SDL_GL_GetProcAddress("glClearDepthf");
				if (drPtr == IntPtr.Zero)
				{
					throw new NoSuitableGraphicsDeviceException(baseErrorString);
				}
				glClearDepthf = (ClearDepthf) Marshal.GetDelegateForFunctionPointer(
					drPtr,
					typeof(ClearDepthf)
				);
				glClearDepth = ClearDepthFloat;
			}

			/* Silently fail if using GLES. You didn't need these, right...? >_> */
			try
			{
				glTexImage3D = (TexImage3D) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glTexImage3D", "OES"),
					typeof(TexImage3D)
				);
				glTexSubImage3D = (TexSubImage3D) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glTexSubImage3D", "OES"),
					typeof(TexSubImage3D)
				);
				glGenQueries = (GenQueries) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenQueries"),
					typeof(GenQueries)
				);
				glDeleteQueries = (DeleteQueries) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteQueries"),
					typeof(DeleteQueries)
				);
				glBeginQuery = (BeginQuery) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBeginQuery"),
					typeof(BeginQuery)
				);
				glEndQuery = (EndQuery) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEndQuery"),
					typeof(EndQuery)
				);
				glGetQueryObjectuiv = (GetQueryObjectuiv) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetQueryObjectuiv"),
					typeof(GetQueryObjectuiv)
				);
			}
			catch
			{
				if (useES2)
				{
					System.Console.WriteLine("Some non-ES functions failed to load. Beware...");
				}
				else
				{
					throw new NoSuitableGraphicsDeviceException(baseErrorString);
				}
			}

			/* ARB_framebuffer_object. We're flexible, but not _that_ flexible. */
			try
			{
				glGenFramebuffers = (GenFramebuffers) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glGenFramebuffers"),
					typeof(GenFramebuffers)
				);
				glDeleteFramebuffers = (DeleteFramebuffers) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glDeleteFramebuffers"),
					typeof(DeleteFramebuffers)
				);
				glBindFramebuffer = (G_BindFramebuffer) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glBindFramebuffer"),
					typeof(G_BindFramebuffer)
				);
				glFramebufferTexture2D = (FramebufferTexture2D) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glFramebufferTexture2D"),
					typeof(FramebufferTexture2D)
				);
				glFramebufferRenderbuffer = (FramebufferRenderbuffer) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glFramebufferRenderbuffer"),
					typeof(FramebufferRenderbuffer)
				);
				glGenerateMipmap = (GenerateMipmap) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glGenerateMipmap"),
					typeof(GenerateMipmap)
				);
				glGenRenderbuffers = (GenRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glGenRenderbuffers"),
					typeof(GenRenderbuffers)
				);
				glDeleteRenderbuffers = (DeleteRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glDeleteRenderbuffers"),
					typeof(DeleteRenderbuffers)
				);
				glBindRenderbuffer = (BindRenderbuffer) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glBindRenderbuffer"),
					typeof(BindRenderbuffer)
				);
				glRenderbufferStorage = (RenderbufferStorage) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glRenderbufferStorage"),
					typeof(RenderbufferStorage)
				);
			}
			catch
			{
				throw new NoSuitableGraphicsDeviceException("OpenGL framebuffer support is required!");
			}

			/* EXT_framebuffer_blit (or ARB_framebuffer_object) is needed by the faux-backbuffer. */
			supportsFauxBackbuffer = true;
			try
			{
				glBlitFramebuffer = (BlitFramebuffer) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glBlitFramebuffer"),
					typeof(BlitFramebuffer)
				);
			}
			catch
			{
				supportsFauxBackbuffer = false;
			}

			/* ARB_instanced_arrays/ARB_draw_instanced are almost optional. */
			SupportsHardwareInstancing = true;
			try
			{
				glVertexAttribDivisor = (VertexAttribDivisor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribDivisor"),
					typeof(VertexAttribDivisor)
				);
				glDrawElementsInstanced = (DrawElementsInstanced) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawElementsInstanced"),
					typeof(DrawElementsInstanced)
				);
			}
			catch
			{
				SupportsHardwareInstancing = false;
			}

			/* EXT_draw_buffers2 is probably used by nobody. */
			try
			{
				glColorMaskIndexedEXT = (ColorMaskIndexedEXT) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glColorMaskIndexedEXT"),
					typeof(ColorMaskIndexedEXT)
				);
			}
			catch
			{
				// FIXME: SupportsIndependentWriteMasks? -flibit
			}

			/* EXT_framebuffer_multisample/ARB_texture_multisample is glitter -flibit */
			supportsMultisampling = true;
			try
			{
				glRenderbufferStorageMultisample = (RenderbufferStorageMultisample) Marshal.GetDelegateForFunctionPointer(
					TryGetEPEXT("glRenderbufferStorageMultisample"),
					typeof(RenderbufferStorageMultisample)
				);
				glSampleMaski = (SampleMaski) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSampleMaski"),
					typeof(SampleMaski)
				);
			}
			catch
			{
				supportsMultisampling = false;
			}

			if (useCoreProfile)
			{
				try
				{
					INTERNAL_glGetStringi = (GetStringi) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGetStringi"),
						typeof(GetStringi)
					);
					glGenVertexArrays = (GenVertexArrays) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGenVertexArrays"),
						typeof(GenVertexArrays)
					);
					glDeleteVertexArrays = (DeleteVertexArrays) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glDeleteVertexArrays"),
						typeof(DeleteVertexArrays)
					);
					glBindVertexArray = (BindVertexArray) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glBindVertexArray"),
						typeof(BindVertexArray)
					);
				}
				catch
				{
					throw new NoSuitableGraphicsDeviceException("OpenGL 3.2 support is required!");
				}
			}

#if DEBUG
			/* ARB_debug_output, for debug contexts */
			IntPtr messageCallback = SDL.SDL_GL_GetProcAddress("glDebugMessageCallbackARB");
			IntPtr messageControl = SDL.SDL_GL_GetProcAddress("glDebugMessageControlARB");
			if (messageCallback == IntPtr.Zero || messageControl == IntPtr.Zero)
			{
				System.Console.WriteLine("ARB_debug_output not supported!");
			}
			else
			{
				glDebugMessageCallbackARB = (DebugMessageCallback) Marshal.GetDelegateForFunctionPointer(
					messageCallback,
					typeof(DebugMessageCallback)
				);
				glDebugMessageControlARB = (DebugMessageControl) Marshal.GetDelegateForFunctionPointer(
					messageControl,
					typeof(DebugMessageControl)
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					0,
					IntPtr.Zero,
					true
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER_ARB,
					GLenum.GL_DEBUG_SEVERITY_LOW_ARB,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER_ARB,
					GLenum.GL_DEBUG_SEVERITY_NOTIFICATION_ARB,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageCallbackARB(DebugCall, IntPtr.Zero);
			}

			/* GREMEDY_string_marker, for apitrace */
			IntPtr stringMarkerCallback = SDL.SDL_GL_GetProcAddress("glStringMarkerGREMEDY");
			if (stringMarkerCallback == IntPtr.Zero)
			{
				System.Console.WriteLine("GREMEDY_string_marker not supported!");
			}
			else
			{
				glStringMarkerGREMEDY = (StringMarkerGREMEDY) Marshal.GetDelegateForFunctionPointer(
					stringMarkerCallback,
					typeof(StringMarkerGREMEDY)
				);
			}
#endif
		}

		private IntPtr TryGetEPEXT(string ep, string ext = "EXT")
		{
			IntPtr result = SDL.SDL_GL_GetProcAddress(ep);
			if (result == IntPtr.Zero)
			{
				result = SDL.SDL_GL_GetProcAddress(ep + ext);
			}
			return result;
		}

		private void DrawRangeElementsUnchecked(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices
		) {
			glDrawElements(
				mode,
				count,
				type,
				indices
			);
		}

		private void DepthRangeFloat(double near, double far)
		{
			glDepthRangef((float) near, (float) far);
		}

		private void ClearDepthFloat(double depth)
		{
			glClearDepthf((float) depth);
		}

		private void PolygonModeESError(GLenum face, GLenum mode)
		{
			throw new NotSupportedException("glPolygonMode is not available in ES!");
		}

		private void GetTexImageESError(
			GLenum target,
			int level,
			GLenum format,
			GLenum type,
			IntPtr pixels
		) {
			throw new NotSupportedException("glGetTexImage is not available in ES!");
		}

		private void GetBufferSubDataESError(
			GLenum target,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		) {
			throw new NotSupportedException("glGetBufferSubData is not available in ES!");
		}

		#endregion
	}
}
