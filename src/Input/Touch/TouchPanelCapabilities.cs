#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchpanelcapabilities.aspx
	public struct TouchPanelCapabilities
	{
		#region Public Properties

		public bool IsConnected
		{
			get;
			set;
		}

		public int MaximumTouchCount
		{
			get;
			set;
		}

		#endregion

		#region Internal Constructor

		internal TouchPanelCapabilities(
			bool isConnected,
			int maximumTouchCount
		) : this() {
			IsConnected = isConnected;
			MaximumTouchCount = maximumTouchCount;
		}

		#endregion
	}
}
