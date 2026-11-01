using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Test
{
	struct Request
	{
		public string classFullName;
		public string methodName;
		public Type[] paramType;
		public object[] args;

		internal Request(BinaryReader reader)
		{
			classFullName = reader.ReadString();
			methodName = reader.ReadString();
			paramType = new Type[reader.ReadByte()];
			for (int i = 0; i < paramType.Length; i++)
			{
				paramType[i] = Type.GetType(reader.ReadString());
			}
			args = new object[reader.ReadByte()];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = RpcFormatter.ReadObject(reader);
			}
		}

		internal static byte[] ToBytes(MethodInfo methodInfo, object[] args)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(memoryStream))
			{
				writer.Write(methodInfo.DeclaringType.FullName);
				writer.Write(methodInfo.Name);
				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				writer.Write((byte) parameterInfos.Length);
				for (int i = 0; i < parameterInfos.Length; i++)
				{
					writer.Write(parameterInfos[i].ParameterType.FullName);
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
						RpcFormatter.WriteObject(writer, args[i]);
					}
				}
				return memoryStream.ToArray();
			}
		}
	}
	struct Respose
	{
		public object[] results;
		public string exception;
		public Exception[] exceptions;
		internal Respose(BinaryReader reader)
		{
			results = new object[reader.ReadByte()];
			for (int i = 0; i < results.Length; i++)
			{
				results[i] = RpcFormatter.ReadObject(reader);
			}
			exception = reader.ReadString();
			exceptions = new Exception[reader.ReadByte()];
			for (int i = 0; i < exceptions.Length; i++)
			{
				exceptions[i] = (Exception) RpcFormatter.ReadObject(reader);
			}
		}

		internal static byte[] ToBytes(List<object> results, string exception, List<Exception> exceptions)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(memoryStream))
			{
				writer.Write((byte) results.Count);
				foreach (object result in results)
				{
					RpcFormatter.WriteObject(writer, result);
				}
				writer.Write(exception);
				writer.Write((byte) exceptions.Count);
				for (int i = 0; i < exceptions.Count; i++)
				{
					RpcFormatter.WriteObject(writer, exceptions[i]);
				}
				return memoryStream.ToArray();
			}
		}
	}

	static class RpcFormatter
	{
		private static readonly byte[] tmpBuffer = new byte[64];
#pragma warning disable SYSLIB0050
		private static readonly FormatterConverter formatter = new FormatterConverter();
#pragma warning restore SYSLIB0050

		internal static object ReadObject(BinaryReader reader)
		{
			TypeCode typeCode = (TypeCode) reader.ReadByte();
			switch (typeCode)
			{
				case TypeCode.Empty:
					return null;
				case TypeCode.Object:
#pragma warning disable SYSLIB0050
					SerializationInfo entries = new SerializationInfo(typeof(ISerializable), formatter);
					int count = reader.ReadByte();
					for (int i = 0; i < count; i++)
					{
						entries.AddValue(reader.ReadString(), ReadObject(reader));
					}
					return Type.GetType(entries.GetString("ClassName"))
						.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null)
						.Invoke(new object[] { entries, new StreamingContext(StreamingContextStates.CrossProcess) });
#pragma warning restore SYSLIB0050
				case TypeCode.DBNull:
					Type type;
					string typeString = reader.ReadString();
					if (typeString.StartsWith("Microsoft.Xna.Framework."))
					{
						type = typeof(Vector4).Assembly.GetType(typeString);
					}
					else
					{
						type = typeof(object).Assembly.GetType(typeString);
					}
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
			if (typeCode == TypeCode.Object && obj is ValueType)
			{
				typeCode = TypeCode.DBNull;
			}
			writer.Write((byte) typeCode);
			switch (typeCode)
			{
				case TypeCode.Object:
#pragma warning disable SYSLIB0050
					SerializationInfo entries = new SerializationInfo(typeof(ISerializable), formatter);
					((ISerializable) obj).GetObjectData(entries, new StreamingContext(StreamingContextStates.CrossProcess));
#pragma warning restore SYSLIB0050
					writer.Write((byte) entries.MemberCount);
					foreach (SerializationEntry entry in entries)
					{
						writer.Write(entry.Name);
						WriteObject(writer, entry.Value);
					}
					break;
				case TypeCode.DBNull:
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
				case TypeCode.Boolean:
					writer.Write((bool) obj);
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
