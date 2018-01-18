#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
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

		#endregion

		#region Public Static Methods

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

		#endregion
	}
}
