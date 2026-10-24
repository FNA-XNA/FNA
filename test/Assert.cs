using System;
using System.Collections.Generic;

static class Assert
{
	internal static List<Exception> Exceptions = new List<Exception>();
	internal static void Throw(Action action)
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
		Exceptions.Add(exception);
	}
}
