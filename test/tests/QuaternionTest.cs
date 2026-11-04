namespace Microsoft.Xna.Framework
{
	public static class QuaternionTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Quaternion.Identity);
		}

		[Test]
		public static void AddSubtract()
		{
			Quaternion v1 = new Quaternion(2f, 10f, 23f, 35f);
			Quaternion v2 = new Quaternion(1f, 5f, 16f, 33f);
			Assert.Result(v1 + v2);
			Assert.Result(v1 - v2);
			Assert.Result(Quaternion.Add(v1, v2));
			Assert.Result(Quaternion.Subtract(v1, v2));
			Quaternion result;
			Quaternion.Add(ref v1, ref v2, out result);
			Assert.Result(result);
			Quaternion.Subtract(ref v1, ref v2, out result);
			Assert.Result(result);
		}
	}
}
