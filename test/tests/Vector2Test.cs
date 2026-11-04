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
			const float f = 23f;
			Vector2 v1 = new Vector2(3f, 7f);
			Vector2 v2 = new Vector2(2f, 5f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 * f);
			Assert.Result(f * v2);
			Assert.Result(v1 / v2);
			Assert.Result(v1 / f);
			Assert.Result(Vector2.Multiply(v1, v2));
			Assert.Result(Vector2.Multiply(v1, f));
			Assert.Result(Vector2.Divide(v1, v2));
			Assert.Result(Vector2.Divide(v1, f));
			Vector2 result;
			Vector2.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector2.Multiply(ref v1, f, out result);
			Assert.Result(result);
			Vector2.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector2.Divide(ref v1, f, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Negate()
		{
			Vector2 vector = new Vector2(1f, 2f);
			Assert.Result(-vector);
			Assert.Result(Vector2.Negate(vector));
			Vector2 result;
			Vector2.Negate(ref vector, out result);
			Assert.Result(result);
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
