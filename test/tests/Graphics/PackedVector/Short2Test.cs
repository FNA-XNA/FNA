namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public static class Short2Test
	{
		[Argument(float.NaN, -32768.6f)]
		[Argument(-32767.6f, 32767.6f)]
		public static void PackFrom(float x, float y)
		{
			Assert.Result(new Short2(x, y));
			Assert.Result(new Short2(new Vector2(x, y)));
			IPackedVector packedVector = new Short2();
			packedVector.PackFromVector4(new Vector4(x, y, 0f, 0f));
			Assert.Result(packedVector);
		}
	}
}
