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
	public struct NormalizedByte2 : IPackedVector<ushort>, IEquatable<NormalizedByte2>
	{
		#region Public Properties

		[CLSCompliant(false)]
		public ushort PackedValue
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

		private ushort packedValue;

		#endregion

		#region Public Constructors

		public NormalizedByte2(Vector2 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		public NormalizedByte2(float x, float y)
		{
			packedValue = Pack(x, y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			return new Vector2(
				((sbyte) (packedValue & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 8) & 0xFF)) / 127.0f
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
			return new Vector4(ToVector2(), 0.0f, 1.0f);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(NormalizedByte2 a, NormalizedByte2 b)
		{
			return a.packedValue != b.packedValue;
		}

		public static bool operator ==(NormalizedByte2 a, NormalizedByte2 b)
		{
			return a.packedValue == b.packedValue;
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedByte2) && Equals((NormalizedByte2) obj);
		}

		public bool Equals(NormalizedByte2 other)
		{
			return packedValue == other.packedValue;
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

		private static ushort Pack(float x, float y)
		{
			return (ushort) (
				((((ushort) (MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) << 0) & 0x00FF) |
				((((ushort) (MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) << 8) & 0xFF00)
			);
		}

		#endregion
	}
}
