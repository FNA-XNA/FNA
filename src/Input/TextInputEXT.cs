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
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public static class TextInputEXT
	{
		#region Event

		/// <summary>
		/// Use this event to retrieve text for objects like textboxes.
		/// This event is not raised by noncharacter keys.
		/// This event also supports key repeat.
		/// For more information this event is based off:
		/// http://msdn.microsoft.com/en-AU/library/system.windows.forms.control.keypress.aspx
		/// </summary>
		public static event Action<char> TextInput;

		/// <summary>
		/// This event notifies you of in-progress text composition happening in an IME or other tool
		///  and allows you to display the draft text appropriately before it has become input.
		/// For more information, see SDL's tutorial: https://wiki.libsdl.org/Tutorials-TextInput
		/// </summary>
		public static event Action<string, int, int> TextEditing;

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns if text input state is active
		///
		/// Note: For on-screen keyboard, this may remain true on
		/// some platforms if an external event closed the keyboard.
		/// In this case, check IsScreenKeyboardShow instead.
		/// </summary>
		/// <returns>True if text input state is active</returns>
		public static bool IsTextInputActive()
		{
			return FNAPlatform.IsTextInputActive();
		}

		public static bool IsScreenKeyboardShown(IntPtr window)
		{
			return FNAPlatform.IsScreenKeyboardShown(window);
		}

		public static void StartTextInput()
		{
			FNAPlatform.StartTextInput();
		}

		public static void StopTextInput()
		{
			FNAPlatform.StopTextInput();
		}

		/// <summary>
		/// Sets the location within the game window where the text input is located.
		/// This is used to set the location of the IME suggestions
		/// </summary>
		/// <param name="rectangle">Text input location relative to GameWindow.ClientBounds</param>
		public static void SetInputRectangle(Rectangle rectangle)
		{
			FNAPlatform.SetTextInputRectangle(rectangle);
		}

		#endregion

		#region Internal Event Access Method

		internal static void OnTextInput(char c)
		{
			if (TextInput != null)
			{
				TextInput(c);
			}
		}

		internal static void OnTextEditing(string text, int start, int length)
		{
			if (TextEditing != null)
			{
				TextEditing(text, start, length);
			}
		}

		#endregion
	}
}
