#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
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
			byte[] data = input.ReadBytes(vertexCount * declaration.VertexStride);

			VertexBuffer buffer = new VertexBuffer(
				input.GraphicsDevice,
				declaration,
				vertexCount,
				BufferUsage.None
			);
			buffer.SetData(data);
			return buffer;
		}

		#endregion
	}
}
