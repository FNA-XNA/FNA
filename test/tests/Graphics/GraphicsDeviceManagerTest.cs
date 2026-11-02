namespace Microsoft.Xna.Framework.Graphics
{
	public static class GraphicsDeviceManagerTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(GraphicsDeviceManager.DefaultBackBufferWidth);
			Assert.Result(GraphicsDeviceManager.DefaultBackBufferHeight);
		}
	}
}
