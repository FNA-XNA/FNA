#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class MarshalHelper
	{
		internal static int SizeOf<T>()
		{
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
			return Marshal.SizeOf<T>();
#else
			return Marshal.SizeOf(typeof(T));
#endif
		}

		internal static string PtrToInternedStringAnsi(IntPtr ptr)
		{
			string result = Marshal.PtrToStringAnsi(ptr);
			if (result != null)
				result = string.Intern(result);
			return result;
		}
	}

	static class IconExtractor
	{
		internal static int ExtractIcon(string path)
		{
			int rsrcRVA = 0;
			int rsrcBase = 0;
			FileStream fs = File.OpenRead(path);
			using (PositionBinaryReader reader = new PositionBinaryReader(fs))
			{
				fs.Position = 0x3C;
				short peBase = reader.ReadInt16();
				fs.Position = peBase + 6;
				short NumberOfSections = reader.ReadInt16();
				fs.Position = peBase + 0x14;
				int sectionHeader = peBase + 0x18 + reader.ReadInt16();
				for (short i = 0; i < NumberOfSections; i++)
				{
					fs.Position = sectionHeader + 0x28 * i;
					if (reader.ReadASCIIString(8) == ".rsrc\0\0\0")
					{
						reader.ReadInt32();
						rsrcRVA = reader.ReadInt32();
						reader.ReadInt32();
						rsrcBase = reader.ReadInt32();
					}
				}
				if (rsrcBase == 0)
				{
					return 0;
				}
				int OffsetToData;
				fs.Position = rsrcBase;

				OffsetToData = ReadDirectory(reader, 3);
				if (OffsetToData == -1)
				{
					return 0;
				}
				fs.Position = rsrcBase + OffsetToData & 0x7FFFFFFF;

				OffsetToData = ReadDirectory(reader, 0);
				if (OffsetToData == -1)
				{
					return 0;
				}
				fs.Position = rsrcBase + OffsetToData & 0x7FFFFFFF;

				OffsetToData = ReadDirectory(reader, 0);
				if (OffsetToData == -1)
				{
					return 0;
				}
				fs.Position = rsrcBase + OffsetToData;

				return reader.ReadInt32() - rsrcRVA + rsrcBase;
			}
		}

		static int ReadDirectory(PositionBinaryReader reader, int resource)
		{
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			int NumberOfEntries = reader.ReadInt16() + reader.ReadInt16();
			for (; NumberOfEntries > 0; NumberOfEntries--)
			{
				int Name = reader.ReadInt32();
				int OffsetToData = reader.ReadInt32();
				if (resource == 0 || Name == resource)
				{
					return OffsetToData;
				}
			}
			return -1;
		}
	}

	class PositionBinaryReader : BinaryReader
	{
		internal int Position = 0;
		public PositionBinaryReader(Stream input) : base(input, Encoding.ASCII) { }

		public override short ReadInt16()
		{
			Position += 2;
			return base.ReadInt16();
		}

		public override int ReadInt32()
		{
			Position += 4;
			return base.ReadInt32();
		}

		public override ushort ReadUInt16()
		{
			Position += 2;
			return base.ReadUInt16();
		}

		public override uint ReadUInt32()
		{
			Position += 4;
			return base.ReadUInt32();
		}

		internal byte[] StrictReadBytes(int count)
		{
			byte[] bytes = PRIVATE_ReadBytes(count);
			if (bytes == null)
			{
				throw new EndOfStreamException();
			}
			return bytes;
		}

		internal string ReadASCIIString(int count)
		{
			byte[] bytes = PRIVATE_ReadBytes(count);
			return bytes == null ? null : Encoding.ASCII.GetString(bytes);
		}

		private byte[] PRIVATE_ReadBytes(int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			byte[] bytes = new byte[count];
			int pos = 0;
			while (count > 0)
			{
				int readed = BaseStream.Read(bytes, pos, count);
				if (readed == 0)
				{
					return null;
				}
				pos += readed;
				count -= readed;
			}
			Position += bytes.Length;
			return bytes;
		}
	}
}
