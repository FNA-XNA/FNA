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
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
	{
		#region Public Properties

		public GraphicsProfile GraphicsProfile
		{
			get;
			set;
		}

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return graphicsDevice;
			}
		}

		private bool INTERNAL_isFullScreen;
		public bool IsFullScreen
		{
			get
			{
				return INTERNAL_isFullScreen;
			}
			set
			{
				INTERNAL_isFullScreen = value;
				prefsChanged = true;
			}
		}

		private bool INTERNAL_preferMultiSampling;
		public bool PreferMultiSampling
		{
			get
			{
				return INTERNAL_preferMultiSampling;
			}
			set
			{
				INTERNAL_preferMultiSampling = value;
				prefsChanged = true;
			}
		}

		private SurfaceFormat INTERNAL_preferredBackBufferFormat;
		public SurfaceFormat PreferredBackBufferFormat
		{
			get
			{
				return INTERNAL_preferredBackBufferFormat;
			}
			set
			{
				INTERNAL_preferredBackBufferFormat = value;
				prefsChanged = true;
			}
		}

		private int INTERNAL_preferredBackBufferHeight;
		public int PreferredBackBufferHeight
		{
			get
			{
				return INTERNAL_preferredBackBufferHeight;
			}
			set
			{
				INTERNAL_preferredBackBufferHeight = value;
				prefsChanged = true;
			}
		}

		private int INTERNAL_preferredBackBufferWidth;
		public int PreferredBackBufferWidth
		{
			get
			{
				return INTERNAL_preferredBackBufferWidth;
			}
			set
			{
				INTERNAL_preferredBackBufferWidth = value;
				prefsChanged = true;
			}
		}

		private DepthFormat INTERNAL_preferredDepthStencilFormat;
		public DepthFormat PreferredDepthStencilFormat
		{
			get
			{
				return INTERNAL_preferredDepthStencilFormat;
			}
			set
			{
				INTERNAL_preferredDepthStencilFormat = value;
				prefsChanged = true;
			}
		}

		private bool INTERNAL_synchronizeWithVerticalRetrace;
		public bool SynchronizeWithVerticalRetrace
		{
			get
			{
				return INTERNAL_synchronizeWithVerticalRetrace;
			}
			set
			{
				INTERNAL_synchronizeWithVerticalRetrace = value;
				prefsChanged = true;
			}
		}

		private DisplayOrientation INTERNAL_supportedOrientations;
		public DisplayOrientation SupportedOrientations
		{
			get
			{
				return INTERNAL_supportedOrientations;
			}
			set
			{
				INTERNAL_supportedOrientations = value;
				prefsChanged = true;
			}
		}

		#endregion

		#region Private Variables

		private Game game;
		private GraphicsDevice graphicsDevice;
		private bool drawBegun;
		private bool disposed;
		private bool prefsChanged;
		private bool supportsOrientations;
		private bool useResizedBackBuffer;
		private int resizedBackBufferWidth;
		private int resizedBackBufferHeight;

		#endregion

		#region Public Static Fields

		public static readonly int DefaultBackBufferWidth = 800;
		public static readonly int DefaultBackBufferHeight = 480;

		#endregion

		#region Public Events

		public event EventHandler<EventArgs> Disposed;

		#endregion

		#region IGraphicsDeviceService Events

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

		#endregion

		#region Public Constructor

		public GraphicsDeviceManager(Game game)
		{
			if (game == null)
			{
				throw new ArgumentNullException("The game cannot be null!");
			}

			this.game = game;

			INTERNAL_supportedOrientations = DisplayOrientation.Default;

			INTERNAL_preferredBackBufferHeight = DefaultBackBufferHeight;
			INTERNAL_preferredBackBufferWidth = DefaultBackBufferWidth;

			INTERNAL_preferredBackBufferFormat = SurfaceFormat.Color;
			INTERNAL_preferredDepthStencilFormat = DepthFormat.Depth24;

			INTERNAL_synchronizeWithVerticalRetrace = true;

			INTERNAL_preferMultiSampling = false;

			if (game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
			{
				throw new ArgumentException("Graphics Device Manager Already Present");
			}

			game.Services.AddService(typeof(IGraphicsDeviceManager), this);
			game.Services.AddService(typeof(IGraphicsDeviceService), this);

			prefsChanged = true;
			useResizedBackBuffer = false;
			supportsOrientations = FNAPlatform.SupportsOrientationChanges();
			game.Window.ClientSizeChanged += INTERNAL_OnClientSizeChanged;
		}

		#endregion

		#region Destructor

		~GraphicsDeviceManager()
		{
			Dispose(false);
		}

		#endregion

		#region Dispose Methods

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				game.Services.RemoveService(typeof(IGraphicsDeviceManager));
				game.Services.RemoveService(typeof(IGraphicsDeviceService));
				if (disposing)
				{
					if (graphicsDevice != null)
					{
						graphicsDevice.Dispose();
						graphicsDevice = null;
					}
				}
				if (Disposed != null)
				{
					Disposed(this, EventArgs.Empty);
				}
				disposed = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Public Methods

		public void ApplyChanges()
		{
			/* Calling ApplyChanges() before CreateDevice() forces CreateDevice.
			 * We can then return early since CreateDevice basically does all of
			 * this work for us anyway.
			 *
			 * Note that if you hit this block, it's probably because you called
			 * ApplyChanges in the constructor. The device created here gets
			 * destroyed and recreated by the game, so maybe don't do that!
			 *
			 * -flibit
			 */
			if (graphicsDevice == null)
			{
				#if DEBUG
				FNALoggerEXT.LogWarn("Forcing CreateDevice! Avoid calling ApplyChanges before Game.Run!");
				#endif

				(this as IGraphicsDeviceManager).CreateDevice();
				return;
			}

			// ApplyChanges() calls with no actual changes should be ignored.
			if (!prefsChanged && !useResizedBackBuffer)
			{
				return;
			}

			// Recreate device information before resetting
			GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
			gdi.Adapter = graphicsDevice.Adapter;
			gdi.PresentationParameters = graphicsDevice.PresentationParameters.Clone();
			INTERNAL_CreateGraphicsDeviceInformation(gdi);

			// Prepare the window...
			if (supportsOrientations)
			{
				game.Window.SetSupportedOrientations(
					INTERNAL_supportedOrientations
				);
			}
			game.Window.BeginScreenDeviceChange(
				gdi.PresentationParameters.IsFullScreen
			);
			game.Window.EndScreenDeviceChange(
				gdi.Adapter.DeviceName,
				gdi.PresentationParameters.BackBufferWidth,
				gdi.PresentationParameters.BackBufferHeight
			);

			// FIXME: Everything below should be before EndScreenDeviceChange! -flibit

			// Reset!
			graphicsDevice.Reset(
				gdi.PresentationParameters,
				gdi.Adapter
			);
			prefsChanged = false;
		}

		public void ToggleFullScreen()
		{
			IsFullScreen = !IsFullScreen;
			ApplyChanges();
		}

		#endregion

		#region Protected Methods

		protected virtual void OnDeviceCreated(object sender, EventArgs args)
		{
			if (DeviceCreated != null)
			{
				DeviceCreated(sender, args);
			}
		}

		protected virtual void OnDeviceDisposing(object sender, EventArgs args)
		{
			if (DeviceDisposing != null)
			{
				DeviceDisposing(this, args);
			}
		}

		protected virtual void OnDeviceReset(object sender, EventArgs args)
		{
			if (DeviceReset != null)
			{
				DeviceReset(this, args);
			}
		}

		protected virtual void OnDeviceResetting(object sender, EventArgs args)
		{
			if (DeviceResetting != null)
			{
				DeviceResetting(this, args);
			}
		}

		protected virtual void OnPreparingDeviceSettings(
			object sender,
			PreparingDeviceSettingsEventArgs args
		) {
			if (PreparingDeviceSettings != null)
			{
				PreparingDeviceSettings(sender, args);
			}
		}

		protected virtual bool CanResetDevice(
			GraphicsDeviceInformation newDeviceInfo
		) {
			throw new NotImplementedException();
		}

		protected virtual GraphicsDeviceInformation FindBestDevice(
			bool anySuitableDevice
		) {
			throw new NotImplementedException();
		}

		protected virtual void RankDevices(
			List<GraphicsDeviceInformation> foundDevices
		) {
			throw new NotImplementedException();
		}

		#endregion

		#region Private Methods

		private void INTERNAL_OnClientSizeChanged(object sender, EventArgs e)
		{
			GameWindow window = (sender as GameWindow);

			Rectangle size = window.ClientBounds;
			resizedBackBufferWidth = size.Width;
			resizedBackBufferHeight = size.Height;

			FNAPlatform.ScaleForWindow(
				window.Handle,
				true,
				ref resizedBackBufferWidth,
				ref resizedBackBufferHeight
			);

			useResizedBackBuffer = true;
			ApplyChanges();
		}

		private void INTERNAL_CreateGraphicsDeviceInformation(
			GraphicsDeviceInformation gdi
		) {
			/* Apply the GraphicsDevice changes to the new Parameters.
			 * Note that PreparingDeviceSettings can override any of these!
			 * -flibit
			 */
			if (useResizedBackBuffer)
			{
				gdi.PresentationParameters.BackBufferWidth =
					resizedBackBufferWidth;
				gdi.PresentationParameters.BackBufferHeight =
					resizedBackBufferHeight;
				useResizedBackBuffer = false;
			}
			else
			{
				if (!supportsOrientations)
				{
					gdi.PresentationParameters.BackBufferWidth =
						PreferredBackBufferWidth;
					gdi.PresentationParameters.BackBufferHeight =
						PreferredBackBufferHeight;
				}
				else
				{
					/* Flip the backbuffer dimensions to scale
					 * appropriately to the current orientation.
					 */
					int min = Math.Min(PreferredBackBufferWidth, PreferredBackBufferHeight);
					int max = Math.Max(PreferredBackBufferWidth, PreferredBackBufferHeight);

					if (gdi.PresentationParameters.DisplayOrientation == DisplayOrientation.Portrait)
					{
						gdi.PresentationParameters.BackBufferWidth = min;
						gdi.PresentationParameters.BackBufferHeight = max;
					}
					else
					{
						gdi.PresentationParameters.BackBufferWidth = max;
						gdi.PresentationParameters.BackBufferHeight = min;
					}
				}
			}
			gdi.PresentationParameters.BackBufferFormat =
				PreferredBackBufferFormat;
			gdi.PresentationParameters.DepthStencilFormat =
				PreferredDepthStencilFormat;
			gdi.PresentationParameters.IsFullScreen =
				IsFullScreen;
			gdi.PresentationParameters.PresentationInterval =
				SynchronizeWithVerticalRetrace ?
					PresentInterval.One :
					PresentInterval.Immediate;
			if (!PreferMultiSampling)
			{
				gdi.PresentationParameters.MultiSampleCount = 0;
			}
			else if (gdi.PresentationParameters.MultiSampleCount == 0)
			{
				/* XNA4 seems to have an upper limit of 8, but I'm willing to
				 * limit this only in GraphicsDeviceManager's default setting.
				 * If you want even higher values, Reset() with a custom value.
				 * -flibit
				 */
				int maxMultiSampleCount = 0;
				if (graphicsDevice != null)
				{
					maxMultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
						graphicsDevice.GLDevice,
						gdi.PresentationParameters.BackBufferFormat,
						8
					);
				}
				gdi.PresentationParameters.MultiSampleCount = Math.Min(
					maxMultiSampleCount,
					8
				);
			}
			gdi.GraphicsProfile = GraphicsProfile;

			// Give the user a chance to override the above settings.
			OnPreparingDeviceSettings(
				this,
				new PreparingDeviceSettingsEventArgs(gdi)
			);
		}

		#endregion

		#region IGraphicsDeviceManager Methods

		void IGraphicsDeviceManager.CreateDevice()
		{
			// This function can recreate the device from scratch!
			if (graphicsDevice != null)
			{
				graphicsDevice.Dispose();
				graphicsDevice = null;
			}

			// Set the default device information
			GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
			gdi.Adapter = GraphicsAdapter.DefaultAdapter;
			gdi.PresentationParameters = new PresentationParameters();
			gdi.PresentationParameters.DeviceWindowHandle = game.Window.Handle;
			INTERNAL_CreateGraphicsDeviceInformation(gdi);

			// Prepare the window...
			if (supportsOrientations)
			{
				game.Window.SetSupportedOrientations(
					INTERNAL_supportedOrientations
				);
			}
			game.Window.BeginScreenDeviceChange(
				gdi.PresentationParameters.IsFullScreen
			);
			game.Window.EndScreenDeviceChange(
				gdi.Adapter.DeviceName,
				gdi.PresentationParameters.BackBufferWidth,
				gdi.PresentationParameters.BackBufferHeight
			);

			// FIXME: Everything below should be before EndScreenDeviceChange! -flibit

			// Create the GraphicsDevice, hook the callbacks
			graphicsDevice = new GraphicsDevice(
				gdi.Adapter,
				gdi.GraphicsProfile,
				gdi.PresentationParameters
			);
			graphicsDevice.Disposing += OnDeviceDisposing;
			graphicsDevice.DeviceResetting += OnDeviceResetting;
			graphicsDevice.DeviceReset += OnDeviceReset;

			// Call the DeviceCreated Event
			OnDeviceCreated(this, EventArgs.Empty);
		}

		bool IGraphicsDeviceManager.BeginDraw()
		{
			if (graphicsDevice == null)
			{
				return false;
			}

			drawBegun = true;
			return true;
		}

		void IGraphicsDeviceManager.EndDraw()
		{
			if (graphicsDevice != null && drawBegun)
			{
				drawBegun = false;
				graphicsDevice.Present();
			}
		}

		#endregion
	}
}
