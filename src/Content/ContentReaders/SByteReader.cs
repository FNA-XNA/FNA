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
	internal class SByteReader : ContentTypeReader<sbyte>
	{
		#region Internal Constructor

		internal SByteReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override sbyte Read(
			ContentReader input,
			sbyte existingInstance
		) {
			return input.ReadSByte();
		}

		#endregion
	}
}
