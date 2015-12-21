#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Defines a type of gamepad.
	/// </summary>
	public enum GamePadType
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown,
		/// <summary>
		/// GamePad is the XBOX controller.
		/// </summary>
		GamePad,
		/// <summary>
		/// GamePad is a wheel.
		/// </summary>
		Wheel,
		/// <summary>
		/// GamePad is an arcade stick.
		/// </summary>
		ArcadeStick,
		/// <summary>
		/// GamePad is a flight stick.
		/// </summary>
		FlightStick,
		/// <summary>
		/// GamePad is a dance pad.
		/// </summary>
		DancePad,
		/// <summary>
		/// GamePad is a guitar.
		/// </summary>
		Guitar,
		/// <summary>
		/// GamePad is an alternate guitar.
		/// </summary>
		AlternateGuitar,
		/// <summary>
		/// GamePad is a drum kit.
		/// </summary>
		DrumKit,
		/// <summary>
		/// GamePad is a big button pad.
		/// </summary>
		BigButtonPad,
	}
}
