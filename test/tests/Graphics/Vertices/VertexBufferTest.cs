using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public static class VertexBufferTest
	{
		struct NullVertexType : IVertexType
		{
			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					return null;
				}
			}
		}
		struct EmptyVertexType : IVertexType
		{
			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					return new VertexDeclaration(new VertexElement());
				}
			}
		}
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new VertexBuffer(null, (Type) null, 0, (BufferUsage) (-1)); });
			Assert.Throw(() => { new VertexBuffer(null, typeof(Game), 0, (BufferUsage) (-1)); });
			Assert.Throw(() => { new VertexBuffer(null, typeof(Viewport), 0, (BufferUsage) (-1)); });
			Assert.Throw(() => { new VertexBuffer(null, typeof(NullVertexType), 0, (BufferUsage) (-1)); });
			Assert.Throw(() => { new VertexBuffer(null, typeof(EmptyVertexType), 0, (BufferUsage) (-1)); });
			Assert.Throw(() => { new VertexBuffer(null, typeof(VertexPositionColor), 0, (BufferUsage) (-1)); });
		}
	}
}
