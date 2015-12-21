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
using System.Text;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Defines a viewing frustum for intersection operations.
	/// </summary>
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public class BoundingFrustum : IEquatable<BoundingFrustum>
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the <see cref="Matrix"/> of the frustum.
		/// </summary>
		public Matrix Matrix
		{
			get
			{
				return this.matrix;
			}
			set
			{
				/* FIXME: The odds are the planes will be used a lot more often than
				 * the matrix is updated, so this should help performance. I hope. ;)
				 */
				this.matrix = value;
				this.CreatePlanes();
				this.CreateCorners();
			}
		}

		/// <summary>
		/// Gets the near plane of the frustum.
		/// </summary>
		public Plane Near
		{
			get
			{
				return this.planes[0];
			}
		}

		/// <summary>
		/// Gets the far plane of the frustum.
		/// </summary>
		public Plane Far
		{
			get
			{
				return this.planes[1];
			}
		}

		/// <summary>
		/// Gets the left plane of the frustum.
		/// </summary>
		public Plane Left
		{
			get
			{
				return this.planes[2];
			}
		}

		/// <summary>
		/// Gets the right plane of the frustum.
		/// </summary>
		public Plane Right
		{
			get
			{
				return this.planes[3];
			}
		}

		/// <summary>
		/// Gets the top plane of the frustum.
		/// </summary>
		public Plane Top
		{
			get
			{
				return this.planes[4];
			}
		}

		/// <summary>
		/// Gets the bottom plane of the frustum.
		/// </summary>
		public Plane Bottom
		{
			get
			{
				return this.planes[5];
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					"Near( ", planes[0].DebugDisplayString, " ) \r\n",
					"Far( ", planes[1].DebugDisplayString, " ) \r\n",
					"Left( ", planes[2].DebugDisplayString, " ) \r\n",
					"Right( ", planes[3].DebugDisplayString, " ) \r\n",
					"Top( ", planes[4].DebugDisplayString, " ) \r\n",
					"Bottom( ", planes[5].DebugDisplayString, " ) "
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The number of corner points in the frustum.
		/// </summary>
		public const int CornerCount = 8;

		#endregion

		#region Private Fields

		private Matrix matrix;
		private readonly Vector3[] corners = new Vector3[CornerCount];
		private readonly Plane[] planes = new Plane[PlaneCount];

		/// <summary>
		/// The number of planes in the frustum.
		/// </summary>
		private const int PlaneCount = 6;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs the frustum by extracting the view planes from a matrix.
		/// </summary>
		/// <param name="value">Combined matrix which usually is (View * Projection).</param>
		public BoundingFrustum(Matrix value)
		{
			this.matrix = value;
			this.CreatePlanes();
			this.CreateCorners();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="frustum">A <see cref="BoundingFrustum"/> for testing.</param>
		/// <returns>Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingFrustum"/>.</returns>
		public ContainmentType Contains(BoundingFrustum frustum)
		{
			if (this == frustum)
			{
				return ContainmentType.Contains;
			}
			bool intersects = false;
			for (int i = 0; i < PlaneCount; i += 1)
			{
				PlaneIntersectionType planeIntersectionType;
				frustum.Intersects(ref planes[i], out planeIntersectionType);
				if (planeIntersectionType == PlaneIntersectionType.Front)
				{
					return ContainmentType.Disjoint;
				}
				else if (planeIntersectionType == PlaneIntersectionType.Intersecting)
				{
					intersects = true;
				}
			}
			return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingBox"/>.
		/// </summary>
		/// <param name="box">A <see cref="BoundingBox"/> for testing.</param>
		/// <returns>Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingBox"/>.</returns>
		public ContainmentType Contains(BoundingBox box)
		{
			ContainmentType result = default(ContainmentType);
			this.Contains(ref box, out result);
			return result;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingBox"/>.
		/// </summary>
		/// <param name="box">A <see cref="BoundingBox"/> for testing.</param>
		/// <param name="result">Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingBox"/> as an output parameter.</param>
		public void Contains(ref BoundingBox box, out ContainmentType result)
		{
			bool intersects = false;
			for (int i = 0; i < PlaneCount; i += 1)
			{
				PlaneIntersectionType planeIntersectionType = default(PlaneIntersectionType);
				box.Intersects(ref this.planes[i], out planeIntersectionType);
				switch (planeIntersectionType)
				{
				case PlaneIntersectionType.Front:
					result = ContainmentType.Disjoint;
					return;
				case PlaneIntersectionType.Intersecting:
					intersects = true;
					break;
				}
			}
			result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingSphere"/>.
		/// </summary>
		/// <param name="sphere">A <see cref="BoundingSphere"/> for testing.</param>
		/// <returns>Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingSphere"/>.</returns>
		public ContainmentType Contains(BoundingSphere sphere)
		{
			ContainmentType result = default(ContainmentType);
			this.Contains(ref sphere, out result);
			return result;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingSphere"/>.
		/// </summary>
		/// <param name="sphere">A <see cref="BoundingSphere"/> for testing.</param>
		/// <param name="result">Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="BoundingSphere"/> as an output parameter.</param>
		public void Contains(ref BoundingSphere sphere, out ContainmentType result)
		{
			bool intersects = false;
			for (int i = 0; i < PlaneCount; i += 1)
			{
				PlaneIntersectionType planeIntersectionType = default(PlaneIntersectionType);

				// TODO: We might want to inline this for performance reasons.
				sphere.Intersects(ref this.planes[i], out planeIntersectionType);
				switch (planeIntersectionType)
				{
				case PlaneIntersectionType.Front:
					result = ContainmentType.Disjoint;
					return;
				case PlaneIntersectionType.Intersecting:
					intersects = true;
					break;
				}
			}
			result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="point">A <see cref="Vector3"/> for testing.</param>
		/// <returns>Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="Vector3"/>.</returns>
		public ContainmentType Contains(Vector3 point)
		{
			ContainmentType result = default(ContainmentType);
			this.Contains(ref point, out result);
			return result;
		}

		/// <summary>
		/// Containment test between this <see cref="BoundingFrustum"/> and specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="point">A <see cref="Vector3"/> for testing.</param>
		/// <param name="result">Result of testing for containment between this <see cref="BoundingFrustum"/> and specified <see cref="Vector3"/> as an output parameter.</param>
		public void Contains(ref Vector3 point, out ContainmentType result)
		{
			bool intersects = false;
			for (int i = 0; i < PlaneCount; i += 1)
			{
				float classifyPoint = (
					(point.X * planes[i].Normal.X) +
					(point.Y * planes[i].Normal.Y) +
					(point.Z * planes[i].Normal.Z) +
					planes[i].D
				);
				if (classifyPoint > 0)
				{
					result = ContainmentType.Disjoint;
					return;
				}
				else if (classifyPoint == 0)
				{
					intersects = true;
					break;
				}
			}
			result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
		}

		/// <summary>
		/// Returns a copy of internal corners array.
		/// </summary>
		/// <returns>The array of corners.</returns>
		public Vector3[] GetCorners()
		{
			return (Vector3[]) this.corners.Clone();
		}

		/// <summary>
		/// Returns a copy of internal corners array.
		/// </summary>
		/// <param name="corners">The array which values will be replaced to corner values of this instance. It must have size of <see cref="BoundingFrustum.CornerCount"/>.</param>
		public void GetCorners(Vector3[] corners)
		{
			if (corners == null)
			{
				throw new ArgumentNullException("corners");
			}
			if (corners.Length < CornerCount)
			{
				throw new ArgumentOutOfRangeException("corners");
			}

			this.corners.CopyTo(corners, 0);
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingFrustum"/> intersects with this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="frustum">An other <see cref="BoundingFrustum"/> for intersection test.</param>
		/// <returns><c>true</c> if other <see cref="BoundingFrustum"/> intersects with this <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(BoundingFrustum frustum)
		{
			return (Contains(frustum) != ContainmentType.Disjoint);
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingBox"/> intersects with this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="box">A <see cref="BoundingBox"/> for intersection test.</param>
		/// <returns><c>true</c> if specified <see cref="BoundingBox"/> intersects with this <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(BoundingBox box)
		{
			bool result = false;
			this.Intersects(ref box, out result);
			return result;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingBox"/> intersects with this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="box">A <see cref="BoundingBox"/> for intersection test.</param>
		/// <param name="result"><c>true</c> if specified <see cref="BoundingBox"/> intersects with this <see cref="BoundingFrustum"/>; <c>false</c> otherwise as an output parameter.</param>
		public void Intersects(ref BoundingBox box, out bool result)
		{
			ContainmentType containment = default(ContainmentType);
			this.Contains(ref box, out containment);
			result = containment != ContainmentType.Disjoint;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingSphere"/> intersects with this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="sphere">A <see cref="BoundingSphere"/> for intersection test.</param>
		/// <returns><c>true</c> if specified <see cref="BoundingSphere"/> intersects with this <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
		public bool Intersects(BoundingSphere sphere)
		{
			bool result = default(bool);
			this.Intersects(ref sphere, out result);
			return result;
		}

		/// <summary>
		/// Gets whether or not a specified <see cref="BoundingSphere"/> intersects with this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="sphere">A <see cref="BoundingSphere"/> for intersection test.</param>
		/// <param name="result"><c>true</c> if specified <see cref="BoundingSphere"/> intersects with this <see cref="BoundingFrustum"/>; <c>false</c> otherwise as an output parameter.</param>
		public void Intersects(ref BoundingSphere sphere, out bool result)
		{
			ContainmentType containment = default(ContainmentType);
			this.Contains(ref sphere, out containment);
			result = containment != ContainmentType.Disjoint;
		}

		/// <summary>
		/// Gets type of intersection between specified <see cref="Plane"/> and this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="plane">A <see cref="Plane"/> for intersection test.</param>
		/// <returns>A plane intersection type.</returns>
		public PlaneIntersectionType Intersects(Plane plane)
		{
			PlaneIntersectionType result;
			Intersects(ref plane, out result);
			return result;
		}

		/// <summary>
		/// Gets type of intersection between specified <see cref="Plane"/> and this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="plane">A <see cref="Plane"/> for intersection test.</param>
		/// <param name="result">A plane intersection type as an output parameter.</param>
		public void Intersects(ref Plane plane, out PlaneIntersectionType result)
		{
			result = plane.Intersects(ref corners[0]);
			for (int i = 1; i < corners.Length; i += 1)
			{
				if (plane.Intersects(ref corners[i]) != result)
				{
					result = PlaneIntersectionType.Intersecting;
				}
			}
		}

		/// <summary>
		/// Gets the distance of intersection of <see cref="Ray"/> and this <see cref="BoundingFrustum"/> or null if no intersection happens.
		/// </summary>
		/// <param name="ray">A <see cref="Ray"/> for intersection test.</param>
		/// <returns>Distance at which ray intersects with this <see cref="BoundingFrustum"/> or null if no intersection happens.</returns>
		public float? Intersects(Ray ray)
		{
			float? result;
			Intersects(ref ray, out result);
			return result;
		}

		/// <summary>
		/// Gets the distance of intersection of <see cref="Ray"/> and this <see cref="BoundingFrustum"/> or null if no intersection happens.
		/// </summary>
		/// <param name="ray">A <see cref="Ray"/> for intersection test.</param>
		/// <param name="result">Distance at which ray intersects with this <see cref="BoundingFrustum"/> or null if no intersection happens as an output parameter.</param>
		public void Intersects(ref Ray ray, out float? result)
		{
			ContainmentType ctype;
			Contains(ref ray.Position, out ctype);

			if (ctype == ContainmentType.Disjoint)
			{
				result = null;
				return;
			}
			if (ctype == ContainmentType.Contains)
			{
				result = 0.0f;
				return;
			}
			if (ctype != ContainmentType.Intersects)
			{
				throw new ArgumentOutOfRangeException("ctype");
			}

			// TODO: Needs additional test for not 0.0 and null results.
			result = null;
			float min = float.MinValue;
			float max = float.MaxValue;
			foreach (Plane plane in planes)
			{
				Vector3 normal = plane.Normal;
				float result2;
				Vector3.Dot(ref ray.Direction, ref normal, out result2);
				float result3;
				Vector3.Dot(ref ray.Position, ref normal, out result3);
				result3 += plane.D;
				if ((double) Math.Abs(result2) < 9.99999974737875E-06)
				{
					if ((double) result3 > 0.0)
					{
						return;
					}
				}
				else
				{
					float result4 = -result3 / result2;
					if ((double) result2 < 0.0)
					{
						if ((double) result4 > (double) max)
						{
							return;
						}
						if ((double) result4 > (double) min)
						{
							min = result4;
						}
					}
					else
					{
						if ((double) result4 < (double) min)
						{
							return;
						}
						if ((double) result4 < (double) max)
						{
							max = result4;
						}
					}
				}
				float? distance = ray.Intersects(plane);
				if (distance.HasValue)
				{
					min = Math.Min(min, distance.Value);
					max = Math.Max(max, distance.Value);
				}
			}
			float temp = min >= 0.0 ? min : max;
			if (temp < 0.0)
			{
				return;
			}
			result = temp;
		}

		#endregion

		#region Private Methods

		private void CreateCorners()
		{
			IntersectionPoint(
				ref this.planes[0],
				ref this.planes[2],
				ref this.planes[4],
				out this.corners[0]
			);
			IntersectionPoint(
				ref this.planes[0],
				ref this.planes[3],
				ref this.planes[4],
				out this.corners[1]
			);
			IntersectionPoint(
				ref this.planes[0],
				ref this.planes[3],
				ref this.planes[5],
				out this.corners[2]
			);
			IntersectionPoint(
				ref this.planes[0],
				ref this.planes[2],
				ref this.planes[5],
				out this.corners[3]
			);
			IntersectionPoint(
				ref this.planes[1],
				ref this.planes[2],
				ref this.planes[4],
				out this.corners[4]
			);
			IntersectionPoint(
				ref this.planes[1],
				ref this.planes[3],
				ref this.planes[4],
				out this.corners[5]
			);
			IntersectionPoint(
				ref this.planes[1],
				ref this.planes[3],
				ref this.planes[5],
				out this.corners[6]
			);
			IntersectionPoint(
				ref this.planes[1],
				ref this.planes[2],
				ref this.planes[5],
				out this.corners[7]
			);
		}

		private void CreatePlanes()
		{
			this.planes[0] = new Plane(
				-this.matrix.M13,
				-this.matrix.M23,
				-this.matrix.M33,
				-this.matrix.M43
			);
			this.planes[1] = new Plane(
				this.matrix.M13 - this.matrix.M14,
				this.matrix.M23 - this.matrix.M24,
				this.matrix.M33 - this.matrix.M34,
				this.matrix.M43 - this.matrix.M44
			);
			this.planes[2] = new Plane(
				-this.matrix.M14 - this.matrix.M11,
				-this.matrix.M24 - this.matrix.M21,
				-this.matrix.M34 - this.matrix.M31,
				-this.matrix.M44 - this.matrix.M41
			);
			this.planes[3] = new Plane(
				this.matrix.M11 - this.matrix.M14,
				this.matrix.M21 - this.matrix.M24,
				this.matrix.M31 - this.matrix.M34,
				this.matrix.M41 - this.matrix.M44
			);
			this.planes[4] = new Plane(
				this.matrix.M12 - this.matrix.M14,
				this.matrix.M22 - this.matrix.M24,
				this.matrix.M32 - this.matrix.M34,
				this.matrix.M42 - this.matrix.M44
			);
			this.planes[5] = new Plane(
				-this.matrix.M14 - this.matrix.M12,
				-this.matrix.M24 - this.matrix.M22,
				-this.matrix.M34 - this.matrix.M32,
				-this.matrix.M44 - this.matrix.M42
			);

			this.NormalizePlane(ref this.planes[0]);
			this.NormalizePlane(ref this.planes[1]);
			this.NormalizePlane(ref this.planes[2]);
			this.NormalizePlane(ref this.planes[3]);
			this.NormalizePlane(ref this.planes[4]);
			this.NormalizePlane(ref this.planes[5]);
		}

		private void NormalizePlane(ref Plane p)
		{
			float factor = 1f / p.Normal.Length();
			p.Normal.X *= factor;
			p.Normal.Y *= factor;
			p.Normal.Z *= factor;
			p.D *= factor;
		}

		#endregion

		#region Private Static Methods

		private static void IntersectionPoint(
			ref Plane a,
			ref Plane b,
			ref Plane c,
			out Vector3 result
		) {
			/* Formula used
			 *                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
			 * P =   -------------------------------------------------------------------
			 *                             N1 . ( N2 * N3 )
			 *
			 * Note: N refers to the normal, d refers to the displacement. '.' means dot
			 * product. '*' means cross product
			 */

			Vector3 v1, v2, v3;
			Vector3 cross;

			Vector3.Cross(ref b.Normal, ref c.Normal, out cross);

			float f;
			Vector3.Dot(ref a.Normal, ref cross, out f);
			f *= -1.0f;

			Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
			Vector3.Multiply(ref cross, a.D, out v1);
			// v1 = (a.D * (Vector3.Cross(b.Normal, c.Normal)));


			Vector3.Cross(ref c.Normal, ref a.Normal, out cross);
			Vector3.Multiply(ref cross, b.D, out v2);
			// v2 = (b.D * (Vector3.Cross(c.Normal, a.Normal)));


			Vector3.Cross(ref a.Normal, ref b.Normal, out cross);
			Vector3.Multiply(ref cross, c.D, out v3);
			// v3 = (c.D * (Vector3.Cross(a.Normal, b.Normal)));

			result.X = (v1.X + v2.X + v3.X) / f;
			result.Y = (v1.Y + v2.Y + v3.Y) / f;
			result.Z = (v1.Z + v2.Z + v3.Z) / f;
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Compares whether two <see cref="BoundingFrustum"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="BoundingFrustum"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="BoundingFrustum"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
		{
			if (object.Equals(a, null))
			{
				return (object.Equals(b, null));
			}

			if (object.Equals(b, null))
			{
				return (object.Equals(a, null));
			}

			return a.matrix == (b.matrix);
		}

		/// <summary>
		/// Compares whether two <see cref="BoundingFrustum"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="BoundingFrustum"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="BoundingFrustum"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="other">The <see cref="BoundingFrustum"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(BoundingFrustum other)
		{
			return (this == other);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			BoundingFrustum f = obj as BoundingFrustum;
			return (object.Equals(f, null)) ? false : (this == f);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="BoundingFrustum"/> in the format:
		/// {Near:[nearPlane] Far:[farPlane] Left:[leftPlane] Right:[rightPlane] Top:[topPlane] Bottom:[bottomPlane]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="BoundingFrustum"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(256);
			sb.Append("{Near:");
			sb.Append(this.planes[0].ToString());
			sb.Append(" Far:");
			sb.Append(this.planes[1].ToString());
			sb.Append(" Left:");
			sb.Append(this.planes[2].ToString());
			sb.Append(" Right:");
			sb.Append(this.planes[3].ToString());
			sb.Append(" Top:");
			sb.Append(this.planes[4].ToString());
			sb.Append(" Bottom:");
			sb.Append(this.planes[5].ToString());
			sb.Append("}");
			return sb.ToString();
		}

		/// <summary>
		/// Gets the hash code of this <see cref="BoundingFrustum"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="BoundingFrustum"/>.</returns>
		public override int GetHashCode()
		{
			return this.matrix.GetHashCode();
		}

		#endregion
	}
}

