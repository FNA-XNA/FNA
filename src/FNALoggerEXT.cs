#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Graphics;

using System;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class FNALoggerEXT
	{
		#region Public Static Variables

		/* Use to spit out useful information to the player/dev */
		public static Action<string> LogInfo;

		/* Use when something sketchy happens, but isn't deadly */
		public static Action<string> LogWarn;

		/* Use when something has gone horribly, horribly wrong */
		public static Action<string> LogError;

		#endregion

		#region Private Static Variables

		private static FNA3D.FNA3D_LogFunc LogInfoFunc = FNA3DLogInfo;
		private static FNA3D.FNA3D_LogFunc LogWarnFunc = FNA3DLogWarn;
		private static FNA3D.FNA3D_LogFunc LogErrorFunc = FNA3DLogError;

		#endregion

		#region Internal Static Functions

		internal static void Initialize()
		{
			/* Don't overwrite application log hooks! */
			if (FNALoggerEXT.LogInfo == null)
			{
				FNALoggerEXT.LogInfo = Console.WriteLine;
			}
			if (FNALoggerEXT.LogWarn == null)
			{
				FNALoggerEXT.LogWarn = Console.WriteLine;
			}
			if (FNALoggerEXT.LogError == null)
			{
				FNALoggerEXT.LogError = Console.WriteLine;
			}

			/* Try to hook into the FNA3D logging system */
			try
			{
				FNA3D.FNA3D_HookLogFunctions(
					LogInfoFunc,
					LogWarnFunc,
					LogErrorFunc
				);
			}
			catch (DllNotFoundException)
			{
				/* Nothing to see here... */
			}
		}

		#endregion

		#region Private Static Functions

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogInfo(string msg)
		{
			LogInfo(msg);
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogWarn(string msg)
		{
			LogWarn(msg);
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogError(string msg)
		{
			LogError(msg);
			throw new InvalidOperationException(msg);
		}

		#endregion
	}
}
