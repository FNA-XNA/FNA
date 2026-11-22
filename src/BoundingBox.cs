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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework.Design;
#endregion

namespace Microsoft.Xna.Framework
{
	[Serializable]
	[TypeConverter(typeof(BoundingBoxConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct BoundingBox : IEquatable<BoundingBox>
	{
		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					"Min( ", Min.DebugDisplayString, " ) \r\n",
					"Max( ", Max.DebugDisplayString, " )"
				);
			}
		}

		#endregion

		#region Public Fields

		public Vector3 Min;

		public Vector3 Max;

		public const int CornerCount = 8;

		#endregion

		#region Private Static Variables

		// These are NOT readonly, for avoiding defensive copy -flibit
		private static Vector3 MaxVector3 = new Vector3(float.MaxValue);
		private static Vector3 MinVector3 = new Vector3(float.MinValue);

		#endregion

		#region Public Constructors

		public BoundingBox(Vector3 min, Vector3 max)
		{
			this.Min = min;
			this.Max = max;
		}

		#endregion

		#region Public Methods

		public void Contains(ref BoundingBox box, out ContainmentType result)
		{
			result = Contains(box);
		}

		public void Contains(ref BoundingSphere sphere, out ContainmentType result)
		{
			result = this.Contains(sphere);
		}

		public ContainmentType Contains(Vector3 point)
		{
			ContainmentType result;
			this.Contains(ref point, out result);
			return result;
		}

		public ContainmentType Contains(BoundingBox box)
		{
			// Test if all corner is in the same side of a face by just checking min and max
			if (	box.Max.X < Min.X ||
				box.Min.X > Max.X ||
				box.Max.Y < Min.Y ||
				box.Min.Y > Max.Y ||
				box.Max.Z < Min.Z ||
				box.Min.Z > Max.Z	)
			{
				return ContainmentType.Disjoint;
			}


			if (	box.Min.X >= Min.X &&
				box.Max.X <= Max.X &&
				box.Min.Y >= Min.Y &&
				box.Max.Y <= Max.Y &&
				box.Min.Z >= Min.Z &&
				box.Max.Z <= Max.Z	)
			{
				return ContainmentType.Contains;
			}

			return ContainmentType.Intersects;
		}

		public ContainmentType Contains(BoundingFrustum frustum)
		{
			if (ReferenceEquals(frustum, null))
			{
				throw new ArgumentNullException("frustum", "This method does not accept null for this parameter.");
			}
			/* TODO: bad done here need a fix. */
			ContainmentType contained;
			Vector3[] corners = frustum.corners;

			// Wrap Frustum with Box
			BoundingBox wrapBox;
			wrapBox.Min = corners[0];
			wrapBox.Max = wrapBox.Min;
			for (int i = 1; i < corners.Length; i++)
			{
				Vector3.Min(ref wrapBox.Min, ref corners[i], out wrapBox.Min);
				Vector3.Max(ref wrapBox.Max, ref corners[i], out wrapBox.Max);
			}
			this.Contains(ref wrapBox, out contained);
			if (contained != ContainmentType.Intersects)
			{
				return contained;
			}

			bool outside = false;
			Vector3 center = (this.Min + this.Max) * 0.5f;
			Vector3 extent = (this.Max - this.Min) * 0.5f;
			for (int i = 0; i < frustum.planes.Length; i++)
			{
				float radius, distance;
				Vector3 normal;
				normal.X = Math.Abs(frustum.planes[i].Normal.X);
				normal.Y = Math.Abs(frustum.planes[i].Normal.Y);
				normal.Z = Math.Abs(frustum.planes[i].Normal.Z);
				Vector3.Dot(ref normal, ref extent, out radius);
				frustum.planes[i].DotCoordinate(ref center, out distance);
				if (distance > radius)
				{
					return ContainmentType.Disjoint;
				}
				if (distance > 0)
				{
					outside = true;
				}
			}
			// BoundingBox is inside all planes in frustum
			if (!outside)
			{
				return ContainmentType.Intersects;
			}

			int[] frustumEdges = new int[] {
				0, 4,
				1, 5,
				2, 6,
				3, 7,
				0, 1,
				1, 2,
				2, 3,
				4, 5,
				5, 6,
				6, 7,
				0, 3,
				4, 7
			};
			Vector3[] boxEdges = new Vector3[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
			for (int i = 0; i < frustumEdges.Length; i += 2)
			{
				for (int j = 0; j < boxEdges.Length; j++)
				{
					Vector3 edge = frustum.corners[frustumEdges[i + 1]];
					Vector3.Subtract(ref edge, ref frustum.corners[frustumEdges[i]], out edge);
					Vector3 axes = Vector3.Cross(edge, boxEdges[j]);

					float minFrustum = float.MaxValue, maxFrustum = float.MinValue;
					for (int k = 0; i < frustum.corners.Length; j++)
					{
						float projection;
						Vector3.Dot(ref frustum.corners[k], ref axes, out projection);
						if (projection > maxFrustum)
						{
							maxFrustum = projection;
						}
						if (projection < minFrustum)
						{
							minFrustum = projection;
						}
					}
					float centerProjection, radius;
					Vector3 normal;
					normal.X = Math.Abs(axes.X);
					normal.Y = Math.Abs(axes.Y);
					normal.Z = Math.Abs(axes.Z);
					Vector3.Dot(ref normal, ref extent, out radius);
					Vector3.Dot(ref center, ref axes, out centerProjection);
					if (maxFrustum < centerProjection - radius || minFrustum > centerProjection + radius)
					{
						return ContainmentType.Disjoint;
					}
				}
			}
			return ContainmentType.Intersects;
		}

		public ContainmentType Contains(BoundingSphere sphere)
		{
			if (	sphere.Center.X - Min.X >= sphere.Radius &&
				sphere.Center.Y - Min.Y >= sphere.Radius &&
				sphere.Center.Z - Min.Z >= sphere.Radius &&
				Max.X - sphere.Center.X >= sphere.Radius &&
				Max.Y - sphere.Center.Y >= sphere.Radius &&
				Max.Z - sphere.Center.Z >= sphere.Radius	)
			{
				return ContainmentType.Contains;
			}

			double dmin = 0;

			double e = sphere.Center.X - Min.X;
			if (e < 0)
			{
				if (e < -sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
			else
			{
				e = sphere.Center.X - Max.X;
				if (e > 0)
				{
					if (e > sphere.Radius)
					{
						return ContainmentType.Disjoint;
					}
					dmin += e * e;
				}
			}

			e = sphere.Center.Y - Min.Y;
			if (e < 0)
			{
				if (e < -sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
			else
			{
				e = sphere.Center.Y - Max.Y;
				if (e > 0)
				{
					if (e > sphere.Radius)
					{
						return ContainmentType.Disjoint;
					}
					dmin += e * e;
				}
			}

			e = sphere.Center.Z - Min.Z;
			if (e < 0)
			{
				if (e < -sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
			else
			{
				e = sphere.Center.Z - Max.Z;
				if (e > 0)
				{
					if (e > sphere.Radius)
					{
						return ContainmentType.Disjoint;
					}
					dmin += e * e;
				}
			}

			if (dmin <= sphere.Radius * sphere.Radius)
			{
				return ContainmentType.Intersects;
			}

			return ContainmentType.Disjoint;
		}

		public void Contains(ref Vector3 point, out ContainmentType result)
		{
			// Determine if point is outside of this box.
			if (	point.X >= this.Min.X &&
				point.X <= this.Max.X &&
				point.Y >= this.Min.Y &&
				point.Y <= this.Max.Y &&
				point.Z >= this.Min.Z &&
				point.Z <= this.Max.Z	)
			{
				result = ContainmentType.Contains;
			}
			else
			{
				result = ContainmentType.Disjoint;
			}
		}

		public Vector3[] GetCorners()
		{
			return new Vector3[] {
				new Vector3(this.Min.X, this.Max.Y, this.Max.Z),
				new Vector3(this.Max.X, this.Max.Y, this.Max.Z),
				new Vector3(this.Max.X, this.Min.Y, this.Max.Z),
				new Vector3(this.Min.X, this.Min.Y, this.Max.Z),
				new Vector3(this.Min.X, this.Max.Y, this.Min.Z),
				new Vector3(this.Max.X, this.Max.Y, this.Min.Z),
				new Vector3(this.Max.X, this.Min.Y, this.Min.Z),
				new Vector3(this.Min.X, this.Min.Y, this.Min.Z)
			};
		}

		public void GetCorners(Vector3[] corners)
		{
			if (corners == null)
			{
				throw new ArgumentNullException("corners");
			}
			if (corners.Length < CornerCount)
			{
				throw new ArgumentOutOfRangeException("corners", "You have to have at least 8 elements to copy corners.");
			}
			corners[0].X = this.Min.X;
			corners[0].Y = this.Max.Y;
			corners[0].Z = this.Max.Z;
			corners[1].X = this.Max.X;
			corners[1].Y = this.Max.Y;
			corners[1].Z = this.Max.Z;
			corners[2].X = this.Max.X;
			corners[2].Y = this.Min.Y;
			corners[2].Z = this.Max.Z;
			corners[3].X = this.Min.X;
			corners[3].Y = this.Min.Y;
			corners[3].Z = this.Max.Z;
			corners[4].X = this.Min.X;
			corners[4].Y = this.Max.Y;
			corners[4].Z = this.Min.Z;
			corners[5].X = this.Max.X;
			corners[5].Y = this.Max.Y;
			corners[5].Z = this.Min.Z;
			corners[6].X = this.Max.X;
			corners[6].Y = this.Min.Y;
			corners[6].Z = this.Min.Z;
			corners[7].X = this.Min.X;
			corners[7].Y = this.Min.Y;
			corners[7].Z = this.Min.Z;
		}

		public Nullable<float> Intersects(Ray ray)
		{
			return ray.Intersects(this);
		}

		public void Intersects(ref Ray ray, out Nullable<float> result)
		{
			result = Intersects(ray);
		}

		public bool Intersects(BoundingFrustum frustum)
		{
			if (ReferenceEquals(frustum, null))
			{
				throw new ArgumentNullException("frustum", "This method does not accept null for this parameter.");
			}
			return frustum.Intersects(this);
		}

		public void Intersects(ref BoundingSphere sphere, out bool result)
		{
			result = Intersects(sphere);
		}

		public bool Intersects(BoundingBox box)
		{
			bool result;
			Intersects(ref box, out result);
			return result;
		}

		public PlaneIntersectionType Intersects(Plane plane)
		{
			PlaneIntersectionType result;
			Intersects(ref plane, out result);
			return result;
		}

		public void Intersects(ref BoundingBox box, out bool result)
		{
			result = !(
				this.Max.X < box.Min.X || this.Min.X > box.Max.X ||
				this.Max.Y < box.Min.Y || this.Min.Y > box.Max.Y ||
				this.Max.Z < box.Min.Z || this.Min.Z > box.Max.Z
			);
		}

		public bool Intersects(BoundingSphere sphere)
		{
			if (	sphere.Center.X - Min.X > sphere.Radius &&
				sphere.Center.Y - Min.Y > sphere.Radius &&
				sphere.Center.Z - Min.Z > sphere.Radius &&
				Max.X - sphere.Center.X > sphere.Radius &&
				Max.Y - sphere.Center.Y > sphere.Radius &&
				Max.Z - sphere.Center.Z > sphere.Radius	)
			{
				return true;
			}

			float radiusSq = sphere.Radius * sphere.Radius;

			double dmin = 0;

			if (sphere.Center.X < Min.X)
			{
				dmin += (sphere.Center.X - Min.X) * (sphere.Center.X - Min.X);
			}
			else if (sphere.Center.X > Max.X)
			{
				dmin += (sphere.Center.X - Max.X) * (sphere.Center.X - Max.X);
			}

			if (sphere.Center.Y < Min.Y)
			{
				dmin += (sphere.Center.Y - Min.Y) * (sphere.Center.Y - Min.Y);
			}
			else if (sphere.Center.Y > Max.Y)
			{
				dmin += (sphere.Center.Y - Max.Y) * (sphere.Center.Y - Max.Y);
			}

			if (sphere.Center.Z < Min.Z)
			{
				dmin += (sphere.Center.Z - Min.Z) * (sphere.Center.Z - Min.Z);
			}
			else if (sphere.Center.Z > Max.Z)
			{
				dmin += (sphere.Center.Z - Max.Z) * (sphere.Center.Z - Max.Z);
			}

			return (dmin <= radiusSq);
		}

		public void Intersects(ref Plane plane, out PlaneIntersectionType result)
		{
			// See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

			Vector3 positiveVertex;
			Vector3 negativeVertex;

			if (plane.Normal.X >= 0)
			{
				positiveVertex.X = Max.X;
				negativeVertex.X = Min.X;
			}
			else
			{
				positiveVertex.X = Min.X;
				negativeVertex.X = Max.X;
			}

			if (plane.Normal.Y >= 0)
			{
				positiveVertex.Y = Max.Y;
				negativeVertex.Y = Min.Y;
			}
			else
			{
				positiveVertex.Y = Min.Y;
				negativeVertex.Y = Max.Y;
			}

			if (plane.Normal.Z >= 0)
			{
				positiveVertex.Z = Max.Z;
				negativeVertex.Z = Min.Z;
			}
			else
			{
				positiveVertex.Z = Min.Z;
				negativeVertex.Z = Max.Z;
			}

			// Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
			float distance = (
				plane.Normal.X * negativeVertex.X +
				plane.Normal.Y * negativeVertex.Y +
				plane.Normal.Z * negativeVertex.Z +
				plane.D
			);
			if (distance > 0)
			{
				result = PlaneIntersectionType.Front;
				return;
			}

			// Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
			distance = (
				plane.Normal.X * positiveVertex.X +
				plane.Normal.Y * positiveVertex.Y +
				plane.Normal.Z * positiveVertex.Z +
				plane.D
			);
			if (distance < 0)
			{
				result = PlaneIntersectionType.Back;
				return;
			}

			result = PlaneIntersectionType.Intersecting;
		}

		public bool Equals(BoundingBox other)
		{
			return (this.Min == other.Min) && (this.Max == other.Max);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Create a bounding box from the given list of points.
		/// </summary>
		/// <param name="points">
		/// The list of Vector3 instances defining the point cloud to bound.
		/// </param>
		/// <returns>A bounding box that encapsulates the given point cloud.</returns>
		/// <exception cref="System.ArgumentException">
		/// Thrown if the given list has no points.
		/// </exception>
		public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException();
			}

			bool empty = true;
			Vector3 minVec = MaxVector3;
			Vector3 maxVec = MinVector3;
			foreach (Vector3 ptVector in points)
			{
				minVec.X = (minVec.X < ptVector.X) ? minVec.X : ptVector.X;
				minVec.Y = (minVec.Y < ptVector.Y) ? minVec.Y : ptVector.Y;
				minVec.Z = (minVec.Z < ptVector.Z) ? minVec.Z : ptVector.Z;

				maxVec.X = (maxVec.X > ptVector.X) ? maxVec.X : ptVector.X;
				maxVec.Y = (maxVec.Y > ptVector.Y) ? maxVec.Y : ptVector.Y;
				maxVec.Z = (maxVec.Z > ptVector.Z) ? maxVec.Z : ptVector.Z;

				empty = false;
			}
			if (empty)
			{
				throw new ArgumentException("You should have at least one point in points");
			}

			return new BoundingBox(minVec, maxVec);
		}

		public static BoundingBox CreateFromSphere(BoundingSphere sphere)
		{
			BoundingBox result;
			CreateFromSphere(ref sphere, out result);
			return result;
		}

		public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
		{
			Vector3 corner = new Vector3(sphere.Radius);
			result.Min = sphere.Center - corner;
			result.Max = sphere.Center + corner;
		}

		public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
		{
			BoundingBox result;
			CreateMerged(ref original, ref additional, out result);
			return result;
		}

		public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
		{
			result.Min.X = Math.Min(original.Min.X, additional.Min.X);
			result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
			result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
			result.Max.X = Math.Max(original.Max.X, additional.Max.X);
			result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
			result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override bool Equals(object obj)
		{
			return (obj is BoundingBox) && Equals((BoundingBox) obj);
		}

		public override int GetHashCode()
		{
			return this.Min.GetHashCode() + this.Max.GetHashCode();
		}

		public static bool operator ==(BoundingBox a, BoundingBox b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BoundingBox a, BoundingBox b)
		{
			return !a.Equals(b);
		}

		public override string ToString()
		{
			return (
				"{{Min:" + Min.ToString() +
				" Max:" + Max.ToString() +
				"}}"
			);
		}

		#endregion
	}
}
