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
	public interface IEffectLights
	{
		Vector3 AmbientLightColor
		{
			get;
			set;
		}

		DirectionalLight DirectionalLight0
		{
			get;
		}

		DirectionalLight DirectionalLight1
		{
			get;
		}

		DirectionalLight DirectionalLight2
		{
			get;
		}

		bool LightingEnabled
		{
			get;
			set;
		}

		void EnableDefaultLighting();
	}
}
