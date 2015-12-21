#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
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
			Format = format;

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

			GraphicsDevice.GLDevice.SetTextureData2D(
				texture,
				Format,
				x,
				y,
				w,
				h,
				level,
				data,
				startIndex,
				elementCount
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

			GraphicsDevice.GLDevice.GetTextureData2D(
				texture,
				Format,
				Width,
				Height,
				level,
				rect,
				data,
				startIndex,
				elementCount
			);
		}

		#endregion

		#region Public Texture2D Save Methods

		public void SaveAsJpeg(Stream stream, int width, int height)
		{
			// dealwithit.png -flibit
			throw new NotSupportedException("It's 2015. Time to move on.");
		}

		public void SaveAsPng(Stream stream, int width, int height)
		{
			// Get the Texture2D pixels
			byte[] data = new byte[Width * Height * GetFormatSize(Format)];
			GetData(data);
			Game.Instance.Platform.SavePNG(
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
			Game.Instance.Platform.TextureDataFromStream(
				stream,
				out width,
				out height,
				out pixels,
				requestedWidth,
				requestedHeight,
				zoom
			);
		}

		#endregion
	}
}
