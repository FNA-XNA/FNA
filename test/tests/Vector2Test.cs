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

		[Test]
		public static void AddSubtract()
		{
			Vector2 v1 = new Vector2(2f, 10f);
			Vector2 v2 = new Vector2(1f, 5f);
			Assert.Result(v1 + v2);
			Assert.Result(v1 - v2);
			Assert.Result(Vector2.Add(v1, v2));
			Assert.Result(Vector2.Subtract(v1, v2));
			Vector2 result;
			Vector2.Add(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector2.Subtract(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void MultiplyDivide()
		{
			Vector2 v1 = new Vector2(3f, 7f);
			Vector2 v2 = new Vector2(2f, 5f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 / v2);
			Assert.Result(Vector2.Multiply(v1, v2));
			Assert.Result(Vector2.Divide(v1, v2));
			Vector2 result;
			Vector2.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector2.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Divide()
		{
			const float dividend = 1.4399751E-37f;
			const float divisor = 784f;
			Vector2 vector1 = new Vector2(dividend);
			Assert.Result((vector1 / divisor).X);
			Assert.Result(Vector2.Divide(vector1, divisor).X);
			Vector2 vector2;
			Vector2.Divide(ref vector1, divisor, out vector2);
			Assert.Result(vector2.X);
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
