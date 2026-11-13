namespace Microsoft.Xna.Framework.Design
{
	public static class MathTypeConverterTest
	{
		class CustomConverter : MathTypeConverter
		{
			internal bool SupportStringConvert { get {  return supportStringConvert; } }
		}

		[Test]
		public static void Default()
		{
			CustomConverter converter = new CustomConverter();
			Assert.Result(converter.SupportStringConvert);
			Assert.Result(converter.GetCreateInstanceSupported());
		}
	}
}
