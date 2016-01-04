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

namespace Microsoft.Xna.Framework.Graphics
{
	[Serializable]
	public sealed class NoSuitableGraphicsDeviceException : Exception
	{
		public NoSuitableGraphicsDeviceException()
			: base()
		{
		}

		public NoSuitableGraphicsDeviceException(string message)
			: base(message)
		{
		}

		public NoSuitableGraphicsDeviceException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
