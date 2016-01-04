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

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Defines the intersection between a <see cref="Plane"/> and a bounding volume.
	/// </summary>
	public enum PlaneIntersectionType
	{
		/// <summary>
		/// There is no intersection, the bounding volume is in the negative half space of the plane.
		/// </summary>
		Front,
		/// <summary>
		/// There is no intersection, the bounding volume is in the positive half space of the plane.
		/// </summary>
		Back,
		/// <summary>
		/// The plane is intersected.
		/// </summary>
		Intersecting
	}
}
