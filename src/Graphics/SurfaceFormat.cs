#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines types of surface formats.
	/// </summary>
	public enum SurfaceFormat
	{
		/// <summary>
		/// Unsigned 32-bit ARGB pixel format for store 8 bits per channel.
		/// </summary>
		Color,
		/// <summary>
		/// Unsigned 16-bit BGR pixel format for store 5 bits for blue, 6 bits for green, and 5 bits for red.
		/// </summary>
		Bgr565,
		/// <summary>
		/// Unsigned 16-bit BGRA pixel format where 5 bits reserved for each color and last bit is reserved for alpha.
		/// </summary>
		Bgra5551,
		/// <summary>
		/// Unsigned 16-bit BGRA pixel format for store 4 bits per channel.
		/// </summary>
		Bgra4444,
		/// <summary>
		/// DXT1. Texture format with compression. Surface dimensions must be a multiple 4.
		/// </summary>
		Dxt1,
		/// <summary>
		/// DXT3. Texture format with compression. Surface dimensions must be a multiple 4.
		/// </summary>
		Dxt3,
		/// <summary>
		/// DXT5. Texture format with compression. Surface dimensions must be a multiple 4.
		/// </summary>
		Dxt5,
		/// <summary>
		/// Signed 16-bit bump-map format for store 8 bits for <c>u</c> and <c>v</c> data.
		/// </summary>
		NormalizedByte2,
		/// <summary>
		/// Signed 16-bit bump-map format for store 8 bits per channel.
		/// </summary>
		NormalizedByte4,
		/// <summary>
		/// Unsigned 32-bit RGBA pixel format for store 10 bits for each color and 2 bits for alpha.
		/// </summary>
		Rgba1010102,
		/// <summary>
		/// Unsigned 32-bit RG pixel format using 16 bits per channel.
		/// </summary>
		Rg32,
		/// <summary>
		/// Unsigned 64-bit RGBA pixel format using 16 bits per channel.
		/// </summary>
		Rgba64,
		/// <summary>
		/// Unsigned A 8-bit format for store 8 bits to alpha channel.
		/// </summary>
		Alpha8,
		/// <summary>
		/// IEEE 32-bit R float format for store 32 bits to red channel.
		/// </summary>
		Single,
		/// <summary>
		/// IEEE 64-bit RG float format for store 32 bits per channel.
		/// </summary>
		Vector2,
		/// <summary>
		/// IEEE 128-bit RGBA float format for store 32 bits per channel.
		/// </summary>
		Vector4,
		/// <summary>
		/// Float 16-bit R format for store 16 bits to red channel.
		/// </summary>
		HalfSingle,
		/// <summary>
		/// Float 32-bit RG format for store 16 bits per channel.
		/// </summary>
		HalfVector2,
		/// <summary>
		/// Float 64-bit ARGB format for store 16 bits per channel.
		/// </summary>
		HalfVector4,
		/// <summary>
		/// Float pixel format for high dynamic range data.
		/// </summary>
		HdrBlendable,
		/// <summary>
		/// Unsigned 32-bit ABGR pixel format for store 8 bits per channel (XNA3)
		/// </summary>
		ColorBgraEXT,
		/// <summary>
		/// Unsigned 32-bit ARGB pixel format for store 8 bits per channel.
		/// Byte encoding is in sRGB colorspace, read in shader in linear colorspace.
		/// </summary>
		ColorSrgbEXT,
		/// <summary>
		/// DXT5. Texture format with compression. Surface dimensions must be a multiple 4.
		/// Byte encoding is in sRGB colorspace, read in shader in linear colorspace.
		/// </summary>
		Dxt5SrgbEXT,
		/// <summary>
		/// BC7 block texture format
		/// </summary>
		Bc7EXT,
		/// <summary>
		/// BC7 block texture format where the R/G/B values are non-linear sRGB.
		/// </summary>
		Bc7SrgbEXT,
		/// <summary>
		/// Unsigned 8-bit R pixel format.
		/// </summary>
		ByteEXT,
		/// <summary>
		/// Unsigned 16-bit R pixel format.
		/// </summary>
		UShortEXT,
	}

	internal abstract class SurfaceFormatInfo
	{
		static readonly SurfaceFormatInfoAlign align1 = new SurfaceFormatInfoAlign(1);
		static readonly SurfaceFormatInfoAlign align2 = new SurfaceFormatInfoAlign(2);
		static readonly SurfaceFormatInfoAlign align4 = new SurfaceFormatInfoAlign(4);
		static readonly SurfaceFormatInfoAlign align8 = new SurfaceFormatInfoAlign(8);
		static readonly SurfaceFormatInfoAlign align16 = new SurfaceFormatInfoAlign(16);
		static readonly SurfaceFormatInfoBC8 bc8 = new SurfaceFormatInfoBC8();
		static readonly SurfaceFormatInfoBC16 bc16 = new SurfaceFormatInfoBC16();

		sealed class SurfaceFormatInfoAlign : SurfaceFormatInfo
		{
			internal SurfaceFormatInfoAlign(int align) : base(align) { }
			internal override int GetSize(int width, int height)
			{
				return width * height * Align;
			}
		}

		sealed class SurfaceFormatInfoBC8 : SurfaceFormatInfo
		{
			internal SurfaceFormatInfoBC8() : base(1) { }
			internal override int GetSize(int width, int height)
			{
				return (width + 3 >> 2) * (height + 3 >> 2) * 8;
			}
		}

		sealed class SurfaceFormatInfoBC16 : SurfaceFormatInfo
		{
			internal SurfaceFormatInfoBC16() : base(1) { }
			internal override int GetSize(int width, int height)
			{
				return (width + 3 >> 2) * (height + 3 >> 2) * 16;
			}
		}

		internal readonly int Align;

		SurfaceFormatInfo(int align)
		{
			Align = align;
		}

		internal abstract int GetSize(int width, int height);

		internal static SurfaceFormatInfo GetInstance(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return bc8;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.Dxt5SrgbEXT:
				case SurfaceFormat.Bc7EXT:
				case SurfaceFormat.Bc7SrgbEXT:
					return bc16;
				case SurfaceFormat.Alpha8:
				case SurfaceFormat.ByteEXT:
					return align1;
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.UShortEXT:
					return align2;
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
				case SurfaceFormat.ColorSrgbEXT:
					return align4;
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.HdrBlendable:
					return align8;
				case SurfaceFormat.Vector4:
					return align16;
				default:
					return null;
			}
		}
	}
}
