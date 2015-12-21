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
	internal class Int16Reader : ContentTypeReader<short>
	{
		#region Internal Constructor

		internal Int16Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override short Read(
			ContentReader input,
			short existingInstance
		) {
			return input.ReadInt16();
		}

		#endregion
	}
}
