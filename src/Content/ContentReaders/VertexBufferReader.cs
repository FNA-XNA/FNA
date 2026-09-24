#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System.IO;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	class VertexBufferReader : ContentTypeReader<VertexBuffer>
	{
		#region Protected Read Method

		protected internal override VertexBuffer Read(
			ContentReader input,
			VertexBuffer existingInstance
		) {
			VertexDeclaration declaration = input.ReadRawObject<VertexDeclaration>();
			int vertexCount = (int) input.ReadUInt32();
			int bufferSize = vertexCount * declaration.VertexStride;
			byte[] data;
			int offset;
			MemoryStream memoryStream = input.BaseStream as MemoryStream;
			if (memoryStream == null)
			{
				data = SharedBuffer.Rent(bufferSize);
				input.Read(data, 0, bufferSize);
				offset = 0;
			}
			else
			{
				data = memoryStream.GetBuffer();
				offset = (int) memoryStream.Position;
				memoryStream.Position = offset + bufferSize;
			}

			VertexBuffer buffer = new VertexBuffer(
				input.ContentManager.GetGraphicsDevice(),
				declaration,
				vertexCount,
				BufferUsage.None
			);
			buffer.SetData(data, offset, bufferSize);
			return buffer;
		}

		#endregion
	}
}
