namespace Microsoft.Xna.Framework.Audio
{
	public static class DynamicSoundEffectInstanceTest
	{
		[TestException]
		public static void DynamicSoundEffectInstanceCtor()
		{
			Assert.Throw(() => { new DynamicSoundEffectInstance(1999, AudioChannels.Mono).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(8000, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(8000, (AudioChannels) 3).Dispose(); });
		}
	}
}
