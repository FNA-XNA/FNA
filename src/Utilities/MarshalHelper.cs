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

		/* Hope use generic to avoid boxing. But it need where T:unmanaged. It need C# 7.2
		 * error CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `T'
		 * - 7aGiven */
		internal static unsafe int GetHashCode(object obj)
		{
			int hashcode = 0;
			GCHandle gchandle = GCHandle.Alloc(obj, GCHandleType.Pinned);
			try
			{
				int* ptr = (int*) gchandle.AddrOfPinnedObject().ToPointer();
				for (int i = Marshal.SizeOf(obj) / 4 - 1; i >= 0; i--)
				{
					hashcode ^= ptr[i];
				}
			}
			finally
			{
				gchandle.Free();
			}
			return hashcode == 0 ? int.MaxValue : hashcode;
		}
	}
}
