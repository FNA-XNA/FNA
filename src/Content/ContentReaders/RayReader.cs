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
	internal class RayReader : ContentTypeReader<Ray>
	{
		#region Internal Constructor

		internal RayReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Ray Read(
			ContentReader input,
			Ray existingInstance
		) {
#if NETCOREAPP3_0_OR_GREATER
			if (input.Read(MemoryMarshal.CreateSpan(ref Unsafe.As<Ray, byte>(ref existingInstance), 24)) != 24)
			{
				throw new EndOfStreamException("Unable to read beyond the end of the stream.");
			}
#else
			existingInstance.Position = input.ReadVector3();
			existingInstance.Direction = input.ReadVector3();
#endif
			return existingInstance;
		}

		#endregion
	}
}
