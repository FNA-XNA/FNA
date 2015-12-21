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
	public struct Short2 : IPackedVector<uint>, IEquatable<Short2>
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

		public Short2(Vector2 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		public Short2(Single x,Single y)
		{
			packedValue = Pack(x, y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			return new Vector2(
				(short) (packedValue & 0xFFFF),
				(short) (packedValue >> 0x10)
			);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(
				(short) (packedValue & 0xFFFF),
				(short) (packedValue >> 0x10),
				0,
				1
			);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(Short2 a, Short2 b)
		{
			return a.packedValue != b.packedValue;
		}

		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.packedValue == b.packedValue;
		}

		public override bool Equals(object obj)
		{
			return (obj is Short2) && Equals((Short2) obj);
		}

		public bool Equals(Short2 other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public override string ToString()
		{
			return packedValue.ToString("x8");
		}

		#endregion

		#region Private Static Pack Method

		private static uint Pack(float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
			const float minNeg = ~((int) maxPos);

			return (
				((uint) ((int) Math.Max (Math.Min (vectorX, maxPos), minNeg) & 0xFFFF)) |
				((uint) (((int) Math.Max (Math.Min (vectorY, maxPos), minNeg) & 0xFFFF) << 0x10))
			);
		}

		#endregion
	}
}
