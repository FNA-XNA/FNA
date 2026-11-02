namespace Microsoft.Xna.Framework.Graphics
{
	public static class IndexBufferTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new IndexBuffer(null, IndexElementSize.SixteenBits, 0, BufferUsage.None); });
			Assert.Throw(() => { new IndexBuffer(null, IndexElementSize.SixteenBits, 1, BufferUsage.None); });
		}

		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			IndexBuffer buffer = new IndexBuffer(gd, (IndexElementSize) 3, 1, BufferUsage.None);
			Assert.Result(buffer.Name);
			Assert.Result(buffer.Tag);
			Assert.Result(buffer.IndexElementSize);
			buffer.Dispose();
		}
	}
}
