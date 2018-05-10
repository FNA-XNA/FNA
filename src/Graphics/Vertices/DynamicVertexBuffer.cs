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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicVertexBuffer : VertexBuffer
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

		public DynamicVertexBuffer(
			GraphicsDevice graphicsDevice,
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage
		) : base(
			graphicsDevice,
			vertexDeclaration,
			vertexCount,
			bufferUsage,
			true
		) {
		}

		public DynamicVertexBuffer(
			GraphicsDevice graphicsDevice,
			Type type,
			int vertexCount,
			BufferUsage bufferUsage
		) : base(
			graphicsDevice,
			VertexDeclaration.FromType(type),
			vertexCount,
			bufferUsage,
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
			int vertexStride,
			SetDataOptions options
		) where T : struct {
			ErrorCheck(data, startIndex, elementCount, vertexStride);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.SetVertexBufferData(
				buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T))),
				elementCount * Marshal.SizeOf(typeof(T)),
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
			ErrorCheck(data, startIndex, elementCount, Marshal.SizeOf(typeof(T)));

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.SetVertexBufferData(
				buffer,
				0,
				handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T))),
				elementCount * Marshal.SizeOf(typeof(T)),
				options
			);
			handle.Free();
		}

		#endregion
	}
}
