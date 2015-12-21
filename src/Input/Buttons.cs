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
#endregion

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Defines the buttons on gamepad.
	/// </summary>
	[Flags]
	public enum Buttons
	{
		/// <summary>
		/// Directional pad down.
		/// </summary>
		DPadUp =		0x00000001,
		/// <summary>
		/// Directional pad up.
		/// </summary>
		DPadDown =		0x00000002,
		/// <summary>
		/// Directional pad left.
		/// </summary>
		DPadLeft =		0x00000004,
		/// <summary>
		/// Directional pad right.
		/// </summary>
		DPadRight =		0x00000008,
		/// <summary>
		/// START button.
		/// </summary>
		Start =			0x00000010,
		/// <summary>
		/// BACK button.
		/// </summary>
		Back =			0x00000020,
		/// <summary>
		/// Left stick button (pressing the left stick).
		/// </summary>
		LeftStick =		0x00000040,
		/// <summary>
		/// Right stick button (pressing the right stick).
		/// </summary>
		RightStick =		0x00000080,
		/// <summary>
		/// Left bumper (shoulder) button.
		/// </summary>
		LeftShoulder =		0x00000100,
		/// <summary>
		/// Right bumper (shoulder) button.
		/// </summary>
		RightShoulder =		0x00000200,
		/// <summary>
		/// Big button.
		/// </summary>
		BigButton =		0x00000800,
		/// <summary>
		/// A button.
		/// </summary>
		A =			0x00001000,
		/// <summary>
		/// B button.
		/// </summary>
		B =			0x00002000,
		/// <summary>
		/// X button.
		/// </summary>
		X =			0x00004000,
		/// <summary>
		/// Y button.
		/// </summary>
		Y =			0x00008000,
		/// <summary>
		/// Left stick is towards the left.
		/// </summary>
		LeftThumbstickLeft =	0x00200000,
		/// <summary>
		/// Right trigger.
		/// </summary>
		RightTrigger =		0x00400000,
		/// <summary>
		/// Left trigger.
		/// </summary>
		LeftTrigger =		0x00800000,
		/// <summary>
		/// Right stick is towards up.
		/// </summary>
		RightThumbstickUp =	0x01000000,
		/// <summary>
		/// Right stick is towards down.
		/// </summary>
		RightThumbstickDown =	0x02000000,
		/// <summary>
		/// Right stick is towards the right.
		/// </summary>
		RightThumbstickRight =	0x04000000,
		/// <summary>
		/// Right stick is towards the left.
		/// </summary>
		RightThumbstickLeft =	0x08000000,
		/// <summary>
		/// Left stick is towards up.
		/// </summary>
		LeftThumbstickUp =	0x10000000,
		/// <summary>
		/// Left stick is towards down.
		/// </summary>
		LeftThumbstickDown =	0x20000000,
		/// <summary>
		/// Left stick is towards the right.
		/// </summary>
		LeftThumbstickRight =	0x40000000
	}
}
