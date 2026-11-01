namespace Microsoft.Xna.Framework
{
	public static class GameComponentTest
	{
		[Test]
		public static void Default()
		{
			GameComponent gameComponent = new GameComponent(null);
			Assert.Result(gameComponent.Enabled);
			Assert.Result(gameComponent.UpdateOrder);
		}
	}
}
