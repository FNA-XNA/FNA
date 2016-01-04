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
	internal class PointReader : ContentTypeReader<Point>
	{
		#region Internal Constructor

		internal PointReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override Point Read(
			ContentReader input,
			Point existingInstance
		) {
			int X = input.ReadInt32();
			int Y = input.ReadInt32();
			return new Point(X, Y);
		}

		#endregion
	}
}
