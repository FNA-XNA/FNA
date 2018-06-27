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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal interface IGLDevice
	{
		Color BlendFactor
		{
			get;
			set;
		}

		int MultiSampleMask
		{
			get;
			set;
		}

		int ReferenceStencil
		{
			get;
			set;
		}

		bool SupportsDxt1
		{
			get;
		}

		bool SupportsS3tc
		{
			get;
		}

		bool SupportsHardwareInstancing
		{
			get;
		}

		int MaxTextureSlots
		{
			get;
		}

		int MaxMultiSampleCount
		{
			get;
		}

		IGLBackbuffer Backbuffer
		{
			get;
		}

		void Dispose();

		void ResetBackbuffer(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter,
			bool renderTargetBound
		);
		void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		);
		void SetStringMarker(string text);

		void DrawIndexedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			IndexBuffer indices
		);
		void DrawInstancedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount,
			IndexBuffer indices
		);
		void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		);
		void DrawUserIndexedPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int numVertices,
			IntPtr indexData,
			int indexOffset,
			IndexElementSize indexElementSize,
			int primitiveCount
		);
		void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		);

		void SetViewport(Viewport vp, bool renderTargetBound);
		void SetScissorRect(
			Rectangle scissorRect,
			bool renderTargetBound
		);
		void SetBlendState(BlendState blendState);
		void SetDepthStencilState(DepthStencilState depthStencilState);
		void ApplyRasterizerState(
			RasterizerState rasterizerState,
			bool renderTargetBound
		);
		void VerifySampler(
			int index,
			Texture texture,
			SamplerState sampler
		);

		void Clear(
			ClearOptions options,
			Vector4 color,
			float depth,
			int stencil
		);

		void SetRenderTargets(
			RenderTargetBinding[] renderTargets,
			IGLRenderbuffer renderbuffer,
			DepthFormat depthFormat
		);
		void ResolveTarget(RenderTargetBinding target);

		void ReadBackbuffer(
			IntPtr data,
			int dataLen,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int subX,
			int subY,
			int subW,
			int subH
		);

		IGLTexture CreateTexture2D(
			SurfaceFormat format,
			int width,
			int height,
			int levelCount
		);
		IGLTexture CreateTexture3D(
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount
		);
		IGLTexture CreateTextureCube(
			SurfaceFormat format,
			int size,
			int levelCount
		);
		void AddDisposeTexture(IGLTexture texture);
		void SetTextureData2D(
			IGLTexture texture,
			SurfaceFormat format,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength
		);
		void SetTextureData3D(
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
		);
		void SetTextureDataCube(
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
		);
		void SetTextureData2DPointer(Texture2D texture, IntPtr ptr);
		void GetTextureData2D(
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
		);
		void GetTextureData3D(
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
		);
		void GetTextureDataCube(
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
		);

		IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount
		);
		IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		);
		void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer);

		GLBuffer GenVertexBuffer(
			bool dynamic,
			int vertexCount,
			int vertexStride
		);
		void AddDisposeVertexBuffer(GLBuffer buffer);
		void SetVertexBufferData(
			GLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);
		void GetVertexBufferData(
			GLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride
		);

		GLBuffer GenIndexBuffer(
			bool dynamic,
			int indexCount,
			IndexElementSize indexElementSize
		);
		void AddDisposeIndexBuffer(GLBuffer buffer);
		void SetIndexBufferData(
			GLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);
		void GetIndexBufferData(
			GLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		);

		IGLEffect CreateEffect(byte[] effectCode);
		IGLEffect CloneEffect(IGLEffect effect);
		void AddDisposeEffect(IGLEffect effect);
		void ApplyEffect(
			IGLEffect effect,
			IntPtr technique,
			uint pass,
			IntPtr stateChanges
		);
		void BeginPassRestore(IGLEffect effect, IntPtr stateChanges);
		void EndPassRestore(IGLEffect effect);

		void ApplyVertexAttributes(
			VertexBufferBinding[] bindings,
			int numBindings,
			bool bindingsUpdated,
			int baseVertex
		);
		void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		);

		IGLQuery CreateQuery();
		void AddDisposeQuery(IGLQuery query);
		void QueryBegin(IGLQuery query);
		void QueryEnd(IGLQuery query);
		bool QueryComplete(IGLQuery query);
		int QueryPixelCount(IGLQuery query);
	}

	internal interface IGLTexture
	{
	}

	internal interface IGLRenderbuffer
	{
	}

	internal enum GLenum : int {
		// Hint Enum Value
		GL_DONT_CARE = 0x1100,
		// 0/1
		GL_ZERO = 0x0000,
		GL_ONE = 0x0001,
		// Types
		GL_BYTE = 0x1400,
		GL_UNSIGNED_BYTE = 0x1401,
		GL_SHORT = 0x1402,
		GL_UNSIGNED_SHORT = 0x1403,
		GL_UNSIGNED_INT = 0x1405,
		GL_FLOAT = 0x1406,
		GL_HALF_FLOAT = 0x140B,
		GL_UNSIGNED_SHORT_4_4_4_4_REV = 0x8365,
		GL_UNSIGNED_SHORT_5_5_5_1_REV = 0x8366,
		GL_UNSIGNED_INT_2_10_10_10_REV = 0x8368,
		GL_UNSIGNED_SHORT_5_6_5 = 0x8363,
		GL_UNSIGNED_INT_24_8 = 0x84FA,
		// Strings
		GL_VENDOR = 0x1F00,
		GL_RENDERER = 0x1F01,
		GL_VERSION = 0x1F02,
		GL_EXTENSIONS = 0x1F03,
		// Clear Mask
		GL_COLOR_BUFFER_BIT = 0x4000,
		GL_DEPTH_BUFFER_BIT = 0x0100,
		GL_STENCIL_BUFFER_BIT = 0x0400,
		// Enable Caps
		GL_SCISSOR_TEST = 0x0C11,
		GL_DEPTH_TEST = 0x0B71,
		GL_STENCIL_TEST = 0x0B90,
		// Polygons
		GL_LINE = 0x1B01,
		GL_FILL = 0x1B02,
		GL_CW = 0x0900,
		GL_CCW = 0x0901,
		GL_FRONT = 0x0404,
		GL_BACK = 0x0405,
		GL_FRONT_AND_BACK = 0x0408,
		GL_CULL_FACE = 0x0B44,
		GL_POLYGON_OFFSET_FILL = 0x8037,
		// Texture Type
		GL_TEXTURE_2D = 0x0DE1,
		GL_TEXTURE_3D = 0x806F,
		GL_TEXTURE_CUBE_MAP = 0x8513,
		GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515,
		// Blend Mode
		GL_BLEND = 0x0BE2,
		GL_SRC_COLOR = 0x0300,
		GL_ONE_MINUS_SRC_COLOR = 0x0301,
		GL_SRC_ALPHA = 0x0302,
		GL_ONE_MINUS_SRC_ALPHA = 0x0303,
		GL_DST_ALPHA = 0x0304,
		GL_ONE_MINUS_DST_ALPHA = 0x0305,
		GL_DST_COLOR = 0x0306,
		GL_ONE_MINUS_DST_COLOR = 0x0307,
		GL_SRC_ALPHA_SATURATE = 0x0308,
		GL_CONSTANT_COLOR = 0x8001,
		GL_ONE_MINUS_CONSTANT_COLOR = 0x8002,
		// Equations
		GL_MIN = 0x8007,
		GL_MAX = 0x8008,
		GL_FUNC_ADD = 0x8006,
		GL_FUNC_SUBTRACT = 0x800A,
		GL_FUNC_REVERSE_SUBTRACT = 0x800B,
		// Comparisons
		GL_NEVER = 0x0200,
		GL_LESS = 0x0201,
		GL_EQUAL = 0x0202,
		GL_LEQUAL = 0x0203,
		GL_GREATER = 0x0204,
		GL_NOTEQUAL = 0x0205,
		GL_GEQUAL = 0x0206,
		GL_ALWAYS = 0x0207,
		// Stencil Operations
		GL_INVERT = 0x150A,
		GL_KEEP = 0x1E00,
		GL_REPLACE = 0x1E01,
		GL_INCR = 0x1E02,
		GL_DECR = 0x1E03,
		GL_INCR_WRAP = 0x8507,
		GL_DECR_WRAP = 0x8508,
		// Wrap Modes
		GL_REPEAT = 0x2901,
		GL_CLAMP_TO_EDGE = 0x812F,
		GL_MIRRORED_REPEAT = 0x8370,
		// Filters
		GL_NEAREST = 0x2600,
		GL_LINEAR = 0x2601,
		GL_NEAREST_MIPMAP_NEAREST = 0x2700,
		GL_NEAREST_MIPMAP_LINEAR = 0x2702,
		GL_LINEAR_MIPMAP_NEAREST = 0x2701,
		GL_LINEAR_MIPMAP_LINEAR = 0x2703,
		// Attachments
		GL_COLOR_ATTACHMENT0 = 0x8CE0,
		GL_DEPTH_ATTACHMENT = 0x8D00,
		GL_STENCIL_ATTACHMENT = 0x8D20,
		GL_DEPTH_STENCIL_ATTACHMENT = 0x821A,
		// Texture Formats
		GL_RED = 0x1903,
		GL_RGB = 0x1907,
		GL_RGBA = 0x1908,
		GL_LUMINANCE = 0x1909,
		GL_LUMINANCE8 = 0x8040,
		GL_RGB8 = 0x8051,
		GL_RGBA8 = 0x8058,
		GL_RGBA4 = 0x8056,
		GL_RGB5_A1 = 0x8057,
		GL_RGB10_A2_EXT = 0x8059,
		GL_RGBA16 = 0x805B,
		GL_BGRA = 0x80E1,
		GL_DEPTH_COMPONENT16 = 0x81A5,
		GL_DEPTH_COMPONENT24 = 0x81A6,
		GL_RG = 0x8227,
		GL_RG8 = 0x822B,
		GL_RG16 = 0x822C,
		GL_R16F = 0x822D,
		GL_R32F = 0x822E,
		GL_RG16F = 0x822F,
		GL_RG32F = 0x8230,
		GL_RGBA32F = 0x8814,
		GL_RGBA16F = 0x881A,
		GL_DEPTH24_STENCIL8 = 0x88F0,
		GL_COMPRESSED_TEXTURE_FORMATS = 0x86A3,
		GL_COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1,
		GL_COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2,
		GL_COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3,
		GL_RGB565 = 0x8D62,
		// Texture Internal Formats
		GL_DEPTH_COMPONENT = 0x1902,
		GL_DEPTH_STENCIL = 0x84F9,
		// Textures
		GL_TEXTURE_WRAP_S = 0x2802,
		GL_TEXTURE_WRAP_T = 0x2803,
		GL_TEXTURE_WRAP_R = 0x8072,
		GL_TEXTURE_MAG_FILTER = 0x2800,
		GL_TEXTURE_MIN_FILTER = 0x2801,
		GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE,
		GL_TEXTURE_BASE_LEVEL = 0x813C,
		GL_TEXTURE_MAX_LEVEL = 0x813D,
		GL_TEXTURE_LOD_BIAS = 0x8501,
		GL_UNPACK_ALIGNMENT = 0x0CF5,
		// Multitexture
		GL_TEXTURE0 = 0x84C0,
		GL_MAX_TEXTURE_IMAGE_UNITS = 0x8872,
		GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C,
		// Buffer objects
		GL_ARRAY_BUFFER = 0x8892,
		GL_ELEMENT_ARRAY_BUFFER = 0x8893,
		GL_STREAM_DRAW = 0x88E0,
		GL_STATIC_DRAW = 0x88E4,
		GL_MAX_VERTEX_ATTRIBS = 0x8869,
		// Render targets
		GL_FRAMEBUFFER = 0x8D40,
		GL_READ_FRAMEBUFFER = 0x8CA8,
		GL_DRAW_FRAMEBUFFER = 0x8CA9,
		GL_RENDERBUFFER = 0x8D41,
		GL_MAX_DRAW_BUFFERS = 0x8824,
		// Draw Primitives
		GL_POINTS = 0x0000,
		GL_LINES = 0x0001,
		GL_LINE_STRIP = 0x0003,
		GL_TRIANGLES = 0x0004,
		GL_TRIANGLE_STRIP = 0x0005,
		// Query Objects
		GL_QUERY_RESULT = 0x8866,
		GL_QUERY_RESULT_AVAILABLE = 0x8867,
		GL_SAMPLES_PASSED = 0x8914,
		// Multisampling
		GL_MULTISAMPLE = 0x809D,
		GL_MAX_SAMPLES = 0x8D57,
		GL_SAMPLE_MASK = 0x8E51,
		// 3.2 Core Profile
		GL_NUM_EXTENSIONS = 0x821D,
		// Source Enum Values
		GL_DEBUG_SOURCE_API_ARB = 0x8246,
		GL_DEBUG_SOURCE_WINDOW_SYSTEM_ARB = 0x8247,
		GL_DEBUG_SOURCE_SHADER_COMPILER_ARB = 0x8248,
		GL_DEBUG_SOURCE_THIRD_PARTY_ARB = 0x8249,
		GL_DEBUG_SOURCE_APPLICATION_ARB = 0x824A,
		GL_DEBUG_SOURCE_OTHER_ARB = 0x824B,
		// Type Enum Values
		GL_DEBUG_TYPE_ERROR_ARB = 0x824C,
		GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR_ARB = 0x824D,
		GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR_ARB = 0x824E,
		GL_DEBUG_TYPE_PORTABILITY_ARB = 0x824F,
		GL_DEBUG_TYPE_PERFORMANCE_ARB = 0x8250,
		GL_DEBUG_TYPE_OTHER_ARB = 0x8251,
		// Severity Enum Values
		GL_DEBUG_SEVERITY_HIGH_ARB = 0x9146,
		GL_DEBUG_SEVERITY_MEDIUM_ARB = 0x9147,
		GL_DEBUG_SEVERITY_LOW_ARB = 0x9148,
		GL_DEBUG_SEVERITY_NOTIFICATION_ARB = 0x826B
	}

	internal struct GLBuffer {
		public uint Handle;
		public IntPtr BufferSize;
		public GLenum Dynamic;

		public GLBuffer(
			uint handle,
			IntPtr bufferSize,
			GLenum dynamic
		) {
			Handle = handle;
			BufferSize = bufferSize;
			Dynamic = dynamic;
		}

		public static readonly GLBuffer NullBuffer = new GLBuffer(0, IntPtr.Zero, GLenum.GL_ZERO);
	}

	internal interface IGLEffect
	{
		IntPtr EffectData
		{
			get;
		}
	}

	internal interface IGLQuery
	{
	}

	internal interface IGLBackbuffer
	{
		int Width
		{
			get;
		}

		int Height
		{
			get;
		}

		DepthFormat DepthFormat
		{
			get;
		}

		int MultiSampleCount
		{
			get;
		}

		void ResetFramebuffer(
			PresentationParameters presentationParameters,
			bool renderTargetBound
		);
	}
}
