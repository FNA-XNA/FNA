#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Defines the orientation of the display.
	/// </summary>
	[Flags]
	public enum DisplayOrientation
	{
		/// <summary>
		/// The default orientation.
		/// </summary>
		Default = 0,
		/// <summary>
		/// The display is rotated counterclockwise into a landscape orientation. Width is greater than height.
		/// </summary>
		LandscapeLeft = 1,
		/// <summary>
		/// The display is rotated clockwise into a landscape orientation. Width is greater than height.
		/// </summary>
		LandscapeRight = 2,
		/// <summary>
		/// The display is rotated as portrait, where height is greater than width.
		/// </summary>
		Portrait = 4
	}
}
