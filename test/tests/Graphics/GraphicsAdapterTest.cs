namespace Microsoft.Xna.Framework.Graphics
{
	public static class GraphicsAdapterTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(GraphicsAdapter.UseNullDevice);
			Assert.Result(GraphicsAdapter.UseReferenceDevice);
		}
	}
}
