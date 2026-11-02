namespace Microsoft.Xna.Framework
{
	public static class CurveKeyTest
	{
		[Test]
		public static void Default()
		{
			CurveKey curveKey;
			curveKey = new CurveKey(0f, 0f);
			Assert.Result(curveKey.TangentIn);
			Assert.Result(curveKey.TangentOut);
			Assert.Result(curveKey.Continuity);
			curveKey = new CurveKey(0f, 0f, 0f, 0f);
			Assert.Result(curveKey.Continuity);
		}

		[Argument(float.NaN, float.NaN)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		[Argument(1f, 2f)]
		[Argument(2f, 1f)]
		[Argument(1f, 1f)]
		public static void CompareTo(float f1, float f2)
		{
			Assert.Result(new CurveKey(f1, 0).CompareTo(new CurveKey(f2, 0)));
		}
	}
}
