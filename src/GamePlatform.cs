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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Microsoft.Xna.Framework
{
	abstract class GamePlatform : IDisposable
	{
		#region Public Properties

		/// <summary>
		/// Gets the Game instance that owns this GamePlatform instance.
		/// </summary>
		public Game Game
		{
			get;
			private set;
		}

		public string OSVersion
		{
			get;
			private set;
		}

		#endregion

		#region Protected Properties

		protected bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Protected Constructor

		protected GamePlatform(Game game, string osVersion)
		{
			if (game == null)
			{
				throw new ArgumentNullException("game");
			}
			Game = game;
			OSVersion = osVersion;
			IsDisposed = false;
		}

		#endregion

		#region Deconstructor

		~GamePlatform()
		{
			Dispose(false);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Log the specified Message.
		/// </summary>
		/// <param name='Message'>
		/// The string to print to the log.
		/// </param>
		public abstract void Log(string Message);

		/// <summary>
		/// Gives derived classes an opportunity to do work before any
		/// components are initialized. Note that the base implementation sets
		/// IsActive to true, so derived classes should either call the base
		/// implementation or set IsActive to true by their own means.
		/// </summary>
		public abstract void BeforeInitialize();

		/// <summary>
		/// When implemented in a derived class, starts the run loop and blocks
		/// until it has ended.
		/// </summary>
		public abstract void RunLoop();

		public abstract void OnIsMouseVisibleChanged(bool visible);

		public abstract void ShowRuntimeError(
			String title,
			String message
		);

		public abstract GraphicsAdapter[] GetGraphicsAdapters();

		public abstract void SetPresentationInterval(PresentInterval interval);

		public abstract void TextureDataFromStream(
			Stream stream,
			out int width,
			out int height,
			out byte[] pixels,
			int reqWidth = -1,
			int reqHeight = -1,
			bool zoom = false
		);

		public abstract void SavePNG(
			Stream stream,
			int width,
			int height,
			int imgWidth,
			int imgHeight,
			byte[] data
		);

		public abstract Keys GetKeyFromScancode(Keys scancode);

		public abstract string GetStorageRoot();

		public abstract bool IsStoragePathConnected(string path);

		#endregion

		#region Public Static Methods

		public static GamePlatform Create(Game game)
		{
			/* I suspect you may have an urge to put an #if in here for new
			 * GamePlatform implementations.
			 *
			 * DON'T.
			 *
			 * Determine this at runtime, or load dynamically.
			 * No amount of whining will get me to budge on this.
			 * -flibit
			 */
			return new SDL2_GamePlatform(game);
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IsDisposed = true;
			}
		}

		#endregion
	}
}
