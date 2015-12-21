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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	[Serializable]
	public class PresentationParameters
	{
		#region Public Properties

		public SurfaceFormat BackBufferFormat
		{
			get;
			set;
		}

		public int BackBufferHeight
		{
			get;
			set;
		}

		public int BackBufferWidth
		{
			get;
			set;
		}

		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(0, 0, BackBufferWidth, BackBufferHeight);
			}
		}

		public IntPtr DeviceWindowHandle
		{
			get;
			set;
		}

		public DepthFormat DepthStencilFormat
		{
			get;
			set;
		}

		public bool IsFullScreen
		{
			get;
			set;
		}

		public int MultiSampleCount
		{
			get;
			set;
		}

		public PresentInterval PresentationInterval
		{
			get;
			set;
		}

		public DisplayOrientation DisplayOrientation
		{
			get;
			set;
		}

		public RenderTargetUsage RenderTargetUsage
		{
			get;
			set;
		}

		#endregion

		#region Public Constructors

		public PresentationParameters()
		{
			BackBufferFormat = SurfaceFormat.Color;
			BackBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
			BackBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
			DeviceWindowHandle = IntPtr.Zero;
			IsFullScreen = false; // FIXME: Is this the default?
			DepthStencilFormat = DepthFormat.None;
			MultiSampleCount = 0;
			PresentationInterval = PresentInterval.Default;
			DisplayOrientation = DisplayOrientation.Default;
		}

		#endregion

		#region Public Methods

		public PresentationParameters Clone()
		{
			PresentationParameters clone = new PresentationParameters();
			clone.BackBufferFormat = BackBufferFormat;
			clone.BackBufferHeight = BackBufferHeight;
			clone.BackBufferWidth = BackBufferWidth;
			clone.DeviceWindowHandle = DeviceWindowHandle;
			clone.IsFullScreen = IsFullScreen;
			clone.DepthStencilFormat = DepthStencilFormat;
			clone.MultiSampleCount = MultiSampleCount;
			clone.PresentationInterval = PresentationInterval;
			clone.DisplayOrientation = DisplayOrientation;
			return clone;
		}

		#endregion
	}
}
