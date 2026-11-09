namespace Microsoft.Xna.Framework.Design
{
	public static class QuaternionConverterTest
	{
		class CustomConverter : QuaternionConverter
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
			QuaternionConverter converter = new QuaternionConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
