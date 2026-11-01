namespace Microsoft.Xna.Framework.Audio
{
	public static class AudioEmitterTest
	{
		[TestException]
		public static void DopplerScale()
		{
			Assert.Throw(() => { new AudioEmitter().DopplerScale = -float.Epsilon; });
			Assert.Throw(() => { new AudioEmitter().DopplerScale = 0; });
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
