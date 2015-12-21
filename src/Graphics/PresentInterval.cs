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
	/// Defines how <see cref="GraphicsDevice.Present"/> updates the game window.
	/// </summary>
	public enum PresentInterval
	{
		/// <summary>
		/// Equivalent to <see cref="PresentInterval.One"/>.
		/// </summary>
		Default = 0,
		/// <summary>
		/// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than the screen refresh rate.
		/// </summary>
		One = 1,
		/// <summary>
		/// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than every second screen refresh.
		/// </summary>
		Two = 2,
		/// <summary>
		/// The driver updates the window client area immediately. Present operations might be affected immediately. There is no limit for framerate.
		/// </summary>
		Immediate = 3,
	}
}
