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
	/// <summary>
	/// Defines filtering types for texture sampler.
	/// </summary>
	public enum TextureFilter
	{
		/// <summary>
		/// Use linear filtering.
		/// </summary>
		Linear,
		/// <summary>
		/// Use point filtering.
		/// </summary>
		Point,
		/// <summary>
		/// Use anisotropic filtering.
		/// </summary>
		Anisotropic,
		/// <summary>
		/// Use linear filtering to shrink or expand, and point filtering between mipmap levels (mip).
		/// </summary>
		LinearMipPoint,
		/// <summary>
		/// Use point filtering to shrink (minify) or expand (magnify), and linear filtering between mipmap levels.
		/// </summary>
		PointMipLinear,
		/// <summary>
		/// Use linear filtering to shrink, point filtering to expand, and linear filtering between mipmap levels.
		/// </summary>
		MinLinearMagPointMipLinear,
		/// <summary>
		/// Use linear filtering to shrink, point filtering to expand, and point filtering between mipmap levels.
		/// </summary>
		MinLinearMagPointMipPoint,
		/// <summary>
		/// Use point filtering to shrink, linear filtering to expand, and linear filtering between mipmap levels.
		/// </summary>
		MinPointMagLinearMipLinear,
		/// <summary>
		/// Use point filtering to shrink, linear filtering to expand, and point filtering between mipmap levels.
		/// </summary>
		MinPointMagLinearMipPoint,
	}
}
