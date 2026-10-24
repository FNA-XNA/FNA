namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public static class HalfSingleTest
	{
		[Argument(float.MaxValue)]
		public static ushort F32toF16(float value)
		{
			return new HalfSingle(value).PackedValue;
		}
	}
}
