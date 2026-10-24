using System;

class TestAttribute : Attribute { }

class TestExceptionAttribute : Attribute { }

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
class ArgumentAttribute : Attribute
{
	public object[] args;
	internal ArgumentAttribute(params object[] args)
	{
		this.args = args;
	}
}
