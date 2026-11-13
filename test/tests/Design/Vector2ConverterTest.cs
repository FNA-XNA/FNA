namespace Microsoft.Xna.Framework.Design
{
	public static class Vector2ConverterTest
	{
		class CustomConverter : Vector2Converter
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
			Vector2Converter converter = new Vector2Converter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
