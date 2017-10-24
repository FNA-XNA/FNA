#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2017 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class X360TexUtil
	{
		#region Internal Static Methods
		internal static byte[] SwapColor(byte[] imageData)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				return SwapColor(imageStream, imageData.Length);
			}
		}

		internal static byte[] SwapColor(Stream imageStream, int imageLength)
		{
			byte[] imageData = new byte[imageLength];

			using (BinaryReader imageReader = new BinaryReader(imageStream))
			{
				for (int i = 0; i < imageLength; i += sizeof(uint))
				{
					uint data = imageReader.ReadUInt32();
					imageData[i + 0] = (byte) ((data >> 24) & 0xFF);
					imageData[i + 1] = (byte) ((data >> 16) & 0xFF);
					imageData[i + 2] = (byte) ((data >> 8)  & 0xFF);
					imageData[i + 3] = (byte) ((data >> 0)  & 0xFF);
				}
			}

			return imageData;
		}

		internal static byte[] SwapDxt3(byte[] imageData)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				return SwapDxt3(imageStream, imageData.Length);
			}
		}

		internal static byte[] SwapDxt3(Stream imageStream, int imageLength)
		{
			byte[] imageData = new byte[imageLength];

			using (BinaryReader imageReader = new BinaryReader(imageStream))
			{
				for (int i = 0; i < imageLength; i += sizeof(ushort))
				{
					ushort data = imageReader.ReadUInt16();
					imageData[i + 0] = (byte) ((data >> 8) & 0xFF);
					imageData[i + 1] = (byte) ((data >> 0) & 0xFF);
				}
			}

			return imageData;
		}
		#endregion

	}
}
