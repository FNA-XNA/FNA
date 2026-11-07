namespace Microsoft.Xna.Framework
{
	public static class BoundingFrustumTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(BoundingFrustum.CornerCount);
		}

		[TestException]
		public static void GetCorners()
		{
			BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);
			Assert.Throw(() => { frustum.GetCorners(null); });
			Assert.Throw(() => { frustum.GetCorners(new Vector3[7]); });
		}
	}
}
