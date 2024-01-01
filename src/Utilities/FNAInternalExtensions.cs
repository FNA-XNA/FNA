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
using System.IO;
using System.Reflection;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class FNAInternalExtensions
	{
		#region MemoryStream.TryGetBuffer Extension

		private static readonly FieldInfo f_MemoryStream_Public =
			// .NET
			typeof(MemoryStream).GetField("_exposable", BindingFlags.NonPublic | BindingFlags.Instance) ??
			// Old Mono
			typeof(MemoryStream).GetField("allowGetBuffer", BindingFlags.NonPublic | BindingFlags.Instance);

		/// <summary>
		/// Returns the array of unsigned bytes from which this stream was created.
		/// The return value indicates whether the conversion succeeded.
		/// This is similar to .NET 4.6's TryGetBuffer.
		/// </summary>
		/// <param name="stream">The stream to get the buffer from.</param>
		/// <param name="buffer">The byte array from which this stream was created.</param>
		/// <returns><b>true</b> if the conversion was successful; otherwise, <b>false</b>.</returns>
		internal static bool TryGetBuffer(this MemoryStream stream, out byte[] buffer)
		{
			// Check if the buffer is public by reflecting into a known internal field.
			if (f_MemoryStream_Public != null)
			{
				if ((bool) f_MemoryStream_Public.GetValue(stream))
				{
					buffer = stream.GetBuffer();
					return true;
				}
				buffer = null;
				return false;
			}

			// If no known field can be found, use a horribly slow try-catch instead.
			try
			{
				buffer = stream.GetBuffer();
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				buffer = null;
				return false;
			}
		}

		#endregion
	}
}
