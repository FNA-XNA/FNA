#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Specifies a type of dead zone processing to apply to the controllers analog sticks when
	/// calling GetState.
	/// </summary>
	/// <param name="Circular">
	/// The combined X and Y position of each stick is compared to the dead zone. This provides
	/// better control than IndependentAxes when the stick is used as a two-dimensional control
	/// surface, such as when controlling a character's view in a first-person game.
	/// </param>
	/// <param name="IndependentAxes">
	/// The X and Y positions of each stick are compared against the dead zone independently.
	/// This setting is the default when calling GetState.
	/// </param>
	/// <param name="None">
	/// The values of each stick are not processed and are returned by GetState as "raw" values.
	/// This is best if you intend to implement your own dead zone processing.
	/// </param>
	public enum GamePadDeadZone
	{
		None,
		IndependentAxes,
		Circular
	}
}
