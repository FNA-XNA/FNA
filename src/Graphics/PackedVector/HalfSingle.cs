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
	public struct HalfSingle : IPackedVector<UInt16>, IEquatable<HalfSingle>, IPackedVector
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

		private UInt16 packedValue;

		#endregion

		#region Public Constructors

		public HalfSingle(float single)
		{
			packedValue = HalfTypeHelper.Convert(single);
		}

		#endregion

		#region Public Methods

		public float ToSingle()
		{
			return HalfTypeHelper.Convert(packedValue);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = HalfTypeHelper.Convert(vector.X);
		}

		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(ToSingle(), 0f, 0f, 1f);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override bool Equals(object obj)
		{
			return (obj is HalfSingle) && Equals((HalfSingle) obj);
		}

		public bool Equals(HalfSingle other)
		{
			return packedValue == other.packedValue;
		}

		public override string ToString()
		{
			return ToSingle().ToString();
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

		public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
		{
			return lhs.packedValue != rhs.packedValue;
		}

		#endregion
	}
}
