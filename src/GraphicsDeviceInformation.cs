#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// The settings used in creation of the graphics device.
	/// See <see cref="GraphicsDeviceManager.PreparingDeviceSettings"/>.
	/// </summary>
	public class GraphicsDeviceInformation
	{
		#region Public Properties

		/// <summary>
		/// The graphics adapter on which the graphics device will be created.
		/// </summary>
		/// <remarks>
		/// This is only valid on desktop systems where multiple graphics
		/// adapters are possible. Defaults to <see cref="GraphicsAdapter.DefaultAdapter"/>.
		/// </remarks>
		public GraphicsAdapter Adapter
		{
			get;
			set;
		}

		/// <summary>
		/// The requested graphics device feature set.
		/// </summary>
		public GraphicsProfile GraphicsProfile
		{
			get;
			set;
		}

		/// <summary>
		/// The settings that define how graphics will be presented to the display.
		/// </summary>
		public PresentationParameters PresentationParameters
		{
			get { return presentationParameters; }
			set { presentationParameters = value; }
		}

		#endregion

		#region Private Variable

		private PresentationParameters presentationParameters = new PresentationParameters();

		#endregion

		#region Public Methods

		public override bool Equals(object obj)
		{
			GraphicsDeviceInformation gdi = obj as GraphicsDeviceInformation;
			return (
				gdi != null &&
				gdi.Adapter.Equals(Adapter) &&
				gdi.GraphicsProfile == GraphicsProfile &&
				gdi.presentationParameters.BackBufferWidth == presentationParameters.BackBufferWidth &&
				gdi.presentationParameters.BackBufferHeight == presentationParameters.BackBufferHeight &&
				gdi.presentationParameters.BackBufferFormat == presentationParameters.BackBufferFormat &&
				gdi.presentationParameters.DepthStencilFormat == presentationParameters.DepthStencilFormat &&
				gdi.presentationParameters.MultiSampleCount == presentationParameters.MultiSampleCount &&
				gdi.presentationParameters.DisplayOrientation == presentationParameters.DisplayOrientation &&
				gdi.presentationParameters.PresentationInterval == presentationParameters.PresentationInterval &&
				gdi.presentationParameters.RenderTargetUsage == presentationParameters.RenderTargetUsage &&
				gdi.presentationParameters.DeviceWindowHandle == presentationParameters.DeviceWindowHandle &&
				gdi.presentationParameters.IsFullScreen == presentationParameters.IsFullScreen
			);
		}
		public override int GetHashCode()
		{
			return (
				GraphicsProfile.GetHashCode() ^
				Adapter.GetHashCode() ^
				presentationParameters.BackBufferWidth.GetHashCode() ^
				presentationParameters.BackBufferHeight.GetHashCode() ^
				presentationParameters.BackBufferFormat.GetHashCode() ^
				presentationParameters.DepthStencilFormat.GetHashCode() ^
				presentationParameters.MultiSampleCount.GetHashCode() ^
				presentationParameters.DisplayOrientation.GetHashCode() ^
				presentationParameters.PresentationInterval.GetHashCode() ^
				presentationParameters.RenderTargetUsage.GetHashCode() ^
				presentationParameters.DeviceWindowHandle.GetHashCode() ^
				presentationParameters.IsFullScreen.GetHashCode()
			);
		}

		public GraphicsDeviceInformation Clone()
		{
			return new GraphicsDeviceInformation()
			{
				Adapter = Adapter,
				GraphicsProfile = GraphicsProfile,
				presentationParameters = presentationParameters.Clone()
			};
		}

		#endregion
	}
}
