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
				return FNA3D.FNA3D_QueryPixelCount(
					GraphicsDevice.GLDevice,
					query
				);
			}
		}

		#endregion

		#region Private FNA3D Variables

		private IntPtr query;

		#endregion

		#region Public Constructor

		public OcclusionQuery(GraphicsDevice graphicsDevice)
		{
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
			base.Dispose(disposing);
		}

		#endregion

		#region Public Begin/End Methods

		public void Begin()
		{
			FNA3D.FNA3D_QueryBegin(GraphicsDevice.GLDevice, query);
		}

		public void End()
		{
			FNA3D.FNA3D_QueryEnd(GraphicsDevice.GLDevice, query);
		}

		#endregion
	}
}
