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

		[Test]
		public static void NegateConjugate()
		{
			Quaternion vector = new Quaternion(1f, 2f, 3f, 4f);
			Assert.Result(-vector);
			Assert.Result(Quaternion.Negate(vector));
			Assert.Result(Quaternion.Conjugate(vector));
			Quaternion result;
			Quaternion.Negate(ref vector, out result);
			Assert.Result(result);
			Quaternion.Conjugate(ref vector, out result);
			Assert.Result(result);
			vector.Conjugate();
			Assert.Result(result);
		}

		[Test]
		public static void CreateFromAxisAngle()
		{
			const float angle = 1004441f;
			Vector3 vector3 = Vector3.One;
			Assert.Result(Quaternion.CreateFromAxisAngle(vector3, angle));
			Quaternion result;
			Quaternion.CreateFromAxisAngle(ref vector3, angle, out result);
			Assert.Result(result);
		}
	}
}
