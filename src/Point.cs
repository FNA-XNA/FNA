#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.ComponentModel;

using Microsoft.Xna.Framework.Design;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Describes a 2D-point.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(PointConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Point : IEquatable<Point>
	{
		#region Public Static Properties

		/// <summary>
		/// Returns a <see cref="Point"/> with coordinates 0, 0.
		/// </summary>
		public static Point Zero
		{
			get
			{
				return zeroPoint;
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					X.ToString(), " ",
					Y.ToString()
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of this <see cref="Point"/>.
		/// </summary>
		public int X;

		/// <summary>
		/// The y coordinate of this <see cref="Point"/>.
		/// </summary>
		public int Y;

		#endregion

		#region Private Static Variables

		private static readonly Point zeroPoint = new Point();

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs a point with X and Y from two values.
		/// </summary>
		/// <param name="x">The x coordinate in 2d-space.</param>
		/// <param name="y">The y coordinate in 2d-space.</param>
		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Point"/>.
		/// </summary>
		/// <param name="other">The <see cref="Point"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Point other)
		{
			return ((X == other.X) && (Y == other.Y));
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return (obj is Point) && Equals((Point) obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Point"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Point"/>.</returns>
		public override int GetHashCode()
		{
			return X ^ Y;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Point"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="Point"/>.</returns>
		public override string ToString()
		{
			return (
				"{X:" + X.ToString() +
				" Y:" + Y.ToString() +
				"}"
			);
		}

		#endregion

		#region Public Static Operators

		/// <summary>
		/// Adds two points.
		/// </summary>
		/// <param name="value1">Source <see cref="Point"/> on the left of the add sign.</param>
		/// <param name="value2">Source <see cref="Point"/> on the right of the add sign.</param>
		/// <returns>Sum of the points.</returns>
		public static Point operator +(Point value1, Point value2)
		{
			return new Point(value1.X + value2.X, value1.Y + value2.Y);
		}

		/// <summary>
		/// Subtracts a <see cref="Point"/> from a <see cref="Point"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Point"/> on the left of the sub sign.</param>
		/// <param name="value2">Source <see cref="Point"/> on the right of the sub sign.</param>
		/// <returns>Result of the subtraction.</returns>
		public static Point operator -(Point value1, Point value2)
		{
			return new Point(value1.X - value2.X, value1.Y - value2.Y);
		}

		/// <summary>
		/// Multiplies the components of two points by each other.
		/// </summary>
		/// <param name="value1">Source <see cref="Point"/> on the left of the mul sign.</param>
		/// <param name="value2">Source <see cref="Point"/> on the right of the mul sign.</param>
		/// <returns>Result of the multiplication.</returns>
		public static Point operator *(Point value1, Point value2)
		{
			return new Point(value1.X * value2.X, value1.Y * value2.Y);
		}

		/// <summary>
		/// Divides the components of a <see cref="Point"/> by the components of another <see cref="Point"/>.
		/// </summary>
		/// <param name="source">Source <see cref="Point"/> on the left of the div sign.</param>
		/// <param name="divisor">Divisor <see cref="Point"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the points.</returns>
		public static Point operator /(Point value1, Point value2)
		{
			return new Point(value1.X / value2.X, value1.Y / value2.Y);
		}

		/// <summary>
		/// Compares whether two <see cref="Point"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="Point"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="Point"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Point a, Point b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Compares whether two <see cref="Point"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="Point"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="Point"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Point a, Point b)
		{
			return !a.Equals(b);
		}

		#endregion
	}
}
