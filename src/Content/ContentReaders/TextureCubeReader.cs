#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
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
		#region Private Supported File Extensions Variable

		private static string[] supportedExtensions = new string[]
		{
			".dds"
		};

		#endregion

		#region Internal Filename Normalizer Method

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		#endregion

		#region Protected Read Method

		protected internal override TextureCube Read(
			ContentReader reader,
			TextureCube existingInstance
		) {
			TextureCube textureCube;

			SurfaceFormat surfaceFormat = (SurfaceFormat) reader.ReadInt32();
			int size = reader.ReadInt32();
			int levels = reader.ReadInt32();

			if (existingInstance == null)
			{
				textureCube = new TextureCube(
					reader.ContentManager.GetGraphicsDevice(),
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
