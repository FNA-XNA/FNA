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

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Allows reading position and button click information from mouse.
	/// </summary>
	public static class Mouse
	{
		#region Public Properties

		public static IntPtr WindowHandle
		{
			get;
			set;
		}

		#endregion

		#region Internal Variables

		internal static int INTERNAL_WindowWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
		internal static int INTERNAL_WindowHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
		internal static int INTERNAL_BackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
		internal static int INTERNAL_BackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;

		internal static int INTERNAL_MouseWheel = 0;

		// FIXME: Remove when global mouse state is accessible! -flibit
		internal static bool INTERNAL_IsWarped = false;
		internal static int INTERNAL_warpX = 0;
		internal static int INTERNAL_warpY = 0;

		#endregion

		#region Public Interface

		/// <summary>
		/// Gets mouse state information that includes position and button
		/// presses for the provided window
		/// </summary>
		/// <returns>Current state of the mouse.</returns>
		public static MouseState GetState()
		{
			int x, y;
			ButtonState left, middle, right, x1, x2;

			FNAPlatform.GetMouseState(
				out x,
				out y,
				out left,
				out middle,
				out right,
				out x1,
				out x2
			);

			// If we warped the mouse, we've already done this in SetPosition.
			if (INTERNAL_IsWarped)
			{
				x = INTERNAL_warpX;
				y = INTERNAL_warpY;
			}
			else
			{
				// Scale the mouse coordinates for the faux-backbuffer
				x = (int) ((double) x * INTERNAL_BackBufferWidth / INTERNAL_WindowWidth);
				y = (int) ((double) y * INTERNAL_BackBufferHeight / INTERNAL_WindowHeight);
			}

			return new MouseState(
				x,
				y,
				INTERNAL_MouseWheel,
				left,
				middle,
				right,
				x1,
				x2
			);
		}

		/// <summary>
		/// Sets mouse cursor's relative position to game-window.
		/// </summary>
		/// <param name="x">Relative horizontal position of the cursor.</param>
		/// <param name="y">Relative vertical position of the cursor.</param>
		public static void SetPosition(int x, int y)
		{
			// The state should appear to be what they _think_ they're setting first.
			INTERNAL_warpX = x;
			INTERNAL_warpY = y;

			// Scale the mouse coordinates for the faux-backbuffer
			x = (int) ((double) x * INTERNAL_WindowWidth / INTERNAL_BackBufferWidth);
			y = (int) ((double) y * INTERNAL_WindowHeight / INTERNAL_BackBufferHeight);

			FNAPlatform.SetMousePosition(WindowHandle, x, y);
			INTERNAL_IsWarped = true;
		}

		#endregion
	}
}
