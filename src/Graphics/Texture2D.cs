#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class Texture2D : Texture
	{
		#region Public Properties

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(0, 0, Width, Height);
			}
		}

		#endregion

		#region Public Constructors

		public Texture2D(
			GraphicsDevice graphicsDevice,
			int width,
			int height
		) : this(
			graphicsDevice,
			width,
			height,
			false,
			SurfaceFormat.Color
		) {
		}

		public Texture2D(
			GraphicsDevice graphicsDevice,
			int width,
			int height,
			bool mipMap,
			SurfaceFormat format
		) {
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			GraphicsDevice = graphicsDevice;
			Width = width;
			Height = height;
			LevelCount = mipMap ? CalculateMipLevels(width, height) : 1;

			// TODO: Use QueryRenderTargetFormat!
			if (	this is IRenderTarget &&
				format != SurfaceFormat.Color &&
				format != SurfaceFormat.Rgba1010102 &&
				format != SurfaceFormat.Rg32 &&
				format != SurfaceFormat.Rgba64 &&
				format != SurfaceFormat.Single &&
				format != SurfaceFormat.Vector2 &&
				format != SurfaceFormat.Vector4 &&
				format != SurfaceFormat.HalfSingle &&
				format != SurfaceFormat.HalfVector2 &&
				format != SurfaceFormat.HalfVector4 &&
				format != SurfaceFormat.HdrBlendable	)
			{
				Format = SurfaceFormat.Color;
			}
			else
			{
				Format = format;
			}

			texture = GraphicsDevice.GLDevice.CreateTexture2D(
				Format,
				Width,
				Height,
				LevelCount
			);
		}

		#endregion

		#region Public SetData Methods

		public void SetData<T>(T[] data) where T : struct
		{
			SetData(
				0,
				null,
				data,
				0,
				data.Length
			);
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			SetData(
				0,
				null,
				data,
				startIndex,
				elementCount
			);
		}

		public void SetData<T>(
			int level,
			Rectangle? rect,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			int x, y, w, h;
			if (rect.HasValue)
			{
				x = rect.Value.X;
				y = rect.Value.Y;
				w = rect.Value.Width;
				h = rect.Value.Height;
			}
			else
			{
				x = 0;
				y = 0;
				w = Math.Max(Width >> level, 1);
				h = Math.Max(Height >> level, 1);
			}
			int elementSize = Marshal.SizeOf(typeof(T));
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.SetTextureData2D(
				texture,
				Format,
				x,
				y,
				w,
				h,
				level,
				handle.AddrOfPinnedObject() + startIndex * elementSize,
				elementCount * elementSize
			);
			handle.Free();
		}

		public void SetDataPointerEXT(
			int level,
			Rectangle? rect,
			IntPtr data,
			int dataLength
		) {
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException("data");
			}

			int x, y, w, h;
			if (rect.HasValue)
			{
				x = rect.Value.X;
				y = rect.Value.Y;
				w = rect.Value.Width;
				h = rect.Value.Height;
			}
			else
			{
				x = 0;
				y = 0;
				w = Math.Max(Width >> level, 1);
				h = Math.Max(Height >> level, 1);
			}

			GraphicsDevice.GLDevice.SetTextureData2D(
				texture,
				Format,
				x,
				y,
				w,
				h,
				level,
				data,
				dataLength
			);
		}

		#endregion

		#region Public GetData Methods

		public void GetData<T>(T[] data) where T : struct
		{
			GetData(
				0,
				null,
				data,
				0,
				data.Length
			);
		}

		public void GetData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GetData(
				0,
				null,
				data,
				startIndex,
				elementCount
			);
		}

		public void GetData<T>(
			int level,
			Rectangle? rect,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			if (data == null || data.Length == 0)
			{
				throw new ArgumentException("data cannot be null");
			}
			if (data.Length < startIndex + elementCount)
			{
				throw new ArgumentException(
					"The data passed has a length of " + data.Length.ToString() +
					" but " + elementCount.ToString() + " pixels have been requested."
				);
			}

			int subX, subY, subW, subH;
			if (rect == null)
			{
				subX = 0;
				subY = 0;
				subW = Width >> level;
				subH = Height >> level;
			}
			else
			{
				subX = rect.Value.X;
				subY = rect.Value.Y;
				subW = rect.Value.Width;
				subH = rect.Value.Height;
			}

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.GetTextureData2D(
				texture,
				Format,
				Width >> level,
				Height >> level,
				level,
				subX,
				subY,
				subW,
				subH,
				handle.AddrOfPinnedObject(),
				startIndex,
				elementCount,
				Marshal.SizeOf(typeof(T))
			);
			handle.Free();
		}

		#endregion

		#region Public Texture2D Save Methods

		public void SaveAsJpeg(Stream stream, int width, int height)
		{
			// Get the Texture2D pixels
			byte[] data = new byte[Width * Height * GetFormatSize(Format)];
			GetData(data);
			FNAPlatform.SaveJPG(
				stream,
				width,
				height,
				Width,
				Height,
				data
			);
		}

		public void SaveAsPng(Stream stream, int width, int height)
		{
			// Get the Texture2D pixels
			byte[] data = new byte[Width * Height * GetFormatSize(Format)];
			GetData(data);
			FNAPlatform.SavePNG(
				stream,
				width,
				height,
				Width,
				Height,
				data
			);
		}

		#endregion

		#region Public Static Texture2D Load Methods

		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
			// Read the image data from the stream
			int width, height;
			byte[] pixels;
			TextureDataFromStreamEXT(stream, out width, out height, out pixels);

			// Create the Texture2D from the raw pixel data
			Texture2D result = new Texture2D(
				graphicsDevice,
				width,
				height
			);
			result.SetData(pixels);
			return result;
		}

		public static Texture2D FromStream(
			GraphicsDevice graphicsDevice,
			Stream stream,
			int width,
			int height,
			bool zoom
		) {
			// Read the image data from the stream
			int realWidth, realHeight;
			byte[] pixels;
			TextureDataFromStreamEXT(
				stream,
				out realWidth,
				out realHeight,
				out pixels,
				width,
				height,
				zoom
			);

			// Create the Texture2D from the raw pixel data
			Texture2D result = new Texture2D(
				graphicsDevice,
				realWidth,
				realHeight
			);
			result.SetData(pixels);
			return result;
		}

		#endregion

		#region Public Static Texture2D Extensions
		
		/// <summary>
		/// Loads image data from a given stream.
		/// </summary>
		/// <remarks>
		/// This is an extension of XNA 4 and is not compatible with XNA. It exists to help with dynamically reloading
		/// textures while games are running. Games can use this method to read a stream into memory and then call
		/// SetData on a texture with that data, rather than having to dispose the texture and recreate it entirely.
		/// </remarks>
		/// <param name="stream">The stream from which to read the image data.</param>
		/// <param name="width">Outputs the width of the image.</param>
		/// <param name="height">Outputs the height of the image.</param>
		/// <param name="pixels">Outputs the pixel data of the image, in non-premultiplied RGBA format.</param>
		/// <param name="requestedWidth">Preferred width of the resulting image data</param>
		/// <param name="requestedHeight">Preferred height of the resulting image data</param>
		/// <param name="zoom">false to maintain aspect ratio, true to crop image</param>
		public static void TextureDataFromStreamEXT(
			Stream stream,
			out int width,
			out int height,
			out byte[] pixels,
			int requestedWidth = -1,
			int requestedHeight = -1,
			bool zoom = false
		) {
			FNAPlatform.TextureDataFromStream(
				stream,
				out width,
				out height,
				out pixels,
				requestedWidth,
				requestedHeight,
				zoom
			);
		}

		// DDS loading extension, based on MojoDDS
		public static Texture2D DDSFromStreamEXT(
			GraphicsDevice graphicsDevice,
			Stream stream
		) {
			// A whole bunch of magic numbers, yay DDS!
			const uint DDS_MAGIC = 0x20534444;
			const uint DDS_HEADERSIZE = 124;
			const uint DDS_PIXFMTSIZE = 32;
			const uint DDSD_CAPS = 0x1;
			const uint DDSD_HEIGHT = 0x2;
			const uint DDSD_WIDTH = 0x4;
			const uint DDSD_PITCH = 0x8;
			const uint DDSD_FMT = 0x1000;
			const uint DDSD_LINEARSIZE = 0x80000;
			const uint DDSD_REQ = (
				DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_FMT
			);
			const uint DDSCAPS_MIPMAP = 0x400000;
			const uint DDSCAPS_TEXTURE = 0x1000;
			const uint DDPF_FOURCC = 0x4;
			const uint DDPF_RGB = 0x40;
			const uint FOURCC_DXT1 = 0x31545844;
			const uint FOURCC_DXT3 = 0x33545844;
			const uint FOURCC_DXT5 = 0x35545844;
			// const uint FOURCC_DX10 = 0x30315844;
			const uint pitchAndLinear = (
				DDSD_PITCH | DDSD_LINEARSIZE
			);

			Texture2D result;

			// Begin BinaryReader, ignoring a tab!
			using (BinaryReader reader = new BinaryReader(stream))
			{

			// File should start with 'DDS '
			if (reader.ReadUInt32() != DDS_MAGIC)
			{
				return null;
			}

			// Texture info
			uint size = reader.ReadUInt32();
			if (size != DDS_HEADERSIZE)
			{
				return null;
			}
			uint flags = reader.ReadUInt32();
			if ((flags & DDSD_REQ) != DDSD_REQ)
			{
				return null;
			}
			if ((flags & pitchAndLinear) == pitchAndLinear)
			{
				return null;
			}
			int height = reader.ReadInt32();
			int width = reader.ReadInt32();
			reader.ReadUInt32(); // dwPitchOrLinearSize, unused
			reader.ReadUInt32(); // dwDepth, unused
			int levels = reader.ReadInt32();

			// "Reserved"
			reader.ReadBytes(4 * 11);

			// Format info
			uint formatSize = reader.ReadUInt32();
			if (formatSize != DDS_PIXFMTSIZE)
			{
				return null;
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
				return null;
			}
			uint caps2 = reader.ReadUInt32();
			if (caps2 != 0)
			{
				return null;
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
			SurfaceFormat format;
			int levelSize;
			int blockSize = 0;
			if ((formatFlags & DDPF_FOURCC) == DDPF_FOURCC)
			{
				if (formatFourCC == FOURCC_DXT1)
				{
					format = SurfaceFormat.Dxt1;
					blockSize = 8;
				}
				else if (formatFourCC == FOURCC_DXT3)
				{
					format = SurfaceFormat.Dxt3;
					blockSize = 16;
				}
				else if (formatFourCC == FOURCC_DXT5)
				{
					format = SurfaceFormat.Dxt5;
					blockSize = 16;
				}
				else
				{
					throw new NotSupportedException(
						"Unsupported DDS texture format"
					);
				}
				levelSize = (
					((width > 0 ? ((width + 3) / 4) : 1) * blockSize) *
					(height > 0 ? ((height + 3) / 4) : 1)
				);
			}
			else if ((formatFlags & DDPF_RGB) == DDPF_RGB)
			{
				if (	formatRGBBitCount != 32 ||
					formatRBitMask != 0x00FF0000 ||
					formatGBitMask != 0x0000FF00 ||
					formatBBitMask != 0x000000FF ||
					formatABitMask != 0xFF000000	)
				{
					throw new NotSupportedException(
						"Unsupported DDS texture format"
					);
				}

				format = SurfaceFormat.ColorBgraEXT;
				levelSize = (int) (
					(((width * formatRGBBitCount) + 7) / 8) *
					height
				);
			}
			else
			{
				throw new NotSupportedException(
					"Unsupported DDS texture format"
				);
			}

			// Allocate/Load texture
			result = new Texture2D(
				graphicsDevice,
				width,
				height,
				levels > 1,
				format
			);
			
			byte[] tex = null;
			if (	stream is MemoryStream &&
				((MemoryStream) stream).TryGetBuffer(out tex)	)
			{
				for (int i = 0; i < levels; i += 1)
				{
					result.SetData(
						i,
						null,
						tex,
						(int) stream.Seek(0, SeekOrigin.Current),
						levelSize
					);
					stream.Seek(
						levelSize,
						SeekOrigin.Current
					);
					levelSize = Math.Max(
						levelSize >> 2,
						blockSize
					);
				}
			}
			else
			{
				for (int i = 0; i < levels; i += 1)
				{
					tex = reader.ReadBytes(levelSize);
					result.SetData(
						i,
						null,
						tex,
						0,
						tex.Length
					);
					levelSize = Math.Max(
						levelSize >> 2,
						blockSize
					);
				}
			}

			// End BinaryReader
			}

			// Finally.
			return result;
		}

		#endregion
	}
}
