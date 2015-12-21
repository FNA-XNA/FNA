#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region BASIC_PROFILER Option
// #define BASIC_PROFILER
/* Sometimes you need a really quick way to determine if performance is either
 * CPU- or GPU-bound. For XNA games, the fastest generic way is to just time the
 * Update() and Draw() functions, respectively. This is not to say that each
 * function can only have problems for either the CPU or GPU, but the graph can
 * say a lot about one of the two processes if either is notably slower than the
 * other one.
 *
 * This option will draw a rectangle on the right side of the screen. The two
 * colors indicate a rough percentage of time spent in both Update() and Draw().
 * Blue is Update(), Red is Draw(). There may be time spent in other parts of
 * the frame (usually GraphicsDevice.Present if you're faster than the display's
 * refresh rate), but compares to these two functions, the time spent is likely
 * marginal in comparison.
 *
 * If you want _real_ profile data, use a _real_ profiler!
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	public class Game : IDisposable
	{
		#region Public Properties

		public LaunchParameters LaunchParameters
		{
			get;
			private set;
		}

		public GameComponentCollection Components
		{
			get
			{
				return _components;
			}
		}

		public TimeSpan InactiveSleepTime
		{
			get
			{
				return _inactiveSleepTime;
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
				if (_inactiveSleepTime != value)
				{
					_inactiveSleepTime = value;
				}
			}
		}

		public bool IsActive
		{
			get
			{
				return Platform.IsActive;
			}
		}

		public bool IsMouseVisible
		{
			get
			{
				return Platform.IsMouseVisible;
			}
			set
			{
				Platform.IsMouseVisible = value;
			}
		}

		public TimeSpan TargetElapsedTime
		{
			get
			{
				return _targetElapsedTime;
			}
			set
			{
				/* Give GamePlatform implementations an opportunity to override
				 * the new value.
				 */
				value = Platform.TargetElapsedTimeChanging(value);

				if (value <= TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(
						"The time must be positive and non-zero.",
						default(Exception)
					);
				}

				if (value != _targetElapsedTime)
				{
					_targetElapsedTime = value;
					Platform.TargetElapsedTimeChanged();
				}
			}
		}

		public bool IsFixedTimeStep
		{
			get
			{
				return _isFixedTimeStep;
			}
			set
			{
				_isFixedTimeStep = value;
			}
		}

		public GameServiceContainer Services
		{
			get
			{
				return _services;
			}
		}

		public ContentManager Content
		{
			get
			{
				return _content;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_content = value;
			}
		}

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				if (_graphicsDeviceService == null)
				{
					_graphicsDeviceService = (IGraphicsDeviceService)
						Services.GetService(typeof(IGraphicsDeviceService));

					if (_graphicsDeviceService == null)
					{
						throw new InvalidOperationException(
							"No Graphics Device Service"
						);
					}
				}
				return _graphicsDeviceService.GraphicsDevice;
			}
		}

		public GameWindow Window
		{
			get
			{
				return Platform.Window;
			}
		}

		#endregion

		#region Internal Fields

		internal static Game Instance = null;

		internal GamePlatform Platform;

		#endregion

		#region Private Fields

		private GameComponentCollection _components;
		private GameServiceContainer _services;
		private ContentManager _content;

		private SortingFilteringCollection<IDrawable> _drawables =
			new SortingFilteringCollection<IDrawable>(
				d => d.Visible,
				(d, handler) => d.VisibleChanged += handler,
				(d, handler) => d.VisibleChanged -= handler,
				(d1, d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
				(d, handler) => d.DrawOrderChanged += handler,
				(d, handler) => d.DrawOrderChanged -= handler
			);

		private SortingFilteringCollection<IUpdateable> _updateables =
			new SortingFilteringCollection<IUpdateable>(
				u => u.Enabled,
				(u, handler) => u.EnabledChanged += handler,
				(u, handler) => u.EnabledChanged -= handler,
				(u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
				(u, handler) => u.UpdateOrderChanged += handler,
				(u, handler) => u.UpdateOrderChanged -= handler
			);

		private IGraphicsDeviceService _graphicsDeviceService;

		private bool _initialized = false;
		private bool _isFixedTimeStep = true;

		private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667); // 60fps

		private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);

		private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

		private bool _suppressDraw;

#if BASIC_PROFILER
		private long drawStart;
		private long drawTime;
		private long updateStart;
		private long updateTime;
		private BasicEffect profileEffect;
		private Matrix projection;
		private VertexPositionColor[] profilePrimitives;
#endif

		#endregion

		#region Public Constructors

		public Game()
		{
			Instance = this;

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			LaunchParameters = new LaunchParameters();
			_services = new GameServiceContainer();
			_components = new GameComponentCollection();
			_content = new ContentManager(_services);

			Platform = GamePlatform.Create(this);
			Platform.Activated += OnActivated;
			Platform.Deactivated += OnDeactivated;
			_services.AddService(typeof(GamePlatform), Platform);

			AudioDevice.Initialize();
		}

		#endregion

		#region Deconstructor

		~Game()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable Implementation

		private bool _isDisposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			Raise(Disposed, EventArgs.Empty);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					// Dispose loaded game components.
					for (int i = 0; i < _components.Count; i += 1)
					{
						IDisposable disposable = _components[i] as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
					_components = null;

					if (_content != null)
					{
						_content.Dispose();
						_content = null;
					}


					if (_graphicsDeviceService != null)
					{
						// FIXME: Does XNA4 require the GDM to be disposable? -flibit
						(_graphicsDeviceService as IDisposable).Dispose();
						_graphicsDeviceService = null;
					}

					AudioDevice.Dispose();

					if (Platform != null)
					{
						Platform.Activated -= OnActivated;
						Platform.Deactivated -= OnDeactivated;
						_services.RemoveService(typeof(GamePlatform));
						Platform.Dispose();
						Platform = null;
					}

					ContentTypeReaderManager.ClearTypeCreators();
				}

				AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

				_isDisposed = true;
				Instance = null;
			}
		}

		[DebuggerNonUserCode]
		private void AssertNotDisposed()
		{
			if (_isDisposed)
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

		#region Events

		public event EventHandler<EventArgs> Activated;
		public event EventHandler<EventArgs> Deactivated;
		public event EventHandler<EventArgs> Disposed;
		public event EventHandler<EventArgs> Exiting;

		#endregion

		#region Public Methods

		public void Exit()
		{
			Platform.Exit();
			_suppressDraw = true;
		}

		public void ResetElapsedTime()
		{
			if (_initialized)
			{
				Platform.ResetElapsedTime();
				_gameTimer.Reset();
				_gameTimer.Start();
				_accumulatedElapsedTime = TimeSpan.Zero;
				_gameTime.ElapsedGameTime = TimeSpan.Zero;
				_previousTicks = 0L;
			}
		}

		public void SuppressDraw()
		{
			_suppressDraw = true;
		}

		public void RunOneFrame()
		{
			if (Platform == null || !Platform.BeforeRun())
			{
				return;
			}

			if (!_initialized)
			{
				DoInitialize();
				_gameTimer = Stopwatch.StartNew();
				_initialized = true;
			}

			BeginRun();

			// FIXME: Not quite right..
			Tick();

			EndRun();
		}

		public void Run()
		{
			AssertNotDisposed();
			if (!Platform.BeforeRun())
			{
				BeginRun();
				_gameTimer = Stopwatch.StartNew();
				return;
			}

			if (!_initialized)
			{
				DoInitialize();
				_initialized = true;
			}

			BeginRun();
			_gameTimer = Stopwatch.StartNew();

			Platform.RunLoop();

			EndRun();

			OnExiting(this, EventArgs.Empty);
			UnloadContent();
		}

		private TimeSpan _accumulatedElapsedTime;
		private readonly GameTime _gameTime = new GameTime();
		private Stopwatch _gameTimer;
		private long _previousTicks = 0;
		private int _updateFrameLag;

		public void Tick()
		{
			/* NOTE: This code is very sensitive and can break very badly,
			 * even with what looks like a safe change. Be sure to test
			 * any change fully in both the fixed and variable timestep
			 * modes across multiple devices and platforms.
			 */

		RetryTick:

			// Advance the accumulated elapsed time.
			long currentTicks = _gameTimer.Elapsed.Ticks;
			_accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
			_previousTicks = currentTicks;

			/* If we're in the fixed timestep mode and not enough time has elapsed
			 * to perform an update we sleep off the the remaining time to save battery
			 * life and/or release CPU time to other threads and processes.
			 */
			if (IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
			{
				int sleepTime = (
					(int) (TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds
				);

				/* NOTE: While sleep can be inaccurate in general it is
				 * accurate enough for frame limiting purposes if some
				 * fluctuation is an acceptable result.
				 */
				System.Threading.Thread.Sleep(sleepTime);

				goto RetryTick;
			}

			// Do not allow any update to take longer than our maximum.
			if (_accumulatedElapsedTime > _maxElapsedTime)
			{
				_accumulatedElapsedTime = _maxElapsedTime;
			}

			if (IsFixedTimeStep)
			{
				_gameTime.ElapsedGameTime = TargetElapsedTime;
				int stepCount = 0;

				// Perform as many full fixed length time steps as we can.
				while (_accumulatedElapsedTime >= TargetElapsedTime)
				{
					_gameTime.TotalGameTime += TargetElapsedTime;
					_accumulatedElapsedTime -= TargetElapsedTime;
					stepCount += 1;

					DoUpdate(_gameTime);
				}

				// Every update after the first accumulates lag
				_updateFrameLag += Math.Max(0, stepCount - 1);

				/* If we think we are running slowly, wait
				 * until the lag clears before resetting it
				 */
				if (_gameTime.IsRunningSlowly)
				{
					if (_updateFrameLag == 0)
					{
						_gameTime.IsRunningSlowly = false;
					}
				}
				else if (_updateFrameLag >= 5)
				{
					/* If we lag more than 5 frames,
					 * start thinking we are running slowly.
					 */
					_gameTime.IsRunningSlowly = true;
				}

				/* Every time we just do one update and one draw,
				 * then we are not running slowly, so decrease the lag.
				 */
				if (stepCount == 1 && _updateFrameLag > 0)
				{
					_updateFrameLag -= 1;
				}

				/* Draw needs to know the total elapsed time
				 * that occured for the fixed length updates.
				 */
				_gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
			}
			else
			{
				// Perform a single variable length update.
				_gameTime.ElapsedGameTime = _accumulatedElapsedTime;
				_gameTime.TotalGameTime += _accumulatedElapsedTime;
				_accumulatedElapsedTime = TimeSpan.Zero;

				DoUpdate(_gameTime);
			}

			// Draw unless the update suppressed it.
			if (_suppressDraw)
			{
				_suppressDraw = false;
			}
			else
			{
				/* Draw/EndDraw should not be called if BeginDraw returns false.
				 * http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
				 * http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
				 */
				if (Platform.BeforeDraw(_gameTime) && BeginDraw())
				{
					Draw(_gameTime);
					EndDraw();
				}
			}
		}

		#endregion

		#region Protected Methods

		protected virtual bool BeginDraw()
		{
#if BASIC_PROFILER
			drawStart = _gameTimer.ElapsedTicks;
#endif
			return true;
		}

		protected virtual void EndDraw()
		{
#if BASIC_PROFILER
			drawTime = _gameTimer.ElapsedTicks - drawStart;
			Viewport viewport = GraphicsDevice.Viewport;
			float top = 50;
			float bottom = viewport.Height - 50;
			float middle = 50 + (bottom - top) * (updateTime / (float) (updateTime + drawTime));
			float left = viewport.Width - 100;
			float right = left + 50;
			profilePrimitives[0].Position.X = left;
			profilePrimitives[0].Position.Y = top;
			profilePrimitives[1].Position.X = right;
			profilePrimitives[1].Position.Y = top;
			profilePrimitives[2].Position.X = left;
			profilePrimitives[2].Position.Y = middle;
			profilePrimitives[3].Position.X = right;
			profilePrimitives[3].Position.Y = middle;
			profilePrimitives[4].Position.X = left;
			profilePrimitives[4].Position.Y = middle;
			profilePrimitives[5].Position.X = right;
			profilePrimitives[5].Position.Y = top;
			profilePrimitives[6].Position.X = left;
			profilePrimitives[6].Position.Y = middle;
			profilePrimitives[7].Position.X = right;
			profilePrimitives[7].Position.Y = middle;
			profilePrimitives[8].Position.X = left;
			profilePrimitives[8].Position.Y = bottom;
			profilePrimitives[9].Position.X = right;
			profilePrimitives[9].Position.Y = bottom;
			profilePrimitives[10].Position.X = left;
			profilePrimitives[10].Position.Y = bottom;
			profilePrimitives[11].Position.X = right;
			profilePrimitives[11].Position.Y = middle;
			projection.M11 = (float) (2.0 / (double) viewport.Width);
			projection.M22 = (float) (-2.0 / (double) viewport.Height);
			profileEffect.Projection = projection;
			profileEffect.CurrentTechnique.Passes[0].Apply();
			GraphicsDevice.DrawUserPrimitives(
				PrimitiveType.TriangleList,
				profilePrimitives,
				0,
				12
			);
#endif
			Platform.Present();
		}

		protected virtual void BeginRun()
		{
#if BASIC_PROFILER
			profileEffect = new BasicEffect(GraphicsDevice);
			profileEffect.FogEnabled = false;
			profileEffect.LightingEnabled = false;
			profileEffect.TextureEnabled = false;
			profileEffect.VertexColorEnabled = true;
			projection = new Matrix(
				1337.0f,
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				-1337.0f,
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				-1.0f,
				1.0f,
				0.0f,
				1.0f
			);
			profilePrimitives = new VertexPositionColor[12];
			int i = 0;
			do
			{
				profilePrimitives[i].Position = Vector3.Zero;
				profilePrimitives[i].Color = Color.Blue;
			} while (++i < 6);
			do
			{
				profilePrimitives[i].Position = Vector3.Zero;
				profilePrimitives[i].Color = Color.Red;
			} while (++i < 12);
#endif
		}

		protected virtual void EndRun()
		{
#if BASIC_PROFILER
			profileEffect.Dispose();
#endif
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

			_graphicsDeviceService = (IGraphicsDeviceService)
				Services.GetService(typeof(IGraphicsDeviceService));

			/* FIXME: If this test fails, is LoadContent ever called?
			 * This seems like a condition that warrants an exception more
			 * than a silent failure.
			 */
			if (	_graphicsDeviceService != null &&
				_graphicsDeviceService.GraphicsDevice != null	)
			{
				LoadContent();
			}
		}

		private static readonly Action<IDrawable, GameTime> DrawAction =
			(drawable, gameTime) => drawable.Draw(gameTime);

		protected virtual void Draw(GameTime gameTime)
		{
			_drawables.ForEachFilteredItem(DrawAction, gameTime);
		}

		private static readonly Action<IUpdateable, GameTime> UpdateAction =
			(updateable, gameTime) => updateable.Update(gameTime);

		protected virtual void Update(GameTime gameTime)
		{
			_updateables.ForEachFilteredItem(UpdateAction, gameTime);
		}

		protected virtual void OnExiting(object sender, EventArgs args)
		{
			Raise(Exiting, args);
		}

		protected virtual void OnActivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			Raise(Activated, args);
		}

		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
			Raise(Deactivated, args);
		}

		protected virtual bool ShowMissingRequirementMessage(Exception exception)
		{
			if (exception is NoAudioHardwareException)
			{
				Platform.ShowRuntimeError(
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
				Platform.ShowRuntimeError(
					Window.Title,
					"Could not find a suitable graphics device." +
					" More information:\n\n" + exception.Message
				);
				return true;
			}
			return false;
		}

		#endregion

		#region Internal Methods

		[Conditional("DEBUG")]
		internal void Log(string Message)
		{
			if (Platform != null)
			{
				Platform.Log(Message);
			}
		}

		#endregion

		#region Private Methods

		/* FIXME: We should work toward eliminating internal methods. They
		 * could eliminate the possibility that additional platforms could
		 * be added by third parties without changing FNA itself.
		 */

		private void DoUpdate(GameTime gameTime)
		{
			AssertNotDisposed();
#if BASIC_PROFILER
			updateStart = _gameTimer.ElapsedTicks;
#endif
			if (Platform.BeforeUpdate(gameTime))
			{
				AudioDevice.Update();

				Update(gameTime);
			}
#if BASIC_PROFILER
			updateTime = _gameTimer.ElapsedTicks - updateStart;
#endif
		}

		private void DoInitialize()
		{
			AssertNotDisposed();
			Platform.BeforeInitialize();
			Initialize();

			/* We need to do this after virtual Initialize(...) is called.
			 * 1. Categorize components into IUpdateable and IDrawable lists.
			 * 2. Subscribe to Added/Removed events to keep the categorized
			 * lists synced and to Initialize future components as they are
			 * added.
			 */
			CategorizeComponents();
			_components.ComponentAdded += Components_ComponentAdded;
			_components.ComponentRemoved += Components_ComponentRemoved;
		}

		private void CategorizeComponents()
		{
			DecategorizeComponents();
			for (int i = 0; i < Components.Count; i += 1)
			{
				CategorizeComponent(Components[i]);
			}
		}

		/* FIXME: I am open to a better name for this method.
		 * It does the opposite of CategorizeComponents.
		 */
		private void DecategorizeComponents()
		{
			_updateables.Clear();
			_drawables.Clear();
		}

		private void CategorizeComponent(IGameComponent component)
		{
			IUpdateable updateable = component as IUpdateable;

			if (updateable != null)
			{
				_updateables.Add(updateable);
			}

			IDrawable drawable = component as IDrawable;

			if (drawable != null)
			{
				_drawables.Add(drawable);
			}
		}

		/* FIXME: I am open to a better name for this method.
		 * It does the opposite of CategorizeComponent.
		 */
		private void DecategorizeComponent(IGameComponent component)
		{
			IUpdateable updateable = component as IUpdateable;

			if (updateable != null)
			{
				_updateables.Remove(updateable);
			}

			IDrawable drawable = component as IDrawable;

			if (drawable != null)
			{
				_drawables.Remove(drawable);
			}
		}

		private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e)
			where TEventArgs : EventArgs
		{
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		#region Private Event Handlers

		private void Components_ComponentAdded(
			object sender,
			GameComponentCollectionEventArgs e
		) {
			/* Since we only subscribe to ComponentAdded after the graphics
			 * devices are set up, it is safe to just blindly call Initialize.
			 */
			e.GameComponent.Initialize();
			CategorizeComponent(e.GameComponent);
		}

		private void Components_ComponentRemoved(
			object sender,
			GameComponentCollectionEventArgs e
		) {
			DecategorizeComponent(e.GameComponent);
		}

		private void OnUnhandledException(
			object sender,
			UnhandledExceptionEventArgs args
		) {
			ShowMissingRequirementMessage(args.ExceptionObject as Exception);
		}

		#endregion

		#region SortingFilteringCollection class

		/// <summary>
		/// The SortingFilteringCollection class provides efficient, reusable
		/// sorting and filtering based on a configurable sort comparer, filter
		/// predicate, and associate change events.
		/// </summary>
		class SortingFilteringCollection<T> : ICollection<T>
		{
			private readonly List<T> _items;
			private readonly List<AddJournalEntry<T>> _addJournal;
			private readonly Comparison<AddJournalEntry<T>> _addJournalSortComparison;
			private readonly List<int> _removeJournal;
			private readonly List<T> _cachedFilteredItems;
			private bool _shouldRebuildCache;

			private readonly Predicate<T> _filter;
			private readonly Comparison<T> _sort;
			private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;
			private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;
			private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;
			private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;

			public SortingFilteringCollection(
				Predicate<T> filter,
				Action<T, EventHandler<EventArgs>> filterChangedSubscriber,
				Action<T, EventHandler<EventArgs>> filterChangedUnsubscriber,
				Comparison<T> sort,
				Action<T, EventHandler<EventArgs>> sortChangedSubscriber,
				Action<T, EventHandler<EventArgs>> sortChangedUnsubscriber
			) {
				_items = new List<T>();
				_addJournal = new List<AddJournalEntry<T>>();
				_removeJournal = new List<int>();
				_cachedFilteredItems = new List<T>();
				_shouldRebuildCache = true;

				_filter = filter;
				_filterChangedSubscriber = filterChangedSubscriber;
				_filterChangedUnsubscriber = filterChangedUnsubscriber;
				_sort = sort;
				_sortChangedSubscriber = sortChangedSubscriber;
				_sortChangedUnsubscriber = sortChangedUnsubscriber;

				_addJournalSortComparison = CompareAddJournalEntry;
			}

			private int CompareAddJournalEntry(AddJournalEntry<T> x, AddJournalEntry<T> y)
			{
				int result = _sort(x.Item, y.Item);
				if (result != 0)
				{
					return result;
				}
				return x.Order - y.Order;
			}

			public void ForEachFilteredItem<TUserData>(
				Action<T, TUserData> action,
				TUserData userData
			) {
				if (_shouldRebuildCache)
				{
					ProcessRemoveJournal();
					ProcessAddJournal();

					// Rebuild the cache.
					_cachedFilteredItems.Clear();
					for (int i = 0; i < _items.Count; i += 1)
						if (_filter(_items[i]))
						{
							_cachedFilteredItems.Add(_items[i]);
						}

					_shouldRebuildCache = false;
				}

				for (int i = 0; i < _cachedFilteredItems.Count; i += 1)
				{
					action(_cachedFilteredItems[i], userData);
				}

				/* If the cache was invalidated as a result of processing items,
				 * now is a good time to clear it and give the GC (more of) a
				 * chance to do its thing.
				 */
				if (_shouldRebuildCache)
				{
					_cachedFilteredItems.Clear();
				}
			}

			public void Add(T item)
			{
				/* NOTE: We subscribe to item events after items in _addJournal
				 * have been merged.
				 */
				_addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
				InvalidateCache();
			}

			public bool Remove(T item)
			{
				if (_addJournal.Remove(AddJournalEntry<T>.CreateKey(item)))
				{
					return true;
				}

				int index = _items.IndexOf(item);
				if (index >= 0)
				{
					UnsubscribeFromItemEvents(item);
					_removeJournal.Add(index);
					InvalidateCache();
					return true;
				}
				return false;
			}

			public void Clear()
			{
				for (int i = 0; i < _items.Count; i += 1)
				{
					_filterChangedUnsubscriber(_items[i], Item_FilterPropertyChanged);
					_sortChangedUnsubscriber(_items[i], Item_SortPropertyChanged);
				}

				_addJournal.Clear();
				_removeJournal.Clear();
				_items.Clear();

				InvalidateCache();
			}

			public bool Contains(T item)
			{
				return _items.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				_items.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get
				{
					return _items.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _items.GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return ((System.Collections.IEnumerable) _items).GetEnumerator();
			}

			// Sort high to low.
			private static readonly Comparison<int> RemoveJournalSortComparison =
				(x, y) => Comparer<int>.Default.Compare(y, x);

			private void ProcessRemoveJournal()
			{
				if (_removeJournal.Count == 0)
				{
					return;
				}

				/* Remove items in reverse. (Technically there exist faster
				 * ways to bulk-remove from a variable-length array, but List<T>
				 * does not provide such a method.)
				 */
				_removeJournal.Sort(RemoveJournalSortComparison);
				for (int i = 0; i < _removeJournal.Count; i += 1)
				{
					_items.RemoveAt(_removeJournal[i]);
				}
				_removeJournal.Clear();
			}

			private void ProcessAddJournal()
			{
				if (_addJournal.Count == 0)
				{
					return;
				}

				/* Prepare the _addJournal to be merge-sorted with _items.
				 * _items is already sorted (because it is always sorted).
				 */
				_addJournal.Sort(_addJournalSortComparison);

				int iAddJournal = 0;
				int iItems = 0;

				while (iItems < _items.Count && iAddJournal < _addJournal.Count)
				{
					T addJournalItem = _addJournal[iAddJournal].Item;
					/* If addJournalItem is less than (belongs before)
					 * _items[iItems], insert it.
					 */
					if (_sort(addJournalItem, _items[iItems]) < 0)
					{
						SubscribeToItemEvents(addJournalItem);
						_items.Insert(iItems, addJournalItem);
						iAddJournal += 1;
					}
					/* Always increment iItems, either because we inserted and
					 * need to move past the insertion, or because we didn't
					 * insert and need to consider the next element.
					 */
					iItems += 1;
				}

				// If _addJournal had any "tail" items, append them all now.
				for (; iAddJournal < _addJournal.Count; iAddJournal += 1)
				{
					T addJournalItem = _addJournal[iAddJournal].Item;
					SubscribeToItemEvents(addJournalItem);
					_items.Add(addJournalItem);
				}

				_addJournal.Clear();
			}

			private void SubscribeToItemEvents(T item)
			{
				_filterChangedSubscriber(item, Item_FilterPropertyChanged);
				_sortChangedSubscriber(item, Item_SortPropertyChanged);
			}

			private void UnsubscribeFromItemEvents(T item)
			{
				_filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
				_sortChangedUnsubscriber(item, Item_SortPropertyChanged);
			}

			private void InvalidateCache()
			{
				_shouldRebuildCache = true;
			}

			private void Item_FilterPropertyChanged(object sender, EventArgs e)
			{
				InvalidateCache();
			}

			private void Item_SortPropertyChanged(object sender, EventArgs e)
			{
				T item = (T)sender;
				int index = _items.IndexOf(item);

				_addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
				_removeJournal.Add(index);

				/* Until the item is back in place, we don't care about its
				 * events. We will re-subscribe when _addJournal is processed.
				 */
				UnsubscribeFromItemEvents(item);
				InvalidateCache();
			}
		}

		#endregion

		#region AddJournalEntry struct

		private struct AddJournalEntry<T>
		{
			public readonly int Order;
			public readonly T Item;

			public AddJournalEntry(int order, T item)
			{
				Order = order;
				Item = item;
			}

			public static AddJournalEntry<T> CreateKey(T item)
			{
				return new AddJournalEntry<T>(-1, item);
			}

			public override int GetHashCode()
			{
				return Item.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (!(obj is AddJournalEntry<T>))
				{
					return false;
				}

				return object.Equals(Item, ((AddJournalEntry<T>) obj).Item);
			}
		}

		#endregion
	}
}
