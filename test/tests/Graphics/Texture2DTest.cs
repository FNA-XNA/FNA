using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public static class Texture2DTest
	{
		class NoSeekStream : Stream
		{
			public override bool CanRead => throw new System.NotImplementedException();
			public override bool CanSeek => false;
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
		public static void Texture2Dctor()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { new Texture2D(null, 0, 0); });
			Assert.Throw(() => { new Texture2D(gd, 0, 1); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0); });

			Assert.Throw(() => { new Texture2D(null, 0, 0, false, SurfaceFormat.Dxt1); });
			Assert.Throw(() => { new Texture2D(gd, 0, 0, false, SurfaceFormat.Dxt1); });
			Assert.Throw(() => { new Texture2D(gd, 1, 0, false, SurfaceFormat.Dxt1); });
			Assert.Throw(() => { new Texture2D(gd, 1, 1, false, SurfaceFormat.Dxt1); });
		}

		[TestException]
		public static void FromStream()
		{
			GraphicsDevice gd = GameTest.gdm.GraphicsDevice;
			Assert.Throw(() => { Texture2D.FromStream(null, null); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new NoSeekStream()); });

			Assert.Throw(() => { Texture2D.FromStream(null, null); });
			Assert.Throw(() => { Texture2D.FromStream(gd, null, 0, 0, false); });
			Assert.Throw(() => { Texture2D.FromStream(gd, new NoSeekStream(), 0, 0, false); });
		}
	}
}
