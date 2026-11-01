namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public static class Short4Test
	{
		[Argument(float.NaN, -32768.6f, -32767.6f, 32767.6f)]
		public static void PackFrom(float x, float y, float z, float w)
		{
			Assert.Result(new Short4(x, y, z, w));
			Vector4 vector4 = new Vector4(x, y, z, w);
			Assert.Result(new Short4(vector4));
			IPackedVector packedVector = new Short4();
			packedVector.PackFromVector4(vector4);
			Assert.Result(packedVector);
		}
	}
}
