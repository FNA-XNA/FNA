namespace Microsoft.Xna.Framework.Graphics
{
	public static class SamplerStateTest
	{
		[Test]
		public static void Default()
		{
			SamplerState samplerState = new SamplerState();
			Assert.Result(samplerState.Tag);
			AssertSamplerState(samplerState);
			samplerState.Dispose();
			AssertSamplerState(SamplerState.AnisotropicClamp);
			AssertSamplerState(SamplerState.AnisotropicWrap);
			AssertSamplerState(SamplerState.LinearClamp);
			AssertSamplerState(SamplerState.LinearWrap);
			AssertSamplerState(SamplerState.PointClamp);
			AssertSamplerState(SamplerState.PointWrap);
		}

		static void AssertSamplerState(SamplerState samplerState)
		{
			Assert.Result(samplerState.Name);
			Assert.Result(samplerState.AddressU);
			Assert.Result(samplerState.AddressV);
			Assert.Result(samplerState.AddressW);
			Assert.Result(samplerState.Filter);
			Assert.Result(samplerState.MaxAnisotropy);
			Assert.Result(samplerState.MaxMipLevel);
			Assert.Result(samplerState.MipMapLevelOfDetailBias);
		}
	}
}
