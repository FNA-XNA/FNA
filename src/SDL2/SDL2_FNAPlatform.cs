#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region USE_SCANCODES Option
// #define USE_SCANCODES
/* XNA Keys are based on keycodes, rather than scancodes.
 *
 * With SDL2 you can actually pick between SDL_Keycode and SDL_Scancode, but
 * scancodes will not be accurate to XNA4. The benefit is that scancodes will
 * essentially ignore "foreign" keyboard layouts, making default keyboard
 * layouts work out of the box everywhere (unless the actual symbol for the keys
 * matters in your game).
 *
 * At the same time, the TextInputEXT extension will still read the actual chars
 * correctly, so you can (mostly) have your cake and eat it too if you don't
 * care about your bindings menu not making a lot of sense on foreign layouts.
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SDL2;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class SDL2_FNAPlatform
	{
		#region Public Static Constants

		public static readonly string OSVersion = SDL.SDL_GetPlatform();

		#endregion

		#region Public Static Methods

		public static void Init(Game game)
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

			// Set and initialize the SDL2 window
			bool forceES2 = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_ES2"
			) == "1";
			bool forceCoreProfile = Environment.GetEnvironmentVariable(
				"FNA_OPENGL_FORCE_CORE_PROFILE"
			) == "1";
			game.Window = new SDL2_GameWindow(
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
		}

		public static void Dispose(Game game)
		{
			if (game.Window != null)
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

				SDL.SDL_DestroyWindow(game.Window.Handle);

				game.Window = null;
			}

			// This _should_ be the last SDL call we make...
			SDL.SDL_Quit();
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

			// Active Key List
			List<Keys> keys = new List<Keys>();

			/* Setup Text Input Control Character Arrays
			 * (Only 4 control keys supported at this time)
			 */
			bool[] INTERNAL_TextInputControlDown = new bool[4];
			int[] INTERNAL_TextInputControlRepeat = new int[4];
			bool INTERNAL_TextInputSuppress = false;

			SDL.SDL_Event evt;

			while (game.RunApplication)
			{
				while (SDL.SDL_PollEvent(out evt) == 1)
				{
					// Keyboard
					if (evt.type == SDL.SDL_EventType.SDL_KEYDOWN)
					{
#if USE_SCANCODES
						Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.scancode);
#else
						Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.sym);
#endif
						if (!keys.Contains(key))
						{
							keys.Add(key);
							if (key == Keys.Back)
							{
								INTERNAL_TextInputControlDown[0] = true;
								INTERNAL_TextInputControlRepeat[0] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput((char) 8); // Backspace
							}
							else if (key == Keys.Tab)
							{
								INTERNAL_TextInputControlDown[1] = true;
								INTERNAL_TextInputControlRepeat[1] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput((char) 9); // Tab
							}
							else if (key == Keys.Enter)
							{
								INTERNAL_TextInputControlDown[2] = true;
								INTERNAL_TextInputControlRepeat[2] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput((char) 13); // Enter
							}
							else if (keys.Contains(Keys.LeftControl) && key == Keys.V)
							{
								INTERNAL_TextInputControlDown[3] = true;
								INTERNAL_TextInputControlRepeat[3] = Environment.TickCount + 400;
								TextInputEXT.OnTextInput((char) 22); // Control-V (Paste)
								INTERNAL_TextInputSuppress = true;
							}
						}
					}
					else if (evt.type == SDL.SDL_EventType.SDL_KEYUP)
					{
#if USE_SCANCODES
						Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.scancode);
#else
						Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.sym);
#endif
						if (keys.Remove(key))
						{
							if (key == Keys.Back)
							{
								INTERNAL_TextInputControlDown[0] = false;
							}
							else if (key == Keys.Tab)
							{
								INTERNAL_TextInputControlDown[1] = false;
							}
							else if (key == Keys.Enter)
							{
								INTERNAL_TextInputControlDown[2] = false;
							}
							else if ((!keys.Contains(Keys.LeftControl) && INTERNAL_TextInputControlDown[3]) || key == Keys.V)
							{
								INTERNAL_TextInputControlDown[3] = false;
								INTERNAL_TextInputSuppress = false;
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
						GamePad.INTERNAL_AddInstance(evt.cdevice.which);
					}
					else if (evt.type == SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED)
					{
						GamePad.INTERNAL_RemoveInstance(evt.cdevice.which);
					}

					// Text Input
					else if (evt.type == SDL.SDL_EventType.SDL_TEXTINPUT && !INTERNAL_TextInputSuppress)
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
				if (INTERNAL_TextInputControlDown[0] && INTERNAL_TextInputControlRepeat[0] <= Environment.TickCount)
				{
					TextInputEXT.OnTextInput((char) 8);
				}
				if (INTERNAL_TextInputControlDown[1] && INTERNAL_TextInputControlRepeat[1] <= Environment.TickCount)
				{
					TextInputEXT.OnTextInput((char) 9);
				}
				if (INTERNAL_TextInputControlDown[2] && INTERNAL_TextInputControlRepeat[2] <= Environment.TickCount)
				{
					TextInputEXT.OnTextInput((char) 13);
				}
				if (INTERNAL_TextInputControlDown[3] && INTERNAL_TextInputControlRepeat[3] <= Environment.TickCount)
				{
					TextInputEXT.OnTextInput((char) 22);
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
				GamePad.INTERNAL_AddInstance(evt[0].cdevice.which);
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
						System.Console.WriteLine("Using EXT_swap_control_tear VSync!");
					}
					else
					{
						System.Console.WriteLine("EXT_swap_control_tear unsupported. Fall back to standard VSync.");
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
				throw new Exception("Unrecognized PresentInterval!");
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

		public static Keys GetKeyFromScancode(Keys scancode)
		{
			return SDL2_KeyboardUtil.KeyFromScancode(scancode);
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

		public static void SetMousePosition(IntPtr window, int x, int y)
		{
			SDL.SDL_WarpMouseInWindow(window, x, y);
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
			throw new Exception("StorageDevice: Platform.OSVersion not handled!");
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
			throw new Exception("StorageDevice: Platform.OSVersion not handled!");
		}

		public static void Log(string Message)
		{
			Console.WriteLine(Message);
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
