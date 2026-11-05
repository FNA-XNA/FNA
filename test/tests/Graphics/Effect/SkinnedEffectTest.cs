namespace Microsoft.Xna.Framework.Graphics
{
	public static class SkinnedEffectTest
	{
		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			SkinnedEffect effect = new SkinnedEffect(gd);
			Assert.Result(SkinnedEffect.MaxBones);
			Assert.Result(effect.Alpha);
			Assert.Result(effect.AmbientLightColor);
			Assert.Result(effect.DiffuseColor);
			Assert.Result(effect.DirectionalLight0);
			Assert.Result(effect.DirectionalLight1);
			Assert.Result(effect.DirectionalLight2);
			Assert.Result(effect.EmissiveColor);
			Assert.Result(effect.FogColor);
			Assert.Result(effect.FogEnabled);
			Assert.Result(effect.FogEnd);
			Assert.Result(effect.FogStart);
			Assert.Result(effect.PreferPerPixelLighting);
			Assert.Result(effect.Projection);
			Assert.Result(effect.SpecularColor);
			Assert.Result(effect.SpecularPower);
			Assert.Result(effect.View);
			Assert.Result(effect.WeightsPerVertex);
			Assert.Result(effect.World);
		}
	}
}
