namespace Microsoft.Xna.Framework.Graphics
{
	public static class BasicEffectTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			BasicEffect effect = new BasicEffect(gd);
			Assert.Result(effect.Alpha);
			Assert.Result(effect.AmbientLightColor);
			Assert.Result(effect.DiffuseColor);
			DirectionalLightTest.AssertDirectionalLight(effect.DirectionalLight0);
			DirectionalLightTest.AssertDirectionalLight(effect.DirectionalLight1);
			DirectionalLightTest.AssertDirectionalLight(effect.DirectionalLight2);
			Assert.Result(effect.EmissiveColor);
			Assert.Result(effect.FogColor);
			Assert.Result(effect.FogEnabled);
			Assert.Result(effect.FogEnd);
			Assert.Result(effect.FogStart);
			Assert.Result(effect.LightingEnabled);
			Assert.Result(effect.PreferPerPixelLighting);
			Assert.Result(effect.Projection);
			Assert.Result(effect.SpecularColor);
			Assert.Result(effect.SpecularPower);
			Assert.Result(effect.TextureEnabled);
			Assert.Result(effect.VertexColorEnabled);
			Assert.Result(effect.View);
			Assert.Result(effect.World);
		}
	}
}
