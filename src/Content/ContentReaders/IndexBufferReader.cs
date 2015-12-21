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
			byte[] data = input.ReadBytes(dataSize);
			if (indexBuffer == null)
			{
				if (sixteenBits)
				{
					indexBuffer = new IndexBuffer(
						input.GraphicsDevice,
						IndexElementSize.SixteenBits,
						dataSize / 2,
						BufferUsage.None
					);
				}
				else
				{
					indexBuffer = new IndexBuffer(
						input.GraphicsDevice,
						IndexElementSize.ThirtyTwoBits,
						dataSize / 4,
						BufferUsage.None
					);
				}
			}

			indexBuffer.SetData(data);
			return indexBuffer;
		}

		#endregion
	}
}
