namespace Microsoft.Xna.Framework
{
	public static class MatrixTest
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Matrix.Identity);
		}

		[Test]
		public static void AddSubtract()
		{
			Matrix v1 = new Matrix(2f, 12f, 33f, 69f, 116f, 135f, 164f, 178f, 203f, 232f, 259f, 290f, 318f, 339f, 366f, 386f);
			Matrix v2 = new Matrix(1f, 5f, 27f, 57f, 77f, 118f, 151f, 169f, 189f, 214f, 246f, 277f, 300f, 329f, 353f, 378f);
			Assert.Result(v1 + v2);
			Assert.Result(v1 - v2);
			Assert.Result(Matrix.Add(v1, v2));
			Assert.Result(Matrix.Subtract(v1, v2));
			Matrix result;
			Matrix.Add(ref v1, ref v2, out result);
			Assert.Result(result);
			Matrix.Subtract(ref v1, ref v2, out result);
			Assert.Result(result);
		}

		[Test]
		public static void MultiplyDivide()
		{
			const float f = 137f;
			Matrix v1 = new Matrix(3f, 7f, 13f, 19f, 29f, 37f, 43f, 53f, 61f, 71f, 79f, 89f, 101f, 107f, 113f, 131f);
			Matrix v2 = new Matrix(2f, 5f, 11f, 17f, 23f, 31f, 41f, 47f, 59f, 67f, 73f, 83f, 97f, 103f, 109f, 127f);
			Assert.Result(v1 * v2);
			Assert.Result(v1 * f);
			Assert.Result(f * v2);
			Assert.Result(v1 / v2);
			Assert.Result(v1 / f);
			Assert.Result(Matrix.Multiply(v1, v2));
			Assert.Result(Matrix.Multiply(v1, f));
			Assert.Result(Matrix.Divide(v1, v2));
			Assert.Result(Matrix.Divide(v1, f));
			Matrix result;
			Matrix.Multiply(ref v1, ref v2, out result);
			Assert.Result(result);
			Matrix.Multiply(ref v1, f, out result);
			Assert.Result(result);
			Matrix.Divide(ref v1, ref v2, out result);
			Assert.Result(result);
			Matrix.Divide(ref v1, f, out result);
			Assert.Result(result);
		}

		[Test]
		public static void Divide()
		{
			const float dividend = 1.4399751E-37f;
			const float divisor = 784f;
			Matrix matrix1 = new Matrix(
				dividend, dividend, dividend, dividend,
				dividend, dividend, dividend, dividend,
				dividend, dividend, dividend, dividend,
				dividend, dividend, dividend, dividend
			);
			Assert.Result((matrix1 / divisor).M11);
			Assert.Result(Matrix.Divide(matrix1, divisor).M11);
			Matrix matrix2;
			Matrix.Divide(ref matrix1, divisor, out matrix2);
			Assert.Result(matrix2.M11);
		}
	}
}
