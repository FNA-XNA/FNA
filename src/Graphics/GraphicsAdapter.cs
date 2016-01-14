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
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class GraphicsAdapter
	{
		#region Public Properties

		public DisplayMode CurrentDisplayMode
		{
			get;
			private set;
		}

		public DisplayModeCollection SupportedDisplayModes
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public int DeviceId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public string DeviceName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsDefaultAdapter
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets a <see cref="System.Boolean"/> indicating whether
		/// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
		/// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
		/// Common widescreen modes include 16:9, 16:10 and 2:1.
		/// </summary>
		public bool IsWideScreen
		{
			get
			{
				/* Common non-widescreen modes: 4:3, 5:4, 1:1
				 * Common widescreen modes: 16:9, 16:10, 2:1
				 * XNA does not appear to account for rotated displays on the desktop
				 */
				const float limit = 4.0f / 3.0f;
				float aspect = CurrentDisplayMode.AspectRatio;
				return aspect > limit;
			}
		}

		public IntPtr MonitorHandle
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Revision
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int SubSystemId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool UseNullDevice
		{
			get;
			set;
		}

		public bool UseReferenceDevice
		{
			get;
			set;
		}

		public int VendorId
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Public Static Properties

		public static GraphicsAdapter DefaultAdapter
		{
			get
			{
				return Adapters[0];
			}
		}

		public static ReadOnlyCollection<GraphicsAdapter> Adapters
		{
			get
			{
				if (adapters == null)
				{
					adapters = new ReadOnlyCollection<GraphicsAdapter>(
						FNAPlatform.GetGraphicsAdapters()
					);
				}
				return adapters;
			}
		}

		#endregion

		#region Private Static Variables

		private static ReadOnlyCollection<GraphicsAdapter> adapters;

		#endregion

		#region Internal Constructor

		internal GraphicsAdapter(
			DisplayMode currentMode,
			DisplayModeCollection modes,
			string description
		) {
			CurrentDisplayMode = currentMode;
			SupportedDisplayModes = modes;
			Description = description;
			UseNullDevice = false;
			UseReferenceDevice = false;
		}

		#endregion

		#region Public Methods

		public bool IsProfileSupported(GraphicsProfile graphicsProfile)
		{
			/* TODO: This method could be genuinely useful!
			 * Maybe look into the difference between Reach/HiDef and add the
			 * appropriate properties to the GLDevice.
			 * -flibit
			 */
			return true;
		}

		public bool QueryRenderTargetFormat(
			GraphicsProfile graphicsProfile,
			SurfaceFormat format,
			DepthFormat depthFormat,
			int multiSampleCount,
			out SurfaceFormat selectedFormat,
			out DepthFormat selectedDepthFormat,
			out int selectedMultiSampleCount)
		{
			throw new NotImplementedException();
		}

		public bool QueryBackBufferFormat(
			GraphicsProfile graphicsProfile,
			SurfaceFormat format,
			DepthFormat depthFormat,
			int multiSampleCount,
			out SurfaceFormat selectedFormat,
			out DepthFormat selectedDepthFormat,
			out int selectedMultiSampleCount)
		{
			/* FIXME: MultiSampleCount could actually be dynamic.
			 * However, the rest is essentially what we decide when doing
			 * the faux-backbuffer, so here's me caring.
			 * -flibit
			 */
			selectedFormat = SurfaceFormat.Color; // Seriously?
			selectedDepthFormat = depthFormat;
			selectedMultiSampleCount = 0; // Okay, sure, sorry.

			return (	format == selectedFormat &&
					depthFormat == selectedDepthFormat &&
					multiSampleCount == selectedMultiSampleCount	);
		}

		#endregion
	}
}
