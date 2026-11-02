namespace Microsoft.Xna.Framework.Graphics
{
	public static class DepthStencilStateTest
	{
		[Test]
		public static void Default()
		{
			DepthStencilState depthStencilState = new DepthStencilState();
			Assert.Result(depthStencilState.Tag);
			AsserDepthStencilState(depthStencilState);
			depthStencilState.Dispose();
			AsserDepthStencilState(DepthStencilState.Default);
			AsserDepthStencilState(DepthStencilState.DepthRead);
			AsserDepthStencilState(DepthStencilState.None);
		}

		static void AsserDepthStencilState(DepthStencilState depthStencilState)
		{
			Assert.Result(depthStencilState.Name);
			Assert.Result(depthStencilState.CounterClockwiseStencilDepthBufferFail);
			Assert.Result(depthStencilState.CounterClockwiseStencilFail);
			Assert.Result(depthStencilState.CounterClockwiseStencilFunction);
			Assert.Result(depthStencilState.CounterClockwiseStencilPass);
			Assert.Result(depthStencilState.DepthBufferEnable);
			Assert.Result(depthStencilState.DepthBufferFunction);
			Assert.Result(depthStencilState.DepthBufferWriteEnable);
			Assert.Result(depthStencilState.ReferenceStencil);
			Assert.Result(depthStencilState.StencilDepthBufferFail);
			Assert.Result(depthStencilState.StencilEnable);
			Assert.Result(depthStencilState.StencilFail);
			Assert.Result(depthStencilState.StencilFunction);
			Assert.Result(depthStencilState.StencilMask);
			Assert.Result(depthStencilState.StencilPass);
			Assert.Result(depthStencilState.StencilWriteMask);
			Assert.Result(depthStencilState.TwoSidedStencilMode);
		}
	}
}
