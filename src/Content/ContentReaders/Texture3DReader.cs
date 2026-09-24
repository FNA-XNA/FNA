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
using System.IO;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class Texture3DReader : ContentTypeReader<Texture3D>
	{
		#region Protected Read Method

		protected internal override Texture3D Read(
			ContentReader reader,
			Texture3D existingInstance
		) {
			Texture3D texture;

			SurfaceFormat format = (SurfaceFormat) reader.ReadInt32();
			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			int depth = reader.ReadInt32();
			int levelCount = reader.ReadInt32();

			if (existingInstance == null)
			{
				texture = new Texture3D(
					reader.ContentManager.GetGraphicsDevice(),
					width,
					height,
					depth,
					levelCount > 1,
					format
				);
			}
			else
			{
				texture = existingInstance;
			}

			for (int i = 0; i < levelCount; i += 1)
			{
				int dataSize = reader.ReadInt32();
				byte[] data;
				int offset;
				MemoryStream memoryStream = reader.BaseStream as MemoryStream;
				if (memoryStream == null)
				{
					data = SharedBuffer.Rent(dataSize);
					reader.Read(data, 0, dataSize);
					offset = 0;
				}
				else
				{
					data = memoryStream.GetBuffer();
					offset = (int) memoryStream.Position;
					memoryStream.Position = offset + dataSize;
				}
				texture.SetData(
					i,
					0,
					0,
					width,
					height,
					0,
					depth,
					data,
					offset,
					dataSize
				);

				// Calculate dimensions of next mip level.
				width = Math.Max(width >> 1, 1);
				height = Math.Max(height >> 1, 1);
				depth = Math.Max(depth >> 1, 1);
			}

			return texture;
		}

		#endregion
	}
}
