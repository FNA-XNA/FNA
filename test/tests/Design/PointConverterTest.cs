namespace Microsoft.Xna.Framework.Design
{
	public static class PointConverterTest
	{
		class CustomConverter : PointConverter
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
			PointConverter converter = new PointConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
