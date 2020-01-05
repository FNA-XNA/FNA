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
	/// Packed vector type containing four 16-bit signed integer values.
	/// </summary>
	public struct Short4 : IPackedVector<ulong>, IEquatable<Short4>
	{
		#region Public Properties

		/// <summary>
		/// Directly gets or sets the packed representation of the value.
		/// </summary>
		/// <value>The packed representation of the value.</value>
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

		/// <summary>
		/// Initializes a new instance of the Short4 class.
		/// </summary>
		/// <param name="vector">
		/// A vector containing the initial values for the components of the Short4 structure.
		/// </param>
		public Short4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		/// <summary>
		/// Initializes a new instance of the Short4 class.
		/// </summary>
		/// <param name="x">Initial value for the x component.</param>
		/// <param name="y">Initial value for the y component.</param>
		/// <param name="z">Initial value for the z component.</param>
		/// <param name="w">Initial value for the w component.</param>
		public Short4(float x, float y, float z, float w)
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
				(short) (packedValue & 0xFFFF),
				(short) ((packedValue >> 16) & 0xFFFF),
				(short) ((packedValue >> 32) & 0xFFFF),
				(short) (packedValue >> 48)
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
		public static bool operator !=(Short4 a, Short4 b)
		{
			return a.PackedValue != b.PackedValue;
		}

		/// <summary>
		/// Compares the current instance of a class to another instance to determine
		/// whether they are the same.
		/// </summary>
		/// <param name="a">The object to the left of the equality operator.</param>
		/// <param name="b">The object to the right of the equality operator.</param>
		/// <returns>True if the objects are the same; false otherwise.</returns>
		public static bool operator ==(Short4 a, Short4 b)
		{
			return a.PackedValue == b.PackedValue;
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
			return (obj is Short4) && Equals((Short4) obj);
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to a
		/// specified object.
		/// </summary>
		/// <param name="other">The object with which to make the comparison.</param>
		/// <returns>
		/// True if the current instance is equal to the specified object; false otherwise.
		/// </returns>
		public bool Equals(Short4 other)
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
		/// Packs a vector into a ulong.
		/// </summary>
		/// <param name="vector">The vector containing the values to pack.</param>
		/// <returns>The ulong containing the packed values.</returns>
		static ulong Pack(float x, float y, float z, float w)
		{
			return (ulong) (
				((long) Math.Round(MathHelper.Clamp(x, -32768, 32767)) & 0xFFFF ) |
				(((long) Math.Round(MathHelper.Clamp(y, -32768, 32767)) << 16) & 0xFFFF0000) |
				(((long) Math.Round(MathHelper.Clamp(z, -32768, 32767)) << 32) & 0xFFFF00000000) |
				((long) Math.Round(MathHelper.Clamp(w, -32768, 32767)) << 48)
			);
		}

		#endregion
	}
}
