#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class UInt16Reader : ContentTypeReader<ushort>
	{
		#region Internal Constructor

		internal UInt16Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override ushort Read(
			ContentReader input,
			ushort existingInstance
		) {
			return input.ReadUInt16();
		}

		#endregion
	}
}
