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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : GraphicsResource
	{
		#region Public Properties

		public BufferUsage BufferUsage
		{
			get;
			private set;
		}

		public int VertexCount
		{
			get;
			private set;
		}

		public VertexDeclaration VertexDeclaration
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		internal IGLBuffer buffer;

		#endregion

		#region Public Constructors

		public VertexBuffer(
			GraphicsDevice graphicsDevice,
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage
		) : this(
			graphicsDevice,
			vertexDeclaration,
			vertexCount,
			bufferUsage,
			false
		) {
		}

		public VertexBuffer(
			GraphicsDevice graphicsDevice,
			Type type,
			int vertexCount,
			BufferUsage bufferUsage
		) : this(
			graphicsDevice,
			VertexDeclaration.FromType(type),
			vertexCount,
			bufferUsage,
			false
		) {
		}

		#endregion

		#region Protected Constructor

		protected VertexBuffer(
			GraphicsDevice graphicsDevice,
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage,
			bool dynamic
		) {
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			GraphicsDevice = graphicsDevice;
			VertexDeclaration = vertexDeclaration;
			VertexCount = vertexCount;
			BufferUsage = bufferUsage;

			// Make sure the graphics device is assigned in the vertex declaration.
			if (vertexDeclaration.GraphicsDevice != graphicsDevice)
			{
				vertexDeclaration.GraphicsDevice = graphicsDevice;
			}

			buffer = GraphicsDevice.GLDevice.GenVertexBuffer(
				dynamic,
				VertexCount,
				VertexDeclaration.VertexStride
			);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				GraphicsDevice.GLDevice.AddDisposeVertexBuffer(buffer);
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Public GetData Methods

		public void GetData<T>(T[] data) where T : struct
		{
			GetData<T>(
				0,
				data,
				0,
				data.Length,
				Marshal.SizeOf(typeof(T))
			);
		}

		public void GetData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GetData<T>(
				0,
				data,
				startIndex,
				elementCount,
				Marshal.SizeOf(typeof(T))
			);
		}

		public void GetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride
		) where T : struct {
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.Length < (startIndex + elementCount))
			{
				throw new ArgumentOutOfRangeException(
					"elementCount",
					"This parameter must be a valid index within the array."
				);
			}
			if (BufferUsage == BufferUsage.WriteOnly)
			{
				throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			}

			if (vertexStride == 0)
			{
				vertexStride = Marshal.SizeOf(typeof(T));
			}
			if (	elementCount > 1 &&
				(elementCount * vertexStride) > (VertexCount * VertexDeclaration.VertexStride)	)
			{
				throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");
			}

			GraphicsDevice.GLDevice.GetVertexBufferData(
				buffer,
				offsetInBytes,
				data,
				startIndex,
				elementCount,
				vertexStride
			);
		}

		#endregion

		#region Public SetData Methods

		public void SetData<T>(T[] data) where T : struct
		{
			SetDataInternal<T>(
				0,
				data,
				0,
				data.Length,
				Marshal.SizeOf(typeof(T)),
				SetDataOptions.None
			);
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			SetDataInternal<T>(
				0,
				data,
				startIndex,
				elementCount,
				Marshal.SizeOf(typeof(T)),
				SetDataOptions.None
			);
		}

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride
		) where T : struct {
			SetDataInternal<T>(
				offsetInBytes,
				data,
				startIndex,
				elementCount,
				vertexStride,
				SetDataOptions.None
			);
		}

		#endregion

		#region Internal Master SetData Methods

		protected void SetDataInternal<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride,
			SetDataOptions options
		) where T : struct {
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if ((startIndex + elementCount > data.Length) || elementCount <= 0)
			{
				throw new InvalidOperationException(
					"The array specified in the data parameter" +
					" is not the correct size for the amount of" +
					" data requested."
				);
			}

			if (	elementCount > 1 &&
				(elementCount * vertexStride > (int) buffer.BufferSize)	)
			{
				throw new InvalidOperationException(
					"The vertex stride is larger than the vertex buffer."
				);
			}

			int elementSizeInBytes = Marshal.SizeOf(typeof(T));
			if (vertexStride == 0)
			{
				vertexStride = elementSizeInBytes;
			}
			if (vertexStride < elementSizeInBytes)
			{
				throw new ArgumentOutOfRangeException(
					"The vertex stride must be greater than" +
					" or equal to the size of the specified data (" +
					elementSizeInBytes.ToString() + ")."
				);
			}

			GraphicsDevice.GLDevice.SetVertexBufferData(
				buffer,
				elementSizeInBytes,
				offsetInBytes,
				data,
				startIndex,
				elementCount,
				options
			);
		}

		#endregion

		#region Internal Context Reset Method

		/// <summary>
		/// The GraphicsDevice is resetting, so GPU resources must be recreated.
		/// </summary>
		internal protected override void GraphicsDeviceResetting()
		{
			// FIXME: Do we even want to bother with DeviceResetting for GL? -flibit
		}

		#endregion
	}
}
