namespace Microsoft.Xna.Framework
{
	public static class CurveTest
	{
		[Test]
		public static void Default()
		{
			Curve curve = new Curve();
			Assert.Result(curve.Keys == null);
			curve.Keys.Add(new CurveKey(0f, 0f));
			Assert.Result(curve.IsConstant);
			curve.Keys.Add(new CurveKey(float.Epsilon, 0f));
			Assert.Result(curve.IsConstant);
		}

		[TestException]
		public static void ComputeTangent()
		{
			Curve curve = new Curve();
			curve.Keys.Add(new CurveKey(0f, 0f));
			Assert.Throw(() => { curve.ComputeTangent(-1, CurveTangent.Flat, CurveTangent.Flat); });
			Assert.Throw(() => { curve.ComputeTangent(1, CurveTangent.Flat, CurveTangent.Flat); });
		}
	}
}
