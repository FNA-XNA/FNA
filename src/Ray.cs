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
	[TypeConverter(typeof(RayConverter))]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Ray : IEquatable<Ray>
	{
		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					"Pos( ", Position.DebugDisplayString, " ) \r\n",
					"Dir( ", Direction.DebugDisplayString, " )"
				);
			}
		}

		#endregion

		#region Public Fields

		public Vector3 Position;
		public Vector3 Direction;

		#endregion


		#region Public Constructors

		public Ray(Vector3 position, Vector3 direction)
		{
			Position = position;
			Direction = direction;
		}

		#endregion


		#region Public Methods

		public override bool Equals(object obj)
		{
			return (obj is Ray) && Equals((Ray) obj);
		}


		public bool Equals(Ray other)
		{
			return (	this.Position.Equals(other.Position) &&
					this.Direction.Equals(other.Direction)	);
		}


		public override int GetHashCode()
		{
			return Position.GetHashCode() ^ Direction.GetHashCode();
		}

		// Adapted from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
		public float? Intersects(BoundingBox box)
		{
			float? tMin = null, tMax = null;

			if (MathHelper.WithinEpsilon(Direction.X, 0.0f))
			{
				if (Position.X < box.Min.X || Position.X > box.Max.X)
				{
					return null;
				}
			}
			else
			{
				tMin = (box.Min.X - Position.X) / Direction.X;
				tMax = (box.Max.X - Position.X) / Direction.X;

				if (tMin > tMax)
				{
					float? temp = tMin;
					tMin = tMax;
					tMax = temp;
				}
			}

			if (MathHelper.WithinEpsilon(Direction.Y, 0.0f))
			{
				if (Position.Y < box.Min.Y || Position.Y > box.Max.Y)
				{
					return null;
				}
			}
			else
			{
				float tMinY = (box.Min.Y - Position.Y) / Direction.Y;
				float tMaxY = (box.Max.Y - Position.Y) / Direction.Y;

				if (tMinY > tMaxY)
				{
					float temp = tMinY;
					tMinY = tMaxY;
					tMaxY = temp;
				}

				if (	(tMin.HasValue && tMin > tMaxY) ||
					(tMax.HasValue && tMinY > tMax)	)
				{
					return null;
				}

				if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
				if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
			}

			if (MathHelper.WithinEpsilon(Direction.Z, 0.0f))
			{
				if (Position.Z < box.Min.Z || Position.Z > box.Max.Z)
				{
					return null;
				}
			}
			else
			{
				float tMinZ = (box.Min.Z - Position.Z) / Direction.Z;
				float tMaxZ = (box.Max.Z - Position.Z) / Direction.Z;

				if (tMinZ > tMaxZ)
				{
					float temp = tMinZ;
					tMinZ = tMaxZ;
					tMaxZ = temp;
				}

				if (	(tMin.HasValue && tMin > tMaxZ) ||
					(tMax.HasValue && tMinZ > tMax)	)
				{
					return null;
				}

				if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
				if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
			}

			/* Having a positive tMin and a negative tMax means the ray is inside the
			 * box we expect the intesection distance to be 0 in that case.
			 */
			if ((tMin.HasValue && tMin < 0) && tMax > 0) return 0;

			/* A negative tMin means that the intersection point is behind the ray's
			 * origin. We discard these as not hitting the AABB.
			 */
			if (tMin < 0) return null;

			return tMin;
		}


		public void Intersects(ref BoundingBox box, out float? result)
		{
			result = Intersects(box);
		}

		public float? Intersects(BoundingSphere sphere)
		{
			float? result;
			Intersects(ref sphere, out result);
			return result;
		}

		public float? Intersects(Plane plane)
		{
			float? result;
			Intersects(ref plane, out result);
			return result;
		}

		public float? Intersects(BoundingFrustum frustum)
		{
			float? result;
			frustum.Intersects(ref this, out result);
			return result;
		}

		public void Intersects(ref Plane plane, out float? result)
		{
			float den = Vector3.Dot(Direction, plane.Normal);
			if (Math.Abs(den) < 0.00001f)
			{
				result = null;
				return;
			}

			result = (-plane.D - Vector3.Dot(plane.Normal, Position)) / den;

			if (result < 0.0f)
			{
				if (result < -0.00001f)
				{
					result = null;
					return;
				}

				result = 0.0f;
			}
		}

		public void Intersects(ref BoundingSphere sphere, out float? result)
		{
			// Find the vector between where the ray starts the the sphere's center.
			Vector3 difference = sphere.Center - this.Position;

			float differenceLengthSquared = difference.LengthSquared();
			float sphereRadiusSquared = sphere.Radius * sphere.Radius;

			float distanceAlongRay;

			/* If the distance between the ray start and the sphere's center is less than
			 * the radius of the sphere, it means we've intersected. Checking the
			 * LengthSquared is faster.
			 */
			if (differenceLengthSquared < sphereRadiusSquared)
			{
				result = 0.0f;
				return;
			}

			Vector3.Dot(ref this.Direction, ref difference, out distanceAlongRay);
			// If the ray is pointing away from the sphere then we don't ever intersect.
			if (distanceAlongRay < 0)
			{
				result = null;
				return;
			}

			/* Next we kinda use Pythagoras to check if we are within the bounds of the
			 * sphere.
			 * if x = radius of sphere
			 * if y = distance between ray position and sphere centre
			 * if z = the distance we've travelled along the ray
			 * if x^2 + z^2 - y^2 < 0, we do not intersect
			 */
			float dist = (
				sphereRadiusSquared +
				(distanceAlongRay * distanceAlongRay) -
				differenceLengthSquared
			);

			result = (dist < 0) ? null : distanceAlongRay - (float?) Math.Sqrt(dist);
		}

		#endregion

		#region Public Static Methods

		public static bool operator !=(Ray a, Ray b)
		{
			return !a.Equals(b);
		}


		public static bool operator ==(Ray a, Ray b)
		{
			return a.Equals(b);
		}


		public override string ToString()
		{
			return (
				"{{Position:" + Position.ToString() +
				" Direction:" + Direction.ToString() +
				"}}"
			);
		}

		#endregion
	}
}
