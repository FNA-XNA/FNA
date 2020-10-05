#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
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
					GraphicsDevice.NativeDevice,
					query
				) == 1;
			}
		}

		public int PixelCount
		{
			get
			{
				return FNA3D.FNA3D_QueryPixelCount(
					GraphicsDevice.NativeDevice,
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
			query = FNA3D.FNA3D_CreateQuery(GraphicsDevice.NativeDevice);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				FNA3D.FNA3D_AddDisposeQuery(GraphicsDevice.NativeDevice, query);
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Public Begin/End Methods

		public void Begin()
		{
			FNA3D.FNA3D_QueryBegin(GraphicsDevice.NativeDevice, query);
		}

		public void End()
		{
			FNA3D.FNA3D_QueryEnd(GraphicsDevice.NativeDevice, query);
		}

		#endregion
	}
}
