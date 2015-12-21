#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public interface IEffectFog
	{
		Vector3 FogColor
		{
			get;
			set;
		}

		bool FogEnabled
		{
			get;
			set;
		}

		float FogEnd
		{
			get;
			set;
		}

		float FogStart
		{
			get;
			set;
		}
	}
}
