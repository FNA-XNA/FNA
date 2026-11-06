namespace Microsoft.Xna.Framework
{
	public static class PlaneTest
	{
		[Argument(float.NaN)]
		[Argument(1f / (1 << 11))]
		[Argument(1f / (1 << 12))]
		public static void Normalize(float x)
		{
			Plane plane = new Plane(x, 0f, 1f, 6f);
			plane.Normalize();
			Assert.Result(plane);
			plane = new Plane(x, 0f, 1f, 6f);
			Assert.Result(Plane.Normalize(plane));
			Plane result;
			Plane.Normalize(ref plane, out result);
			Assert.Result(result);
		}
	}
}
