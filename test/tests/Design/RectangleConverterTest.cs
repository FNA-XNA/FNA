namespace Microsoft.Xna.Framework.Design
{
	public static class RectangleConverterTest
	{
		class CustomConverter : RectangleConverter
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
			RectangleConverter converter = new RectangleConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
