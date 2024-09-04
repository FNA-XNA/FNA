#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Allows getting keystrokes from keyboard.
	/// </summary>
	public static class Keyboard
	{
		#region Public Static Methods

		/// <summary>
		/// Returns the current keyboard state.
		/// </summary>
		/// <returns>Current keyboard state.</returns>
		public static KeyboardState GetState()
		{
			activeKeys.Clear();
			FNAPlatform.GetKeyboardState(activeKeys);
			return new KeyboardState(activeKeys);
		}

		/// <summary>
		/// Returns the current keyboard state for a given player.
		/// </summary>
		/// <param name="playerIndex">Player index of the keyboard.</param>
		/// <returns>Current keyboard state.</returns>
		public static KeyboardState GetState(PlayerIndex playerIndex)
		{
			return GetState();
		}

		#endregion

		#region Public Static FNA Extensions

		public static Keys GetKeyFromScancodeEXT(Keys scancode)
		{
			return FNAPlatform.GetKeyFromScancode(scancode);
		}

		#endregion

		#region Private Static Variables

		private static List<Keys> activeKeys = new List<Keys>();

		#endregion
	}
}
