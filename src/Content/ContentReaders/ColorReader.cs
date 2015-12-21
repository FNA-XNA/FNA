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
	internal class ColorReader : ContentTypeReader<Color>
	{
		#region Internal Constructor

		internal ColorReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Color Read(
			ContentReader input,
			Color existingInstance
		) {
			/* Read RGBA as four separate bytes to make sure we
			 * comply with XNB format document
			 */
			byte r = input.ReadByte();
			byte g = input.ReadByte();
			byte b = input.ReadByte();
			byte a = input.ReadByte();
			return new Color(r, g, b, a);
		}

		#endregion
	}
}
