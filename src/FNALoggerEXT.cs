#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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
		}

		internal static void HookFNA3D()
		{
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
		private static void FNA3DLogInfo(IntPtr msg)
		{
			LogInfo(UTF8_ToManaged(msg));
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogWarn(IntPtr msg)
		{
			LogWarn(UTF8_ToManaged(msg));
		}

		[ObjCRuntime.MonoPInvokeCallback(typeof(FNA3D.FNA3D_LogFunc))]
		private static void FNA3DLogError(IntPtr msg)
		{
			string err = UTF8_ToManaged(msg);
			LogError(err);
			throw new InvalidOperationException(err);
		}

		private static unsafe string UTF8_ToManaged(IntPtr s)
		{
			/* We get to do strlen ourselves! */
			byte* ptr = (byte*) s;
			while (*ptr != 0)
			{
				ptr++;
			}

			/* TODO: This #ifdef is only here because the equivalent
			 * .NET 2.0 constructor appears to be less efficient?
			 * Here's the pretty version, maybe steal this instead:
			 *
			string result = new string(
				(sbyte*) s, // Also, why sbyte???
				0,
				(int) (ptr - (byte*) s),
				System.Text.Encoding.UTF8
			);
			 * See the CoreCLR source for more info.
			 * -flibit
			 */
#if NETSTANDARD2_0
			/* Modern C# lets you just send the byte*, nice! */
			string result = System.Text.Encoding.UTF8.GetString(
				(byte*) s,
				(int) (ptr - (byte*) s)
			);
#else
			/* Old C# requires an extra memcpy, bleh! */
			int len = (int) (ptr - (byte*) s);
			if (len == 0)
			{
				return string.Empty;
			}
			char* chars = stackalloc char[len];
			int strLen = System.Text.Encoding.UTF8.GetChars((byte*) s, len, chars, len);
			string result = new string(chars, 0, strLen);
#endif
			return result;
		}

		#endregion
	}
}
