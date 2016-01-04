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
#endregion License

namespace Microsoft.Xna.Framework.Content
{
	internal class CharReader : ContentTypeReader<char>
	{
		#region Internal Constructor

		internal CharReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override char Read(
			ContentReader input,
			char existingInstance
		) {
			return input.ReadChar();
		}

		#endregion
	}
}
