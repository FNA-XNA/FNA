#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines usage for vertex elements.
	/// </summary>
	public enum VertexElementUsage
	{
		/// <summary>
		/// Position data.
		/// </summary>
		Position,
		/// <summary>
		/// Color data.
		/// </summary>
		Color,
		/// <summary>
		/// Texture coordinate data or can be used for user-defined data.
		/// </summary>
		TextureCoordinate,
		/// <summary>
		/// Normal data.
		/// </summary>
		Normal,
		/// <summary>
		/// Binormal data.
		/// </summary>
		Binormal,
		/// <summary>
		/// Tangent data.
		/// </summary>
		Tangent,
		/// <summary>
		/// Blending indices data.
		/// </summary>
		BlendIndices,
		/// <summary>
		/// Blending weight data.
		/// </summary>
		BlendWeight,
		/// <summary>
		/// Depth data.
		/// </summary>
		Depth,
		/// <summary>
		/// Fog data.
		/// </summary>
		Fog,
		/// <summary>
		/// Point size data. Usable for drawing point sprites.
		/// </summary>
		PointSize,
		/// <summary>
		/// Sampler data for specifies the displacement value to look up.
		/// </summary>
		Sample,
		/// <summary>
		/// Single, positive float value, specifies a tessellation factor used in the tessellation unit to control the rate of tessellation.
		/// </summary>
		TessellateFactor
	}
}
