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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework.Design;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Describes a sphere in 3D-space for bounding operations.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(BoundingSphereConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct BoundingSphere : IEquatable<BoundingSphere>
	{
		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					"Center( ", Center.DebugDisplayString, " ) \r\n",
					"Radius( ", Radius.ToString(), " ) "
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The sphere center.
		/// </summary>
		public Vector3 Center;

		/// <summary>
		/// The sphere radius.
		/// </summary>
		public float Radius;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs a bounding sphere with the specified center and radius.
		/// </summary>
		/// <param name="center">The sphere center.</param>
		/// <param name="radius">The sphere radius.</param>
		public BoundingSphere(Vector3 center, float radius)
		{
			this.Center = center;
			this.Radius = radius;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a new <see cref="BoundingSphere"/> that contains a transformation of translation and scale from this sphere by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <returns>Transformed <see cref="BoundingSphere"/>.</returns>
		public BoundingSphere Transform(Matrix matrix)
		{
			BoundingSphere sphere = new BoundingSphere();
			sphere.Center = Vector3.Transform(this.Center, matrix);
			sphere.Radius = this.Radius *
				(
					(float) Math.Sqrt((double) Math.Max(
						((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13),
						Math.Max(
							((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23),
							((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33))
						)
					)
				);
			return sphere;
		}

		/// <summary>
		/// Creates a new <see cref="BoundingSphere"/> that contains a transformation of translation and scale from this sphere by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="result">Transformed <see cref="BoundingSphere"/> as an output parameter.</param>
		public void Transform(ref Matrix matrix, out BoundingSphere result)
		{
			result.Center = Vector3.Transform(this.Center, matrix);
			result.Radius = this.Radius *
				(
					(float) Math.Sqrt((double) Math.Max(
						((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13),
						Math.Max(
							((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23),
							((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33))
						)
					)
				);
		}

		/// <summary>
		/// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="box">The box for testing.</param>
		/// <param name="result">The containment type as an output parameter.</param>
		public void Contains(ref BoundingBox box, out ContainmentType result)
		{
			result = this.Contains(box);
		}

		/// <summary>
		/// Test if a sphere is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="sphere">The other sphere for testing.</param>
		/// <param name="result">The containment type as an output parameter.</param>
		public void Contains(ref BoundingSphere sphere, out ContainmentType result)
		{
			result = Contains(sphere);
		}

		/// <summary>
		/// Test if a point is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="point">The vector in 3D-space for testing.</param>
		/// <param name="result">The containment type as an output parameter.</param>
		public void Contains(ref Vector3 point, out ContainmentType result)
		{
			result = Contains(point);
		}

		/// <summary>
		/// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="box">The box for testing.</param>
		/// <returns>The containment type.</returns>
		public ContainmentType Contains(BoundingBox box)
		{
			// Check if all corners are in sphere.
			bool inside = true;
			foreach (Vector3 corner in box.GetCorners())
			{
				if (this.Contains(corner) == ContainmentType.Disjoint)
				{
					inside = false;
					break;
				}
			}

			if (inside)
			{
				return ContainmentType.Contains;
			}

			// Check if the distance from sphere center to cube face is less than radius.
			double dmin = 0;

			if (Center.X < box.Min.X)
			{
				dmin += (Center.X - box.Min.X) * (Center.X - box.Min.X);
			}
			else if (Center.X > box.Max.X)
			{
				dmin += (Center.X - box.Max.X) * (Center.X - box.Max.X);
			}

			if (Center.Y < box.Min.Y)
			{
				dmin += (Center.Y - box.Min.Y) * (Center.Y - box.Min.Y);
			}
			else if (Center.Y > box.Max.Y)
			{
				dmin += (Center.Y - box.Max.Y) * (Center.Y - box.Max.Y);
			}

			if (Center.Z < box.Min.Z)
			{
				dmin += (Center.Z - box.Min.Z) * (Center.Z - box.Min.Z);
			}
			else if (Center.Z > box.Max.Z)
			{
				dmin += (Center.Z - box.Max.Z) * (Center.Z - box.Max.Z);
			}

			if (dmin <= Radius * Radius)
			{
				return ContainmentType.Intersects;
			}

			// Else disjoint
			return ContainmentType.Disjoint;
		}

		/// <summary>
		/// Test if a frustum is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="box">The box for testing.</param>
		/// <param name="result">The containment type as an output parameter.</param>
		public ContainmentType Contains(BoundingFrustum frustum)
		{
			// Check if all corners are in sphere.
			bool inside = true;

			Vector3[] corners = frustum.GetCorners();
			foreach (Vector3 corner in corners)
			{
				if (this.Contains(corner) == ContainmentType.Disjoint)
				{
					inside = false;
					break;
				}
			}
			if (inside)
			{
				return ContainmentType.Contains;
			}

			// Check if the distance from sphere center to frustrum face is less than radius.
			double dmin = 0;
			// TODO : calcul dmin

			if (dmin <= Radius * Radius)
			{
				return ContainmentType.Intersects;
			}

			// Else disjoint
			return ContainmentType.Disjoint;
		}

		/// <summary>
		/// Test if a sphere is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="sphere">The other sphere for testing.</param>
		/// <returns>The containment type.</returns>
		public ContainmentType Contains(BoundingSphere sphere)
		{
			float sqDistance;
			Vector3.DistanceSquared(ref sphere.Center, ref Center, out sqDistance);

			if (sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius))
			{
				return ContainmentType.Disjoint;
			}
			else if (sqDistance <= (Radius * sphere.Radius) * (Radius - sphere.Radius))
			{
				return ContainmentType.Contains;
			}
			return ContainmentType.Intersects;
		}

		/// <summary>
		/// Test if a point is fully inside, outside, or just intersecting the sphere.
		/// </summary>
		/// <param name="point">The vector in 3D-space for testing.</param>
		/// <returns>The containment type.</returns>
		public ContainmentType Contains(Vector3 point)
		{
			float sqRadius = Radius * Radius;
			float sqDistance;
			Vector3.DistanceSquared(ref point, ref Center, out sqDistance);

			if (sqDistance > sqRadius)
			{
				return ContainmentType.Disjoint;
			}
			else if (sqDistance < sqRadius)
			{
				return ContainmentType.Contains;
			}
			return ContainmentType.Intersects;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="BoundingSphere"/>.
		/// </summary>
		/// <param name="other">The <see cref="BoundingSphere"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(BoundingSphere other)
		{
			return (	this.Center == other.Center &&
					MathHelper.WithinEpsilon(this.Radius, other.Radius)	);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain a specified <see cref="BoundingBox"/>.
		/// </summary>
		/// <param name="box">The box to create the sphere from.</param>
		/// <returns>The new <see cref="BoundingSphere"/>.</returns>
		public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
		{
			BoundingSphere result;
			CreateFromBoundingBox(ref box, out result);
			return result;
		}

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain a specified <see cref="BoundingBox"/>.
		/// </summary>
		/// <param name="box">The box to create the sphere from.</param>
		/// <param name="result">The new <see cref="BoundingSphere"/> as an output parameter.</param>
		public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
		{
			// Find the center of the box.
			Vector3 center = new Vector3(
				(box.Min.X + box.Max.X) / 2.0f,
				(box.Min.Y + box.Max.Y) / 2.0f,
				(box.Min.Z + box.Max.Z) / 2.0f
			);

			// Find the distance between the center and one of the corners of the box.
			float radius = Vector3.Distance(center, box.Max);

			result = new BoundingSphere(center, radius);
		}

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain a specified <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="frustum">The frustum to create the sphere from.</param>
		/// <returns>The new <see cref="BoundingSphere"/>.</returns>
		public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
		{
			return CreateFromPoints(frustum.GetCorners());
		}

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain a specified list of points in 3D-space.
		/// </summary>
		/// <param name="points">List of point to create the sphere from.</param>
		/// <returns>The new <see cref="BoundingSphere"/>.</returns>
		public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}

			// From "Real-Time Collision Detection" (Page 89)

			Vector3 minx = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 maxx = -minx;
			Vector3 miny = minx;
			Vector3 maxy = -minx;
			Vector3 minz = minx;
			Vector3 maxz = -minx;

			// Find the most extreme points along the principle axis.
			int numPoints = 0;
			foreach (Vector3 pt in points)
			{
				numPoints += 1;

				if (pt.X < minx.X)
				{
					minx = pt;
				}
				if (pt.X > maxx.X)
				{
					maxx = pt;
				}
				if (pt.Y < miny.Y)
				{
					miny = pt;
				}
				if (pt.Y > maxy.Y)
				{
					maxy = pt;
				}
				if (pt.Z < minz.Z)
				{
					minz = pt;
				}
				if (pt.Z > maxz.Z)
				{
					maxz = pt;
				}
			}

			if (numPoints == 0)
			{
				throw new ArgumentException(
					"You should have at least one point in points."
				);
			}

			float sqDistX = Vector3.DistanceSquared(maxx, minx);
			float sqDistY = Vector3.DistanceSquared(maxy, miny);
			float sqDistZ = Vector3.DistanceSquared(maxz, minz);

			// Pick the pair of most distant points.
			Vector3 min = minx;
			Vector3 max = maxx;
			if (sqDistY > sqDistX && sqDistY > sqDistZ)
			{
				max = maxy;
				min = miny;
			}
			if (sqDistZ > sqDistX && sqDistZ > sqDistY)
			{
				max = maxz;
				min = minz;
			}
			
			Vector3 center = (min + max) * 0.5f;
			float radius = Vector3.Distance(max, center);

			// Test every point and expand the sphere.
			// The current bounding sphere is just a good approximation and may not enclose all points.
			// From: Mathematics for 3D Game Programming and Computer Graphics, Eric Lengyel, Third Edition.
			// Page 218
			float sqRadius = radius * radius;
			foreach (Vector3 pt in points)
			{
				Vector3 diff = (pt - center);
				float sqDist = diff.LengthSquared();
				if (sqDist > sqRadius)
				{
					float distance = (float) Math.Sqrt(sqDist); // equal to diff.Length();
					Vector3 direction = diff / distance;
					Vector3 G = center - radius * direction;
					center = (G + pt) / 2;
					radius = Vector3.Distance(pt, center);
					sqRadius = radius * radius;
				}
			}

			return new BoundingSphere(center, radius);
		}

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain two spheres.
		/// </summary>
		/// <param name="original">First sphere.</param>
		/// <param name="additional">Second sphere.</param>
		/// <returns>The new <see cref="BoundingSphere"/>.</returns>
		public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
		{
			BoundingSphere result;
			CreateMerged(ref original, ref additional, out result);
			return result;
		}

		/// <summary>
		/// Creates the smallest <see cref="BoundingSphere"/> that can contain two spheres.
		/// </summary>
		/// <param name="original">First sphere.</param>
		/// <param name="additional">Second sphere.</param>
		/// <param name="result">The new <see cref="BoundingSphere"/> as an output parameter.</param>
		public static void CreateMerged(
			ref BoundingSphere original,
			ref BoundingSphere additional,
			out BoundingSphere result
		) {
			Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
			float distance = ocenterToaCenter.Length();

			// Intersect
			if (distance <= original.Radius + additional.Radius)
			{
				// Original contains additional.
				if (distance <= original.Radius - additional.Radius)
				{
					result = original;
					return;
				}

				// Additional contains original.
				if (distance <= additional.Radius - original.Radius)
				{
					result = additional;
					return;
				}
			}

			// Else find center of new sphere and radius
			float leftRadius = Math.Max(original.Radius - distance, additional.Radius);
			float Rightradius = Math.Max(original.Radius + distance, additional.Radius);

			// oCenterToResultCenter
			ocenterToaCenter = ocenterToaCenter +
				(
					((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length()))
					* ocenterToaCenter
				);

			result = new BoundingSphere();
			result.Center = original.Center + ocenterToaCenter;
			result.Radius = (leftRadius + Rightradius) / 2;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingBox"/> intersects with this sphere.
		/// </summary>
		/// <param name="box">The box for testing.</param>
		/// <returns><c>true</c> if <see cref="BoundingBox"/> intersects with this sphere; <c>false</c> otherwise.</returns>
		public bool Intersects(BoundingBox box)
		{
			return box.Intersects(this);
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingBox"/> intersects with this sphere.
		/// </summary>
		/// <param name="box">The box for testing.</param>
		/// <param name="result"><c>true</c> if <see cref="BoundingBox"/> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref BoundingBox box, out bool result)
		{
			box.Intersects(ref this, out result);
		}

		public bool Intersects(BoundingFrustum frustum)
		{
			return frustum.Intersects(this);
		}

		/// <summary>
		/// Gets whether or not the other <see cref="BoundingSphere"/> intersects with this sphere.
		/// </summary>
		/// <param name="sphere">The other sphere for testing.</param>
		/// <returns><c>true</c> if other <see cref="BoundingSphere"/> intersects with this sphere; <c>false</c> otherwise.</returns>
		public bool Intersects(BoundingSphere sphere)
		{
			bool result;
			Intersects(ref sphere, out result);
			return result;
		}

		/// <summary>
		/// Gets whether or not the other <see cref="BoundingSphere"/> intersects with this sphere.
		/// </summary>
		/// <param name="sphere">The other sphere for testing.</param>
		/// <param name="result"><c>true</c> if other <see cref="BoundingSphere"/> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref BoundingSphere sphere, out bool result)
		{
			float sqDistance;
			Vector3.DistanceSquared(ref sphere.Center, ref Center, out sqDistance);
			result = !(sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius));
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Ray"/> intersects with this sphere.
		/// </summary>
		/// <param name="ray">The ray for testing.</param>
		/// <returns>Distance of ray intersection or <c>null</c> if there is no intersection.</returns>
		public float? Intersects(Ray ray)
		{
			return ray.Intersects(this);
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Ray"/> intersects with this sphere.
		/// </summary>
		/// <param name="ray">The ray for testing.</param>
		/// <param name="result">Distance of ray intersection or <c>null</c> if there is no intersection as an output parameter.</param>
		public void Intersects(ref Ray ray, out float? result)
		{
			ray.Intersects(ref this, out result);
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Plane"/> intersects with this sphere.
		/// </summary>
		/// <param name="plane">The plane for testing.</param>
		/// <returns>Type of intersection.</returns>
		public PlaneIntersectionType Intersects(Plane plane)
		{
			PlaneIntersectionType result = default(PlaneIntersectionType);
			// TODO: We might want to inline this for performance reasons.
			this.Intersects(ref plane, out result);
			return result;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="Plane"/> intersects with this sphere.
		/// </summary>
		/// <param name="plane">The plane for testing.</param>
		/// <param name="result">Type of intersection as an output parameter.</param>
		public void Intersects(ref Plane plane, out PlaneIntersectionType result)
		{
			float distance = default(float);
			// TODO: We might want to inline this for performance reasons.
			Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
			distance += plane.D;
			if (distance > this.Radius)
			{
				result = PlaneIntersectionType.Front;
			}
			else if (distance < -this.Radius)
			{
				result = PlaneIntersectionType.Back;
			}
			else
			{
				result = PlaneIntersectionType.Intersecting;
			}
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is BoundingSphere)
			{
				return this.Equals((BoundingSphere)obj);
			}

			return false;
		}

		/// <summary>
		/// Gets the hash code of this <see cref="BoundingSphere"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="BoundingSphere"/>.</returns>
		public override int GetHashCode()
		{
			return this.Center.GetHashCode() + this.Radius.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="BoundingSphere"/> in the format:
		/// {Center:[<see cref="Center"/>] Radius:[<see cref="Radius"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="BoundingSphere"/>.</returns>
		public override string ToString()
		{
			return (
				"{Center:" + Center.ToString() +
				" Radius:" + Radius.ToString() +
				"}"
			);
		}

		/// <summary>
		/// Compares whether two <see cref="BoundingSphere"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="BoundingSphere"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="BoundingSphere"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(BoundingSphere a, BoundingSphere b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Compares whether two <see cref="BoundingSphere"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="BoundingSphere"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="BoundingSphere"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(BoundingSphere a, BoundingSphere b)
		{
			return !a.Equals(b);
		}

		#endregion
	}
}
