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
using System.Collections.Generic;
using System.ComponentModel;

using SDL2;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	class SDL2_GameWindow : GameWindow
	{
		#region Public GameWindow Properties

		[DefaultValue(false)]
		public override bool AllowUserResizing
		{
			/* FIXME: This change should happen immediately. However, SDL2 does
			 * not yet have an SDL_SetWindowResizable, so for now this is
			 * basically just a check for when the window is first made.
			 * -flibit
			 */
			get
			{
				return Environment.GetEnvironmentVariable(
					"FNA_WORKAROUND_WINDOW_RESIZABLE"
				) == "1";
			}
			set
			{
				// No-op. :(
			}
		}

		public override Rectangle ClientBounds
		{
			get
			{
				Rectangle result;
				if (INTERNAL_isFullscreen)
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
							INTERNAL_sdlWindow
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
						INTERNAL_sdlWindow,
						out result.X,
						out result.Y
					);
					SDL.SDL_GetWindowSize(
						INTERNAL_sdlWindow,
						out result.Width,
						out result.Height
					);
				}
				return result;
			}
		}

		public override DisplayOrientation CurrentOrientation
		{
			get
			{
				// SDL2 has no orientation.
				return DisplayOrientation.LandscapeLeft;
			}
		}

		public override IntPtr Handle
		{
			get
			{
				return INTERNAL_sdlWindow;
			}
		}

		public override bool IsBorderlessEXT
		{
			get
			{
				return ((SDL.SDL_GetWindowFlags(INTERNAL_sdlWindow) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0);
			}
			set
			{
				SDL.SDL_SetWindowBordered(
					INTERNAL_sdlWindow,
					value ? SDL.SDL_bool.SDL_FALSE : SDL.SDL_bool.SDL_TRUE
				);
			}
		}

		public override string ScreenDeviceName
		{
			get
			{
				return INTERNAL_deviceName;
			}
		}

		#endregion

		#region Private SDL2 Window Variables

		private IntPtr INTERNAL_sdlWindow;

		private bool INTERNAL_isFullscreen;
		private bool INTERNAL_wantsFullscreen;

		private string INTERNAL_deviceName;

		#endregion

		#region Internal Constructor

		internal SDL2_GameWindow(bool useES2, bool useCoreProfile)
		{
			SDL.SDL_WindowFlags initFlags = (
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL |
				SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
				SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
				SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS
			);

			// FIXME: Once we have SDL_SetWindowResizable, remove this. -flibit
			if (AllowUserResizing)
			{
				initFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
			}

			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
			if (useES2)
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
					2
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
			else if (useCoreProfile)
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
#if DEBUG
			SDL.SDL_GL_SetAttribute(
				SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS,
				(int) SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG
			);
#endif

			string title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();
			INTERNAL_sdlWindow = SDL.SDL_CreateWindow(
				title,
				SDL.SDL_WINDOWPOS_CENTERED,
				SDL.SDL_WINDOWPOS_CENTERED,
				GraphicsDeviceManager.DefaultBackBufferWidth,
				GraphicsDeviceManager.DefaultBackBufferHeight,
				initFlags
			);
			INTERNAL_SetIcon(title);

			INTERNAL_deviceName = SDL.SDL_GetDisplayName(
				SDL.SDL_GetWindowDisplayIndex(INTERNAL_sdlWindow)
			);
			INTERNAL_isFullscreen = false;
			INTERNAL_wantsFullscreen = false;
		}

		#endregion

		#region Public GameWindow Methods

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			INTERNAL_wantsFullscreen = willBeFullScreen;
		}

		public override void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight
		) {
			// Fullscreen
			if (	INTERNAL_wantsFullscreen &&
				(SDL.SDL_GetWindowFlags(INTERNAL_sdlWindow) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) == 0	)
			{
				/* FIXME: SDL2/OSX bug!
				 * For whatever reason, Spaces windows on OSX
				 * like to be high-DPI if you set fullscreen
				 * while the window is hidden. But, if you just
				 * show the window first, everything is fine.
				 * -flibit
				 */
				SDL.SDL_ShowWindow(INTERNAL_sdlWindow);
			}

			// When windowed, set the size before moving
			if (!INTERNAL_wantsFullscreen)
			{
				SDL.SDL_SetWindowFullscreen(INTERNAL_sdlWindow, 0);
				SDL.SDL_SetWindowSize(INTERNAL_sdlWindow, clientWidth, clientHeight);
			}

			// Get on the right display!
			int displayIndex = 0;
			for (int i = 0; i < GraphicsAdapter.Adapters.Count; i += 1)
			{
				// FIXME: Should be checking Name, not Description! -flibit
				if (screenDeviceName == GraphicsAdapter.Adapters[i].Description)
				{
					displayIndex = i;
					break;
				}
			}

			// Just to be sure, become a window first before changing displays
			if (INTERNAL_deviceName != screenDeviceName)
			{
				SDL.SDL_SetWindowFullscreen(INTERNAL_sdlWindow, 0);
				INTERNAL_deviceName = screenDeviceName;
			}

			// Window always gets centered, per XNA behavior
			int pos = SDL.SDL_WINDOWPOS_CENTERED_DISPLAY(displayIndex);
			SDL.SDL_SetWindowPosition(
				INTERNAL_sdlWindow,
				pos,
				pos
			);

			// Set fullscreen after we've done all the ugly stuff.
			if (INTERNAL_wantsFullscreen)
			{
				SDL.SDL_SetWindowFullscreen(
					INTERNAL_sdlWindow,
					(uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP
				);
			}

			// Current window state has just been updated.
			INTERNAL_isFullscreen = INTERNAL_wantsFullscreen;
		}

		#endregion

		#region Internal Methods

		internal void INTERNAL_ClientSizeChanged()
		{
			OnClientSizeChanged();
		}

		#endregion

		#region Protected GameWindow Methods

		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			// No-op. SDL2 has no orientation.
		}

		protected override void SetTitle(string title)
		{
			SDL.SDL_SetWindowTitle(
				INTERNAL_sdlWindow,
				title
			);
		}

		#endregion

		#region Private Window Icon Method

		private void INTERNAL_SetIcon(string title)
		{
			string fileIn = String.Empty;

			/* If the game's using SDL2_image, provide the option to use a PNG
			 * instead of a BMP. Nice for anyone who cares about transparency.
			 * -flibit
			 */
			try
			{
				fileIn = INTERNAL_GetIconName(title, ".png");
				if (!String.IsNullOrEmpty(fileIn))
				{
					IntPtr icon = SDL_image.IMG_Load(fileIn);
					SDL.SDL_SetWindowIcon(INTERNAL_sdlWindow, icon);
					SDL.SDL_FreeSurface(icon);
					return;
				}
			}
			catch(DllNotFoundException)
			{
				// Not that big a deal guys.
			}

			fileIn = INTERNAL_GetIconName(title, ".bmp");
			if (!String.IsNullOrEmpty(fileIn))
			{
				IntPtr icon = SDL.SDL_LoadBMP(fileIn);
				SDL.SDL_SetWindowIcon(INTERNAL_sdlWindow, icon);
				SDL.SDL_FreeSurface(icon);
			}
		}

		#endregion

		#region Private Static Icon Filename Method

		private static string INTERNAL_GetIconName(string title, string extension)
		{
			string fileIn = String.Empty;
			if (System.IO.File.Exists(title + extension))
			{
				// If the title and filename work, it just works. Fine.
				fileIn = title + extension;
			}
			else
			{
				// But sometimes the title has invalid characters inside.

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
				badChars.AddRange(System.IO.Path.GetInvalidFileNameChars());
				badChars.AddRange(hardCodeBadChars);

				string stripChars = title;
				foreach (char c in badChars)
				{
					stripChars = stripChars.Replace(c.ToString(), "");
				}
				stripChars += extension;

				if (System.IO.File.Exists(stripChars))
				{
					fileIn = stripChars;
				}
			}
			return fileIn;
		}

		#endregion
	}
}
