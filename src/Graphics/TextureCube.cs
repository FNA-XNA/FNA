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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class TextureCube : Texture
	{
		#region Public Properties

		/// <summary>
		/// Gets the width and height of the cube map face in pixels.
		/// </summary>
		/// <value>The width and height of a cube map face in pixels.</value>
		public int Size
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		public TextureCube(
			GraphicsDevice graphicsDevice,
			int size,
			bool mipMap,
			SurfaceFormat format
		) {
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			GraphicsDevice = graphicsDevice;
			Size = size;
			LevelCount = mipMap ? CalculateMipLevels(size) : 1;

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

			texture = GraphicsDevice.GLDevice.CreateTextureCube(
				Format,
				Size,
				LevelCount
			);
		}

		#endregion

		#region Public SetData Methods

		public void SetData<T>(
			CubeMapFace cubeMapFace,
			T[] data
		) where T : struct {
			SetData(
				cubeMapFace,
				0,
				null,
				data,
				0,
				data.Length
			);
		}

		public void SetData<T>(
			CubeMapFace cubeMapFace,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			SetData(
				cubeMapFace,
				0,
				null,
				data,
				startIndex,
				elementCount
			);
		}

		public void SetData<T>(
			CubeMapFace cubeMapFace,
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

			int xOffset, yOffset, width, height;
			if (rect.HasValue)
			{
				xOffset = rect.Value.X;
				yOffset = rect.Value.Y;
				width = rect.Value.Width;
				height = rect.Value.Height;
			}
			else
			{
				xOffset = 0;
				yOffset = 0;
				width = Math.Max(1, Size >> level);
				height = Math.Max(1, Size >> level);
			}

			int elementSizeInBytes = Marshal.SizeOf(typeof(T));
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.SetTextureDataCube(
				texture,
				Format,
				xOffset,
				yOffset,
				width,
				height,
				cubeMapFace,
				level,
				handle.AddrOfPinnedObject() + startIndex * elementSizeInBytes,
				elementCount * elementSizeInBytes
			);
			handle.Free();
		}

		public void SetDataPointerEXT(
			CubeMapFace cubeMapFace,
			int level,
			Rectangle? rect,
			IntPtr data,
			int dataLength
		) {
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException("data");
			}

			int xOffset, yOffset, width, height;
			if (rect.HasValue)
			{
				xOffset = rect.Value.X;
				yOffset = rect.Value.Y;
				width = rect.Value.Width;
				height = rect.Value.Height;
			}
			else
			{
				xOffset = 0;
				yOffset = 0;
				width = Math.Max(1, Size >> level);
				height = Math.Max(1, Size >> level);
			}

			GraphicsDevice.GLDevice.SetTextureDataCube(
				texture,
				Format,
				xOffset,
				yOffset,
				width,
				height,
				cubeMapFace,
				level,
				data,
				dataLength
			);
		}
		#endregion

		#region Public GetData Method

		public void GetData<T>(
			CubeMapFace cubeMapFace,
			T[] data
		) where T : struct {
			GetData(
				cubeMapFace,
				0,
				null,
				data,
				0,
				data.Length
			);
		}

		public void GetData<T>(
			CubeMapFace cubeMapFace,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GetData(
				cubeMapFace,
				0,
				null,
				data,
				startIndex,
				elementCount
			);
		}

		public void GetData<T>(
			CubeMapFace cubeMapFace,
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
				subW = Size >> level;
				subH = Size >> level;
			}
			else
			{
				subX = rect.Value.X;
				subY = rect.Value.Y;
				subW = rect.Value.Width;
				subH = rect.Value.Height;
			}

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			GraphicsDevice.GLDevice.GetTextureDataCube(
				texture,
				Format,
				Size >> level,
				cubeMapFace,
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
	}
}
