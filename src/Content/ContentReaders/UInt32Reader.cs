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
	internal class UInt32Reader : ContentTypeReader<uint>
	{
		#region Internal Constructor

		internal UInt32Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override uint Read(
			ContentReader input,
			uint existingInstance
		) {
			return input.ReadUInt32();
		}

		#endregion
	}
}
