#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Defines the different tangent types to be calculated for <see cref="CurveKey"/> points in a <see cref="Curve"/>.
	/// </summary>
	public enum CurveTangent
	{
		/// <summary>
		/// The tangent which always has a value equal to zero.
		/// </summary>
		Flat,
		/// <summary>
		/// The tangent which contains a difference between current tangent value and the tangent value from the previous <see cref="CurveKey"/>.
		/// </summary>
		Linear,
		/// <summary>
		/// The smoouth tangent which contains the inflection between <see cref="CurveKey.TangentIn"/> and <see cref="CurveKey.TangentOut"/> by taking into account the values of both neighbors of the <see cref="CurveKey"/>.
		/// </summary>
		Smooth
	}
}
