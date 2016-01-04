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
	internal class RectangleReader : ContentTypeReader<Rectangle>
	{
		#region Internal Constructor

		internal RectangleReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Rectangle Read(
			ContentReader input,
			Rectangle existingInstance
		) {
			int left = input.ReadInt32();
			int top = input.ReadInt32();
			int width = input.ReadInt32();
			int height = input.ReadInt32();
			return new Rectangle(left, top, width, height);
		}

		#endregion
	}
}
