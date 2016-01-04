#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Key point on the <see cref="Curve"/>.
	/// </summary>
	[Serializable]
	public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous.
		/// </summary>
		public CurveContinuity Continuity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a position of the key on the curve.
		/// </summary>
		public float Position
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a tangent when approaching this point from the previous point on the curve.
		/// </summary>
		public float TangentIn
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a tangent when leaving this point to the next point on the curve.
		/// </summary>
		public float TangentOut
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value of this point.
		/// </summary>
		public float Value
		{
			get;
			set;
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a new instance of <see cref="CurveKey"/> class.
		/// </summary>
		/// <param name="position">Position on the curve.</param>
		/// <param name="value">Value of the control point.</param>
		public CurveKey(
			float position,
			float value
		) : this(
			position,
			value,
			0,
			0,
			CurveContinuity.Smooth
		) {
		}

		/// <summary>
		/// Creates a new instance of <see cref="CurveKey"/> class.
		/// </summary>
		/// <param name="position">Position on the curve.</param>
		/// <param name="value">Value of the control point.</param>
		/// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
		/// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
		public CurveKey(
			float position,
			float value,
			float tangentIn,
			float tangentOut
		) : this(
			position,
			value,
			tangentIn,
			tangentOut,
			CurveContinuity.Smooth
		) {
		}

		/// <summary>
		/// Creates a new instance of <see cref="CurveKey"/> class.
		/// </summary>
		/// <param name="position">Position on the curve.</param>
		/// <param name="value">Value of the control point.</param>
		/// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
		/// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
		/// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
		public CurveKey(
			float position,
			float value,
			float tangentIn,
			float tangentOut,
			CurveContinuity continuity
		) {
			Position = position;
			Value = value;
			TangentIn = tangentIn;
			TangentOut = tangentOut;
			Continuity = continuity;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a copy of this key.
		/// </summary>
		/// <returns>A copy of this key.</returns>
		public CurveKey Clone()
		{
			return new CurveKey(
				Position,
				Value,
				TangentIn,
				TangentOut,
				Continuity
			);
		}

		public int CompareTo(CurveKey other)
		{
			return Position.CompareTo(other.Position);
		}

		public bool Equals(CurveKey other)
		{
			return (this == other);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares whether two <see cref="CurveKey"/> instances are not equal.
		/// </summary>
		/// <param name="value1"><see cref="CurveKey"/> instance on the left of the not equal sign.</param>
		/// <param name="value2"><see cref="CurveKey"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(CurveKey a, CurveKey b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Compares whether two <see cref="CurveKey"/> instances are equal.
		/// </summary>
		/// <param name="value1"><see cref="CurveKey"/> instance on the left of the equal sign.</param>
		/// <param name="value2"><see cref="CurveKey"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(CurveKey a, CurveKey b)
		{
			if (object.Equals(a, null))
			{
				return object.Equals(b, null);
			}

			if (object.Equals(b, null))
			{
				return object.Equals(a, null);
			}

			return (	(a.Position == b.Position) &&
					(a.Value == b.Value) &&
					(a.TangentIn == b.TangentIn) &&
					(a.TangentOut == b.TangentOut) &&
					(a.Continuity == b.Continuity)	);
		}

		public override bool Equals(object obj)
		{
			return (obj as CurveKey) == this;
		}

		public override int GetHashCode()
		{
			return (
				Position.GetHashCode() ^
				Value.GetHashCode() ^
				TangentIn.GetHashCode() ^
				TangentOut.GetHashCode() ^
				Continuity.GetHashCode()
			);
		}

		#endregion
	}
}
