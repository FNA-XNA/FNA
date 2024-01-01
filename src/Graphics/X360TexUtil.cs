#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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

		internal static byte[] SwapDxt1(byte[] imageData, int width, int height)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				return SwapDxt1(imageStream, imageData.Length, width, height);
			}
		}

		internal static byte[] SwapDxt1(Stream imageStream, int imageLength, int width, int height)
		{
			byte[] imageData = new byte[imageLength];
			using (MemoryStream imageDataStream = new MemoryStream(imageData))
			using (BinaryWriter imageWriter = new BinaryWriter(imageDataStream))
			using (BinaryReader imageReader = new BinaryReader(imageStream))
			{
				int blockCountX = (width + 3) / 4;
				int blockCountY = (height + 3) / 4;

				for (int y = 0; y < blockCountY; y++)
				{
					for (int x = 0; x < blockCountX; x++)
					{
						SwapDxt1Block(imageReader, imageWriter);
					}
				}
			}
			return imageData;
		}

		internal static byte[] SwapDxt3(byte[] imageData, int width, int height)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				return SwapDxt3(imageStream, imageData.Length, width, height);
			}
		}

		internal static byte[] SwapDxt3(Stream imageStream, int imageLength, int width, int height)
		{
			byte[] imageData = new byte[imageLength];
			using (MemoryStream imageDataStream = new MemoryStream(imageData))
			using (BinaryWriter imageWriter = new BinaryWriter(imageDataStream))
			using (BinaryReader imageReader = new BinaryReader(imageStream))
			{
				int blockCountX = (width + 3) / 4;
				int blockCountY = (height + 3) / 4;

				for (int y = 0; y < blockCountY; y++)
				{
					for (int x = 0; x < blockCountX; x++)
					{
						SwapDxt3Block(imageReader, imageWriter);
					}
				}
			}
			return imageData;
		}

		internal static byte[] SwapDxt5(byte[] imageData, int width, int height)
		{
			using (MemoryStream imageStream = new MemoryStream(imageData))
			{
				return SwapDxt5(imageStream, imageData.Length, width, height);
			}
		}

		internal static byte[] SwapDxt5(Stream imageStream, int imageLength, int width, int height)
		{
			byte[] imageData = new byte[imageLength];
			using (MemoryStream imageDataStream = new MemoryStream(imageData))
			using (BinaryWriter imageWriter = new BinaryWriter(imageDataStream))
			using (BinaryReader imageReader = new BinaryReader(imageStream))
			{
				int blockCountX = (width + 3) / 4;
				int blockCountY = (height + 3) / 4;

				for (int y = 0; y < blockCountY; y++)
				{
					for (int x = 0; x < blockCountX; x++)
					{
						SwapDxt5Block(imageReader, imageWriter);
					}
				}
			}
			return imageData;
		}

		#endregion

		#region Private Static Methods

		public static ushort SwapEndian(ushort data)
		{
			return (ushort) (
				((ushort) ((data & 0xFF) << 8)) |
				((ushort) ((data >> 8) & 0xFF))
			);
		}

		private static void SwapDxt1Block(BinaryReader imageReader, BinaryWriter imageWriter)
		{
			// Fix the following two big-endian words to litte-endian words.
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));

			// Two words / 16 bit values instead of a 4 byte / 32 bit table.
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
		}

		private static void SwapDxt3Block(BinaryReader imageReader, BinaryWriter imageWriter)
		{
			// Alpha data. 16 4-bit values, but written to / read from as 4 16-bit values.
			// Somehow, that one test game I had worked just fine with this unchanged... -ade
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));

			SwapDxt1Block(imageReader, imageWriter);
		}

		private static void SwapDxt5Block(BinaryReader imageReader, BinaryWriter imageWriter)
		{
			// Alpha minimum and maximum. Two bytes, but handled internally as one word.
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));

			// Alpha indices. 16 3-bit values, but written to / read from as 3 16-bit values.
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));
			imageWriter.Write(SwapEndian(imageReader.ReadUInt16()));

			SwapDxt1Block(imageReader, imageWriter);
		}

		#endregion
	}
}
