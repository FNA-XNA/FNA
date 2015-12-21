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
	/// Defines formats for depth-stencil buffer.
	/// </summary>
	public enum DepthFormat
	{
		/// <summary>
		/// Depth-stencil buffer will not be created.
		/// </summary>
		None,
		/// <summary>
		/// 16-bit depth buffer.
		/// </summary>
		Depth16,
		/// <summary>
		/// 24-bit depth buffer.
		/// </summary>
		Depth24,
		/// <summary>
		/// 32-bit depth-stencil buffer. Where 24-bit depth and 8-bit for stencil used.
		/// </summary>
		Depth24Stencil8
	}
}
