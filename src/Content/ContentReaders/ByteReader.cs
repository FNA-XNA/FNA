#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class ByteReader : ContentTypeReader<byte>
	{
		#region Internal Constructor

		internal ByteReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override byte Read(
			ContentReader input,
			byte existingInstance
		) {
			return input.ReadByte();
		}

		#endregion
	}
}
