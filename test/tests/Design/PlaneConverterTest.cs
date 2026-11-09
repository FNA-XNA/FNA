namespace Microsoft.Xna.Framework.Design
{
	public static class PlaneConverterTest
	{
		class CustomConverter : PlaneConverter
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
			PlaneConverter converter = new PlaneConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
