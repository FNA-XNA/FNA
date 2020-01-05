#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
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
	/// Packed vector type containing four 8-bit unsigned integer values, ranging from 0 to 255.
	/// </summary>
	public struct Byte4 : IPackedVector<uint>, IEquatable<Byte4>, IPackedVector
	{
		#region Public Properties

		/// <summary>
		/// Directly gets or sets the packed representation of the value.
		/// </summary>
		/// <value>The packed representation of the value.</value>
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

		/// <summary>
		/// Initializes a new instance of the Byte4 class.
		/// </summary>
		/// <param name="vector">
		/// A vector containing the initial values for the components of the Byte4 structure.
		/// </param>
		public Byte4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		/// <summary>
		/// Initializes a new instance of the Byte4 class.
		/// </summary>
		/// <param name="x">Initial value for the x component.</param>
		/// <param name="y">Initial value for the y component.</param>
		/// <param name="z">Initial value for the z component.</param>
		/// <param name="w">Initial value for the w component.</param>
		public Byte4(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Expands the packed representation into a Vector4.
		/// </summary>
		/// <returns>The expanded vector.</returns>
		public Vector4 ToVector4()
		{
			return new Vector4(
				(packedValue & 0xFF),
				((packedValue >> 8) & 0xFF),
				((packedValue >> 16) & 0xFF),
				(packedValue >> 24)
			);
		}

		#endregion

		#region IPackedVector Methods

		/// <summary>
		/// Sets the packed representation from a Vector4.
		/// </summary>
		/// <param name="vector">The vector to create the packed representation from.</param>
		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares the current instance of a class to another instance to determine
		/// whether they are different.
		/// </summary>
		/// <param name="a">The object to the left of the equality operator.</param>
		/// <param name="b">The object to the right of the equality operator.</param>
		/// <returns>True if the objects are different; false otherwise.</returns>
		public static bool operator !=(Byte4 a, Byte4 b)
		{
			return a.packedValue != b.packedValue;
		}

		/// <summary>
		/// Compares the current instance of a class to another instance to determine
		/// whether they are the same.
		/// </summary>
		/// <param name="a">The object to the left of the equality operator.</param>
		/// <param name="b">The object to the right of the equality operator.</param>
		/// <returns>True if the objects are the same; false otherwise.</returns>
		public static bool operator ==(Byte4 a, Byte4 b)
		{
			return a.packedValue == b.packedValue;
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to a
		/// specified object.
		/// </summary>
		/// <param name="obj">The object with which to make the comparison.</param>
		/// <returns>
		/// True if the current instance is equal to the specified object; false otherwise.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is Byte4) && Equals((Byte4) obj);
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to a
		/// specified object.
		/// </summary>
		/// <param name="other">The object with which to make the comparison.</param>
		/// <returns>
		/// True if the current instance is equal to the specified object; false otherwise.
		/// </returns>
		public bool Equals(Byte4 other)
		{
			return this == other;
		}

		/// <summary>
		/// Gets the hash code for the current instance.
		/// </summary>
		/// <returns>Hash code for the instance.</returns>
		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of the current instance.
		/// </summary>
		/// <returns>String that represents the object.</returns>
		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		#endregion

		#region Private Static Pack Method

		/// <summary>
		/// Packs a vector into a uint.
		/// </summary>
		/// <param name="vector">The vector containing the values to pack.</param>
		/// <returns>The ulong containing the packed values.</returns>
		static uint Pack(float x, float y, float z, float w)
		{
			return (uint) (
				((uint) Math.Round(MathHelper.Clamp(x, 0, 255))) |
				((uint) Math.Round(MathHelper.Clamp(y, 0, 255)) << 8) |
				((uint) Math.Round(MathHelper.Clamp(z, 0, 255)) << 16) |
				((uint) Math.Round(MathHelper.Clamp(w, 0, 255)) << 24)
			);
		}

		#endregion
	}
}
