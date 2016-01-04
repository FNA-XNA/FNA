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
				return GraphicsDevice.GLDevice.QueryComplete(query);
			}
		}

		public int PixelCount
		{
			get
			{
				return GraphicsDevice.GLDevice.QueryPixelCount(query);
			}
		}

		#endregion

		#region Private OpenGL Variables

		private IGLQuery query;

		#endregion

		#region Public Constructor

		public OcclusionQuery(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
			query = GraphicsDevice.GLDevice.CreateQuery();
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				GraphicsDevice.GLDevice.AddDisposeQuery(query);
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Public Begin/End Methods

		public void Begin()
		{
			GraphicsDevice.GLDevice.QueryBegin(query);
		}

		public void End()
		{
			GraphicsDevice.GLDevice.QueryEnd(query);
		}

		#endregion
	}
}
