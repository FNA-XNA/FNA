namespace Microsoft.Xna.Framework.Graphics
{
	public static class RenderTargetCubeTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new RenderTargetCube(null, 1, false, SurfaceFormat.Color, DepthFormat.None).Dispose(); });
		}
	}
}
