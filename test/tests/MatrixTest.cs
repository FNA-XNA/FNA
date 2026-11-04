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
