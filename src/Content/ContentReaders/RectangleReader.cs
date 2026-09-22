#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
#if NETCOREAPP3_0_OR_GREATER
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endif
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class RectangleReader : ContentTypeReader<Rectangle>
	{
		#region Internal Constructor

		internal RectangleReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Rectangle Read(
			ContentReader input,
			Rectangle existingInstance
		) {
#if NETCOREAPP3_0_OR_GREATER
			if (input.Read(MemoryMarshal.CreateSpan(ref Unsafe.As<Rectangle, byte>(ref existingInstance), 16)) != 16)
			{
				throw new EndOfStreamException("Unable to read beyond the end of the stream.");
			}
#else
			existingInstance.X = input.ReadInt32();
			existingInstance.Y = input.ReadInt32();
			existingInstance.Width = input.ReadInt32();
			existingInstance.Height = input.ReadInt32();
#endif
			return existingInstance;
		}

		#endregion
	}
}
