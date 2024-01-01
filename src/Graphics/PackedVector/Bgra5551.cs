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
	/// <summary>
	/// Packed vector type containing unsigned normalized values ranging from 0 to 1. 
	/// The x and z components use 5 bits, and the y component uses 6 bits.
	/// </summary>
	public struct Bgra5551 : IPackedVector<ushort>, IEquatable<Bgra5551>, IPackedVector
	{
		#region Public Properties

		/// <summary>
		/// Gets and sets the packed value.
		/// </summary>
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

		/// <summary>
		/// Creates a new instance of Bgra5551.
		/// </summary>
		/// <param name="x">The x component</param>
		/// <param name="y">The y component</param>
		/// <param name="z">The z component</param>
		/// <param name="w">The w component</param>
		public Bgra5551(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		/// <summary>
		/// Creates a new instance of Bgra5551.
		/// </summary>
		/// <param name="vector">
		/// Vector containing the components for the packed vector.
		/// </param>
		public Bgra5551(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the packed vector in Vector4 format.
		/// </summary>
		/// <returns>The packed vector in Vector4 format</returns>
		public Vector4 ToVector4()
		{
			return new Vector4(
				((packedValue >> 10) & 0x1F) / 31.0f,
				((packedValue >> 5) & 0x1F) / 31.0f,
				(packedValue & 0x1F) / 31.0f,
				(float) (packedValue >> 15)
			);
		}

		#endregion

		#region IPackedVector Methods

		/// <summary>
		/// Sets the packed vector from a Vector4.
		/// </summary>
		/// <param name="vector">Vector containing the components.</param>
		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares an object with the packed vector.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>True if the object is equal to the packed vector.</returns>
		public override bool Equals(object obj)
		{
			return (obj is Bgra5551) && Equals((Bgra5551) obj);
		}

		/// <summary>
		/// Compares another Bgra5551 packed vector with the packed vector.
		/// </summary>
		/// <param name="other">The Bgra5551 packed vector to compare.</param>
		/// <returns>True if the packed vectors are equal.</returns>
		public bool Equals(Bgra5551 other)
		{
			return packedValue == other.packedValue;
		}

		/// <summary>
		/// Gets a string representation of the packed vector.
		/// </summary>
		/// <returns>A string representation of the packed vector.</returns>
		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		/// <summary>
		/// Gets a hash code of the packed vector.
		/// </summary>
		/// <returns>The hash code for the packed vector.</returns>
		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public static bool operator ==(Bgra5551 lhs, Bgra5551 rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

		public static bool operator !=(Bgra5551 lhs, Bgra5551 rhs)
		{
			return lhs.packedValue != rhs.packedValue;
		}

		#endregion

		#region Private Static Pack Method

		private static ushort Pack(float x, float y, float z, float w)
		{
			return (ushort) (
				(((ushort) Math.Round(MathHelper.Clamp(x, 0, 1) * 31.0f)) << 10) |
				(((ushort) Math.Round(MathHelper.Clamp(y, 0, 1) * 31.0f)) << 5) |
				((ushort) Math.Round(MathHelper.Clamp(z, 0, 1) * 31.0f)) |
				((ushort) Math.Round(MathHelper.Clamp(w, 0, 1)) << 15)
			);
		}

		#endregion
	}
}
