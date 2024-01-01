#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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
	internal class EffectReader : ContentTypeReader<Effect>
	{
		#region Protected Read Method

		protected internal override Effect Read(
			ContentReader input,
			Effect existingInstance
		) {
			int length = input.ReadInt32();
			Effect effect = new Effect(
				input.ContentManager.GetGraphicsDevice(),
				input.ReadBytes(length)
			);
			effect.Name = input.AssetName;
			return effect;
		}

		#endregion
	}
}
