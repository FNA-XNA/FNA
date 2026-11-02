namespace Microsoft.Xna.Framework
{
	public static class CurveKeyCollectionTest
	{
		[Test]
		public static void Default()
		{
			CurveKeyCollection curveKeys = new CurveKeyCollection();
			Assert.Result(curveKeys.IsReadOnly);
		}

		[Test]
		public static void Test()
		{
			CurveKeyCollection curveKeys = new CurveKeyCollection
			{
				new CurveKey(0f, 0f),
				new CurveKey(-float.Epsilon, 0f),
				new CurveKey(float.Epsilon, 0f),
				new CurveKey(-2 * float.Epsilon, 0f),
				new CurveKey(2 * float.Epsilon, 0f)
			};
			foreach (CurveKey curveKey in curveKeys)
			{
				Assert.Result(curveKey.Position);
			}
		}

		[TestException]
		public static void TestException()
		{
			CurveKeyCollection curveKeys = new CurveKeyCollection
			{
				new CurveKey(0f, 0f)
			};
			Assert.Throw(() => { curveKeys.Add(null); });
			Assert.Throw(() => { curveKeys[0] = null; });
		}
	}
}
