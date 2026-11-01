namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public static class Rgba64Test
	{
		[Argument(float.NaN, -1.1f / 65535f, 65534.6f / 65535f, 65536.1f / 65535f)]
		public static void PackFrom(float x, float y, float z, float w)
		{
			Assert.Result(new Rgba64(x, y, z, w));
			Vector4 vector4 = new Vector4(x, y, z, w);
			Assert.Result(new Rgba64(vector4));
			IPackedVector packedVector = new Rgba64();
			packedVector.PackFromVector4(vector4);
			Assert.Result(packedVector);
		}
	}
}
