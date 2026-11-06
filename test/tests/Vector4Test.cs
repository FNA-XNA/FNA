namespace Microsoft.Xna.Framework
{
	public static class Vector4Test
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Vector4.One);
			Assert.Result(Vector4.UnitW);
			Assert.Result(Vector4.UnitX);
			Assert.Result(Vector4.UnitY);
			Assert.Result(Vector4.UnitZ);
			Assert.Result(Vector4.Zero);
		}

		[Test]
		public static void AddSubtract()
		{
			Vector4 v1 = new Vector4(2f, 12f, 33f, 69f);
			Vector4 v2 = new Vector4(1f, 5f, 27f, 57f);
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
			const float f = 137f;
			Vector4 v1 = new Vector4(3f, 7f, 13f, 19f);
			Vector4 v2 = new Vector4(2f, 5f, 11f, 17f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 * f);
			Assert.Result(f * v2);
			Assert.Result(v1 / v2);
			Assert.Result(v1 / f);
			Assert.Result(Vector4.Multiply(v1, v2));
			Assert.Result(Vector4.Multiply(v1, f));
			Assert.Result(Vector4.Divide(v1, v2));
			Assert.Result(Vector4.Divide(v1, f));
			Vector4 result;
			Vector4.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector4.Multiply(ref v1, f, out result);
			Assert.Result(result);
			Vector4.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
			Vector4.Divide(ref v1, f, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Negate()
		{
			Vector4 vector = new Vector4(1f, 2f, 3f, 4f);
			Assert.Result(-vector);
			Assert.Result(Vector4.Negate(vector));
			Vector4 result;
			Vector4.Negate(ref vector, out result);
			Assert.Result(result);
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
