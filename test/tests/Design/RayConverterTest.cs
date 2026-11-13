namespace Microsoft.Xna.Framework.Design
{
	public static class RayConverterTest
	{
		class CustomConverter : RayConverter
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
			RayConverter converter = new RayConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
