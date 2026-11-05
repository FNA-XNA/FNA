using System;

namespace Microsoft.Xna.Framework.Audio
{
	public static class MicrophoneTest
	{
		[Test]
		public static void Default()
		{
			Microphone microphone = Microphone.Default;
			Assert.Result(microphone.BufferDuration);
			Assert.Result(microphone.IsHeadset);
			Assert.Result(microphone.SampleRate);
		}

		[TestException]
		public static void GetSampleDuration()
		{
			Microphone microphone = Microphone.Default;
			Assert.Throw(() => { microphone.GetSampleDuration(-1); });
			Assert.Throw(() => { microphone.GetSampleDuration(0); });
		}

		[TestException]
		public static void GetSampleSizeInBytes()
		{
			Microphone microphone = Microphone.Default;
			Assert.Throw(() => { microphone.GetSampleSizeInBytes(new TimeSpan(-1)); });
			Assert.Throw(() => { microphone.GetSampleSizeInBytes(new TimeSpan(0)); });
			long ticks = 0x80000000 * TimeSpan.TicksPerMillisecond / 2 * 1000 / 44100;
			Assert.Throw(() => { microphone.GetSampleSizeInBytes(new TimeSpan(ticks)); });
			Assert.Throw(() => { microphone.GetSampleSizeInBytes(new TimeSpan(ticks + 1)); });
		}

		[TestException]
		public static void TestException()
		{
			Microphone microphone = Microphone.Default;
			Assert.Throw(() => { microphone.GetData(null); });
			Assert.Throw(() => { microphone.GetData(new byte[0]); });
			Assert.Throw(() => { microphone.GetData(new byte[3]); });

			Assert.Throw(() => { microphone.GetData(null, -1, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[0], -1, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[3], -1, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[4], -1, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[4], 3, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[4], 4, 0); });
			Assert.Throw(() => { microphone.GetData(new byte[4], 0, 3); });
			Assert.Throw(() => { microphone.GetData(new byte[4], 0, 5); });
		}
	}
}
