namespace Microsoft.Xna.Framework.Audio
{
	public static class AudioEmitterTest
	{
		[TestException]
		public static void DopplerScale()
		{
			Assert.Throw(() => { new AudioEmitter().DopplerScale = 0f; });
			Assert.Throw(() => { new AudioEmitter().DopplerScale = -float.Epsilon; });
			Assert.Throw(() => { new AudioEmitter().DopplerScale = float.Epsilon; });
		}

		[Test]
		public static void Default()
		{
			AudioEmitter emitter = new AudioEmitter();
			Assert.Result(emitter.DopplerScale);
			Assert.Result(emitter.Forward);
			Assert.Result(emitter.Position);
			Assert.Result(emitter.Velocity);
		}
	}
}
