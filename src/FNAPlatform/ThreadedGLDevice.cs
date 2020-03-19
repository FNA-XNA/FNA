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
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal class ThreadedGLDevice : IGLDevice
	{
		#region Public Properties

		public Color BlendFactor
		{
			get
			{
				Color result = Color.Black;
				ForceToMainThread(() =>
				{
					result = GLDevice.BlendFactor;
				}); // End ForceToMainThread
				return result;
			}
			set
			{
				ForceToMainThread(() =>
				{
					GLDevice.BlendFactor = value;
				}); // End ForceToMainThread
			}
		}

		public int MultiSampleMask
		{
			get
			{
				int result = 0;
				ForceToMainThread(() =>
				{
					result = GLDevice.MultiSampleMask;
				}); // End ForceToMainThread
				return result;
			}
			set
			{
				ForceToMainThread(() =>
				{
					GLDevice.MultiSampleMask = value;
				}); // End ForceToMainThread
			}
		}

		public int ReferenceStencil
		{
			get
			{
				int result = 0;
				ForceToMainThread(() =>
				{
					result = GLDevice.ReferenceStencil;
				}); // End ForceToMainThread
				return result;
			}
			set
			{
				ForceToMainThread(() =>
				{
					GLDevice.ReferenceStencil = value;
				}); // End ForceToMainThread
			}
		}

		public bool SupportsDxt1
		{
			get
			{
				bool result = false;
				ForceToMainThread(() =>
				{
					result = GLDevice.SupportsDxt1;
				}); // End ForceToMainThread
				return result;
			}
		}

		public bool SupportsS3tc
		{
			get
			{
				bool result = false;
				ForceToMainThread(() =>
				{
					result = GLDevice.SupportsS3tc;
				}); // End ForceToMainThread
				return result;
			}
		}

		public bool SupportsHardwareInstancing
		{
			get
			{
				bool result = false;
				ForceToMainThread(() =>
				{
					result = GLDevice.SupportsHardwareInstancing;
				}); // End ForceToMainThread
				return result;
			}
		}

		public bool SupportsNoOverwrite
		{
			get
			{
				bool result = false;
				ForceToMainThread(() =>
				{
					result = GLDevice.SupportsNoOverwrite;
				}); // End ForceToMainThread
				return result;
			}
		}

		public int MaxTextureSlots
		{
			get
			{
				int result = 0;
				ForceToMainThread(() =>
				{
					result = GLDevice.MaxTextureSlots;
				}); // End ForceToMainThread
				return result;
			}
		}

		public int MaxMultiSampleCount
		{
			get
			{
				int result = 0;
				ForceToMainThread(() =>
				{
					result = GLDevice.MaxMultiSampleCount;
				}); // End ForceToMainThread
				return result;
			}
		}

		public IGLBackbuffer Backbuffer
		{
			get
			{
				IGLBackbuffer result = null;
				ForceToMainThread(() =>
				{
					result = GLDevice.Backbuffer;
				}); // End ForceToMainThread
				return result;
			}
		}

		#endregion

		#region Private Variables

		private IGLDevice GLDevice;
		private Thread csThread;
		private bool csDone = false;
		private AutoResetEvent csEvent = new AutoResetEvent(false);
		private List<Action> actions = new List<Action>();

		#endregion

		#region Private Graphics Object Disposal Queues

		private ConcurrentQueue<IGLTexture> GCTextures = new ConcurrentQueue<IGLTexture>();
		private ConcurrentQueue<IGLRenderbuffer> GCRenderbuffers = new ConcurrentQueue<IGLRenderbuffer>();
		private ConcurrentQueue<IGLBuffer> GCVertexBuffers = new ConcurrentQueue<IGLBuffer>();
		private ConcurrentQueue<IGLBuffer> GCIndexBuffers = new ConcurrentQueue<IGLBuffer>();
		private ConcurrentQueue<IGLEffect> GCEffects = new ConcurrentQueue<IGLEffect>();
		private ConcurrentQueue<IGLQuery> GCQueries = new ConcurrentQueue<IGLQuery>();

		#endregion

		#region Thread Functions

		private void csThreadProc()
		{
			while (!csDone)
			{
				csEvent.WaitOne();
				lock (actions)
				{
					foreach (Action action in actions)
					{
						action();
					}
					actions.Clear();
				}
			}
			GLDevice.Dispose();
		}

		private void ForceToMainThread(Action action)
		{
			ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
			lock (actions)
			{
				actions.Add(() =>
				{
					action();
					resetEvent.Set();
				});
			}
			csEvent.Set();
			resetEvent.Wait();
		}

		#endregion

		#region Constructor/Disposal

		public ThreadedGLDevice(PresentationParameters presentationParameters)
		{
			csThread = new Thread(new ThreadStart(csThreadProc));
			csThread.Start();

			FNALoggerEXT.LogInfo("Running with ThreadedGLDevice!");
			ForceToMainThread(() =>
			{
				if (Environment.GetEnvironmentVariable(
					"FNA_THREADEDGLDEVICE_GLDEVICE"
				) == "OpenGLDevice") {
					GLDevice = new OpenGLDevice(
						presentationParameters
					);
				}
				else
				{
					GLDevice = new ModernGLDevice(
						presentationParameters
					);
				}
			}); // End ForceToMainThread
		}

		public void Dispose()
		{
			ForceToMainThread(() =>
			{
				csDone = true;
			}); // End ForceToMainThread
			csThread.Join();
		}

		#endregion

		#region BeginFrame Operations

		public void BeginFrame()
		{
			ForceToMainThread(() =>
			{
				GLDevice.BeginFrame();
			}); // End ForceToMainThread
		}

		#endregion

		#region Backbuffer Operations

		public void ResetBackbuffer(PresentationParameters presentationParameters)
		{
			ForceToMainThread(() =>
			{
				GLDevice.ResetBackbuffer(
					presentationParameters
				);
			}); // End ForceToMainThread
		}

		public void SwapBuffers(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			ForceToMainThread(() =>
			{
				GLDevice.SwapBuffers(
					sourceRectangle,
					destinationRectangle,
					overrideWindowHandle
				);

				IGLTexture gcTexture;
				while (GCTextures.TryDequeue(out gcTexture))
				{
					GLDevice.AddDisposeTexture(gcTexture);
				}
				IGLRenderbuffer gcRenderbuffer;
				while (GCRenderbuffers.TryDequeue(out gcRenderbuffer))
				{
					GLDevice.AddDisposeRenderbuffer(gcRenderbuffer);
				}
				IGLBuffer gcBuffer;
				while (GCVertexBuffers.TryDequeue(out gcBuffer))
				{
					GLDevice.AddDisposeVertexBuffer(gcBuffer);
				}
				while (GCIndexBuffers.TryDequeue(out gcBuffer))
				{
					GLDevice.AddDisposeIndexBuffer(gcBuffer);
				}
				IGLEffect gcEffect;
				while (GCEffects.TryDequeue(out gcEffect))
				{
					GLDevice.AddDisposeEffect(gcEffect);
				}
				IGLQuery gcQuery;
				while (GCQueries.TryDequeue(out gcQuery))
				{
					GLDevice.AddDisposeQuery(gcQuery);
				}
			}); // End ForceToMainThread
		}

		#endregion

		#region SetStringMarkerEXT

		public void SetStringMarker(string text)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetStringMarker(text);
			}); // End ForceToMainThread
		}

		#endregion

		#region Draw Calls

		public void DrawIndexedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			IGLBuffer indices,
			IndexElementSize indexElementSize
		) {
			ForceToMainThread(() =>
			{
				GLDevice.DrawIndexedPrimitives(
					primitiveType,
					baseVertex,
					minVertexIndex,
					numVertices,
					startIndex,
					primitiveCount,
					indices,
					indexElementSize
				);
			}); // End ForceToMainThread
		}

		public void DrawInstancedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount,
			IGLBuffer indices,
			IndexElementSize indexElementSize
		) {
			ForceToMainThread(() =>
			{
				GLDevice.DrawInstancedPrimitives(
					primitiveType,
					baseVertex,
					minVertexIndex,
					numVertices,
					startIndex,
					primitiveCount,
					instanceCount,
					indices,
					indexElementSize
				);
			}); // End ForceToMainThread
		}

		public void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		) {
			ForceToMainThread(() =>
			{
				GLDevice.DrawPrimitives(
					primitiveType,
					vertexStart,
					primitiveCount
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.DrawUserIndexedPrimitives(
					primitiveType,
					vertexData,
					vertexOffset,
					numVertices,
					indexData,
					indexOffset,
					indexElementSize,
					primitiveCount
				);
			}); // End ForceToMainThread
		}

		public void DrawUserPrimitives(
			PrimitiveType primitiveType,
			IntPtr vertexData,
			int vertexOffset,
			int primitiveCount
		) {
			ForceToMainThread(() =>
			{
				GLDevice.DrawUserPrimitives(
					primitiveType,
					vertexData,
					vertexOffset,
					primitiveCount
				);
			}); // End ForceToMainThread
		}

		#endregion

		#region Render States

		public void SetPresentationInterval(PresentInterval presentInterval)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetPresentationInterval(presentInterval);
			}); // End ForceToMainThread
		}

		public void SetViewport(Viewport vp)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetViewport(vp);
			}); // End ForceToMainThread
		}

		public void SetScissorRect(Rectangle scissorRect)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetScissorRect(scissorRect);
			}); // End ForceToMainThread
		}

		public void SetBlendState(BlendState blendState)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetBlendState(blendState);
			}); // End ForceToMainThread
		}

		public void SetDepthStencilState(DepthStencilState depthStencilState)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetDepthStencilState(
					depthStencilState
				);
			}); // End ForceToMainThread
		}

		public void ApplyRasterizerState(RasterizerState rasterizerState)
		{
			ForceToMainThread(() =>
			{
				GLDevice.ApplyRasterizerState(rasterizerState);
			}); // End ForceToMainThread
		}

		public void VerifySampler(
			int index,
			Texture texture,
			SamplerState sampler
		) {
			ForceToMainThread(() =>
			{
				GLDevice.VerifySampler(index, texture, sampler);
			}); // End ForceToMainThread
		}

		#endregion

		#region Target Reads/Writes

		public void Clear(
			ClearOptions options,
			Vector4 color,
			float depth,
			int stencil
		) {
			ForceToMainThread(() =>
			{
				GLDevice.Clear(options, color, depth, stencil);
			}); // End ForceToMainThread
		}

		public void SetRenderTargets(
			RenderTargetBinding[] renderTargets,
			IGLRenderbuffer renderbuffer,
			DepthFormat depthFormat
		) {
			ForceToMainThread(() =>
			{
				GLDevice.SetRenderTargets(
					renderTargets,
					renderbuffer,
					depthFormat
				);
			}); // End ForceToMainThread
		}

		public void ResolveTarget(RenderTargetBinding target)
		{
			ForceToMainThread(() =>
			{
				GLDevice.ResolveTarget(target);
			}); // End ForceToMainThread
		}

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
			ForceToMainThread(() =>
			{
				GLDevice.ReadBackbuffer(
					data,
					dataLen,
					startIndex,
					elementCount,
					elementSizeInBytes,
					subX,
					subY,
					subW,
					subH
				);
			}); // End ForceToMainThread
		}

		#endregion

		#region Textures

		public IGLTexture CreateTexture2D(
			SurfaceFormat format,
			int width,
			int height,
			int levelCount,
			bool isRenderTarget
		) {
			IGLTexture result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CreateTexture2D(
					format,
					width,
					height,
					levelCount,
					isRenderTarget
				);
			}); // End ForceToMainThread
			return result;
		}

		public IGLTexture CreateTexture3D(
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount
		) {
			IGLTexture result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CreateTexture3D(
					format,
					width,
					height,
					depth,
					levelCount
				);
			}); // End ForceToMainThread
			return result;
		}

		public IGLTexture CreateTextureCube(
			SurfaceFormat format,
			int size,
			int levelCount,
			bool isRenderTarget
		) {
			IGLTexture result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CreateTextureCube(
					format,
					size,
					levelCount,
					isRenderTarget
				);
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeTexture(IGLTexture texture)
		{
			GCTextures.Enqueue(texture);
		}

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
			ForceToMainThread(() =>
			{
				GLDevice.SetTextureData2D(
					texture,
					format,
					x,
					y,
					w,
					h,
					level,
					data,
					dataLength
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.SetTextureData3D(
					texture,
					format,
					level,
					left,
					top,
					right,
					bottom,
					front,
					back,
					data,
					dataLength
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.SetTextureDataCube(
					texture,
					format,
					xOffset,
					yOffset,
					width,
					height,
					cubeMapFace,
					level,
					data,
					dataLength
				);
			}); // End ForceToMainThread
		}

		public void SetTextureDataYUV(Texture2D[] textures, IntPtr ptr)
		{
			ForceToMainThread(() =>
			{
				GLDevice.SetTextureDataYUV(textures, ptr);
			}); // End ForceToMainThread
		}

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
			ForceToMainThread(() =>
			{
				GLDevice.GetTextureData2D(
					texture,
					format,
					width,
					height,
					level,
					subX,
					subY,
					subW,
					subH,
					data,
					startIndex,
					elementCount,
					elementSizeInBytes
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.GetTextureData3D(
					texture,
					format,
					left,
					top,
					front,
					right,
					bottom,
					back,
					level,
					data,
					startIndex,
					elementCount,
					elementSizeInBytes
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.GetTextureDataCube(
					texture,
					format,
					size,
					cubeMapFace,
					level,
					subX,
					subY,
					subW,
					subH,
					data,
					startIndex,
					elementCount,
					elementSizeInBytes
				);
			}); // End ForceToMainThread
		}

		#endregion

		#region Renderbuffers

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount,
			IGLTexture texture
		) {
			IGLRenderbuffer result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.GenRenderbuffer(
					width,
					height,
					format,
					multiSampleCount,
					texture
				);
			}); // End ForceToMainThread
			return result;
		}

		public IGLRenderbuffer GenRenderbuffer(
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount
		) {
			IGLRenderbuffer result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.GenRenderbuffer(
					width,
					height,
					format,
					multiSampleCount
				);
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeRenderbuffer(IGLRenderbuffer renderbuffer)
		{
			GCRenderbuffers.Enqueue(renderbuffer);
		}

		#endregion

		#region Vertex/Index Buffers

		public IGLBuffer GenVertexBuffer(
			bool dynamic,
			BufferUsage usage,
			int vertexCount,
			int vertexStride
		) {
			IGLBuffer result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.GenVertexBuffer(
					dynamic,
					usage,
					vertexCount,
					vertexStride
				);
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeVertexBuffer(IGLBuffer buffer)
		{
			GCVertexBuffers.Enqueue(buffer);
		}

		public void SetVertexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			ForceToMainThread(() =>
			{
				GLDevice.SetVertexBufferData(
					buffer,
					offsetInBytes,
					data,
					dataLength,
					options
				);
			}); // End ForceToMainThread
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
			ForceToMainThread(() =>
			{
				GLDevice.GetVertexBufferData(
					buffer,
					offsetInBytes,
					data,
					startIndex,
					elementCount,
					elementSizeInBytes,
					vertexStride
				);
			}); // End ForceToMainThread
		}

		public IGLBuffer GenIndexBuffer(
			bool dynamic,
			BufferUsage usage,
			int indexCount,
			IndexElementSize indexElementSize
		) {
			IGLBuffer result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.GenIndexBuffer(
					dynamic,
					usage,
					indexCount,
					indexElementSize
				);
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeIndexBuffer(IGLBuffer buffer)
		{
			GCIndexBuffers.Enqueue(buffer);
		}

		public void SetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			ForceToMainThread(() =>
			{
				GLDevice.SetIndexBufferData(
					buffer,
					offsetInBytes,
					data,
					dataLength,
					options
				);
			}); // End ForceToMainThread
		}

		public void GetIndexBufferData(
			IGLBuffer buffer,
			int offsetInBytes,
			IntPtr data,
			int startIndex,
			int elementCount,
			int elementSizeInBytes
		) {
			ForceToMainThread(() =>
			{
				GLDevice.GetIndexBufferData(
					buffer,
					offsetInBytes,
					data,
					startIndex,
					elementCount,
					elementSizeInBytes
				);
			}); // End ForceToMainThread
		}

		#endregion

		#region Effects

		public IGLEffect CreateEffect(byte[] effectCode)
		{
			IGLEffect result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CreateEffect(effectCode);
			}); // End ForceToMainThread
			return result;
		}

		public IGLEffect CloneEffect(IGLEffect effect)
		{
			IGLEffect result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CloneEffect(effect);
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeEffect(IGLEffect effect)
		{
			GCEffects.Enqueue(effect);
		}

		public void ApplyEffect(
			IGLEffect effect,
			IntPtr technique,
			uint pass,
			IntPtr stateChanges
		) {
			ForceToMainThread(() =>
			{
				GLDevice.ApplyEffect(
					effect,
					technique,
					pass,
					stateChanges
				);
			}); // End ForceToMainThread
		}

		public void BeginPassRestore(IGLEffect effect, IntPtr stateChanges)
		{
			ForceToMainThread(() =>
			{
				GLDevice.BeginPassRestore(effect, stateChanges);
			}); // End ForceToMainThread
		}

		public void EndPassRestore(IGLEffect effect)
		{
			ForceToMainThread(() =>
			{
				GLDevice.EndPassRestore(effect);
			}); // End ForceToMainThread
		}

		#endregion

		#region Vertex Attributes

		public void ApplyVertexAttributes(
			VertexBufferBinding[] bindings,
			int numBindings,
			bool bindingsUpdated,
			int baseVertex
		) {
			ForceToMainThread(() =>
			{
				GLDevice.ApplyVertexAttributes(
					bindings,
					numBindings,
					bindingsUpdated,
					baseVertex
				);
			}); // End ForceToMainThread
		}

		public void ApplyVertexAttributes(
			VertexDeclaration vertexDeclaration,
			IntPtr ptr,
			int vertexOffset
		) {
			ForceToMainThread(() =>
			{
				GLDevice.ApplyVertexAttributes(
					vertexDeclaration,
					ptr,
					vertexOffset
				);
			}); // End ForceToMainThread
		}

		#endregion

		#region Queries

		public IGLQuery CreateQuery()
		{
			IGLQuery result = null;
			ForceToMainThread(() =>
			{
				result = GLDevice.CreateQuery();
			}); // End ForceToMainThread
			return result;
		}

		public void AddDisposeQuery(IGLQuery query)
		{
			GCQueries.Enqueue(query);
		}

		public void QueryBegin(IGLQuery query)
		{
			ForceToMainThread(() =>
			{
				GLDevice.QueryBegin(query);
			}); // End ForceToMainThread
		}

		public void QueryEnd(IGLQuery query)
		{
			ForceToMainThread(() =>
			{
				GLDevice.QueryEnd(query);
			}); // End ForceToMainThread
		}

		public bool QueryComplete(IGLQuery query)
		{
			bool result = false;
			ForceToMainThread(() =>
			{
				result = GLDevice.QueryComplete(query);
			}); // End ForceToMainThread
			return result;
		}

		public int QueryPixelCount(IGLQuery query)
		{
			int result = 0;
			ForceToMainThread(() =>
			{
				result = GLDevice.QueryPixelCount(query);
			}); // End ForceToMainThread
			return result;
		}

		#endregion
	}
}
