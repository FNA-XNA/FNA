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
	internal class DecimalReader : ContentTypeReader<decimal>
	{
		#region Internal Constructor

		internal DecimalReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override decimal Read(
			ContentReader input,
			decimal existingInstance
		) {
			return input.ReadDecimal();
		}

		#endregion
	}
}
