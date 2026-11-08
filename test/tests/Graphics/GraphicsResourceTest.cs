namespace Microsoft.Xna.Framework.Graphics
{
	public static class GraphicsResourceTest
	{
		static bool Disposed;
		class MyBlendState : BlendState
		{
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyDepthStencilState : DepthStencilState
		{
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MySamplerState : SamplerState
		{
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyRasterizerState : RasterizerState
		{
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyBasicEffect : BasicEffect
		{
			public MyBasicEffect(GraphicsDevice device) : base(device) { }
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyTexture2D : Texture2D
		{
			internal MyTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) { }
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyTexture3D : Texture3D
		{
			internal MyTexture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format) : base(graphicsDevice, width, height, depth, mipMap, format) { }

			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyTextureCube : TextureCube
		{
			public MyTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format) : base(graphicsDevice, size, mipMap, format) { }

			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyIndexBuffer : IndexBuffer
		{
			public MyIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) : base(graphicsDevice, indexElementSize, indexCount, bufferUsage) { }
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MyVertexBuffer : VertexBuffer
		{
			public MyVertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage) : base(graphicsDevice, vertexDeclaration, vertexCount, usage) { }
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}
		class MySpriteBatch : SpriteBatch
		{
			public MySpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }
			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}

		[Test]
		public static void Test()
		{
			Game game = new Game();
			PresentationParameters pp = new PresentationParameters() { DeviceWindowHandle = game.Window.Handle, IsFullScreen = false };
			GraphicsDevice gd = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);

			BlendState blendState = new MyBlendState();
			DepthStencilState depthStencilState = new MyDepthStencilState();
			SamplerState samplerState = new MySamplerState();
			RasterizerState rasterizerState = new MyRasterizerState();
			BasicEffect basicEffect = new MyBasicEffect(gd);
			Texture2D texture2D = new MyTexture2D(gd, 1, 1);
			Texture3D texture3D = new MyTexture3D(gd, 1, 1, 1, false, SurfaceFormat.Color);
			TextureCube textureCube = new MyTextureCube(gd, 1, false, SurfaceFormat.Color);
			IndexBuffer indexBuffer = new MyIndexBuffer(gd, IndexElementSize.SixteenBits, 1, BufferUsage.None);
			VertexBuffer vertexBuffer = new MyVertexBuffer(gd, VertexPositionColor.VertexDeclaration, 1, BufferUsage.None);
			SpriteBatch spriteBatch = new MySpriteBatch(gd);

			gd.Dispose();
			Assert.Result(Disposed);

			blendState.Dispose();
			depthStencilState.Dispose();
			samplerState.Dispose();
			rasterizerState.Dispose();
			basicEffect.Dispose();
			texture2D.Dispose();
			texture3D.Dispose();
			textureCube.Dispose();
			indexBuffer.Dispose();
			vertexBuffer.Dispose();
			spriteBatch.Dispose();

			game.Dispose();
		}
	}
}
