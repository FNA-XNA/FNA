#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{	
	public abstract class GraphicsResource : IDisposable
	{
		#region Public Properties

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return graphicsDevice;
			}
			internal set
			{
				if (graphicsDevice == value)
				{
					return;
				}

				/* VertexDeclaration objects can be bound to
				 * multiple GraphicsDevice objects during their
				 * lifetime. But only one GraphicsDevice should
				 * retain ownership.
				 */
				if (graphicsDevice != null)
				{
					graphicsDevice.RemoveResourceReference(selfReference);
					selfReference = null;
				}

				graphicsDevice = value;

				selfReference = new WeakReference(this);
				graphicsDevice.AddResourceReference(selfReference);
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			set;
		}

		public Object Tag
		{
			get;
			set;
		}

		#endregion

		#region Private Variables

		private WeakReference selfReference;

		private GraphicsDevice graphicsDevice;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Internal Constructor and Deconstructor

		internal GraphicsResource()
		{
		}

		~GraphicsResource()
		{
			// Pass false so the managed objects are not released
			// FIXME: This can lock up your game from the GC! -flibit
			// Dispose(false);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			// Dispose of managed objects as well
			Dispose(true);
			// Since we have been manually disposed, do not call the finalizer on this object
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Called before the device is reset. Allows graphics resources to
		/// invalidate their state so they can be recreated after the device reset.
		/// Warning: This may be called after a call to Dispose() up until
		/// the resource is garbage collected.
		/// </summary>
		internal protected virtual void GraphicsDeviceResetting()
		{
		}

		#endregion

		#region Protected Dispose Method

		/// <summary>
		/// The method that derived classes should override to implement disposing of
		/// managed and native resources.
		/// </summary>
		/// <param name="disposing">True if managed objects should be disposed.</param>
		/// <remarks>
		/// Native resources should always be released regardless of the value of the
		/// disposing parameter.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				// Do not trigger the event if called from the finalizer
				if (disposing && Disposing != null)
				{
					Disposing(this, EventArgs.Empty);
				}

				// Remove from the list of graphics resources
				if (graphicsDevice != null)
				{
					graphicsDevice.RemoveResourceReference(selfReference);
				}

				selfReference = null;
				graphicsDevice = null;
				IsDisposed = true;
			}
		}

		#endregion
	}
}
