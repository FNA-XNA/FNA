namespace Microsoft.Xna.Framework.Graphics
{
	public static class RenderTargetBindingTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new RenderTargetBinding(null); });
			Assert.Throw(() => { new RenderTargetBinding(null, 0); });
			Assert.Throw(() => { _ = (RenderTargetBinding) null; });
		}

		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			RenderTarget2D renderTarget2D = new RenderTarget2D(gd, 1, 1);
			Assert.Result(new RenderTargetBinding(renderTarget2D).CubeMapFace);
			Assert.Result(((RenderTargetBinding) renderTarget2D).CubeMapFace);
			renderTarget2D.Dispose();
		}
	}
}
