namespace Microsoft.Xna.Framework.Media
{
	public static class VideoPlayerTest
	{
		[Test]
		public static void Default()
		{
			VideoPlayer videoPlayer = new VideoPlayer();
			Assert.Result(videoPlayer.IsLooped);
			Assert.Result(videoPlayer.IsMuted);
			Assert.Result(videoPlayer.Volume);
		}

		[TestException]
		public static void TestException()
		{
			VideoPlayer videoPlayer = new VideoPlayer();
			Assert.Throw(() => { videoPlayer.Volume = float.MinValue; });
			Assert.Throw(() => { videoPlayer.Volume = float.MaxValue; });
			Assert.Throw(() => { videoPlayer.Play(null); });
			videoPlayer.Dispose();
			Assert.Throw(() => { videoPlayer.IsLooped = false; });
			Assert.Throw(() => { videoPlayer.IsMuted = false; });
			Assert.Throw(() => { var _ = videoPlayer.PlayPosition; });
			Assert.Throw(() => { var _ = videoPlayer.State; });
			Assert.Throw(() => { videoPlayer.Volume = float.MinValue; });
			Assert.Throw(() => { videoPlayer.GetTexture(); });
			Assert.Throw(() => { videoPlayer.Pause(); });
			Assert.Throw(() => { videoPlayer.Play(null); });
			Assert.Throw(() => { videoPlayer.Resume(); });
			Assert.Throw(() => { videoPlayer.Stop(); });
		}
	}
}
