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
	/// <summary>
	/// Packed vector type containing unsigned normalized values ranging from 0 to 1. 
	/// The x and z components use 5 bits, and the y component uses 6 bits.
	/// </summary>
	public struct Alpha8 : IPackedVector<byte>, IEquatable<Alpha8>, IPackedVector
	{
		#region Public Properties

		/// <summary>
		/// Gets and sets the packed value.
		/// </summary>
		[CLSCompliant(false)]
		public byte PackedValue
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

		private byte packedValue;

		#endregion

		#region Public Constructor

		/// <summary>
		/// Creates a new instance of Alpha8.
		/// </summary>
		/// <param name="alpha">The alpha component</param>
		public Alpha8(float alpha)
		{
			packedValue = Pack(alpha);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the packed vector in float format.
		/// </summary>
		/// <returns>The packed vector in Vector3 format</returns>
		public float ToAlpha()
		{
			return (float) (packedValue / 255.0f);
		}

		#endregion

		#region IPackedVector Methods

		/// <summary>
		/// Sets the packed vector from a Vector4.
		/// </summary>
		/// <param name="vector">Vector containing the components.</param>
		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.W);
		}

		/// <summary>
		/// Gets the packed vector in Vector4 format.
		/// </summary>
		/// <returns>The packed vector in Vector4 format</returns>
		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(
				0.0f,
				0.0f,
				0.0f,
				(float) (packedValue / 255.0f)
			);
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
			return (obj is Alpha8) && Equals((Alpha8) obj);
		}

		/// <summary>
		/// Compares another Bgra5551 packed vector with the packed vector.
		/// </summary>
		/// <param name="other">The Bgra5551 packed vector to compare.</param>
		/// <returns>True if the packed vectors are equal.</returns>
		public bool Equals(Alpha8 other)
		{
			return packedValue == other.packedValue;
		}

		/// <summary>
		/// Gets a string representation of the packed vector.
		/// </summary>
		/// <returns>A string representation of the packed vector.</returns>
		public override string ToString()
		{
			return (packedValue / 255.0f).ToString();
		}

		/// <summary>
		/// Gets a hash code of the packed vector.
		/// </summary>
		/// <returns>The hash code for the packed vector.</returns>
		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public static bool operator ==(Alpha8 lhs, Alpha8 rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

		public static bool operator !=(Alpha8 lhs, Alpha8 rhs)
		{
			return lhs.packedValue != rhs.packedValue;
		}

		#endregion

		#region Private Static Pack Method

		private static byte Pack(float alpha)
		{
			return (byte) (
				MathHelper.Clamp(alpha, 0, 1) * 255.0f
			);
		}

		#endregion
	}
}
