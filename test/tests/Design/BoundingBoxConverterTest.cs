namespace Microsoft.Xna.Framework.Design
{
	public static class BoundingBoxConverterTest
	{
		class CustomConverter : BoundingBoxConverter
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
			BoundingBoxConverter converter = new BoundingBoxConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
