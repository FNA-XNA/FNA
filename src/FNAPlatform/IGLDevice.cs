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

		IGLBuffer GenVertexBuffer(
			bool dynamic,
			int vertexCount,
			int vertexStride
		);
		void AddDisposeVertexBuffer(IGLBuffer buffer);
		void SetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);
		void GetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride
		);

		IGLBuffer GenIndexBuffer(
			bool dynamic,
			int indexCount,
			IndexElementSize indexElementSize
		);
		void AddDisposeIndexBuffer(IGLBuffer buffer);
		void SetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		);
		void GetIndexBufferData(
			IGLBuffer buffer,
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

	internal interface IGLBuffer
	{
		IntPtr BufferSize
		{
			get;
		}
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
