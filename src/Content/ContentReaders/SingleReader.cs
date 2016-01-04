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
	internal class SingleReader : ContentTypeReader<float>
	{
		#region Internal Constructor

		internal SingleReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override float Read(
			ContentReader input,
			float existingInstance
		) {
			return input.ReadSingle();
		}

		#endregion
	}
}
