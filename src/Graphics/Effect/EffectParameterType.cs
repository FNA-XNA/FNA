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
	/// Defines types for effect parameters and shader constants.
	/// </summary>
	public enum EffectParameterType
	{
		/// <summary>
		/// Pointer to void type.
		/// </summary>
		Void,
		/// <summary>
		/// Boolean type. Any non-zero will be <c>true</c>; <c>false</c> otherwise.
		/// </summary>
		Bool,
		/// <summary>
		/// 32-bit integer type.
		/// </summary>
		Int32,
		/// <summary>
		/// Float type.
		/// </summary>
		Single,
		/// <summary>
		/// String type.
		/// </summary>
		String,
		/// <summary>
		/// Any texture type.
		/// </summary>
		Texture,
		/// <summary>
		/// 1D-texture type.
		/// </summary>
		Texture1D,
		/// <summary>
		/// 2D-texture type.
		/// </summary>
		Texture2D,
		/// <summary>
		/// 3D-texture type.
		/// </summary>
		Texture3D,
		/// <summary>
		/// Cubic texture type.
		/// </summary>
		TextureCube
	}
}
