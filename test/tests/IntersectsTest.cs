namespace Microsoft.Xna.Framework
{
	public static class IntersectsTest
	{
		[Argument(float.NaN)]
		[Argument(0f)]
		[Argument(-float.Epsilon)]
		[Argument(float.Epsilon)]
		public static void BoxSphere(float x)
		{
			BoundingSphere sphere = new BoundingSphere(new Vector3(x, 0f, 0f), 1f);
			BoundingBox box = new BoundingBox(new Vector3(1f, -1f, -1f), new Vector3(2f, 1f, 1f));
			Assert.Result(sphere.Intersects(box));
			Assert.Result(box.Intersects(sphere));
			bool result;
			sphere.Intersects(ref box, out result);
			Assert.Result(result);
			box.Intersects(ref sphere, out result);
			Assert.Result(result);
		}

		[Argument(float.NaN)]
		[Argument(0f)]
		[Argument(-float.Epsilon)]
		[Argument(float.Epsilon)]
		public static void SphereSphere(float x)
		{
			BoundingSphere sphere1 = new BoundingSphere(new Vector3(x, 0f, 0f), 1f);
			BoundingSphere sphere2 = new BoundingSphere(new Vector3(2f, 0f, 0f), 1f);
			Assert.Result(sphere1.Intersects(sphere2));
			bool result;
			sphere1.Intersects(ref sphere2, out result);
			Assert.Result(result);
		}

		[Argument(float.NaN)]
		[Argument(0f)]
		[Argument(-float.Epsilon)]
		[Argument(float.Epsilon)]
		public static void SpherePlane(float x)
		{
			BoundingSphere sphere = new BoundingSphere(new Vector3(x, 0f, 0f), 1f);
			Plane plane = new Plane(new Vector3(1f, 0f, 0f), 1f);
			Assert.Result(sphere.Intersects(plane));
			Assert.Result(plane.Intersects(sphere));
			PlaneIntersectionType result;
			sphere.Intersects(ref plane, out result);
			Assert.Result(result);
			plane.Intersects(ref sphere, out result);
			Assert.Result(result);
		}
	}
}
