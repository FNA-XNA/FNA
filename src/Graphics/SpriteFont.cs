#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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
			get
			{
				return lineSpacing;
			}
			set
			{
				lineSpacing = value;
			}
		}

		public float Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				spacing = value;
			}
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

		/* If, by chance, you're seeing this and thinking about using
		 * reflection to access the fields:
		 * Don't.
		 * To date, one (1) game is using the fields directly,
		 * even though the properties are publicly accessible.
		 * Not even FNA uses the fields directly.
		 * -ade
		 */
		internal int lineSpacing;
		internal float spacing;

		/* This is not a part of the spec as far as we know, but we
		 * added this because it's WAY faster than going to characterMap
		 * and calling IndexOf on each character.
		 */
		internal Dictionary<char, int> characterIndexMap;

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

			characterIndexMap = new Dictionary<char, int>(characters.Count);
			for (int i = 0; i < characters.Count; i += 1)
			{
				characterIndexMap[characters[i]] = i;
			}
		}

		#endregion

		#region Internal MeasureString Method

		internal Vector2 MeasureString<T>(ref T text) where T : IReadOnlyCharList
		{
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
				int index;
				if (!characterIndexMap.TryGetValue(c, out index))
				{
					if (!DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = characterIndexMap[DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				Vector3 cKern = kerning[index];
				if (firstInLine)
				{
					curLineWidth += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += Spacing + cKern.X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += cKern.Y + cKern.Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				int cCropHeight = croppingData[index].Height;
				if (cCropHeight > finalLineHeight)
				{
					finalLineHeight = cCropHeight;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}

		#endregion

		#region Public MeasureString Methods

		public Vector2 MeasureString(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			StringView view = new StringView(text);
			return MeasureString(ref view);
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			StringBuilderView view = new StringBuilderView(text);
			return MeasureString(ref view);
		}

		#endregion

		#region Internal IReadOnlyCharList Structs
		internal interface IReadOnlyCharList
		{
			int Length { get; }
			char this[int index] { get; }
		}
		internal struct StringView : IReadOnlyCharList
		{
			private readonly string view;
			internal StringView(string view)
			{
				this.view = view;
			}
			public int Length
			{
				get
				{
					return view.Length;
				}
			}
			public char this[int index]
			{
				get
				{
					return view[index];
				}
			}
		}
		internal struct StringBuilderView : IReadOnlyCharList
		{
			private readonly StringBuilder view;
			internal StringBuilderView(StringBuilder view)
			{
				this.view = view;
			}
			public int Length
			{
				get
				{
					return view.Length;
				}
			}
			public char this[int index]
			{
				get
				{
					return view[index];
				}
			}
		}
		#endregion
	}
}
