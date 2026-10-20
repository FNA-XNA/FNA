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
#endregion

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	internal static class HalfTypeHelper
	{
		#region Internal Static Methods

		internal static ushort Convert(float value)
		{
			const uint MinExp = 0x38800000u;
			const uint Exponent126 = 0x3f000000;
			const uint SingleBiasedExponentMask = 0x7F800000;
			const uint Exponent13 = 0x06800000u;
			uint bitValue = SingleToUInt32Bits(value);
			uint sign = bitValue >> 16 & 1u << 15;
			bitValue &= 0x7FFFFFFF;
			bitValue = Math.Min(bitValue, 0x47FFEFFF);
			value = UInt32BitsToSingle(bitValue);
			bitValue = Math.Max(bitValue, MinExp);
			value += UInt32BitsToSingle(bitValue + Exponent13 & SingleBiasedExponentMask);
			bitValue = SingleToUInt32Bits(value);
			bitValue -= Exponent126;
			return (ushort) (bitValue + (bitValue >> 13) | sign);
		}

		internal static float Convert(ushort value)
		{
			uint rst;
			uint mantissa = (uint)(value & 1023);
			uint exp = 0xfffffff2;

			if ((value & 0x7C00) == 0)
			{
				if (mantissa != 0)
				{
					while ((mantissa & 1024) == 0)
					{
						exp--;
						mantissa = mantissa << 1;
					}
					mantissa &= 0xfffffbff;
					rst = ((uint) ((((uint) value & 0x8000) << 16) | ((exp + 127) << 23))) | (mantissa << 13);
				}
				else
				{
					rst = (uint) ((value & 0x8000) << 16);
				}
			}
			else
			{
				rst = (uint) (((((uint) value & 0x8000) << 16) | ((((((uint) value >> 10) & 0x1f) - 15) + 127) << 23)) | (mantissa << 13));
			}

			unsafe
			{
				return *(float*) &rst;
			}
		}

		#endregion

		#region Private Static Methods

		static unsafe uint SingleToUInt32Bits(float value)
		{
			return *(uint*) &value;
		}

		static unsafe float UInt32BitsToSingle(uint value)
		{
			return *(float*) &value;
		}

		#endregion
	}
}
