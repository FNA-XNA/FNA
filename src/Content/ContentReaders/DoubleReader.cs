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
	internal class DoubleReader : ContentTypeReader<double>
	{
		#region Internal Constructor

		internal DoubleReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override double Read(
			ContentReader input,
			double existingInstance
		) {
			return input.ReadDouble();
		}

		#endregion
	}
}

