#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	class BoundingBoxReader : ContentTypeReader<BoundingBox>
	{
		#region Protected Read Method

		protected internal override BoundingBox Read(
			ContentReader input,
			BoundingBox existingInstance
		) {
			BoundingBox result = new BoundingBox(
				input.ReadVector3(),
				input.ReadVector3()
			);
			return result;
		}

		#endregion
	}
}
