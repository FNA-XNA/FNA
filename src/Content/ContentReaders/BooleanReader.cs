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
	internal class BooleanReader : ContentTypeReader<bool>
	{
		#region Internal Constructor

		internal BooleanReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override bool Read(
			ContentReader input,
			bool existingInstance
		) {
			return input.ReadBoolean();
		}

		#endregion
	}
}
