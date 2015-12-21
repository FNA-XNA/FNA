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
	internal class BoundingSphereReader : ContentTypeReader<BoundingSphere>
	{
		#region Internal Constructor

		internal BoundingSphereReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override BoundingSphere Read(
			ContentReader input,
			BoundingSphere existingInstance
		) {
			Vector3 center = input.ReadVector3();
			float radius = input.ReadSingle();
			return new BoundingSphere(center, radius);
		}

		#endregion
	}
}
