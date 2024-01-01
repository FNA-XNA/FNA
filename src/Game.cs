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
using System.Diagnostics;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Microsoft.Xna.Framework
{
	public class Game : IDisposable
	{
		#region Public Properties

		public GameComponentCollection Components
		{
			get;
			private set;
		}

		private ContentManager INTERNAL_content;
		public ContentManager Content
		{
			get
			{
				return INTERNAL_content;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				INTERNAL_content = value;
			}
		}

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				if (graphicsDeviceService == null)
				{
					graphicsDeviceService = (IGraphicsDeviceService)
						Services.GetService(typeof(IGraphicsDeviceService));

					if (graphicsDeviceService == null)
					{
						throw new InvalidOperationException(
							"No Graphics Device Service"
						);
					}
				}
				return graphicsDeviceService.GraphicsDevice;
			}
		}

		private TimeSpan INTERNAL_inactiveSleepTime;
		public TimeSpan InactiveSleepTime
		{
			get
			{
				return INTERNAL_inactiveSleepTime;
			}
			set
			{
				if (value < TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(
						"The time must be positive.",
						default(Exception)
					);
				}
				if (INTERNAL_inactiveSleepTime != value)
				{
					INTERNAL_inactiveSleepTime = value;
				}
			}
		}

		private bool INTERNAL_isActive;
		public bool IsActive
		{
			get
			{
				return INTERNAL_isActive;
			}
			internal set
			{
				if (INTERNAL_isActive != value)
				{
					INTERNAL_isActive = value;
					if (INTERNAL_isActive)
					{
						OnActivated(this, EventArgs.Empty);
					}
					else
					{
						OnDeactivated(this, EventArgs.Empty);
					}
				}
			}
		}

		public bool IsFixedTimeStep
		{
			get;
			set;
		}

		private bool INTERNAL_isMouseVisible;
		public bool IsMouseVisible
		{
			get
			{
				return INTERNAL_isMouseVisible;
			}
			set
			{
				if (INTERNAL_isMouseVisible != value)
				{
					INTERNAL_isMouseVisible = value;
					FNAPlatform.OnIsMouseVisibleChanged(value);
				}
			}
		}

		public LaunchParameters LaunchParameters
		{
			get;
			private set;
		}

		private TimeSpan INTERNAL_targetElapsedTime;
		public TimeSpan TargetElapsedTime
		{
			get
			{
				return INTERNAL_targetElapsedTime;
			}
			set
			{
				if (value <= TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(
						"The time must be positive and non-zero.",
						default(Exception)
					);
				}

				INTERNAL_targetElapsedTime = value;
			}
		}

		public GameServiceContainer Services
		{
			get;
			private set;
		}

		public GameWindow Window
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		internal bool RunApplication;

		#endregion

		#region Private Variables

		/* You will notice that these lists have some locks on them in the code.
		 * Technically this is not accurate to XNA4, as they just happily crash
		 * whenever there's an Add/Remove happening mid-copy.
		 *
		 * But do you really think I want to get reports about that crap?
		 * -flibit
		 */
		private List<IUpdateable> updateableComponents;
		private List<IUpdateable> currentlyUpdatingComponents;
		private List<IDrawable> drawableComponents;
		private List<IDrawable> currentlyDrawingComponents;

		private IGraphicsDeviceService graphicsDeviceService;
		private IGraphicsDeviceManager graphicsDeviceManager;
		private GraphicsAdapter currentAdapter;
		private bool hasInitialized;
		private bool suppressDraw;
		private bool isDisposed;

		private readonly GameTime gameTime;
		private Stopwatch gameTimer;
		private TimeSpan accumulatedElapsedTime;
		private long previousTicks = 0;
		private int updateFrameLag;
		private bool forceElapsedTimeToZero = false;

		// must be a power of 2 so we can do a bitmask optimization when checking worst case
		private const int PREVIOUS_SLEEP_TIME_COUNT = 128;
		private const int SLEEP_TIME_MASK = PREVIOUS_SLEEP_TIME_COUNT - 1;
		private TimeSpan[] previousSleepTimes = new TimeSpan[PREVIOUS_SLEEP_TIME_COUNT];
		private int sleepTimeIndex = 0;
		private TimeSpan worstCaseSleepPrecision = TimeSpan.FromMilliseconds(1);

		private static readonly TimeSpan MaxElapsedTime = TimeSpan.FromMilliseconds(500);

		private bool[] textInputControlDown;
		private bool textInputSuppress;

		#endregion

		#region Events

		public event EventHandler<EventArgs> Activated;
		public event EventHandler<EventArgs> Deactivated;
		public event EventHandler<EventArgs> Disposed;
		public event EventHandler<EventArgs> Exiting;

		#endregion

		#region Public Constructor

		public Game()
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			LaunchParameters = new LaunchParameters();
			Components = new GameComponentCollection();
			Services = new GameServiceContainer();
			Content = new ContentManager(Services);

			updateableComponents = new List<IUpdateable>();
			currentlyUpdatingComponents = new List<IUpdateable>();
			drawableComponents = new List<IDrawable>();
			currentlyDrawingComponents = new List<IDrawable>();

			IsMouseVisible = false;
			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromTicks(166667); // 60fps
			InactiveSleepTime = TimeSpan.FromSeconds(0.02);
			for (int i = 0; i < previousSleepTimes.Length; i += 1)
			{
				previousSleepTimes[i] = TimeSpan.FromMilliseconds(1);
			}

			textInputControlDown = new bool[FNAPlatform.TextInputCharacters.Length];

			hasInitialized = false;
			suppressDraw = false;
			isDisposed = false;

			gameTime = new GameTime();

			Window = FNAPlatform.CreateWindow();
			Mouse.WindowHandle = Window.Handle;
			TouchPanel.WindowHandle = Window.Handle;

			FrameworkDispatcher.Update();

			// Ready to run the loop!
			RunApplication = true;
		}

		#endregion

		#region Destructor

		~Game()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable Implementation

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			if (Disposed != null)
			{
				Disposed(this, EventArgs.Empty);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					// Dispose loaded game components.
					for (int i = 0; i < Components.Count; i += 1)
					{
						IDisposable disposable = Components[i] as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}

					if (Content != null)
					{
						Content.Dispose();
					}

					if (graphicsDeviceService != null)
					{
						// FIXME: Does XNA4 require the GDM to be disposable? -flibit
						(graphicsDeviceService as IDisposable).Dispose();
					}

					if (Window != null)
					{
						FNAPlatform.DisposeWindow(Window);
					}

					ContentTypeReaderManager.ClearTypeCreators();
				}

				AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

				isDisposed = true;
			}
		}

		[DebuggerNonUserCode]
		private void AssertNotDisposed()
		{
			if (isDisposed)
			{
				string name = GetType().Name;
				throw new ObjectDisposedException(
					name,
					string.Format(
						"The {0} object was used after being Disposed.",
						name
					)
				);
			}
		}

		#endregion

		#region Public Methods

		public void Exit()
		{
			RunApplication = false;
			suppressDraw = true;
		}

		public void ResetElapsedTime()
		{
			/* This only matters the next tick, and ONLY when
			 * IsFixedTimeStep is false!
			 * For fixed timestep, this is totally ignored.
			 * -flibit
			 */
			if (!IsFixedTimeStep)
			{
				forceElapsedTimeToZero = true;
			}
		}

		public void SuppressDraw()
		{
			suppressDraw = true;
		}

		public void RunOneFrame()
		{
			if (!hasInitialized)
			{
				DoInitialize();
				gameTimer = Stopwatch.StartNew();
				hasInitialized = true;
			}

			Tick();
		}

		public void Run()
		{
			AssertNotDisposed();

			if (!hasInitialized)
			{
				DoInitialize();
				hasInitialized = true;
			}

			BeginRun();
			BeforeLoop();

			gameTimer = Stopwatch.StartNew();
			RunLoop();

			EndRun();
			AfterLoop();
		}

		public void Tick()
		{
			/* NOTE: This code is very sensitive and can break very badly,
			 * even with what looks like a safe change. Be sure to test
			 * any change fully in both the fixed and variable timestep
			 * modes across multiple devices and platforms.
			 */

			AdvanceElapsedTime();

			if (IsFixedTimeStep)
			{
				/* If we are in fixed timestep, we want to wait until the next frame,
				 * but we don't want to oversleep. Requesting repeated 1ms sleeps and
				 * seeing how long we actually slept for lets us estimate the worst case
				 * sleep precision so we don't oversleep the next frame.
				 */
				while (accumulatedElapsedTime + worstCaseSleepPrecision < TargetElapsedTime)
				{
					System.Threading.Thread.Sleep(1);
					TimeSpan timeAdvancedSinceSleeping = AdvanceElapsedTime();
					UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping);
				}

				/* Now that we have slept into the sleep precision threshold, we need to wait
				 * for just a little bit longer until the target elapsed time has been reached.
				 * SpinWait(1) works by pausing the thread for very short intervals, so it is
				 * an efficient and time-accurate way to wait out the rest of the time.
				 */
				while (accumulatedElapsedTime < TargetElapsedTime)
				{
					System.Threading.Thread.SpinWait(1);
					AdvanceElapsedTime();
				}
			}

			// Now that we are going to perform an update, let's poll events.
			FNAPlatform.PollEvents(
				this,
				ref currentAdapter,
				textInputControlDown,
				ref textInputSuppress
			);

			// Do not allow any update to take longer than our maximum.
			if (accumulatedElapsedTime > MaxElapsedTime)
			{
				accumulatedElapsedTime = MaxElapsedTime;
			}

			if (IsFixedTimeStep)
			{
				gameTime.ElapsedGameTime = TargetElapsedTime;
				int stepCount = 0;

				// Perform as many full fixed length time steps as we can.
				while (accumulatedElapsedTime >= TargetElapsedTime)
				{
					gameTime.TotalGameTime += TargetElapsedTime;
					accumulatedElapsedTime -= TargetElapsedTime;
					stepCount += 1;

					AssertNotDisposed();
					Update(gameTime);
				}

				// Every update after the first accumulates lag
				updateFrameLag += Math.Max(0, stepCount - 1);

				/* If we think we are running slowly, wait
				 * until the lag clears before resetting it
				 */
				if (gameTime.IsRunningSlowly)
				{
					if (updateFrameLag == 0)
					{
						gameTime.IsRunningSlowly = false;
					}
				}
				else if (updateFrameLag >= 5)
				{
					/* If we lag more than 5 frames,
					 * start thinking we are running slowly.
					 */
					gameTime.IsRunningSlowly = true;
				}

				/* Every time we just do one update and one draw,
				 * then we are not running slowly, so decrease the lag.
				 */
				if (stepCount == 1 && updateFrameLag > 0)
				{
					updateFrameLag -= 1;
				}

				/* Draw needs to know the total elapsed time
				 * that occured for the fixed length updates.
				 */
				gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
			}
			else
			{
				// Perform a single variable length update.
				if (forceElapsedTimeToZero)
				{
					/* When ResetElapsedTime is called,
					 * Elapsed is forced to zero and
					 * Total is ignored entirely.
					 * -flibit
					 */
					gameTime.ElapsedGameTime = TimeSpan.Zero;
					forceElapsedTimeToZero = false;
				}
				else
				{
					gameTime.ElapsedGameTime = accumulatedElapsedTime;
					gameTime.TotalGameTime += gameTime.ElapsedGameTime;
				}

				accumulatedElapsedTime = TimeSpan.Zero;
				AssertNotDisposed();
				Update(gameTime);
			}

			// Draw unless the update suppressed it.
			if (suppressDraw)
			{
				suppressDraw = false;
			}
			else
			{
				/* Draw/EndDraw should not be called if BeginDraw returns false.
				 * http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
				 * http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
				 */
				if (BeginDraw())
				{
					Draw(gameTime);
					EndDraw();
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void RedrawWindow()
		{
			/* Draw/EndDraw should not be called if BeginDraw returns false.
			 * http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
			 * http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
			 *
			 * Additionally, if we haven't even started yet, be quiet until we have!
			 * -flibit
			 */
			if (gameTime.TotalGameTime != TimeSpan.Zero && BeginDraw())
			{
				Draw(new GameTime(gameTime.TotalGameTime, TimeSpan.Zero));
				EndDraw();
			}
		}

		#endregion

		#region Protected Methods

		protected virtual bool BeginDraw()
		{
			if (graphicsDeviceManager != null)
			{
				return graphicsDeviceManager.BeginDraw();
			}
			return true;
		}

		protected virtual void EndDraw()
		{
			if (graphicsDeviceManager != null)
			{
				graphicsDeviceManager.EndDraw();
			}
		}

		protected virtual void BeginRun()
		{
		}

		protected virtual void EndRun()
		{
		}

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void Initialize()
		{
			/* According to the information given on MSDN, all GameComponents
			 * in Components at the time Initialize() is called are initialized:
			 *
			 * http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
			 *
			 * Note, however, that we are NOT using a foreach. It's actually
			 * possible to add something during initialization, and those must
			 * also be initialized. There may be a safer way to account for it,
			 * considering it may be possible to _remove_ components as well,
			 * but for now, let's worry about initializing everything we get.
			 * -flibit
			 */
			for (int i = 0; i < Components.Count; i += 1)
			{
				Components[i].Initialize();
			}

			/* This seems like a condition that warrants a major
			 * exception more than a silent failure, but for some
			 * reason it's okay... but only sort of. You can get
			 * away with initializing just before base.Initialize(),
			 * but everything gets super broken on the IManager side
			 * (IService doesn't seem to matter anywhere else).
			 */
			graphicsDeviceService = (IGraphicsDeviceService)
				Services.GetService(typeof(IGraphicsDeviceService));
			if (graphicsDeviceService != null)
			{
				graphicsDeviceService.DeviceDisposing += (o, e) => UnloadContent();
				if (graphicsDeviceService.GraphicsDevice != null)
				{
					LoadContent();
				}
				else
				{
					graphicsDeviceService.DeviceCreated += (o, e) => LoadContent();
				}
			}
		}

		protected virtual void Draw(GameTime gameTime)
		{
			lock (drawableComponents)
			{
				for (int i = 0; i < drawableComponents.Count; i += 1)
				{
					currentlyDrawingComponents.Add(drawableComponents[i]);
				}
			}
			foreach (IDrawable drawable in currentlyDrawingComponents)
			{
				if (drawable.Visible)
				{
					drawable.Draw(gameTime);
				}
			}
			currentlyDrawingComponents.Clear();
		}

		protected virtual void Update(GameTime gameTime)
		{
			lock (updateableComponents)
			{
				for (int i = 0; i < updateableComponents.Count; i += 1)
				{
					currentlyUpdatingComponents.Add(updateableComponents[i]);
				}
			}
			foreach (IUpdateable updateable in currentlyUpdatingComponents)
			{
				if (updateable.Enabled)
				{
					updateable.Update(gameTime);
				}
			}
			currentlyUpdatingComponents.Clear();

			FrameworkDispatcher.Update();
		}

		protected virtual void OnExiting(object sender, EventArgs args)
		{
			if (Exiting != null)
			{
				Exiting(this, args);
			}
		}

		protected virtual void OnActivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			if (Activated != null)
			{
				Activated(this, args);
			}
		}

		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			if (Deactivated != null)
			{
				Deactivated(this, args);
			}
		}

		protected virtual bool ShowMissingRequirementMessage(Exception exception)
		{
			if (exception is NoAudioHardwareException)
			{
				FNAPlatform.ShowRuntimeError(
					Window.Title,
					"Could not find a suitable audio device. " +
					" Verify that a sound card is\ninstalled," +
					" and check the driver properties to make" +
					" sure it is not disabled."
				);
				return true;
			}
			if (exception is NoSuitableGraphicsDeviceException)
			{
				FNAPlatform.ShowRuntimeError(
					Window.Title,
					"Could not find a suitable graphics device." +
					" More information:\n\n" + exception.Message
				);
				return true;
			}
			return false;
		}

		#endregion

		#region Private Methods

		private void DoInitialize()
		{
			AssertNotDisposed();

			/* If this is late, you can still create it yourself.
			 * In fact, you can even go as far as creating the
			 * _manager_ before base.Initialize(), but Begin/EndDraw
			 * will not get called. Just... please, make the service
			 * before calling Run().
			 */
			graphicsDeviceManager = (IGraphicsDeviceManager)
				Services.GetService(typeof(IGraphicsDeviceManager));
			if (graphicsDeviceManager != null)
			{
				graphicsDeviceManager.CreateDevice();
			}

			Initialize();

			/* We need to do this after virtual Initialize(...) is called.
			 * 1. Categorize components into IUpdateable and IDrawable lists.
			 * 2. Subscribe to Added/Removed events to keep the categorized
			 * lists synced and to Initialize future components as they are
			 * added.
			 */
			updateableComponents.Clear();
			drawableComponents.Clear();
			for (int i = 0; i < Components.Count; i += 1)
			{
				CategorizeComponent(Components[i]);
			}
			Components.ComponentAdded += OnComponentAdded;
			Components.ComponentRemoved += OnComponentRemoved;
		}

		private void CategorizeComponent(IGameComponent component)
		{
			IUpdateable updateable = component as IUpdateable;
			if (updateable != null)
			{
				lock (updateableComponents)
				{
					SortUpdateable(updateable);
				}
				updateable.UpdateOrderChanged += OnUpdateOrderChanged;
			}

			IDrawable drawable = component as IDrawable;
			if (drawable != null)
			{
				lock (drawableComponents)
				{
					SortDrawable(drawable);
				}
				drawable.DrawOrderChanged += OnDrawOrderChanged;
			}
		}

		private void SortUpdateable(IUpdateable updateable)
		{
			for (int i = 0; i < updateableComponents.Count; i += 1)
			{
				if (updateable.UpdateOrder < updateableComponents[i].UpdateOrder)
				{
					updateableComponents.Insert(i, updateable);
					return;
				}
			}
			updateableComponents.Add(updateable);
		}

		private void SortDrawable(IDrawable drawable)
		{
			for (int i = 0; i < drawableComponents.Count; i += 1)
			{
				if (drawable.DrawOrder < drawableComponents[i].DrawOrder)
				{
					drawableComponents.Insert(i, drawable);
					return;
				}
			}
			drawableComponents.Add(drawable);
		}

		private void BeforeLoop()
		{
			currentAdapter = FNAPlatform.RegisterGame(this);
			IsActive = true;

			// Perform initial check for a touch device
			TouchPanel.TouchDeviceExists = FNAPlatform.GetTouchCapabilities().IsConnected;
		}

		private void AfterLoop()
		{
			FNAPlatform.UnregisterGame(this);
		}

		private void RunLoop()
		{
			/* Some platforms (i.e. Emscripten) don't support
			 * indefinite while loops, so instead we have to
			 * surrender control to the platform's main loop.
			 * -caleb
			 */
			if (FNAPlatform.NeedsPlatformMainLoop())
			{
				/* This breaks control flow and jumps
				 * directly into the platform main loop.
				 * Nothing below this call will be executed.
				 */
				FNAPlatform.RunPlatformMainLoop(this);
			}

			while (RunApplication)
			{
				Tick();
			}
			OnExiting(this, EventArgs.Empty);
		}

		private TimeSpan AdvanceElapsedTime()
		{
			long currentTicks = gameTimer.Elapsed.Ticks;
			TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - previousTicks);
			accumulatedElapsedTime += timeAdvanced;
			previousTicks = currentTicks;
			return timeAdvanced;
		}

		/* To calculate the sleep precision of the OS, we take the worst case
		 * time spent sleeping over the results of previous requests to sleep 1ms.
		 */
		private void UpdateEstimatedSleepPrecision(TimeSpan timeSpentSleeping)
		{
			/* It is unlikely that the scheduler will actually be more imprecise than
			 * 4ms and we don't want to get wrecked by a single long sleep so we cap this
			 * value at 4ms for sanity.
			 */
			TimeSpan upperTimeBound = TimeSpan.FromMilliseconds(4);

			if (timeSpentSleeping > upperTimeBound)
			{
				timeSpentSleeping = upperTimeBound;
			}

			/* We know the previous worst case - it's saved in worstCaseSleepPrecision.
			 * We also know the current index. So the only way the worst case changes
			 * is if we either 1) just got a new worst case, or 2) the worst case was
			 * the oldest entry on the list.
			 */
			if (timeSpentSleeping >= worstCaseSleepPrecision)
			{
				worstCaseSleepPrecision = timeSpentSleeping;
			}
			else if (previousSleepTimes[sleepTimeIndex] == worstCaseSleepPrecision)
			{
				TimeSpan maxSleepTime = TimeSpan.MinValue;
				for (int i = 0; i < previousSleepTimes.Length; i += 1)
				{
					if (previousSleepTimes[i] > maxSleepTime)
					{
						maxSleepTime = previousSleepTimes[i];
					}
				}
				worstCaseSleepPrecision = maxSleepTime;
			}

			previousSleepTimes[sleepTimeIndex] = timeSpentSleeping;
			sleepTimeIndex = (sleepTimeIndex + 1) & SLEEP_TIME_MASK;
		}

		#endregion

		#region Private Event Handlers

		private void OnComponentAdded(
			object sender,
			GameComponentCollectionEventArgs e
		) {
			/* Since we only subscribe to ComponentAdded after the graphics
			 * devices are set up, it is safe to just blindly call Initialize.
			 */
			e.GameComponent.Initialize();
			CategorizeComponent(e.GameComponent);
		}

		private void OnComponentRemoved(
			object sender,
			GameComponentCollectionEventArgs e
		) {
			IUpdateable updateable = e.GameComponent as IUpdateable;
			if (updateable != null)
			{
				lock (updateableComponents)
				{
					updateableComponents.Remove(updateable);
				}
				updateable.UpdateOrderChanged -= OnUpdateOrderChanged;
			}

			IDrawable drawable = e.GameComponent as IDrawable;
			if (drawable != null)
			{
				lock (drawableComponents)
				{
					drawableComponents.Remove(drawable);
				}
				drawable.DrawOrderChanged -= OnDrawOrderChanged;
			}
		}

		private void OnUpdateOrderChanged(object sender, EventArgs e)
		{
			// FIXME: Is there a better way to re-sort one item? -flibit
			IUpdateable updateable = sender as IUpdateable;
			lock (updateableComponents)
			{
				updateableComponents.Remove(updateable);
				SortUpdateable(updateable);
			}
		}

		private void OnDrawOrderChanged(object sender, EventArgs e)
		{
			// FIXME: Is there a better way to re-sort one item? -flibit
			IDrawable drawable = sender as IDrawable;
			lock (drawableComponents)
			{
				drawableComponents.Remove(drawable);
				SortDrawable(drawable);
			}
		}

		private void OnUnhandledException(
			object sender,
			UnhandledExceptionEventArgs args
		) {
			ShowMissingRequirementMessage(args.ExceptionObject as Exception);
		}

		#endregion
	}
}
