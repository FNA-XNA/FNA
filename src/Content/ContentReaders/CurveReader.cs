#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class CurveReader : ContentTypeReader<Curve>
	{
		#region Protected Read Method

		protected internal override Curve Read(
			ContentReader input,
			Curve existingInstance
		) {
			Curve curve = existingInstance;
			if (curve == null)
			{
				curve = new Curve();
			}

			curve.PreLoop = (CurveLoopType) input.ReadInt32();
			curve.PostLoop = (CurveLoopType) input.ReadInt32();
			int num6 = input.ReadInt32();
			for (int i = 0; i < num6; i += 1)
			{
				float position = input.ReadSingle();
				float num4 = input.ReadSingle();
				float tangentIn = input.ReadSingle();
				float tangentOut = input.ReadSingle();
				CurveContinuity continuity = (CurveContinuity) input.ReadInt32();
				curve.Keys.Add(
					new CurveKey(
						position,
						num4,
						tangentIn,
						tangentOut,
						continuity
					)
				);
			}
			return curve;
		}

		#endregion
	}
}

