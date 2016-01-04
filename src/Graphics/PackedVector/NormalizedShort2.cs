#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
	{
		#region Public Properties

		[CLSCompliant(false)]
		public uint PackedValue
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

		private uint packedValue;

		#endregion

		#region Public Constructors

		public NormalizedShort2(Vector2 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		public NormalizedShort2(float x, float y)
		{
			packedValue = Pack(x, y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			const float maxVal = 0x7FFF;

			return new Vector2(
				((short) (packedValue & 0xFFFF)) / maxVal,
				((short) (packedValue >> 0x10)) / maxVal
			);;
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4()
		{
			const float maxVal = 0x7FFF;

			return new Vector4(
				((short) ((packedValue >> 0x00) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x10) & 0xFFFF)) / maxVal,
				0,
				1
			);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
		{
			return a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedShort2) && Equals((NormalizedShort2) obj);
		}

		public bool Equals(NormalizedShort2 other)
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

		private static uint Pack(float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
			const float minNeg = -maxPos;

			// Round rather than truncate
			return (
				((uint) ((int) MathHelper.Clamp((float) Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF)) |
				((uint) (((int) MathHelper.Clamp((float) Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10))
			);
		}

		#endregion
	}
}
