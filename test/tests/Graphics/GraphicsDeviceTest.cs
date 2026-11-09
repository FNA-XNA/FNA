namespace Microsoft.Xna.Framework.Graphics
{
	public static class GraphicsDeviceTest
	{
		[Test]
		public static Color GetBackBufferData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;

			Texture2D white = new Texture2D(gd, 1, 1);
			white.SetData(new[] { Color.White });

			int width = gd.PresentationParameters.BackBufferWidth;
			int height = gd.PresentationParameters.BackBufferHeight;

			SpriteBatch sb = new SpriteBatch(gd);
			sb.Begin();
			sb.Draw(white, new Rectangle(0, 0, width, height >> 1), Color.Red);
			sb.Draw(white, new Rectangle(0, height >> 1, width, height), Color.Blue);
			sb.End();

			Color[] backBuffer = new Color[1];
			gd.GetBackBufferData(new Rectangle(0, 0, 1, 1), backBuffer, 0, 1);
			return backBuffer[0];
		}

		[TestException]
		public static void Disposed()
		{
			Game game = new Game();
			PresentationParameters pp = new PresentationParameters() { DeviceWindowHandle = game.Window.Handle, IsFullScreen = false };
			GraphicsDevice gd = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
			gd.Dispose();
			game.Dispose();
			Assert.Throw(() => { gd.BlendFactor = new Color(); });
			Assert.Throw(() => { gd.BlendState = null; });
			Assert.Throw(() => { gd.DepthStencilState = null; });
			Assert.Throw(() => { var _ = gd.DisplayMode; });
			Assert.Throw(() => { var _ = gd.GraphicsDeviceStatus; });
			Assert.Throw(() => { gd.Indices = null; });
			Assert.Throw(() => { gd.MultiSampleMask = 0; });
			Assert.Throw(() => { gd.RasterizerState = null; });
			Assert.Throw(() => { gd.ReferenceStencil = 0; });
			Assert.Throw(() => { var _ = gd.ScissorRectangle; });
			Assert.Throw(() => { gd.ScissorRectangle = new Rectangle(); });
			Assert.Throw(() => { var _ = gd.Viewport; });
			Assert.Throw(() => { gd.Viewport = new Viewport(); });

			Assert.Throw(() => { gd.Clear(0, Color.Black, 0f, 0); });
			Assert.Throw(() => { gd.Clear(0, Vector4.One, 0f, 0); });
			Assert.Throw(() => { gd.DrawIndexedPrimitives(0, 0, 0, 0, 0, 0); });
			Assert.Throw(() => { gd.DrawInstancedPrimitives(0, 0, 0, 0, 0, 0, 0); });
			Assert.Throw(() => { gd.DrawPrimitives(0, 0, 0); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<VertexBufferTest.NullVertexType>(0, null, 0, 0, (int[]) null, 0, 0); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<VertexPositionColor>(0, null, 0, 0, (int[]) null, 0, 0); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<VertexBufferTest.NullVertexType>(0, null, 0, 0, (short[]) null, 0, 0); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<VertexPositionColor>(0, null, 0, 0, (short[]) null, 0, 0); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<int>(0, null, 0, 0, (int[]) null, 0, 0, null); });
			Assert.Throw(() => { gd.DrawUserIndexedPrimitives<int>(0, null, 0, 0, (short[]) null, 0, 0, null); });
			Assert.Throw(() => { gd.DrawUserPrimitives<VertexPositionColor>(0, null, 0, 0); });
			Assert.Throw(() => { gd.DrawUserPrimitives<VertexPositionColor>(0, null, 0, 0, null); });
			Assert.Throw(() => { gd.GetBackBufferData<int>(null); });
			Assert.Throw(() => { gd.GetBackBufferData<int>(null, 0, 0); });
			Assert.Throw(() => { gd.GetBackBufferData<int>(null, null, 0, 0); });
			Assert.Throw(() => { gd.Reset(); });
			Assert.Throw(() => { gd.Reset(null); });
			Assert.Throw(() => { gd.Reset(null, null); });
			Assert.Throw(() => { gd.SetRenderTarget(null); });
			Assert.Throw(() => { gd.SetRenderTarget(null, 0); });
			Assert.Throw(() => { gd.SetRenderTargets(null); });
			Assert.Throw(() => { gd.SetVertexBuffer(null); });
			Assert.Throw(() => { gd.SetVertexBuffer(null, 0); });
			Assert.Throw(() => { gd.SetVertexBuffers(null); });
		}
	}
}
