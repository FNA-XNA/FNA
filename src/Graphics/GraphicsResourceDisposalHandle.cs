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
