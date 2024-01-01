#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.instanceplaylimitexception.aspx
	[Serializable]
	public sealed class InstancePlayLimitException : ExternalException
	{
		public InstancePlayLimitException()
		{
		}

		public InstancePlayLimitException(String message)
			: base(message)
		{
		}

		public InstancePlayLimitException(String message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
