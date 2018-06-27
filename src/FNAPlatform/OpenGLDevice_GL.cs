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
	internal partial class OpenGLDevice : IGLDevice
	{
		#region Private OpenGL Entry Points

	
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

		private delegate void DrawBuffers(int n, IntPtr bufs);
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

		private delegate void DrawElementsInstancedBaseVertex(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount,
			int baseVertex
		);
		private DrawElementsInstancedBaseVertex glDrawElementsInstancedBaseVertex;

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
			IntPtr debugCallback,
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
			supportsBaseVertex = ep != IntPtr.Zero;
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
				/* DrawRangeElements is better, but some ES3 targets don't have it. */
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

			/* EXT_draw_buffers2 is probably used by nobody. */
			try
			{
				glColorMaskIndexedEXT = (ColorMaskIndexedEXT) GetProcAddress(
					"glColorMaskIndexedEXT",
					typeof(ColorMaskIndexedEXT)
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
