#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines if the previous content in a render target is preserved when it set on the graphics device.
	/// </summary>
	public enum RenderTargetUsage
	{
		/// <summary>
		/// The render target content will not be preserved.
		/// </summary>
		DiscardContents,
		/// <summary>
		/// The render target content will be preserved even if it is slow or requires extra memory.
		/// </summary>
		PreserveContents,
		/// <summary>
		/// The render target content might be preserved if the platform can do so without a penalty in performance or memory usage.
		/// </summary>
		PlatformContents
	}
}
