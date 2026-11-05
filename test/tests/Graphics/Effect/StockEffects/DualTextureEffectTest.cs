namespace Microsoft.Xna.Framework.Graphics
{
	public static class DualTextureEffectTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			DualTextureEffect effect = new DualTextureEffect(gd);
			Assert.Result(effect.Alpha);
			Assert.Result(effect.DiffuseColor);
			Assert.Result(effect.FogColor);
			Assert.Result(effect.FogEnabled);
			Assert.Result(effect.FogEnd);
			Assert.Result(effect.FogStart);
			Assert.Result(effect.Projection);
			Assert.Result(effect.VertexColorEnabled);
			Assert.Result(effect.View);
			Assert.Result(effect.World);
		}
	}
}
