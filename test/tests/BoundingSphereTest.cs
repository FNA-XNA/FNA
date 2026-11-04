namespace Microsoft.Xna.Framework
{
	public static class BoundingSphereTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new BoundingSphere(Vector3.Zero, -float.Epsilon); });
			new BoundingSphere(Vector3.Zero, 0f);
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

		[Argument(float.NaN, float.NaN)]
		[Argument(0f, 5f)]
		[Argument(-float.Epsilon, 5f)]
		[Argument(float.Epsilon, 5f)]
		[Argument(0f, 3f)]
		[Argument(-float.Epsilon, 3f)]
		[Argument(float.Epsilon, 3f)]
		public static void ContainsBoundingSphere(float x1, float x2)
		{
			BoundingSphere sphere1 = new BoundingSphere(new Vector3(x1, 0f, 0f), 4f);
			BoundingSphere sphere2 = new BoundingSphere(new Vector3(x2, 0f, 0f), 1f);
			Assert.Result(sphere1.Contains(sphere2));
			ContainmentType result;
			sphere1.Contains(ref sphere2, out result);
			Assert.Result(result);
		}
	}
}
