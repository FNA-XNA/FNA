namespace Microsoft.Xna.Framework.Graphics
{
	public static class PresentationParametersTest
	{
		[Test]
		public static void Default()
		{
			PresentationParameters pp = new PresentationParameters();
			Assert.Result(pp.BackBufferFormat);
			Assert.Result(pp.BackBufferHeight);
			Assert.Result(pp.BackBufferWidth);
			Assert.Result(pp.DepthStencilFormat);
			Assert.Result(pp.DeviceWindowHandle);
			Assert.Result(pp.DisplayOrientation);
			Assert.Result(pp.IsFullScreen);
			Assert.Result(pp.MultiSampleCount);
			Assert.Result(pp.PresentationInterval);
			Assert.Result(pp.RenderTargetUsage);
		}
	}
}
