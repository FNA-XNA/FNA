using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Test
{
	static class MainXNA
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			int pid = int.Parse(args[0]);
			AnonymousPipeClientStream pipeIn = new AnonymousPipeClientStream(PipeDirection.In, args[1]);
			AnonymousPipeClientStream pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, args[2]);
			try
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				BinaryReader reader = new BinaryReader(pipeIn, Encoding.ASCII);
				while (true)
				{
					Request request = new Request(reader);
					Global.exceptions.Clear();
					Global.results.Clear();
					string exception = "";
					MethodInfo methodInfo = assembly.GetType(request.classFullName).GetMethod(request.methodName, request.paramType);
					try
					{
						methodInfo.Invoke(null, request.args);
					}
					catch (Exception ex)
					{
						exception = ex.ToString();
					}
					byte[] bytes = Respose.ToBytes(Global.results, exception, Global.exceptions);
					pipeOut.Write(bytes, 0, bytes.Length);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Process.GetProcessById(pid).Kill();
			}
		}
	}
}
