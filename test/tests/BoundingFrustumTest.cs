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

		[Argument(-1e-4f)]
		[Argument(-1e-6f)]
		[Argument(1e-6f)]
		[Argument(1e-4f)]
		public static void ContainsPoint(float z)
		{
			BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);
			Vector3 point = new Vector3(0f, 0f, z);
			Assert.Result(frustum.Contains(point));
			ContainmentType result;
			frustum.Contains(ref point, out result);
			Assert.Result(result);
		}

		[Argument(1f)]
		[Argument(-1f)]
		[Argument(3f)]
		public static void ContainsSphere(float z)
		{
			BoundingFrustum frustum = new BoundingFrustum(Matrix.CreateOrthographicOffCenter(-1, 1, -1, 1, 0, -2));
			BoundingSphere sphere = new BoundingSphere(new Vector3(0f, 0f, z), 1f);
			Assert.Result(frustum.Contains(sphere));
			ContainmentType result;
			frustum.Contains(ref sphere, out result);
			Assert.Result(result);
		}
	}
}
