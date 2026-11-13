namespace Microsoft.Xna.Framework
{
	public static class PlaneTest
	{
		[TestException]
		public static void BoundingFrustum()
		{
			Assert.Throw(() => { new Plane().Intersects(null); });
		}

		[Argument(float.NaN)]
		[Argument(0.99999988f)]
		[Argument(0.99999994f)]
		public static void Normalize(float x)
		{
			Plane plane = new Plane(x, 0f, 0f, 6f);
			plane.Normalize();
			Assert.Result(plane);
			plane = new Plane(x, 0f, 0f, 6f);
			Assert.Result(Plane.Normalize(plane));
			Plane result;
			Plane.Normalize(ref plane, out result);
			Assert.Result(result);
		}
	}
}
