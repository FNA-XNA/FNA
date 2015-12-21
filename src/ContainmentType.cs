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

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Defines how the bounding volumes intersects or contain one another.
	/// </summary>
	public enum ContainmentType
	{
		/// <summary>
		/// Indicates that there is no overlap between two bounding volumes.
		/// </summary>
		Disjoint,
		/// <summary>
		/// Indicates that one bounding volume completely contains another volume.
		/// </summary>
		Contains,
		/// <summary>
		/// Indicates that bounding volumes partially overlap one another.
		/// </summary>
		Intersects
	}
}
