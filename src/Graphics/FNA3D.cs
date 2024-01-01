#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	[System.Security.SuppressUnmanagedCodeSecurity]
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
			public int data1; /* width for 2D, size for Cube */
			public int data2; /* height for 2D, face for Cube */
			public int levelCount;
			public int multiSampleCount;
			public IntPtr texture;
			public IntPtr colorBuffer;
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
		public delegate void FNA3D_LogFunc(IntPtr msg);

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

		#region Presentation

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

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_VerifyVertexSampler(
			IntPtr device,
			int index,
			IntPtr texture, /* FNA3D_Texture* */
			ref FNA3D_SamplerState sampler
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_ApplyVertexBufferBindings(
			IntPtr device,
			FNA3D_VertexBufferBinding* bindings,
			int numBindings,
			byte bindingsUpdated,
			int baseVertex
		);

		#endregion

		#region Render Targets

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetRenderTargets(
			IntPtr device,
			IntPtr renderTargets, /* FNA3D_RenderTargetBinding* */
			int numRenderTargets,
			IntPtr depthStencilBuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat,
			byte preserveDepthStencilContents
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_SetRenderTargets(
			IntPtr device,
			FNA3D_RenderTargetBinding* renderTargets,
			int numRenderTargets,
			IntPtr depthStencilBuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat,
			byte preserveDepthStencilContents
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResolveTarget(
			IntPtr device,
			ref FNA3D_RenderTargetBinding target
		);

		#endregion

		#region Backbuffer Functions

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResetBackbuffer(
			IntPtr device,
			ref FNA3D_PresentationParameters presentationParameters
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ReadBackbuffer(
			IntPtr device,
			int x,
			int y,
			int w,
			int h,
			IntPtr data,
			int dataLen
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetBackbufferSize(
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
			int x,
			int y,
			int z,
			int w,
			int h,
			int d,
			int level,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureDataCube(
			IntPtr device,
			IntPtr texture,
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
			int yWidth,
			int yHeight,
			int uvWidth,
			int uvHeight,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData2D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData3D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int z,
			int w,
			int h,
			int d,
			int level,
			IntPtr data,
			int dataLength
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureDataCube(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			CubeMapFace cubeMapFace,
			int level,
			IntPtr data,
			int dataLength
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
			int sizeInBytes
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
			int elementCount,
			int elementSizeInBytes,
			int vertexStride,
			SetDataOptions options
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetVertexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
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
			int sizeInBytes
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
			int dataLength
		);

		#endregion

		#region Effects

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_CreateEffect(
			IntPtr device,
			byte[] effectCode,
			int length,
			out IntPtr effect,
			out IntPtr effectData
		);

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_CloneEffect(
			IntPtr device,
			IntPtr cloneSource,
			out IntPtr effect,
			out IntPtr effectData
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeEffect(
			IntPtr device,
			IntPtr effect
		);

		/* effect refers to a MOJOSHADER_effect*, technique to a MOJOSHADER_effectTechnique* */
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetEffectTechnique(
			IntPtr device,
			IntPtr effect,
			IntPtr technique
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyEffect(
			IntPtr device,
			IntPtr effect,
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
		public static extern byte FNA3D_SupportsBC7(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsHardwareInstancing(
			IntPtr device
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsNoOverwrite(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsSRGBRenderTargets(IntPtr device);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetMaxTextureSlots(
			IntPtr device,
			out int textures,
			out int vertexTextures
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMaxMultiSampleCount(
			IntPtr device,
			SurfaceFormat format,
			int preferredMultiSampleCount
		);

		#endregion

		#region Debugging

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern unsafe void FNA3D_SetStringMarker(
			IntPtr device,
			byte* text
		);

		public static unsafe void FNA3D_SetStringMarker(
			IntPtr device,
			string text
		) {
			byte* utf8Text = SDL2.SDL.Utf8EncodeHeap(text);
			FNA3D_SetStringMarker(device, utf8Text);
			Marshal.FreeHGlobal((IntPtr) utf8Text);
		}
		
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern unsafe void FNA3D_SetTextureName(
			IntPtr device,
			IntPtr texture,
			byte* text
		);

		public static unsafe void FNA3D_SetTextureName(
			IntPtr device,
			IntPtr texture,
			string text 
		) {
			byte* utf8Text = SDL2.SDL.Utf8EncodeHeap(text);
			FNA3D_SetTextureName(device, texture, utf8Text);
			Marshal.FreeHGlobal((IntPtr) utf8Text);
		}

		#endregion

		#region Image Read API

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FNA3D_Image_ReadFunc(
			IntPtr context,
			IntPtr data,
			int size
		);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void FNA3D_Image_SkipFunc(
			IntPtr context,
			int n
		);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FNA3D_Image_EOFFunc(IntPtr context);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FNA3D_Image_Load(
			FNA3D_Image_ReadFunc readFunc,
			FNA3D_Image_SkipFunc skipFunc,
			FNA3D_Image_EOFFunc eofFunc,
			IntPtr context,
			out int width,
			out int height,
			out int len,
			int forceW,
			int forceH,
			byte zoom
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_Image_Free(IntPtr mem);

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D_Image_ReadFunc))]
		private static int INTERNAL_Read(
			IntPtr context,
			IntPtr data,
			int size
		) {
			Stream stream;
			lock (readStreams)
			{
				stream = readStreams[context];
			}
			byte[] buf = new byte[size]; // FIXME: Preallocate!
			int result = stream.Read(buf, 0, size);
			Marshal.Copy(buf, 0, data, result);
			return result;
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D_Image_SkipFunc))]
		private static void INTERNAL_Skip(IntPtr context, int n)
		{
			Stream stream;
			lock (readStreams)
			{
				stream = readStreams[context];
			}
			stream.Seek(n, SeekOrigin.Current);
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D_Image_EOFFunc))]
		private static int INTERNAL_EOF(IntPtr context)
		{
			Stream stream;
			lock (readStreams)
			{
				stream = readStreams[context];
			}
			return (stream.Position == stream.Length) ? 1 : 0;
		}

		private static FNA3D_Image_ReadFunc readFunc = INTERNAL_Read;
		private static FNA3D_Image_SkipFunc skipFunc = INTERNAL_Skip;
		private static FNA3D_Image_EOFFunc eofFunc = INTERNAL_EOF;

		private static int readGlobal = 0;
		private static System.Collections.Generic.Dictionary<IntPtr, Stream> readStreams =
			new System.Collections.Generic.Dictionary<IntPtr, Stream>();

		public static IntPtr ReadImageStream(
			Stream stream,
			out int width,
			out int height,
			out int len,
			int forceW = -1,
			int forceH = -1,
			bool zoom = false
		) {
			IntPtr context;
			lock (readStreams)
			{
				context = (IntPtr) readGlobal++;
				readStreams.Add(context, stream);
			}
			IntPtr pixels = FNA3D_Image_Load(
				readFunc,
				skipFunc,
				eofFunc,
				context,
				out width,
				out height,
				out len,
				forceW,
				forceH,
				(byte) (zoom ? 1 : 0)
			);
			lock (readStreams)
			{
				readStreams.Remove(context);
			}
			return pixels;
		}

		#endregion

		#region Image Write API

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void FNA3D_Image_WriteFunc(
			IntPtr context,
			IntPtr data,
			int size
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FNA3D_Image_SavePNG(
			FNA3D_Image_WriteFunc writeFunc,
			IntPtr context,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data
		);

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FNA3D_Image_SaveJPG(
			FNA3D_Image_WriteFunc writeFunc,
			IntPtr context,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data,
			int quality
		);

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D_Image_WriteFunc))]
		private static void INTERNAL_Write(
			IntPtr context,
			IntPtr data,
			int size
		) {
			Stream stream;
			lock (writeStreams)
			{
				stream = writeStreams[context];
			}
			byte[] buf = new byte[size]; // FIXME: Preallocate!
			Marshal.Copy(data, buf, 0, size);
			stream.Write(buf, 0, size);
		}

		private static FNA3D_Image_WriteFunc writeFunc = INTERNAL_Write;

		private static int writeGlobal = 0;
		private static System.Collections.Generic.Dictionary<IntPtr, Stream> writeStreams =
			new System.Collections.Generic.Dictionary<IntPtr, Stream>();

		public static void WritePNGStream(
			Stream stream,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data
		) {
			IntPtr context;
			lock (writeStreams)
			{
				context = (IntPtr) writeGlobal++;
				writeStreams.Add(context, stream);
			}
			FNA3D_Image_SavePNG(
				writeFunc,
				context,
				srcW,
				srcH,
				dstW,
				dstH,
				data
			);
			lock (writeStreams)
			{
				writeStreams.Remove(context);
			}
		}

		public static void WriteJPGStream(
			Stream stream,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data,
			int quality
		) {
			IntPtr context;
			lock (writeStreams)
			{
				context = (IntPtr) writeGlobal++;
				writeStreams.Add(context, stream);
			}
			FNA3D_Image_SaveJPG(
				writeFunc,
				context,
				srcW,
				srcH,
				dstW,
				dstH,
				data,
				quality
			);
			lock (writeStreams)
			{
				writeStreams.Remove(context);
			}
		}

		#endregion
	}
}
