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
using System;
using System.IO;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class Texture2DReader : ContentTypeReader<Texture2D>
	{
		#region Internal Constructor

		internal Texture2DReader()
		{
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
					surfaceFormat = SurfaceFormat.ColorBgraEXT;
				}
				else if (legacyFormat == 28)
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

			GraphicsDevice device = reader.ContentManager.GetGraphicsDevice();

			// Check to see if we need to convert the surface data
			SurfaceFormat convertedFormat = surfaceFormat;
			if (	surfaceFormat == SurfaceFormat.Dxt1 &&
				FNA3D.FNA3D_SupportsDXT1(device.GLDevice) == 0	)
			{
				convertedFormat = SurfaceFormat.Color;
			}
			else if (	(	surfaceFormat == SurfaceFormat.Dxt3 ||
						surfaceFormat == SurfaceFormat.Dxt5	) &&
					FNA3D.FNA3D_SupportsS3TC(device.GLDevice) == 0	)
			{
				convertedFormat = SurfaceFormat.Color;
			}

			// Check for duplicate instances
			if (existingInstance == null)
			{
				texture = new Texture2D(
					device,
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

				// Swap the image data if required.
				if (reader.platform == 'x')
				{
					if (	surfaceFormat == SurfaceFormat.Color ||
						surfaceFormat == SurfaceFormat.ColorBgraEXT	)
					{
						levelData = X360TexUtil.SwapColor(
							reader.ReadBytes(levelDataSizeInBytes)
						);
						levelDataSizeInBytes = levelData.Length;
					}
					else if (surfaceFormat == SurfaceFormat.Dxt1)
					{
						levelData = X360TexUtil.SwapDxt1(
							reader.ReadBytes(levelDataSizeInBytes),
							levelWidth,
							levelHeight
						);
						levelDataSizeInBytes = levelData.Length;
					}
					else if (surfaceFormat == SurfaceFormat.Dxt3)
					{
						levelData = X360TexUtil.SwapDxt3(
							reader.ReadBytes(levelDataSizeInBytes),
							levelWidth,
							levelHeight
						);
						levelDataSizeInBytes = levelData.Length;
					}
					else if (surfaceFormat == SurfaceFormat.Dxt5)
					{
						levelData = X360TexUtil.SwapDxt5(
							reader.ReadBytes(levelDataSizeInBytes),
							levelWidth,
							levelHeight
						);
						levelDataSizeInBytes = levelData.Length;
					}
				}

				// Convert the image data if required
				if (convertedFormat != surfaceFormat)
				{
					// May already be read in by 'x' conversion
					if (levelData == null)
					{
						levelData = reader.ReadBytes(levelDataSizeInBytes);
					}
					if (surfaceFormat == SurfaceFormat.Dxt1)
					{
						levelData = DxtUtil.DecompressDxt1(
							levelData,
							levelWidth,
							levelHeight
						);
					}
					else if (surfaceFormat == SurfaceFormat.Dxt3)
					{
						levelData = DxtUtil.DecompressDxt3(
							levelData,
							levelWidth,
							levelHeight
						);
					}
					else if (surfaceFormat == SurfaceFormat.Dxt5)
					{
						levelData = DxtUtil.DecompressDxt5(
							levelData,
							levelWidth,
							levelHeight
						);
					}
					levelDataSizeInBytes = levelData.Length;
				}

				int levelDataByteOffset = 0;
				if (levelData == null)
				{
					if (	reader.BaseStream is MemoryStream &&
						((MemoryStream) reader.BaseStream).TryGetBuffer(out levelData)	)
					{
						/* Ideally, we didn't have to perform any conversion or
						 * unnecessary reading. Just throw the buffer directly
						 * into SetData, skipping a redundant byte[] copy.
						 */
						levelDataByteOffset = (int) reader.BaseStream.Seek(0, SeekOrigin.Current);
						reader.BaseStream.Seek(
							levelDataSizeInBytes,
							SeekOrigin.Current
						);
					}
					else
					{
						/* If we don't have to perform any conversion and
						 * the ContentReader is not backed by a MemoryStream
						 * with a public buffer, we have to read the data in.
						 */
						levelData = reader.ReadBytes(levelDataSizeInBytes);
					}
				}
				texture.SetData(
					level,
					null,
					levelData,
					levelDataByteOffset,
					levelDataSizeInBytes
				);

			}

			return texture;
		}

		#endregion
	}
}
