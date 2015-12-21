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
	class MatrixReader : ContentTypeReader<Matrix>
	{
		#region Protected Read Method

		protected internal override Matrix Read(
			ContentReader input,
			Matrix existingInstance
		) {
			// 4x4 matrix
			return new Matrix(
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle()
			);
		}

		#endregion
	}
}
