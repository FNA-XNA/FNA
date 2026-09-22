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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics.PackedVector;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Describes a 32-bit packed color.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(ColorConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Color : IEquatable<Color>, IPackedVector, IPackedVector<uint>
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the blue component.
		/// </summary>
		public byte B
		{
			get
			{
				unchecked
				{
					return (byte) (this.packedValue >> 16);
				}
			}
			set
			{
				this.packedValue = (this.packedValue & 0xff00ffff) | ((uint) value << 16);
			}
		}

		/// <summary>
		/// Gets or sets the green component.
		/// </summary>
		public byte G
		{
			get
			{
				unchecked
				{
					return (byte) (this.packedValue >> 8);
				}
			}
			set
			{
				this.packedValue = (this.packedValue & 0xffff00ff) | ((uint) value << 8);
			}
		}

		/// <summary>
		/// Gets or sets the red component.
		/// </summary>
		public byte R
		{
			get
			{
				unchecked
				{
					return (byte) (this.packedValue);
				}
			}
			set
			{
				this.packedValue = (this.packedValue & 0xffffff00) | value;
			}
		}

		/// <summary>
		/// Gets or sets the alpha component.
		/// </summary>
		public byte A
		{
			get
			{
				unchecked
				{
					return (byte) (this.packedValue >> 24);
				}
			}
			set
			{
				this.packedValue = (this.packedValue & 0x00ffffff) | ((uint) value << 24);
			}
		}

		/// <summary>
		/// Gets or sets packed value of this <see cref="Color"/>.
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 PackedValue
		{
			get
			{
				return packedValue;
			}
			set
			{
				packedValue = value;
			}
		}

		#endregion

		#region Public Static Color Properties

		/// <summary>
		/// Transparent color (R:0,G:0,B:0,A:0).
		/// </summary>
		public static Color Transparent
		{
			get { return new Color(0x0); }
		}

		/// <summary>
		/// AliceBlue color (R:240,G:248,B:255,A:255).
		/// </summary>
		public static Color AliceBlue
		{
			get { return new Color(0xFFFFF8F0); }
		}

		/// <summary>
		/// AntiqueWhite color (R:250,G:235,B:215,A:255).
		/// </summary>
		public static Color AntiqueWhite
		{
			get { return new Color(0xFFD7EBFA); }
		}

		/// <summary>
		/// Aqua color (R:0,G:255,B:255,A:255).
		/// </summary>
		public static Color Aqua
		{
			get { return new Color(0xFFFFFF00); }
		}

		/// <summary>
		/// Aquamarine color (R:127,G:255,B:212,A:255).
		/// </summary>
		public static Color Aquamarine
		{
			get { return new Color(0xFFD4FF7F); }
		}

		/// <summary>
		/// Azure color (R:240,G:255,B:255,A:255).
		/// </summary>
		public static Color Azure
		{
			get { return new Color(0xFFFFFFF0); }
		}

		/// <summary>
		/// Beige color (R:245,G:245,B:220,A:255).
		/// </summary>
		public static Color Beige
		{
			get { return new Color(0xFFDCF5F5); }
		}

		/// <summary>
		/// Bisque color (R:255,G:228,B:196,A:255).
		/// </summary>
		public static Color Bisque
		{
			get { return new Color(0xFFC4E4FF); }
		}

		/// <summary>
		/// Black color (R:0,G:0,B:0,A:255).
		/// </summary>
		public static Color Black
		{
			get { return new Color(0xFF000000); }
		}

		/// <summary>
		/// BlanchedAlmond color (R:255,G:235,B:205,A:255).
		/// </summary>
		public static Color BlanchedAlmond
		{
			get { return new Color(0xFFCDEBFF); }
		}

		/// <summary>
		/// Blue color (R:0,G:0,B:255,A:255).
		/// </summary>
		public static Color Blue
		{
			get { return new Color(0xFFFF0000); }
		}

		/// <summary>
		/// BlueViolet color (R:138,G:43,B:226,A:255).
		/// </summary>
		public static Color BlueViolet
		{
			get { return new Color(0xFFE22B8A); }
		}

		/// <summary>
		/// Brown color (R:165,G:42,B:42,A:255).
		/// </summary>
		public static Color Brown
		{
			get { return new Color(0xFF2A2AA5); }
		}

		/// <summary>
		/// BurlyWood color (R:222,G:184,B:135,A:255).
		/// </summary>
		public static Color BurlyWood
		{
			get { return new Color(0xFF87B8DE); }
		}

		/// <summary>
		/// CadetBlue color (R:95,G:158,B:160,A:255).
		/// </summary>
		public static Color CadetBlue
		{
			get { return new Color(0xFFA09E5F); }
		}

		/// <summary>
		/// Chartreuse color (R:127,G:255,B:0,A:255).
		/// </summary>
		public static Color Chartreuse
		{
			get { return new Color(0xFF00FF7F); }
		}

		/// <summary>
		/// Chocolate color (R:210,G:105,B:30,A:255).
		/// </summary>
		public static Color Chocolate
		{
			get { return new Color(0xFF1E69D2); }
		}

		/// <summary>
		/// Coral color (R:255,G:127,B:80,A:255).
		/// </summary>
		public static Color Coral
		{
			get { return new Color(0xFF507FFF); }
		}

		/// <summary>
		/// CornflowerBlue color (R:100,G:149,B:237,A:255).
		/// </summary>
		public static Color CornflowerBlue
		{
			get { return new Color(0xFFED9564); }
		}

		/// <summary>
		/// Cornsilk color (R:255,G:248,B:220,A:255).
		/// </summary>
		public static Color Cornsilk
		{
			get { return new Color(0xFFDCF8FF); }
		}

		/// <summary>
		/// Crimson color (R:220,G:20,B:60,A:255).
		/// </summary>
		public static Color Crimson
		{
			get { return new Color(0xFF3C14DC); }
		}

		/// <summary>
		/// Cyan color (R:0,G:255,B:255,A:255).
		/// </summary>
		public static Color Cyan
		{
			get { return new Color(0xFFFFFF00); }
		}

		/// <summary>
		/// DarkBlue color (R:0,G:0,B:139,A:255).
		/// </summary>
		public static Color DarkBlue
		{
			get { return new Color(0xFF8B0000); }
		}

		/// <summary>
		/// DarkCyan color (R:0,G:139,B:139,A:255).
		/// </summary>
		public static Color DarkCyan
		{
			get { return new Color(0xFF8B8B00); }
		}

		/// <summary>
		/// DarkGoldenrod color (R:184,G:134,B:11,A:255).
		/// </summary>
		public static Color DarkGoldenrod
		{
			get { return new Color(0xFF0B86B8); }
		}

		/// <summary>
		/// DarkGray color (R:169,G:169,B:169,A:255).
		/// </summary>
		public static Color DarkGray
		{
			get { return new Color(0xFFA9A9A9); }
		}

		/// <summary>
		/// DarkGreen color (R:0,G:100,B:0,A:255).
		/// </summary>
		public static Color DarkGreen
		{
			get { return new Color(0xFF006400); }
		}

		/// <summary>
		/// DarkKhaki color (R:189,G:183,B:107,A:255).
		/// </summary>
		public static Color DarkKhaki
		{
			get { return new Color(0xFF6BB7BD); }
		}

		/// <summary>
		/// DarkMagenta color (R:139,G:0,B:139,A:255).
		/// </summary>
		public static Color DarkMagenta
		{
			get { return new Color(0xFF8B008B); }
		}

		/// <summary>
		/// DarkOliveGreen color (R:85,G:107,B:47,A:255).
		/// </summary>
		public static Color DarkOliveGreen
		{
			get { return new Color(0xFF2F6B55); }
		}

		/// <summary>
		/// DarkOrange color (R:255,G:140,B:0,A:255).
		/// </summary>
		public static Color DarkOrange
		{
			get { return new Color(0xFF008CFF); }
		}

		/// <summary>
		/// DarkOrchid color (R:153,G:50,B:204,A:255).
		/// </summary>
		public static Color DarkOrchid
		{
			get { return new Color(0xFFCC3299); }
		}

		/// <summary>
		/// DarkRed color (R:139,G:0,B:0,A:255).
		/// </summary>
		public static Color DarkRed
		{
			get { return new Color(0xFF00008B); }
		}

		/// <summary>
		/// DarkSalmon color (R:233,G:150,B:122,A:255).
		/// </summary>
		public static Color DarkSalmon
		{
			get { return new Color(0xFF7A96E9); }
		}

		/// <summary>
		/// DarkSeaGreen color (R:143,G:188,B:139,A:255).
		/// </summary>
		public static Color DarkSeaGreen
		{
			get { return new Color(0xFF8BBC8F); }
		}

		/// <summary>
		/// DarkSlateBlue color (R:72,G:61,B:139,A:255).
		/// </summary>
		public static Color DarkSlateBlue
		{
			get { return new Color(0xFF8B3D48); }
		}

		/// <summary>
		/// DarkSlateGray color (R:47,G:79,B:79,A:255).
		/// </summary>
		public static Color DarkSlateGray
		{
			get { return new Color(0xFF4F4F2F); }
		}

		/// <summary>
		/// DarkTurquoise color (R:0,G:206,B:209,A:255).
		/// </summary>
		public static Color DarkTurquoise
		{
			get { return new Color(0xFFD1CE00); }
		}

		/// <summary>
		/// DarkViolet color (R:148,G:0,B:211,A:255).
		/// </summary>
		public static Color DarkViolet
		{
			get { return new Color(0xFFD30094); }
		}

		/// <summary>
		/// DeepPink color (R:255,G:20,B:147,A:255).
		/// </summary>
		public static Color DeepPink
		{
			get { return new Color(0xFF9314FF); }
		}

		/// <summary>
		/// DeepSkyBlue color (R:0,G:191,B:255,A:255).
		/// </summary>
		public static Color DeepSkyBlue
		{
			get { return new Color(0xFFFFBF00); }
		}

		/// <summary>
		/// DimGray color (R:105,G:105,B:105,A:255).
		/// </summary>
		public static Color DimGray
		{
			get { return new Color(0xFF696969); }
		}

		/// <summary>
		/// DodgerBlue color (R:30,G:144,B:255,A:255).
		/// </summary>
		public static Color DodgerBlue
		{
			get { return new Color(0xFFFF901E); }
		}

		/// <summary>
		/// Firebrick color (R:178,G:34,B:34,A:255).
		/// </summary>
		public static Color Firebrick
		{
			get { return new Color(0xFF2222B2); }
		}

		/// <summary>
		/// FloralWhite color (R:255,G:250,B:240,A:255).
		/// </summary>
		public static Color FloralWhite
		{
			get { return new Color(0xFFF0FAFF); }
		}

		/// <summary>
		/// ForestGreen color (R:34,G:139,B:34,A:255).
		/// </summary>
		public static Color ForestGreen
		{
			get { return new Color(0xFF228B22); }
		}

		/// <summary>
		/// Fuchsia color (R:255,G:0,B:255,A:255).
		/// </summary>
		public static Color Fuchsia
		{
			get { return new Color(0xFFFF00FF); }
		}

		/// <summary>
		/// Gainsboro color (R:220,G:220,B:220,A:255).
		/// </summary>
		public static Color Gainsboro
		{
			get { return new Color(0xFFDCDCDC); }
		}

		/// <summary>
		/// GhostWhite color (R:248,G:248,B:255,A:255).
		/// </summary>
		public static Color GhostWhite
		{
			get { return new Color(0xFFFFF8F8); }
		}

		/// <summary>
		/// Gold color (R:255,G:215,B:0,A:255).
		/// </summary>
		public static Color Gold
		{
			get { return new Color(0xFF00D7FF); }
		}

		/// <summary>
		/// Goldenrod color (R:218,G:165,B:32,A:255).
		/// </summary>
		public static Color Goldenrod
		{
			get { return new Color(0xFF20A5DA); }
		}

		/// <summary>
		/// Gray color (R:128,G:128,B:128,A:255).
		/// </summary>
		public static Color Gray
		{
			get { return new Color(0xFF808080); }
		}

		/// <summary>
		/// Green color (R:0,G:128,B:0,A:255).
		/// </summary>
		public static Color Green
		{
			get { return new Color(0xFF008000); }
		}

		/// <summary>
		/// GreenYellow color (R:173,G:255,B:47,A:255).
		/// </summary>
		public static Color GreenYellow
		{
			get { return new Color(0xFF2FFFAD); }
		}

		/// <summary>
		/// Honeydew color (R:240,G:255,B:240,A:255).
		/// </summary>
		public static Color Honeydew
		{
			get { return new Color(0xFFF0FFF0); }
		}

		/// <summary>
		/// HotPink color (R:255,G:105,B:180,A:255).
		/// </summary>
		public static Color HotPink
		{
			get { return new Color(0xFFB469FF); }
		}

		/// <summary>
		/// IndianRed color (R:205,G:92,B:92,A:255).
		/// </summary>
		public static Color IndianRed
		{
			get { return new Color(0xFF5C5CCD); }
		}

		/// <summary>
		/// Indigo color (R:75,G:0,B:130,A:255).
		/// </summary>
		public static Color Indigo
		{
			get { return new Color(0xFF82004B); }
		}

		/// <summary>
		/// Ivory color (R:255,G:255,B:240,A:255).
		/// </summary>
		public static Color Ivory
		{
			get { return new Color(0xFFF0FFFF); }
		}

		/// <summary>
		/// Khaki color (R:240,G:230,B:140,A:255).
		/// </summary>
		public static Color Khaki
		{
			get { return new Color(0xFF8CE6F0); }
		}

		/// <summary>
		/// Lavender color (R:230,G:230,B:250,A:255).
		/// </summary>
		public static Color Lavender
		{
			get { return new Color(0xFFFAE6E6); }
		}

		/// <summary>
		/// LavenderBlush color (R:255,G:240,B:245,A:255).
		/// </summary>
		public static Color LavenderBlush
		{
			get { return new Color(0xFFF5F0FF); }
		}

		/// <summary>
		/// LawnGreen color (R:124,G:252,B:0,A:255).
		/// </summary>
		public static Color LawnGreen
		{
			get { return new Color(0xFF00FC7C); }
		}

		/// <summary>
		/// LemonChiffon color (R:255,G:250,B:205,A:255).
		/// </summary>
		public static Color LemonChiffon
		{
			get { return new Color(0xFFCDFAFF); }
		}

		/// <summary>
		/// LightBlue color (R:173,G:216,B:230,A:255).
		/// </summary>
		public static Color LightBlue
		{
			get { return new Color(0xFFE6D8AD); }
		}

		/// <summary>
		/// LightCoral color (R:240,G:128,B:128,A:255).
		/// </summary>
		public static Color LightCoral
		{
			get { return new Color(0xFF8080F0); }
		}

		/// <summary>
		/// LightCyan color (R:224,G:255,B:255,A:255).
		/// </summary>
		public static Color LightCyan
		{
			get { return new Color(0xFFFFFFE0); }
		}

		/// <summary>
		/// LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
		/// </summary>
		public static Color LightGoldenrodYellow
		{
			get { return new Color(0xFFD2FAFA); }
		}

		/// <summary>
		/// LightGreen color (R:144,G:238,B:144,A:255).
		/// </summary>
		public static Color LightGreen
		{
			get { return new Color(0xFF90EE90); }
		}

		/// <summary>
		/// LightGray color (R:211,G:211,B:211,A:255).
		/// </summary>
		public static Color LightGray
		{
			get { return new Color(0xFFD3D3D3); }
		}

		/// <summary>
		/// LightPink color (R:255,G:182,B:193,A:255).
		/// </summary>
		public static Color LightPink
		{
			get { return new Color(0xFFC1B6FF); }
		}

		/// <summary>
		/// LightSalmon color (R:255,G:160,B:122,A:255).
		/// </summary>
		public static Color LightSalmon
		{
			get { return new Color(0xFF7AA0FF); }
		}

		/// <summary>
		/// LightSeaGreen color (R:32,G:178,B:170,A:255).
		/// </summary>
		public static Color LightSeaGreen
		{
			get { return new Color(0xFFAAB220); }
		}

		/// <summary>
		/// LightSkyBlue color (R:135,G:206,B:250,A:255).
		/// </summary>
		public static Color LightSkyBlue
		{
			get { return new Color(0xFFFACE87); }
		}

		/// <summary>
		/// LightSlateGray color (R:119,G:136,B:153,A:255).
		/// </summary>
		public static Color LightSlateGray
		{
			get { return new Color(0xFF998877); }
		}

		/// <summary>
		/// LightSteelBlue color (R:176,G:196,B:222,A:255).
		/// </summary>
		public static Color LightSteelBlue
		{
			get { return new Color(0xFFDEC4B0); }
		}

		/// <summary>
		/// LightYellow color (R:255,G:255,B:224,A:255).
		/// </summary>
		public static Color LightYellow
		{
			get { return new Color(0xFFE0FFFF); }
		}

		/// <summary>
		/// Lime color (R:0,G:255,B:0,A:255).
		/// </summary>
		public static Color Lime
		{
			get { return new Color(0xFF00FF00); }
		}

		/// <summary>
		/// LimeGreen color (R:50,G:205,B:50,A:255).
		/// </summary>
		public static Color LimeGreen
		{
			get { return new Color(0xFF32CD32); }
		}

		/// <summary>
		/// Linen color (R:250,G:240,B:230,A:255).
		/// </summary>
		public static Color Linen
		{
			get { return new Color(0xFFE6F0FA); }
		}

		/// <summary>
		/// Magenta color (R:255,G:0,B:255,A:255).
		/// </summary>
		public static Color Magenta
		{
			get { return new Color(0xFFFF00FF); }
		}

		/// <summary>
		/// Maroon color (R:128,G:0,B:0,A:255).
		/// </summary>
		public static Color Maroon
		{
			get { return new Color(0xFF000080); }
		}

		/// <summary>
		/// MediumAquamarine color (R:102,G:205,B:170,A:255).
		/// </summary>
		public static Color MediumAquamarine
		{
			get { return new Color(0xFFAACD66); }
		}

		/// <summary>
		/// MediumBlue color (R:0,G:0,B:205,A:255).
		/// </summary>
		public static Color MediumBlue
		{
			get { return new Color(0xFFCD0000); }
		}

		/// <summary>
		/// MediumOrchid color (R:186,G:85,B:211,A:255).
		/// </summary>
		public static Color MediumOrchid
		{
			get { return new Color(0xFFD355BA); }
		}

		/// <summary>
		/// MediumPurple color (R:147,G:112,B:219,A:255).
		/// </summary>
		public static Color MediumPurple
		{
			get { return new Color(0xFFDB7093); }
		}

		/// <summary>
		/// MediumSeaGreen color (R:60,G:179,B:113,A:255).
		/// </summary>
		public static Color MediumSeaGreen
		{
			get { return new Color(0xFF71B33C); }
		}

		/// <summary>
		/// MediumSlateBlue color (R:123,G:104,B:238,A:255).
		/// </summary>
		public static Color MediumSlateBlue
		{
			get { return new Color(0xFFEE687B); }
		}

		/// <summary>
		/// MediumSpringGreen color (R:0,G:250,B:154,A:255).
		/// </summary>
		public static Color MediumSpringGreen
		{
			get { return new Color(0xFF9AFA00); }
		}

		/// <summary>
		/// MediumTurquoise color (R:72,G:209,B:204,A:255).
		/// </summary>
		public static Color MediumTurquoise
		{
			get { return new Color(0xFFCCD148); }
		}

		/// <summary>
		/// MediumVioletRed color (R:199,G:21,B:133,A:255).
		/// </summary>
		public static Color MediumVioletRed
		{
			get { return new Color(0xFF8515C7); }
		}

		/// <summary>
		/// MidnightBlue color (R:25,G:25,B:112,A:255).
		/// </summary>
		public static Color MidnightBlue
		{
			get { return new Color(0xFF701919); }
		}

		/// <summary>
		/// MintCream color (R:245,G:255,B:250,A:255).
		/// </summary>
		public static Color MintCream
		{
			get { return new Color(0xFFFAFFF5); }
		}

		/// <summary>
		/// MistyRose color (R:255,G:228,B:225,A:255).
		/// </summary>
		public static Color MistyRose
		{
			get { return new Color(0xFFE1E4FF); }
		}

		/// <summary>
		/// Moccasin color (R:255,G:228,B:181,A:255).
		/// </summary>
		public static Color Moccasin
		{
			get { return new Color(0xFFB5E4FF); }
		}

		/// <summary>
		/// NavajoWhite color (R:255,G:222,B:173,A:255).
		/// </summary>
		public static Color NavajoWhite
		{
			get { return new Color(0xFFADDEFF); }
		}

		/// <summary>
		/// Navy color (R:0,G:0,B:128,A:255).
		/// </summary>
		public static Color Navy
		{
			get { return new Color(0xFF800000); }
		}

		/// <summary>
		/// OldLace color (R:253,G:245,B:230,A:255).
		/// </summary>
		public static Color OldLace
		{
			get { return new Color(0xFFE6F5FD); }
		}

		/// <summary>
		/// Olive color (R:128,G:128,B:0,A:255).
		/// </summary>
		public static Color Olive
		{
			get { return new Color(0xFF008080); }
		}

		/// <summary>
		/// OliveDrab color (R:107,G:142,B:35,A:255).
		/// </summary>
		public static Color OliveDrab
		{
			get { return new Color(0xFF238E6B); }
		}

		/// <summary>
		/// Orange color (R:255,G:165,B:0,A:255).
		/// </summary>
		public static Color Orange
		{
			get { return new Color(0xFF00A5FF); }
		}

		/// <summary>
		/// OrangeRed color (R:255,G:69,B:0,A:255).
		/// </summary>
		public static Color OrangeRed
		{
			get { return new Color(0xFF0045FF); }
		}

		/// <summary>
		/// Orchid color (R:218,G:112,B:214,A:255).
		/// </summary>
		public static Color Orchid
		{
			get { return new Color(0xFFD670DA); }
		}

		/// <summary>
		/// PaleGoldenrod color (R:238,G:232,B:170,A:255).
		/// </summary>
		public static Color PaleGoldenrod
		{
			get { return new Color(0xFFAAE8EE); }
		}

		/// <summary>
		/// PaleGreen color (R:152,G:251,B:152,A:255).
		/// </summary>
		public static Color PaleGreen
		{
			get { return new Color(0xFF98FB98); }
		}

		/// <summary>
		/// PaleTurquoise color (R:175,G:238,B:238,A:255).
		/// </summary>
		public static Color PaleTurquoise
		{
			get { return new Color(0xFFEEEEAF); }
		}

		/// <summary>
		/// PaleVioletRed color (R:219,G:112,B:147,A:255).
		/// </summary>
		public static Color PaleVioletRed
		{
			get { return new Color(0xFF9370DB); }
		}

		/// <summary>
		/// PapayaWhip color (R:255,G:239,B:213,A:255).
		/// </summary>
		public static Color PapayaWhip
		{
			get { return new Color(0xFFD5EFFF); }
		}

		/// <summary>
		/// PeachPuff color (R:255,G:218,B:185,A:255).
		/// </summary>
		public static Color PeachPuff
		{
			get { return new Color(0xFFB9DAFF); }
		}

		/// <summary>
		/// Peru color (R:205,G:133,B:63,A:255).
		/// </summary>
		public static Color Peru
		{
			get { return new Color(0xFF3F85CD); }
		}

		/// <summary>
		/// Pink color (R:255,G:192,B:203,A:255).
		/// </summary>
		public static Color Pink
		{
			get { return new Color(0xFFCBC0FF); }
		}

		/// <summary>
		/// Plum color (R:221,G:160,B:221,A:255).
		/// </summary>
		public static Color Plum
		{
			get { return new Color(0xFFDDA0DD); }
		}

		/// <summary>
		/// PowderBlue color (R:176,G:224,B:230,A:255).
		/// </summary>
		public static Color PowderBlue
		{
			get { return new Color(0xFFE6E0B0); }
		}

		/// <summary>
		/// Purple color (R:128,G:0,B:128,A:255).
		/// </summary>
		public static Color Purple
		{
			get { return new Color(0xFF800080); }
		}

		/// <summary>
		/// Red color (R:255,G:0,B:0,A:255).
		/// </summary>
		public static Color Red
		{
			get { return new Color(0xFF0000FF); }
		}

		/// <summary>
		/// RosyBrown color (R:188,G:143,B:143,A:255).
		/// </summary>
		public static Color RosyBrown
		{
			get { return new Color(0xFF8F8FBC); }
		}

		/// <summary>
		/// RoyalBlue color (R:65,G:105,B:225,A:255).
		/// </summary>
		public static Color RoyalBlue
		{
			get { return new Color(0xFFE16941); }
		}

		/// <summary>
		/// SaddleBrown color (R:139,G:69,B:19,A:255).
		/// </summary>
		public static Color SaddleBrown
		{
			get { return new Color(0xFF13458B); }
		}

		/// <summary>
		/// Salmon color (R:250,G:128,B:114,A:255).
		/// </summary>
		public static Color Salmon
		{
			get { return new Color(0xFF7280FA); }
		}

		/// <summary>
		/// SandyBrown color (R:244,G:164,B:96,A:255).
		/// </summary>
		public static Color SandyBrown
		{
			get { return new Color(0xFF60A4F4); }
		}

		/// <summary>
		/// SeaGreen color (R:46,G:139,B:87,A:255).
		/// </summary>
		public static Color SeaGreen
		{
			get { return new Color(0xFF578B2E); }
		}

		/// <summary>
		/// SeaShell color (R:255,G:245,B:238,A:255).
		/// </summary>
		public static Color SeaShell
		{
			get { return new Color(0xFFEEF5FF); }
		}

		/// <summary>
		/// Sienna color (R:160,G:82,B:45,A:255).
		/// </summary>
		public static Color Sienna
		{
			get { return new Color(0xFF2D52A0); }
		}

		/// <summary>
		/// Silver color (R:192,G:192,B:192,A:255).
		/// </summary>
		public static Color Silver
		{
			get { return new Color(0xFFC0C0C0); }
		}

		/// <summary>
		/// SkyBlue color (R:135,G:206,B:235,A:255).
		/// </summary>
		public static Color SkyBlue
		{
			get { return new Color(0xFFEBCE87); }
		}

		/// <summary>
		/// SlateBlue color (R:106,G:90,B:205,A:255).
		/// </summary>
		public static Color SlateBlue
		{
			get { return new Color(0xFFCD5A6A); }
		}

		/// <summary>
		/// SlateGray color (R:112,G:128,B:144,A:255).
		/// </summary>
		public static Color SlateGray
		{
			get { return new Color(0xFF908070); }
		}

		/// <summary>
		/// Snow color (R:255,G:250,B:250,A:255).
		/// </summary>
		public static Color Snow
		{
			get { return new Color(0xFFFAFAFF); }
		}

		/// <summary>
		/// SpringGreen color (R:0,G:255,B:127,A:255).
		/// </summary>
		public static Color SpringGreen
		{
			get { return new Color(0xFF7FFF00); }
		}

		/// <summary>
		/// SteelBlue color (R:70,G:130,B:180,A:255).
		/// </summary>
		public static Color SteelBlue
		{
			get { return new Color(0xFFB48246); }
		}

		/// <summary>
		/// Tan color (R:210,G:180,B:140,A:255).
		/// </summary>
		public static Color Tan
		{
			get { return new Color(0xFF8CB4D2); }
		}

		/// <summary>
		/// Teal color (R:0,G:128,B:128,A:255).
		/// </summary>
		public static Color Teal
		{
			get { return new Color(0xFF808000); }
		}

		/// <summary>
		/// Thistle color (R:216,G:191,B:216,A:255).
		/// </summary>
		public static Color Thistle
		{
			get { return new Color(0xFFD8BFD8); }
		}

		/// <summary>
		/// Tomato color (R:255,G:99,B:71,A:255).
		/// </summary>
		public static Color Tomato
		{
			get { return new Color(0xFF4763FF); }
		}

		/// <summary>
		/// Turquoise color (R:64,G:224,B:208,A:255).
		/// </summary>
		public static Color Turquoise
		{
			get { return new Color(0xFFD0E040); }
		}

		/// <summary>
		/// Violet color (R:238,G:130,B:238,A:255).
		/// </summary>
		public static Color Violet
		{
			get { return new Color(0xFFEE82EE); }
		}

		/// <summary>
		/// Wheat color (R:245,G:222,B:179,A:255).
		/// </summary>
		public static Color Wheat
		{
			get { return new Color(0xFFB3DEF5); }
		}

		/// <summary>
		/// White color (R:255,G:255,B:255,A:255).
		/// </summary>
		public static Color White
		{
			get { return new Color(0xFFFFFFFF); }
		}

		/// <summary>
		/// WhiteSmoke color (R:245,G:245,B:245,A:255).
		/// </summary>
		public static Color WhiteSmoke
		{
			get { return new Color(0xFFF5F5F5); }
		}

		/// <summary>
		/// Yellow color (R:255,G:255,B:0,A:255).
		/// </summary>
		public static Color Yellow
		{
			get { return new Color(0xFF00FFFF); }
		}

		/// <summary>
		/// YellowGreen color (R:154,G:205,B:50,A:255).
		/// </summary>
		public static Color YellowGreen
		{
			get { return new Color(0xFF32CD9A); }
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					R.ToString(), " ",
					G.ToString(), " ",
					B.ToString(), " ",
					A.ToString()
				);
			}
		}

		#endregion

		#region Private Variables

		// ARGB. Keep this name as it is used by XNA games in reflection!
		internal uint packedValue;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a new instance of <see cref="Color"/> struct.
		/// </summary>
		/// <param name="color">A <see cref="Vector4"/> representing a color.</param>
		public Color(Vector4 color)
		{
			packedValue = 0;

			R = (byte) MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
			A = (byte) MathHelper.Clamp(color.W * 255, Byte.MinValue, Byte.MaxValue);
		}

		/// <summary>
		/// Constructs an RGBA color from the XYZW unit length components of a vector.
		/// </summary>
		/// <param name="color">A <see cref="Vector3"/> representing a color.</param>
		public Color(Vector3 color)
		{
			packedValue = 0;

			R = (byte) MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
			A = 255;
		}

		/// <summary>
		/// Constructs an RGBA color from scalars which representing red, green and blue values. Alpha value will be opaque.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		public Color(float r, float g, float b)
		{
			packedValue = 0;

			R = (byte) MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
			A = 255;
		}

		/// <summary>
		/// Constructs an RGBA color from scalars which representing red, green and blue values. Alpha value will be opaque.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		public Color(int r, int g, int b)
		{
			packedValue = 0;
			R = (byte) MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
			A = (byte)255;
		}

		/// <summary>
		/// Constructs an RGBA color from scalars which representing red, green, blue and alpha values.
		/// </summary>
		/// <param name="r">Red component value from 0 to 255.</param>
		/// <param name="g">Green component value from 0 to 255.</param>
		/// <param name="b">Blue component value from 0 to 255.</param>
		/// <param name="alpha">Alpha component value from 0 to 255.</param>
		public Color(int r, int g, int b, int alpha)
		{
			packedValue = 0;
			R = (byte) MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
			A = (byte) MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);
		}

		/// <summary>
		/// Constructs an RGBA color from scalars which representing red, green, blue and alpha values.
		/// </summary>
		/// <param name="r">Red component value from 0.0f to 1.0f.</param>
		/// <param name="g">Green component value from 0.0f to 1.0f.</param>
		/// <param name="b">Blue component value from 0.0f to 1.0f.</param>
		/// <param name="alpha">Alpha component value from 0.0f to 1.0f.</param>
		public Color(float r, float g, float b, float alpha)
		{
			packedValue = 0;

			R = (byte) MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
			G = (byte) MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
			B = (byte) MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
			A = (byte) MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
		}

		#endregion

		#region Private Constructors

		private Color(uint packedValue)
		{
			this.packedValue = packedValue;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Color"/>.
		/// </summary>
		/// <param name="other">The <see cref="Color"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Color other)
		{
			return this.packedValue == other.packedValue;
		}

		/// <summary>
		/// Gets a <see cref="Vector3"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Vector3"/> representation for this object.</returns>
		public Vector3 ToVector3()
		{
			return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
		}

		/// <summary>
		/// Gets a <see cref="Vector4"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Vector4"/> representation for this object.</returns>
		public Vector4 ToVector4()
		{
			return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Performs linear interpolation of <see cref="Color"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Color"/>.</param>
		/// <param name="value2">Destination <see cref="Color"/>.</param>
		/// <param name="amount">Interpolation factor.</param>
		/// <returns>Interpolated <see cref="Color"/>.</returns>
		public static Color Lerp(Color value1, Color value2, float amount)
		{
			amount = MathHelper.Clamp(amount, 0.0f, 1.0f);
			return new Color(
				(int) MathHelper.Lerp(value1.R, value2.R, amount),
				(int) MathHelper.Lerp(value1.G, value2.G, amount),
				(int) MathHelper.Lerp(value1.B, value2.B, amount),
				(int) MathHelper.Lerp(value1.A, value2.A, amount)
			);
		}

		/// <summary>
		/// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/>
		/// that contains premultiplied alpha.
		/// </summary>
		/// <param name="vector">A <see cref="Vector4"/> representing color.</param>
		/// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
		public static Color FromNonPremultiplied(Vector4 vector)
		{
			return new Color(
				vector.X * vector.W,
				vector.Y * vector.W,
				vector.Z * vector.W,
				vector.W
			);
		}

		/// <summary>
		/// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/>
		/// that contains premultiplied alpha.
		/// </summary>
		/// <param name="r">Red component value.</param>
		/// <param name="g">Green component value.</param>
		/// <param name="b">Blue component value.</param>
		/// <param name="a">Alpha component value.</param>
		/// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
		public static Color FromNonPremultiplied(int r, int g, int b, int a)
		{
			return new Color(
				(r * a / 255),
				(g * a / 255),
				(b * a / 255),
				a
			);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares whether two <see cref="Color"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="Color"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="Color"/> instance on the right of the equal sign.</param>
		/// <returns><c>True</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Color a, Color b)
		{
			return a.packedValue == b .packedValue;
		}

		/// <summary>
		/// Compares whether two <see cref="Color"/> instances are not equal.
		/// </summary>
		/// <param name="a">
		/// <see cref="Color"/> instance on the left of the not equal sign.
		/// </param>
		/// <param name="b">
		/// <see cref="Color"/> instance on the right of the not equal sign.
		/// </param>
		/// <returns>
		/// <c>True</c> if the instances are not equal; <c>false</c> otherwise.
		/// </returns>
		public static bool operator !=(Color a, Color b)
		{
			return a.packedValue != b.packedValue;
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Color"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Color"/>.</returns>
		public override int GetHashCode()
		{
			return this.packedValue.GetHashCode();
		}

		/// <summary>
		/// Compares whether current instance is equal to specified object.
		/// </summary>
		/// <param name="obj">The <see cref="Color"/> to compare.</param>
		/// <returns><c>True</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return ((obj is Color) && this.Equals((Color) obj));
		}

		/// <summary>
		/// Multiply <see cref="Color"/> by value.
		/// </summary>
		/// <param name="value">Source <see cref="Color"/>.</param>
		/// <param name="scale">Multiplicator.</param>
		/// <returns>Multiplication result.</returns>
		public static Color Multiply(Color value, float scale)
		{
			return new Color(
				(int) (value.R * scale),
				(int) (value.G * scale),
				(int) (value.B * scale),
				(int) (value.A * scale)
			);
		}

		/// <summary>
		/// Multiply <see cref="Color"/> by value.
		/// </summary>
		/// <param name="value">Source <see cref="Color"/>.</param>
		/// <param name="scale">Multiplicator.</param>
		/// <returns>Multiplication result.</returns>
		public static Color operator *(Color value, float scale)
		{
			return new Color(
				(int) (value.R * scale),
				(int) (value.G * scale),
				(int) (value.B * scale),
				(int) (value.A * scale)
			);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Color"/> in the format:
		/// {R:[red] G:[green] B:[blue] A:[alpha]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="Color"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(25);
			sb.Append("{R:");
			sb.Append(R);
			sb.Append(" G:");
			sb.Append(G);
			sb.Append(" B:");
			sb.Append(B);
			sb.Append(" A:");
			sb.Append(A);
			sb.Append("}");
			return sb.ToString();
		}

		#endregion

		#region IPackedVector Member

		/// <summary>
		/// Pack a four-component color from a vector format into the format of a color object.
		/// </summary>
		/// <param name="vector">A four-component color.</param>
		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			// Should we round here?
			R = (byte) (vector.X * 255.0f);
			G = (byte) (vector.Y * 255.0f);
			B = (byte) (vector.Z * 255.0f);
			A = (byte) (vector.W * 255.0f);
		}

		#endregion

	}
}
