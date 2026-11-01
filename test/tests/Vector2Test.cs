namespace Microsoft.Xna.Framework
{
	public class Vector2Test
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Vector2.One);
			Assert.Result(Vector2.UnitX);
			Assert.Result(Vector2.UnitY);
			Assert.Result(Vector2.Zero);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Max(float value1, float value2)
		{
			Assert.Result(Vector2.Max(new Vector2(value1), new Vector2(value2)));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Min(float value1, float value2)
		{
			Assert.Result(Vector2.Min(new Vector2(value1), new Vector2(value2)));
		}
	}
}
