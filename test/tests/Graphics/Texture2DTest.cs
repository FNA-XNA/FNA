using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public static class Texture2DTest
	{
		struct RGB
		{
			readonly byte r;
			readonly byte g;
			readonly byte b;
		}
		class SeekStream : Stream
		{
			bool canSeek;
			internal SeekStream(bool canSeek)
			{
				this.canSeek = canSeek;
			}

			public override bool CanRead
			{
				get
				{
					throw new System.NotImplementedException();
				}
			}

			public override bool CanSeek
			{
				get
				{
					return canSeek;
				}
			}

			public override bool CanWrite
			{
				get
				{
					throw new System.NotImplementedException();
				}
			}

			public override long Length
			{
				get
				{
					throw new System.NotImplementedException();
				}
			}

			public override long Position
			{
				get
				{
					throw new System.NotImplementedException();
				}

				set
				{
					throw new System.NotImplementedException();
				}
			}

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
			Assert.Throw(() => { new Texture2D(null, 0, 0).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 0, 0).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0).Dispose(); });

			Assert.Throw(() => { new Texture2D(null, 0, 0, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 0, 0, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0, false, SurfaceFormat.Color).Dispose(); });
			Assert.Throw(() => { new Texture2D(gd, 1, 1, false, SurfaceFormat.Dxt1).Dispose(); });
		}

		[TestException]
		public static void FromStream()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { Texture2D.FromStream(null, null).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(false)).Dispose(); });

			Assert.Throw(() => { Texture2D.FromStream(null, null, 0, 0, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null, 0, 0, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(false), 0, 0, false).Dispose(); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new SeekStream(true), 0, 0, false).Dispose(); });
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
			using (texture = new Texture2D(gd, 2, 8, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.GetData<Color>(-1, new Rectangle(-1, -1, 0, 0), null, -1, 0); });
				Assert.Throw(() => { texture.GetData(-1, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.GetData(-1, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });
				Assert.Throw(() => { texture.GetData(4, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 0); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 1, 0); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 2, 0); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 2); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, -1, 0, 0), new RGB[1], 0, 1); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(-1, 0, 1, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, -1, 1, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 0, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 0), new int[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 2, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 3), new int[1], 0, 1); });

				Assert.Throw(() => { texture.GetData(2, new Rectangle(0, 0, 1, 2), new int[1], 0, 1); });
			}
			Assert.Throw(() => { texture.GetData<Color>(null); });
		}

		[TestException]
		public static void SetData()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Texture2D texture;
			using (texture = new Texture2D(gd, 2, 8, true, SurfaceFormat.Color))
			{
				Assert.Throw(() => { texture.SetData<Color>(-1, new Rectangle(-1, -1, 0, 0), null, -1, 0); });
				Assert.Throw(() => { texture.SetData(-1, new Rectangle(-1, -1, 0, 0), new long[0], -1, 0); });

				Assert.Throw(() => { texture.SetData(-1, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });
				Assert.Throw(() => { texture.SetData(4, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], -1, 0); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 0); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 1, 0); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 2, 0); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 2); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new long[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, -1, 0, 0), new RGB[1], 0, 1); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(-1, 0, 1, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, -1, 1, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 0, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 0), new int[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 2, 1), new int[1], 0, 1); });
				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 3), new int[1], 0, 1); });

				Assert.Throw(() => { texture.SetData(2, new Rectangle(0, 0, 1, 2), new int[1], 0, 1); });
			}
			Assert.Throw(() => { texture.SetData<Color>(null); });
		}
	}
}
