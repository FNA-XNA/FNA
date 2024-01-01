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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicIndexBuffer : IndexBuffer
	{
		#region Public Properties

		public bool IsContentLost
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ContentLost Event

#pragma warning disable 0067
		// We never lose data, but lol XNA4 compliance -flibit
		public event EventHandler<EventArgs> ContentLost;
#pragma warning restore 0067

		#endregion

		#region Public Constructors

		public DynamicIndexBuffer(
			GraphicsDevice graphicsDevice,
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage usage
		) : base(
			graphicsDevice,
			indexElementSize,
			indexCount,
			usage,
			true
		) {
		}

		public DynamicIndexBuffer(
			GraphicsDevice graphicsDevice,
			Type indexType,
			int indexCount,
			BufferUsage usage
		) : base(
			graphicsDevice,
			indexType,
			indexCount,
			usage,
			true
		) {
		}

		#endregion

		#region Public SetData Methods

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			SetDataOptions options
		) where T : struct {
			ErrorCheck(data, startIndex, elementCount);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * MarshalHelper.SizeOf<T>()),
				elementCount * MarshalHelper.SizeOf<T>(),
				options
			);
			handle.Free();
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount,
			SetDataOptions options
		) where T : struct {
			ErrorCheck(data, startIndex, elementCount);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				buffer,
				0,
				handle.AddrOfPinnedObject() + (startIndex * MarshalHelper.SizeOf<T>()),
				elementCount * MarshalHelper.SizeOf<T>(),
				options
			);
			handle.Free();
		}

		#endregion
	}
}
