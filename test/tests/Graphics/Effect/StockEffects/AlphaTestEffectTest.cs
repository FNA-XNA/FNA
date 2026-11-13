namespace Microsoft.Xna.Framework.Graphics
{
	public static class AlphaTestEffectTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			AlphaTestEffect effect = new AlphaTestEffect(gd);
			Assert.Result(effect.Alpha);
			Assert.Result(effect.AlphaFunction);
			Assert.Result(effect.DiffuseColor);
			Assert.Result(effect.FogColor);
			Assert.Result(effect.FogEnabled);
			Assert.Result(effect.FogEnd);
			Assert.Result(effect.FogStart);
			Assert.Result(effect.Projection);
			Assert.Result(effect.ReferenceAlpha);
			Assert.Result(effect.VertexColorEnabled);
			Assert.Result(effect.View);
			Assert.Result(effect.World);
		}
	}
}
