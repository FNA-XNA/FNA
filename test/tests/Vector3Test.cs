namespace Microsoft.Xna.Framework
{
	public class Vector3Test
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Vector3.Backward);
			Assert.Result(Vector3.Down);
			Assert.Result(Vector3.Forward);
			Assert.Result(Vector3.Left);
			Assert.Result(Vector3.One);
			Assert.Result(Vector3.Right);
			Assert.Result(Vector3.UnitX);
			Assert.Result(Vector3.UnitY);
			Assert.Result(Vector3.UnitZ);
			Assert.Result(Vector3.Up);
			Assert.Result(Vector3.Zero);
		}

		[Test]
		public static void AddSubtract()
		{
			Vector3 v1 = new Vector3(2f, 10f, 23f);
			Vector3 v2 = new Vector3(1f, 5f, 16f);
			Assert.Result(v1 + v2);
			Assert.Result(v1 - v2);
			Assert.Result(Vector3.Add(v1, v2));
			Assert.Result(Vector3.Subtract(v1, v2));
			Vector3 result;
			Vector3.Add(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector3.Subtract(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void MultiplyDivide()
		{
			Vector3 v1 = new Vector3(3f, 7f, 13f);
			Vector3 v2 = new Vector3(2f, 5f, 11f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 / v2);
			Assert.Result(Vector3.Multiply(v1, v2));
			Assert.Result(Vector3.Divide(v1, v2));
			Vector3 result;
			Vector3.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector3.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Divide()
		{
			const float dividend = 1.4399751E-37f;
			const float divisor = 784f;
			Vector3 vector1 = new Vector3(dividend);
			Assert.Result((vector1 / divisor).X);
			Assert.Result(Vector3.Divide(vector1, divisor).X);
			Vector3 vector2;
			Vector3.Divide(ref vector1, divisor, out vector2);
			Assert.Result(vector2.X);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Max(float value1, float value2)
		{
			Assert.Result(Vector3.Max(new Vector3(value1), new Vector3(value2)));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Min(float value1, float value2)
		{
			Assert.Result(Vector3.Min(new Vector3(value1), new Vector3(value2)));
		}
	}
}
