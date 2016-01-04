#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class TextureReader : ContentTypeReader<Texture>
	{
		#region Protected Read Method

		protected internal override Texture Read(
			ContentReader reader,
			Texture existingInstance
		) {
			return existingInstance;
		}

		#endregion
	}
}
