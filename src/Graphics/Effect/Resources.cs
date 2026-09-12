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
	internal static class Resources
	{
		#region Public Static Properties

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

		/* This Effect is used by the AV1 VideoPlayer. */
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

		private static byte[] yuvToRGBAEffect;
		private static byte[] yuvToRGBAEffectR;

		#endregion

		#region Private Static Methods

		internal static byte[] GetResource(string name)
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

	#region Internal Static EffectCode Classes

	internal static class AlphaTestEffectCode
	{
		internal static byte[] Code = Resources.GetResource("AlphaTestEffect");
	}

	internal static class BasicEffectCode
	{
		internal static byte[] Code = Resources.GetResource("BasicEffect");
	}

	internal static class DualTextureEffectCode
	{
		internal static byte[] Code = Resources.GetResource("DualTextureEffect");
	}

	internal static class EnvironmentMapEffectCode
	{
		internal static byte[] Code = Resources.GetResource("EnvironmentMapEffect");
	}

	internal static class SkinnedEffectCode
	{
		internal static byte[] Code = Resources.GetResource("SkinnedEffect");
	}

	internal static class SpriteEffectCode
	{
		internal static byte[] Code = Resources.GetResource("SpriteEffect");
	}

	#endregion
}
