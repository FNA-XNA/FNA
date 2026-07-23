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
				gdi.PresentationParameters.BackBufferWidth == PresentationParameters.BackBufferWidth &&
				gdi.PresentationParameters.BackBufferHeight == PresentationParameters.BackBufferHeight &&
				gdi.PresentationParameters.BackBufferFormat == PresentationParameters.BackBufferFormat &&
				gdi.PresentationParameters.DepthStencilFormat == PresentationParameters.DepthStencilFormat &&
				gdi.PresentationParameters.MultiSampleCount == PresentationParameters.MultiSampleCount &&
				gdi.PresentationParameters.DisplayOrientation == PresentationParameters.DisplayOrientation &&
				gdi.PresentationParameters.PresentationInterval == PresentationParameters.PresentationInterval &&
				gdi.PresentationParameters.RenderTargetUsage == PresentationParameters.RenderTargetUsage &&
				gdi.PresentationParameters.DeviceWindowHandle == PresentationParameters.DeviceWindowHandle &&
				gdi.PresentationParameters.IsFullScreen == PresentationParameters.IsFullScreen
			);
		}
		public override int GetHashCode()
		{
			return (
				GraphicsProfile.GetHashCode() ^
				Adapter.GetHashCode() ^
				PresentationParameters.BackBufferWidth.GetHashCode() ^
				PresentationParameters.BackBufferHeight.GetHashCode() ^
				PresentationParameters.BackBufferFormat.GetHashCode() ^
				PresentationParameters.DepthStencilFormat.GetHashCode() ^
				PresentationParameters.MultiSampleCount.GetHashCode() ^
				PresentationParameters.DisplayOrientation.GetHashCode() ^
				PresentationParameters.PresentationInterval.GetHashCode() ^
				PresentationParameters.RenderTargetUsage.GetHashCode() ^
				PresentationParameters.DeviceWindowHandle.GetHashCode() ^
				PresentationParameters.IsFullScreen.GetHashCode()
			);
		}

		public GraphicsDeviceInformation Clone()
		{
			return new GraphicsDeviceInformation()
			{
				Adapter = Adapter,
				GraphicsProfile = GraphicsProfile,
				PresentationParameters = PresentationParameters.Clone()
			};
		}

		#endregion
	}
}
