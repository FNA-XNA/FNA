namespace Microsoft.Xna.Framework
{
	public static class Vector4Test
	{
		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector4 Max(float value1, float value2)
		{
			return Vector4.Max(new Vector4(value1), new Vector4(value2));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector4 Min(float value1, float value2)
		{
			return Vector4.Min(new Vector4(value1), new Vector4(value2));
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
