#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadDPad
	{
		#region Public Properties

		public ButtonState Down
		{
			get;
			internal set;
		}

		public ButtonState Left
		{
			get;
			internal set;
		}

		public ButtonState Right
		{
			get;
			internal set;
		}

		public ButtonState Up
		{
			get;
			internal set;
		}

		#endregion

		#region Public Constructor

		public GamePadDPad(
			ButtonState upValue,
			ButtonState downValue,
			ButtonState leftValue,
			ButtonState rightValue
		) : this() {
			Up = upValue;
			Down = downValue;
			Left = leftValue;
			Right = rightValue;
		}

		#endregion

		#region Internal Constructor

		internal GamePadDPad(params Buttons[] buttons) : this()
		{
			foreach (Buttons b in buttons)
			{
				if ((b & Buttons.DPadDown) == Buttons.DPadDown)
				{
					Down = ButtonState.Pressed;
				}
				if ((b & Buttons.DPadLeft) == Buttons.DPadLeft)
				{
					Left = ButtonState.Pressed;
				}
				if ((b & Buttons.DPadRight) == Buttons.DPadRight)
				{
					Right = ButtonState.Pressed;
				}
				if ((b & Buttons.DPadUp) == Buttons.DPadUp)
				{
					Up = ButtonState.Pressed;
				}
			}
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadDPad"/> are equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator ==(GamePadDPad left, GamePadDPad right)
		{
			return	(	(left.Down == right.Down) &&
					(left.Left == right.Left) &&
					(left.Right == right.Right) &&
					(left.Up == right.Up)	);
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadDPad"/> are not
		/// equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator !=(GamePadDPad left, GamePadDPad right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a <see cref="GamePadDPad"/> and has the same
		/// value as this instance; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is GamePadDPad) && (this == (GamePadDPad) obj);
		}

		public override int GetHashCode ()
		{
			return (
				(Down  == ButtonState.Pressed ? 1 : 0) +
				(Left  == ButtonState.Pressed ? 2 : 0) +
				(Right == ButtonState.Pressed ? 4 : 0) +
				(Up    == ButtonState.Pressed ? 8 : 0)
			);
		}

		#endregion
	}
}
