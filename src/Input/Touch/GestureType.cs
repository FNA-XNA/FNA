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

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.gesturetype.aspx
	[Flags]
	public enum GestureType
	{
		// FIXME: Check the real XNA enum values!
		None =			0x000,
		Tap =			0x001,
		DoubleTap =		0x002,
		Hold = 			0x004,
		HorizontalDrag =	0x008,
		VerticalDrag =		0x010,
		FreeDrag =		0x020,
		Pinch =			0x040,
		Flick =			0x080,
		DragComplete =		0x100,
		PinchComplete =		0x200
	}
}
