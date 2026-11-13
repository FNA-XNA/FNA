namespace Microsoft.Xna.Framework.Design
{
	public static class MatrixConverterTest
	{
		class CustomConverter : MatrixConverter
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
			MatrixConverter converter = new MatrixConverter();
			Assert.Throw(() => { converter.CreateInstance(null, null); });
		}
	}
}
