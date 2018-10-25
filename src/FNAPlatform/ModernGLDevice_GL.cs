#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
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
	internal partial class ModernGLDevice : IGLDevice
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
			GL_RGB565 =				0x8D62,
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
		private delegate void CreateTextures(
			GLenum target,
			int n,
			out uint textures
		);
		private CreateTextures glCreateTextures;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteTextures(
			int n,
			ref uint textures
		);
		private DeleteTextures glDeleteTextures;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BindTextureUnit(int unit, uint texture);
		private BindTextureUnit glBindTextureUnit;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TextureStorage2D(
			uint texture,
			int levels,
			GLenum internalformat,
			int width,
			int height
		);
		private TextureStorage2D glTextureStorage2D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TextureSubImage2D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TextureSubImage2D glTextureSubImage2D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CompressedTextureSubImage2D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTextureSubImage2D glCompressedTextureSubImage2D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TextureStorage3D(
			uint texture,
			int levels,
			GLenum internalFormat,
			int width,
			int height,
			int depth
		);
		private TextureStorage3D glTextureStorage3D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void TextureSubImage3D(
			uint texture,
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
		private TextureSubImage3D glTextureSubImage3D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CompressedTextureSubImage3D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTextureSubImage3D glCompressedTextureSubImage3D;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetTextureSubImage(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			GLenum type,
			int bufSize,
			IntPtr pixels
		);
		private GetTextureSubImage glGetTextureSubImage;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CreateSamplers(
			int n,
			IntPtr samplers
		);
		private CreateSamplers glCreateSamplers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BindSampler(
			int unit,
			uint sampler
		);
		private BindSampler glBindSampler;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void SamplerParameteri(
			uint sampler,
			GLenum pname,
			int param
		);
		private SamplerParameteri glSamplerParameteri;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void SamplerParameterf(
			uint sampler,
			GLenum pname,
			float param
		);
		private SamplerParameterf glSamplerParameterf;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void PixelStorei(GLenum pname, int param);
		private PixelStorei glPixelStorei;

		/* END TEXTURE FUNCTIONS */

		/* BEGIN BUFFER FUNCTIONS */

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CreateBuffers(int n, out uint buffers);
		private CreateBuffers glCreateBuffers;

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
		private delegate void NamedBufferData(
			uint buffer,
			IntPtr size,
			IntPtr data,
			GLenum usage
		);
		private NamedBufferData glNamedBufferData;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedBufferSubData(
			uint buffer,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private NamedBufferSubData glNamedBufferSubData;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void GetNamedBufferSubData(
			uint buffer,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private GetNamedBufferSubData glGetNamedBufferSubData;

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
		private delegate void ClearStencil(int s);
		private ClearStencil glClearStencil;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void G_Clear(GLenum mask);
		private G_Clear glClear;

		/* END CLEAR FUNCTIONS */

		/* BEGIN FRAMEBUFFER FUNCTIONS */

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
		private delegate void GenerateTextureMipmap(uint texture);
		private GenerateTextureMipmap glGenerateTextureMipmap;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CreateFramebuffers(
			int n,
			out uint framebuffers
		);
		private CreateFramebuffers glCreateFramebuffers;

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
		private delegate void NamedFramebufferTexture(
			uint framebuffer,
			GLenum attachment,
			uint texture,
			int level
		);
		private NamedFramebufferTexture glNamedFramebufferTexture;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedFramebufferTextureLayer(
			uint framebuffer,
			GLenum attachment,
			uint texture,
			int level,
			int layer
		);
		private NamedFramebufferTextureLayer glNamedFramebufferTextureLayer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedFramebufferRenderbuffer(
			uint framebuffer,
			GLenum attachment,
			GLenum renderbuffertarget,
			uint renderbuffer
		);
		private NamedFramebufferRenderbuffer glNamedFramebufferRenderbuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedFramebufferDrawBuffers(
			uint framebuffer,
			int n,
			IntPtr bufs
		);
		private NamedFramebufferDrawBuffers glNamedFramebufferDrawBuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BlitNamedFramebuffer(
			uint readFramebuffer,
			uint drawFramebuffer,
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
		private BlitNamedFramebuffer glBlitNamedFramebuffer;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void CreateRenderbuffers(
			int n,
			out uint renderbuffers
		);
		private CreateRenderbuffers glCreateRenderbuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DeleteRenderbuffers(
			int n,
			ref uint renderbuffers
		);
		private DeleteRenderbuffers glDeleteRenderbuffers;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedRenderbufferStorage(
			uint renderbuffer,
			GLenum internalformat,
			int width,
			int height
		);
		private NamedRenderbufferStorage glNamedRenderbufferStorage;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void NamedRenderbufferStorageMultisample(
			uint renderbuffer,
			int samples,
			GLenum internalformat,
			int width,
			int height
		);
		private NamedRenderbufferStorageMultisample glNamedRenderbufferStorageMultisample;

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
		private DebugMessageCallback glDebugMessageCallbackARB;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
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
			if (type == GLenum.GL_DEBUG_TYPE_ERROR_ARB)
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

		private void LoadGLEntryPoints()
		{
			string baseErrorString = "OpenGL 4.5 support is required!";

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
				glDepthRange = (DepthRange) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthRange"),
					typeof(DepthRange)
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
				glColorMaski = (ColorMaski) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glColorMaski"),
					typeof(ColorMaski)
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
				glPolygonMode = (PolygonMode) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPolygonMode"),
					typeof(PolygonMode)
				);
				glCreateTextures = (CreateTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateTextures"),
					typeof(CreateTextures)
				);
				glDeleteTextures = (DeleteTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteTextures"),
					typeof(DeleteTextures)
				);
				glBindTextureUnit = (BindTextureUnit) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindTextureUnit"),
					typeof(BindTextureUnit)
				);
				glTextureStorage2D = (TextureStorage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureStorage2D"),
					typeof(TextureStorage2D)
				);
				glTextureSubImage2D = (TextureSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureSubImage2D"),
					typeof(TextureSubImage2D)
				);
				glCompressedTextureSubImage2D = (CompressedTextureSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTextureSubImage2D"),
					typeof(CompressedTextureSubImage2D)
				);
				glTextureStorage3D = (TextureStorage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureStorage3D"),
					typeof(TextureStorage3D)
				);
				glTextureSubImage3D = (TextureSubImage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureSubImage3D"),
					typeof(TextureSubImage3D)
				);
				glCompressedTextureSubImage3D = (CompressedTextureSubImage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTextureSubImage3D"),
					typeof(CompressedTextureSubImage3D)
				);
				glGetTextureSubImage = (GetTextureSubImage) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetTextureSubImage"),
					typeof(GetTextureSubImage)
				);
				glCreateSamplers = (CreateSamplers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateSamplers"),
					typeof(CreateSamplers)
				);
				glBindSampler = (BindSampler) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindSampler"),
					typeof(BindSampler)
				);
				glSamplerParameteri = (SamplerParameteri) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSamplerParameteri"),
					typeof(SamplerParameteri)
				);
				glSamplerParameterf = (SamplerParameterf) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSamplerParameterf"),
					typeof(SamplerParameterf)
				);
				glPixelStorei = (PixelStorei) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPixelStorei"),
					typeof(PixelStorei)
				);
				glCreateBuffers = (CreateBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateBuffers"),
					typeof(CreateBuffers)
				);
				glDeleteBuffers = (DeleteBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteBuffers"),
					typeof(DeleteBuffers)
				);
				glBindBuffer = (BindBuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindBuffer"),
					typeof(BindBuffer)
				);
				glNamedBufferData = (NamedBufferData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedBufferData"),
					typeof(NamedBufferData)
				);
				glNamedBufferSubData = (NamedBufferSubData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedBufferSubData"),
					typeof(NamedBufferSubData)
				);
				glGetNamedBufferSubData = (GetNamedBufferSubData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetNamedBufferSubData"),
					typeof(GetNamedBufferSubData)
				);
				glClearColor = (ClearColor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearColor"),
					typeof(ClearColor)
				);
				glClearDepth = (ClearDepth) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearDepth"),
					typeof(ClearDepth)
				);
				glClearStencil = (ClearStencil) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearStencil"),
					typeof(ClearStencil)
				);
				glClear = (G_Clear) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClear"),
					typeof(G_Clear)
				);
				glReadPixels = (ReadPixels) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glReadPixels"),
					typeof(ReadPixels)
				);
				glCreateFramebuffers = (CreateFramebuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateFramebuffers"),
					typeof(CreateFramebuffers)
				);
				glDeleteFramebuffers = (DeleteFramebuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteFramebuffers"),
					typeof(DeleteFramebuffers)
				);
				glBindFramebuffer = (G_BindFramebuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindFramebuffer"),
					typeof(G_BindFramebuffer)
				);
				glNamedFramebufferTexture = (NamedFramebufferTexture) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferTexture"),
					typeof(NamedFramebufferTexture)
				);
				glNamedFramebufferTextureLayer = (NamedFramebufferTextureLayer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferTextureLayer"),
					typeof(NamedFramebufferTextureLayer)
				);
				glNamedFramebufferRenderbuffer = (NamedFramebufferRenderbuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferRenderbuffer"),
					typeof(NamedFramebufferRenderbuffer)
				);
				glNamedFramebufferDrawBuffers = (NamedFramebufferDrawBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferDrawBuffers"),
					typeof(NamedFramebufferDrawBuffers)
				);
				glGenerateTextureMipmap = (GenerateTextureMipmap) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenerateTextureMipmap"),
					typeof(GenerateTextureMipmap)
				);
				glCreateRenderbuffers = (CreateRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateRenderbuffers"),
					typeof(CreateRenderbuffers)
				);
				glDeleteRenderbuffers = (DeleteRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteRenderbuffers"),
					typeof(DeleteRenderbuffers)
				);
				glNamedRenderbufferStorage = (NamedRenderbufferStorage) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedRenderbufferStorage"),
					typeof(NamedRenderbufferStorage)
				);
				glNamedRenderbufferStorageMultisample = (NamedRenderbufferStorageMultisample) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedRenderbufferStorageMultisample"),
					typeof(NamedRenderbufferStorageMultisample)
				);
				glSampleMaski = (SampleMaski) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSampleMaski"),
					typeof(SampleMaski)
				);
				glBlitNamedFramebuffer = (BlitNamedFramebuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlitNamedFramebuffer"),
					typeof(BlitNamedFramebuffer)
				);
				glVertexAttribPointer = (VertexAttribPointer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribPointer"),
					typeof(VertexAttribPointer)
				);
				glVertexAttribDivisor = (VertexAttribDivisor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribDivisor"),
					typeof(VertexAttribDivisor)
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
				glDrawRangeElements = (DrawRangeElements) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawRangeElements"),
					typeof(DrawRangeElements)
				);
				glDrawRangeElementsBaseVertex = (DrawRangeElementsBaseVertex) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawRangeElementsBaseVertex"),
					typeof(DrawRangeElementsBaseVertex)
				);
				glDrawElementsInstancedBaseVertex = (DrawElementsInstancedBaseVertex) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawElementsInstancedBaseVertex"),
					typeof(DrawElementsInstancedBaseVertex)
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
				throw new NoSuitableGraphicsDeviceException(baseErrorString);
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
				FNALoggerEXT.LogWarn("ARB_debug_output not supported!");
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
				glDebugMessageCallbackARB(Marshal.GetFunctionPointerForDelegate(DebugCall), IntPtr.Zero);
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

		#endregion
	}
}
