namespace Microsoft.Xna.Framework
{
	public static class Vector4Test
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Vector4.One);
			Assert.Result(Vector4.UnitX);
			Assert.Result(Vector4.UnitY);
			Assert.Result(Vector4.UnitZ);
			Assert.Result(Vector4.UnitW);
			Assert.Result(Vector4.Zero);
		}

		[Test]
		public static void AddSubtract()
		{
			Vector4 v1 = new Vector4(2f, 10f, 23f, 35f);
			Vector4 v2 = new Vector4(1f, 5f, 16f, 33f);
			Assert.Result(v1 + v2);
			Assert.Result(v1 - v2);
			Assert.Result(Vector4.Add(v1, v2));
			Assert.Result(Vector4.Subtract(v1, v2));
			Vector4 result;
			Vector4.Add(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector4.Subtract(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void MultiplyDivide()
		{
			Vector4 v1 = new Vector4(3f, 7f, 13f, 19f);
			Vector4 v2 = new Vector4(2f, 5f, 11f, 17f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 / v2);
			Assert.Result(Vector4.Multiply(v1, v2));
			Assert.Result(Vector4.Divide(v1, v2));
			Vector4 result;
			Vector4.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector4.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Divide()
		{
			const float dividend = 1.4399751E-37f;
			const float divisor = 784f;
			Vector4 vector1 = new Vector4(dividend);
			Assert.Result((vector1 / divisor).X);
			Assert.Result(Vector4.Divide(vector1, divisor).X);
			Vector4 vector2;
			Vector4.Divide(ref vector1, divisor, out vector2);
			Assert.Result(vector2.X);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Max(float value1, float value2)
		{
			Assert.Result(Vector4.Max(new Vector4(value1), new Vector4(value2)));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Min(float value1, float value2)
		{
			Assert.Result(Vector4.Min(new Vector4(value1), new Vector4(value2)));
		}

		[TestException]
		public static void TransformMatrix()
		{
			Matrix matrix = new Matrix();
			Assert.Throw(() => { Vector4.Transform(null, int.MaxValue, ref matrix, null, 0, 1); });
			Assert.Throw(() => { Vector4.Transform(new Vector4[0], int.MaxValue, ref matrix, null, 0, 1); });
			Assert.Throw(() => { Vector4.Transform(new Vector4[0], int.MaxValue, ref matrix, new Vector4[0], 0, 1); });
		}
	}
}
