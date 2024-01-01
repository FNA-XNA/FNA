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
using System.Runtime.InteropServices;
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
				if (graphicsDevice != null && selfReference.IsAllocated)
				{
					graphicsDevice.RemoveResourceReference(selfReference);
					selfReference.Free();
				}

				graphicsDevice = value;

				selfReference = GCHandle.Alloc(this, GCHandleType.Weak);
				graphicsDevice.AddResourceReference(selfReference);
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		protected string _Name;

		public virtual string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				_Name = value;
			}
		}

		public Object Tag
		{
			get;
			set;
		}

		#endregion

		#region Private Variables

		private GCHandle selfReference;

		private GraphicsDevice graphicsDevice;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Internal Constructor and Destructor

		internal GraphicsResource()
		{
		}

		~GraphicsResource()
		{
			if (!IsDisposed && (graphicsDevice != null && !graphicsDevice.IsDisposed))
			{
#if DEBUG
				// If the graphics device associated with this resource was already disposed, we assume
				//  that your game is in the middle of shutting down, and you don't care about leaks of stray
				//  resources like SamplerStates or other odds and ends.
				// We also ignore leaks of resources with no graphicsDevice yet, because they don't have
				//  any way to have native memory associated with them yet.
				// We also ignore leaks of resources with no associated native memory (via IsHarmlessToLeakInstance).
				if (!IsHarmlessToLeakInstance)
				{
					// If you see this log message, you leaked a graphics resource without disposing it!
					// This means your game may eventually run out of native memory for mysterious reasons.
					// To troubleshoot this, try setting a Name and/or Tag on your resources to identify them. -kg
					FNALoggerEXT.LogWarn(string.Format("A resource of type {0} with tag {1} and name {2} was not Disposed.", GetType().Name, Tag, Name));
				}
#endif
				Dispose(false);
			}
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			// Dispose of unmanaged objects as well
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

		internal protected virtual bool IsHarmlessToLeakInstance
		{
			get
			{
				return false;
			}
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
				if (graphicsDevice != null && selfReference.IsAllocated)
				{
					graphicsDevice.RemoveResourceReference(selfReference);
					selfReference.Free();
				}

				IsDisposed = true;
			}
		}

		#endregion
	}
}
