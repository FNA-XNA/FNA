#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class VertexDeclarationReader : ContentTypeReader<VertexDeclaration>
	{
		#region Protected Read Method

		protected internal override VertexDeclaration Read(
			ContentReader reader,
			VertexDeclaration existingInstance
		) {
			int vertexStride = reader.ReadInt32();
			int elementCount = reader.ReadInt32();
			VertexElement[] elements = new VertexElement[elementCount];
			for (int i = 0; i < elementCount; i += 1)
			{
				int offset = reader.ReadInt32();
				VertexElementFormat elementFormat = (VertexElementFormat) reader.ReadInt32();
				VertexElementUsage elementUsage = (VertexElementUsage) reader.ReadInt32();
				int usageIndex = reader.ReadInt32();
				elements[i] = new VertexElement(
					offset,
					elementFormat,
					elementUsage,
					usageIndex
				);
			}

			/* TODO: This process generates alot of duplicate VertexDeclarations
			 * which in turn complicates other systems trying to share GPU resources
			 * like DX11 vertex input layouts.
			 *
			 * We should consider caching vertex declarations here and returning
			 * previously created declarations when they are in our cache.
			 */
			return new VertexDeclaration(vertexStride, elements);
		}

		#endregion
	}
}

