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
#endregion

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct NormalizedShort4 : IPackedVector<ulong>, IEquatable<NormalizedShort4>
	{
		#region Public Properties

		[CLSCompliant(false)]
		public ulong PackedValue
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

		#region Private Variables

		private ulong packedValue;

		#endregion

		#region Public Constructors

		public NormalizedShort4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		public NormalizedShort4(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		#endregion

		#region Public Methods

		public Vector4 ToVector4()
		{
			const float maxVal = 0x7FFF;

			return new Vector4(
				((short) ((packedValue >> 0x00) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x10) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x20) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x30) & 0xFFFF)) / maxVal
			);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
		{
			return a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedShort4) && Equals((NormalizedShort4) obj);
		}

		public bool Equals(NormalizedShort4 other)
		{
			return packedValue.Equals(other.packedValue);
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		#endregion

		#region Private Static Pack Method

		private static ulong Pack(float vectorX, float vectorY, float vectorZ, float vectorW)
		{
			const float maxPos = 0x7FFF;
			const float minNeg = -maxPos;

			return (
				(((ulong) MathHelper.Clamp((float) Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF) << 0x00) |
				(((ulong) MathHelper.Clamp((float) Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10) |
				(((ulong) MathHelper.Clamp((float) Math.Round(vectorZ * maxPos), minNeg, maxPos) & 0xFFFF) << 0x20) |
				(((ulong) MathHelper.Clamp((float) Math.Round(vectorW * maxPos), minNeg, maxPos) & 0xFFFF) << 0x30)
			);
		}

		#endregion
	}
}
