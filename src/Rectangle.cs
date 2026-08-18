#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework.Design;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Describes a 2D-rectangle.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(RectangleConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Rectangle : IEquatable<Rectangle>
	{
		#region Public Properties

		/// <summary>
		/// Returns the x coordinate of the left edge of this <see cref="Rectangle"/>.
		/// </summary>
		public int Left
		{
			get
			{
				return X;
			}
		}

		/// <summary>
		/// Returns the x coordinate of the right edge of this <see cref="Rectangle"/>.
		/// </summary>
		public int Right
		{
			get
			{
				return (X + Width);
			}
		}

		/// <summary>
		/// Returns the y coordinate of the top edge of this <see cref="Rectangle"/>.
		/// </summary>
		public int Top
		{
			get
			{
				return Y;
			}
		}

		/// <summary>
		/// Returns the y coordinate of the bottom edge of this <see cref="Rectangle"/>.
		/// </summary>
		public int Bottom
		{
			get
			{
				return (Y + Height);
			}
		}

		/// <summary>
		/// The top-left coordinates of this <see cref="Rectangle"/>.
		/// </summary>
		public Point Location
		{
			get
			{
				return new Point(X, Y);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// A <see cref="Point"/> located in the center of this <see cref="Rectangle"/>'s bounds.
		/// </summary>
		/// <remarks>
		/// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
		/// the center point will be rounded down.
		/// </remarks>
		public Point Center
		{
			get
			{
				return new Point(
					X + (Width / 2),
					Y + (Height / 2)
				);
			}
		}

		/// <summary>
		/// Whether or not this <see cref="Rectangle"/> has a width and
		/// height of 0, and a position of (0, 0).
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return (	(Width == 0) &&
						(Height == 0) &&
						(X == 0) &&
						(Y == 0)	);
			}
		}

		#endregion

		#region Public Static Properties

		/// <summary>
		/// Returns a <see cref="Rectangle"/> with X=0, Y=0, Width=0, and Height=0.
		/// </summary>
		public static Rectangle Empty
		{
			get
			{
				return emptyRectangle;
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
					Y.ToString(), " ",
					Width.ToString(), " ",
					Height.ToString()
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of the top-left corner of this <see cref="Rectangle"/>.
		/// </summary>
		public int X;

		/// <summary>
		/// The y coordinate of the top-left corner of this <see cref="Rectangle"/>.
		/// </summary>
		public int Y;

		/// <summary>
		/// The width of this <see cref="Rectangle"/>.
		/// </summary>
		public int Width;

		/// <summary>
		/// The height of this <see cref="Rectangle"/>.
		/// </summary>
		public int Height;

		#endregion

		#region Private Static Fields

		private static Rectangle emptyRectangle = new Rectangle();

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a <see cref="Rectangle"/> with the specified
		/// position, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate of the top-left corner of the created <see cref="Rectangle"/>.</param>
		/// <param name="y">The y coordinate of the top-left corner of the created <see cref="Rectangle"/>.</param>
		/// <param name="width">The width of the created <see cref="Rectangle"/>.</param>
		/// <param name="height">The height of the created <see cref="Rectangle"/>.</param>
		public Rectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
		public bool Contains(int x, int y)
		{
			return (	(this.X <= x) &&
					(x < (this.X + this.Width)) &&
					(this.Y <= y) &&
					(y < (this.Y + this.Height))	);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="Rectangle"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
		public bool Contains(Point value)
		{
			return (	(this.X <= value.X) &&
					(value.X < (this.X + this.Width)) &&
					(this.Y <= value.Y) &&
					(value.Y < (this.Y + this.Height))	);
		}

		/// <summary>
		/// Gets whether or not the provided <see cref="Rectangle"/> lies within the bounds of this <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="value">The <see cref="Rectangle"/> to check for inclusion in this <see cref="Rectangle"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Rectangle"/>'s bounds lie entirely inside this <see cref="Rectangle"/>. <c>false</c> otherwise.</returns>
		public bool Contains(Rectangle value)
		{
			return (	(this.X <= value.X) &&
					((value.X + value.Width) <= (this.X + this.Width)) &&
					(this.Y <= value.Y) &&
					((value.Y + value.Height) <= (this.Y + this.Height))	);
		}

		public void Contains(ref Point value, out bool result)
		{
			result = (	(this.X <= value.X) &&
					(value.X < (this.X + this.Width)) &&
					(this.Y <= value.Y) &&
					(value.Y < (this.Y + this.Height))	);
		}

		public void Contains(ref Rectangle value, out bool result)
		{
			result = (	(this.X <= value.X) &&
					((value.X + value.Width) <= (this.X + this.Width)) &&
					(this.Y <= value.Y) &&
					((value.Y + value.Height) <= (this.Y + this.Height))	);
		}

		/// <summary>
		/// Increments this <see cref="Rectangle"/>'s <see cref="Location"/> by the
		/// x and y components of the provided <see cref="Point"/>.
		/// </summary>
		/// <param name="offset">The x and y components to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
		public void Offset(Point offset)
		{
			X += offset.X;
			Y += offset.Y;
		}

		/// <summary>
		/// Increments this <see cref="Rectangle"/>'s <see cref="Location"/> by the
		/// provided x and y coordinates.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="Rectangle"/>'s <see cref="Location"/>.</param>
		public void Offset(int offsetX, int offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}

		public void Inflate(int horizontalValue, int verticalValue)
		{
			X -= horizontalValue;
			Y -= verticalValue;
			Width += horizontalValue * 2;
			Height += verticalValue * 2;
		}

		/// <summary>
		/// Checks whether or not this <see cref="Rectangle"/> is equivalent
		/// to a provided <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="other">The <see cref="Rectangle"/> to test for equality.</param>
		/// <returns>
		/// <c>true</c> if this <see cref="Rectangle"/>'s x coordinate, y coordinate, width, and height
		/// match the values for the provided <see cref="Rectangle"/>. <c>false</c> otherwise.
		/// </returns>
		public bool Equals(Rectangle other)
		{
			return this == other;
		}

		/// <summary>
		/// Checks whether or not this <see cref="Rectangle"/> is equivalent
		/// to a provided object.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to test for equality.</param>
		/// <returns>
		/// <c>true</c> if the provided object is a <see cref="Rectangle"/>, and this
		/// <see cref="Rectangle"/>'s x coordinate, y coordinate, width, and height
		/// match the values for the provided <see cref="Rectangle"/>. <c>false</c> otherwise.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is Rectangle) && this == ((Rectangle) obj);
		}

		public override string ToString()
		{
			return (
				"{X:" + X.ToString() +
				" Y:" + Y.ToString() +
				" Width:" + Width.ToString() +
				" Height:" + Height.ToString() +
				"}"
			);
		}

		public override int GetHashCode()
		{
			return this.X + this.Y + this.Width + this.Height;
		}

		/// <summary>
		/// Gets whether or not the other <see cref="Rectangle"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <returns><c>true</c> if other <see cref="Rectangle"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
		public bool Intersects(Rectangle value)
		{
			return (	value.Left < Right &&
					Left < value.Right &&
					value.Top < Bottom &&
					Top < value.Bottom	);
		}

		/// <summary>
		/// Gets whether or not the other <see cref="Rectangle"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <param name="result"><c>true</c> if other <see cref="Rectangle"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref Rectangle value, out bool result)
		{
			result = (	value.Left < Right &&
					Left < value.Right &&
					value.Top < Bottom &&
					Top < value.Bottom	);
		}

		#endregion

		#region Public Static Methods

		public static bool operator ==(Rectangle a, Rectangle b)
		{
			return (	(a.X == b.X) &&
					(a.Y == b.Y) &&
					(a.Width == b.Width) &&
					(a.Height == b.Height)	);
		}

		public static bool operator !=(Rectangle a, Rectangle b)
		{
			return !(a == b);
		}

		public static Rectangle Intersect(Rectangle value1, Rectangle value2)
		{
			Rectangle rectangle;
			Intersect(ref value1, ref value2, out rectangle);
			return rectangle;
		}

		public static void Intersect(
			ref Rectangle value1,
			ref Rectangle value2,
			out Rectangle result
		) {
			int right1 = value1.X + value1.Width;	// inline value1.Right
			int right2 = value2.X + value2.Width;	// inline value2.Right
			int bottom1 = value1.Y + value1.Height;	// inline value1.Bottom
			int bottom2 = value2.Y + value2.Height;	// inline value2.Bottom
			if (value1.X < right2 && value2.X < right1 && value1.Y < bottom2 && value2.Y < bottom1)
			{
				result.X = Math.Max(value1.X, value1.Y);
				result.Y = Math.Max(value1.Y, value2.Y);
				result.Width = Math.Min(right1, right2) - result.X;
				result.Height = Math.Min(bottom1, bottom2) - result.Y;
			}
			else
			{
				result = new Rectangle();
			}
		}

		public static Rectangle Union(Rectangle value1, Rectangle value2)
		{
			Rectangle result;
			result.X = Math.Min(value1.X, value2.X);
			result.Y = Math.Min(value1.Y, value2.Y);
			result.Width = Math.Max(value1.Right, value2.Right) - result.X;
			result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
			return result;
		}

		public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
		{
			result.X = Math.Min(value1.X, value2.X);
			result.Y = Math.Min(value1.Y, value2.Y);
			result.Width = Math.Max(value1.Right, value2.Right) - result.X;
			result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
		}

		#endregion
	}
}
