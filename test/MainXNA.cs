using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;

namespace Test
{
	static class MainXNA
	{
		static void Main(string[] args)
		{
			int pid = int.Parse(args[0]);
			AnonymousPipeClientStream pipeIn = new AnonymousPipeClientStream(PipeDirection.In, args[1]);
			AnonymousPipeClientStream pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, args[2]);
			try
			{
				Loop(pipeIn, pipeOut);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Process.GetProcessById(pid).Kill();
			}
		}

		static void Loop(AnonymousPipeClientStream pipeIn, AnonymousPipeClientStream pipeOut)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			BinaryReader reader = new BinaryReader(pipeIn, Encoding.ASCII);
			while (true)
			{
				Request request = new Request(reader);
				if (request.TestException)
				{
					Assert.Exceptions.Clear();
				}
				Respose respose = new Respose();
				respose.exception = "";
				MethodInfo methodInfo = assembly.GetType(request.className).GetMethod(request.methodName, request.paramType);
				try
				{
					respose.result = methodInfo.Invoke(null, request.args);
				}
				catch (Exception ex)
				{
					respose.exception = ex.ToString();
				}
				if (request.TestException)
				{
					respose.result = null;
				}
				byte[] bytes = respose.ToBytes(Assert.Exceptions);
				pipeOut.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
