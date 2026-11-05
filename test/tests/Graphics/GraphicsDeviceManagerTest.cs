namespace Microsoft.Xna.Framework.Graphics
{
	public static class GraphicsDeviceManagerTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(GraphicsDeviceManager.DefaultBackBufferWidth);
			Assert.Result(GraphicsDeviceManager.DefaultBackBufferHeight);
			Assert.Result(GameTest.gdm.GraphicsProfile);
			Assert.Result(GameTest.gdm.IsFullScreen);
			Assert.Result(GameTest.gdm.PreferMultiSampling);
			Assert.Result(GameTest.gdm.PreferredBackBufferFormat);
			Assert.Result(GameTest.gdm.PreferredBackBufferHeight);
			Assert.Result(GameTest.gdm.PreferredBackBufferWidth);
			Assert.Result(GameTest.gdm.PreferredDepthStencilFormat);
			Assert.Result(GameTest.gdm.SupportedOrientations);
			Assert.Result(GameTest.gdm.SynchronizeWithVerticalRetrace);
		}
	}
}
