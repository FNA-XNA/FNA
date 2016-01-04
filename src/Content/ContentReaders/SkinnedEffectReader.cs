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
	class SkinnedEffectReader : ContentTypeReader<SkinnedEffect>
	{
		#region Protected Read Method

		protected internal override SkinnedEffect Read(
			ContentReader input,
			SkinnedEffect existingInstance
		) {
			SkinnedEffect effect = new SkinnedEffect(input.GraphicsDevice);
			effect.Texture = input.ReadExternalReference<Texture>() as Texture2D;
			effect.WeightsPerVertex = input.ReadInt32();
			effect.DiffuseColor = input.ReadVector3();
			effect.EmissiveColor = input.ReadVector3();
			effect.SpecularColor = input.ReadVector3();
			effect.SpecularPower = input.ReadSingle();
			effect.Alpha = input.ReadSingle();
			return effect;
		}

		#endregion
	}
}
