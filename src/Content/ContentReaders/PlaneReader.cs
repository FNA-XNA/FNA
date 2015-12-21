#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class PlaneReader : ContentTypeReader<Plane>
	{
		#region Internal Constructor

		internal PlaneReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Plane Read(
			ContentReader input,
			Plane existingInstance
		) {
			existingInstance.Normal = input.ReadVector3();
			existingInstance.D = input.ReadSingle();
			return existingInstance;
		}

		#endregion
	}
}
