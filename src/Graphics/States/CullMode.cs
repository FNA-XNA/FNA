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
	/// Defines a culling mode for faces in rasterization process.
	/// </summary>
	public enum CullMode
	{
		/// <summary>
		/// Do not cull faces.
		/// </summary>
		None,
		/// <summary>
		/// Cull faces with clockwise order.
		/// </summary>
		CullClockwiseFace,
		/// <summary>
		/// Cull faces with counter clockwise order.
		/// </summary>
		CullCounterClockwiseFace
	}
}
