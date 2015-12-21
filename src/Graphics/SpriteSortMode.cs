#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines sprite sort rendering options.
	/// </summary>
	public enum SpriteSortMode
	{
		/// <summary>
		/// All sprites are drawing when <see cref="SpriteBatch.End"/> invokes, in order of draw call sequence. Depth is ignored.
		/// </summary>
		Deferred = 0,
		/// <summary>
		/// Each sprite is drawing at individual draw call, instead of <see cref="SpriteBatch.End"/>. Depth is ignored.
		/// </summary>
		Immediate = 1,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by texture prior to drawing. Depth is ignored.
		/// </summary>
		Texture = 2,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by depth in back-to-front order prior to drawing.
		/// </summary>
		BackToFront = 3,
		/// <summary>
		/// Same as <see cref="SpriteSortMode.Deferred"/>, except sprites are sorted by depth in front-to-back order prior to drawing.
		/// </summary>
		FrontToBack = 4
	}
}
