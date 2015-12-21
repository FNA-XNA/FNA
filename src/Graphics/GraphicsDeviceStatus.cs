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
	/// Describes the status of the <see cref="GraphicsDevice"/>.
	/// </summary>
	public enum GraphicsDeviceStatus
	{
		/// <summary>
		/// The device is normal.
		/// </summary>
		Normal,
		/// <summary>
		/// The device has been lost.
		/// </summary>
		Lost,
		/// <summary>
		/// The device has not been reset.
		/// </summary>
		NotReset
	}
}
