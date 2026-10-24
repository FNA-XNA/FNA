using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Test
{
	struct ExceptionInfo
	{
		public string Type;
		public string Message;
	}

	struct Request
	{
		public string className;
		public string methodName;
		public Type[] paramType;
		public object[] args;
		public bool TestException;

		internal Request(BinaryReader reader)
		{
			className = reader.ReadString();
			methodName = reader.ReadString();
			paramType = new Type[reader.ReadByte()];
			for (int i = 0; i < paramType.Length; i++)
			{
				paramType[i] = Type.GetType(reader.ReadString());
			}
			args = new object[reader.ReadByte()];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = Rpc.ReadObject(reader);
			}
			TestException = reader.ReadBoolean();
		}

		internal byte[] ToBytes()
		{
			MemoryStream memoryStream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(memoryStream))
			{
				writer.Write(className);
				writer.Write(methodName);
				writer.Write((byte) paramType.Length);
				for (int i = 0; i < paramType.Length; i++)
				{
					writer.Write(paramType[i].FullName);
				}
				if (args == null)
				{
					writer.Write((byte) 0);
				}
				else
				{
					writer.Write((byte) args.Length);
					for (int i = 0; i < args.Length; i++)
					{
						Rpc.WriteObject(writer, args[i]);
					}
				}
				writer.Write(TestException);
				return memoryStream.ToArray();
			}
		}
	}
	struct Respose
	{
		public object result;
		public string exception;
		public ExceptionInfo[] exceptions;
		internal Respose(BinaryReader reader)
		{
			result = Rpc.ReadObject(reader);
			exception = reader.ReadString();
			exceptions = new ExceptionInfo[reader.ReadByte()];
			for (int i = 0; i < exceptions.Length; i++)
			{
				exceptions[i].Type = reader.ReadString();
				exceptions[i].Message = reader.ReadString();
			}
		}

		internal byte[] ToBytes(List<Exception> exceptions)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(memoryStream))
			{
				Rpc.WriteObject(writer, result);
				writer.Write(exception);
				if (exceptions == null)
				{
					writer.Write((byte) 0);
				}
				else
				{
					writer.Write((byte) exceptions.Count);
					for (int i = 0; i < exceptions.Count; i++)
					{
						Exception ex = exceptions[i];
						writer.Write(ex.GetType().FullName);
						writer.Write(ex.ToString());
					}
				}
				return memoryStream.ToArray();
			}
		}
	}

	static class Rpc
	{
		private static readonly byte[] tmpBuffer = new byte[16];

		internal static object ReadObject(BinaryReader reader)
		{
			TypeCode typeCode = (TypeCode) reader.ReadByte();
			switch (typeCode)
			{
				case TypeCode.Empty:
					return null;
				case TypeCode.Object:
					string typeString = reader.ReadString();
					Type type = typeof(Vector4).Assembly.GetType(typeString);
					reader.Read(tmpBuffer, 0, Marshal.SizeOf(type));
					unsafe
					{
						fixed (void* p = tmpBuffer)
						{
							return Marshal.PtrToStructure((IntPtr) p, type);
						}
					}
				case TypeCode.Boolean:
					return reader.ReadBoolean();
				case TypeCode.Char:
					return reader.ReadChar();
				case TypeCode.SByte:
					return reader.ReadSByte();
				case TypeCode.Byte:
					return reader.ReadByte();
				case TypeCode.Int16:
					return reader.ReadInt16();
				case TypeCode.UInt16:
					return reader.ReadUInt16();
				case TypeCode.Int32:
					return reader.ReadInt32();
				case TypeCode.UInt32:
					return reader.ReadUInt32();
				case TypeCode.Int64:
					return reader.ReadInt64();
				case TypeCode.UInt64:
					return reader.ReadUInt64();
				case TypeCode.Single:
					return reader.ReadSingle();
				case TypeCode.Double:
					return reader.ReadDouble();
				case TypeCode.String:
					return reader.ReadString();
				default:
					throw new NotSupportedException("TypeCode");
			}
		}

		internal static void WriteObject(BinaryWriter writer, object obj)
		{
			if (obj == null)
			{
				writer.Write((byte) TypeCode.Empty);
				return;
			}
			Type type = obj.GetType();
			TypeCode typeCode = Type.GetTypeCode(type);
			writer.Write((byte) typeCode);
			switch (typeCode)
			{
				case TypeCode.Boolean:
					writer.Write((bool) obj);
					break;
				case TypeCode.Object:
					int size = Marshal.SizeOf(type);
					if (size > tmpBuffer.Length)
					{
						throw new NotSupportedException("struct size too large");
					}
					unsafe
					{
						fixed (void* p = tmpBuffer)
						{
							Marshal.StructureToPtr(obj, (IntPtr) p, false);
						}
					}
					writer.Write(type.FullName);
					writer.Write(tmpBuffer, 0, size);
					break;
				case TypeCode.Char:
					writer.Write((char) obj);
					break;
				case TypeCode.SByte:
					writer.Write((sbyte) obj);
					break;
				case TypeCode.Byte:
					writer.Write((byte) obj);
					break;
				case TypeCode.Int16:
					writer.Write((short) obj);
					break;
				case TypeCode.UInt16:
					writer.Write((ushort) obj);
					break;
				case TypeCode.Int32:
					writer.Write((int) obj);
					break;
				case TypeCode.UInt32:
					writer.Write((uint) obj);
					break;
				case TypeCode.Int64:
					writer.Write((long) obj);
					break;
				case TypeCode.UInt64:
					writer.Write((ulong) obj);
					break;
				case TypeCode.Single:
					writer.Write((float) obj);
					break;
				case TypeCode.Double:
					writer.Write((double) obj);
					break;
				case TypeCode.String:
					writer.Write((string) obj);
					break;
				default:
					throw new NotSupportedException("TypeCode");
			}
		}
	}
}
