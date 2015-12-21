#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class SpriteFontReader : ContentTypeReader<SpriteFont>
	{
		#region Internal Constructor

		internal SpriteFontReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override SpriteFont Read(
			ContentReader input,
			SpriteFont existingInstance
		) {
			if (existingInstance != null)
			{
				// Read the texture into the existing texture instance
				input.ReadObject<Texture2D>(existingInstance.textureValue);

				/* Discard the rest of the SpriteFont data as we are only
				 * reloading GPU resources for now
				 */
				input.ReadObject<List<Rectangle>>();
				input.ReadObject<List<Rectangle>>();
				input.ReadObject<List<char>>();
				input.ReadInt32();
				input.ReadSingle();
				input.ReadObject<List<Vector3>>();
				if (input.ReadBoolean())
				{
					input.ReadChar();
				}
				return existingInstance;
			}
			else
			{
				// Create a fresh SpriteFont instance
				Texture2D texture = input.ReadObject<Texture2D>();
				List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
				List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
				List<char> charMap = input.ReadObject<List<char>>();
				int lineSpacing = input.ReadInt32();
				float spacing = input.ReadSingle();
				List<Vector3> kerning = input.ReadObject<List<Vector3>>();
				char? defaultCharacter = null;
				if (input.ReadBoolean())
				{
					defaultCharacter = new char?(input.ReadChar());
				}
				return new SpriteFont(
					texture,
					glyphs,
					cropping,
					charMap,
					lineSpacing,
					spacing,
					kerning,
					defaultCharacter
				);
			}
		}

		#endregion
	}
}
