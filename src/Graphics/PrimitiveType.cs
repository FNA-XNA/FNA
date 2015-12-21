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
	/// Defines how vertex data is ordered.
	/// </summary>
	public enum PrimitiveType
	{
		/// <summary>
		/// Renders the specified vertices as a sequence of isolated triangles. Each group of three vertices defines a separate triangle. Back-face culling is affected by the current winding-order render state.
		/// </summary>
		TriangleList,

		/// <summary>
		/// Renders the vertices as a triangle strip. The back-face culling flag is flipped automatically on even-numbered triangles.
		/// </summary>
		TriangleStrip,

		/// <summary>
		/// Renders the vertices as a list of isolated straight line segments; the count may be any positive integer.
		/// </summary>
		LineList,

		/// <summary>
		/// Renders the vertices as a single polyline; the count may be any positive integer.
		/// </summary>
		LineStrip,
	}
}
