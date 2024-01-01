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
using System.IO;
using System.Collections.Generic;

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

			SetEnv = SDL2_FNAPlatform.SetEnv;

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
				SetEnv(
					"FNA3D_FORCE_DRIVER",
					arg
				);
			}
			if (args.TryGetValue("enablelateswaptear", out arg) && arg == "1")
			{
				SetEnv(
					"FNA3D_ENABLE_LATESWAPTEAR",
					"1"
				);
			}
			if (args.TryGetValue("mojoshaderprofile", out arg))
			{
				SetEnv(
					"FNA3D_MOJOSHADER_PROFILE",
					arg
				);
			}
			if (args.TryGetValue("backbufferscalenearest", out arg) && arg == "1")
			{
				SetEnv(
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
			if (args.TryGetValue("nukesteaminput", out arg) && arg == "1")
			{
				Environment.SetEnvironmentVariable(
					"FNA_NUKE_STEAM_INPUT",
					"1"
				);
			}

			Malloc =			SDL2_FNAPlatform.Malloc;
			Free =				SDL2.SDL.SDL_free;
			CreateWindow =			SDL2_FNAPlatform.CreateWindow;
			DisposeWindow =			SDL2_FNAPlatform.DisposeWindow;
			ApplyWindowChanges =		SDL2_FNAPlatform.ApplyWindowChanges;
			ScaleForWindow =		SDL2_FNAPlatform.ScaleForWindow;
			GetWindowBounds =		SDL2_FNAPlatform.GetWindowBounds;
			GetWindowResizable =		SDL2_FNAPlatform.GetWindowResizable;
			SetWindowResizable =		SDL2_FNAPlatform.SetWindowResizable;
			GetWindowBorderless =		SDL2_FNAPlatform.GetWindowBorderless;
			SetWindowBorderless =		SDL2_FNAPlatform.SetWindowBorderless;
			SetWindowTitle =		SDL2_FNAPlatform.SetWindowTitle;
			IsScreenKeyboardShown =		SDL2_FNAPlatform.IsScreenKeyboardShown;
			RegisterGame =			SDL2_FNAPlatform.RegisterGame;
			UnregisterGame =		SDL2_FNAPlatform.UnregisterGame;
			PollEvents =			SDL2_FNAPlatform.PollEvents;
			GetGraphicsAdapters =		SDL2_FNAPlatform.GetGraphicsAdapters;
			GetCurrentDisplayMode =		SDL2_FNAPlatform.GetCurrentDisplayMode;
			GetKeyFromScancode =		SDL2_FNAPlatform.GetKeyFromScancode;
			IsTextInputActive =		SDL2_FNAPlatform.IsTextInputActive;
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
			SetGamePadTriggerVibration =	SDL2_FNAPlatform.SetGamePadTriggerVibration;
			GetGamePadGUID =		SDL2_FNAPlatform.GetGamePadGUID;
			SetGamePadLightBar =		SDL2_FNAPlatform.SetGamePadLightBar;
			GetGamePadGyro = 		SDL2_FNAPlatform.GetGamePadGyro;
			GetGamePadAccelerometer =	SDL2_FNAPlatform.GetGamePadAccelerometer;
			GetStorageRoot =		SDL2_FNAPlatform.GetStorageRoot;
			GetDriveInfo =			SDL2_FNAPlatform.GetDriveInfo;
			ReadFileToPointer =		SDL2_FNAPlatform.ReadToPointer;
			FreeFilePointer =		SDL2_FNAPlatform.FreeFilePointer;
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
			NeedsPlatformMainLoop = 	SDL2_FNAPlatform.NeedsPlatformMainLoop;
			RunPlatformMainLoop =		SDL2_FNAPlatform.RunPlatformMainLoop;

			FNALoggerEXT.Initialize();

			AppDomain.CurrentDomain.ProcessExit += SDL2_FNAPlatform.ProgramExit;
			TitleLocation = SDL2_FNAPlatform.ProgramInit(args);

			/* Do this AFTER ProgramInit so the platform library
			 * has a chance to load first!
			 */
			FNALoggerEXT.HookFNA3D();
		}

		#endregion

		#region Public Static Variables

		public static readonly string TitleLocation;

		/* Setup Text Input Control Character Arrays
		 * (Only 7 control keys supported at this time)
		 */
		public static readonly char[] TextInputCharacters = new char[]
		{
			(char) 2,	// Home
			(char) 3,	// End
			(char) 8,	// Backspace
			(char) 9,	// Tab
			(char) 13,	// Enter
			(char) 127,	// Delete
			(char) 22	// Ctrl+V (Paste)
		};
		public static readonly Dictionary<Keys, int> TextInputBindings = new Dictionary<Keys, int>()
		{
			{ Keys.Home,	0 },
			{ Keys.End,	1 },
			{ Keys.Back,	2 },
			{ Keys.Tab,	3 },
			{ Keys.Enter,	4 },
			{ Keys.Delete,	5 }
			// Ctrl+V is special!
		};

		#endregion

		#region Public Static Methods

		/* Technically this should be IntPtr, but oh well... */
		public delegate IntPtr MallocFunc(int size);
		public static readonly MallocFunc Malloc;

		public delegate void FreeFunc(IntPtr ptr);
		public static readonly FreeFunc Free;

		public delegate void SetEnvFunc(string name, string value);
		public static readonly SetEnvFunc SetEnv;

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

		public delegate void ScaleForWindowFunc(IntPtr window, bool invert, ref int w, ref int h);
		public static readonly ScaleForWindowFunc ScaleForWindow;

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

		public delegate bool IsScreenKeyboardShownFunc(IntPtr window);
		public static readonly IsScreenKeyboardShownFunc IsScreenKeyboardShown;

		public delegate GraphicsAdapter RegisterGameFunc(Game game);
		public static readonly RegisterGameFunc RegisterGame;

		public delegate void UnregisterGameFunc(Game game);
		public static readonly UnregisterGameFunc UnregisterGame;

		public delegate void PollEventsFunc(
			Game game,
			ref GraphicsAdapter currentAdapter,
			bool[] textInputControlDown,
			ref bool textInputSuppress
		);
		public static readonly PollEventsFunc PollEvents;

		public delegate GraphicsAdapter[] GetGraphicsAdaptersFunc();
		public static readonly GetGraphicsAdaptersFunc GetGraphicsAdapters;

		public delegate DisplayMode GetCurrentDisplayModeFunc(int adapterIndex);
		public static readonly GetCurrentDisplayModeFunc GetCurrentDisplayMode;

		public delegate Keys GetKeyFromScancodeFunc(Keys scancode);
		public static readonly GetKeyFromScancodeFunc GetKeyFromScancode;

		public delegate bool IsTextInputActiveFunc();
		public static readonly IsTextInputActiveFunc IsTextInputActive;

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

		public delegate bool SetGamePadTriggerVibrationFunc(
			int index,
			float leftTrigger,
			float rightTrigger
		);
		public static readonly SetGamePadTriggerVibrationFunc SetGamePadTriggerVibration;

		public delegate string GetGamePadGUIDFunc(int index);
		public static readonly GetGamePadGUIDFunc GetGamePadGUID;

		public delegate void SetGamePadLightBarFunc(int index, Color color);
		public static readonly SetGamePadLightBarFunc SetGamePadLightBar;

		public delegate bool GetGamePadGyroFunc(int index, out Vector3 gyro);
		public static readonly GetGamePadGyroFunc GetGamePadGyro;

		public delegate bool GetGamePadAccelerometerFunc(int index, out Vector3 accel);
		public static readonly GetGamePadAccelerometerFunc GetGamePadAccelerometer;

		public delegate string GetStorageRootFunc();
		public static readonly GetStorageRootFunc GetStorageRoot;

		public delegate DriveInfo GetDriveInfoFunc(string storageRoot);
		public static readonly GetDriveInfoFunc GetDriveInfo;

		public delegate IntPtr ReadFileToPointerFunc(string path, out IntPtr size);
		public static readonly ReadFileToPointerFunc ReadFileToPointer;

		public delegate void FreeFilePointerFunc(IntPtr file);
		public static readonly FreeFilePointerFunc FreeFilePointer;

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

		public delegate bool NeedsPlatformMainLoopFunc();
		public static readonly NeedsPlatformMainLoopFunc NeedsPlatformMainLoop;

		public delegate void RunPlatformMainLoopFunc(Game game);
		public static readonly RunPlatformMainLoopFunc RunPlatformMainLoop;

		#endregion
	}
}
