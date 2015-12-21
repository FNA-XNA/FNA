#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
	class EnvironmentMapEffectReader : ContentTypeReader<EnvironmentMapEffect>
	{
		#region Protected Read Method

		protected internal override EnvironmentMapEffect Read(
			ContentReader input,
			EnvironmentMapEffect existingInstance
		) {
			EnvironmentMapEffect effect = new EnvironmentMapEffect(input.GraphicsDevice);
			effect.Texture = input.ReadExternalReference<Texture>() as Texture2D;
			effect.EnvironmentMap = input.ReadExternalReference<TextureCube>() as TextureCube;
			effect.EnvironmentMapAmount = input.ReadSingle();
			effect.EnvironmentMapSpecular = input.ReadVector3();
			effect.FresnelFactor = input.ReadSingle();
			effect.DiffuseColor = input.ReadVector3();
			effect.EmissiveColor = input.ReadVector3();
			effect.Alpha = input.ReadSingle();
			return effect;
		}

		#endregion
	}
}
