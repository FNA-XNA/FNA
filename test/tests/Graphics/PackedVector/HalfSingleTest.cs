namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public static class HalfSingleTest
	{
		[Argument(float.MaxValue)]
		[Argument(float.NaN)]
		public static void F32toF16(float value)
		{
			Assert.Result(new HalfSingle(value));
		}
	}
}
