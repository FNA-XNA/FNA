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
	class AlphaTestEffectReader : ContentTypeReader<AlphaTestEffect>
	{
		#region Protected Read Method

		protected internal override AlphaTestEffect Read(ContentReader input, AlphaTestEffect existingInstance)
		{
			AlphaTestEffect effect = new AlphaTestEffect(input.GraphicsDevice);
			effect.Texture = input.ReadExternalReference<Texture>() as Texture2D;
			effect.AlphaFunction = (CompareFunction) input.ReadInt32();
			effect.ReferenceAlpha = (int) input.ReadUInt32();
			effect.DiffuseColor = input.ReadVector3();
			effect.Alpha = input.ReadSingle();
			effect.VertexColorEnabled = input.ReadBoolean();
			return effect;
		}

		#endregion
	}
}
