namespace Microsoft.Xna.Framework.Graphics
{
	public static class BlendStateTest
	{
		[Test]
		public static void Default()
		{
			BlendState blendState = new BlendState();
			Assert.Result(blendState.Tag);
			AssertBlendState(blendState);
			blendState.Dispose();
			AssertBlendState(BlendState.Additive);
			AssertBlendState(BlendState.AlphaBlend);
			AssertBlendState(BlendState.NonPremultiplied);
			AssertBlendState(BlendState.Opaque);
		}

		static void AssertBlendState(BlendState blendState)
		{
			Assert.Result(blendState.Name);
			Assert.Result(blendState.AlphaBlendFunction);
			Assert.Result(blendState.AlphaDestinationBlend);
			Assert.Result(blendState.AlphaSourceBlend);
			Assert.Result(blendState.BlendFactor);
			Assert.Result(blendState.ColorBlendFunction);
			Assert.Result(blendState.ColorDestinationBlend);
			Assert.Result(blendState.ColorSourceBlend);
			Assert.Result(blendState.ColorWriteChannels);
			Assert.Result(blendState.ColorWriteChannels1);
			Assert.Result(blendState.ColorWriteChannels2);
			Assert.Result(blendState.ColorWriteChannels3);
			Assert.Result(blendState.MultiSampleMask);
		}
	}
}
