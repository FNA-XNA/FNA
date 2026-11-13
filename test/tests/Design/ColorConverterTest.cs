namespace Microsoft.Xna.Framework.Design
{
	public static class ColorConverterTest
	{
		class CustomConverter : ColorConverter
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
			ColorConverter converter = new ColorConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
