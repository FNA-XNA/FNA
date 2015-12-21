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
	internal class TextureCubeReader : ContentTypeReader<TextureCube>
	{
		#region Protected Read Method

		protected internal override TextureCube Read(
			ContentReader reader,
			TextureCube existingInstance
		) {
			TextureCube textureCube = null;

			SurfaceFormat surfaceFormat = (SurfaceFormat) reader.ReadInt32();
			int size = reader.ReadInt32();
			int levels = reader.ReadInt32();

			if (existingInstance == null)
			{
				textureCube = new TextureCube(
					reader.GraphicsDevice,
					size,
					levels > 1,
					surfaceFormat
				);
			}
			else
			{
				textureCube = existingInstance;
			}

			for (int face = 0; face < 6; face += 1)
			{
				for (int i = 0; i < levels; i += 1)
				{
					int faceSize = reader.ReadInt32();
					byte[] faceData = reader.ReadBytes(faceSize);
					textureCube.SetData<byte>(
						(CubeMapFace) face,
						i,
						null,
						faceData,
						0,
						faceSize
					);
				}
			}

			return textureCube;
		}

		#endregion
	}
}
