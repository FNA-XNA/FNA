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
	internal class Int64Reader : ContentTypeReader<long>
	{
		#region Internal Constructor

		internal Int64Reader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override long Read(
			ContentReader input,
			long existingInstance
		) {
			return input.ReadInt64();
		}

		#endregion
	}
}
