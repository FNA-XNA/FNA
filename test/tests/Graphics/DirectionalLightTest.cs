namespace Microsoft.Xna.Framework.Graphics
{
	public static class DirectionalLightTest
	{
		internal static void AssertDirectionalLight(DirectionalLight directionalLight)
		{
			Assert.Result(directionalLight.DiffuseColor);
			Assert.Result(directionalLight.Direction);
			Assert.Result(directionalLight.Enabled);
			Assert.Result(directionalLight.SpecularColor);		
		}
	}
}
