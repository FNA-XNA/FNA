namespace Microsoft.Xna.Framework.Design
{
	public static class Vector3ConverterTest
	{
		class CustomConverter : Vector3Converter
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
			Vector3Converter converter = new Vector3Converter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
