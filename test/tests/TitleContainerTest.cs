namespace Microsoft.Xna.Framework
{
	public static class TitleContainerTest
	{
		[TestException]
		public static void TestException()
		{
			Assert.Throw(() => { TitleContainer.OpenStream(null); });
			Assert.Throw(() => { TitleContainer.OpenStream(""); });
			Assert.Throw(() => { TitleContainer.OpenStream(":"); });
			Assert.Throw(() => { TitleContainer.OpenStream("*"); });
			Assert.Throw(() => { TitleContainer.OpenStream("?"); });
			Assert.Throw(() => { TitleContainer.OpenStream("\""); });
			Assert.Throw(() => { TitleContainer.OpenStream("<"); });
			Assert.Throw(() => { TitleContainer.OpenStream(">"); });
			Assert.Throw(() => { TitleContainer.OpenStream("|"); });
		}
	}
}
