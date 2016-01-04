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
	internal class BoundingFrustumReader : ContentTypeReader<BoundingFrustum>
	{
		#region Internal Constructor

		internal BoundingFrustumReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override BoundingFrustum Read(
			ContentReader input,
			BoundingFrustum existingInstance
		) {
			return new BoundingFrustum(input.ReadMatrix());
		}

		#endregion
	}
}
