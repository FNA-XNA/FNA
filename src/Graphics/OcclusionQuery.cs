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
using System.Threading;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class OcclusionQuery : GraphicsResource
	{
		#region Public Properties

		public bool IsComplete
		{
			get
			{
				_hasIsCompleteBeenQueried = true;
				return FNA3D.FNA3D_QueryComplete(
					GraphicsDevice.GLDevice,
					query
				) == 1;
			}
		}

		public int PixelCount
		{
			get
			{
				if (!IsComplete)
				{
					throw new InvalidOperationException("The query data is not yet available. Use the IsComplete property to determine if the data is available before attempting to retrieve it.");
				}
				return FNA3D.FNA3D_QueryPixelCount(
					GraphicsDevice.GLDevice,
					query
				);
			}
		}

		#endregion

		#region Private Variables

		private bool _hasIsCompleteBeenQueried;
		private bool _isInBeginEndPair = false;

		#endregion

		#region Private FNA3D Variables

		private IntPtr query;

		#endregion

		#region Public Constructor

		public OcclusionQuery(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
			}
			_hasIsCompleteBeenQueried = true;
			GraphicsDevice = graphicsDevice;
			query = FNA3D.FNA3D_CreateQuery(GraphicsDevice.GLDevice);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IntPtr toDispose = Interlocked.Exchange(ref query, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeQuery(GraphicsDevice.GLDevice, toDispose);
				}
			}
			base.Dispose(false);
		}

		#endregion

		#region Public Begin/End Methods

		public void Begin()
		{
			if (_isInBeginEndPair)
			{
				throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
			}
			if (!_hasIsCompleteBeenQueried)
			{
				throw new InvalidOperationException("Begin may not be called on this query object again before IsComplete has been checked.");
			}
			FNA3D.FNA3D_QueryBegin(GraphicsDevice.GLDevice, query);
			_isInBeginEndPair = true;
			_hasIsCompleteBeenQueried = false;
		}

		public void End()
		{
			if (!_isInBeginEndPair)
			{
				throw new InvalidOperationException("Begin must be called successfully before End can be called.");
			}
			FNA3D.FNA3D_QueryEnd(GraphicsDevice.GLDevice, query);
			_isInBeginEndPair = false;
		}

		#endregion
	}
}
