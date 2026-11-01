namespace Microsoft.Xna.Framework
{
	public static class MathHelperTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(MathHelper.E);
			Assert.Result(MathHelper.Log2E);
			Assert.Result(MathHelper.Log10E);
			Assert.Result(MathHelper.Pi);
			Assert.Result(MathHelper.PiOver2);
			Assert.Result(MathHelper.PiOver4);
			Assert.Result(MathHelper.TwoPi);
		}

		[Argument(2f, 1f, 3f)]
		[Argument(2f, 3f, 1f)]
		[Argument(2f, float.NaN, float.NaN)]
		public static void Clamp(float value, float min, float max)
		{
			Assert.Result(MathHelper.Clamp(value, min, max));
		}

		[Argument(0f, 0f, 1f, 0f, 1f / (1L << 42))]
		public static void Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
		{
			Assert.Result(MathHelper.Hermite(value1, tangent1, value2, tangent2, amount));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Max(float value1, float value2)
		{
			Assert.Result(MathHelper.Max(value1, value2));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Min(float value1, float value2)
		{
			Assert.Result(MathHelper.Min(value1, value2));
		}

		[Argument(0f, 1f, 1f / (1L << 42))]
		public static void SmoothStep(float value1, float value2, float amount)
		{
			Assert.Result(MathHelper.SmoothStep(value1, value2, amount));
		}

		[Argument(float.NaN)]
		[Argument(180f)]
		public static void ToDegrees(float radians)
		{
			Assert.Result(MathHelper.ToDegrees(radians));
		}

		[Argument(float.NaN)]
		[Argument(180f)]
		public static void ToRadians(float degrees)
		{
			Assert.Result(MathHelper.ToRadians(degrees));
		}

		[Argument(float.NaN)]
		[Argument(float.MinValue)]
		[Argument(float.MaxValue)]
		[Argument(MathHelper.Pi)]
		[Argument(-MathHelper.Pi)]
		public static void WrapAngle(float angle)
		{
			Assert.Result(MathHelper.WrapAngle(angle));
		}
	}
}
