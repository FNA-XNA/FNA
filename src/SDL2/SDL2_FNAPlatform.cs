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
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SDL2;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class SDL2_FNAPlatform
	{
		#region Static Constants

		private static readonly string OSVersion = SDL.SDL_GetPlatform();

		#endregion

		#region Public Static Methods

		public static void ProgramInit()
		{
			/* SDL2 might complain if an OS that uses SDL_main has not actually
			 * used SDL_main by the time you initialize SDL2.
			 * The only platform that is affected is Windows, but we can skip
			 * their WinMain. This was only added to prevent iOS from exploding.
			 * -flibit
			 */
			SDL.SDL_SetMainReady();

			// This _should_ be the first real SDL call we make...
			SDL.SDL_Init(
				SDL.SDL_INIT_VIDEO |
				SDL.SDL_INIT_JOYSTICK |
				SDL.SDL_INIT_GAMECONTROLLER |
				SDL.SDL_INIT_HAPTIC
			);

			// Set any hints to match XNA4 behavior...
			string hint = SDL.SDL_GetHint(SDL.SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS);
			if (String.IsNullOrEmpty(hint))
			{
				SDL.SDL_SetHint(
					SDL.SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS,
					"1"
				);
			}

			// If available, load the SDL_GameControllerDB
			string mappingsDB = Path.Combine(
				TitleContainer.Location,
				"gamecontrollerdb.txt"
			);
			if (File.Exists(mappingsDB))
			{
				SDL.SDL_GameControllerAddMappingsFromFile(
					mappingsDB
				);
			}
		}

		public static void ProgramExit(object sender, EventArgs e)
		{
			// This _should_ be the last SDL call we make...
			SDL.SDL_Quit();
		}

		public static GameWindow CreateWindow()
		{
			// GLContext environment variables
			bool forceES2 = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_ES2"
			) == "1";
			bool forceCoreProfile = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_CORE_PROFILE"
			) == "1";

			// Set and initialize the SDL2 window
			GameWindow result = new SDL2_GameWindow(
				forceES2 ||
				OSVersion.Equals("Emscripten") ||
				OSVersion.Equals("Android") ||
				OSVersion.Equals("iOS"),
				forceCoreProfile
			);

			// Disable the screensaver.
			SDL.SDL_DisableScreenSaver();

			// We hide the mouse cursor by default.
			SDL.SDL_ShowCursor(0);

			return result;
		}

		public static void DisposeWindow(GameWindow window)
		{
			/* Some window managers might try to minimize the window as we're
			 * destroying it. This looks pretty stupid and could cause problems,
			 * so set this hint right before we destroy everything.
			 * -flibit
			 */
			SDL.SDL_SetHintWithPriority(
				SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS,
				"0",
				SDL.SDL_HintPriority.SDL_HINT_OVERRIDE
			);

			SDL.SDL_DestroyWindow(window.Handle);
		}

		public static void RunLoop(Game game)
		{
			SDL.SDL_ShowWindow(game.Window.Handle);

			// Which display did we end up on?
			int displayIndex = SDL.SDL_GetWindowDisplayIndex(
				game.Window.Handle
			);

			// OSX has some fancy fullscreen features, let's use them!
			bool osxUseSpaces;
			if (OSVersion.Equals("Mac OS X"))
			{
				string hint = SDL.SDL_GetHint(SDL.SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES);
				osxUseSpaces = (String.IsNullOrEmpty(hint) || hint.Equals("1"));
			}
			else
			{
				osxUseSpaces = false;
			}

			// Do we want to read keycodes or scancodes?
			SDL2_KeyboardUtil.UseScancodes = Environment.GetEnvironmentVariable(
				"FNA_KEYBOARD_USE_SCANCODES"
			) == "1";
			if (SDL2_KeyboardUtil.UseScancodes)
			{
				FNAPlatform.Log("Using scancodes instead of keycodes!");
			}

			// Active Key List
			List<Keys> keys = new List<Keys>();

			/* Setup Text Input Control Character Arrays
			 * (Only 7 control keys supported at this time)
			 */
			char[] textInputCharacters = new char[]
			{
				(char) 2,	// Home
				(char) 3,	// End
				(char) 8,	// Backspace
				(char) 9,	// Tab
				(char) 13,	// Enter
				(char) 127,	// Delete
				(char) 22	// Ctrl+V (Paste)
			};
			Dictionary<Keys, int> textInputBindings = new Dictionary<Keys, int>()
			{
				{ Keys.Home,	0 },
				{ Keys.End,	1 },
				{ Keys.Back,	2 },
				{ Keys.Tab,	3 },
				{ Keys.Enter,	4 },
				{ Keys.Delete,	5 }
				// Ctrl+V is special!
			};
			bool[] textInputControlDown = new bool[textInputCharacters.Length];
			int[] textInputControlRepeat = new int[textInputCharacters.Length];
			bool textInputSuppress = false;

			SDL.SDL_Event evt;

			while (game.RunApplication)
			{
				while (SDL.SDL_PollEvent(out evt) == 1)
				{
					// Keyboard
					if (evt.type == SDL.SDL_EventType.SDL_KEYDOWN)
					{
						Keys key = SDL2_KeyboardUtil.ToXNA(ref evt.key.keysym);
						if (!keys.Contains(key))
						{
							keys.Add(key);
							if (textInputBindings.ContainsKey(key))
							{
								int textIndex = textInputBindings[key];
								textInputControlDown[textIndex] = true;
								textInputControlRepeat[textIndex] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput(textInputCharacters[textIndex]);
							}
							else if (keys.Contains(Keys.LeftControl) && key == Keys.V)
							{
								textInputControlDown[6] = true;
								textInputControlRepeat[6] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput(textInputCharacters[6]);
								textInputSuppress = true;
							}
						}
					}
					else if (evt.type == SDL.SDL_EventType.SDL_KEYUP)
					{
						Keys key = SDL2_KeyboardUtil.ToXNA(ref evt.key.keysym);
						if (keys.Remove(key))
						{
							if (textInputBindings.ContainsKey(key))
							{
								textInputControlDown[textInputBindings[key]] = false;
							}
							else if ((!keys.Contains(Keys.LeftControl) && textInputControlDown[3]) || key == Keys.V)
							{
								textInputControlDown[6] = false;
								textInputSuppress = false;
							}
						}
					}

					// Mouse Input
					else if (evt.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
					{
						Mouse.INTERNAL_IsWarped = false;
					}
					else if (evt.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
					{
						// 120 units per notch. Because reasons.
						Mouse.INTERNAL_MouseWheel += evt.wheel.y * 120;
					}

					// Various Window Events...
					else if (evt.type == SDL.SDL_EventType.SDL_WINDOWEVENT)
					{
						// Window Focus
						if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
						{
							game.IsActive = true;

							if (!osxUseSpaces)
							{
								// If we alt-tab away, we lose the 'fullscreen desktop' flag on some WMs
								SDL.SDL_SetWindowFullscreen(
									game.Window.Handle,
									game.GraphicsDevice.PresentationParameters.IsFullScreen ?
										(uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP :
										0
								);
							}

							// Disable the screensaver when we're back.
							SDL.SDL_DisableScreenSaver();
						}
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
						{
							game.IsActive = false;

							if (!osxUseSpaces)
							{
								SDL.SDL_SetWindowFullscreen(game.Window.Handle, 0);
							}

							// Give the screensaver back, we're not that important now.
							SDL.SDL_EnableScreenSaver();
						}

						// Window Resize
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
						{
							Mouse.INTERNAL_WindowWidth = evt.window.data1;
							Mouse.INTERNAL_WindowHeight = evt.window.data2;

							// Should be called on user resize only, NOT ApplyChanges!
							((SDL2_GameWindow) game.Window).INTERNAL_ClientSizeChanged();
						}
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED)
						{
							Mouse.INTERNAL_WindowWidth = evt.window.data1;
							Mouse.INTERNAL_WindowHeight = evt.window.data2;

							// Need to reset the graphics device any time the window size changes
							GraphicsDeviceManager gdm = game.Services.GetService(
								typeof(IGraphicsDeviceService)
							) as GraphicsDeviceManager;
							// FIXME: gdm == null? -flibit
							if (gdm.IsFullScreen)
							{
								GraphicsDevice device = game.GraphicsDevice;
								gdm.INTERNAL_ResizeGraphicsDevice(
									device.GLDevice.Backbuffer.Width,
									device.GLDevice.Backbuffer.Height
								);
							}
							else
							{
								gdm.INTERNAL_ResizeGraphicsDevice(
									evt.window.data1,
									evt.window.data2
								);
							}
						}

						// Window Move
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED)
						{
							/* Apparently if you move the window to a new
							 * display, a GraphicsDevice Reset occurs.
							 * -flibit
							 */
							int newIndex = SDL.SDL_GetWindowDisplayIndex(
								game.Window.Handle
							);
							if (newIndex != displayIndex)
							{
								displayIndex = newIndex;
								game.GraphicsDevice.Reset(
									game.GraphicsDevice.PresentationParameters,
									GraphicsAdapter.Adapters[displayIndex]
								);
							}
						}

						// Mouse Focus
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER)
						{
							SDL.SDL_DisableScreenSaver();
						}
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE)
						{
							SDL.SDL_EnableScreenSaver();
						}
					}

					// Controller device management
					else if (evt.type == SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED)
					{
						INTERNAL_AddInstance(evt.cdevice.which);
					}
					else if (evt.type == SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED)
					{
						INTERNAL_RemoveInstance(evt.cdevice.which);
					}

					// Text Input
					else if (evt.type == SDL.SDL_EventType.SDL_TEXTINPUT && !textInputSuppress)
					{
						string text;

						// Based on the SDL2# LPUtf8StrMarshaler
						unsafe
						{
							byte* endPtr = evt.text.text;
							while (*endPtr != 0)
							{
								endPtr++;
							}
							byte[] bytes = new byte[endPtr - evt.text.text];
							Marshal.Copy((IntPtr) evt.text.text, bytes, 0, bytes.Length);
							text = System.Text.Encoding.UTF8.GetString(bytes);
						}

						if (text.Length > 0)
						{
							TextInputEXT.OnTextInput(text[0]);
						}
					}

					// Quit
					else if (evt.type == SDL.SDL_EventType.SDL_QUIT)
					{
						game.RunApplication = false;
						break;
					}
				}
				// Text Input Controls Key Handling
				for (int i = 0; i < textInputCharacters.Length; i += 1)
				{
					if (textInputControlDown[i] && textInputControlRepeat[i] <= Environment.TickCount)
					{
						TextInputEXT.OnTextInput(textInputCharacters[i]);
					}
				}

				Keyboard.SetKeys(keys);
				game.Tick();
			}

			// We out.
			game.Exit();
		}

		public static void BeforeInitialize()
		{
			// We want to initialize the controllers ASAP!
			SDL.SDL_Event[] evt = new SDL.SDL_Event[1];
			SDL.SDL_PumpEvents(); // Required to get OSX device events this early.
			while (SDL.SDL_PeepEvents(
				evt,
				1,
				SDL.SDL_eventaction.SDL_GETEVENT,
				SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED,
				SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED
			) == 1) {
				INTERNAL_AddInstance(evt[0].cdevice.which);
			}
		}

		public static IGLDevice CreateGLDevice(
			PresentationParameters presentationParameters
		) {
			// This loads the OpenGL entry points.
			return new OpenGLDevice(presentationParameters);
		}

		public static IALDevice CreateALDevice()
		{
			try
			{
				return new OpenALDevice();
			}
			catch(DllNotFoundException e)
			{
				FNAPlatform.Log("OpenAL not found! Need FNA.dll.config?");
				throw e;
			}
			catch(Exception)
			{
				/* We ignore device creation exceptions,
				 * as they are handled down the line with Instance != null
				 */
				return null;
			}
		}

		public static void SetPresentationInterval(PresentInterval interval)
		{
			if (interval == PresentInterval.Default || interval == PresentInterval.One)
			{
				if (OSVersion.Equals("Mac OS X"))
				{
					// Apple is a big fat liar about swap_control_tear. Use stock VSync.
					SDL.SDL_GL_SetSwapInterval(1);
				}
				else
				{
					if (SDL.SDL_GL_SetSwapInterval(-1) != -1)
					{
						FNAPlatform.Log("Using EXT_swap_control_tear VSync!");
					}
					else
					{
						FNAPlatform.Log("EXT_swap_control_tear unsupported. Fall back to standard VSync.");
						SDL.SDL_ClearError();
						SDL.SDL_GL_SetSwapInterval(1);
					}
				}
			}
			else if (interval == PresentInterval.Immediate)
			{
				SDL.SDL_GL_SetSwapInterval(0);
			}
			else if (interval == PresentInterval.Two)
			{
				SDL.SDL_GL_SetSwapInterval(2);
			}
			else
			{
				throw new NotSupportedException("Unrecognized PresentInterval!");
			}
		}

		public static GraphicsAdapter[] GetGraphicsAdapters()
		{
			SDL.SDL_DisplayMode filler = new SDL.SDL_DisplayMode();
			GraphicsAdapter[] adapters = new GraphicsAdapter[SDL.SDL_GetNumVideoDisplays()];
			for (int i = 0; i < adapters.Length; i += 1)
			{
				List<DisplayMode> modes = new List<DisplayMode>();
				int numModes = SDL.SDL_GetNumDisplayModes(i);
				for (int j = 0; j < numModes; j += 1)
				{
					SDL.SDL_GetDisplayMode(i, j, out filler);

					// Check for dupes caused by varying refresh rates.
					bool dupe = false;
					foreach (DisplayMode mode in modes)
					{
						if (filler.w == mode.Width && filler.h == mode.Height)
						{
							dupe = true;
						}
					}
					if (!dupe)
					{
						modes.Add(
							new DisplayMode(
								filler.w,
								filler.h,
								SurfaceFormat.Color // FIXME: Assumption!
							)
						);
					}
				}
				SDL.SDL_GetCurrentDisplayMode(i, out filler);
				adapters[i] = new GraphicsAdapter(
					new DisplayMode(
						filler.w,
						filler.h,
						SurfaceFormat.Color // FIXME: Assumption!
					),
					new DisplayModeCollection(modes),
					SDL.SDL_GetDisplayName(i)
				);
			}
			return adapters;
		}

		public static void GetMouseState(
			out int x,
			out int y,
			out ButtonState left,
			out ButtonState middle,
			out ButtonState right,
			out ButtonState x1,
			out ButtonState x2
		) {
			uint flags = SDL.SDL_GetMouseState(out x, out y);
			left =		(ButtonState) (flags & SDL.SDL_BUTTON_LMASK);
			middle =	(ButtonState) ((flags & SDL.SDL_BUTTON_MMASK) >> 1);
			right =		(ButtonState) ((flags & SDL.SDL_BUTTON_RMASK) >> 2);
			x1 =		(ButtonState) ((flags & SDL.SDL_BUTTON_X1MASK) >> 3);
			x2 =		(ButtonState) ((flags & SDL.SDL_BUTTON_X2MASK) >> 4);
		}

		public static void OnIsMouseVisibleChanged(bool visible)
		{
			SDL.SDL_ShowCursor(visible ? 1 : 0);
		}

		public static string GetStorageRoot()
		{
			if (OSVersion.Equals("Windows"))
			{
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"SavedGames"
				);
			}
			if (OSVersion.Equals("Mac OS X"))
			{
				string osConfigDir = Environment.GetEnvironmentVariable("HOME");
				if (String.IsNullOrEmpty(osConfigDir))
				{
					return "."; // Oh well.
				}
				osConfigDir += "/Library/Application Support";
				return osConfigDir;
			}
			if (OSVersion.Equals("Linux"))
			{
				// Assuming a non-OSX Unix platform will follow the XDG. Which it should.
				string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (String.IsNullOrEmpty(osConfigDir))
				{
					osConfigDir = Environment.GetEnvironmentVariable("HOME");
					if (String.IsNullOrEmpty(osConfigDir))
					{
						return ".";	// Oh well.
					}
					osConfigDir += "/.local/share";
				}
				return osConfigDir;
			}
			throw new NotSupportedException("Unhandled SDL2 platform!");
		}

		public static bool IsStoragePathConnected(string path)
		{
			if (	OSVersion.Equals("Linux") ||
				OSVersion.Equals("Mac OS X")	)
			{
				/* Linux and Mac use locally connected storage in the user's
				 * home location, which should always be "connected".
				 */
				return true;
			}
			if (OSVersion.Equals("Windows"))
			{
				try
				{
					return new DriveInfo(path).IsReady;
				}
				catch
				{
					// The storageRoot path is invalid / has been removed.
					return false;
				}
			}
			throw new NotSupportedException("Unhandled SDL2 platform");
		}

		public static void ShowRuntimeError(string title, string message)
		{
			SDL.SDL_ShowSimpleMessageBox(
				SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
				title,
				message,
				IntPtr.Zero
			);
		}

		public static void TextureDataFromStream(
			Stream stream,
			out int width,
			out int height,
			out byte[] pixels,
			int reqWidth = -1,
			int reqHeight = -1,
			bool zoom = false
		) {
			// Load the Stream into an SDL_RWops*
			byte[] mem = new byte[stream.Length];
			GCHandle handle = GCHandle.Alloc(mem, GCHandleType.Pinned);
			stream.Read(mem, 0, mem.Length);
			IntPtr rwops = SDL.SDL_RWFromMem(mem, mem.Length);

			// Load the SDL_Surface* from RWops, get the image data
			IntPtr surface = SDL_image.IMG_Load_RW(rwops, 1);
			handle.Free();
			if (surface == IntPtr.Zero)
			{
				// File not found, supported, etc.
				width = 0;
				height = 0;
				pixels = null;
				return;
			}
			surface = INTERNAL_convertSurfaceFormat(surface);

			// Image scaling, if applicable
			if (reqWidth != -1 && reqHeight != -1)
			{
				// Get the file surface dimensions now...
				int rw;
				int rh;
				unsafe
				{
					SDL.SDL_Surface* surPtr = (SDL.SDL_Surface*) surface;
					rw = surPtr->w;
					rh = surPtr->h;
				}

				// Calculate the image scale factor
				bool scaleWidth;
				if (zoom)
				{
					scaleWidth = rw < rh;
				}
				else
				{
					scaleWidth = rw > rh;
				}
				float scale;
				if (scaleWidth)
				{
					scale = reqWidth / (float) rw;
				}
				else
				{
					scale = reqHeight / (float) rh;
				}

				// Calculate the scaled image size, crop if zoomed
				int resultWidth;
				int resultHeight;
				SDL.SDL_Rect crop = new SDL.SDL_Rect();
				if (zoom)
				{
					resultWidth = reqWidth;
					resultHeight = reqHeight;
					if (scaleWidth)
					{
						crop.x = 0;
						crop.w = rw;
						crop.y = (int) (rh / 2 - (reqHeight / scale) / 2);
						crop.h = (int) (reqHeight / scale);
					}
					else
					{
						crop.y = 0;
						crop.h = rh;
						crop.x = (int) (rw / 2 - (reqWidth / scale) / 2);
						crop.w = (int) (reqWidth / scale);
					}
				}
				else
				{
					resultWidth = (int) (rw * scale);
					resultHeight = (int) (rh * scale);
				}

				// Alloc surface, blit!
				IntPtr newSurface = SDL.SDL_CreateRGBSurface(
					0,
					resultWidth,
					resultHeight,
					32,
					0x000000FF,
					0x0000FF00,
					0x00FF0000,
					0xFF000000
				);
				SDL.SDL_SetSurfaceBlendMode(
					surface,
					SDL.SDL_BlendMode.SDL_BLENDMODE_NONE
				);
				if (zoom)
				{
					SDL.SDL_BlitScaled(
						surface,
						ref crop,
						newSurface,
						IntPtr.Zero
					);
				}
				else
				{
					SDL.SDL_BlitScaled(
						surface,
						IntPtr.Zero,
						newSurface,
						IntPtr.Zero
					);
				}
				SDL.SDL_FreeSurface(surface);
				surface = newSurface;
			}

			// Copy surface data to output managed byte array
			unsafe
			{
				SDL.SDL_Surface* surPtr = (SDL.SDL_Surface*) surface;
				width = surPtr->w;
				height = surPtr->h;
				pixels = new byte[width * height * 4]; // MUST be SurfaceFormat.Color!
				Marshal.Copy(surPtr->pixels, pixels, 0, pixels.Length);
			}
			SDL.SDL_FreeSurface(surface);

			/* Ensure that the alpha pixels are... well, actual alpha.
			 * You think this looks stupid, but be assured: Your paint program is
			 * almost certainly even stupider.
			 * -flibit
			 */
			for (int i = 0; i < pixels.Length; i += 4)
			{
				if (pixels[i + 3] == 0)
				{
					pixels[i] = 0;
					pixels[i + 1] = 0;
					pixels[i + 2] = 0;
				}
			}
		}

		public static void SavePNG(
			Stream stream,
			int width,
			int height,
			int imgWidth,
			int imgHeight,
			byte[] data
		) {
			// Create an SDL_Surface*, write the pixel data
			IntPtr surface = SDL.SDL_CreateRGBSurface(
				0,
				imgWidth,
				imgHeight,
				32,
				0x000000FF,
				0x0000FF00,
				0x00FF0000,
				0xFF000000
			);
			SDL.SDL_LockSurface(surface);
			unsafe
			{
				SDL.SDL_Surface* surPtr = (SDL.SDL_Surface*) surface;
				Marshal.Copy(
					data,
					0,
					surPtr->pixels,
					data.Length
				);
			}
			SDL.SDL_UnlockSurface(surface);
			data = null; // We're done with the original pixel data.

			// Blit to a scaled surface of the size we want, if needed.
			if (width != imgWidth || height != imgHeight)
			{
				IntPtr scaledSurface = SDL.SDL_CreateRGBSurface(
					0,
					width,
					height,
					32,
					0x000000FF,
					0x0000FF00,
					0x00FF0000,
					0xFF000000
				);
				SDL.SDL_BlitScaled(
					surface,
					IntPtr.Zero,
					scaledSurface,
					IntPtr.Zero
				);
				SDL.SDL_FreeSurface(surface);
				surface = scaledSurface;
			}

			// Create an SDL_RWops*, save PNG to RWops
			const int pngHeaderSize = 41;
			const int pngFooterSize = 57;
			byte[] pngOut = new byte[
				(width * height * 4) +
				pngHeaderSize +
				pngFooterSize +
				256 // FIXME: Arbitrary zlib data padding for low-res images
			]; // Max image size
			IntPtr dst = SDL.SDL_RWFromMem(pngOut, pngOut.Length);
			SDL_image.IMG_SavePNG_RW(surface, dst, 1);
			SDL.SDL_FreeSurface(surface); // We're done with the surface.

			// Get PNG size, write to Stream
			int size = (
				(pngOut[33] << 24) |
				(pngOut[34] << 16) |
				(pngOut[35] << 8) |
				(pngOut[36])
			) + pngHeaderSize + pngFooterSize;
			stream.Write(pngOut, 0, size);
		}

		#endregion

		#region GamePad Backend

		private enum HapticType
		{
			Simple = 0,
			LeftRight = 1,
			LeftRightMacHack = 2
		}

		// Controller device information
		private static IntPtr[] INTERNAL_devices = new IntPtr[GamePad.GAMEPAD_COUNT];
		private static Dictionary<int, int> INTERNAL_instanceList = new Dictionary<int, int>();
		private static string[] INTERNAL_guids = GenStringArray();

		// Haptic device information
		private static IntPtr[] INTERNAL_haptics = new IntPtr[GamePad.GAMEPAD_COUNT];
		private static HapticType[] INTERNAL_hapticTypes = new HapticType[GamePad.GAMEPAD_COUNT];

		// Light bar information
		private static string[] INTERNAL_lightBars = GenStringArray();

		// Cached GamePadStates/Capabilities
		private static GamePadState[] INTERNAL_states = new GamePadState[GamePad.GAMEPAD_COUNT];
		private static GamePadCapabilities[] INTERNAL_capabilities = new GamePadCapabilities[GamePad.GAMEPAD_COUNT];

		// We use this to apply XInput-like rumble effects.
		private static SDL.SDL_HapticEffect INTERNAL_leftRightEffect = new SDL.SDL_HapticEffect
		{
			type = SDL.SDL_HAPTIC_LEFTRIGHT,
			leftright = new SDL.SDL_HapticLeftRight
			{
				type = SDL.SDL_HAPTIC_LEFTRIGHT,
				length = SDL.SDL_HAPTIC_INFINITY,
				large_magnitude = ushort.MaxValue,
				small_magnitude = ushort.MaxValue
			}
		};

		// We use this to get left/right support on OSX via a nice driver workaround!
		private static ushort[] leftRightMacHackData = {0, 0};
		private static GCHandle leftRightMacHackPArry = GCHandle.Alloc(leftRightMacHackData, GCHandleType.Pinned);
		private static IntPtr leftRightMacHackPtr = leftRightMacHackPArry.AddrOfPinnedObject();
		private static SDL.SDL_HapticEffect INTERNAL_leftRightMacHackEffect = new SDL.SDL_HapticEffect
		{
			type = SDL.SDL_HAPTIC_CUSTOM,
			custom = new SDL.SDL_HapticCustom
			{
				type = SDL.SDL_HAPTIC_CUSTOM,
				length = SDL.SDL_HAPTIC_INFINITY,
				channels = 2,
				period = 1,
				samples = 2,
				data = leftRightMacHackPtr
			}
		};

		// FIXME: SDL_GameController config input inversion!
		private static float invertAxis = Environment.GetEnvironmentVariable(
			"FNA_WORKAROUND_INVERT_YAXIS"
		) == "1" ? -1.0f : 1.0f;

		public static GamePadCapabilities GetGamePadCapabilities(int index)
		{
			if (INTERNAL_devices[index] == IntPtr.Zero)
			{
				return new GamePadCapabilities();
			}
			return INTERNAL_capabilities[index];
		}

		public static GamePadState GetGamePadState(int index, GamePadDeadZone deadZoneMode)
		{
			IntPtr device = INTERNAL_devices[index];
			if (device == IntPtr.Zero)
			{
				return new GamePadState();
			}

			// Do not attempt to understand this number at all costs!
			const float DeadZoneSize = 0.27f;

			// The "master" button state is built from this.
			Buttons gc_buttonState = (Buttons) 0;

			// Sticks
			GamePadThumbSticks gc_sticks = new GamePadThumbSticks(
				new Vector2(
					(float) SDL.SDL_GameControllerGetAxis(
						device,
						SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
					) / 32768.0f,
					(float) SDL.SDL_GameControllerGetAxis(
						device,
						SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
					) / -32768.0f * invertAxis
				),
				new Vector2(
					(float) SDL.SDL_GameControllerGetAxis(
						device,
						SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
					) / 32768.0f,
					(float) SDL.SDL_GameControllerGetAxis(
						device,
						SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
					) / -32768.0f * invertAxis
				),
				deadZoneMode
			);
			gc_buttonState |= READ_StickToButtons(
				gc_sticks.Left,
				Buttons.LeftThumbstickLeft,
				Buttons.LeftThumbstickRight,
				Buttons.LeftThumbstickUp,
				Buttons.LeftThumbstickDown,
				DeadZoneSize
			);
			gc_buttonState |= READ_StickToButtons(
				gc_sticks.Right,
				Buttons.RightThumbstickLeft,
				Buttons.RightThumbstickRight,
				Buttons.RightThumbstickUp,
				Buttons.RightThumbstickDown,
				DeadZoneSize
			);

			// Triggers
			GamePadTriggers gc_triggers = new GamePadTriggers(
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
				) / 32768.0f,
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
				) / 32768.0f
			);
			gc_buttonState |= READ_TriggerToButton(
				gc_triggers.Left,
				Buttons.LeftTrigger,
				DeadZoneSize
			);
			gc_buttonState |= READ_TriggerToButton(
				gc_triggers.Right,
				Buttons.RightTrigger,
				DeadZoneSize
			);

			// Buttons
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) != 0)
			{
				gc_buttonState |= Buttons.A;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) != 0)
			{
				gc_buttonState |= Buttons.B;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) != 0)
			{
				gc_buttonState |= Buttons.X;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) != 0)
			{
				gc_buttonState |= Buttons.Y;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) != 0)
			{
				gc_buttonState |= Buttons.Back;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE) != 0)
			{
				gc_buttonState |= Buttons.BigButton;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) != 0)
			{
				gc_buttonState |= Buttons.Start;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) != 0)
			{
				gc_buttonState |= Buttons.LeftStick;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) != 0)
			{
				gc_buttonState |= Buttons.RightStick;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) != 0)
			{
				gc_buttonState |= Buttons.LeftShoulder;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) != 0)
			{
				gc_buttonState |= Buttons.RightShoulder;
			}

			// DPad
			GamePadDPad gc_dpad;
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) != 0)
			{
				gc_buttonState |= Buttons.DPadUp;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) != 0)
			{
				gc_buttonState |= Buttons.DPadDown;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) != 0)
			{
				gc_buttonState |= Buttons.DPadLeft;
			}
			if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) != 0)
			{
				gc_buttonState |= Buttons.DPadRight;
			}
			gc_dpad = new GamePadDPad(gc_buttonState);

			// Compile the master buttonstate
			GamePadButtons gc_buttons = new GamePadButtons(gc_buttonState);

			// Build the GamePadState, increment PacketNumber if state changed.
			GamePadState gc_builtState = new GamePadState(
				gc_sticks,
				gc_triggers,
				gc_buttons,
				gc_dpad
			);
			gc_builtState.IsConnected = true;
			gc_builtState.PacketNumber = INTERNAL_states[index].PacketNumber;
			if (gc_builtState != INTERNAL_states[index])
			{
				gc_builtState.PacketNumber += 1;
				INTERNAL_states[index] = gc_builtState;
			}

			return gc_builtState;
		}

		public static bool SetGamePadVibration(int index, float leftMotor, float rightMotor)
		{
			IntPtr haptic = INTERNAL_haptics[index];
			HapticType type = INTERNAL_hapticTypes[index];

			if (haptic == IntPtr.Zero)
			{
				return false;
			}

			if (leftMotor <= 0.0f && rightMotor <= 0.0f)
			{
				SDL.SDL_HapticStopAll(haptic);
			}
			else if (type == HapticType.LeftRight)
			{
				INTERNAL_leftRightEffect.leftright.large_magnitude = (ushort) (65535.0f * leftMotor);
				INTERNAL_leftRightEffect.leftright.small_magnitude = (ushort) (65535.0f * rightMotor);
				SDL.SDL_HapticUpdateEffect(
					haptic,
					0,
					ref INTERNAL_leftRightEffect
				);
				SDL.SDL_HapticRunEffect(
					haptic,
					0,
					1
				);
			}
			else if (type == HapticType.LeftRightMacHack)
			{
				leftRightMacHackData[0] = (ushort) (65535.0f * leftMotor);
				leftRightMacHackData[1] = (ushort) (65535.0f * rightMotor);
				SDL.SDL_HapticUpdateEffect(
					haptic,
					0,
					ref INTERNAL_leftRightMacHackEffect
				);
				SDL.SDL_HapticRunEffect(
					haptic,
					0,
					1
				);
			}
			else
			{
				SDL.SDL_HapticRumblePlay(
					haptic,
					Math.Max(leftMotor, rightMotor),
					SDL.SDL_HAPTIC_INFINITY // Oh dear...
				);
			}
			return true;
		}

		public static string GetGamePadGUID(int index)
		{
			return INTERNAL_guids[index];
		}

		public static void SetGamePadLightBar(int index, Color color)
		{
			if (String.IsNullOrEmpty(INTERNAL_lightBars[index]))
			{
				return;
			}

			string baseDir = INTERNAL_lightBars[index];
			try
			{
				File.WriteAllText(baseDir + "red/brightness", color.R.ToString());
				File.WriteAllText(baseDir + "green/brightness", color.G.ToString());
				File.WriteAllText(baseDir + "blue/brightness", color.B.ToString());
			}
			catch
			{
				// If something went wrong, assume the worst and just remove it.
				INTERNAL_lightBars[index] = String.Empty;
			}
		}

		private static void INTERNAL_AddInstance(int dev)
		{
			int which = -1;
			for (int i = 0; i < INTERNAL_devices.Length; i += 1)
			{
				if (INTERNAL_devices[i] == IntPtr.Zero)
				{
					which = i;
					break;
				}
			}
			if (which == -1)
			{
				return; // Ignoring more than 4 controllers.
			}

			// Clear the error buffer. We're about to do a LOT of dangerous stuff.
			SDL.SDL_ClearError();

			// Open the device!
			INTERNAL_devices[which] = SDL.SDL_GameControllerOpen(dev);

			// We use this when dealing with Haptic/GUID initialization.
			IntPtr thisJoystick = SDL.SDL_GameControllerGetJoystick(INTERNAL_devices[which]);

			// Pair up the instance ID to the player index.
			// FIXME: Remove check after 2.0.4? -flibit
			int thisInstance = SDL.SDL_JoystickInstanceID(thisJoystick);
			if (INTERNAL_instanceList.ContainsKey(thisInstance))
			{
				// Duplicate? Usually this is OSX being dumb, but...?
				INTERNAL_devices[which] = IntPtr.Zero;
				return;
			}
			INTERNAL_instanceList.Add(thisInstance, which);

			// Start with a fresh state.
			INTERNAL_states[which] = new GamePadState();
			INTERNAL_states[which].IsConnected = true;

			// Initialize the haptics for the joystick, if applicable.
			if (SDL.SDL_JoystickIsHaptic(thisJoystick) == 1)
			{
				INTERNAL_haptics[which] = SDL.SDL_HapticOpenFromJoystick(thisJoystick);
				if (INTERNAL_haptics[which] == IntPtr.Zero)
				{
					FNAPlatform.Log("HAPTIC OPEN ERROR: " + SDL.SDL_GetError());
				}
			}
			if (INTERNAL_haptics[which] != IntPtr.Zero)
			{
				if (	OSVersion.Equals("Mac OS X") &&
					SDL.SDL_HapticEffectSupported(INTERNAL_haptics[which], ref INTERNAL_leftRightMacHackEffect) == 1	)
				{
					INTERNAL_hapticTypes[which] = HapticType.LeftRightMacHack;
					SDL.SDL_HapticNewEffect(INTERNAL_haptics[which], ref INTERNAL_leftRightMacHackEffect);
				}
				else if (	!OSVersion.Equals("Mac OS X") &&
						SDL.SDL_HapticEffectSupported(INTERNAL_haptics[which], ref INTERNAL_leftRightEffect) == 1	)
				{
					INTERNAL_hapticTypes[which] = HapticType.LeftRight;
					SDL.SDL_HapticNewEffect(INTERNAL_haptics[which], ref INTERNAL_leftRightEffect);
				}
				else if (SDL.SDL_HapticRumbleSupported(INTERNAL_haptics[which]) == 1)
				{
					INTERNAL_hapticTypes[which] = HapticType.Simple;
					SDL.SDL_HapticRumbleInit(INTERNAL_haptics[which]);
				}
				else
				{
					// We can't even play simple rumble, this haptic device is useless to us.
					SDL.SDL_HapticClose(INTERNAL_haptics[which]);
					INTERNAL_haptics[which] = IntPtr.Zero;
				}
			}

			// An SDL_GameController _should_ always be complete...
			INTERNAL_capabilities[which] = new GamePadCapabilities()
			{
				IsConnected = true,
				HasAButton = true,
				HasBButton = true,
				HasXButton = true,
				HasYButton = true,
				HasBackButton = true,
				HasStartButton = true,
				HasDPadDownButton = true,
				HasDPadLeftButton = true,
				HasDPadRightButton = true,
				HasDPadUpButton = true,
				HasLeftShoulderButton = true,
				HasRightShoulderButton = true,
				HasLeftStickButton = true,
				HasRightStickButton = true,
				HasLeftTrigger = true,
				HasRightTrigger = true,
				HasLeftXThumbStick = true,
				HasLeftYThumbStick = true,
				HasRightXThumbStick = true,
				HasRightYThumbStick = true,
				HasBigButton = true,
				HasLeftVibrationMotor = INTERNAL_haptics[which] != IntPtr.Zero,
				HasRightVibrationMotor = INTERNAL_haptics[which] != IntPtr.Zero,
				HasVoiceSupport = false
			};

			// Store the GUID string for this device
			StringBuilder result = new StringBuilder();
			byte[] resChar = new byte[33]; // FIXME: Sort of arbitrary.
			SDL.SDL_JoystickGetGUIDString(
				SDL.SDL_JoystickGetGUID(thisJoystick),
				resChar,
				resChar.Length
			);
			if (OSVersion.Equals("Linux"))
			{
				result.Append((char) resChar[8]);
				result.Append((char) resChar[9]);
				result.Append((char) resChar[10]);
				result.Append((char) resChar[11]);
				result.Append((char) resChar[16]);
				result.Append((char) resChar[17]);
				result.Append((char) resChar[18]);
				result.Append((char) resChar[19]);
			}
			else if (OSVersion.Equals("Mac OS X"))
			{
				result.Append((char) resChar[0]);
				result.Append((char) resChar[1]);
				result.Append((char) resChar[2]);
				result.Append((char) resChar[3]);
				result.Append((char) resChar[16]);
				result.Append((char) resChar[17]);
				result.Append((char) resChar[18]);
				result.Append((char) resChar[19]);
			}
			else if (OSVersion.Equals("Windows"))
			{
				bool isXInput = true;
				foreach (byte b in resChar)
				{
					if (((char) b) != '0' && b != 0)
					{
						isXInput = false;
						break;
					}
				}
				if (isXInput)
				{
					result.Append("xinput");
				}
				else
				{
					result.Append((char) resChar[0]);
					result.Append((char) resChar[1]);
					result.Append((char) resChar[2]);
					result.Append((char) resChar[3]);
					result.Append((char) resChar[4]);
					result.Append((char) resChar[5]);
					result.Append((char) resChar[6]);
					result.Append((char) resChar[7]);
				}
			}
			else
			{
				throw new NotSupportedException("Unhandled SDL2 platform!");
			}
			INTERNAL_guids[which] = result.ToString();

			// Initialize light bar
			if (	OSVersion.Equals("Linux") &&
				INTERNAL_guids[which].Equals("4c05c405")	)
			{
				// Get all of the individual PS4 LED instances
				List<string> ledList = new List<string>();
				string[] dirs = Directory.GetDirectories("/sys/class/leds/");
				foreach (string dir in dirs)
				{
					if (	dir.Contains("054C:05C4") &&
						dir.EndsWith("blue")	)
					{
						ledList.Add(dir.Substring(0, dir.LastIndexOf(':') + 1));
					}
				}
				// Find how many of these are already in use
				int numLights = 0;
				for (int i = 0; i < INTERNAL_lightBars.Length; i += 1)
				{
					if (!String.IsNullOrEmpty(INTERNAL_lightBars[i]))
					{
						numLights += 1;
					}
				}
				// If all are not already in use, use the first unused light
				if (numLights < ledList.Count)
				{
					INTERNAL_lightBars[which] = ledList[numLights];
				}
			}

			// Print controller information to stdout.
			FNAPlatform.Log(
				"Controller " + which.ToString() + ": " +
				SDL.SDL_GameControllerName(INTERNAL_devices[which])
			);
		}

		private static void INTERNAL_RemoveInstance(int dev)
		{
			int output;
			if (!INTERNAL_instanceList.TryGetValue(dev, out output))
			{
				// Odds are, this is controller 5+ getting removed.
				return;
			}
			INTERNAL_instanceList.Remove(dev);
			if (INTERNAL_haptics[output] != IntPtr.Zero)
			{
				SDL.SDL_HapticClose(INTERNAL_haptics[output]);
				INTERNAL_haptics[output] = IntPtr.Zero;
			}
			SDL.SDL_GameControllerClose(INTERNAL_devices[output]);
			INTERNAL_devices[output] = IntPtr.Zero;
			INTERNAL_states[output] = new GamePadState();
			INTERNAL_guids[output] = String.Empty;

			// A lot of errors can happen here, but honestly, they can be ignored...
			SDL.SDL_ClearError();

			FNAPlatform.Log("Removed device, player: " + output.ToString());
		}

		// GetState can convert stick values to button values
		private static Buttons READ_StickToButtons(Vector2 stick, Buttons left, Buttons right, Buttons up , Buttons down, float DeadZoneSize)
		{
			Buttons b = (Buttons) 0;

			if (stick.X > DeadZoneSize)
			{
				b |= right;
			}
			if (stick.X < -DeadZoneSize)
			{
				b |= left;
			}
			if (stick.Y > DeadZoneSize)
			{
				b |= up;
			}
			if (stick.Y < -DeadZoneSize)
			{
				b |= down;
			}

			return b;
		}

		// GetState can convert trigger values to button values
		private static Buttons READ_TriggerToButton(float trigger, Buttons button, float DeadZoneSize)
		{
			Buttons b = (Buttons) 0;

			if (trigger > DeadZoneSize)
			{
				b |= button;
			}

			return b;
		}

		private static string[] GenStringArray()
		{
			string[] result = new string[GamePad.GAMEPAD_COUNT];
			for (int i = 0; i < result.Length; i += 1)
			{
				result[i] = String.Empty;
			}
			return result;
		}

		#endregion

		#region Private Static SDL_Surface Interop

		private static unsafe IntPtr INTERNAL_convertSurfaceFormat(IntPtr surface)
		{
			IntPtr result = surface;
			unsafe
			{
				SDL.SDL_Surface* surPtr = (SDL.SDL_Surface*) surface;
				SDL.SDL_PixelFormat* pixelFormatPtr = (SDL.SDL_PixelFormat*) surPtr->format;

				// SurfaceFormat.Color is SDL_PIXELFORMAT_ABGR8888
				if (pixelFormatPtr->format != SDL.SDL_PIXELFORMAT_ABGR8888)
				{
					// Create a properly formatted copy, free the old surface
					result = SDL.SDL_ConvertSurfaceFormat(surface, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
					SDL.SDL_FreeSurface(surface);
				}
			}
			return result;
		}

		#endregion
	}
}
