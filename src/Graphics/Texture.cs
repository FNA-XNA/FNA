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
using System.Threading;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public abstract class Texture : GraphicsResource
	{
		#region Public Properties

		public SurfaceFormat Format
		{
			get;
			protected set;
		}

		public int LevelCount
		{
			get;
			protected set;
		}

		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				// Avoid calling SetTextureName when the value hasn't changed.
				if (value == base.Name)
				{
					return;
				}

				base.Name = value;

				// Never pass a null string pointer through to SetTextureName.
				// Since base.Name will be null by default, this will only happen if
				//  you first set a name for the texture, then try to null it out.
				if (value == null)
				{
					value = string.Empty;
				}
				FNA3D.FNA3D_SetTextureName(GraphicsDevice.GLDevice, texture, value);
			}
		}

		#endregion

		#region Internal FNA3D Variables

		internal IntPtr texture;

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				GraphicsDevice.Textures.RemoveDisposedTexture(this);
				GraphicsDevice.VertexTextures.RemoveDisposedTexture(this);

				IntPtr toDispose = Interlocked.Exchange(ref texture, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeTexture(
						GraphicsDevice.GLDevice,
						toDispose
					);
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Internal Context Reset Method

		internal protected override void GraphicsDeviceResetting()
		{
			// FIXME: Do we even want to bother with DeviceResetting for GL? -flibit
		}

		#endregion

		#region Static SurfaceFormat Size Methods

		public static int GetBlockSizeSquaredEXT(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.Dxt5SrgbEXT:
				case SurfaceFormat.Bc7EXT:
				case SurfaceFormat.Bc7SrgbEXT:
					return 16;
				case SurfaceFormat.Alpha8:
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
				case SurfaceFormat.ColorSrgbEXT:
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.HdrBlendable:
				case SurfaceFormat.Vector4:
				case SurfaceFormat.ByteEXT:
				case SurfaceFormat.UShortEXT:
					return 1;
				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
			}
		}

		public static int GetFormatSizeEXT(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return 8;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.Dxt5SrgbEXT:
				case SurfaceFormat.Bc7EXT:
				case SurfaceFormat.Bc7SrgbEXT:
					return 16;
				case SurfaceFormat.Alpha8:
				case SurfaceFormat.ByteEXT:
					return 1;
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.UShortEXT:
					return 2;
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
				case SurfaceFormat.ColorSrgbEXT:
					return 4;
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.HdrBlendable:
					return 8;
				case SurfaceFormat.Vector4:
					return 16;
				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
			}
		}

		internal static int GetPixelStoreAlignment(SurfaceFormat format)
		{
			/*
			 * https://github.com/FNA-XNA/FNA/pull/238
			 * https://www.khronos.org/registry/OpenGL/specs/gl/glspec21.pdf
			 * OpenGL 2.1 Specification, section 3.6.1, table 3.1 specifies that the pixelstorei alignment cannot exceed 8
			 */
			return Math.Min(8, GetFormatSizeEXT(format));
		}

		internal static void ValidateGetDataFormat(
			SurfaceFormat format,
			int elementSizeInBytes
		) {
			if (GetFormatSizeEXT(format) % elementSizeInBytes != 0)
			{
				throw new ArgumentException(
					"The type you are using for T in this" +
					" method is an invalid size for this" +
					" resource"
				);
			}
		}

		#endregion

		#region Static Mipmap Level Calculator

		internal static int CalculateMipLevels(
			int width,
			int height = 0,
			int depth = 0
		) {
			int levels = 1;
			for (
				int size = Math.Max(Math.Max(width, height), depth);
				size > 1;
				levels += 1
			) {
				size /= 2;
			}
			return levels;
		}

		#endregion

		#region Static DDS Parser

		internal static int CalculateDDSLevelSize(
			int width,
			int height,
			SurfaceFormat format
		) {
			if (format == SurfaceFormat.Color || format == SurfaceFormat.ColorBgraEXT)
			{
				return (((width * 32) + 7) / 8) * height;
			}
			else if (format == SurfaceFormat.HalfVector4)
			{
				return (((width * 64) + 7) / 8) * height;
			}
			else if (format == SurfaceFormat.Vector4)
			{
				return (((width * 128) + 7) / 8) * height;
			}
			else
			{
				int blockSize = 16;
				if (format == SurfaceFormat.Dxt1)
				{
					blockSize = 8;
				}
				width = Math.Max(width, 1);
				height = Math.Max(height, 1);
				return (
					((width + 3) / 4) *
					((height + 3) / 4) *
					blockSize
				);
			}
		}

		// DDS loading extension, based on MojoDDS
		internal static void ParseDDS(
			BinaryReader reader,
			out SurfaceFormat format,
			out int width,
			out int height,
			out int levels,
			out bool isCube
		) {
			// A whole bunch of magic numbers, yay DDS!
			const uint DDS_MAGIC = 0x20534444;
			const uint DDS_HEADERSIZE = 124;
			const uint DDS_PIXFMTSIZE = 32;
			const uint DDSD_HEIGHT = 0x2;
			const uint DDSD_WIDTH = 0x4;
			const uint DDSD_PITCH = 0x8;
			const uint DDSD_LINEARSIZE = 0x80000;
			const uint DDSD_REQ = (
				/* Per the spec, this should also be or'd with DDSD_CAPS | DDSD_FMT,
				 * but some compression tools don't obey the spec, so here we are...
				 */
				DDSD_HEIGHT | DDSD_WIDTH
			);
			const uint DDSCAPS_MIPMAP = 0x400000;
			const uint DDSCAPS_TEXTURE = 0x1000;
			const uint DDSCAPS2_CUBEMAP = 0x200;
			const uint DDPF_FOURCC = 0x4;
			const uint DDPF_RGB = 0x40;
			const uint FOURCC_DXT1 = 0x31545844;
			const uint FOURCC_DXT3 = 0x33545844;
			const uint FOURCC_DXT5 = 0x35545844;
			const uint FOURCC_DX10 = 0x30315844;
			const uint pitchAndLinear = (
				DDSD_PITCH | DDSD_LINEARSIZE
			);

			// File should start with 'DDS '
			if (reader.ReadUInt32() != DDS_MAGIC)
			{
				throw new NotSupportedException("Not a DDS!");
			}

			// Texture info
			uint size = reader.ReadUInt32();
			if (size != DDS_HEADERSIZE)
			{
				throw new NotSupportedException("Invalid DDS header!");
			}
			uint flags = reader.ReadUInt32();
			if ((flags & DDSD_REQ) != DDSD_REQ)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}
			if ((flags & pitchAndLinear) == pitchAndLinear)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}
			height = reader.ReadInt32();
			width = reader.ReadInt32();
			reader.ReadUInt32(); // dwPitchOrLinearSize, unused
			reader.ReadUInt32(); // dwDepth, unused
			levels = reader.ReadInt32();

			// "Reserved"
			reader.ReadBytes(4 * 11);

			// Format info
			uint formatSize = reader.ReadUInt32();
			if (formatSize != DDS_PIXFMTSIZE)
			{
				throw new NotSupportedException("Bogus PIXFMTSIZE!");
			}
			uint formatFlags = reader.ReadUInt32();
			uint formatFourCC = reader.ReadUInt32();
			uint formatRGBBitCount = reader.ReadUInt32();
			uint formatRBitMask = reader.ReadUInt32();
			uint formatGBitMask = reader.ReadUInt32();
			uint formatBBitMask = reader.ReadUInt32();
			uint formatABitMask = reader.ReadUInt32();

			// dwCaps "stuff"
			uint caps = reader.ReadUInt32();
			if ((caps & DDSCAPS_TEXTURE) == 0)
			{
				throw new NotSupportedException("Not a texture!");
			}

			isCube = false;

			uint caps2 = reader.ReadUInt32();
			if (caps2 != 0)
			{
				if ((caps2 & DDSCAPS2_CUBEMAP) == DDSCAPS2_CUBEMAP)
				{
					isCube = true;
				}
				else
				{
					throw new NotSupportedException("Invalid caps2!");
				}
			}

			reader.ReadUInt32(); // dwCaps3, unused
			reader.ReadUInt32(); // dwCaps4, unused

			// "Reserved"
			reader.ReadUInt32();

			// Mipmap sanity check
			if ((caps & DDSCAPS_MIPMAP) != DDSCAPS_MIPMAP)
			{
				levels = 1;
			}

			// Determine texture format
			if ((formatFlags & DDPF_FOURCC) == DDPF_FOURCC)
			{
				switch (formatFourCC)
				{
					case 0x71: // D3DFMT_A16B16G16R16F
						format = SurfaceFormat.HalfVector4;
						break;
					case 0x74: // D3DFMT_A32B32G32R32F
						format = SurfaceFormat.Vector4;
						break;
					case FOURCC_DXT1:
						format = SurfaceFormat.Dxt1;
						break;
					case FOURCC_DXT3:
						format = SurfaceFormat.Dxt3;
						break;
					case FOURCC_DXT5:
						format = SurfaceFormat.Dxt5;
						break;
					case FOURCC_DX10:
						// If the fourCC is DX10, there is an extra header with additional format information.
						uint dxgiFormat = reader.ReadUInt32();

						// These values are taken from the DXGI_FORMAT enum.
						switch (dxgiFormat)
						{
							case 2:
								format = SurfaceFormat.Vector4;
								break;

							case 10:
								format = SurfaceFormat.HalfVector4;
								break;

							case 71:
								format = SurfaceFormat.Dxt1;
								break;

							case 74:
								format = SurfaceFormat.Dxt3;
								break;

							case 77:
								format = SurfaceFormat.Dxt5;
								break;

							case 98:
								format = SurfaceFormat.Bc7EXT;
								break;

							case 99:
								format = SurfaceFormat.Bc7SrgbEXT;
								break;

							default:
								throw new NotSupportedException(
									"Unsupported DDS texture format"
								);
						}

						uint resourceDimension = reader.ReadUInt32();

						// These values are taken from the D3D10_RESOURCE_DIMENSION enum.
						switch (resourceDimension)
						{
							case 0: // Unknown
							case 1: // Buffer
								throw new NotSupportedException(
									"Unsupported DDS texture format"
								);
							default:
								break;
						}

						/*
						 * This flag seemingly only indicates if the texture is a cube map.
						 * This is already determined above. Cool!
						 */
						reader.ReadUInt32();

						/*
						 * Indicates the number of elements in the texture array.
						 * We don't support texture arrays so just throw if it's greater than 1.
						 */
						uint arraySize = reader.ReadUInt32();

						if (arraySize > 1)
						{
							throw new NotSupportedException(
								"Unsupported DDS texture format"
							);
						}

						reader.ReadUInt32(); // reserved

						break;
					default:
						throw new NotSupportedException(
							"Unsupported DDS texture format"
						);
				}
			}
			else if ((formatFlags & DDPF_RGB) == DDPF_RGB)
			{
				if (formatRGBBitCount != 32)
					throw new NotSupportedException("Unsupported DDS texture format: Alpha channel required");

				bool isBgra = (formatRBitMask == 0x00FF0000 &&
					formatGBitMask == 0x0000FF00 &&
					formatBBitMask == 0x000000FF &&
					formatABitMask == 0xFF000000);
				bool isRgba = (formatRBitMask == 0x000000FF &&
					formatGBitMask == 0x0000FF00 &&
					formatBBitMask == 0x00FF0000 &&
					formatABitMask == 0xFF000000);

				if (isBgra)
					format = SurfaceFormat.ColorBgraEXT;
				else if (isRgba)
					format = SurfaceFormat.Color;
				else
					throw new NotSupportedException("Unsupported DDS texture format: Only RGBA and BGRA are supported");
			}
			else
			{
				throw new NotSupportedException(
					"Unsupported DDS texture format"
				);
			}
		}

		#endregion
	}
}
