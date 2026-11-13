namespace Microsoft.Xna.Framework.Design
{
	public static class Vector4ConverterTest
	{
		class CustomConverter : Vector4Converter
		{
			internal bool SupportStringConvert { get { return supportStringConvert; } }
		}

		[Test]
		public static void Default()
		{
			CustomConverter converter = new CustomConverter();
			Assert.Result(converter.SupportStringConvert);
		}

		[TestException]
		public static void TestException()
		{
			Vector4Converter converter = new Vector4Converter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
