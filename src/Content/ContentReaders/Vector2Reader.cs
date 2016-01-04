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
	internal class Vector2Reader : ContentTypeReader<Vector2>
	{
		#region Internal Constructor

		internal Vector2Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Vector2 Read(
			ContentReader input,
			Vector2 existingInstance
		) {
			return input.ReadVector2();
		}

		#endregion
	}
}
