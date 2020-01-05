#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public interface IEffectMatrices
	{
		Matrix Projection
		{
			get;
			set;
		}

		Matrix View
		{
			get;
			set;
		}

		Matrix World
		{
			get;
			set;
		}
	}
}
