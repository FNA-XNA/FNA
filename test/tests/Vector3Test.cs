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
			Vector3 v1 = new Vector3(2f, 12f, 33f);
			Vector3 v2 = new Vector3(1f, 5f, 27f);
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
			const float f = 137f;
			Vector3 v1 = new Vector3(3f, 7f, 13f);
			Vector3 v2 = new Vector3(2f, 5f, 11f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 * f);
			Assert.Result(f * v2);
			Assert.Result(v1 / v2);
			Assert.Result(v1 / f);
			Assert.Result(Vector3.Multiply(v1, v2));
			Assert.Result(Vector3.Multiply(v1, f));
			Assert.Result(Vector3.Divide(v1, v2));
			Assert.Result(Vector3.Divide(v1, f));
			Vector3 result;
			Vector3.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector3.Multiply(ref v1, f, out result);
			Assert.Result(result);
			Vector3.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector3.Divide(ref v1, f, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Negate()
		{
			Vector3 vector = new Vector3(1f, 2f, 3f);
			Assert.Result(-vector);
			Assert.Result(Vector3.Negate(vector));
			Vector3 result;
			Vector3.Negate(ref vector, out result);
			Assert.Result(result);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void MaxMin(float value1, float value2)
		{
			Assert.Result(Vector3.Max(new Vector3(value1), new Vector3(value2)));
			Assert.Result(Vector3.Min(new Vector3(value1), new Vector3(value2)));
		}
	}
}
