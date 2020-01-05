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
			GL_UNSIGNED_SHORT_4_4_4_4_REV =		0x8365,
			GL_UNSIGNED_SHORT_5_5_5_1_REV =		0x8366,
			GL_UNSIGNED_INT_2_10_10_10_REV =	0x8368,
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
			// Points
			GL_POINT_SPRITE =			0x8861,
			GL_COORD_REPLACE =			0x8862,
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
			GL_ALPHA =				0x1906,
			GL_RGB =				0x1907,
			GL_RGBA =				0x1908,
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
			GL_POINTS =				0x0000,
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
			GL_DEBUG_SOURCE_API =			0x8246,
			GL_DEBUG_SOURCE_WINDOW_SYSTEM =		0x8247,
			GL_DEBUG_SOURCE_SHADER_COMPILER =	0x8248,
			GL_DEBUG_SOURCE_THIRD_PARTY =		0x8249,
			GL_DEBUG_SOURCE_APPLICATION =		0x824A,
			GL_DEBUG_SOURCE_OTHER =			0x824B,
			// Type Enum Values
			GL_DEBUG_TYPE_ERROR =			0x824C,
			GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR =	0x824D,
			GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR =	0x824E,
			GL_DEBUG_TYPE_PORTABILITY =		0x824F,
			GL_DEBUG_TYPE_PERFORMANCE =		0x8250,
			GL_DEBUG_TYPE_OTHER =			0x8251,
			// Severity Enum Values
			GL_DEBUG_SEVERITY_HIGH =		0x9146,
			GL_DEBUG_SEVERITY_MEDIUM =		0x9147,
			GL_DEBUG_SEVERITY_LOW =			0x9148,
			GL_DEBUG_SEVERITY_NOTIFICATION =	0x826B
		}

		// Entry Points

		/* BEGIN GET FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate IntPtr GetString(GLenum pname);
		private GetString INTERNAL_glGetString;
		private string glGetString(GLenum pname)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetString(pname));
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetIntegerv(GLenum pname, out int param);
		private GetIntegerv glGetIntegerv;

		/* END GET FUNCTIONS */

		/* BEGIN ENABLE/DISABLE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void Enable(GLenum cap);
		private Enable glEnable;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void Disable(GLenum cap);
		private Disable glDisable;

		/* END ENABLE/DISABLE FUNCTIONS */

		/* BEGIN VIEWPORT/SCISSOR FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void G_Viewport(
			int x,
			int y,
			int width,
			int height
		);
		private G_Viewport glViewport;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DepthRange(
			double near_val,
			double far_val
		);
		private DepthRange glDepthRange;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DepthRangef(
			float near_val,
			float far_val
		);
		private DepthRangef glDepthRangef;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void Scissor(
			int x,
			int y,
			int width,
			int height
		);
		private Scissor glScissor;

		/* END VIEWPORT/SCISSOR FUNCTIONS */

		/* BEGIN BLEND STATE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BlendColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private BlendColor glBlendColor;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BlendFuncSeparate(
			GLenum srcRGB,
			GLenum dstRGB,
			GLenum srcAlpha,
			GLenum dstAlpha
		);
		private BlendFuncSeparate glBlendFuncSeparate;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BlendEquationSeparate(
			GLenum modeRGB,
			GLenum modeAlpha
		);
		private BlendEquationSeparate glBlendEquationSeparate;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ColorMask(
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMask glColorMask;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ColorMaski(
			uint buf,
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMaski glColorMaski;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void SampleMaski(uint maskNumber, uint mask);
		private SampleMaski glSampleMaski;

		/* END BLEND STATE FUNCTIONS */

		/* BEGIN DEPTH/STENCIL STATE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DepthMask(bool flag);
		private DepthMask glDepthMask;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DepthFunc(GLenum func);
		private DepthFunc glDepthFunc;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StencilMask(int mask);
		private StencilMask glStencilMask;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StencilFuncSeparate(
			GLenum face,
			GLenum func,
			int reference,
			int mask
		);
		private StencilFuncSeparate glStencilFuncSeparate;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StencilOpSeparate(
			GLenum face,
			GLenum sfail,
			GLenum dpfail,
			GLenum dppass
		);
		private StencilOpSeparate glStencilOpSeparate;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StencilFunc(
			GLenum fail,
			int reference,
			int mask
		);
		private StencilFunc glStencilFunc;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StencilOp(
			GLenum fail,
			GLenum zfail,
			GLenum zpass
		);
		private StencilOp glStencilOp;

		/* END DEPTH/STENCIL STATE FUNCTIONS */

		/* BEGIN RASTERIZER STATE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void FrontFace(GLenum mode);
		private FrontFace glFrontFace;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void PolygonMode(GLenum face, GLenum mode);
		private PolygonMode glPolygonMode;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void PolygonOffset(float factor, float units);
		private PolygonOffset glPolygonOffset;

		/* END RASTERIZER STATE FUNCTIONS */

		/* BEGIN TEXTURE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenTextures(int n, out uint textures);
		private GenTextures glGenTextures;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteTextures(
			int n,
			ref uint textures
		);
		private DeleteTextures glDeleteTextures;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void G_BindTexture(GLenum target, uint texture);
		private G_BindTexture glBindTexture;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetTexImage(
			GLenum target,
			int level,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private GetTexImage glGetTexImage;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TexParameteri(
			GLenum target,
			GLenum pname,
			int param
		);
		private TexParameteri glTexParameteri;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TexParameterf(
			GLenum target,
			GLenum pname,
			float param
		);
		private TexParameterf glTexParameterf;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TexEnvi(
			GLenum target,
			GLenum pname,
			int param
		);
		private TexEnvi glTexEnvi;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ActiveTexture(GLenum texture);
		private ActiveTexture glActiveTexture;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void PixelStorei(GLenum pname, int param);
		private PixelStorei glPixelStorei;

		/* END TEXTURE FUNCTIONS */

		/* BEGIN BUFFER FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenBuffers(int n, out uint buffers);
		private GenBuffers glGenBuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteBuffers(
			int n,
			ref uint buffers
		);
		private DeleteBuffers glDeleteBuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BindBuffer(GLenum target, uint buffer);
		private BindBuffer glBindBuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BufferData(
			GLenum target,
			IntPtr size,
			IntPtr data,
			GLenum usage
		);
		private BufferData glBufferData;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BufferSubData(
			GLenum target,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private BufferSubData glBufferSubData;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetBufferSubData(
			GLenum target,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private GetBufferSubData glGetBufferSubData;

		/* END BUFFER FUNCTIONS */

		/* BEGIN CLEAR FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ClearColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private ClearColor glClearColor;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ClearDepth(double depth);
		private ClearDepth glClearDepth;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ClearDepthf(float depth);
		private ClearDepthf glClearDepthf;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void ClearStencil(int s);
		private ClearStencil glClearStencil;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void G_Clear(GLenum mask);
		private G_Clear glClear;

		/* END CLEAR FUNCTIONS */

		/* BEGIN FRAMEBUFFER FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawBuffers(int n, IntPtr bufs);
		private DrawBuffers glDrawBuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenerateMipmap(GLenum target);
		private GenerateMipmap glGenerateMipmap;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenFramebuffers(
			int n,
			out uint framebuffers
		);
		private GenFramebuffers glGenFramebuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteFramebuffers(
			int n,
			ref uint framebuffers
		);
		private DeleteFramebuffers glDeleteFramebuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void G_BindFramebuffer(
			GLenum target,
			uint framebuffer
		);
		private G_BindFramebuffer glBindFramebuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void FramebufferTexture2D(
			GLenum target,
			GLenum attachment,
			GLenum textarget,
			uint texture,
			int level
		);
		private FramebufferTexture2D glFramebufferTexture2D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void FramebufferRenderbuffer(
			GLenum target,
			GLenum attachment,
			GLenum renderbuffertarget,
			uint renderbuffer
		);
		private FramebufferRenderbuffer glFramebufferRenderbuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InvalidateFramebuffer(
			GLenum target,
			int numAttachments,
			IntPtr attachments
		);
		InvalidateFramebuffer glInvalidateFramebuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenRenderbuffers(
			int n,
			out uint renderbuffers
		);
		private GenRenderbuffers glGenRenderbuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteRenderbuffers(
			int n,
			ref uint renderbuffers
		);
		private DeleteRenderbuffers glDeleteRenderbuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BindRenderbuffer(
			GLenum target,
			uint renderbuffer
		);
		private BindRenderbuffer glBindRenderbuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void RenderbufferStorage(
			GLenum target,
			GLenum internalformat,
			int width,
			int height
		);
		private RenderbufferStorage glRenderbufferStorage;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void VertexAttribPointer(
			int index,
			int size,
			GLenum type,
			bool normalized,
			int stride,
			IntPtr pointer
		);
		private VertexAttribPointer glVertexAttribPointer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void VertexAttribDivisor(
			int index,
			int divisor
		);
		private VertexAttribDivisor glVertexAttribDivisor;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void EnableVertexAttribArray(int index);
		private EnableVertexAttribArray glEnableVertexAttribArray;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DisableVertexAttribArray(int index);
		private DisableVertexAttribArray glDisableVertexAttribArray;

		/* END VERTEX ATTRIBUTE FUNCTIONS */

		/* BEGIN DRAWING FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawElementsInstanced(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount
		);
		private DrawElementsInstanced glDrawElementsInstanced;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawRangeElements(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices
		);
		private DrawRangeElements glDrawRangeElements;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawElementsInstancedBaseVertex(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount,
			int baseVertex
		);
		private DrawElementsInstancedBaseVertex glDrawElementsInstancedBaseVertex;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawRangeElementsBaseVertex(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices,
			int baseVertex
		);
		private DrawRangeElementsBaseVertex glDrawRangeElementsBaseVertex;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawElements(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices
		);
		private DrawElements glDrawElements;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DrawArrays(
			GLenum mode,
			int first,
			int count
		);
		private DrawArrays glDrawArrays;

		/* END DRAWING FUNCTIONS */

		/* BEGIN QUERY FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenQueries(int n, out uint ids);
		private GenQueries glGenQueries;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteQueries(int n, ref uint ids);
		private DeleteQueries glDeleteQueries;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BeginQuery(GLenum target, uint id);
		private BeginQuery glBeginQuery;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void EndQuery(GLenum target);
		private EndQuery glEndQuery;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetQueryObjectuiv(
			uint id,
			GLenum pname,
			out uint param
		);
		private GetQueryObjectuiv glGetQueryObjectuiv;

		/* END QUERY FUNCTIONS */

		/* BEGIN 3.2 CORE PROFILE FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate IntPtr GetStringi(GLenum pname, uint index);
		private GetStringi INTERNAL_glGetStringi;
		private string glGetStringi(GLenum pname, uint index)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetStringi(pname, index));
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GenVertexArrays(int n, out uint arrays);
		private GenVertexArrays glGenVertexArrays;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteVertexArrays(int n, ref uint arrays);
		private DeleteVertexArrays glDeleteVertexArrays;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BindVertexArray(uint array);
		private BindVertexArray glBindVertexArray;

		/* END 3.2 CORE PROFILE FUNCTIONS */

#if DEBUG
		/* BEGIN DEBUG OUTPUT FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DebugMessageCallback(
			IntPtr debugCallback,
			IntPtr userParam
		);
		private DebugMessageCallback glDebugMessageCallback;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DebugMessageControl(
			GLenum source,
			GLenum type,
			GLenum severity,
			int count,
			IntPtr ids, // const GLuint*
			bool enabled
		);
		private DebugMessageControl glDebugMessageControl;

		// ARB_debug_output/KHR_debug callback
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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
			string err = (
				Marshal.PtrToStringAnsi(message) +
				"\n\tSource: " +
				source.ToString() +
				"\n\tType: " +
				type.ToString() +
				"\n\tSeverity: " +
				severity.ToString()
			);
			if (type == GLenum.GL_DEBUG_TYPE_ERROR)
			{
				FNALoggerEXT.LogError(err);
				throw new InvalidOperationException(err);
			}
			FNALoggerEXT.LogWarn(err);
		}

		/* END DEBUG OUTPUT FUNCTIONS */

		/* BEGIN STRING MARKER FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void StringMarkerGREMEDY(int length, IntPtr chars);
		private StringMarkerGREMEDY glStringMarkerGREMEDY;

		/* END STRING MARKER FUNCTIONS */
#endif
		private void LoadGLGetString()
		{
			try
			{
				INTERNAL_glGetString = (GetString) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetString"),
					typeof(GetString)
				);
			}
			catch
			{
				throw new NoSuitableGraphicsDeviceException("GRAPHICS DRIVER IS EXTREMELY BROKEN!");
			}
		}

		private void LoadGLEntryPoints(string driver)
		{
			string baseErrorString;
			if (useES3)
			{
				baseErrorString = "OpenGL ES 3.0";
			}
			else
			{
				baseErrorString = "OpenGL 2.1";
			}
			baseErrorString += " support is required!";

			/* Basic entry points. If you don't have these, you're screwed. */
			try
			{
				glGetIntegerv = (GetIntegerv) GetProcAddress(
					"glGetIntegerv",
					typeof(GetIntegerv)
				);
				glEnable = (Enable) GetProcAddress(
					"glEnable",
					typeof(Enable)
				);
				glDisable = (Disable) GetProcAddress(
					"glDisable",
					typeof(Disable)
				);
				glViewport = (G_Viewport) GetProcAddress(
					"glViewport",
					typeof(G_Viewport)
				);
				glScissor = (Scissor) GetProcAddress(
					"glScissor",
					typeof(Scissor)
				);
				glBlendColor = (BlendColor) GetProcAddress(
					"glBlendColor",
					typeof(BlendColor)
				);
				glBlendFuncSeparate = (BlendFuncSeparate) GetProcAddress(
					"glBlendFuncSeparate",
					typeof(BlendFuncSeparate)
				);
				glBlendEquationSeparate = (BlendEquationSeparate) GetProcAddress(
					"glBlendEquationSeparate",
					typeof(BlendEquationSeparate)
				);
				glColorMask = (ColorMask) GetProcAddress(
					"glColorMask",
					typeof(ColorMask)
				);
				glDepthMask = (DepthMask) GetProcAddress(
					"glDepthMask",
					typeof(DepthMask)
				);
				glDepthFunc = (DepthFunc) GetProcAddress(
					"glDepthFunc",
					typeof(DepthFunc)
				);
				glStencilMask = (StencilMask) GetProcAddress(
					"glStencilMask",
					typeof(StencilMask)
				);
				glStencilFuncSeparate = (StencilFuncSeparate) GetProcAddress(
					"glStencilFuncSeparate",
					typeof(StencilFuncSeparate)
				);
				glStencilOpSeparate = (StencilOpSeparate) GetProcAddress(
					"glStencilOpSeparate",
					typeof(StencilOpSeparate)
				);
				glStencilFunc = (StencilFunc) GetProcAddress(
					"glStencilFunc",
					typeof(StencilFunc)
				);
				glStencilOp = (StencilOp) GetProcAddress(
					"glStencilOp",
					typeof(StencilOp)
				);
				glFrontFace = (FrontFace) GetProcAddress(
					"glFrontFace",
					typeof(FrontFace)
				);
				glPolygonOffset = (PolygonOffset) GetProcAddress(
					"glPolygonOffset",
					typeof(PolygonOffset)
				);
				glGenTextures = (GenTextures) GetProcAddress(
					"glGenTextures",
					typeof(GenTextures)
				);
				glDeleteTextures = (DeleteTextures) GetProcAddress(
					"glDeleteTextures",
					typeof(DeleteTextures)
				);
				glBindTexture = (G_BindTexture) GetProcAddress(
					"glBindTexture",
					typeof(G_BindTexture)
				);
				glTexImage2D = (TexImage2D) GetProcAddress(
					"glTexImage2D",
					typeof(TexImage2D)
				);
				glTexSubImage2D = (TexSubImage2D) GetProcAddress(
					"glTexSubImage2D",
					typeof(TexSubImage2D)
				);
				glCompressedTexImage2D = (CompressedTexImage2D) GetProcAddress(
					"glCompressedTexImage2D",
					typeof(CompressedTexImage2D)
				);
				glCompressedTexSubImage2D = (CompressedTexSubImage2D) GetProcAddress(
					"glCompressedTexSubImage2D",
					typeof(CompressedTexSubImage2D)
				);
				glTexParameteri = (TexParameteri) GetProcAddress(
					"glTexParameteri",
					typeof(TexParameteri)
				);
				glTexParameterf = (TexParameterf) GetProcAddress(
					"glTexParameterf",
					typeof(TexParameterf)
				);
				glActiveTexture = (ActiveTexture) GetProcAddress(
					"glActiveTexture",
					typeof(ActiveTexture)
				);
				glPixelStorei = (PixelStorei) GetProcAddress(
					"glPixelStorei",
					typeof(PixelStorei)
				);
				glGenBuffers = (GenBuffers) GetProcAddress(
					"glGenBuffers",
					typeof(GenBuffers)
				);
				glDeleteBuffers = (DeleteBuffers) GetProcAddress(
					"glDeleteBuffers",
					typeof(DeleteBuffers)
				);
				glBindBuffer = (BindBuffer) GetProcAddress(
					"glBindBuffer",
					typeof(BindBuffer)
				);
				glBufferData = (BufferData) GetProcAddress(
					"glBufferData",
					typeof(BufferData)
				);
				glBufferSubData = (BufferSubData) GetProcAddress(
					"glBufferSubData",
					typeof(BufferSubData)
				);
				glClearColor = (ClearColor) GetProcAddress(
					"glClearColor",
					typeof(ClearColor)
				);
				glClearStencil = (ClearStencil) GetProcAddress(
					"glClearStencil",
					typeof(ClearStencil)
				);
				glClear = (G_Clear) GetProcAddress(
					"glClear",
					typeof(G_Clear)
				);
				glDrawBuffers = (DrawBuffers) GetProcAddress(
					"glDrawBuffers",
					typeof(DrawBuffers)
				);
				glReadPixels = (ReadPixels) GetProcAddress(
					"glReadPixels",
					typeof(ReadPixels)
				);
				glVertexAttribPointer = (VertexAttribPointer) GetProcAddress(
					"glVertexAttribPointer",
					typeof(VertexAttribPointer)
				);
				glEnableVertexAttribArray = (EnableVertexAttribArray) GetProcAddress(
					"glEnableVertexAttribArray",
					typeof(EnableVertexAttribArray)
				);
				glDisableVertexAttribArray = (DisableVertexAttribArray) GetProcAddress(
					"glDisableVertexAttribArray",
					typeof(DisableVertexAttribArray)
				);
				glDrawArrays = (DrawArrays) GetProcAddress(
					"glDrawArrays",
					typeof(DrawArrays)
				);
			}
			catch (Exception e)
			{
				throw new NoSuitableGraphicsDeviceException(
					baseErrorString +
					"\nEntry Point: " + e.Message +
					"\n" + driver
				);
			}

			/* ARB_draw_elements_base_vertex is ideal! */
			IntPtr ep = SDL.SDL_GL_GetProcAddress("glDrawRangeElementsBaseVertex");
			if (ep == IntPtr.Zero)
			{
				ep = SDL.SDL_GL_GetProcAddress("glDrawRangeElementsBaseVertexOES");
			}
			supportsBaseVertex = ep != IntPtr.Zero && BUG_HACK_NOTANGLE;
			if (supportsBaseVertex)
			{
				glDrawRangeElementsBaseVertex = (DrawRangeElementsBaseVertex) Marshal.GetDelegateForFunctionPointer(
					ep,
					typeof(DrawRangeElementsBaseVertex)
				);
				glDrawRangeElements = (DrawRangeElements) GetProcAddress(
					"glDrawRangeElements",
					typeof(DrawRangeElements)
				);
			}
			else
			{
				/* DrawRangeElements is better, and ES3+ should have this */
				ep = SDL.SDL_GL_GetProcAddress("glDrawRangeElements");
				if (ep != IntPtr.Zero)
				{
					glDrawRangeElements = (DrawRangeElements) Marshal.GetDelegateForFunctionPointer(
						ep,
						typeof(DrawRangeElements)
					);
					glDrawRangeElementsBaseVertex = DrawRangeElementsNoBase;
				}
				else
				{
					try
					{
						glDrawElements = (DrawElements) GetProcAddress(
							"glDrawElements",
							typeof(DrawElements)
						);
					}
					catch (Exception e)
					{
						throw new NoSuitableGraphicsDeviceException(
							baseErrorString +
							"\nEntry Point: " + e.Message +
							"\n" + driver
						);
					}
					glDrawRangeElements = DrawRangeElementsUnchecked;
					glDrawRangeElementsBaseVertex = DrawRangeElementsNoBaseUnchecked;
				}
			}

			/* These functions are NOT supported in ES.
			 * NVIDIA or desktop ES might, but real scenarios where you need ES
			 * will certainly not have these.
			 * -flibit
			 */
			if (useES3)
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
				ep = SDL.SDL_GL_GetProcAddress("glTexEnvi");
				if (ep != IntPtr.Zero)
				{
					glTexEnvi = (TexEnvi) Marshal.GetDelegateForFunctionPointer(
						ep,
						typeof(TexEnvi)
					);
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
					glPolygonMode = (PolygonMode) GetProcAddress(
						"glPolygonMode",
						typeof(PolygonMode)
					);
					glGetTexImage = (GetTexImage) GetProcAddress(
						"glGetTexImage",
						typeof(GetTexImage)
					);
					glTexEnvi = (TexEnvi) GetProcAddress(
						"glTexEnvi",
						typeof(TexEnvi)
					);
					glGetBufferSubData = (GetBufferSubData) GetProcAddress(
						"glGetBufferSubData",
						typeof(GetBufferSubData)
					);
				}
				catch(Exception e)
				{
					throw new NoSuitableGraphicsDeviceException(
						baseErrorString +
						"\nEntry Point: " + e.Message +
						"\n" + driver
					);
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
				try
				{
					glDepthRangef = (DepthRangef) GetProcAddress(
						"glDepthRangef",
						typeof(DepthRangef)
					);
				}
				catch(Exception e)
				{
					throw new NoSuitableGraphicsDeviceException(
						baseErrorString +
						"\nEntry Point: " + e.Message +
						"\n" + driver
					);
				}
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
				try
				{
					glClearDepthf = (ClearDepthf) GetProcAddress(
						"glClearDepthf",
						typeof(ClearDepthf)
					);
				}
				catch (Exception e)
				{
					throw new NoSuitableGraphicsDeviceException(
						baseErrorString +
						"\nEntry Point: " + e.Message +
						"\n" + driver
					);
				}
				glClearDepth = ClearDepthFloat;
			}

			/* Silently fail if using GLES. You didn't need these, right...? >_> */
			try
			{
				glTexImage3D = (TexImage3D) GetProcAddressEXT(
					"glTexImage3D",
					typeof(TexImage3D),
					"OES"
				);
				glTexSubImage3D = (TexSubImage3D) GetProcAddressEXT(
					"glTexSubImage3D",
					typeof(TexSubImage3D),
					"OES"
				);
				glGenQueries = (GenQueries) GetProcAddress(
					"glGenQueries",
					typeof(GenQueries)
				);
				glDeleteQueries = (DeleteQueries) GetProcAddress(
					"glDeleteQueries",
					typeof(DeleteQueries)
				);
				glBeginQuery = (BeginQuery) GetProcAddress(
					"glBeginQuery",
					typeof(BeginQuery)
				);
				glEndQuery = (EndQuery) GetProcAddress(
					"glEndQuery",
					typeof(EndQuery)
				);
				glGetQueryObjectuiv = (GetQueryObjectuiv) GetProcAddress(
					"glGetQueryObjectuiv",
					typeof(GetQueryObjectuiv)
				);
			}
			catch
			{
				if (useES3)
				{
					FNALoggerEXT.LogWarn("Some non-ES functions failed to load. Beware...");
				}
				else
				{
					throw new NoSuitableGraphicsDeviceException(
						baseErrorString +
						"\nFailed on Tex3D/Query entries\n" +
						driver
					);
				}
			}

			/* ARB_framebuffer_object. We're flexible, but not _that_ flexible. */
			try
			{
				glGenFramebuffers = (GenFramebuffers) GetProcAddressEXT(
					"glGenFramebuffers",
					typeof(GenFramebuffers)
				);
				glDeleteFramebuffers = (DeleteFramebuffers) GetProcAddressEXT(
					"glDeleteFramebuffers",
					typeof(DeleteFramebuffers)
				);
				glBindFramebuffer = (G_BindFramebuffer) GetProcAddressEXT(
					"glBindFramebuffer",
					typeof(G_BindFramebuffer)
				);
				glFramebufferTexture2D = (FramebufferTexture2D) GetProcAddressEXT(
					"glFramebufferTexture2D",
					typeof(FramebufferTexture2D)
				);
				glFramebufferRenderbuffer = (FramebufferRenderbuffer) GetProcAddressEXT(
					"glFramebufferRenderbuffer",
					typeof(FramebufferRenderbuffer)
				);
				glGenerateMipmap = (GenerateMipmap) GetProcAddressEXT(
					"glGenerateMipmap",
					typeof(GenerateMipmap)
				);
				glGenRenderbuffers = (GenRenderbuffers) GetProcAddressEXT(
					"glGenRenderbuffers",
					typeof(GenRenderbuffers)
				);
				glDeleteRenderbuffers = (DeleteRenderbuffers) GetProcAddressEXT(
					"glDeleteRenderbuffers",
					typeof(DeleteRenderbuffers)
				);
				glBindRenderbuffer = (BindRenderbuffer) GetProcAddressEXT(
					"glBindRenderbuffer",
					typeof(BindRenderbuffer)
				);
				glRenderbufferStorage = (RenderbufferStorage) GetProcAddressEXT(
					"glRenderbufferStorage",
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
				glBlitFramebuffer = (BlitFramebuffer) GetProcAddressEXT(
					"glBlitFramebuffer",
					typeof(BlitFramebuffer)
				);
			}
			catch
			{
				supportsFauxBackbuffer = false;
			}

			/* EXT_framebuffer_multisample (or ARB_framebuffer_object) is glitter */
			supportsMultisampling = true;
			try
			{
				glRenderbufferStorageMultisample = (RenderbufferStorageMultisample) GetProcAddressEXT(
					"glRenderbufferStorageMultisample",
					typeof(RenderbufferStorageMultisample)
				);
			}
			catch
			{
				supportsMultisampling = false;
			}

			/* ARB_instanced_arrays/ARB_draw_instanced are almost optional. */
			SupportsHardwareInstancing = true;
			try
			{
				glVertexAttribDivisor = (VertexAttribDivisor) GetProcAddress(
					"glVertexAttribDivisor",
					typeof(VertexAttribDivisor)
				);
				/* The likelihood of someone having BaseVertex but not Instanced is 0...? */
				if (supportsBaseVertex)
				{
					glDrawElementsInstancedBaseVertex = (DrawElementsInstancedBaseVertex) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glDrawElementsInstancedBaseVertex"),
						typeof(DrawElementsInstancedBaseVertex)
					);
				}
				else
				{
					glDrawElementsInstanced = (DrawElementsInstanced) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glDrawElementsInstanced"),
						typeof(DrawElementsInstanced)
					);
					glDrawElementsInstancedBaseVertex = DrawElementsInstancedNoBase;
				}
			}
			catch
			{
				SupportsHardwareInstancing = false;
			}

			/* ARB_invalidate_subdata makes target swaps faster on mobile targets */
			supportsFBOInvalidation = useES3; // FIXME: Does desktop benefit from this?
			try
			{
				IntPtr ifbo = SDL.SDL_GL_GetProcAddress("glInvalidateFramebuffer");
				if (ifbo == IntPtr.Zero && useES3)
				{
					/* ES2 has EXT_discard_framebuffer as a fallback */
					ifbo = SDL.SDL_GL_GetProcAddress("glDiscardFramebufferEXT");
				}
				glInvalidateFramebuffer = (InvalidateFramebuffer) Marshal.GetDelegateForFunctionPointer(
					ifbo,
					typeof(InvalidateFramebuffer)
				);
			}
			catch
			{
				supportsFBOInvalidation = false;
			}

			/* Indexed color mask is a weird thing.
			 * IndexedEXT was introduced in EXT_draw_buffers2, then
			 * it was introduced in GL 3.0 as "ColorMaski" with no
			 * extension at all, and OpenGL ES introduced it as
			 * ColorMaskiEXT via EXT_draw_buffers_indexed and AGAIN
			 * as ColorMaskiOES via OES_draw_buffers_indexed at the
			 * exact same time. WTF.
			 * -flibit
			 */
			IntPtr cm = SDL.SDL_GL_GetProcAddress("glColorMaski");
			if (cm == IntPtr.Zero)
			{
				cm = SDL.SDL_GL_GetProcAddress("glColorMaskIndexedEXT");
			}
			if (cm == IntPtr.Zero)
			{
				cm = SDL.SDL_GL_GetProcAddress("glColorMaskiOES");
			}
			if (cm == IntPtr.Zero)
			{
				cm = SDL.SDL_GL_GetProcAddress("glColorMaskiEXT");
			}
			try
			{
				glColorMaski = (ColorMaski) Marshal.GetDelegateForFunctionPointer(
					cm,
					typeof(ColorMaski)
				);
			}
			catch
			{
				// FIXME: SupportsIndependentWriteMasks? -flibit
			}

			/* ARB_texture_multisample is probably used by nobody. */
			try
			{
				glSampleMaski = (SampleMaski) GetProcAddress(
					"glSampleMaski",
					typeof(SampleMaski)
				);
			}
			catch
			{
				// FIXME: SupportsMultisampleMasks? -flibit
			}

			if (useCoreProfile)
			{
				try
				{
					INTERNAL_glGetStringi = (GetStringi) GetProcAddress(
						"glGetStringi",
						typeof(GetStringi)
					);
					glGenVertexArrays = (GenVertexArrays) GetProcAddress(
						"glGenVertexArrays",
						typeof(GenVertexArrays)
					);
					glDeleteVertexArrays = (DeleteVertexArrays) GetProcAddress(
						"glDeleteVertexArrays",
						typeof(DeleteVertexArrays)
					);
					glBindVertexArray = (BindVertexArray) GetProcAddress(
						"glBindVertexArray",
						typeof(BindVertexArray)
					);
				}
				catch
				{
					throw new NoSuitableGraphicsDeviceException("OpenGL 3.2 support is required!");
				}
			}

#if DEBUG
			/* ARB_debug_output/KHR_debug, for debug contexts */
			bool supportsDebug = true;
			IntPtr messageCallback;
			IntPtr messageControl;

			/* Try KHR_debug first...
			 *
			 * "NOTE: when implemented in an OpenGL ES context, all entry points defined
			 * by this extension must have a "KHR" suffix. When implemented in an
			 * OpenGL context, all entry points must have NO suffix, as shown below."
			 * https://www.khronos.org/registry/OpenGL/extensions/KHR/KHR_debug.txt
			 */
			if (useES3)
			{
				messageCallback = SDL.SDL_GL_GetProcAddress("glDebugMessageCallbackKHR");
				messageControl = SDL.SDL_GL_GetProcAddress("glDebugMessageControlKHR");
			}
			else
			{
				messageCallback = SDL.SDL_GL_GetProcAddress("glDebugMessageCallback");
				messageControl = SDL.SDL_GL_GetProcAddress("glDebugMessageControl");
			}
			if (messageCallback == IntPtr.Zero || messageControl == IntPtr.Zero)
			{
				/* ... then try ARB_debug_output. */
				messageCallback = SDL.SDL_GL_GetProcAddress("glDebugMessageCallbackARB");
				messageControl = SDL.SDL_GL_GetProcAddress("glDebugMessageControlARB");
			}
			if (messageCallback == IntPtr.Zero || messageControl == IntPtr.Zero)
			{
				supportsDebug = false;
			}

			/* Android developers are incredibly stupid and export stub functions */
			if (useES3)
			{
				if (	SDL.SDL_GL_ExtensionSupported("GL_KHR_debug") == SDL.SDL_bool.SDL_FALSE &&
					SDL.SDL_GL_ExtensionSupported("GL_ARB_debug_output") == SDL.SDL_bool.SDL_FALSE	)
				{
					supportsDebug = false;
				}
			}

			/* Set the callback, finally. */
			if (!supportsDebug)
			{
				FNALoggerEXT.LogWarn("ARB_debug_output/KHR_debug not supported!");
			}
			else
			{
				glDebugMessageCallback = (DebugMessageCallback) Marshal.GetDelegateForFunctionPointer(
					messageCallback,
					typeof(DebugMessageCallback)
				);
				glDebugMessageControl = (DebugMessageControl) Marshal.GetDelegateForFunctionPointer(
					messageControl,
					typeof(DebugMessageControl)
				);
				glDebugMessageControl(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					0,
					IntPtr.Zero,
					true
				);
				glDebugMessageControl(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER,
					GLenum.GL_DEBUG_SEVERITY_LOW,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageControl(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER,
					GLenum.GL_DEBUG_SEVERITY_NOTIFICATION,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageCallback(Marshal.GetFunctionPointerForDelegate(DebugCall), IntPtr.Zero);
			}

			/* GREMEDY_string_marker, for apitrace */
			IntPtr stringMarkerCallback = SDL.SDL_GL_GetProcAddress("glStringMarkerGREMEDY");
			if (stringMarkerCallback == IntPtr.Zero)
			{
				FNALoggerEXT.LogWarn("GREMEDY_string_marker not supported!");
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

		private Delegate GetProcAddress(string name, Type type)
		{
			IntPtr addr = SDL.SDL_GL_GetProcAddress(name);
			if (addr == IntPtr.Zero)
			{
				throw new Exception(name);
			}
			return Marshal.GetDelegateForFunctionPointer(addr, type);
		}

		private Delegate GetProcAddressEXT(string name, Type type, string ext = "EXT")
		{
			IntPtr addr = SDL.SDL_GL_GetProcAddress(name);
			if (addr == IntPtr.Zero)
			{
				addr = SDL.SDL_GL_GetProcAddress(name + ext);
			}
			if (addr == IntPtr.Zero)
			{
				throw new Exception(name);
			}
			return Marshal.GetDelegateForFunctionPointer(addr, type);
		}

		private void DrawRangeElementsNoBase(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices,
			int baseVertex
		) {
			glDrawRangeElements(
				mode,
				start,
				end,
				count,
				type,
				indices
			);
		}

		private void DrawRangeElementsNoBaseUnchecked(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices,
			int baseVertex
		) {
			glDrawElements(
				mode,
				count,
				type,
				indices
			);
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

		private void DrawElementsInstancedNoBase(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount,
			int baseVertex
		) {
			glDrawElementsInstanced(
				mode,
				count,
				type,
				indices,
				instanceCount
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
