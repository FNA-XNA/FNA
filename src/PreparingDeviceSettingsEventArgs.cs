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

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// The arguments to the <see cref="GraphicsDeviceManager.PreparingDeviceSettings"/> event.
	/// </summary>
	public class PreparingDeviceSettingsEventArgs : EventArgs
	{
		#region Public Properties

		/// <summary>
		/// The default settings that will be used in device creation.
		/// </summary>
		public GraphicsDeviceInformation GraphicsDeviceInformation
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		/// <summary>
		/// Create a new instance of the event.
		/// </summary>
		/// <param name="graphicsDeviceInformation">The default settings to be used in device creation.</param>
		public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
		{
			GraphicsDeviceInformation = graphicsDeviceInformation;
		}

		#endregion
	}
}
