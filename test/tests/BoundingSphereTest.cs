namespace Microsoft.Xna.Framework
{
	public static class BoundingSphereTest
	{
		[TestException]
		public static void BoundingSphereCtor()
		{
			Assert.Throw(() => { new BoundingSphere(Vector3.Zero, -float.Epsilon); });
			Assert.Throw(() => { new BoundingSphere(Vector3.Zero, 0f); });
		}

		[TestException]
		public static void CreateFromPoints()
		{
			Assert.Throw(() => { BoundingSphere.CreateFromPoints(null); });
			Assert.Throw(() => { BoundingSphere.CreateFromPoints(new Vector3[0]); });
		}

		[TestException]
		public static void BoundingFrustum()
		{
			Assert.Throw(() => { BoundingSphere.CreateFromFrustum(null); });
			BoundingSphere boundingSphere = new BoundingSphere();
			Assert.Throw(() => { boundingSphere.Intersects(null); });
			Assert.Throw(() => { boundingSphere.Contains(null); });
		}
	}
}
