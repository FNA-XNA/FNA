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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class FNA3D
	{
		#region Private Constants

		private const string nativeLibName = "FNA3D";

		#endregion

		#region Native Structures

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_Viewport
		{
			public int x;
			public int y;
			public int w;
			public int h;
			public float minDepth;
			public float maxDepth;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_BlendState
		{
			public Blend colorSourceBlend;
			public Blend colorDestinationBlend;
			public BlendFunction colorBlendFunction;
			public Blend alphaSourceBlend;
			public Blend alphaDestinationBlend;
			public BlendFunction alphaBlendFunction;
			public ColorWriteChannels colorWriteEnable;
			public ColorWriteChannels colorWriteEnable1;
			public ColorWriteChannels colorWriteEnable2;
			public ColorWriteChannels colorWriteEnable3;
			public Color blendFactor;
			public int multiSampleMask;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_DepthStencilState
		{
			public byte depthBufferEnable;
			public byte depthBufferWriteEnable;
			public CompareFunction depthBufferFunction;
			public byte stencilEnable;
			public int stencilMask;
			public int stencilWriteMask;
			public byte twoSidedStencilMode;
			public StencilOperation stencilFail;
			public StencilOperation stencilDepthBufferFail;
			public StencilOperation stencilPass;
			public CompareFunction stencilFunction;
			public StencilOperation ccwStencilFail;
			public StencilOperation ccwStencilDepthBufferFail;
			public StencilOperation ccwStencilPass;
			public CompareFunction ccwStencilFunction;
			public int referenceStencil;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_RasterizerState
		{
			public FillMode fillMode;
			public CullMode cullMode;
			public float depthBias;
			public float slopeScaleDepthBias;
			public byte scissorTestEnable;
			public byte multiSampleAntiAlias;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_SamplerState
		{
			public TextureFilter filter;
			public TextureAddressMode addressU;
			public TextureAddressMode addressV;
			public TextureAddressMode addressW;
			public float mipMapLevelOfDetailBias;
			public int maxAnisotropy;
			public int maxMipLevel;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_VertexDeclaration
		{
			public int vertexStride;
			public int elementCount;
			public IntPtr elements; /* FNA3D_VertexElement* */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_VertexBufferBinding
		{
			public IntPtr vertexBuffer; /* FNA3D_Buffer* */
			public FNA3D_VertexDeclaration vertexDeclaration;
			public int vertexOffset;
			public int instanceFrequency;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_RenderTargetBinding
		{
			public byte type;
			public SurfaceFormat format;
			public int levelCount;
			public IntPtr texture;
			public int width;
			public int height;
			public RenderTargetUsage renderTargetUsage;
			public IntPtr colorBuffer;
			public DepthFormat depthStencilFormat;
			public int multiSampleCount;
			public CubeMapFace cubeMapFace;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_PresentationParameters
		{
			public int backBufferWidth;
			public int backBufferHeight;
			public SurfaceFormat backBufferFormat;
			public int multiSampleCount;
			public IntPtr deviceWindowHandle;
			public byte isFullScreen;
			public DepthFormat depthStencilFormat;
			public PresentInterval presentationInterval;
			public DisplayOrientation displayOrientation;
			public RenderTargetUsage renderTargetUsage;
		}

		#endregion

		#region Logging

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void FNA3D_LogFunc(string msg);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_HookLogFunctions(
			FNA3D_LogFunc info,
			FNA3D_LogFunc warn,
			FNA3D_LogFunc error
		);

		#endregion

		#region Driver Functions

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint FNA3D_PrepareWindowAttributes();
		
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetDrawableSize(
			IntPtr window,
			out int w,
			out int h
		);

		#endregion

		#region Init/Quit

		/* IntPtr refers to an FNA3D_Device* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateDevice(
			ref FNA3D_PresentationParameters presentationParameters,
			byte debugMode
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DestroyDevice(IntPtr device);

		#endregion

		#region Begin/End Frame

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_BeginFrame(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			ref Rectangle sourceRectangle,
			ref Rectangle destinationRectangle,
			IntPtr overrideWindowHandle
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			IntPtr sourceRectangle, /* null Rectangle */
			IntPtr destinationRectangle, /* null Rectangle */
			IntPtr overrideWindowHandle
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			ref Rectangle sourceRectangle,
			IntPtr destinationRectangle, /* null Rectangle */
			IntPtr overrideWindowHandle
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			IntPtr sourceRectangle, /* null Rectangle */
			ref Rectangle destinationRectangle,
			IntPtr overrideWindowHandle
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetPresentationInterval(
			IntPtr device,
			PresentInterval presentInterval
		);

		#endregion

		#region Drawing

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_Clear(
			IntPtr device,
			ClearOptions options,
			ref Vector4 color,
			float depth,
			int stencil
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawIndexedPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			IntPtr indices, /* FNA3D_Buffer* */
			IndexElementSize indexElementSize
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawInstancedPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount,
			IntPtr indices, /* FNA3D_Buffer* */
			IndexElementSize indexElementSize
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawUserIndexedPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int numVertices,
			IntPtr indexData,
			int indexOffset,
			IndexElementSize indexElementSize,
			int primitiveCount
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawUserPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		);

		#endregion

		#region Mutable Render States

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetViewport(
			IntPtr device,
			ref FNA3D_Viewport viewport
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetScissorRect(
			IntPtr device,
			ref Rectangle scissor
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetBlendFactor(
			IntPtr device,
			out Color blendFactor
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetBlendFactor(
			IntPtr device,
			ref Color blendFactor
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMultiSampleMask(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetMultiSampleMask(
			IntPtr device,
			int mask
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetReferenceStencil(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetReferenceStencil(
			IntPtr device,
			int reference
		);

		#endregion

		#region Immutable Render States

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetBlendState(
			IntPtr device,
			ref FNA3D_BlendState blendState
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetDepthStencilState(
			IntPtr device,
			ref FNA3D_DepthStencilState depthStencilState
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyRasterizerState(
			IntPtr device,
			ref FNA3D_RasterizerState rasterizerState
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_VerifySampler(
			IntPtr device,
			int index,
			IntPtr texture, /* FNA3D_Texture* */
			ref FNA3D_SamplerState sampler
		);

		#endregion

		#region Vertex State

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_ApplyVertexBufferBindings(
			IntPtr device,
			FNA3D_VertexBufferBinding* bindings,
			int numBindings,
			byte bindingsUpdated,
			int baseVertex
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyVertexDeclaration(
			IntPtr device,
			ref FNA3D_VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		);

		#endregion

		#region Render Targets

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetRenderTargets(
			IntPtr device,
			IntPtr renderTargets, /* FNA3D_RenderTargetBinding* */
			int numRenderTargets,
			IntPtr renderbuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_SetRenderTargets(
			IntPtr device,
			FNA3D_RenderTargetBinding* renderTargets,
			int numRenderTargets,
			IntPtr renderbuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResolveTarget(
			IntPtr device,
			ref FNA3D_RenderTargetBinding target
		);

		#endregion

		#region Backbuffer Functions

		/* IntPtr refers to an FNA3D_Backbuffer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GetBackbuffer(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResetBackbuffer(
			IntPtr device,
			ref FNA3D_PresentationParameters presentationParameters
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ReadBackbuffer(
			IntPtr device,
			IntPtr data,
			int dataLen,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int x,
			int y,
			int w,
			int h
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern  void FNA3D_GetBackbufferSize(
			IntPtr device,
			out int w,
			out int h
		);
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SurfaceFormat FNA3D_GetBackbufferSurfaceFormat(
			IntPtr device
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern DepthFormat FNA3D_GetBackbufferDepthFormat(
			IntPtr device
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetBackbufferMultiSampleCount(
			IntPtr device
		);

		#endregion

		#region Textures

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTexture2D(
			IntPtr device,
			SurfaceFormat format,
			int width,
			int height,
			int levelCount,
			byte isRenderTarget
		);

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTexture3D(
			IntPtr device,
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount
		);

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTextureCube(
			IntPtr device,
			SurfaceFormat format,
			int size,
			int levelCount,
			byte isRenderTarget
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeTexture(
			IntPtr device,
			IntPtr texture /* FNA3D_Texture* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureData2D(
			IntPtr device,
			IntPtr texture,
			SurfaceFormat format,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureData3D(
			IntPtr device,
			IntPtr texture,
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

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureDataCube(
			IntPtr device,
			IntPtr texture,
			SurfaceFormat format,
			int x,
			int y,
			int w,
			int h,
			CubeMapFace cubeMapFace,
			int level,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureDataYUV(
			IntPtr device,
			IntPtr y,
			IntPtr u,
			IntPtr v,
			int w,
			int h,
			IntPtr ptr
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData2D(
			IntPtr device,
			IntPtr texture,
			SurfaceFormat format,
			int textureWidth,
			int textureHeight,
			int level,
			int x,
			int y,
			int w,
			int h,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData3D(
			IntPtr device,
			IntPtr texture,
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

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureDataCube(
			IntPtr device,
			IntPtr texture,
			SurfaceFormat format,
			int textureSize,
			CubeMapFace cubeMapFace,
			int level,
			int x,
			int y,
			int w,
			int h,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		);

		#endregion

		#region Renderbuffers

		/* IntPtr refers to an FNA3D_Renderbuffer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenColorRenderbuffer(
			IntPtr device,
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount,
			IntPtr texture /* FNA3D_Texture* */
		);

		/* IntPtr refers to an FNA3D_Renderbuffer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenDepthStencilRenderbuffer(
			IntPtr device,
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeRenderbuffer(
			IntPtr device,
			IntPtr renderbuffer
		);

		#endregion

		#region Vertex Buffers

		/* IntPtr refers to an FNA3D_Buffer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenVertexBuffer(
			IntPtr device,
			byte dynamic,
			BufferUsage usage,
			int vertexCount,
			int vertexStride
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeVertexBuffer(
			IntPtr device,
			IntPtr buffer
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetVertexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetVertexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride
		);

		#endregion

		#region Index Buffers

		/* IntPtr refers to an FNA3D_Buffer* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenIndexBuffer(
			IntPtr device,
			byte dynamic,
			BufferUsage usage,
			int indexCount,
			IndexElementSize indexElementSize
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeIndexBuffer(
			IntPtr device,
			IntPtr buffer
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetIndexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetIndexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		);

		#endregion

		#region Effects

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateEffect(
			IntPtr device,
			byte[] effectCode,
			int length
		);

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CloneEffect(
			IntPtr device,
			IntPtr effect
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeEffect(
			IntPtr device,
			IntPtr effect
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyEffect(
			IntPtr device,
			IntPtr effect,
			IntPtr technique, /* MOJOSHADER_effectTechnique* */
			uint pass,
			IntPtr stateChanges /* MOJOSHADER_effectStateChanges* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_BeginPassRestore(
			IntPtr device,
			IntPtr effect,
			IntPtr stateChanges /* MOJOSHADER_effectStateChanges* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_EndPassRestore(
			IntPtr device,
			IntPtr effect
		);

		#endregion

		#region Queries

		/* IntPtr refers to an FNA3D_Query* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateQuery(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeQuery(
			IntPtr device,
			IntPtr query
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_QueryBegin(
			IntPtr device,
			IntPtr query
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_QueryEnd(
			IntPtr device,
			IntPtr query
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_QueryComplete(
			IntPtr device,
			IntPtr query
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_QueryPixelCount(
			IntPtr device,
			IntPtr query
		);

		#endregion

		#region Feature Queries

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsDXT1(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsS3TC(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsHardwareInstancing(
			IntPtr device
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsNoOverwrite(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMaxTextureSlots(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMaxMultiSampleCount(IntPtr device);

		#endregion

		#region Debugging

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetStringMarker(
			IntPtr device,
			[MarshalAs(UnmanagedType.LPStr)] string text
		);

		/* TODO: Debug callback function...? */

		#endregion

		#region Buffer Objects

		/* IntPtr refers to a size_t/intptr_t */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GetBufferSize(
			IntPtr device,
			IntPtr buffer /* FNA3D_Buffer */
		);

		#endregion

		#region Effect Objects

		/* IntPtr refers to a MOJOSHADER_effect* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GetEffectData(
			IntPtr device,
			IntPtr effect /* FNA3D_Effect */
		);

		#endregion

		#region Backbuffer Objects

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetBackbufferWidth(
			IntPtr device,
			IntPtr backbuffer /* FNA3D_Backbuffer* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetBackbufferHeight(
			IntPtr device,
			IntPtr backbuffer /* FNA3D_Backbuffer* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern DepthFormat FNA3D_GetBackbufferDepthFormat(
			IntPtr device,
			IntPtr backbuffer /* FNA3D_Backbuffer* */
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetBackbufferMultiSampleCount(
			IntPtr device,
			IntPtr backbuffer /* FNA3D_Backbuffer* */
		);

		#endregion
	}
}
