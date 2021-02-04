#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadButtons
	{
		#region Public Properties

		public ButtonState A =>
            (buttons & Buttons.A) == Buttons.A ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState B =>
            (buttons & Buttons.B) == Buttons.B ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState Back =>
            (buttons & Buttons.Back) == Buttons.Back ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState X =>
            (buttons & Buttons.X) == Buttons.X ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState Y =>
            (buttons & Buttons.Y) == Buttons.Y ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState Start =>
            (buttons & Buttons.Start) == Buttons.Start ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState LeftShoulder =>
            (buttons & Buttons.LeftShoulder) == Buttons.LeftShoulder ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState LeftStick =>
            (buttons & Buttons.LeftStick) == Buttons.LeftStick ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState RightShoulder =>
            (buttons & Buttons.RightShoulder) == Buttons.RightShoulder ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState RightStick =>
            (buttons & Buttons.RightStick) == Buttons.RightStick ?
                ButtonState.Pressed :
                ButtonState.Released;

        public ButtonState BigButton =>
            (buttons & Buttons.BigButton) == Buttons.BigButton ?
                ButtonState.Pressed :
                ButtonState.Released;

        #endregion

		#region Internal Variables

		internal Buttons buttons;

		#endregion

		#region Public Constructor

		public GamePadButtons(Buttons buttons)
		{
			this.buttons = buttons;
		}

		#endregion

		#region Internal Static Methods

		/* Used by GamePadState public constructor, DO NOT USE! */
		internal static GamePadButtons FromButtonArray(params Buttons[] buttons)
		{
			Buttons mask = (Buttons) 0;
			foreach (Buttons b in buttons)
			{
				mask |= b;
			}
			return new GamePadButtons(mask);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadButtons"/> are
		/// equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator ==(GamePadButtons left, GamePadButtons right)
		{
			return left.buttons == right.buttons;
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadButtons"/> are
		/// not equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator !=(GamePadButtons left, GamePadButtons right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a <see cref="GamePadButtons"/> and
		/// has the same value as this instance; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is GamePadButtons && this == (GamePadButtons) obj;
		}

		public override int GetHashCode()
		{
			return (int) this.buttons;
		}

		#endregion
	}
}
