#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
			get;
			set;
		}

		#endregion

		#region Public Methods

		public GraphicsDeviceInformation Clone()
		{
			return new GraphicsDeviceInformation()
			{
				Adapter = this.Adapter,
				GraphicsProfile = this.GraphicsProfile,
				PresentationParameters = this.PresentationParameters.Clone()
			};
		}

		#endregion
	}
}
