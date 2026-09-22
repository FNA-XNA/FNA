#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
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
	class BoundingBoxReader : ContentTypeReader<BoundingBox>
	{
		#region Protected Read Method

		protected internal override BoundingBox Read(
			ContentReader input,
			BoundingBox existingInstance
		) {
#if NETCOREAPP3_0_OR_GREATER
			if (input.Read(MemoryMarshal.CreateSpan(ref Unsafe.As<BoundingBox, byte>(ref existingInstance), 24)) != 24)
			{
				throw new EndOfStreamException("Unable to read beyond the end of the stream.");
			}
#else
			existingInstance.Min = input.ReadVector3();
			existingInstance.Max = input.ReadVector3();
#endif
			return existingInstance;
		}

		#endregion
	}
}
