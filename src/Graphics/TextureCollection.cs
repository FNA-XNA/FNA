#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
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
#if DEBUG
				// XNA checks for disposed textures here! -flibit
				if (value != null && value.IsDisposed)
				{
					throw new ObjectDisposedException(
						value.GetType().ToString()
					);
				}
#endif
				textures[index] = value;
				modifiedSamplers[index] = true;
			}
		}

		#endregion

		#region Private Variables

		private readonly Texture[] textures;
		private readonly bool[] modifiedSamplers;

		#endregion

		#region Internal Constructor

		internal TextureCollection(
			int slots,
			bool[] modSamplers
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
