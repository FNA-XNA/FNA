namespace Microsoft.Xna.Framework.Design
{
	public static class BoundingSphereConverterTest
	{
		class CustomConverter : BoundingSphereConverter
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
			BoundingSphereConverter converter = new BoundingSphereConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
