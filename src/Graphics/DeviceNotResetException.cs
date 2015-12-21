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
	[Serializable]
	public sealed class DeviceNotResetException : Exception
	{
		public DeviceNotResetException()
			: base()
		{
		}

		public DeviceNotResetException(string message)
			: base(message)
		{
		}

		public DeviceNotResetException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
