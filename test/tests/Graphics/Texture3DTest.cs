namespace Microsoft.Xna.Framework.Graphics
{
	public static class Texture3DTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Texture3D texture = new Texture3D(gd, 1, 1, 1, false, SurfaceFormat.Color);
			Assert.Result(texture.Name);
			Assert.Result(texture.Tag);
			texture.Dispose();
		}
	}
}
