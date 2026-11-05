namespace Microsoft.Xna.Framework.Graphics
{
	public static class EnvironmentMapEffectTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			EnvironmentMapEffect effect = new EnvironmentMapEffect(gd);
			Assert.Result(effect.Alpha);
			Assert.Result(effect.AmbientLightColor);
			Assert.Result(effect.DiffuseColor);
			Assert.Result(effect.DirectionalLight0);
			Assert.Result(effect.DirectionalLight1);
			Assert.Result(effect.DirectionalLight2);
			Assert.Result(effect.EmissiveColor);
			Assert.Result(effect.EnvironmentMapAmount);
			Assert.Result(effect.EnvironmentMapSpecular);
			Assert.Result(effect.FogColor);
			Assert.Result(effect.FogEnabled);
			Assert.Result(effect.FogEnd);
			Assert.Result(effect.FogStart);
			Assert.Result(effect.FresnelFactor);
			Assert.Result(effect.Projection);
			Assert.Result(effect.View);
			Assert.Result(effect.World);
		}
	}
}
