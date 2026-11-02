namespace Microsoft.Xna.Framework.Graphics
{
	public static class ViewportTest
	{
		[Test]
		public static void Default()
		{
			Viewport viewport = new Viewport(0, 0, 1, 0);
			Assert.Result(viewport.MinDepth);
			Assert.Result(viewport.MaxDepth);
			Assert.Result(viewport.AspectRatio);
		}
	}
}
