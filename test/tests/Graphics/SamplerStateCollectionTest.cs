namespace Microsoft.Xna.Framework.Graphics
{
	public static class SamplerStateCollectionTest
	{
		[TestException]
		public static void TestException()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			SamplerStateCollection samplerStates = gd.SamplerStates;
			Assert.Throw(() => { samplerStates[-1] = null; });
			Assert.Throw(() => { samplerStates[0] = null; });
			Assert.Throw(() => { samplerStates[int.MaxValue] = null; });
			Assert.Throw(() => { var _ = samplerStates[-1]; });
			Assert.Throw(() => { var _ = samplerStates[int.MaxValue]; });
		}
	}
}
