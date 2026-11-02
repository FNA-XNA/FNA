namespace Microsoft.Xna.Framework.Graphics
{
	public static class RenderTargetBindingTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new RenderTargetBinding(null); });
			Assert.Throw(() => { new RenderTargetBinding(null, 0); });
			Assert.Throw(() => { _ = (RenderTargetBinding) null; });
		}
	}
}
