namespace Microsoft.Xna.Framework
{
	public static class DrawableGameComponentTest
	{
		[Test]
		public static void Default()
		{
			DrawableGameComponent gameComponent = new DrawableGameComponent(null);
			Assert.Result(gameComponent.Enabled);
			Assert.Result(gameComponent.UpdateOrder);
			Assert.Result(gameComponent.DrawOrder);
			Assert.Result(gameComponent.Visible);
		}

		[TestException]
		public static void TestException()
		{
			DrawableGameComponent gameComponent = new DrawableGameComponent(null);
			Assert.Throw(() => { _ = gameComponent.GraphicsDevice; });
		}
	}
}
