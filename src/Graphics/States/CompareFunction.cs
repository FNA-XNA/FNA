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
	/// The comparison function used for depth, stencil, and alpha tests.
	/// </summary>
	public enum CompareFunction
	{
		/// <summary>
		/// Always passes the test.
		/// </summary>
		Always,
		/// <summary>
		/// Never passes the test.
		/// </summary>
		Never,
		/// <summary>
		/// Passes the test when the new pixel value is less than current pixel value.
		/// </summary>
		Less,
		/// <summary>
		/// Passes the test when the new pixel value is less than or equal to current pixel value.
		/// </summary>
		LessEqual,
		/// <summary>
		/// Passes the test when the new pixel value is equal to current pixel value.
		/// </summary>
		Equal,
		/// <summary>
		/// Passes the test when the new pixel value is greater than or equal to current pixel value.
		/// </summary>
		GreaterEqual,
		/// <summary>
		/// Passes the test when the new pixel value is greater than current pixel value.
		/// </summary>
		Greater,
		/// <summary>
		/// Passes the test when the new pixel value does not equal to current pixel value.
		/// </summary>
		NotEqual
	}
}
