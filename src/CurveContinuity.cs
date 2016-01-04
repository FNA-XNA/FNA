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
	/// Defines the continuity of keys on a <see cref="Curve"/>.
	/// </summary>
	public enum CurveContinuity
	{
		/// <summary>
		/// Interpolation can be used between this key and the next.
		/// </summary>
		Smooth,
		/// <summary>
		/// Interpolation cannot be used. A position between the two points returns this point.
		/// </summary>
		Step
	}
}
