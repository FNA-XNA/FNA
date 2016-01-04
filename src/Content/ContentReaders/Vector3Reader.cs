#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
	internal class Vector3Reader : ContentTypeReader<Vector3>
	{
		#region Internal Constructor

		internal Vector3Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Vector3 Read(
			ContentReader input,
			Vector3 existingInstance
		) {
			return input.ReadVector3();
		}

		#endregion
	}
}
