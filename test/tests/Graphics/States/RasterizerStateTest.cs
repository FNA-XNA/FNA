namespace Microsoft.Xna.Framework.Graphics
{
	public static class RasterizerStateTest
	{
		[Test]
		public static void Default()
		{
			RasterizerState rasterizerState = new RasterizerState();
			Assert.Result(rasterizerState.Tag);
			AssertRasterizerState(rasterizerState);
			rasterizerState.Dispose();
			AssertRasterizerState(RasterizerState.CullClockwise);
			AssertRasterizerState(RasterizerState.CullCounterClockwise);
			AssertRasterizerState(RasterizerState.CullNone);
		}

		static void AssertRasterizerState(RasterizerState rasterizerState)
		{
			Assert.Result(rasterizerState.Name);
			Assert.Result(rasterizerState.CullMode);
			Assert.Result(rasterizerState.DepthBias);
			Assert.Result(rasterizerState.FillMode);
			Assert.Result(rasterizerState.MultiSampleAntiAlias);
			Assert.Result(rasterizerState.ScissorTestEnable);
			Assert.Result(rasterizerState.SlopeScaleDepthBias);
		}
	}
}
