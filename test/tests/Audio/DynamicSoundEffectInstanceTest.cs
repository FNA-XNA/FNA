using System;

namespace Microsoft.Xna.Framework.Audio
{
	public static class DynamicSoundEffectInstanceTest
	{
		[Test]
		public static void Default()
		{
			DynamicSoundEffectInstance dynamicSoundEffectInstance = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
			Assert.Result(dynamicSoundEffectInstance.IsLooped);
			Assert.Result(dynamicSoundEffectInstance.Pan);
			Assert.Result(dynamicSoundEffectInstance.Pitch);
			Assert.Result(dynamicSoundEffectInstance.Volume);
			dynamicSoundEffectInstance.Dispose();
		}

		[TestException]
		public static void TestException()
		{
			DynamicSoundEffectInstance dynamicSoundEffectInstance = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
			Assert.Throw(() => { dynamicSoundEffectInstance.IsLooped = true; });
			dynamicSoundEffectInstance.Dispose();
			Assert.Throw(()=> { var _ = dynamicSoundEffectInstance.IsLooped; });
			Assert.Throw(() => { dynamicSoundEffectInstance.IsLooped = true; });
			Assert.Throw(() => { var _ = dynamicSoundEffectInstance.PendingBufferCount; });
			Assert.Throw(() => { dynamicSoundEffectInstance.Pan = -2f; });
			Assert.Throw(() => { dynamicSoundEffectInstance.Pitch = -2f; });
			Assert.Throw(() => { var _ = dynamicSoundEffectInstance.State; });
			Assert.Throw(() => { dynamicSoundEffectInstance.Volume = -2f; });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleDuration(0); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleDuration(0); });
			Assert.Throw(() => { dynamicSoundEffectInstance.Apply3D((AudioListener) null, null); });
			Assert.Throw(() => { dynamicSoundEffectInstance.Pause(); });
			Assert.Throw(() => { dynamicSoundEffectInstance.Play(); });
			Assert.Throw(() => { dynamicSoundEffectInstance.Resume(); });
			Assert.Throw(() => { dynamicSoundEffectInstance.Stop(); });
		}

		[TestException]
		public static void DynamicSoundEffectInstanceCtor()
		{
			Assert.Throw(() => { new DynamicSoundEffectInstance(999, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(200001, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(44100, 0).Dispose(); });
			Assert.Throw(() => { new DynamicSoundEffectInstance(44100, (AudioChannels) 3).Dispose(); });
		}

		[TestException]
		public static void GetSampleDuration()
		{
			DynamicSoundEffectInstance dynamicSoundEffectInstance = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleDuration(-1); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleDuration(0); });
			dynamicSoundEffectInstance.Dispose();
		}

		[TestException]
		public static void GetSampleSizeInBytes()
		{
			const long ticks = 0x80000000 * TimeSpan.TicksPerMillisecond / 2 * 1000 / 44100;
			DynamicSoundEffectInstance dynamicSoundEffectInstance = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleSizeInBytes(new TimeSpan(-1)); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleSizeInBytes(new TimeSpan(0)); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleSizeInBytes(new TimeSpan(1)); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleSizeInBytes(new TimeSpan(ticks)); });
			Assert.Throw(() => { dynamicSoundEffectInstance.GetSampleSizeInBytes(new TimeSpan(ticks + 1)); });
			dynamicSoundEffectInstance.Dispose();
		}
	}
}
