namespace Microsoft.Xna.Framework.Graphics
{
	public static class OcclusionQueryTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			OcclusionQuery occlusionQuery = new OcclusionQuery(gd);
			Assert.Result(occlusionQuery.Name);
			Assert.Result(occlusionQuery.Tag);
			occlusionQuery.Dispose();
		}
	}
}
