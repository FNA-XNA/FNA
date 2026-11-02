namespace Microsoft.Xna.Framework
{
	public static class CurveKeyCollectionTest
	{
		[Test]
		public static void Test()
		{
			CurveKeyCollection curveKeys = new CurveKeyCollection();
			curveKeys.Add(new CurveKey(0f, 0f));
			curveKeys.Add(new CurveKey(-1f, 0f));
			curveKeys.Add(new CurveKey(1f, 0f));
			curveKeys.Add(new CurveKey(-2f, 0f));
			curveKeys.Add(new CurveKey(2f, 0f));
			foreach (CurveKey curveKey in curveKeys)
			{
				Assert.Result(curveKey.Position);
			}
		}

		[TestException]
		public static void TestException()
		{
			CurveKeyCollection curveKeys = new CurveKeyCollection();
			curveKeys.Add(new CurveKey(0f, 0f));
			Assert.Throw(() => { curveKeys.Add(null); });
			Assert.Throw(() => { curveKeys[0] = null; });
		}
	}
}
