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
using System.IO;
using System.Text;
using System.Reflection;
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

		private static string OSVersion;

		private static readonly bool UseScancodes = Environment.GetEnvironmentVariable(
			"FNA_KEYBOARD_USE_SCANCODES"
		) == "1";

		private static float HapticMaxWorkaround;

		#endregion

		#region Game Objects

		/* This is needed for asynchronous window events */
		private static List<Game> activeGames = new List<Game>();

		#endregion

		#region Init/Exit Methods

		public static string ProgramInit()
		{
			// This is how we can weed out cases where fnalibs is missing
			try
			{
				OSVersion = SDL.SDL_GetPlatform();
			}
			catch(Exception e)
			{
				FNALoggerEXT.LogError(
					"SDL2 was not found! Do you have fnalibs?"
				);
				throw e;
			}

			/* SDL2 might complain if an OS that uses SDL_main has not actually
			 * used SDL_main by the time you initialize SDL2.
			 * The only platform that is affected is Windows, but we can skip
			 * their WinMain. This was only added to prevent iOS from exploding.
			 * -flibit
			 */
			SDL.SDL_SetMainReady();

			// Also, Windows is an idiot. -flibit
			if (	OSVersion.Equals("Windows") ||
				OSVersion.Equals("WinRT")	)
			{
				// Visual Studio is an idiot.
				if (System.Diagnostics.Debugger.IsAttached)
				{
					SDL.SDL_SetHint(
						SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING,
						"1"
					);
				}

				/* Windows has terrible event pumping and doesn't give us
				 * WM_PAINT events correctly. So we get to do this!
				 * -flibit
				 */
				SDL.SDL_SetEventFilter(
					win32OnPaint,
					IntPtr.Zero
				);
			}

			/* FIXME: SDL bug!
			 * I have a bit of a confession to make: In 2013 I wrote
			 * SDL_HAPTIC_LEFTRIGHT, and I developed it primarily on
			 * Linux, where the force feedback API works with u16
			 * values. The only other platform that legitimately
			 * supports left/right rumble is Windows, where XInput
			 * coincidentally also takes in u16 values.
			 *
			 * As it turns out, both in XInput and Linux, the
			 * existing effect code had bugs where neither platform
			 * adjusted the input value to what SDL _actually_
			 * expects, which is s16 > 0, to mimic DirectInput.
			 * So when writing LEFTRIGHT, it just happened to work
			 * exactly right on the only supported platforms.
			 *
			 * But then someone fixed the bug in XInput, and on
			 * platforms that have emerged since then, our max value
			 * finally broke. Interestingly this may also be wrong
			 * for LeftRightMacHack, but that was someone else's
			 * custom driver and they wrote that path so :shrug:
			 *
			 * For now, we're going to keep using u16 on Linux, and
			 * s16 > 0 everywhere else. Remove this when 2.0.9 is
			 * out, and go back to u16 when 2.1 is out and bases its
			 * magnitudes on XInput rather than DirectInput.
			 * -flibit
			 */
			if (OSVersion.Equals("Linux"))
			{
				HapticMaxWorkaround = 65535.0f;
			}
			else
			{
				HapticMaxWorkaround = 32767.0f;
			}

			/* Mount TitleLocation.Path */
			string titleLocation = GetBaseDirectory();

			// If available, load the SDL_GameControllerDB
			string mappingsDB = Path.Combine(
				titleLocation,
				"gamecontrollerdb.txt"
			);
			if (File.Exists(mappingsDB))
			{
				SDL.SDL_GameControllerAddMappingsFromFile(
					mappingsDB
				);
			}

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

			// We want to initialize the controllers ASAP!
			SDL.SDL_Event[] evt = new SDL.SDL_Event[1];
			SDL.SDL_PumpEvents();
			while (SDL.SDL_PeepEvents(
				evt,
				1,
				SDL.SDL_eventaction.SDL_GETEVENT,
				SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED,
				SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED
			) == 1) {
				INTERNAL_AddInstance(evt[0].cdevice.which);
			}

			return titleLocation;
		}

		public static void ProgramExit(object sender, EventArgs e)
		{
			// This _should_ be the last SDL call we make...
			SDL.SDL_Quit();
		}

		#endregion

		#region Window Methods

		public static GameWindow CreateWindow()
		{
			// GLContext environment variables
			bool forceES3 = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_ES3"
			) == "1";
			bool forceCoreProfile = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_CORE_PROFILE"
			) == "1";
			bool forceCompatProfile = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_COMPATIBILITY_PROFILE"
			) == "1";

			// Some platforms are GLES only
			forceES3 |= (
				OSVersion.Equals("WinRT") ||
				OSVersion.Equals("iOS") ||
				OSVersion.Equals("tvOS") ||
				OSVersion.Equals("Android") ||
				OSVersion.Equals("Emscripten")
			);

			// Set and initialize the SDL2 window
			SDL.SDL_WindowFlags initFlags = (
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL |
				SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
				SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS
			);

			if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1")
			{
				initFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
			}

			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
			if (forceES3)
			{
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_RETAINED_BACKING,
					0
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_ACCELERATED_VISUAL,
					1
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION,
					3
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION,
					0
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK,
					(int) SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES
				);
			}
			else if (forceCoreProfile)
			{
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION,
					3
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION,
					2
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK,
					(int) SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE
				);
			}
			else if (forceCompatProfile)
			{
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION,
					2
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION,
					1
				);
				SDL.SDL_GL_SetAttribute(
					SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK,
					(int) SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_COMPATIBILITY
				);
			}
#if DEBUG
			SDL.SDL_GL_SetAttribute(
				SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS,
				(int) SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG
			);
#endif
			string title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();
			IntPtr window = SDL.SDL_CreateWindow(
				title,
				SDL.SDL_WINDOWPOS_CENTERED,
				SDL.SDL_WINDOWPOS_CENTERED,
				GraphicsDeviceManager.DefaultBackBufferWidth,
				GraphicsDeviceManager.DefaultBackBufferHeight,
				initFlags
			);
			INTERNAL_SetIcon(window, title);

			// Disable the screensaver.
			SDL.SDL_DisableScreenSaver();

			// We hide the mouse cursor by default.
			SDL.SDL_ShowCursor(0);

			/* If high DPI is not found, unset the HIGHDPI var.
			 * This is our way to communicate that it failed...
			 * -flibit
			 */
			int drawX, drawY;
			SDL.SDL_GL_GetDrawableSize(window, out drawX, out drawY);
			if (	drawX == GraphicsDeviceManager.DefaultBackBufferWidth &&
				drawY == GraphicsDeviceManager.DefaultBackBufferHeight	)
			{
				Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "0");
			}

			return new FNAWindow(
				window,
				@"\\.\DISPLAY" + (
					SDL.SDL_GetWindowDisplayIndex(window) + 1
				).ToString()
			);
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

			if (Mouse.WindowHandle == window.Handle)
			{
				Mouse.WindowHandle = IntPtr.Zero;
			}

			SDL.SDL_DestroyWindow(window.Handle);
		}

		public static void ApplyWindowChanges(
			IntPtr window,
			int clientWidth,
			int clientHeight,
			bool wantsFullscreen,
			string screenDeviceName,
			ref string resultDeviceName
		) {
			bool center = false;
			if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1")
			{
				/* For high-DPI windows, halve the size!
				 * The drawable size is now the primary width/height, so
				 * the window needs to accommodate the GL viewport.
				 * -flibit
				 */
				clientWidth /= 2;
				clientHeight /= 2;
			}

			// When windowed, set the size before moving
			if (!wantsFullscreen)
			{
				bool resize = false;
				if ((SDL.SDL_GetWindowFlags(window) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0)
				{
					SDL.SDL_SetWindowFullscreen(window, 0);
					resize = true;
				}
				else
				{
					int w, h;
					SDL.SDL_GetWindowSize(
						window,
						out w,
						out h
					);
					resize = (clientWidth != w || clientHeight != h);
				}
				if (resize)
				{
					SDL.SDL_SetWindowSize(window, clientWidth, clientHeight);
					center = true;
				}
			}

			// Get on the right display!
			int displayIndex = 0;
			for (int i = 0; i < GraphicsAdapter.Adapters.Count; i += 1)
			{
				if (screenDeviceName == GraphicsAdapter.Adapters[i].DeviceName)
				{
					displayIndex = i;
					break;
				}
			}

			// Just to be sure, become a window first before changing displays
			if (resultDeviceName != screenDeviceName)
			{
				SDL.SDL_SetWindowFullscreen(window, 0);
				resultDeviceName = screenDeviceName;
				center = true;
			}

			// Window always gets centered on changes, per XNA behavior
			if (center)
			{
				int pos = SDL.SDL_WINDOWPOS_CENTERED_DISPLAY(displayIndex);
				SDL.SDL_SetWindowPosition(
					window,
					pos,
					pos
				);
			}

			// Set fullscreen after we've done all the ugly stuff.
			if (wantsFullscreen)
			{
				if ((SDL.SDL_GetWindowFlags(window) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) == 0)
				{
					/* If we're still hidden, we can't actually go fullscreen yet.
					 * But, we can at least set the hidden window size to match
					 * what the window/drawable sizes will eventually be later.
					 * -flibit
					 */
					SDL.SDL_DisplayMode mode;
					SDL.SDL_GetCurrentDisplayMode(
						displayIndex,
						out mode
					);
					SDL.SDL_SetWindowSize(window, mode.w, mode.h);
				}
				SDL.SDL_SetWindowFullscreen(
					window,
					(uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP
				);
			}
		}

		public static Rectangle GetWindowBounds(IntPtr window)
		{
			Rectangle result;
			if ((SDL.SDL_GetWindowFlags(window) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0)
			{
				/* FIXME: SDL2 bug!
				 * SDL's a little weird about SDL_GetWindowSize.
				 * If you call it early enough (for example,
				 * Game.Initialize()), it reports outdated ints.
				 * So you know what, let's just use this.
				 * -flibit
				 */
				SDL.SDL_DisplayMode mode;
				SDL.SDL_GetCurrentDisplayMode(
					SDL.SDL_GetWindowDisplayIndex(
						window
					),
					out mode
				);
				result.X = 0;
				result.Y = 0;
				result.Width = mode.w;
				result.Height = mode.h;
			}
			else
			{
				SDL.SDL_GetWindowPosition(
					window,
					out result.X,
					out result.Y
				);
				SDL.SDL_GetWindowSize(
					window,
					out result.Width,
					out result.Height
				);
			}
			return result;
		}

		public static bool GetWindowResizable(IntPtr window)
		{
			return ((SDL.SDL_GetWindowFlags(window) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != 0);
		}

		public static void SetWindowResizable(IntPtr window, bool resizable)
		{
			SDL.SDL_SetWindowResizable(
				window,
				resizable ?
					SDL.SDL_bool.SDL_TRUE :
					SDL.SDL_bool.SDL_FALSE
			);
		}

		public static bool GetWindowBorderless(IntPtr window)
		{
			return ((SDL.SDL_GetWindowFlags(window) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0);
		}

		public static void SetWindowBorderless(IntPtr window, bool borderless)
		{
			SDL.SDL_SetWindowBordered(
				window,
				borderless ?
					SDL.SDL_bool.SDL_FALSE :
					SDL.SDL_bool.SDL_TRUE
			);
		}

		public static void SetWindowTitle(IntPtr window, string title)
		{
			SDL.SDL_SetWindowTitle(
				window,
				title
			);
		}

		private static void INTERNAL_SetIcon(IntPtr window, string title)
		{
			string fileIn = String.Empty;

			/* If the game's using SDL2_image, provide the option to use a PNG
			 * instead of a BMP. Nice for anyone who cares about transparency.
			 * -flibit
			 */
			try
			{
				fileIn = INTERNAL_GetIconName(title + ".png");
				if (!String.IsNullOrEmpty(fileIn))
				{
					IntPtr icon = SDL_image.IMG_Load(fileIn);
					SDL.SDL_SetWindowIcon(window, icon);
					SDL.SDL_FreeSurface(icon);
					return;
				}
			}
			catch(DllNotFoundException)
			{
				// Not that big a deal guys.
			}

			fileIn = INTERNAL_GetIconName(title + ".bmp");
			if (!String.IsNullOrEmpty(fileIn))
			{
				IntPtr icon = SDL.SDL_LoadBMP(fileIn);
				SDL.SDL_SetWindowIcon(window, icon);
				SDL.SDL_FreeSurface(icon);
			}
		}

		private static string INTERNAL_GetIconName(string title)
		{
			string fileIn = Path.Combine(TitleLocation.Path, title);
			if (File.Exists(fileIn))
			{
				// If the title and filename work, it just works. Fine.
				return fileIn;
			}
			else
			{
				// But sometimes the title has invalid characters inside.
				fileIn = Path.Combine(
					TitleLocation.Path,
					INTERNAL_StripBadChars(title)
				);
				if (File.Exists(fileIn))
				{
					return fileIn;
				}
			}
			return String.Empty;
		}

		private static string INTERNAL_StripBadChars(string path)
		{
			/* In addition to the filesystem's invalid charset, we need to
			 * blacklist the Windows standard set too, no matter what.
			 * -flibit
			 */
			char[] hardCodeBadChars = new char[]
			{
				'<',
				'>',
				':',
				'"',
				'/',
				'\\',
				'|',
				'?',
				'*'
			};
			List<char> badChars = new List<char>();
			badChars.AddRange(Path.GetInvalidFileNameChars());
			badChars.AddRange(hardCodeBadChars);

			string stripChars = path;
			foreach (char c in badChars)
			{
				stripChars = stripChars.Replace(c.ToString(), "");
			}
			return stripChars;
		}

		public static void SetTextInputRectangle(Rectangle rectangle)
		{
			SDL.SDL_Rect rect = new SDL.SDL_Rect();
			rect.x = rectangle.X;
			rect.y = rectangle.Y;
			rect.w = rectangle.Width;
			rect.h = rectangle.Height;
			SDL.SDL_SetTextInputRect(ref rect);
		}

		#endregion

		#region Event Loop

		public static void RunLoop(Game game)
		{
			SDL.SDL_ShowWindow(game.Window.Handle);
			game.IsActive = true;

			Rectangle windowBounds = game.Window.ClientBounds;
			Mouse.INTERNAL_WindowWidth = windowBounds.Width;
			Mouse.INTERNAL_WindowHeight = windowBounds.Height;

			// Which display did we end up on?
			int displayIndex = SDL.SDL_GetWindowDisplayIndex(
				game.Window.Handle
			);

			// Store this for internal event filter work
			activeGames.Add(game);

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
			if (UseScancodes)
			{
				FNALoggerEXT.LogInfo("Using scancodes instead of keycodes!");
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
						Keys key = ToXNAKey(ref evt.key.keysym);
						if (!keys.Contains(key))
						{
							keys.Add(key);
							int textIndex;
							if (textInputBindings.TryGetValue(key, out textIndex))
							{
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
						Keys key = ToXNAKey(ref evt.key.keysym);
						if (keys.Remove(key))
						{
							int value;
							if (textInputBindings.TryGetValue(key, out value))
							{
								textInputControlDown[value] = false;
							}
							else if ((!keys.Contains(Keys.LeftControl) && textInputControlDown[3]) || key == Keys.V)
							{
								textInputControlDown[6] = false;
								textInputSuppress = false;
							}
						}
					}

					// Mouse Input
					else if (evt.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
					{
						Mouse.INTERNAL_onClicked(evt.button.button - 1);
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
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED)
						{
							// This is called on both API and WM resizes
							Mouse.INTERNAL_WindowWidth = evt.window.data1;
							Mouse.INTERNAL_WindowHeight = evt.window.data2;
						}
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
						{
							// This is only called on WM resizes, right after SIZE_CHANGED
							((FNAWindow) game.Window).INTERNAL_ClientSizeChanged();
						}
						else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED)
						{
							// This is typically called when the window is made bigger
							game.RedrawWindow();
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
						// Based on the SDL2# LPUtf8StrMarshaler
						unsafe
						{
							byte* endPtr = evt.text.text;
							if (*endPtr != 0)
							{
								int bytes = 0;
								while (*endPtr != 0)
								{
									endPtr++;
									bytes += 1;
								}

								/* UTF8 will never encode more characters
								 * than bytes in a string, so bytes is a
								 * suitable upper estimate of size needed
								 */
								char* charsBuffer = stackalloc char[bytes];
								int chars = Encoding.UTF8.GetChars(
									evt.text.text,
									bytes,
									charsBuffer,
									bytes
								);

								for (int i = 0; i < chars; i += 1)
								{
								   TextInputEXT.OnTextInput(charsBuffer[i]);
								}
							}
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

			// Okay, we don't care about the events anymore
			activeGames.Remove(game);

			// We out.
			game.Exit();
		}

		#endregion

		#region IGL/IAL Methods

		public static IGLDevice CreateGLDevice(
			PresentationParameters presentationParameters,
			GraphicsAdapter adapter
		) {
			// This loads the OpenGL entry points.
			if (Environment.GetEnvironmentVariable("FNA_GRAPHICS_FORCE_GLDEVICE") == "ModernGLDevice")
			{
				// FIXME: This is still experimental! -flibit
				return new ModernGLDevice(presentationParameters, adapter);
			}
			return new OpenGLDevice(presentationParameters, adapter);
		}

		public static IALDevice CreateALDevice()
		{
			try
			{
				return new OpenALDevice();
			}
			catch(DllNotFoundException e)
			{
				FNALoggerEXT.LogError("OpenAL not found! Need FNA.dll.config?");
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

		#endregion

		#region Graphics Methods

		public static void SetPresentationInterval(PresentInterval interval)
		{
			if (interval == PresentInterval.Default || interval == PresentInterval.One)
			{
				bool disableLateSwapTear = (
					OSVersion.Equals("Mac OS X") ||
					Environment.GetEnvironmentVariable("FNA_OPENGL_DISABLE_LATESWAPTEAR") == "1"
				);
				if (disableLateSwapTear)
				{
					SDL.SDL_GL_SetSwapInterval(1);
				}
				else
				{
					if (SDL.SDL_GL_SetSwapInterval(-1) != -1)
					{
						FNALoggerEXT.LogInfo("Using EXT_swap_control_tear VSync!");
					}
					else
					{
						FNALoggerEXT.LogInfo("EXT_swap_control_tear unsupported. Fall back to standard VSync.");
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
				for (int j = numModes - 1; j >= 0; j -= 1)
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
				adapters[i] = new GraphicsAdapter(
					new DisplayModeCollection(modes),
					@"\\.\DISPLAY" + (i + 1).ToString(),
					SDL.SDL_GetDisplayName(i)
				);
			}
			return adapters;
		}

		public static DisplayMode GetCurrentDisplayMode(int adapterIndex)
		{
			SDL.SDL_DisplayMode filler = new SDL.SDL_DisplayMode();
			SDL.SDL_GetCurrentDisplayMode(adapterIndex, out filler);
			return new DisplayMode(
				filler.w,
				filler.h,
				SurfaceFormat.Color // FIXME: Assumption!
			);
		}

		#endregion

		#region Mouse Methods

		public static void GetMouseState(
			IntPtr window,
			out int x,
			out int y,
			out ButtonState left,
			out ButtonState middle,
			out ButtonState right,
			out ButtonState x1,
			out ButtonState x2
		) {
			uint flags;
			if (GetRelativeMouseMode())
			{
				flags = SDL.SDL_GetRelativeMouseState(out x, out y);
			}
			else
			{
				flags = SDL.SDL_GetGlobalMouseState(out x, out y);
				int wx = 0, wy = 0;
				SDL.SDL_GetWindowPosition(window, out wx, out wy);
				x -= wx;
				y -= wy;
			}
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

		public static bool GetRelativeMouseMode()
		{
			return SDL.SDL_GetRelativeMouseMode() == SDL.SDL_bool.SDL_TRUE;
		}

		public static void SetRelativeMouseMode(bool enable)
		{
			SDL.SDL_SetRelativeMouseMode(
				enable ?
					SDL.SDL_bool.SDL_TRUE :
					SDL.SDL_bool.SDL_FALSE
			);
		}

		#endregion

		#region Storage Methods

		private static string GetBaseDirectory()
		{
			if (	OSVersion.Equals("Windows") ||
				OSVersion.Equals("Mac OS X") ||
				OSVersion.Equals("Linux") ||
				OSVersion.Equals("FreeBSD") ||
				OSVersion.Equals("OpenBSD") ||
				OSVersion.Equals("NetBSD")	)
			{
				/* This is mostly here for legacy compatibility.
				 * For most platforms this should be the same as
				 * SDL_GetBasePath, but some platforms (Apple's)
				 * will have a separate Resources folder that is
				 * the "base" directory for applications.
				 *
				 * TODO: Remove this and endure the breakage.
				 * -flibit
				 */
				return AppDomain.CurrentDomain.BaseDirectory;
			}
			string result = SDL.SDL_GetBasePath();
			if (string.IsNullOrEmpty(result))
			{
				result = AppDomain.CurrentDomain.BaseDirectory;
			}
			return result;
		}

		public static string GetStorageRoot()
		{
			// Generate the path of the game's savefolder
			string exeName = Path.GetFileNameWithoutExtension(
				AppDomain.CurrentDomain.FriendlyName
			).Replace(".vshost", "");

			// Get the OS save folder, append the EXE name
			if (OSVersion.Equals("Windows"))
			{
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"SavedGames",
					exeName
				);
			}
			if (OSVersion.Equals("Mac OS X"))
			{
				string osConfigDir = Environment.GetEnvironmentVariable("HOME");
				if (String.IsNullOrEmpty(osConfigDir))
				{
					return "."; // Oh well.
				}
				return Path.Combine(
					osConfigDir,
					"/Library/Application Support",
					exeName
				);
			}
			if (	OSVersion.Equals("Linux") ||
				OSVersion.Equals("FreeBSD") ||
				OSVersion.Equals("OpenBSD") ||
				OSVersion.Equals("NetBSD")	)
			{
				// Assuming a non-macOS Unix platform will follow the XDG. Which it should.
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
				return Path.Combine(osConfigDir, exeName);
			}

			/* There is a minor inaccuracy here: SDL_GetPrefPath
			 * creates the directories right away, whereas XNA will
			 * only create the directory upon creating a container.
			 * So if you create a StorageDevice and hit a property,
			 * the game folder is made early!
			 * -flibit
			 */
			return SDL.SDL_GetPrefPath(null, exeName);
		}

		#endregion

		#region Logging/Messaging Methods

		public static void ShowRuntimeError(string title, string message)
		{
			SDL.SDL_ShowSimpleMessageBox(
				SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
				title,
				message,
				IntPtr.Zero
			);
		}

		#endregion

		#region Image I/O Methods

		public static void TextureDataFromStream(
			Stream stream,
			out int width,
			out int height,
			out byte[] pixels,
			int reqWidth = -1,
			int reqHeight = -1,
			bool zoom = false
		) {
			// Load the SDL_Surface* from RWops, get the image data
			FakeRWops reader = new FakeRWops(stream);
			IntPtr surface = SDL_image.IMG_Load_RW(reader.rwops, 0);
			reader.Free();
			if (surface == IntPtr.Zero)
			{
				// File not found, supported, etc.
				FNALoggerEXT.LogError(
					"TextureDataFromStream: " +
					SDL.SDL_GetError()
				);
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
			IntPtr surface = INTERNAL_getScaledSurface(
				data,
				imgWidth,
				imgHeight,
				width,
				height
			);
			FakeRWops writer = new FakeRWops(stream);
			SDL_image.IMG_SavePNG_RW(surface, writer.rwops, 0);
			writer.Free();
			SDL.SDL_FreeSurface(surface);
		}

		public static void SaveJPG(
			Stream stream,
			int width,
			int height,
			int imgWidth,
			int imgHeight,
			byte[] data
		) {
			// FIXME: What does XNA pick for this? -flibit
			const int quality = 100;

			IntPtr surface = INTERNAL_getScaledSurface(
				data,
				imgWidth,
				imgHeight,
				width,
				height
			);

			// FIXME: Hack for Bugzilla #3972
			IntPtr temp = SDL.SDL_ConvertSurfaceFormat(
				surface,
				SDL.SDL_PIXELFORMAT_RGB24,
				0
			);
			SDL.SDL_FreeSurface(surface);
			surface = temp;

			FakeRWops writer = new FakeRWops(stream);
			SDL_image.IMG_SaveJPG_RW(surface, writer.rwops, 0, quality);
			writer.Free();
			SDL.SDL_FreeSurface(surface);
		}

		public static IntPtr INTERNAL_getScaledSurface(
			byte[] data,
			int srcW,
			int srcH,
			int dstW,
			int dstH
		) {
			// Create an SDL_Surface*, write the pixel data
			IntPtr surface = SDL.SDL_CreateRGBSurface(
				0,
				srcW,
				srcH,
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

			// Blit to a scaled surface of the size we want, if needed.
			if (srcW != dstW || srcH != dstH)
			{
				IntPtr scaledSurface = SDL.SDL_CreateRGBSurface(
					0,
					dstW,
					dstH,
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
				SDL.SDL_BlitScaled(
					surface,
					IntPtr.Zero,
					scaledSurface,
					IntPtr.Zero
				);
				SDL.SDL_FreeSurface(surface);
				surface = scaledSurface;
			}

			return surface;
		}

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

		private class FakeRWops
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate long SizeFunc(IntPtr context);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate long SeekFunc(
				IntPtr context,
				long offset,
				int whence
			);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate IntPtr ReadFunc(
				IntPtr context,
				IntPtr ptr,
				IntPtr size,
				IntPtr maxnum
			);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			private delegate IntPtr WriteFunc(
				IntPtr context,
				IntPtr ptr,
				IntPtr size,
				IntPtr num
			);

			[StructLayout(LayoutKind.Sequential)]
			private struct PartialRWops
			{
				public IntPtr size;
				public IntPtr seek;
				public IntPtr read;
				public IntPtr write;
			}

			[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr SDL_AllocRW();

			[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
			private static extern void SDL_FreeRW(IntPtr area);

			public readonly IntPtr rwops;
			private Stream stream;
			private byte[] temp;

			private SizeFunc sizeFunc;
			private SeekFunc seekFunc;
			private ReadFunc readFunc;
			private WriteFunc writeFunc;

			public FakeRWops(Stream stream)
			{
				this.stream = stream;
				rwops = SDL_AllocRW();
				temp = new byte[8192]; // Based on PNG_ZBUF_SIZE default

				sizeFunc = size;
				seekFunc = seek;
				readFunc = read;
				writeFunc = write;
				unsafe
				{
					PartialRWops* p = (PartialRWops*) rwops;
					p->size = Marshal.GetFunctionPointerForDelegate(sizeFunc);
					p->seek = Marshal.GetFunctionPointerForDelegate(seekFunc);
					p->read = Marshal.GetFunctionPointerForDelegate(readFunc);
					p->write = Marshal.GetFunctionPointerForDelegate(writeFunc);
				}
			}

			public void Free()
			{
				SDL_FreeRW(rwops);
				stream = null;
				temp = null;
			}

			private byte[] GetTemp(int len)
			{
				if (len > temp.Length)
				{
					temp = new byte[len];
				}
				return temp;
			}

			private long size(IntPtr context)
			{
				return -1;
			}

			private long seek(IntPtr context, long offset, int whence)
			{
				stream.Seek(offset, (SeekOrigin) whence);
				return stream.Position;
			}

			private IntPtr read(
				IntPtr context,
				IntPtr ptr,
				IntPtr size,
				IntPtr maxnum
			) {
				int len = size.ToInt32() * maxnum.ToInt32();
				len = stream.Read(
					GetTemp(len),
					0,
					len
				);
				Marshal.Copy(temp, 0, ptr, len);
				return (IntPtr) len;
			}

			private IntPtr write(
				IntPtr context,
				IntPtr ptr,
				IntPtr size,
				IntPtr num
			) {
				int len = size.ToInt32() * num.ToInt32();
				Marshal.Copy(
					ptr,
					GetTemp(len),
					0,
					len
				);
				stream.Write(temp, 0, len);
				return (IntPtr) len;
			}
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

		private static readonly GamePadType[] INTERNAL_gamepadType = new GamePadType[]
		{
			GamePadType.Unknown,
			GamePadType.GamePad,
			GamePadType.Wheel,
			GamePadType.ArcadeStick,
			GamePadType.FlightStick,
			GamePadType.DancePad,
			GamePadType.Guitar,
			GamePadType.DrumKit,
			GamePadType.BigButtonPad
		};

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

			// The "master" button state is built from this.
			Buttons gc_buttonState = (Buttons) 0;

			// Sticks
			Vector2 stickLeft = new Vector2(
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
				) / 32767.0f,
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
				) / -32767.0f
			);
			Vector2 stickRight = new Vector2(
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
				) / 32767.0f,
				(float) SDL.SDL_GameControllerGetAxis(
					device,
					SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
				) / -32767.0f
			);
			gc_buttonState |= READ_StickToButtons(
				stickLeft,
				Buttons.LeftThumbstickLeft,
				Buttons.LeftThumbstickRight,
				Buttons.LeftThumbstickUp,
				Buttons.LeftThumbstickDown,
				GamePad.LeftDeadZone
			);
			gc_buttonState |= READ_StickToButtons(
				stickRight,
				Buttons.RightThumbstickLeft,
				Buttons.RightThumbstickRight,
				Buttons.RightThumbstickUp,
				Buttons.RightThumbstickDown,
				GamePad.RightDeadZone
			);

			// Triggers
			float triggerLeft = (float) SDL.SDL_GameControllerGetAxis(
				device,
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
			) / 32767.0f;
			float triggerRight = (float) SDL.SDL_GameControllerGetAxis(
				device,
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
			) / 32767.0f;
			if (triggerLeft > GamePad.TriggerThreshold)
			{
				gc_buttonState |= Buttons.LeftTrigger;
			}
			if (triggerRight > GamePad.TriggerThreshold)
			{
				gc_buttonState |= Buttons.RightTrigger;
			}

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

			// Build the GamePadState, increment PacketNumber if state changed.
			GamePadState gc_builtState = new GamePadState(
				new GamePadThumbSticks(stickLeft, stickRight, deadZoneMode),
				new GamePadTriggers(triggerLeft, triggerRight, deadZoneMode),
				new GamePadButtons(gc_buttonState),
				new GamePadDPad(gc_buttonState)
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
				INTERNAL_leftRightEffect.leftright.large_magnitude = (ushort) (HapticMaxWorkaround * leftMotor);
				INTERNAL_leftRightEffect.leftright.small_magnitude = (ushort) (HapticMaxWorkaround * rightMotor);
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
				leftRightMacHackData[0] = (ushort) (HapticMaxWorkaround * leftMotor);
				leftRightMacHackData[1] = (ushort) (HapticMaxWorkaround * rightMotor);
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
					FNALoggerEXT.LogError("HAPTIC OPEN ERROR: " + SDL.SDL_GetError());
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
			GamePadCapabilities caps = new GamePadCapabilities();
			caps.IsConnected = true;
			caps.GamePadType = INTERNAL_gamepadType[(int) SDL.SDL_JoystickGetType(thisJoystick)];
			caps.HasAButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasBButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasXButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasYButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasBackButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasBigButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasStartButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftStickButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasRightStickButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftShoulderButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasRightShoulderButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasDPadUpButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasDPadDownButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasDPadLeftButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasDPadRightButton = SDL.SDL_GameControllerGetBindForButton(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftXThumbStick = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftYThumbStick = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasRightXThumbStick = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasRightYThumbStick = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftTrigger = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasRightTrigger = SDL.SDL_GameControllerGetBindForAxis(
				INTERNAL_devices[which],
				SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
			).bindType != SDL.SDL_GameControllerBindType.SDL_CONTROLLER_BINDTYPE_NONE;
			caps.HasLeftVibrationMotor = INTERNAL_haptics[which] != IntPtr.Zero;
			caps.HasRightVibrationMotor = INTERNAL_haptics[which] != IntPtr.Zero;
			caps.HasVoiceSupport = false;
			INTERNAL_capabilities[which] = caps;

			/* Store the GUID string for this device
			 * FIXME: Replace GetGUIDEXT string with 3 short values -flibit
			 */
			ushort vendor = SDL.SDL_JoystickGetVendor(thisJoystick);
			ushort product = SDL.SDL_JoystickGetProduct(thisJoystick);
			if (vendor == 0x00 && product == 0x00)
			{
				INTERNAL_guids[which] = "xinput";
			}
			else
			{
				INTERNAL_guids[which] = string.Format(
					"{0:x2}{1:x2}{2:x2}{3:x2}",
					vendor & 0xFF,
					vendor >> 8,
					product & 0xFF,
					product >> 8
				);
			}

			// Initialize light bar
			if (	OSVersion.Equals("Linux") &&
				(	INTERNAL_guids[which].Equals("4c05c405") ||
					INTERNAL_guids[which].Equals("4c05cc09")	)	)
			{
				// Get all of the individual PS4 LED instances
				List<string> ledList = new List<string>();
				string[] dirs = Directory.GetDirectories("/sys/class/leds/");
				foreach (string dir in dirs)
				{
					if (	dir.EndsWith("blue") &&
						(	dir.Contains("054C:05C4") ||
							dir.Contains("054C:09CC")	)	)
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
			FNALoggerEXT.LogInfo(
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

			FNALoggerEXT.LogInfo("Removed device, player: " + output.ToString());
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

		#region SDL2<->XNA Key Conversion Methods

		/* From: http://blogs.msdn.com/b/shawnhar/archive/2007/07/02/twin-paths-to-garbage-collector-nirvana.aspx
		 * "If you use an enum type as a dictionary key, internal dictionary operations will cause boxing.
		 * You can avoid this by using integer keys, and casting your enum values to ints before adding
		 * them to the dictionary."
		 */
		private static Dictionary<int, Keys> INTERNAL_keyMap = new Dictionary<int, Keys>()
		{
			{ (int) SDL.SDL_Keycode.SDLK_a,			Keys.A },
			{ (int) SDL.SDL_Keycode.SDLK_b,			Keys.B },
			{ (int) SDL.SDL_Keycode.SDLK_c,			Keys.C },
			{ (int) SDL.SDL_Keycode.SDLK_d,			Keys.D },
			{ (int) SDL.SDL_Keycode.SDLK_e,			Keys.E },
			{ (int) SDL.SDL_Keycode.SDLK_f,			Keys.F },
			{ (int) SDL.SDL_Keycode.SDLK_g,			Keys.G },
			{ (int) SDL.SDL_Keycode.SDLK_h,			Keys.H },
			{ (int) SDL.SDL_Keycode.SDLK_i,			Keys.I },
			{ (int) SDL.SDL_Keycode.SDLK_j,			Keys.J },
			{ (int) SDL.SDL_Keycode.SDLK_k,			Keys.K },
			{ (int) SDL.SDL_Keycode.SDLK_l,			Keys.L },
			{ (int) SDL.SDL_Keycode.SDLK_m,			Keys.M },
			{ (int) SDL.SDL_Keycode.SDLK_n,			Keys.N },
			{ (int) SDL.SDL_Keycode.SDLK_o,			Keys.O },
			{ (int) SDL.SDL_Keycode.SDLK_p,			Keys.P },
			{ (int) SDL.SDL_Keycode.SDLK_q,			Keys.Q },
			{ (int) SDL.SDL_Keycode.SDLK_r,			Keys.R },
			{ (int) SDL.SDL_Keycode.SDLK_s,			Keys.S },
			{ (int) SDL.SDL_Keycode.SDLK_t,			Keys.T },
			{ (int) SDL.SDL_Keycode.SDLK_u,			Keys.U },
			{ (int) SDL.SDL_Keycode.SDLK_v,			Keys.V },
			{ (int) SDL.SDL_Keycode.SDLK_w,			Keys.W },
			{ (int) SDL.SDL_Keycode.SDLK_x,			Keys.X },
			{ (int) SDL.SDL_Keycode.SDLK_y,			Keys.Y },
			{ (int) SDL.SDL_Keycode.SDLK_z,			Keys.Z },
			{ (int) SDL.SDL_Keycode.SDLK_0,			Keys.D0 },
			{ (int) SDL.SDL_Keycode.SDLK_1,			Keys.D1 },
			{ (int) SDL.SDL_Keycode.SDLK_2,			Keys.D2 },
			{ (int) SDL.SDL_Keycode.SDLK_3,			Keys.D3 },
			{ (int) SDL.SDL_Keycode.SDLK_4,			Keys.D4 },
			{ (int) SDL.SDL_Keycode.SDLK_5,			Keys.D5 },
			{ (int) SDL.SDL_Keycode.SDLK_6,			Keys.D6 },
			{ (int) SDL.SDL_Keycode.SDLK_7,			Keys.D7 },
			{ (int) SDL.SDL_Keycode.SDLK_8,			Keys.D8 },
			{ (int) SDL.SDL_Keycode.SDLK_9,			Keys.D9 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_0,		Keys.NumPad0 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_1,		Keys.NumPad1 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_2,		Keys.NumPad2 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_3,		Keys.NumPad3 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_4,		Keys.NumPad4 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_5,		Keys.NumPad5 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_6,		Keys.NumPad6 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_7,		Keys.NumPad7 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_8,		Keys.NumPad8 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_9,		Keys.NumPad9 },
			{ (int) SDL.SDL_Keycode.SDLK_KP_CLEAR,		Keys.OemClear },
			{ (int) SDL.SDL_Keycode.SDLK_KP_DECIMAL,	Keys.Decimal },
			{ (int) SDL.SDL_Keycode.SDLK_KP_DIVIDE,		Keys.Divide },
			{ (int) SDL.SDL_Keycode.SDLK_KP_ENTER,		Keys.Enter },
			{ (int) SDL.SDL_Keycode.SDLK_KP_MINUS,		Keys.Subtract },
			{ (int) SDL.SDL_Keycode.SDLK_KP_MULTIPLY,	Keys.Multiply },
			{ (int) SDL.SDL_Keycode.SDLK_KP_PERIOD,		Keys.OemPeriod },
			{ (int) SDL.SDL_Keycode.SDLK_KP_PLUS,		Keys.Add },
			{ (int) SDL.SDL_Keycode.SDLK_F1,		Keys.F1 },
			{ (int) SDL.SDL_Keycode.SDLK_F2,		Keys.F2 },
			{ (int) SDL.SDL_Keycode.SDLK_F3,		Keys.F3 },
			{ (int) SDL.SDL_Keycode.SDLK_F4,		Keys.F4 },
			{ (int) SDL.SDL_Keycode.SDLK_F5,		Keys.F5 },
			{ (int) SDL.SDL_Keycode.SDLK_F6,		Keys.F6 },
			{ (int) SDL.SDL_Keycode.SDLK_F7,		Keys.F7 },
			{ (int) SDL.SDL_Keycode.SDLK_F8,		Keys.F8 },
			{ (int) SDL.SDL_Keycode.SDLK_F9,		Keys.F9 },
			{ (int) SDL.SDL_Keycode.SDLK_F10,		Keys.F10 },
			{ (int) SDL.SDL_Keycode.SDLK_F11,		Keys.F11 },
			{ (int) SDL.SDL_Keycode.SDLK_F12,		Keys.F12 },
			{ (int) SDL.SDL_Keycode.SDLK_F13,		Keys.F13 },
			{ (int) SDL.SDL_Keycode.SDLK_F14,		Keys.F14 },
			{ (int) SDL.SDL_Keycode.SDLK_F15,		Keys.F15 },
			{ (int) SDL.SDL_Keycode.SDLK_F16,		Keys.F16 },
			{ (int) SDL.SDL_Keycode.SDLK_F17,		Keys.F17 },
			{ (int) SDL.SDL_Keycode.SDLK_F18,		Keys.F18 },
			{ (int) SDL.SDL_Keycode.SDLK_F19,		Keys.F19 },
			{ (int) SDL.SDL_Keycode.SDLK_F20,		Keys.F20 },
			{ (int) SDL.SDL_Keycode.SDLK_F21,		Keys.F21 },
			{ (int) SDL.SDL_Keycode.SDLK_F22,		Keys.F22 },
			{ (int) SDL.SDL_Keycode.SDLK_F23,		Keys.F23 },
			{ (int) SDL.SDL_Keycode.SDLK_F24,		Keys.F24 },
			{ (int) SDL.SDL_Keycode.SDLK_SPACE,		Keys.Space },
			{ (int) SDL.SDL_Keycode.SDLK_UP,		Keys.Up },
			{ (int) SDL.SDL_Keycode.SDLK_DOWN,		Keys.Down },
			{ (int) SDL.SDL_Keycode.SDLK_LEFT,		Keys.Left },
			{ (int) SDL.SDL_Keycode.SDLK_RIGHT,		Keys.Right },
			{ (int) SDL.SDL_Keycode.SDLK_LALT,		Keys.LeftAlt },
			{ (int) SDL.SDL_Keycode.SDLK_RALT,		Keys.RightAlt },
			{ (int) SDL.SDL_Keycode.SDLK_LCTRL,		Keys.LeftControl },
			{ (int) SDL.SDL_Keycode.SDLK_RCTRL,		Keys.RightControl },
			{ (int) SDL.SDL_Keycode.SDLK_LGUI,		Keys.LeftWindows },
			{ (int) SDL.SDL_Keycode.SDLK_RGUI,		Keys.RightWindows },
			{ (int) SDL.SDL_Keycode.SDLK_LSHIFT,		Keys.LeftShift },
			{ (int) SDL.SDL_Keycode.SDLK_RSHIFT,		Keys.RightShift },
			{ (int) SDL.SDL_Keycode.SDLK_APPLICATION,	Keys.Apps },
			{ (int) SDL.SDL_Keycode.SDLK_SLASH,		Keys.OemQuestion },
			{ (int) SDL.SDL_Keycode.SDLK_BACKSLASH,		Keys.OemBackslash },
			{ (int) SDL.SDL_Keycode.SDLK_LEFTBRACKET,	Keys.OemOpenBrackets },
			{ (int) SDL.SDL_Keycode.SDLK_RIGHTBRACKET,	Keys.OemCloseBrackets },
			{ (int) SDL.SDL_Keycode.SDLK_CAPSLOCK,		Keys.CapsLock },
			{ (int) SDL.SDL_Keycode.SDLK_COMMA,		Keys.OemComma },
			{ (int) SDL.SDL_Keycode.SDLK_DELETE,		Keys.Delete },
			{ (int) SDL.SDL_Keycode.SDLK_END,		Keys.End },
			{ (int) SDL.SDL_Keycode.SDLK_BACKSPACE,		Keys.Back },
			{ (int) SDL.SDL_Keycode.SDLK_RETURN,		Keys.Enter },
			{ (int) SDL.SDL_Keycode.SDLK_ESCAPE,		Keys.Escape },
			{ (int) SDL.SDL_Keycode.SDLK_HOME,		Keys.Home },
			{ (int) SDL.SDL_Keycode.SDLK_INSERT,		Keys.Insert },
			{ (int) SDL.SDL_Keycode.SDLK_MINUS,		Keys.OemMinus },
			{ (int) SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR,	Keys.NumLock },
			{ (int) SDL.SDL_Keycode.SDLK_PAGEUP,		Keys.PageUp },
			{ (int) SDL.SDL_Keycode.SDLK_PAGEDOWN,		Keys.PageDown },
			{ (int) SDL.SDL_Keycode.SDLK_PAUSE,		Keys.Pause },
			{ (int) SDL.SDL_Keycode.SDLK_PERIOD,		Keys.OemPeriod },
			{ (int) SDL.SDL_Keycode.SDLK_EQUALS,		Keys.OemPlus },
			{ (int) SDL.SDL_Keycode.SDLK_PRINTSCREEN,	Keys.PrintScreen },
			{ (int) SDL.SDL_Keycode.SDLK_QUOTE,		Keys.OemQuotes },
			{ (int) SDL.SDL_Keycode.SDLK_SCROLLLOCK,	Keys.Scroll },
			{ (int) SDL.SDL_Keycode.SDLK_SEMICOLON,		Keys.OemSemicolon },
			{ (int) SDL.SDL_Keycode.SDLK_SLEEP,		Keys.Sleep },
			{ (int) SDL.SDL_Keycode.SDLK_TAB,		Keys.Tab },
			{ (int) SDL.SDL_Keycode.SDLK_BACKQUOTE,		Keys.OemTilde },
			{ (int) SDL.SDL_Keycode.SDLK_VOLUMEUP,		Keys.VolumeUp },
			{ (int) SDL.SDL_Keycode.SDLK_VOLUMEDOWN,	Keys.VolumeDown },
			{ '²' /* FIXME: AZERTY SDL2? -flibit */,	Keys.OemTilde },
			{ 'é' /* FIXME: BEPO SDL2? -flibit */,		Keys.None },
			{ '|' /* FIXME: Norwegian SDL2? -flibit */,	Keys.OemPipe },
			{ '+' /* FIXME: Norwegian SDL2? -flibit */,	Keys.OemPlus },
			{ 'ø' /* FIXME: Norwegian SDL2? -flibit */,	Keys.OemSemicolon },
			{ 'æ' /* FIXME: Norwegian SDL2? -flibit */,	Keys.OemQuotes },
			{ (int) SDL.SDL_Keycode.SDLK_UNKNOWN,		Keys.None }
		};
		private static Dictionary<int, Keys> INTERNAL_scanMap = new Dictionary<int, Keys>()
		{
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_A,		Keys.A },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_B,		Keys.B },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_C,		Keys.C },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_D,		Keys.D },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_E,		Keys.E },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F,		Keys.F },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_G,		Keys.G },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_H,		Keys.H },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_I,		Keys.I },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_J,		Keys.J },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_K,		Keys.K },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_L,		Keys.L },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_M,		Keys.M },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_N,		Keys.N },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_O,		Keys.O },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_P,		Keys.P },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_Q,		Keys.Q },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_R,		Keys.R },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_S,		Keys.S },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_T,		Keys.T },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_U,		Keys.U },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_V,		Keys.V },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_W,		Keys.W },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_X,		Keys.X },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_Y,		Keys.Y },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_Z,		Keys.Z },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_0,		Keys.D0 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_1,		Keys.D1 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_2,		Keys.D2 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_3,		Keys.D3 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_4,		Keys.D4 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_5,		Keys.D5 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_6,		Keys.D6 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_7,		Keys.D7 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_8,		Keys.D8 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_9,		Keys.D9 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_0,		Keys.NumPad0 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_1,		Keys.NumPad1 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_2,		Keys.NumPad2 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_3,		Keys.NumPad3 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_4,		Keys.NumPad4 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_5,		Keys.NumPad5 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_6,		Keys.NumPad6 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_7,		Keys.NumPad7 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_8,		Keys.NumPad8 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_9,		Keys.NumPad9 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR,		Keys.OemClear },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_DECIMAL,	Keys.Decimal },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE,	Keys.Divide },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_ENTER,		Keys.Enter },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS,		Keys.Subtract },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY,	Keys.Multiply },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_PERIOD,	Keys.OemPeriod },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS,		Keys.Add },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F1,		Keys.F1 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F2,		Keys.F2 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F3,		Keys.F3 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F4,		Keys.F4 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F5,		Keys.F5 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F6,		Keys.F6 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F7,		Keys.F7 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F8,		Keys.F8 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F9,		Keys.F9 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F10,		Keys.F10 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F11,		Keys.F11 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F12,		Keys.F12 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F13,		Keys.F13 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F14,		Keys.F14 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F15,		Keys.F15 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F16,		Keys.F16 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F17,		Keys.F17 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F18,		Keys.F18 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F19,		Keys.F19 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F20,		Keys.F20 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F21,		Keys.F21 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F22,		Keys.F22 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F23,		Keys.F23 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_F24,		Keys.F24 },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_SPACE,		Keys.Space },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_UP,		Keys.Up },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_DOWN,		Keys.Down },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LEFT,		Keys.Left },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHT,		Keys.Right },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LALT,		Keys.LeftAlt },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RALT,		Keys.RightAlt },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LCTRL,		Keys.LeftControl },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RCTRL,		Keys.RightControl },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LGUI,		Keys.LeftWindows },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RGUI,		Keys.RightWindows },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT,		Keys.LeftShift },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT,		Keys.RightShift },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_APPLICATION,	Keys.Apps },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_SLASH,		Keys.OemQuestion },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH,	Keys.OemBackslash },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET,	Keys.OemOpenBrackets },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET,	Keys.OemCloseBrackets },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK,		Keys.CapsLock },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_COMMA,		Keys.OemComma },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_DELETE,		Keys.Delete },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_END,		Keys.End },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE,	Keys.Back },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_RETURN,		Keys.Enter },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE,		Keys.Escape },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_HOME,		Keys.Home },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_INSERT,		Keys.Insert },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_MINUS,		Keys.OemMinus },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR,	Keys.NumLock },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP,		Keys.PageUp },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN,		Keys.PageDown },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_PAUSE,		Keys.Pause },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_PERIOD,		Keys.OemPeriod },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_EQUALS,		Keys.OemPlus },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN,	Keys.PrintScreen },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE,	Keys.OemQuotes },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK,	Keys.Scroll },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON,	Keys.OemSemicolon },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_SLEEP,		Keys.Sleep },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_TAB,		Keys.Tab },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_GRAVE,		Keys.OemTilde },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEUP,		Keys.VolumeUp },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN,	Keys.VolumeDown },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN,		Keys.None },
			/* FIXME: The following scancodes need verification! */
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_NONUSHASH,	Keys.None },
			{ (int) SDL.SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH,	Keys.None }
		};
		private static Dictionary<int, SDL.SDL_Scancode> INTERNAL_xnaMap = new Dictionary<int, SDL.SDL_Scancode>()
		{
			{ (int) Keys.A,			SDL.SDL_Scancode.SDL_SCANCODE_A },
			{ (int) Keys.B,			SDL.SDL_Scancode.SDL_SCANCODE_B },
			{ (int) Keys.C,			SDL.SDL_Scancode.SDL_SCANCODE_C },
			{ (int) Keys.D,			SDL.SDL_Scancode.SDL_SCANCODE_D },
			{ (int) Keys.E,			SDL.SDL_Scancode.SDL_SCANCODE_E },
			{ (int) Keys.F,			SDL.SDL_Scancode.SDL_SCANCODE_F },
			{ (int) Keys.G,			SDL.SDL_Scancode.SDL_SCANCODE_G },
			{ (int) Keys.H,			SDL.SDL_Scancode.SDL_SCANCODE_H },
			{ (int) Keys.I,			SDL.SDL_Scancode.SDL_SCANCODE_I },
			{ (int) Keys.J,			SDL.SDL_Scancode.SDL_SCANCODE_J },
			{ (int) Keys.K,			SDL.SDL_Scancode.SDL_SCANCODE_K },
			{ (int) Keys.L,			SDL.SDL_Scancode.SDL_SCANCODE_L },
			{ (int) Keys.M,			SDL.SDL_Scancode.SDL_SCANCODE_M },
			{ (int) Keys.N,			SDL.SDL_Scancode.SDL_SCANCODE_N },
			{ (int) Keys.O,			SDL.SDL_Scancode.SDL_SCANCODE_O },
			{ (int) Keys.P,			SDL.SDL_Scancode.SDL_SCANCODE_P },
			{ (int) Keys.Q,			SDL.SDL_Scancode.SDL_SCANCODE_Q },
			{ (int) Keys.R,			SDL.SDL_Scancode.SDL_SCANCODE_R },
			{ (int) Keys.S,			SDL.SDL_Scancode.SDL_SCANCODE_S },
			{ (int) Keys.T,			SDL.SDL_Scancode.SDL_SCANCODE_T },
			{ (int) Keys.U,			SDL.SDL_Scancode.SDL_SCANCODE_U },
			{ (int) Keys.V,			SDL.SDL_Scancode.SDL_SCANCODE_V },
			{ (int) Keys.W,			SDL.SDL_Scancode.SDL_SCANCODE_W },
			{ (int) Keys.X,			SDL.SDL_Scancode.SDL_SCANCODE_X },
			{ (int) Keys.Y,			SDL.SDL_Scancode.SDL_SCANCODE_Y },
			{ (int) Keys.Z,			SDL.SDL_Scancode.SDL_SCANCODE_Z },
			{ (int) Keys.D0,		SDL.SDL_Scancode.SDL_SCANCODE_0 },
			{ (int) Keys.D1,		SDL.SDL_Scancode.SDL_SCANCODE_1 },
			{ (int) Keys.D2,		SDL.SDL_Scancode.SDL_SCANCODE_2 },
			{ (int) Keys.D3,		SDL.SDL_Scancode.SDL_SCANCODE_3 },
			{ (int) Keys.D4,		SDL.SDL_Scancode.SDL_SCANCODE_4 },
			{ (int) Keys.D5,		SDL.SDL_Scancode.SDL_SCANCODE_5 },
			{ (int) Keys.D6,		SDL.SDL_Scancode.SDL_SCANCODE_6 },
			{ (int) Keys.D7,		SDL.SDL_Scancode.SDL_SCANCODE_7 },
			{ (int) Keys.D8,		SDL.SDL_Scancode.SDL_SCANCODE_8 },
			{ (int) Keys.D9,		SDL.SDL_Scancode.SDL_SCANCODE_9 },
			{ (int) Keys.NumPad0,		SDL.SDL_Scancode.SDL_SCANCODE_KP_0 },
			{ (int) Keys.NumPad1,		SDL.SDL_Scancode.SDL_SCANCODE_KP_1 },
			{ (int) Keys.NumPad2,		SDL.SDL_Scancode.SDL_SCANCODE_KP_2 },
			{ (int) Keys.NumPad3,		SDL.SDL_Scancode.SDL_SCANCODE_KP_3 },
			{ (int) Keys.NumPad4,		SDL.SDL_Scancode.SDL_SCANCODE_KP_4 },
			{ (int) Keys.NumPad5,		SDL.SDL_Scancode.SDL_SCANCODE_KP_5 },
			{ (int) Keys.NumPad6,		SDL.SDL_Scancode.SDL_SCANCODE_KP_6 },
			{ (int) Keys.NumPad7,		SDL.SDL_Scancode.SDL_SCANCODE_KP_7 },
			{ (int) Keys.NumPad8,		SDL.SDL_Scancode.SDL_SCANCODE_KP_8 },
			{ (int) Keys.NumPad9,		SDL.SDL_Scancode.SDL_SCANCODE_KP_9 },
			{ (int) Keys.OemClear,		SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR },
			{ (int) Keys.Decimal,		SDL.SDL_Scancode.SDL_SCANCODE_KP_DECIMAL },
			{ (int) Keys.Divide,		SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE },
			{ (int) Keys.Multiply,		SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY },
			{ (int) Keys.Subtract,		SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS },
			{ (int) Keys.Add,		SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS },
			{ (int) Keys.F1,		SDL.SDL_Scancode.SDL_SCANCODE_F1 },
			{ (int) Keys.F2,		SDL.SDL_Scancode.SDL_SCANCODE_F2 },
			{ (int) Keys.F3,		SDL.SDL_Scancode.SDL_SCANCODE_F3 },
			{ (int) Keys.F4,		SDL.SDL_Scancode.SDL_SCANCODE_F4 },
			{ (int) Keys.F5,		SDL.SDL_Scancode.SDL_SCANCODE_F5 },
			{ (int) Keys.F6,		SDL.SDL_Scancode.SDL_SCANCODE_F6 },
			{ (int) Keys.F7,		SDL.SDL_Scancode.SDL_SCANCODE_F7 },
			{ (int) Keys.F8,		SDL.SDL_Scancode.SDL_SCANCODE_F8 },
			{ (int) Keys.F9,		SDL.SDL_Scancode.SDL_SCANCODE_F9 },
			{ (int) Keys.F10,		SDL.SDL_Scancode.SDL_SCANCODE_F10 },
			{ (int) Keys.F11,		SDL.SDL_Scancode.SDL_SCANCODE_F11 },
			{ (int) Keys.F12,		SDL.SDL_Scancode.SDL_SCANCODE_F12 },
			{ (int) Keys.F13,		SDL.SDL_Scancode.SDL_SCANCODE_F13 },
			{ (int) Keys.F14,		SDL.SDL_Scancode.SDL_SCANCODE_F14 },
			{ (int) Keys.F15,		SDL.SDL_Scancode.SDL_SCANCODE_F15 },
			{ (int) Keys.F16,		SDL.SDL_Scancode.SDL_SCANCODE_F16 },
			{ (int) Keys.F17,		SDL.SDL_Scancode.SDL_SCANCODE_F17 },
			{ (int) Keys.F18,		SDL.SDL_Scancode.SDL_SCANCODE_F18 },
			{ (int) Keys.F19,		SDL.SDL_Scancode.SDL_SCANCODE_F19 },
			{ (int) Keys.F20,		SDL.SDL_Scancode.SDL_SCANCODE_F20 },
			{ (int) Keys.F21,		SDL.SDL_Scancode.SDL_SCANCODE_F21 },
			{ (int) Keys.F22,		SDL.SDL_Scancode.SDL_SCANCODE_F22 },
			{ (int) Keys.F23,		SDL.SDL_Scancode.SDL_SCANCODE_F23 },
			{ (int) Keys.F24,		SDL.SDL_Scancode.SDL_SCANCODE_F24 },
			{ (int) Keys.Space,		SDL.SDL_Scancode.SDL_SCANCODE_SPACE },
			{ (int) Keys.Up,		SDL.SDL_Scancode.SDL_SCANCODE_UP },
			{ (int) Keys.Down,		SDL.SDL_Scancode.SDL_SCANCODE_DOWN },
			{ (int) Keys.Left,		SDL.SDL_Scancode.SDL_SCANCODE_LEFT },
			{ (int) Keys.Right,		SDL.SDL_Scancode.SDL_SCANCODE_RIGHT },
			{ (int) Keys.LeftAlt,		SDL.SDL_Scancode.SDL_SCANCODE_LALT },
			{ (int) Keys.RightAlt,		SDL.SDL_Scancode.SDL_SCANCODE_RALT },
			{ (int) Keys.LeftControl,	SDL.SDL_Scancode.SDL_SCANCODE_LCTRL },
			{ (int) Keys.RightControl,	SDL.SDL_Scancode.SDL_SCANCODE_RCTRL },
			{ (int) Keys.LeftWindows,	SDL.SDL_Scancode.SDL_SCANCODE_LGUI },
			{ (int) Keys.RightWindows,	SDL.SDL_Scancode.SDL_SCANCODE_RGUI },
			{ (int) Keys.LeftShift,		SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT },
			{ (int) Keys.RightShift,	SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT },
			{ (int) Keys.Apps,		SDL.SDL_Scancode.SDL_SCANCODE_APPLICATION },
			{ (int) Keys.OemQuestion,	SDL.SDL_Scancode.SDL_SCANCODE_SLASH },
			{ (int) Keys.OemBackslash,	SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH },
			{ (int) Keys.OemOpenBrackets,	SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET },
			{ (int) Keys.OemCloseBrackets,	SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET },
			{ (int) Keys.CapsLock,		SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK },
			{ (int) Keys.OemComma,		SDL.SDL_Scancode.SDL_SCANCODE_COMMA },
			{ (int) Keys.Delete,		SDL.SDL_Scancode.SDL_SCANCODE_DELETE },
			{ (int) Keys.End,		SDL.SDL_Scancode.SDL_SCANCODE_END },
			{ (int) Keys.Back,		SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE },
			{ (int) Keys.Enter,		SDL.SDL_Scancode.SDL_SCANCODE_RETURN },
			{ (int) Keys.Escape,		SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE },
			{ (int) Keys.Home,		SDL.SDL_Scancode.SDL_SCANCODE_HOME },
			{ (int) Keys.Insert,		SDL.SDL_Scancode.SDL_SCANCODE_INSERT },
			{ (int) Keys.OemMinus,		SDL.SDL_Scancode.SDL_SCANCODE_MINUS },
			{ (int) Keys.NumLock,		SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR },
			{ (int) Keys.PageUp,		SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP },
			{ (int) Keys.PageDown,		SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN },
			{ (int) Keys.Pause,		SDL.SDL_Scancode.SDL_SCANCODE_PAUSE },
			{ (int) Keys.OemPeriod,		SDL.SDL_Scancode.SDL_SCANCODE_PERIOD },
			{ (int) Keys.OemPlus,		SDL.SDL_Scancode.SDL_SCANCODE_EQUALS },
			{ (int) Keys.PrintScreen,	SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN },
			{ (int) Keys.OemQuotes,		SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE },
			{ (int) Keys.Scroll,		SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK },
			{ (int) Keys.OemSemicolon,	SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON },
			{ (int) Keys.Sleep,		SDL.SDL_Scancode.SDL_SCANCODE_SLEEP },
			{ (int) Keys.Tab,		SDL.SDL_Scancode.SDL_SCANCODE_TAB },
			{ (int) Keys.OemTilde,		SDL.SDL_Scancode.SDL_SCANCODE_GRAVE },
			{ (int) Keys.VolumeUp,		SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEUP },
			{ (int) Keys.VolumeDown,	SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN },
			{ (int) Keys.None,		SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN }
		};

		private static Keys ToXNAKey(ref SDL.SDL_Keysym key)
		{
			Keys retVal;
			if (UseScancodes)
			{
				if (INTERNAL_scanMap.TryGetValue((int) key.scancode, out retVal))
				{
					return retVal;
				}
			}
			else
			{
				if (INTERNAL_keyMap.TryGetValue((int) key.sym, out retVal))
				{
					return retVal;
				}
			}
			FNALoggerEXT.LogWarn(
				"KEY/SCANCODE MISSING FROM SDL2->XNA DICTIONARY: " +
				key.sym.ToString() + " " +
				key.scancode.ToString()
			);
			return Keys.None;
		}

		public static Keys GetKeyFromScancode(Keys scancode)
		{
			if (UseScancodes)
			{
				return scancode;
			}
			SDL.SDL_Scancode retVal;
			if (INTERNAL_xnaMap.TryGetValue((int) scancode, out retVal))
			{
				Keys result;
				SDL.SDL_Keycode sym = SDL.SDL_GetKeyFromScancode(retVal);
				if (INTERNAL_keyMap.TryGetValue((int) sym, out result))
				{
					return result;
				}
				FNALoggerEXT.LogWarn(
					"KEYCODE MISSING FROM SDL2->XNA DICTIONARY: " +
					sym.ToString()
				);
			}
			else
			{
				FNALoggerEXT.LogWarn(
					"SCANCODE MISSING FROM XNA->SDL2 DICTIONARY: " +
					scancode.ToString()
				);
			}
			return Keys.None;
		}

		#endregion

		#region Private Static Win32 WM_PAINT Interop

		private static SDL.SDL_EventFilter win32OnPaint = Win32OnPaint;
		private static unsafe int Win32OnPaint(IntPtr func, IntPtr evtPtr)
		{
			SDL.SDL_Event* evt = (SDL.SDL_Event*) evtPtr;
			if (	evt->type == SDL.SDL_EventType.SDL_WINDOWEVENT &&
				evt->window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED	)
			{
				foreach (Game game in activeGames)
				{
					if (	game.Window != null &&
						evt->window.windowID == SDL.SDL_GetWindowID(game.Window.Handle)	)
					{
						game.RedrawWindow();
						return 0;
					}
				}
			}
			return 1;
		}

		#endregion
	}
}
