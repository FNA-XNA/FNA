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
			Matrix matrix1 = new Matrix(
				5f, 5f, 5f, 5f,
				5f, 5f, 5f, 5f,
				5f, 5f, 5f, 5f,
				5f, 5f, 5f, 5f
			);
			Assert.Result(Matrix.Divide(matrix1, 3f).M11);
			Matrix matrix2;
			Matrix.Divide(ref matrix1, 3f, out matrix2);
			Assert.Result(matrix2.M11);
		}
	}
}
