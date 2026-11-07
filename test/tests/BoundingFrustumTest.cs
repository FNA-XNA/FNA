namespace Microsoft.Xna.Framework
{
	public static class BoundingFrustumTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(BoundingFrustum.CornerCount);
		}
	}
}
