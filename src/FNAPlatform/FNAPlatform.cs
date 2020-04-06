#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
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
using Microsoft.Xna.Framework.Input.Touch;
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

			// Built-in command line arguments
			LaunchParameters args = new LaunchParameters();
			string arg;
			if (args.TryGetValue("enablehighdpi", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA_GRAPHICS_ENABLE_HIGHDPI",
					"1"
				);
			}
			if (args.TryGetValue("gldevice", out arg))
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_FORCE_DRIVER",
					arg
				);
			}
			if (args.TryGetValue("mojoshaderprofile", out arg))
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_MOJOSHADER_PROFILE",
					arg
				);
			}
			if (args.TryGetValue("backbufferscalenearest", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA3D_BACKBUFFER_SCALE_NEAREST",
					"1"
				);
			}
			if (args.TryGetValue("usescancodes", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA_KEYBOARD_USE_SCANCODES",
					"1"
				);
			}

			CreateWindow =			SDL2_FNAPlatform.CreateWindow;
			DisposeWindow =			SDL2_FNAPlatform.DisposeWindow;
			ApplyWindowChanges =		SDL2_FNAPlatform.ApplyWindowChanges;
			GetWindowBounds =		SDL2_FNAPlatform.GetWindowBounds;
			GetWindowResizable =		SDL2_FNAPlatform.GetWindowResizable;
			SetWindowResizable =		SDL2_FNAPlatform.SetWindowResizable;
			GetWindowBorderless =		SDL2_FNAPlatform.GetWindowBorderless;
			SetWindowBorderless =		SDL2_FNAPlatform.SetWindowBorderless;
			SetWindowTitle =		SDL2_FNAPlatform.SetWindowTitle;
			RunLoop =			SDL2_FNAPlatform.RunLoop;
			GetGraphicsAdapters =		SDL2_FNAPlatform.GetGraphicsAdapters;
			GetCurrentDisplayMode =		SDL2_FNAPlatform.GetCurrentDisplayMode;
			GetKeyFromScancode =		SDL2_FNAPlatform.GetKeyFromScancode;
			StartTextInput =		SDL2.SDL.SDL_StartTextInput;
			StopTextInput =			SDL2.SDL.SDL_StopTextInput;
			SetTextInputRectangle =		SDL2_FNAPlatform.SetTextInputRectangle;
			GetMouseState =			SDL2_FNAPlatform.GetMouseState;
			SetMousePosition =		SDL2.SDL.SDL_WarpMouseInWindow;
			OnIsMouseVisibleChanged =	SDL2_FNAPlatform.OnIsMouseVisibleChanged;
			GetRelativeMouseMode =		SDL2_FNAPlatform.GetRelativeMouseMode;
			SetRelativeMouseMode =		SDL2_FNAPlatform.SetRelativeMouseMode;
			GetGamePadCapabilities =	SDL2_FNAPlatform.GetGamePadCapabilities;
			GetGamePadState =		SDL2_FNAPlatform.GetGamePadState;
			SetGamePadVibration =		SDL2_FNAPlatform.SetGamePadVibration;
			GetGamePadGUID =		SDL2_FNAPlatform.GetGamePadGUID;
			SetGamePadLightBar =		SDL2_FNAPlatform.SetGamePadLightBar;
			GetStorageRoot =		SDL2_FNAPlatform.GetStorageRoot;
			GetDriveInfo =			SDL2_FNAPlatform.GetDriveInfo;
			ShowRuntimeError =		SDL2_FNAPlatform.ShowRuntimeError;
			GetMicrophones =		SDL2_FNAPlatform.GetMicrophones;
			GetMicrophoneSamples =		SDL2_FNAPlatform.GetMicrophoneSamples;
			GetMicrophoneQueuedBytes =	SDL2_FNAPlatform.GetMicrophoneQueuedBytes;
			StartMicrophone =		SDL2_FNAPlatform.StartMicrophone;
			StopMicrophone =		SDL2_FNAPlatform.StopMicrophone;
			GetTouchCapabilities =		SDL2_FNAPlatform.GetTouchCapabilities;
			UpdateTouchPanelState =		SDL2_FNAPlatform.UpdateTouchPanelState;
			GetNumTouchFingers =		SDL2_FNAPlatform.GetNumTouchFingers;
			SupportsOrientationChanges =	SDL2_FNAPlatform.SupportsOrientationChanges;

			FNALoggerEXT.Initialize();

			AppDomain.CurrentDomain.ProcessExit += SDL2_FNAPlatform.ProgramExit;
			TitleLocation = SDL2_FNAPlatform.ProgramInit(args);
		}

		#endregion

		#region Public Static Variables

		public static readonly string TitleLocation;

		#endregion

		#region Public Static Methods

		public delegate GameWindow CreateWindowFunc();
		public static readonly CreateWindowFunc CreateWindow;

		public delegate void DisposeWindowFunc(GameWindow window);
		public static readonly DisposeWindowFunc DisposeWindow;

		public delegate void ApplyWindowChangesFunc(
			IntPtr window,
			int clientWidth,
			int clientHeight,
			bool wantsFullscreen,
			string screenDeviceName,
			ref string resultDeviceName
		);
		public static readonly ApplyWindowChangesFunc ApplyWindowChanges;

		public delegate Rectangle GetWindowBoundsFunc(IntPtr window);
		public static readonly GetWindowBoundsFunc GetWindowBounds;

		public delegate bool GetWindowResizableFunc(IntPtr window);
		public static readonly GetWindowResizableFunc GetWindowResizable;

		public delegate void SetWindowResizableFunc(IntPtr window, bool resizable);
		public static readonly SetWindowResizableFunc SetWindowResizable;

		public delegate bool GetWindowBorderlessFunc(IntPtr window);
		public static readonly GetWindowBorderlessFunc GetWindowBorderless;

		public delegate void SetWindowBorderlessFunc(IntPtr window, bool borderless);
		public static readonly SetWindowBorderlessFunc SetWindowBorderless;

		public delegate void SetWindowTitleFunc(IntPtr window, string title);
		public static readonly SetWindowTitleFunc SetWindowTitle;

		public delegate void RunLoopFunc(Game game);
		public static readonly RunLoopFunc RunLoop;

		public delegate GraphicsAdapter[] GetGraphicsAdaptersFunc();
		public static readonly GetGraphicsAdaptersFunc GetGraphicsAdapters;

		public delegate DisplayMode GetCurrentDisplayModeFunc(int adapterIndex);
		public static readonly GetCurrentDisplayModeFunc GetCurrentDisplayMode;

		public delegate Keys GetKeyFromScancodeFunc(Keys scancode);
		public static readonly GetKeyFromScancodeFunc GetKeyFromScancode;

		public delegate void StartTextInputFunc();
		public static readonly StartTextInputFunc StartTextInput;

		public delegate void StopTextInputFunc();
		public static readonly StopTextInputFunc StopTextInput;

		public delegate void SetTextInputRectangleFunc(Rectangle rectangle);
		public static readonly SetTextInputRectangleFunc SetTextInputRectangle;

		public delegate void GetMouseStateFunc(
			IntPtr window,
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

		public delegate bool GetRelativeMouseModeFunc();
		public static readonly GetRelativeMouseModeFunc GetRelativeMouseMode;

		public delegate void SetRelativeMouseModeFunc(bool enable);
		public static readonly SetRelativeMouseModeFunc SetRelativeMouseMode;

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

		public delegate DriveInfo GetDriveInfoFunc(string storageRoot);
		public static readonly GetDriveInfoFunc GetDriveInfo;

		public delegate void ShowRuntimeErrorFunc(string title, string message);
		public static readonly ShowRuntimeErrorFunc ShowRuntimeError;

		public delegate Microphone[] GetMicrophonesFunc();
		public static readonly GetMicrophonesFunc GetMicrophones;

		public delegate int GetMicrophoneSamplesFunc(
			uint handle,
			byte[] buffer,
			int offset,
			int count
		);
		public static readonly GetMicrophoneSamplesFunc GetMicrophoneSamples;

		public delegate int GetMicrophoneQueuedBytesFunc(uint handle);
		public static readonly GetMicrophoneQueuedBytesFunc GetMicrophoneQueuedBytes;

		public delegate void StartMicrophoneFunc(uint handle);
		public static readonly StartMicrophoneFunc StartMicrophone;

		public delegate void StopMicrophoneFunc(uint handle);
		public static readonly StopMicrophoneFunc StopMicrophone;

		public delegate TouchPanelCapabilities GetTouchCapabilitiesFunc();
		public static readonly GetTouchCapabilitiesFunc GetTouchCapabilities;

		public delegate void UpdateTouchPanelStateFunc();
		public static readonly UpdateTouchPanelStateFunc UpdateTouchPanelState;

		public delegate int GetNumTouchFingersFunc();
		public static readonly GetNumTouchFingersFunc GetNumTouchFingers;

		public delegate bool SupportsOrientationChangesFunc();
		public static readonly SupportsOrientationChangesFunc SupportsOrientationChanges;

		#endregion
	}
}
