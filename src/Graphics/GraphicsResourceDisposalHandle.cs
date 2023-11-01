#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2023 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	// This allows us to defer native dispose calls from the finalizer thread.
	internal struct GraphicsResourceDisposalHandle
	{
		internal Action<IntPtr, IntPtr> disposeAction;
		internal IntPtr resourceHandle;

		public void Dispose(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}

			if (disposeAction == null)
			{
				return;
			}

			disposeAction(device.GLDevice, resourceHandle);
		}
	}
}
