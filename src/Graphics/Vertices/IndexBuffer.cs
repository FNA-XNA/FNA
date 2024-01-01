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
using System.Threading;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class IndexBuffer : GraphicsResource
	{
		#region Public Properties

		public BufferUsage BufferUsage
		{
			get;
			private set;
		}

		public int IndexCount
		{
			get;
			private set;
		}

		public IndexElementSize IndexElementSize
		{
			get;
			private set;
		}

		#endregion

		#region Internal FNA3D Variables

		internal IntPtr buffer;

		#endregion

		#region Public Constructors

		public IndexBuffer(
			GraphicsDevice graphicsDevice,
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage bufferUsage
		) : this(
			graphicsDevice,
			indexElementSize,
			indexCount,
			bufferUsage,
			false
		) {
		}

		public IndexBuffer(
			GraphicsDevice graphicsDevice,
			Type indexType,
			int indexCount,
			BufferUsage usage
		) : this(
			graphicsDevice,
			SizeForType(graphicsDevice, indexType),
			indexCount,
			usage,
			false
		) {
		}

		#endregion

		#region Protected Constructors

		protected IndexBuffer(
			GraphicsDevice graphicsDevice,
			Type indexType,
			int indexCount,
			BufferUsage usage,
			bool dynamic
		) : this(
			graphicsDevice,
			SizeForType(graphicsDevice, indexType),
			indexCount,
			usage,
			dynamic
		) {
		}

		protected IndexBuffer(
			GraphicsDevice graphicsDevice,
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage usage,
			bool dynamic
		) {
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			GraphicsDevice = graphicsDevice;
			IndexElementSize = indexElementSize;
			IndexCount = indexCount;
			BufferUsage = usage;

			int stride = (indexElementSize == IndexElementSize.ThirtyTwoBits) ? 4 : 2;

			buffer = FNA3D.FNA3D_GenIndexBuffer(
				GraphicsDevice.GLDevice,
				(byte) (dynamic ? 1 : 0),
				usage,
				IndexCount * stride
			);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IntPtr toDispose = Interlocked.Exchange(ref buffer, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeIndexBuffer(
						GraphicsDevice.GLDevice,
						toDispose
					);
				}
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
				data.Length
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
				elementCount
			);
		}

		public void GetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.Length < (startIndex + elementCount))
			{
				throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
			}
			if (BufferUsage == BufferUsage.WriteOnly)
			{
				throw new NotSupportedException(
					"This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. " +
					"Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported."
				);
			}

			int elementSizeInBytes = MarshalHelper.SizeOf<T>();
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_GetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				elementCount * elementSizeInBytes
			);
			handle.Free();
		}

		#endregion

		#region Public SetData Methods

		public void SetData<T>(T[] data) where T : struct
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				0,
				handle.AddrOfPinnedObject(),
				data.Length * MarshalHelper.SizeOf<T>(),
				SetDataOptions.None
			);
			handle.Free();
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			ErrorCheck(data, startIndex, elementCount);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				0,
				handle.AddrOfPinnedObject() + (startIndex * MarshalHelper.SizeOf<T>()),
				elementCount * MarshalHelper.SizeOf<T>(),
				SetDataOptions.None
			);
			handle.Free();
		}

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			ErrorCheck(data, startIndex, elementCount);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * MarshalHelper.SizeOf<T>()),
				elementCount * MarshalHelper.SizeOf<T>(),
				SetDataOptions.None
			);
			handle.Free();
		}

		#endregion

		#region Public Extensions

		public void SetDataPointerEXT(
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options
		) {
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				offsetInBytes,
				data,
				dataLength,
				options
			);
		}

		#endregion

		#region Internal Methods

		[System.Diagnostics.Conditional("DEBUG")]
		internal void ErrorCheck<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.Length < (startIndex + elementCount))
			{
				throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
			}
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

		#region Private Type Size Calculator
		
		/// <summary>
		/// Gets the relevant IndexElementSize enum value for the given type.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <param name="type">The type to use for the index buffer</param>
		/// <returns>The IndexElementSize enum value that matches the type</returns>
		private static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
		{
			int sizeInBytes = Marshal.SizeOf(type);

			if (sizeInBytes == 2)
			{
				return IndexElementSize.SixteenBits;
			}
			if (sizeInBytes == 4)
			{
				return IndexElementSize.ThirtyTwoBits;
			}

			throw new ArgumentOutOfRangeException(
				"type",
				"Index buffers can only be created for types" +
				" that are sixteen or thirty two bits in length"
			);
		}

		#endregion
	}
}
