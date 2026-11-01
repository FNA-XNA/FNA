namespace Microsoft.Xna.Framework
{
	public static class GameServiceContainerTest
	{
		[TestException]
		public static void TestException()
		{
			GameServiceContainer gameServiceContainer = new GameServiceContainer();
			gameServiceContainer.AddService(typeof(int), 0);
			Assert.Throw(() => { gameServiceContainer.AddService(null, 0); });
			Assert.Throw(() => { gameServiceContainer.AddService(typeof(int), null); });
			Assert.Throw(() => { gameServiceContainer.AddService(typeof(int), 0); });
			Assert.Throw(() => { gameServiceContainer.AddService(typeof(long), 0); });
			Assert.Throw(() => { gameServiceContainer.RemoveService(null); });
			Assert.Throw(() => { gameServiceContainer.GetService(null); });
			Assert.Throw(() => { gameServiceContainer.GetService(typeof(long)); });
		}
	}
}
