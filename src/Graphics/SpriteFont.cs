#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.spritefont.aspx
	public sealed class SpriteFont
	{
		#region Public Properties

		public ReadOnlyCollection<char> Characters
		{
			get;
			private set;
		}

		public char? DefaultCharacter
		{
			get;
			set;
		}

		public int LineSpacing
		{
			get;
			set;
		}

		public float Spacing
		{
			get;
			set;
		}

		#endregion

		#region Internal Variables

		/* I've had a bunch of games use reflection on SpriteFont to get
		 * this data. Keep these names as they are for XNA4 accuracy!
		 * -flibit
		 */
		internal Texture2D textureValue;
		internal List<Rectangle> glyphData;
		internal List<Rectangle> croppingData;
		internal List<Vector3> kerning;
		internal List<char> characterMap;

		#endregion

		#region Internal Constructor

		internal SpriteFont(
			Texture2D texture,
			List<Rectangle> glyphBounds,
			List<Rectangle> cropping,
			List<char> characters,
			int lineSpacing,
			float spacing,
			List<Vector3> kerningData,
			char? defaultCharacter
		) {
			Characters = new ReadOnlyCollection<char>(characters.ToArray());
			DefaultCharacter = defaultCharacter;
			LineSpacing = lineSpacing;
			Spacing = spacing;

			textureValue = texture;
			glyphData = glyphBounds;
			croppingData = cropping;
			kerning = kerningData;
			characterMap = characters;
		}

		#endregion

		#region Public MeasureString Methods

		public Vector2 MeasureString(string text)
		{
			/* FIXME: This method is a duplicate of MeasureString(StringBuilder)!
			 * The only difference is how we iterate through the string.
			 * -flibit
			 */
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			// FIXME: This needs an accuracy check! -flibit

			Vector2 result = Vector2.Zero;
			float curLineWidth = 0.0f;
			float finalLineHeight = LineSpacing;
			bool firstInLine = true;

			foreach (char c in text)
			{
				// Special characters
				if (c == '\r')
				{
					continue;
				}
				if (c == '\n')
				{
					result.X = Math.Max(result.X, curLineWidth);
					result.Y += LineSpacing;
					curLineWidth = 0.0f;
					finalLineHeight = LineSpacing;
					firstInLine = true;
					continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				int index = characterMap.IndexOf(c);
				if (index == -1)
				{
					if (!DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = characterMap.IndexOf(DefaultCharacter.Value);
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				if (firstInLine)
				{
					curLineWidth += Math.Abs(kerning[index].X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += Spacing + kerning[index].X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += kerning[index].Y + kerning[index].Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				if (croppingData[index].Height > finalLineHeight)
				{
					finalLineHeight = croppingData[index].Height;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			/* FIXME: This method is a duplicate of MeasureString(string)!
			 * The only difference is how we iterate through the StringBuilder.
			 * We don't use ToString() since it generates garbage.
			 * -flibit
			 */
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			// FIXME: This needs an accuracy check! -flibit

			Vector2 result = Vector2.Zero;
			float curLineWidth = 0.0f;
			float finalLineHeight = LineSpacing;
			bool firstInLine = true;

			for (int i = 0; i < text.Length; i += 1)
			{
				char c = text[i];

				// Special characters
				if (c == '\r')
				{
					continue;
				}
				if (c == '\n')
				{
					result.X = Math.Max(result.X, curLineWidth);
					result.Y += LineSpacing;
					curLineWidth = 0.0f;
					finalLineHeight = LineSpacing;
					firstInLine = true;
					continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				int index = characterMap.IndexOf(c);
				if (index == -1)
				{
					if (!DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = characterMap.IndexOf(DefaultCharacter.Value);
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				if (firstInLine)
				{
					curLineWidth += Math.Abs(kerning[index].X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += Spacing + kerning[index].X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += kerning[index].Y + kerning[index].Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				if (croppingData[index].Height > finalLineHeight)
				{
					finalLineHeight = croppingData[index].Height;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}

		#endregion
	}
}
