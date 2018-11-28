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
using System.ComponentModel;
#endregion

namespace Microsoft.Xna.Framework
{
	/* This class exists because the XNA window handle is an abstract class.
	 *
	 * The idea is that each platform would host its own GameWindow type,
	 * but a lot of this can be handled with static functions and a native
	 * window pointer, rather than messy things like a C# Form.
	 *
	 * So, when implementing new FNAPlatforms, just use this to store your
	 * window pointer and the functions will allow you to do your work.
	 *
	 * -flibit
	 */
	internal class FNAWindow : GameWindow
	{
		#region Public GameWindow Properties

		[DefaultValue(false)]
		public override bool AllowUserResizing
		{
			get
			{
				return FNAPlatform.GetWindowResizable(window);
			}
			set
			{
				FNAPlatform.SetWindowResizable(window, value);
			}
		}

		public override Rectangle ClientBounds
		{
			get
			{
				return FNAPlatform.GetWindowBounds(window);
			}
		}

		public override DisplayOrientation CurrentOrientation
		{
			get;
			internal set;
		}

		public override IntPtr Handle
		{
			get
			{
				return window;
			}
		}

		public override bool IsBorderlessEXT
		{
			get
			{
				return FNAPlatform.GetWindowBorderless(window);
			}
			set
			{
				FNAPlatform.SetWindowBorderless(window, value);
			}
		}

		public override string ScreenDeviceName
		{
			get
			{
				return deviceName;
			}
		}

		#endregion

		#region Private Variables

		private IntPtr window;
		private string deviceName;
		private bool wantsFullscreen;

		#endregion

		#region Internal Constructor

		internal FNAWindow(IntPtr nativeWindow, string display)
		{
			window = nativeWindow;
			deviceName = display;
			wantsFullscreen = false;
		}

		#endregion

		#region Public GameWindow Methods

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			wantsFullscreen = willBeFullScreen;
		}

		public override void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight
		) {
			string prevName = deviceName;
			FNAPlatform.ApplyWindowChanges(
				window,
				clientWidth,
				clientHeight,
				wantsFullscreen,
				screenDeviceName,
				ref deviceName
			);
			if (deviceName != prevName)
			{
				OnScreenDeviceNameChanged();
			}
		}

		#endregion

		#region Internal Methods

		internal void INTERNAL_ClientSizeChanged()
		{
			OnClientSizeChanged();
		}

		internal void INTERNAL_ScreenDeviceNameChanged()
		{
			OnScreenDeviceNameChanged();
		}

		internal void INTERNAL_OnOrientationChanged()
		{
			OnOrientationChanged();
		}

		#endregion

		#region Protected GameWindow Methods

		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			/* XNA on Windows Phone had the ability to change
			 * the list of supported device orientations at runtime.
			 * Unfortunately, we can't support that reliably across
			 * multiple mobile platforms. Therefore this method is
			 * essentially a no-op.
			 *
			 * Instead, you should set your supported orientations
			 * in Info.plist (iOS) or AndroidManifest.xml (Android).
			 *
			 * -caleb
			 */

			FNALoggerEXT.LogWarn("Setting SupportedOrientations has no effect!");
		}

		protected override void SetTitle(string title)
		{
			FNAPlatform.SetWindowTitle(window, title);
		}

		#endregion
	}
}

