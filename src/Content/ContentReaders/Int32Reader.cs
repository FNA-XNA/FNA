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
	internal class Int32Reader : ContentTypeReader<int>
	{
		#region Internal Constructor

		internal Int32Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override int Read(
			ContentReader input,
			int existingInstance
		) {
			return input.ReadInt32();
		}

		#endregion
	}
}
