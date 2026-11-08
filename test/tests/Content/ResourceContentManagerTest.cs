namespace Microsoft.Xna.Framework.Content
{
	public static class ResourceContentManagerTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new ResourceContentManager(null, null); });
			Assert.Throw(() => { new ResourceContentManager(new GameServiceContainer(), null); });
		}
	}
}
