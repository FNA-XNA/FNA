#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal class Resources
	{
		#region Public Static Properties

		public static byte[] AlphaTestEffect
		{
			get
			{
				if (alphaTestEffect == null)
				{
					alphaTestEffect = GetResource("AlphaTestEffect");
				}
				return alphaTestEffect;
			}
		}

		public static byte[] BasicEffect
		{
			get
			{
				if (basicEffect == null)
				{
					basicEffect = GetResource("BasicEffect");
				}
				return basicEffect;
			}
		}

		public static byte[] DualTextureEffect
		{
			get
			{
				if (dualTextureEffect == null)
				{
					dualTextureEffect = GetResource("DualTextureEffect");
				}
				return dualTextureEffect;
			}
		}

		public static byte[] EnvironmentMapEffect
		{
			get
			{
				if (environmentMapEffect == null)
				{
					environmentMapEffect = GetResource("EnvironmentMapEffect");
				}
				return environmentMapEffect;
			}
		}

		public static byte[] SkinnedEffect
		{
			get
			{
				if (skinnedEffect == null)
				{
					skinnedEffect = GetResource("SkinnedEffect");
				}
				return skinnedEffect;
			}
		}

		public static byte[] SpriteEffect
		{
			get
			{
				if (spriteEffect == null)
				{
					spriteEffect = GetResource("SpriteEffect");
				}
				return spriteEffect;
			}
		}

		/* This Effect is used by the Xiph VideoPlayer. */
		public static byte[] YUVToRGBAEffect
		{
			get
			{
				if (yuvToRGBAEffect == null)
				{
					yuvToRGBAEffect = GetResource("YUVToRGBAEffect");
				}
				return yuvToRGBAEffect;
			}
		}

		public static byte[] YUVToRGBAEffectR
		{
			get
			{
				if (yuvToRGBAEffectR == null)
				{
					yuvToRGBAEffectR = GetResource("YUVToRGBAEffectR");
				}
				return yuvToRGBAEffectR;
			}
		}

		#endregion

		#region Private Static Variables

		private static byte[] alphaTestEffect;
		private static byte[] basicEffect;
		private static byte[] dualTextureEffect;
		private static byte[] environmentMapEffect;
		private static byte[] skinnedEffect;
		private static byte[] spriteEffect;
		private static byte[] yuvToRGBAEffect;
		private static byte[] yuvToRGBAEffectR;

		#endregion

		#region Private Static Methods

		private static byte[] GetResource(string name)
		{
			Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(
				"Microsoft.Xna.Framework.Graphics.Effect.Resources." + name + ".fxb"
			);
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		#endregion
	}
}
