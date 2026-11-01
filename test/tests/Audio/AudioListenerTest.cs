namespace Microsoft.Xna.Framework.Audio
{
	public static class AudioListenerTest
	{
		[Test]
		public static void Default()
		{
			AudioListener audioListener = new AudioListener();
			Assert.Result(audioListener.Forward);
			Assert.Result(audioListener.Position);
			Assert.Result(audioListener.Up);
			Assert.Result(audioListener.Velocity);
		}
	}
}
