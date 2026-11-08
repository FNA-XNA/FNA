namespace Microsoft.Xna.Framework.Graphics
{
	public static class VertexDeclarationTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new VertexDeclaration(null); });
			Assert.Throw(() => { new VertexDeclaration(); });
			Assert.Throw(() => { new VertexDeclaration(0, null); });
			Assert.Throw(() => { new VertexDeclaration(0, new VertexElement()); });
			Assert.Throw(() => { new VertexDeclaration(3, new VertexElement()); });
		}
	}
}
