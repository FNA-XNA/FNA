using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

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
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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
							Global.results.Clear();
							Global.codeResult.Clear();
							BeginCallMethod(pipeOut, method, null);
							method.Invoke(null, null);
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							Check(method, null, respose.results, Global.results);
						}
						else if (attributeName == "ArgumentAttribute")
						{
							Global.results.Clear();
							Global.codeResult.Clear();
							object[] args = (object[]) attribute.GetType().GetField("args").GetValue(attribute);
							BeginCallMethod(pipeOut, method, args);
							method.Invoke(null, args);
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							Check(method, args, respose.results, Global.results);
						}
						else if (attributeName == "TestExceptionAttribute")
						{
							Global.exceptions.Clear();
							Global.codeException.Clear();
							BeginCallMethod(pipeOut, method, null);
							method.Invoke(null, null);
							Respose respose = new Respose(reader);
							if (respose.exception.Length != 0)
							{
								Console.Error.WriteLine(respose.exception);
								return;
							}
							Exception[] exceptions1 = respose.exceptions;
							List<Exception> exceptions2 = Global.exceptions;
							for (int i = 0; i < exceptions1.Length; i++)
							{
								Exception exception1 = exceptions1[i];
								Exception exception2 = exceptions2[i];
								if (exception1 == exception2)
								{
									continue;
								}
								ArgumentException argumentException1 = exception1 as ArgumentException;
								ArgumentException argumentException2 = exception2 as ArgumentException;
								if (
									exception1 == null || exception2 == null || exception1.GetType() != exception2.GetType() ||
									argumentException1 == null || string.IsNullOrEmpty(argumentException1.ParamName) || argumentException1.ParamName != argumentException2.ParamName
								) {
									Fail("Throw exception type is not the same");
									Message(ConsoleColor.Cyan, Code(method, Global.codeException[i]));
									Message(ConsoleColor.Yellow, "exceptionXNA:");
									Message(ConsoleColor.Green, exception1 == null ? "null" : exception1.ToString());
									Message(ConsoleColor.Yellow, "exceptionFNA:");
									Message(ConsoleColor.Green, exception2 == null ? "null" : exception2.ToString());
								}
							}
						}
					}
				}
			}
		}

		static void BeginCallMethod(AnonymousPipeServerStream pipeOut, MethodInfo methodInfo, object[] args)
		{
			byte[] bytes = Request.ToBytes(methodInfo, args);
			pipeOut.Write(bytes, 0, bytes.Length);
		}

		static void Check(MethodInfo methodInfo, object[] param, object[] results1, List<object> results2)
		{
			for (int i = 0; i < results1.Length; i++)
			{
				object result1 = results1[i];
				object result2 = results2[i];
				if (!ReferenceEquals(result1, result2))
				{
					if (result1.GetType() != result2.GetType())
					{
						Fail("Result type is not the same");
						Message(ConsoleColor.Cyan, Code(methodInfo, Global.codeResult[i]));
						continue;
					}
					ValueType value1 = result1 as ValueType;
					ValueType value2 = result2 as ValueType;
					if (value1 != null && value2 != null)
					{
						if (!ValueTypeEquals(value1, value2))
						{
							Fail("Result is not the same" + (param == null ? "" : ", argument=[" + string.Join(",", param) + "]") + ", resultXNA=" + result1.ToString() + ", resultFNA=" + result2.ToString());
							Message(ConsoleColor.Cyan, Code(methodInfo, Global.codeResult[i]));
						}
						continue;
					}
					string s1 = result1 as string;
					string s2 = result2 as string;
					if (s1 != null && s2 != null)
					{
						if (s1 != s2)
						{
							Fail("Result is not the same" + (param == null ? "" : ", argument=[" + string.Join(",", param) + "]") + ", resultXNA=" + result1.ToString() + ", resultFNA=" + result2.ToString());
							Message(ConsoleColor.Cyan, Code(methodInfo, Global.codeResult[i]));
						}
						continue;
					}
					throw new NotImplementedException(result1.GetType().ToString());
				}
			}
		}

		static void Fail(string message)
		{
			Failed = true;
			Message(ConsoleColor.Red, message);
		}

		static void Message(ConsoleColor color, string message)
		{
			Console.ForegroundColor = color;
			Console.Error.WriteLine(message);
			Console.ResetColor();
		}

		static string Code(MethodInfo methodInfo, Global.Code code)
		{
			StringBuilder sb = new StringBuilder(20);
			sb.Append("   at ");
			sb.Append(methodInfo.DeclaringType.FullName);
			sb.Append('.');
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

			sb.Append(" in ");
			sb.Append(code.path);
			sb.Append(":line ");
			sb.Append(code.line);
			return sb.ToString();
		}
	}
}
