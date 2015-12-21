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
	internal class UInt64Reader : ContentTypeReader<ulong>
	{
		#region Internal Constructor

		internal UInt64Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override ulong Read(
			ContentReader input,
			ulong existingInstance
		) {
			return input.ReadUInt64();
		}

		#endregion
	}
}
