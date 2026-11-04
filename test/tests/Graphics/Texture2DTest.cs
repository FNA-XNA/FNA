using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public static class Texture2DTest
	{
		struct RGB
		{
			byte r;
			byte g;
			byte b;
		}
		class SeekStream : Stream
		{
			readonly bool canSeek;
			internal SeekStream(bool canSeek)
			{
				this.canSeek = canSeek;
			}
			public override bool CanRead => throw new System.NotImplementedException();
			public override bool CanSeek => canSeek;
			public override bool CanWrite => throw new System.NotImplementedException();
			public override long Length => throw new System.NotImplementedException();
			public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
			public override void Flush()
			{
				throw new System.NotImplementedException();
			}
			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new System.NotImplementedException();
			}
			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new System.NotImplementedException();
			}
			public override void SetLength(long value)
			{
				throw new System.NotImplementedException();
			}
			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new System.NotImplementedException();
			}
		}

		[TestException]
		public static void Ctor()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { new Texture2D(null, 1, 1).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 0, 1).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0).Dispose(); });

			Assert.Throw(() => { new Texture2D(null, 1, 1, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 0, 1, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 1, false, SurfaceFormat.Dxt1).Dispose(); });
		}

		[TestException]
		public static void FromStream()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { Texture2D.FromStream(null, new SeekStream(true)).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(false)).Dispose(); });

			Assert.Throw(() => { Texture2D.FromStream(null, new SeekStream(true), 1, 1, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null, 1, 1, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(false), 1, 1, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(true), 0, 1, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(true), 1, 0, false).Dispose(); });
		}

		[Test]
		public static void Default()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Texture2D texture = new Texture2D(gd, 4, 4);
			Assert.Result(texture.Name);
			Assert.Result(texture.Tag);
			Assert.Result(texture.Format);
			texture.Dispose();
		}

		[TestException]
		public static void GetData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Texture2D texture;
			Color[] colors = new Color[1];
			long[] ls = new long[1];
			RGB[] rgb = new RGB[1];
			using (texture = new Texture2D(gd, 4, 8, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.GetData(4, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.GetData(4, new Rectangle(-1, -1, 0, 0), ls, -1, 0); });

				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), ls, 0, 0); });
				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), ls, -1, 1); });
				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), ls, 2, 1); });
				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), ls, 1, 1); });

				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), ls, 0, 1); });
				Assert.Throw(() => { texture.GetData(3, new Rectangle(-1, -1, 0, 0), rgb, 0, 1); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 0), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 0, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, 0, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, -1, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 2, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 3), colors, 0, 1); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 2), colors, 0, 1); });
			}
			Assert.Throw(() => { texture.GetData<Color>(null); });
		}

		[TestException]
		public static void SetData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Texture2D texture;
			Color[] colors = new Color[1];
			long[] ls = new long[1];
			RGB[] rgb = new RGB[1];
			using (texture = new Texture2D(gd, 4, 8, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.SetData(4, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.SetData(4, new Rectangle(-1, -1, 0, 0), ls, -1, 0); });

				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), ls, 0, 0); });
				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), ls, -1, 1); });
				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), ls, 2, 1); });
				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), ls, 1, 1); });

				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), ls, 0, 1); });
				Assert.Throw(() => { texture.SetData(3, new Rectangle(-1, -1, 0, 0), rgb, 0, 1); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 0), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 0, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, 0, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, -1, 1, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 2, 1), colors, 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 3), colors, 0, 1); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 2), colors, 0, 1); });
			}
			Assert.Throw(() => { texture.SetData<Color>(null); });
		}
	}
}
