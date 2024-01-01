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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	[Serializable]
	public class PresentationParameters
	{
		#region Public Properties

		public SurfaceFormat BackBufferFormat
		{
			get
			{
				return parameters.backBufferFormat;
			}
			set
			{
				parameters.backBufferFormat = value;
			}
		}

		public int BackBufferHeight
		{
			get
			{
				return parameters.backBufferHeight;
			}
			set
			{
				parameters.backBufferHeight = value;
			}
		}

		public int BackBufferWidth
		{
			get
			{
				return parameters.backBufferWidth;
			}
			set
			{
				parameters.backBufferWidth = value;
			}
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
			get
			{
				return parameters.deviceWindowHandle;
			}
			set
			{
				parameters.deviceWindowHandle = value;
			}
		}

		public DepthFormat DepthStencilFormat
		{
			get
			{
				return parameters.depthStencilFormat;
			}
			set
			{
				parameters.depthStencilFormat = value;
			}
		}

		public bool IsFullScreen
		{
			get
			{
				return parameters.isFullScreen == 1;
			}
			set
			{
				parameters.isFullScreen = (byte) (value ? 1 : 0);
			}
		}

		public int MultiSampleCount
		{
			get
			{
				return parameters.multiSampleCount;
			}
			set
			{
				parameters.multiSampleCount = value;
			}
		}

		public PresentInterval PresentationInterval
		{
			get
			{
				return parameters.presentationInterval;
			}
			set
			{
				parameters.presentationInterval = value;
			}
		}

		public DisplayOrientation DisplayOrientation
		{
			get
			{
				return parameters.displayOrientation;
			}
			set
			{
				parameters.displayOrientation = value;
			}
		}

		public RenderTargetUsage RenderTargetUsage
		{
			get
			{
				return parameters.renderTargetUsage;
			}
			set
			{
				parameters.renderTargetUsage = value;
			}
		}

		#endregion

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_PresentationParameters parameters;

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
			RenderTargetUsage = RenderTargetUsage.DiscardContents;
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
			clone.RenderTargetUsage = RenderTargetUsage;
			return clone;
		}

		#endregion
	}
}
