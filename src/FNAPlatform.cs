#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class FNAPlatform
	{
		#region Static Constructor

		static FNAPlatform()
		{
			/* I suspect you may have an urge to put an #if in here for new
			 * FNAPlatform implementations.
			 *
			 * DON'T.
			 *
			 * Determine this at runtime, or load dynamically.
			 * No amount of whining will get me to budge on this.
			 * -flibit
			 */

			// Environment.GetEnvironmentVariable("FNA_PLATFORM_BACKEND");

			CreateWindow =			SDL2_FNAPlatform.CreateWindow;
			DisposeWindow =			SDL2_FNAPlatform.DisposeWindow;
			BeforeInitialize =		SDL2_FNAPlatform.BeforeInitialize;
			RunLoop =			SDL2_FNAPlatform.RunLoop;
			CreateGLDevice =		SDL2_FNAPlatform.CreateGLDevice;
			CreateALDevice =		SDL2_FNAPlatform.CreateALDevice;
			SetPresentationInterval =	SDL2_FNAPlatform.SetPresentationInterval;
			GetGraphicsAdapters =		SDL2_FNAPlatform.GetGraphicsAdapters;
			GetKeyFromScancode =		SDL2_KeyboardUtil.GetKeyFromScancode;
			StartTextInput =		SDL2.SDL.SDL_StartTextInput;
			StopTextInput =			SDL2.SDL.SDL_StopTextInput;
			GetMouseState =			SDL2_FNAPlatform.GetMouseState;
			SetMousePosition =		SDL2.SDL.SDL_WarpMouseInWindow;
			OnIsMouseVisibleChanged =	SDL2_FNAPlatform.OnIsMouseVisibleChanged;
			GetGamePadCapabilities =	SDL2_FNAPlatform.GetGamePadCapabilities;
			GetGamePadState =		SDL2_FNAPlatform.GetGamePadState;
			SetGamePadVibration =		SDL2_FNAPlatform.SetGamePadVibration;
			GetGamePadGUID =		SDL2_FNAPlatform.GetGamePadGUID;
			SetGamePadLightBar =		SDL2_FNAPlatform.SetGamePadLightBar;
			GetStorageRoot =		SDL2_FNAPlatform.GetStorageRoot;
			IsStoragePathConnected =	SDL2_FNAPlatform.IsStoragePathConnected;
			ShowRuntimeError =		SDL2_FNAPlatform.ShowRuntimeError;
			TextureDataFromStream =		SDL2_FNAPlatform.TextureDataFromStream;
			SavePNG =			SDL2_FNAPlatform.SavePNG;

			Log = Console.WriteLine;

			AppDomain.CurrentDomain.ProcessExit += SDL2_FNAPlatform.ProgramExit;
			SDL2_FNAPlatform.ProgramInit();
		}

		#endregion

		#region Public Static Methods

		public static void UnhookLogger()
		{
			Log = Console.WriteLine;
		}

		public static Action<string> Log;

		public delegate GameWindow CreateWindowFunc();
		public static readonly CreateWindowFunc CreateWindow;

		public delegate void DisposeWindowFunc(GameWindow window);
		public static readonly DisposeWindowFunc DisposeWindow;

		public delegate void BeforeInitializeFunc();
		public static readonly BeforeInitializeFunc BeforeInitialize;

		public delegate void RunLoopFunc(Game game);
		public static readonly RunLoopFunc RunLoop;

		public delegate IGLDevice CreateGLDeviceFunc(
			PresentationParameters presentationParameters
		);
		public static readonly CreateGLDeviceFunc CreateGLDevice;

		public delegate IALDevice CreateALDeviceFunc();
		public static readonly CreateALDeviceFunc CreateALDevice;

		public delegate void SetPresentationIntervalFunc(PresentInterval interval);
		public static readonly SetPresentationIntervalFunc SetPresentationInterval;

		public delegate GraphicsAdapter[] GetGraphicsAdaptersFunc();
		public static readonly GetGraphicsAdaptersFunc GetGraphicsAdapters;

		public delegate Keys GetKeyFromScancodeFunc(Keys scancode);
		public static readonly GetKeyFromScancodeFunc GetKeyFromScancode;

		public delegate void StartTextInputFunc();
		public static readonly StartTextInputFunc StartTextInput;

		public delegate void StopTextInputFunc();
		public static readonly StopTextInputFunc StopTextInput;

		public delegate void GetMouseStateFunc(
			out int x,
			out int y,
			out ButtonState left,
			out ButtonState middle,
			out ButtonState right,
			out ButtonState x1,
			out ButtonState x2
		);
		public static readonly GetMouseStateFunc GetMouseState;

		public delegate void SetMousePositionFunc(
			IntPtr window,
			int x,
			int y
		);
		public static readonly SetMousePositionFunc SetMousePosition;

		public delegate void OnIsMouseVisibleChangedFunc(bool visible);
		public static readonly OnIsMouseVisibleChangedFunc OnIsMouseVisibleChanged;

		public delegate GamePadCapabilities GetGamePadCapabilitiesFunc(int index);
		public static readonly GetGamePadCapabilitiesFunc GetGamePadCapabilities;

		public delegate GamePadState GetGamePadStateFunc(
			int index,
			GamePadDeadZone deadZoneMode
		);
		public static readonly GetGamePadStateFunc GetGamePadState;

		public delegate bool SetGamePadVibrationFunc(
			int index,
			float leftMotor,
			float rightMotor
		);
		public static readonly SetGamePadVibrationFunc SetGamePadVibration;

		public delegate string GetGamePadGUIDFunc(int index);
		public static readonly GetGamePadGUIDFunc GetGamePadGUID;

		public delegate void SetGamePadLightBarFunc(int index, Color color);
		public static readonly SetGamePadLightBarFunc SetGamePadLightBar;

		public delegate string GetStorageRootFunc();
		public static readonly GetStorageRootFunc GetStorageRoot;

		public delegate bool IsStoragePathConnectedFunc(string path);
		public static readonly IsStoragePathConnectedFunc IsStoragePathConnected;

		public delegate void ShowRuntimeErrorFunc(string title, string message);
		public static readonly ShowRuntimeErrorFunc ShowRuntimeError;

		public delegate void TextureDataFromStreamFunc(
			Stream stream,
			out int width,
			out int height,
			out byte[] pixels,
			int reqWidth = -1,
			int reqHeight = -1,
			bool zoom = false
		);
		public static readonly TextureDataFromStreamFunc TextureDataFromStream;

		public delegate void SavePNGFunc(
			Stream stream,
			int width,
			int height,
			int imgWidth,
			int imgHeight,
			byte[] data
		);
		public static readonly SavePNGFunc SavePNG;

		#endregion
	}
}
