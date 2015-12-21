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

using SDL2;
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

		internal static int INTERNAL_WindowWidth = 800;
		internal static int INTERNAL_WindowHeight = 600;

		internal static int INTERNAL_MouseWheel = 0;

		internal static bool INTERNAL_IsWarped = false;

		#endregion

		#region Private Variables

		private static MouseState state;

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
			uint flags = SDL.SDL_GetMouseState(out x, out y);

			// If we warped the mouse, we've already done this in SetPosition.
			if (!INTERNAL_IsWarped)
			{
				// Scale the mouse coordinates for the faux-backbuffer
				state.X = (int) ((double) x * Game.Instance.GraphicsDevice.GLDevice.Backbuffer.Width / INTERNAL_WindowWidth);
				state.Y = (int) ((double) y * Game.Instance.GraphicsDevice.GLDevice.Backbuffer.Height / INTERNAL_WindowHeight);
			}

			state.LeftButton =	(ButtonState) (flags & SDL.SDL_BUTTON_LMASK);
			state.MiddleButton =	(ButtonState) ((flags & SDL.SDL_BUTTON_MMASK) >> 1);
			state.RightButton =	(ButtonState) ((flags & SDL.SDL_BUTTON_RMASK) >> 2);
			state.XButton1 =	(ButtonState) ((flags & SDL.SDL_BUTTON_X1MASK) >> 3);
			state.XButton2 =	(ButtonState) ((flags & SDL.SDL_BUTTON_X2MASK) >> 4);

			state.ScrollWheelValue = INTERNAL_MouseWheel;

			return state;
		}

		/// <summary>
		/// Sets mouse cursor's relative position to game-window.
		/// </summary>
		/// <param name="x">Relative horizontal position of the cursor.</param>
		/// <param name="y">Relative vertical position of the cursor.</param>
		public static void SetPosition(int x, int y)
		{
			// The state should appear to be what they _think_ they're setting first.
			state.X = x;
			state.Y = y;

			// Scale the mouse coordinates for the faux-backbuffer
			x = (int) ((double) x * INTERNAL_WindowWidth / Game.Instance.GraphicsDevice.GLDevice.Backbuffer.Width);
			y = (int) ((double) y * INTERNAL_WindowHeight / Game.Instance.GraphicsDevice.GLDevice.Backbuffer.Height);

			SDL.SDL_WarpMouseInWindow(WindowHandle, x, y);
			INTERNAL_IsWarped = true;
		}

		#endregion
	}
}
