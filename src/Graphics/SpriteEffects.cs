#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines sprite visual options for mirroring.
	/// </summary>
	[Flags]
	public enum SpriteEffects
	{
		/// <summary>
		/// No options specified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Render the sprite reversed along the X axis.
		/// </summary>
		FlipHorizontally = 1,
		/// <summary>
		/// Render the sprite reversed along the Y axis.
		/// </summary>
		FlipVertically = 2
	}
}
