using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

static class Assert
{
#if !XNA
	internal struct Code
	{
		internal string path;
		internal int line;
	}
	internal static List<Code> codeResult = new List<Code>();
	internal static List<Code> codeException = new List<Code>();
#endif
	internal static List<object> results = new List<object>();
	internal static List<Exception> exceptions = new List<Exception>();

	internal static void Result(object result, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
	{
		results.Add(result);
#if !XNA
		Code code;
		code.path = path;
		code.line = line;
		codeResult.Add(code);
#endif
	}

	internal static void Throw(Action action, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
	{
		Exception exception = null;
		try
		{
			action();
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		exceptions.Add(exception);
#if !XNA
		Code code;
		code.path = path;
		code.line = line;
		codeException.Add(code);
#endif
	}
}
