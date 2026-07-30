#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion License

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	[Serializable]
	public class DisplayMode
	{
		#region Public Properties

		public float AspectRatio
		{
			get
			{
				if (Height == 0)
				{
					return 0f;
				}
				return (float) Width / (float) Height;
			}
		}

		public SurfaceFormat Format
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public Rectangle TitleSafeArea
		{
			get
			{
				return new Rectangle(0, 0, Width, Height);
			}
		}

		#endregion

		#region Internal Constructor

		internal DisplayMode(int width, int height, SurfaceFormat format)
		{
			Width = width;
			Height = height;
			Format = format;
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override string ToString()
		{
			return (
				"{{Width:" + Width.ToString() +
				" Height:" + Height.ToString() +
				" Format:" + Format.ToString() +
				"}}"
			);
		}

		#endregion
	}
}
