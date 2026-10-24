namespace Microsoft.Xna.Framework
{
	public static class MathHelperTest
	{
		[Argument(2f, 1f, 3f)]
		[Argument(2f, 3f, 1f)]
		[Argument(2f, float.NaN, float.NaN)]
		public static float Clamp(float value, float min, float max)
		{
			return MathHelper.Clamp(value, min, max);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static float Max(float value1, float value2)
		{
			return MathHelper.Max(value1, value2);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static float Min(float value1, float value2)
		{
			return MathHelper.Min(value1, value2);
		}
	}
}
