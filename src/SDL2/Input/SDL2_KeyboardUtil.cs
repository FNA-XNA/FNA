#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Input
{
	internal static class SDL2_KeyboardUtil
	{
		#region Private SDL2->XNA Key Hashmaps

		/* From: http://blogs.msdn.com/b/shawnhar/archive/2007/07/02/twin-paths-to-garbage-collector-nirvana.aspx
		 * "If you use an enum type as a dictionary key, internal dictionary operations will cause boxing.
		 * You can avoid this by using integer keys, and casting your enum values to ints before adding
		 * them to the dictionary."
		 */
		private static Dictionary<int, Keys> INTERNAL_keyMap;
		private static Dictionary<int, Keys> INTERNAL_scanMap;
		private static Dictionary<int, SDL.SDL_Scancode> INTERNAL_xnaMap;

		#endregion

		#region Hashmap Initializer Constructor

		static SDL2_KeyboardUtil()
		{
			// Create the dictionaries...
			INTERNAL_keyMap = new Dictionary<int, Keys>();
			INTERNAL_scanMap = new Dictionary<int, Keys>();
			INTERNAL_xnaMap = new Dictionary<int, SDL.SDL_Scancode>();

			// Then fill them with known keys that match up to XNA Keys.

			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_a,		Keys.A);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_b,		Keys.B);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_c,		Keys.C);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_d,		Keys.D);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_e,		Keys.E);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_f,		Keys.F);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_g,		Keys.G);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_h,		Keys.H);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_i,		Keys.I);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_j,		Keys.J);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_k,		Keys.K);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_l,		Keys.L);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_m,		Keys.M);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_n,		Keys.N);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_o,		Keys.O);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_p,		Keys.P);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_q,		Keys.Q);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_r,		Keys.R);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_s,		Keys.S);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_t,		Keys.T);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_u,		Keys.U);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_v,		Keys.V);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_w,		Keys.W);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_x,		Keys.X);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_y,		Keys.Y);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_z,		Keys.Z);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_0,		Keys.D0);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_1,		Keys.D1);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_2,		Keys.D2);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_3,		Keys.D3);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_4,		Keys.D4);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_5,		Keys.D5);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_6,		Keys.D6);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_7,		Keys.D7);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_8,		Keys.D8);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_9,		Keys.D9);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_0,		Keys.NumPad0);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_1,		Keys.NumPad1);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_2,		Keys.NumPad2);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_3,		Keys.NumPad3);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_4,		Keys.NumPad4);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_5,		Keys.NumPad5);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_6,		Keys.NumPad6);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_7,		Keys.NumPad7);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_8,		Keys.NumPad8);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_9,		Keys.NumPad9);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_CLEAR,	Keys.OemClear);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_DECIMAL,	Keys.Decimal);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_DIVIDE,	Keys.Divide);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_ENTER,	Keys.Enter);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_MINUS,	Keys.Subtract);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_MULTIPLY,	Keys.Multiply);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_PERIOD,	Keys.OemPeriod);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_KP_PLUS,		Keys.Add);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F1,		Keys.F1);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F2,		Keys.F2);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F3,		Keys.F3);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F4,		Keys.F4);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F5,		Keys.F5);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F6,		Keys.F6);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F7,		Keys.F7);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F8,		Keys.F8);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F9,		Keys.F9);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F10,		Keys.F10);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F11,		Keys.F11);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F12,		Keys.F12);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F13,		Keys.F13);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F14,		Keys.F14);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F15,		Keys.F15);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F16,		Keys.F16);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F17,		Keys.F17);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F18,		Keys.F18);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F19,		Keys.F19);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F20,		Keys.F20);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F21,		Keys.F21);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F22,		Keys.F22);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F23,		Keys.F23);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_F24,		Keys.F24);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_SPACE,		Keys.Space);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_UP,		Keys.Up);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_DOWN,		Keys.Down);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LEFT,		Keys.Left);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RIGHT,		Keys.Right);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LALT,		Keys.LeftAlt);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RALT,		Keys.RightAlt);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LCTRL,		Keys.LeftControl);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RCTRL,		Keys.RightControl);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LGUI,		Keys.LeftWindows);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RGUI,		Keys.RightWindows);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LSHIFT,		Keys.LeftShift);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RSHIFT,		Keys.RightShift);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_APPLICATION,	Keys.Apps);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_SLASH,		Keys.OemQuestion);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_BACKSLASH,	Keys.OemBackslash);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_LEFTBRACKET,	Keys.OemOpenBrackets);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RIGHTBRACKET,	Keys.OemCloseBrackets);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_CAPSLOCK,	Keys.CapsLock);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_COMMA,		Keys.OemComma);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_DELETE,		Keys.Delete);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_END,		Keys.End);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_BACKSPACE,	Keys.Back);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_RETURN,		Keys.Enter);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_ESCAPE,		Keys.Escape);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_HOME,		Keys.Home);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_INSERT,		Keys.Insert);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_MINUS,		Keys.OemMinus);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR,	Keys.NumLock);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_PAGEUP,		Keys.PageUp);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_PAGEDOWN,	Keys.PageDown);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_PAUSE,		Keys.Pause);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_PERIOD,		Keys.OemPeriod);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_EQUALS,		Keys.OemPlus);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_PRINTSCREEN,	Keys.PrintScreen);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_QUOTE,		Keys.OemQuotes);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_SCROLLLOCK,	Keys.Scroll);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_SEMICOLON,	Keys.OemSemicolon);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_SLEEP,		Keys.Sleep);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_TAB,		Keys.Tab);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_BACKQUOTE,	Keys.OemTilde);
			INTERNAL_keyMap.Add((int) SDL.SDL_Keycode.SDLK_UNKNOWN,		Keys.None);

			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_A,		Keys.A);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_B,		Keys.B);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_C,		Keys.C);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_D,		Keys.D);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_E,		Keys.E);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F,		Keys.F);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_G,		Keys.G);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_H,		Keys.H);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_I,		Keys.I);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_J,		Keys.J);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_K,		Keys.K);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_L,		Keys.L);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_M,		Keys.M);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_N,		Keys.N);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_O,		Keys.O);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_P,		Keys.P);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_Q,		Keys.Q);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_R,		Keys.R);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_S,		Keys.S);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_T,		Keys.T);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_U,		Keys.U);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_V,		Keys.V);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_W,		Keys.W);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_X,		Keys.X);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_Y,		Keys.Y);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_Z,		Keys.Z);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_0,		Keys.D0);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_1,		Keys.D1);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_2,		Keys.D2);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_3,		Keys.D3);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_4,		Keys.D4);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_5,		Keys.D5);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_6,		Keys.D6);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_7,		Keys.D7);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_8,		Keys.D8);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_9,		Keys.D9);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_0,		Keys.NumPad0);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_1,		Keys.NumPad1);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_2,		Keys.NumPad2);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_3,		Keys.NumPad3);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_4,		Keys.NumPad4);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_5,		Keys.NumPad5);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_6,		Keys.NumPad6);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_7,		Keys.NumPad7);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_8,		Keys.NumPad8);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_9,		Keys.NumPad9);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR,	Keys.OemClear);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_DECIMAL,	Keys.Decimal);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE,	Keys.Divide);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_ENTER,	Keys.Enter);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS,	Keys.Subtract);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY,	Keys.Multiply);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_PERIOD,	Keys.OemPeriod);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS,	Keys.Add);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F1,		Keys.F1);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F2,		Keys.F2);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F3,		Keys.F3);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F4,		Keys.F4);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F5,		Keys.F5);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F6,		Keys.F6);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F7,		Keys.F7);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F8,		Keys.F8);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F9,		Keys.F9);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F10,		Keys.F10);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F11,		Keys.F11);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F12,		Keys.F12);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F13,		Keys.F13);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F14,		Keys.F14);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F15,		Keys.F15);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F16,		Keys.F16);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F17,		Keys.F17);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F18,		Keys.F18);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F19,		Keys.F19);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F20,		Keys.F20);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F21,		Keys.F21);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F22,		Keys.F22);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F23,		Keys.F23);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_F24,		Keys.F24);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_SPACE,		Keys.Space);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_UP,		Keys.Up);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_DOWN,		Keys.Down);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LEFT,		Keys.Left);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHT,		Keys.Right);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LALT,		Keys.LeftAlt);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RALT,		Keys.RightAlt);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL,		Keys.LeftControl);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RCTRL,		Keys.RightControl);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LGUI,		Keys.LeftWindows);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RGUI,		Keys.RightWindows);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT,	Keys.LeftShift);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT,	Keys.RightShift);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_APPLICATION,	Keys.Apps);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_SLASH,		Keys.OemQuestion);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH,	Keys.OemBackslash);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET,	Keys.OemOpenBrackets);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET,	Keys.OemCloseBrackets);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK,	Keys.CapsLock);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_COMMA,		Keys.OemComma);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_DELETE,	Keys.Delete);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_END,		Keys.End);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE,	Keys.Back);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_RETURN,	Keys.Enter);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE,	Keys.Escape);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_HOME,		Keys.Home);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_INSERT,	Keys.Insert);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_MINUS,		Keys.OemMinus);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR,	Keys.NumLock);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP,	Keys.PageUp);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN,	Keys.PageDown);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_PAUSE,		Keys.Pause);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_PERIOD,	Keys.OemPeriod);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_EQUALS,	Keys.OemPlus);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN,	Keys.PrintScreen);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE,	Keys.OemQuotes);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK,	Keys.Scroll);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON,	Keys.OemSemicolon);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_SLEEP,		Keys.Sleep);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_TAB,		Keys.Tab);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_GRAVE,		Keys.OemTilde);
			INTERNAL_scanMap.Add((int) SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN,	Keys.None);

			// Also, fill up another with the reverse, for scancode->keycode lookups

			INTERNAL_xnaMap.Add((int) Keys.A,			SDL.SDL_Scancode.SDL_SCANCODE_A);
			INTERNAL_xnaMap.Add((int) Keys.B,			SDL.SDL_Scancode.SDL_SCANCODE_B);
			INTERNAL_xnaMap.Add((int) Keys.C,			SDL.SDL_Scancode.SDL_SCANCODE_C);
			INTERNAL_xnaMap.Add((int) Keys.D,			SDL.SDL_Scancode.SDL_SCANCODE_D);
			INTERNAL_xnaMap.Add((int) Keys.E,			SDL.SDL_Scancode.SDL_SCANCODE_E);
			INTERNAL_xnaMap.Add((int) Keys.F,			SDL.SDL_Scancode.SDL_SCANCODE_F);
			INTERNAL_xnaMap.Add((int) Keys.G,			SDL.SDL_Scancode.SDL_SCANCODE_G);
			INTERNAL_xnaMap.Add((int) Keys.H,			SDL.SDL_Scancode.SDL_SCANCODE_H);
			INTERNAL_xnaMap.Add((int) Keys.I,			SDL.SDL_Scancode.SDL_SCANCODE_I);
			INTERNAL_xnaMap.Add((int) Keys.J,			SDL.SDL_Scancode.SDL_SCANCODE_J);
			INTERNAL_xnaMap.Add((int) Keys.K,			SDL.SDL_Scancode.SDL_SCANCODE_K);
			INTERNAL_xnaMap.Add((int) Keys.L,			SDL.SDL_Scancode.SDL_SCANCODE_L);
			INTERNAL_xnaMap.Add((int) Keys.M,			SDL.SDL_Scancode.SDL_SCANCODE_M);
			INTERNAL_xnaMap.Add((int) Keys.N,			SDL.SDL_Scancode.SDL_SCANCODE_N);
			INTERNAL_xnaMap.Add((int) Keys.O,			SDL.SDL_Scancode.SDL_SCANCODE_O);
			INTERNAL_xnaMap.Add((int) Keys.P,			SDL.SDL_Scancode.SDL_SCANCODE_P);
			INTERNAL_xnaMap.Add((int) Keys.Q,			SDL.SDL_Scancode.SDL_SCANCODE_Q);
			INTERNAL_xnaMap.Add((int) Keys.R,			SDL.SDL_Scancode.SDL_SCANCODE_R);
			INTERNAL_xnaMap.Add((int) Keys.S,			SDL.SDL_Scancode.SDL_SCANCODE_S);
			INTERNAL_xnaMap.Add((int) Keys.T,			SDL.SDL_Scancode.SDL_SCANCODE_T);
			INTERNAL_xnaMap.Add((int) Keys.U,			SDL.SDL_Scancode.SDL_SCANCODE_U);
			INTERNAL_xnaMap.Add((int) Keys.V,			SDL.SDL_Scancode.SDL_SCANCODE_V);
			INTERNAL_xnaMap.Add((int) Keys.W,			SDL.SDL_Scancode.SDL_SCANCODE_W);
			INTERNAL_xnaMap.Add((int) Keys.X,			SDL.SDL_Scancode.SDL_SCANCODE_X);
			INTERNAL_xnaMap.Add((int) Keys.Y,			SDL.SDL_Scancode.SDL_SCANCODE_Y);
			INTERNAL_xnaMap.Add((int) Keys.Z,			SDL.SDL_Scancode.SDL_SCANCODE_Z);
			INTERNAL_xnaMap.Add((int) Keys.D0,			SDL.SDL_Scancode.SDL_SCANCODE_0);
			INTERNAL_xnaMap.Add((int) Keys.D1,			SDL.SDL_Scancode.SDL_SCANCODE_1);
			INTERNAL_xnaMap.Add((int) Keys.D2,			SDL.SDL_Scancode.SDL_SCANCODE_2);
			INTERNAL_xnaMap.Add((int) Keys.D3,			SDL.SDL_Scancode.SDL_SCANCODE_3);
			INTERNAL_xnaMap.Add((int) Keys.D4,			SDL.SDL_Scancode.SDL_SCANCODE_4);
			INTERNAL_xnaMap.Add((int) Keys.D5,			SDL.SDL_Scancode.SDL_SCANCODE_5);
			INTERNAL_xnaMap.Add((int) Keys.D6,			SDL.SDL_Scancode.SDL_SCANCODE_6);
			INTERNAL_xnaMap.Add((int) Keys.D7,			SDL.SDL_Scancode.SDL_SCANCODE_7);
			INTERNAL_xnaMap.Add((int) Keys.D8,			SDL.SDL_Scancode.SDL_SCANCODE_8);
			INTERNAL_xnaMap.Add((int) Keys.D9,			SDL.SDL_Scancode.SDL_SCANCODE_9);
			INTERNAL_xnaMap.Add((int) Keys.NumPad0,			SDL.SDL_Scancode.SDL_SCANCODE_KP_0);
			INTERNAL_xnaMap.Add((int) Keys.NumPad1,			SDL.SDL_Scancode.SDL_SCANCODE_KP_1);
			INTERNAL_xnaMap.Add((int) Keys.NumPad2,			SDL.SDL_Scancode.SDL_SCANCODE_KP_2);
			INTERNAL_xnaMap.Add((int) Keys.NumPad3,			SDL.SDL_Scancode.SDL_SCANCODE_KP_3);
			INTERNAL_xnaMap.Add((int) Keys.NumPad4,			SDL.SDL_Scancode.SDL_SCANCODE_KP_4);
			INTERNAL_xnaMap.Add((int) Keys.NumPad5,			SDL.SDL_Scancode.SDL_SCANCODE_KP_5);
			INTERNAL_xnaMap.Add((int) Keys.NumPad6,			SDL.SDL_Scancode.SDL_SCANCODE_KP_6);
			INTERNAL_xnaMap.Add((int) Keys.NumPad7,			SDL.SDL_Scancode.SDL_SCANCODE_KP_7);
			INTERNAL_xnaMap.Add((int) Keys.NumPad8,			SDL.SDL_Scancode.SDL_SCANCODE_KP_8);
			INTERNAL_xnaMap.Add((int) Keys.NumPad9,			SDL.SDL_Scancode.SDL_SCANCODE_KP_9);
			INTERNAL_xnaMap.Add((int) Keys.OemClear,		SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR);
			INTERNAL_xnaMap.Add((int) Keys.Decimal,			SDL.SDL_Scancode.SDL_SCANCODE_KP_DECIMAL);
			INTERNAL_xnaMap.Add((int) Keys.Divide,			SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE);
			INTERNAL_xnaMap.Add((int) Keys.Multiply,		SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY);
			INTERNAL_xnaMap.Add((int) Keys.Subtract,		SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS);
			INTERNAL_xnaMap.Add((int) Keys.Add,			SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS);
			INTERNAL_xnaMap.Add((int) Keys.F1,			SDL.SDL_Scancode.SDL_SCANCODE_F1);
			INTERNAL_xnaMap.Add((int) Keys.F2,			SDL.SDL_Scancode.SDL_SCANCODE_F2);
			INTERNAL_xnaMap.Add((int) Keys.F3,			SDL.SDL_Scancode.SDL_SCANCODE_F3);
			INTERNAL_xnaMap.Add((int) Keys.F4,			SDL.SDL_Scancode.SDL_SCANCODE_F4);
			INTERNAL_xnaMap.Add((int) Keys.F5,			SDL.SDL_Scancode.SDL_SCANCODE_F5);
			INTERNAL_xnaMap.Add((int) Keys.F6,			SDL.SDL_Scancode.SDL_SCANCODE_F6);
			INTERNAL_xnaMap.Add((int) Keys.F7,			SDL.SDL_Scancode.SDL_SCANCODE_F7);
			INTERNAL_xnaMap.Add((int) Keys.F8,			SDL.SDL_Scancode.SDL_SCANCODE_F8);
			INTERNAL_xnaMap.Add((int) Keys.F9,			SDL.SDL_Scancode.SDL_SCANCODE_F9);
			INTERNAL_xnaMap.Add((int) Keys.F10,			SDL.SDL_Scancode.SDL_SCANCODE_F10);
			INTERNAL_xnaMap.Add((int) Keys.F11,			SDL.SDL_Scancode.SDL_SCANCODE_F11);
			INTERNAL_xnaMap.Add((int) Keys.F12,			SDL.SDL_Scancode.SDL_SCANCODE_F12);
			INTERNAL_xnaMap.Add((int) Keys.F13,			SDL.SDL_Scancode.SDL_SCANCODE_F13);
			INTERNAL_xnaMap.Add((int) Keys.F14,			SDL.SDL_Scancode.SDL_SCANCODE_F14);
			INTERNAL_xnaMap.Add((int) Keys.F15,			SDL.SDL_Scancode.SDL_SCANCODE_F15);
			INTERNAL_xnaMap.Add((int) Keys.F16,			SDL.SDL_Scancode.SDL_SCANCODE_F16);
			INTERNAL_xnaMap.Add((int) Keys.F17,			SDL.SDL_Scancode.SDL_SCANCODE_F17);
			INTERNAL_xnaMap.Add((int) Keys.F18,			SDL.SDL_Scancode.SDL_SCANCODE_F18);
			INTERNAL_xnaMap.Add((int) Keys.F19,			SDL.SDL_Scancode.SDL_SCANCODE_F19);
			INTERNAL_xnaMap.Add((int) Keys.F20,			SDL.SDL_Scancode.SDL_SCANCODE_F20);
			INTERNAL_xnaMap.Add((int) Keys.F21,			SDL.SDL_Scancode.SDL_SCANCODE_F21);
			INTERNAL_xnaMap.Add((int) Keys.F22,			SDL.SDL_Scancode.SDL_SCANCODE_F22);
			INTERNAL_xnaMap.Add((int) Keys.F23,			SDL.SDL_Scancode.SDL_SCANCODE_F23);
			INTERNAL_xnaMap.Add((int) Keys.F24,			SDL.SDL_Scancode.SDL_SCANCODE_F24);
			INTERNAL_xnaMap.Add((int) Keys.Space,			SDL.SDL_Scancode.SDL_SCANCODE_SPACE);
			INTERNAL_xnaMap.Add((int) Keys.Up,			SDL.SDL_Scancode.SDL_SCANCODE_UP);
			INTERNAL_xnaMap.Add((int) Keys.Down,			SDL.SDL_Scancode.SDL_SCANCODE_DOWN);
			INTERNAL_xnaMap.Add((int) Keys.Left,			SDL.SDL_Scancode.SDL_SCANCODE_LEFT);
			INTERNAL_xnaMap.Add((int) Keys.Right,			SDL.SDL_Scancode.SDL_SCANCODE_RIGHT);
			INTERNAL_xnaMap.Add((int) Keys.LeftAlt,			SDL.SDL_Scancode.SDL_SCANCODE_LALT);
			INTERNAL_xnaMap.Add((int) Keys.RightAlt,		SDL.SDL_Scancode.SDL_SCANCODE_RALT);
			INTERNAL_xnaMap.Add((int) Keys.LeftControl,		SDL.SDL_Scancode.SDL_SCANCODE_LCTRL);
			INTERNAL_xnaMap.Add((int) Keys.RightControl,		SDL.SDL_Scancode.SDL_SCANCODE_RCTRL);
			INTERNAL_xnaMap.Add((int) Keys.LeftWindows,		SDL.SDL_Scancode.SDL_SCANCODE_LGUI);
			INTERNAL_xnaMap.Add((int) Keys.RightWindows,		SDL.SDL_Scancode.SDL_SCANCODE_RGUI);
			INTERNAL_xnaMap.Add((int) Keys.LeftShift,		SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT);
			INTERNAL_xnaMap.Add((int) Keys.RightShift,		SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT);
			INTERNAL_xnaMap.Add((int) Keys.Apps,			SDL.SDL_Scancode.SDL_SCANCODE_APPLICATION);
			INTERNAL_xnaMap.Add((int) Keys.OemQuestion,		SDL.SDL_Scancode.SDL_SCANCODE_SLASH);
			INTERNAL_xnaMap.Add((int) Keys.OemBackslash,		SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH);
			INTERNAL_xnaMap.Add((int) Keys.OemOpenBrackets,		SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET);
			INTERNAL_xnaMap.Add((int) Keys.OemCloseBrackets,	SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET);
			INTERNAL_xnaMap.Add((int) Keys.CapsLock,		SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK);
			INTERNAL_xnaMap.Add((int) Keys.OemComma,		SDL.SDL_Scancode.SDL_SCANCODE_COMMA);
			INTERNAL_xnaMap.Add((int) Keys.Delete,			SDL.SDL_Scancode.SDL_SCANCODE_DELETE);
			INTERNAL_xnaMap.Add((int) Keys.End,			SDL.SDL_Scancode.SDL_SCANCODE_END);
			INTERNAL_xnaMap.Add((int) Keys.Back,			SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE);
			INTERNAL_xnaMap.Add((int) Keys.Enter,			SDL.SDL_Scancode.SDL_SCANCODE_RETURN);
			INTERNAL_xnaMap.Add((int) Keys.Escape,			SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE);
			INTERNAL_xnaMap.Add((int) Keys.Home,			SDL.SDL_Scancode.SDL_SCANCODE_HOME);
			INTERNAL_xnaMap.Add((int) Keys.Insert,			SDL.SDL_Scancode.SDL_SCANCODE_INSERT);
			INTERNAL_xnaMap.Add((int) Keys.OemMinus,		SDL.SDL_Scancode.SDL_SCANCODE_MINUS);
			INTERNAL_xnaMap.Add((int) Keys.NumLock,			SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR);
			INTERNAL_xnaMap.Add((int) Keys.PageUp,			SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP);
			INTERNAL_xnaMap.Add((int) Keys.PageDown,		SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN);
			INTERNAL_xnaMap.Add((int) Keys.Pause,			SDL.SDL_Scancode.SDL_SCANCODE_PAUSE);
			INTERNAL_xnaMap.Add((int) Keys.OemPeriod,		SDL.SDL_Scancode.SDL_SCANCODE_PERIOD);
			INTERNAL_xnaMap.Add((int) Keys.OemPlus,			SDL.SDL_Scancode.SDL_SCANCODE_EQUALS);
			INTERNAL_xnaMap.Add((int) Keys.PrintScreen,		SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN);
			INTERNAL_xnaMap.Add((int) Keys.OemQuotes,		SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE);
			INTERNAL_xnaMap.Add((int) Keys.Scroll,			SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK);
			INTERNAL_xnaMap.Add((int) Keys.OemSemicolon,		SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON);
			INTERNAL_xnaMap.Add((int) Keys.Sleep,			SDL.SDL_Scancode.SDL_SCANCODE_SLEEP);
			INTERNAL_xnaMap.Add((int) Keys.Tab,			SDL.SDL_Scancode.SDL_SCANCODE_TAB);
			INTERNAL_xnaMap.Add((int) Keys.OemTilde,		SDL.SDL_Scancode.SDL_SCANCODE_GRAVE);
			INTERNAL_xnaMap.Add((int) Keys.None,			SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN);
		}

		#endregion

		#region Public SDL2<->XNA Key Conversion Methods

		public static Keys ToXNA(SDL.SDL_Keycode key)
		{
			Keys retVal;
			if (INTERNAL_keyMap.TryGetValue((int) key, out retVal))
			{
				return retVal;
			}
			else
			{
				System.Console.WriteLine("KEY MISSING FROM SDL2->XNA DICTIONARY: " + key.ToString());
				return Keys.None;
			}
		}

		public static Keys ToXNA(SDL.SDL_Scancode key)
		{
			Keys retVal;
			if (INTERNAL_scanMap.TryGetValue((int) key, out retVal))
			{
				return retVal;
			}
			else
			{
				System.Console.WriteLine("SCANCODE MISSING FROM SDL2->XNA DICTIONARY: " + key.ToString());
				return Keys.None;
			}
		}

		public static Keys KeyFromScancode(Keys scancode)
		{
			SDL.SDL_Scancode retVal;
			if (INTERNAL_xnaMap.TryGetValue((int) scancode, out retVal))
			{
				return INTERNAL_keyMap[(int) SDL.SDL_GetKeyFromScancode(retVal)];
			}
			else
			{
				System.Console.WriteLine("SCANCODE MISSING FROM XNA->SDL2 DICTIONARY: " + scancode.ToString());
				return Keys.None;
			}
		}

		#endregion
	}
}

