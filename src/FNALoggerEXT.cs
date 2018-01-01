#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
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
	public static class FNALoggerEXT
	{
		/* Use to spit out useful information to the player/dev */
		public static Action<string> LogInfo;

		/* Use when something sketchy happens, but isn't deadly */
		public static Action<string> LogWarn;

		/* Use when something has gone horribly, horribly wrong */
		public static Action<string> LogError;
	}
}