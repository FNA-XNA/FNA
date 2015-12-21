#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class Texture2DReader : ContentTypeReader<Texture2D>
	{
		#region Private Supported File Extensions Variable

		private static string[] supportedExtensions = new string[]
		{
			".bmp", ".gif", ".jpg", ".jpeg", ".png", ".tga", ".tif", ".tiff"
		};

		#endregion

		#region Internal Constructor

		internal Texture2DReader()
		{
		}

		#endregion

		#region Internal Filename Normalizer Method

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		#endregion

		#region Protected Read Method

		protected internal override Texture2D Read(
			ContentReader reader,
			Texture2D existingInstance
		) {
			Texture2D texture = null;

			SurfaceFormat surfaceFormat;
			if (reader.version < 5)
			{
				/* These integer values are based on the enum values
				 * from previous XNA versions.
				 * -flibit
				 */
				int legacyFormat = reader.ReadInt32();
				if (legacyFormat == 1)
				{
					surfaceFormat = SurfaceFormat.Color;
				}
				if (legacyFormat == 28)
				{
					surfaceFormat = SurfaceFormat.Dxt1;
				}
				else if (legacyFormat == 30)
				{
					surfaceFormat = SurfaceFormat.Dxt3;
				}
				else if (legacyFormat == 32)
				{
					surfaceFormat = SurfaceFormat.Dxt5;
				}
				else
				{
					throw new NotSupportedException(
						"Unsupported legacy surface format."
					);
				}
			}
			else
			{
				surfaceFormat = (SurfaceFormat) reader.ReadInt32();
			}
			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			int levelCount = reader.ReadInt32();
			int levelCountOutput = levelCount;

			// Check to see if we need to convert the surface data
			SurfaceFormat convertedFormat = surfaceFormat;
			if (	surfaceFormat == SurfaceFormat.Dxt1 &&
				!reader.GraphicsDevice.GLDevice.SupportsDxt1	)
			{
				convertedFormat = SurfaceFormat.Color;
			}
			else if (	(	surfaceFormat == SurfaceFormat.Dxt3 ||
						surfaceFormat == SurfaceFormat.Dxt5	) &&
					!reader.GraphicsDevice.GLDevice.SupportsS3tc	)
			{
				convertedFormat = SurfaceFormat.Color;
			}

			// Check for duplicate instances
			if (existingInstance == null)
			{
				texture = new Texture2D(
					reader.GraphicsDevice,
					width,
					height,
					levelCountOutput > 1,
					convertedFormat
				);
			}
			else
			{
				texture = existingInstance;
			}

			for (int level = 0; level < levelCount; level += 1)
			{
				int levelDataSizeInBytes = reader.ReadInt32();
				byte[] levelData = null; // Don't assign this quite yet...
				int levelWidth = width >> level;
				int levelHeight = height >> level;
				if (level >= levelCountOutput)
				{
					continue;
				}

				// Convert the image data if required
				if (	surfaceFormat == SurfaceFormat.Dxt1 &&
					!reader.GraphicsDevice.GLDevice.SupportsDxt1	)
				{
						levelData = reader.ReadBytes(levelDataSizeInBytes);
						levelData = DxtUtil.DecompressDxt1(
							levelData,
							levelWidth,
							levelHeight
						);
				}
				else if (	surfaceFormat == SurfaceFormat.Dxt3 &&
						!reader.GraphicsDevice.GLDevice.SupportsS3tc	)
				{
						levelData = reader.ReadBytes(levelDataSizeInBytes);
						levelData = DxtUtil.DecompressDxt3(
							levelData,
							levelWidth,
							levelHeight
						);
				}
				else if (	surfaceFormat == SurfaceFormat.Dxt5 &&
						!reader.GraphicsDevice.GLDevice.SupportsS3tc	)
				{
						levelData = reader.ReadBytes(levelDataSizeInBytes);
						levelData = DxtUtil.DecompressDxt5(
							levelData,
							levelWidth,
							levelHeight
						);
				}

				if (	levelData == null &&
					reader.BaseStream.GetType() != typeof(System.IO.MemoryStream)	)
				{
					/* If the ContentReader is not backed by a
					 * MemoryStream, we have to read the data in.
					 */
					levelData = reader.ReadBytes(levelDataSizeInBytes);
				}
				if (levelData != null)
				{
					/* If we had to convert the data, or get the data from a
					 * non-MemoryStream, we set the data with our levelData
					 * reference.
					 */
					texture.SetData(level, null, levelData, 0, levelData.Length);
				}
				else
				{
					/* Ideally, we didn't have to perform any conversion or
					 * unnecessary reading. Just throw the buffer directly
					 * into SetData, skipping a redundant byte[] copy.
					 */
					texture.SetData<byte>(
						level,
						null,
						(((System.IO.MemoryStream) (reader.BaseStream)).GetBuffer()),
						(int) reader.BaseStream.Position,
						levelDataSizeInBytes
					);
					reader.BaseStream.Seek(
						levelDataSizeInBytes,
						System.IO.SeekOrigin.Current
					);
				}

			}

			return texture;
		}

		#endregion
	}
}
