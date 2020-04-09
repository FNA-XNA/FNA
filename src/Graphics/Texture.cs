#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
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
				FNA3D.FNA3D_AddDisposeTexture(
					GraphicsDevice.GLDevice,
					texture
				);
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

		internal static int GetFormatSize(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return 8;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
					return 16;
				case SurfaceFormat.Alpha8:
					return 1;
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
					return 2;
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
					return 4;
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
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
			return Math.Min(8, GetFormatSize(format));
		}

		internal static void ValidateGetDataFormat(
			SurfaceFormat format,
			int elementSizeInBytes
		) {
			if (GetFormatSize(format) % elementSizeInBytes != 0)
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
	}
}
