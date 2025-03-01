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
			if (this is IRenderTarget)
			{
				if (format == SurfaceFormat.ColorSrgbEXT)
				{
					if (FNA3D.FNA3D_SupportsSRGBRenderTargets(GraphicsDevice.GLDevice) == 0)
					{
						// Renderable but not on this device
						Format = SurfaceFormat.Color;
					}
					else
					{
						Format = format;
					}
				}
				else if (	format != SurfaceFormat.Color &&
						format != SurfaceFormat.Rgba1010102 &&
						format != SurfaceFormat.Rg32 &&
						format != SurfaceFormat.Rgba64 &&
						format != SurfaceFormat.Single &&
						format != SurfaceFormat.Vector2 &&
						format != SurfaceFormat.Vector4 &&
						format != SurfaceFormat.HalfSingle &&
						format != SurfaceFormat.HalfVector2 &&
						format != SurfaceFormat.HalfVector4 &&
						format != SurfaceFormat.HdrBlendable &&
						format != SurfaceFormat.ByteEXT &&
						format != SurfaceFormat.UShortEXT)
				{
					// Not a renderable format period
					Format = SurfaceFormat.Color;
				}
				else
				{
					Format = format;
				}
			}
			else
			{
				Format = format;
			}

			texture = FNA3D.FNA3D_CreateTexture2D(
				GraphicsDevice.GLDevice,
				Format,
				Width,
				Height,
				LevelCount,
				(byte) ((this is IRenderTarget) ? 1 : 0)
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
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex");
			}
			if (data.Length < (elementCount + startIndex))
			{
				throw new ArgumentOutOfRangeException("elementCount");
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
			int elementSize = MarshalHelper.SizeOf<T>();
			int requiredBytes = (w * h * GetFormatSizeEXT(Format)) / GetBlockSizeSquaredEXT(Format);
			int availableBytes = elementCount * elementSize;
			if (requiredBytes > availableBytes)
			{
				throw new ArgumentOutOfRangeException("rect", "The region you are trying to upload is larger than the amount of data you provided.");
			}

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetTextureData2D(
				GraphicsDevice.GLDevice,
				texture,
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

			FNA3D.FNA3D_SetTextureData2D(
				GraphicsDevice.GLDevice,
				texture,
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

			int elementSizeInBytes = MarshalHelper.SizeOf<T>();
			ValidateGetDataFormat(Format, elementSizeInBytes);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GetDataPointerEXT(
				level,
				rect,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				elementCount * elementSizeInBytes
			);
			handle.Free();
		}

		public void GetDataPointerEXT(
			int level,
			Rectangle? rect,
			IntPtr data,
			int dataLengthBytes
		) {
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
			FNA3D.FNA3D_GetTextureData2D(
				GraphicsDevice.GLDevice,
				texture,
				subX,
				subY,
				subW,
				subH,
				level,
				data,
				dataLengthBytes
			);
		}

		#endregion

		#region Public Texture2D Save Methods

		public void SaveAsJpeg(Stream stream, int width, int height)
		{
			int quality;
			string qualityString = Environment.GetEnvironmentVariable("FNA_GRAPHICS_JPEG_SAVE_QUALITY");
			if (string.IsNullOrEmpty(qualityString) || !int.TryParse(qualityString, out quality))
			{
				quality = 100; // FIXME: What does XNA pick for quality? -flibit
			}

			int len = Width * Height * GetFormatSizeEXT(Format);
			IntPtr data = FNAPlatform.Malloc(len);
			FNA3D.FNA3D_GetTextureData2D(
				GraphicsDevice.GLDevice,
				texture,
				0,
				0,
				Width,
				Height,
				0,
				data,
				len
			);

			FNA3D.WriteJPGStream(
				stream,
				Width,
				Height,
				width,
				height,
				data,
				quality
			);

			FNAPlatform.Free(data);
		}

		public void SaveAsPng(Stream stream, int width, int height)
		{
			int len = Width * Height * GetFormatSizeEXT(Format);
			IntPtr data = FNAPlatform.Malloc(len);
			FNA3D.FNA3D_GetTextureData2D(
				GraphicsDevice.GLDevice,
				texture,
				0,
				0,
				Width,
				Height,
				0,
				data,
				len
			);


			FNA3D.WritePNGStream(
				stream,
				Width,
				Height,
				width,
				height,
				data
			);

			FNAPlatform.Free(data);
		}

		#endregion

		#region Public Static Texture2D Load Methods

		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
			if (stream.CanSeek && stream.Position == stream.Length)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			int width, height, len;
			IntPtr pixels = FNA3D.ReadImageStream(
				stream,
				out width,
				out height,
				out len
			);
			if ((pixels == IntPtr.Zero) || (width <= 0) || (height <= 0))
				throw new Exception("Decoding image failed!");

			Texture2D result = new Texture2D(
				graphicsDevice,
				width,
				height
			);
			result.SetDataPointerEXT(
				0,
				null,
				pixels,
				len
			);

			FNA3D.FNA3D_Image_Free(pixels);
			return result;
		}

		public static Texture2D FromStream(
			GraphicsDevice graphicsDevice,
			Stream stream,
			int width,
			int height,
			bool zoom
		) {
			if (stream.CanSeek && stream.Position == stream.Length)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			int realWidth, realHeight, len;
			IntPtr pixels = FNA3D.ReadImageStream(
				stream,
				out realWidth,
				out realHeight,
				out len,
				width,
				height,
				zoom
			);
			if ((pixels == IntPtr.Zero) || (realWidth <= 0) || (realHeight <= 0))
				throw new Exception("Decoding image failed!");

			Texture2D result = new Texture2D(
				graphicsDevice,
				realWidth,
				realHeight
			);
			result.SetDataPointerEXT(
				0,
				null,
				pixels,
				len
			);

			FNA3D.FNA3D_Image_Free(pixels);
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
			if (stream.CanSeek && stream.Position == stream.Length)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			int len;
			IntPtr pixPtr = FNA3D.ReadImageStream(
				stream,
				out width,
				out height,
				out len,
				requestedWidth,
				requestedHeight,
				zoom
			);

			pixels = new byte[len];
			Marshal.Copy(pixPtr, pixels, 0, len);

			FNA3D.FNA3D_Image_Free(pixPtr);
		}

		public static Texture2D DDSFromStreamEXT(
			GraphicsDevice graphicsDevice,
			Stream stream
		) {
			Texture2D result;

			// Begin BinaryReader, ignoring a tab!
			using (BinaryReader reader = new BinaryReader(stream))
			{

			int width, height, levels;
			bool isCube;
			SurfaceFormat format;
			Texture.ParseDDS(
				reader,
				out format,
				out width,
				out height,
				out levels,
				out isCube
			);

			if (isCube)
			{
				throw new FormatException("This file contains cube map data!");
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
					int levelSize = Texture.CalculateDDSLevelSize(
						width >> i,
						height >> i,
						format
					);
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
				}
			}
			else
			{
				for (int i = 0; i < levels; i += 1)
				{
					tex = reader.ReadBytes(Texture.CalculateDDSLevelSize(
						width >> i,
						height >> i,
						format
					));
					result.SetData(
						i,
						null,
						tex,
						0,
						tex.Length
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
