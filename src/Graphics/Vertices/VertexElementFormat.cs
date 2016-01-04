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
	/// Defines vertex element formats.
	/// </summary>
	public enum VertexElementFormat
	{
		/// <summary>
		/// Single 32-bit floating point number.
		/// </summary>
		Single,
		/// <summary>
		/// Two component 32-bit floating point number.
		/// </summary>
		Vector2,
		/// <summary>
		/// Three component 32-bit floating point number.
		/// </summary>
		Vector3,
		/// <summary>
		/// Four component 32-bit floating point number.
		/// </summary>
		Vector4,
		/// <summary>
		/// Four component, packed unsigned byte, mapped to 0 to 1 range.
		/// </summary>
		Color,
		/// <summary>
		/// Four component unsigned byte.
		/// </summary>
		Byte4,
		/// <summary>
		/// Two component signed 16-bit integer.
		/// </summary>
		Short2,
		/// <summary>
		/// Four component signed 16-bit integer.
		/// </summary>
		Short4,
		/// <summary>
		/// Normalized, two component signed 16-bit integer.
		/// </summary>
		NormalizedShort2,
		/// <summary>
		/// Normalized, four component signed 16-bit integer.
		/// </summary>
		NormalizedShort4,
		/// <summary>
		/// Two component 16-bit floating point number.
		/// </summary>
		HalfVector2,
		/// <summary>
		/// Four component 16-bit floating point number.
		/// </summary>
		HalfVector4
	}
}
