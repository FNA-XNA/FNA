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
	/// Defines stencil buffer operations.
	/// </summary>
	public enum StencilOperation
	{
		/// <summary>
		/// Does not update the stencil buffer entry.
		/// </summary>
		Keep,
		/// <summary>
		/// Sets the stencil buffer entry to 0.
		/// </summary>
		Zero,
		/// <summary>
		/// Replaces the stencil buffer entry with a reference value.
		/// </summary>
		Replace,
		/// <summary>
		/// Increments the stencil buffer entry, wrapping to 0 if the new value exceeds the maximum value.
		/// </summary>
		Increment,
		/// <summary>
		/// Decrements the stencil buffer entry, wrapping to the maximum value if the new value is less than 0.
		/// </summary>
		Decrement,
		/// <summary>
		/// Increments the stencil buffer entry, clamping to the maximum value.
		/// </summary>
		IncrementSaturation,
		/// <summary>
		/// Decrements the stencil buffer entry, clamping to 0.
		/// </summary>
		DecrementSaturation,
		/// <summary>
		/// Inverts the bits in the stencil buffer entry.
		/// </summary>
		Invert
	}
}
