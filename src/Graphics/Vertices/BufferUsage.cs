#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// A usage hint for optimizing memory placement of graphics buffers.
	/// </summary>
	public enum BufferUsage
	{
		/// <summary>
		/// No special usage.
		/// </summary>
		None,
		/// <summary>
		/// The buffer will not be readable and will be optimized for rendering and writing.
		/// </summary>
		WriteOnly
	}
}
