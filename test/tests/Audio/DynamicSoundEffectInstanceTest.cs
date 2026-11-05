namespace Microsoft.Xna.Framework.Audio
{
	public static class DynamicSoundEffectInstanceTest
	{
		[TestException]
		public static void DynamicSoundEffectInstanceCtor()
		{
			Assert.Throw(() => { new DynamicSoundEffectInstance(1999, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(44100, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(44100, (AudioChannels) 3).Dispose(); });
		}
	}
}
