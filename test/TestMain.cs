using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Test
{
	class TestMain
	{
		static bool Failed = false;
		static Func<ValueType, object, bool> ValueTypeEquals;
		static TestMain()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("ValueTypeEquals", typeof(bool), new Type[] { typeof(ValueType), typeof(object) }, typeof(ValueType).Module);
			ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldarg_1);
			ilGenerator.Emit(OpCodes.Call, typeof(ValueType).GetMethod("Equals"));
			ilGenerator.Emit(OpCodes.Ret);
			ValueTypeEquals = (Func<ValueType, object, bool>) dynamicMethod.CreateDelegate(typeof(Func<ValueType, object, bool>));
		}

		static int Main()
		{
			AnonymousPipeServerStream pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
			AnonymousPipeServerStream pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
			using (Process process = new Process
			{
				StartInfo = new ProcessStartInfo("Test.XNA.exe", Process.GetCurrentProcess().Id + " " + pipeOut.GetClientHandleAsString() + " " + pipeIn.GetClientHandleAsString())
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			}) {
				process.OutputDataReceived += Process_OutputDataReceived;
				process.ErrorDataReceived += Process_ErrorDataReceived;
				if (!process.Start())
				{
					return -1;
				}
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				Loop(Assembly.GetExecutingAssembly(), pipeIn, pipeOut);
				return Failed ? -1 : 0;
			}
		}

		private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.Error.WriteLine(e.Data);
		}

		private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
		}

		static void Loop(Assembly assembly, AnonymousPipeServerStream pipeIn, AnonymousPipeServerStream pipeOut)
		{
			BinaryReader reader = new BinaryReader(pipeIn);
			foreach (Type type in assembly.GetExportedTypes())
			{
				foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
				{
					foreach (Attribute attribute in method.GetCustomAttributes(false))
					{
						string attributeName = attribute.GetType().Name;
						if (attributeName == "TestAttribute")
						{
							BeginCallMethod(pipeOut, method, null, false);
							object result = method.Invoke(null, null);
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							Check(method, null, result, respose.result);
						}
						else if (attributeName == "ArgumentAttribute")
						{
							object[] args = (object[]) attribute.GetType().GetField("args").GetValue(attribute);
							BeginCallMethod(pipeOut, method, args, false);
							object result = method.Invoke(null, args);
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							Check(method, args, result, respose.result);
						}
						else if (attributeName == "TestExceptionAttribute")
						{
							Assert.Exceptions.Clear();
							BeginCallMethod(pipeOut, method, null, true);
							method.Invoke(null, null);
							List<Exception> exceptions2 = Assert.Exceptions;
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							ExceptionInfo[] exceptions1 = respose.exceptions;
							for (int i = 0; i < exceptions1.Length; i++)
							{
								ExceptionInfo exception1 = exceptions1[i];
								Exception exception2 = exceptions2[i];
								if (exception1.Type != (exception2 == null ? "" : exception2.GetType().FullName))
								{
									Fail(MethodName(method) + ": Exception not match, index=" + i);
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.Error.WriteLine("exceptionXNA:");
									Console.ForegroundColor = ConsoleColor.Green;
									Console.Error.WriteLine(exception1.Message);
									Console.ForegroundColor = ConsoleColor.Yellow;
									Console.Error.WriteLine("exceptionFNA:");
									Console.ForegroundColor = ConsoleColor.Green;
									Console.Error.WriteLine(exception2 == null ? "null" : exception2.ToString());
									Console.ResetColor();
								}
							}
						}
					}
				}
			}
		}

		static void BeginCallMethod(AnonymousPipeServerStream pipeOut, MethodInfo methodInfo, object[] args, bool TestException)
		{
			Request request;
			request.className = methodInfo.DeclaringType.FullName;
			request.methodName = methodInfo.Name;
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			request.paramType = new Type[parameterInfos.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				request.paramType[i] = parameterInfos[i].ParameterType;
			}
			request.args = args;
			request.TestException = TestException;
			byte[] bytes = request.ToBytes();
			pipeOut.Write(bytes, 0, bytes.Length);
		}

		static void Check(MethodInfo methodInfo, object[] param, object result1, object result2)
		{
			if (!ReferenceEquals(result1, result2))
			{
				ValueType value1 = result1 as ValueType;
				ValueType value2 = result2 as ValueType;
				if (value1 == null || value2 == null)
				{
					Fail(MethodName(methodInfo) + ": Please return struct");
				}
				if (result1.GetType() != result2.GetType())
				{
					Fail(MethodName(methodInfo) + ": Return Type not match");
				}
				if (!ValueTypeEquals(value1, value2))
				{
					Fail(MethodName(methodInfo) + ": Result not the same" + (param == null ? "" : ", argument=[" + string.Join(",", param) + "]") + ", resultXNA=" + result1.ToString() + ", resultFNA=" + result2.ToString());
				}
			}
		}

		static void Fail(string message)
		{
			Failed = true;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(message);
			Console.ResetColor();
		}
		static string MethodName(MethodInfo methodInfo)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(methodInfo.DeclaringType.FullName);
			sb.Append("::");
			sb.Append(methodInfo.Name);
			sb.Append('(');
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				sb.Append(parameterInfo.ParameterType.Name);
				sb.Append(", ");
			}
			if (sb[sb.Length - 1] != '(')
			{
				sb.Length -= 2;
			}
			sb.Append(')');
			return sb.ToString();
		}
	}
}
