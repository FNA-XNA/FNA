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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Storage
{
	public class StorageDeviceNotConnectedException : ExternalException
	{
		public StorageDeviceNotConnectedException()
			: base()
		{
		}

		public StorageDeviceNotConnectedException(string message)
			: base(message)
		{
		}

		public StorageDeviceNotConnectedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
