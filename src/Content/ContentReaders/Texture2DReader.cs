#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2017 Ethan Lee and the MonoGame Team
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
			".bmp", ".gif", ".jpg", ".jpeg", ".png", ".tga", ".tif", ".tiff", ".dds"
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

				byte[] fullData = null;
				int fullDataStartIndex = 0;
				if (levelData == null)
				{
					if (reader.BaseStream.GetType() == typeof(System.IO.MemoryStream))
					{
						/* If the ContentReader is backed by a
						 * MemoryStream, we may need the complete
						 * stream buffer sooner or later.
						 */
						fullData = (((System.IO.MemoryStream) (reader.BaseStream)).GetBuffer());
						fullDataStartIndex = (int) reader.BaseStream.Position;
						reader.BaseStream.Seek(
							levelDataSizeInBytes,
							System.IO.SeekOrigin.Current
						);
					}
					else
					{
						/* If the ContentReader is not backed by a
						 * MemoryStream, we have to read the data in.
						 */
						levelData = reader.ReadBytes(levelDataSizeInBytes);
					}
				}

				if (reader.PlatformEXT == 'x')
				{
					byte[] levelDataSrc;
					int offset;
					int length;
					if (levelData != null)
					{
						levelDataSrc = levelData;
						offset = 0;
						length = levelData.Length;
					} else {
						// We're dealing with a MemoryStream containing raw data.
						levelDataSrc = fullData;
						offset = fullDataStartIndex;
						length = levelDataSizeInBytes;
					}

					// We may or may not need to fix the texture data.
					byte[] levelDataDst = null;

					unsafe
					{
						switch (surfaceFormat)
						{
							case SurfaceFormat.Color:
							case SurfaceFormat.ColorBgraEXT:
								levelDataDst = new byte[length];
								for (int i = 0; i < length; i += 4)
								{
									levelDataDst[i + 0] = levelDataSrc[offset + i + 3];
									levelDataDst[i + 1] = levelDataSrc[offset + i + 2];
									levelDataDst[i + 2] = levelDataSrc[offset + i + 1];
									levelDataDst[i + 3] = levelDataSrc[offset + i + 0];
								}
								break;

							case SurfaceFormat.Dxt1:
								// ???
								break;

							case SurfaceFormat.Dxt3:
								levelDataDst = new byte[length];
								for (int i = 0; i < length; i += 2)
								{
									levelDataDst[i + 0] = levelDataSrc[offset + i + 1];
									levelDataDst[i + 1] = levelDataSrc[offset + i + 0];
								}
								break;

							case SurfaceFormat.Dxt5:
								// ???
								break;

							default:
								// ???
								break;
						}

					}

					if (levelDataDst != null)
					{
						// We actually had to fix the data.
						levelData = levelDataDst;
					}
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
						fullData,
						fullDataStartIndex,
						levelDataSizeInBytes
					);
				}

			}

			return texture;
		}

		#endregion
	}
}
