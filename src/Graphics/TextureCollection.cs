#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class TextureCollection
	{
		#region Public Array Access Property

		public Texture this[int index]
		{
			get
			{
				return textures[index];
			}
			set
			{
				textures[index] = value;
				if (!modifiedSamplers.Contains(index))
				{
					modifiedSamplers.Enqueue(index);
				}
			}
		}

		#endregion

		#region Private Variables

		private readonly Texture[] textures;
		private readonly Queue<int> modifiedSamplers;

		#endregion

		#region Internal Constructor

		internal TextureCollection(
			int slots,
			Queue<int> modSamplers
		) {
			textures = new Texture[slots];
			modifiedSamplers = modSamplers;
			for (int i = 0; i < textures.Length; i += 1)
			{
				textures[i] = null;
			}
		}

		#endregion
	}
}
