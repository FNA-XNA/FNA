namespace Microsoft.Xna.Framework.Graphics
{
	public static class TextureCubeTest
	{
		struct RGB
		{
			byte r;
			byte g;
			byte b;
		}

		[TestException]
		public static void TextureCubeCtor()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { new TextureCube(null, 0, true, SurfaceFormat.Dxt1).Dispose(); });
			Assert.Throw(() => { new TextureCube(gd, 0, true, SurfaceFormat.Dxt1).Dispose(); });
			Assert.Throw(() => { new TextureCube(gd, 1, true, SurfaceFormat.Dxt1).Dispose(); });
		}

		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			TextureCube texture = new TextureCube(gd, 1, false, SurfaceFormat.Color);
			Assert.Result(texture.Name);
			Assert.Result(texture.Tag);
			texture.Dispose();
		}

		[TestException]
		public static void GetData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			TextureCube texture;
			Color[] colors = new Color[1];
			long[] ls = new long[1];
			RGB[] rgb = new RGB[1];
			using (texture = new TextureCube(gd, 4, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 3, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 3, new Rectangle(-1, -1, 0, 0), ls, -1, 0); });

				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 0, 0); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, -1, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 2, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 1, 1); });

				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), rgb, 0, 1); });

				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 1, 0), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 0, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, 0, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(0, -1, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 2, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 1, 2), colors, 0, 1); });

				Assert.Throw(() => { texture.GetData(CubeMapFace.PositiveX, 1, new Rectangle(0, 0, 1, 2), colors, 0, 1); });
			}
			Assert.Throw(() => { texture.GetData<Color>(CubeMapFace.PositiveX, null); });
		}

		[TestException]
		public static void SetData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			TextureCube texture;
			Color[] colors = new Color[1];
			long[] ls = new long[1];
			RGB[] rgb = new RGB[1];
			using (texture = new TextureCube(gd, 4, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 3, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 3, new Rectangle(-1, -1, 0, 0), ls, -1, 0); });

				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 0, 0); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, -1, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 2, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 1, 1); });

				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), ls, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, -1, 0, 0), rgb, 0, 1); });

				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 1, 0), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 0, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(-1, 0, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(0, -1, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 2, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 2, new Rectangle(0, 0, 1, 2), colors, 0, 1); });

				Assert.Throw(() => { texture.SetData(CubeMapFace.PositiveX, 1, new Rectangle(0, 0, 1, 2), colors, 0, 1); });
			}
			Assert.Throw(() => { texture.SetData<Color>(CubeMapFace.PositiveX, null); });
		}
	}
}
