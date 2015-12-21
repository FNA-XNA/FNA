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
	internal class RayReader : ContentTypeReader<Ray>
	{
		#region Internal Constructor

		internal RayReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Ray Read(
			ContentReader input,
			Ray existingInstance
		) {
			Vector3 position = input.ReadVector3();
			Vector3 direction = input.ReadVector3();
			return new Ray(position, direction);
		}

		#endregion
	}
}
