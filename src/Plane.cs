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
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework.Design;
#endregion

namespace Microsoft.Xna.Framework
{
	[Serializable]
	[TypeConverter(typeof(PlaneConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Plane : IEquatable<Plane>
	{
		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					Normal.DebugDisplayString, " ",
					D.ToString()
				);
			}
		}

		#endregion

		#region Public Fields

		public Vector3 Normal;
		public float D;

		#endregion

		#region Public Constructors

		public Plane(Vector4 value)
			: this(new Vector3(value.X, value.Y, value.Z), value.W)
		{
		}

		public Plane(Vector3 normal, float d)
		{
			Normal = normal;
			D = d;
		}

		public Plane(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 ab = b - a;
			Vector3 ac = c - a;

			Vector3 cross = Vector3.Cross(ab, ac);
			Normal = Vector3.Normalize(cross);
			D = -(Vector3.Dot(Normal, a));
		}

		public Plane(float a, float b, float c, float d)
			: this(new Vector3(a, b, c), d)
		{

		}

		#endregion

		#region Public Methods

		public float Dot(Vector4 value)
		{
			return (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z) +
				(this.D * value.W)
			);
		}

		public void Dot(ref Vector4 value, out float result)
		{
			result = (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z) +
				(this.D * value.W)
			);
		}

		public float DotCoordinate(Vector3 value)
		{
			return (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z) +
				this.D
			);
		}

		public void DotCoordinate(ref Vector3 value, out float result)
		{
			result = (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z) +
				this.D
			);
		}

		public float DotNormal(Vector3 value)
		{
			return (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z)
			);
		}

		public void DotNormal(ref Vector3 value, out float result)
		{
			result = (
				(this.Normal.X * value.X) +
				(this.Normal.Y * value.Y) +
				(this.Normal.Z * value.Z)
			);
		}

		public void Normalize()
		{
			float factor;
			Vector3 normal = Normal;
			Normal = Vector3.Normalize(Normal);
			factor = (float) Math.Sqrt(
				Normal.X * Normal.X +
				Normal.Y * Normal.Y +
				Normal.Z * Normal.Z
			) / (float) Math.Sqrt(
				normal.X * normal.X +
				normal.Y * normal.Y +
				normal.Z * normal.Z
			);
			D = D * factor;
		}

		public PlaneIntersectionType Intersects(BoundingBox box)
		{
			return box.Intersects(this);
		}

		public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
		{
			box.Intersects(ref this, out result);
		}

		public PlaneIntersectionType Intersects(BoundingSphere sphere)
		{
			return sphere.Intersects(this);
		}

		public void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
		{
			sphere.Intersects(ref this, out result);
		}

		public PlaneIntersectionType Intersects(BoundingFrustum frustum)
		{
			return frustum.Intersects(this);
		}

		#endregion

		#region Internal Methods

		internal PlaneIntersectionType Intersects(ref Vector3 point)
		{
			float distance;
			DotCoordinate(ref point, out distance);
			if (distance > 0)
			{
				return PlaneIntersectionType.Front;
			}
			if (distance < 0)
			{
				return PlaneIntersectionType.Back;
			}
			return PlaneIntersectionType.Intersecting;
		}

		#endregion

		#region Public Static Methods

		public static Plane Normalize(Plane value)
		{
			Plane ret;
			Normalize(ref value, out ret);
			return ret;
		}

		public static void Normalize(ref Plane value, out Plane result)
		{
			float factor;
			result.Normal = Vector3.Normalize(value.Normal);
			factor = (float) Math.Sqrt(
				result.Normal.X * result.Normal.X +
				result.Normal.Y * result.Normal.Y +
				result.Normal.Z * result.Normal.Z
			) / (float) Math.Sqrt(
				value.Normal.X * value.Normal.X +
				value.Normal.Y * value.Normal.Y +
				value.Normal.Z * value.Normal.Z
			);
			result.D = value.D * factor;
		}

		/// <summary>
		/// Transforms a normalized plane by a matrix.
		/// </summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="matrix">The transformation matrix.</param>
		/// <returns>The transformed plane.</returns>
		public static Plane Transform(Plane plane, Matrix matrix)
		{
			Plane result;
			Transform(ref plane, ref matrix, out result);
			return result;
		}

		/// <summary>
		/// Transforms a normalized plane by a matrix.
		/// </summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="matrix">The transformation matrix.</param>
		/// <param name="result">The transformed plane.</param>
		public static void Transform(
			ref Plane plane,
			ref Matrix matrix,
			out Plane result
		) {
			/* See "Transforming Normals" in
			 * http://www.glprogramming.com/red/appendixf.html
			 * for an explanation of how this works.
			 */
			Matrix transformedMatrix;
			Matrix.Invert(ref matrix, out transformedMatrix);
			Matrix.Transpose(
				ref transformedMatrix,
				out transformedMatrix
			);
			Vector4 vector = new Vector4(plane.Normal, plane.D);
			Vector4 transformedVector;
			Vector4.Transform(
				ref vector,
				ref transformedMatrix,
				out transformedVector
			);
			result = new Plane(transformedVector);
		}

		/// <summary>
		/// Transforms a normalized plane by a quaternion rotation.
		/// </summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="rotation">The quaternion rotation.</param>
		/// <returns>The transformed plane.</returns>
		public static Plane Transform(Plane plane, Quaternion rotation)
		{
			Plane result;
			Transform(ref plane, ref rotation, out result);
			return result;
		}

		/// <summary>
		/// Transforms a normalized plane by a quaternion rotation.
		/// </summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="rotation">The quaternion rotation.</param>
		/// <param name="result">The transformed plane.</param>
		public static void Transform(
			ref Plane plane,
			ref Quaternion rotation,
			out Plane result
		) {
			Vector3.Transform(
				ref plane.Normal,
				ref rotation,
				out result.Normal
			);
			result.D = plane.D;
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(Plane plane1, Plane plane2)
		{
			return !plane1.Equals(plane2);
		}

		public static bool operator ==(Plane plane1, Plane plane2)
		{
			return plane1.Equals(plane2);
		}

		public override bool Equals(object other)
		{
			return (other is Plane) ? this.Equals((Plane) other) : false;
		}

		public bool Equals(Plane other)
		{
			return ((Normal == other.Normal) && (MathHelper.WithinEpsilon(D, other.D)));
		}

		public override int GetHashCode()
		{
			return Normal.GetHashCode() ^ D.GetHashCode();
		}

		public override string ToString()
		{
			return (
				"{Normal:" + Normal.ToString() +
				" D:" + D.ToString() +
				"}"
			);
		}

		#endregion
	}
}
