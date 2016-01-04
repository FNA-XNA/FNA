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
	/// Defines a set of graphic capabilities.
	/// </summary>
	public enum GraphicsProfile
	{
		/// <summary>
		/// Use a limited set of graphic features and capabilities, allowing the game to support the widest variety of devices.
		/// </summary>
		Reach,
		/// <summary>
		/// Use the largest available set of graphic features and capabilities to target devices, that have more enhanced graphic capabilities.
		/// </summary>
		HiDef
	}
}
