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
	internal class QuaternionReader : ContentTypeReader<Quaternion>
	{
		#region Internal Constructor

		internal QuaternionReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Quaternion Read(
			ContentReader input,
			Quaternion existingInstance
		) {
			return input.ReadQuaternion();
		}

		#endregion
	}
}
