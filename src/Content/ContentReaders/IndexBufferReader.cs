#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.IO;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	class IndexBufferReader : ContentTypeReader<IndexBuffer>
	{
		#region Protected Read Method

		protected internal override IndexBuffer Read(
			ContentReader input,
			IndexBuffer existingInstance
		) {
			IndexBuffer indexBuffer = existingInstance;
			bool sixteenBits = input.ReadBoolean();
			int dataSize = input.ReadInt32();
			byte[] data;
			int offset;
			MemoryStream memoryStream = input.BaseStream as MemoryStream;
			if (memoryStream == null)
			{
				data = SharedBuffer.Rent(dataSize);
				input.Read(data, 0, dataSize);
				offset = 0;
			}
			else
			{
				data = memoryStream.GetBuffer();
				offset = (int) memoryStream.Position;
				memoryStream.Position = offset + dataSize;
			}
			if (indexBuffer == null)
			{
				if (sixteenBits)
				{
					indexBuffer = new IndexBuffer(
						input.ContentManager.GetGraphicsDevice(),
						IndexElementSize.SixteenBits,
						dataSize / 2,
						BufferUsage.None
					);
				}
				else
				{
					indexBuffer = new IndexBuffer(
						input.ContentManager.GetGraphicsDevice(),
						IndexElementSize.ThirtyTwoBits,
						dataSize / 4,
						BufferUsage.None
					);
				}
			}

			indexBuffer.SetData(data, offset, dataSize);
			return indexBuffer;
		}

		#endregion
	}
}
