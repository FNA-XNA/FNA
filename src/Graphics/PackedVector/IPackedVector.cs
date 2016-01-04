#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.packedvector.ipackedvector.aspx
	public interface IPackedVector
	{
		void PackFromVector4(Vector4 vector);

		Vector4 ToVector4();
	}
	
	// PackedVector Generic interface
	// http://msdn.microsoft.com/en-us/library/bb197661.aspx
	public interface IPackedVector<TPacked> : IPackedVector
	{
		TPacked PackedValue
		{
			get;
			set;
		}
	}
}
