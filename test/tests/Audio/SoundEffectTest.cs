using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	public static class SoundEffectTest
	{
		[TestException]
		public static void SoundEffectCtor()
		{
			//Assert.Throw(() => { new SoundEffect(null, 0, (AudioChannels) (-1)).Dispose(); });
			//Assert.Throw(() => { new SoundEffect(null, 0, 0, 0, (AudioChannels) (-1), 0, 0).Dispose(); });
		}

		[Test]
		public static void Default()
		{
			Assert.Result(SoundEffect.DistanceScale);
			Assert.Result(SoundEffect.DopplerScale);
			Assert.Result(SoundEffect.MasterVolume);
			Assert.Result(SoundEffect.SpeedOfSound);
		}

		[TestException]
		public static void DistanceScale()
		{
			float DistanceScale = SoundEffect.DistanceScale;
			Assert.Throw(() => { SoundEffect.DistanceScale = float.NaN; });
			Assert.Throw(() => { SoundEffect.DistanceScale = 0f; });
			Assert.Throw(() => { SoundEffect.DistanceScale = -float.Epsilon; });
			SoundEffect.DistanceScale = DistanceScale;
		}

		[TestException]
		public static void DopplerScale()
		{
			float DopplerScale = SoundEffect.DopplerScale;
			Assert.Throw(() => { SoundEffect.DopplerScale = float.NaN; });
			Assert.Throw(() => { SoundEffect.DopplerScale = 0f; });
			Assert.Throw(() => { SoundEffect.DopplerScale = -float.Epsilon; });
			SoundEffect.DopplerScale = DopplerScale;
		}

		[TestException]
		public static void MasterVolume()
		{
			float MasterVolume = SoundEffect.MasterVolume;
			Assert.Throw(() => { SoundEffect.MasterVolume = float.NaN; });
			SoundEffect.MasterVolume = MasterVolume;
		}

		[TestException]
		public static void SpeedOfSound()
		{
			float SpeedOfSound = SoundEffect.SpeedOfSound;
			Assert.Throw(() => { SoundEffect.SpeedOfSound = float.NaN; });
			Assert.Throw(() => { SoundEffect.SpeedOfSound = 0f; });
			Assert.Throw(() => { SoundEffect.SpeedOfSound = float.Epsilon; });
			SoundEffect.SpeedOfSound = SpeedOfSound;
		}

		[TestException]
		public static void FromStream()
		{
			Assert.Throw(() => { SoundEffect.FromStream(null); });
			Assert.Throw(() => { SoundEffect.FromStream(new MemoryStream()); });
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'G' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'F' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'F', 'G' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'G' });
				writer.Write(32);
				writer.Write(new char[] { 'W', 'A', 'V', 'G' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'F' });
				writer.Write(32);
				writer.Write(new char[] { 'W', 'A', 'V', 'G' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'F' });
				writer.Write(4);
				writer.Write(new char[] { 'W', 'A', 'V', 'G' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				writer.Write(new char[] { 'R', 'I', 'F', 'F' });
				writer.Write(4);
				writer.Write(new char[] { 'W', 'A', 'V', 'E' });
				writer.BaseStream.Position = 0;
				Assert.Throw(() => { SoundEffect.FromStream(writer.BaseStream).Dispose(); });
			}
		}

		[TestException]
		public static void GetSampleDuration()
		{
			Assert.Throw(() => { SoundEffect.GetSampleDuration(-1, -1, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleDuration(0, -1, AudioChannels.Mono); });
			Assert.Throw(() => { SoundEffect.GetSampleDuration(0, 44100, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleDuration(0, 44100, (AudioChannels) 3); });
		}

		[TestException]
		public static void GetSampleSizeInBytes()
		{
			long ticks = 0x80000000 * TimeSpan.TicksPerMillisecond / 2 * 1000 / 44100;
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(-1), -1, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(0), -1, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(ticks), 44100, AudioChannels.Mono); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(ticks + 1), 44100, AudioChannels.Mono); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(1), 44100, AudioChannels.Mono); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(1), 0, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(1), 44100, 0); });
			Assert.Throw(() => { SoundEffect.GetSampleSizeInBytes(new TimeSpan(1), 44100, (AudioChannels) 3); });
		}
	}
}
