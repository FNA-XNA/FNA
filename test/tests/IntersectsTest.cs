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
	}
}
