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
	public interface IGraphicsDeviceService
	{
		GraphicsDevice GraphicsDevice
		{
			get;
		}

		event EventHandler<EventArgs> DeviceCreated;
		event EventHandler<EventArgs> DeviceDisposing;
		event EventHandler<EventArgs> DeviceReset;
		event EventHandler<EventArgs> DeviceResetting;
	}
}
