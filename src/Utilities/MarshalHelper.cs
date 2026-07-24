#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class MarshalHelper
	{
		internal static int SizeOf<T>()
		{
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
			return Marshal.SizeOf<T>();
#else
			return Marshal.SizeOf(typeof(T));
#endif
		}

		internal static string PtrToInternedStringAnsi(IntPtr ptr)
		{
			string result = Marshal.PtrToStringAnsi(ptr);
			if (result != null)
				result = string.Intern(result);
			return result;
		}

		internal static unsafe int GetHashCode<T>(T value) where T : struct
		{

			int hashcode = 0;
			int* ptr = (int*) &value;
			for (int i = sizeof(T) / 4 - 1; i >= 0; i--)
			{
				hashcode ^= ptr[i];
			}
			if (hashcode == 0)
			{
				hashcode = int.MaxValue;
			}
			return hashcode;
		}
	}
}
