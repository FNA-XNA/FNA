using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	public static class XactTest
	{
		[TestException]
		public static void AudioEngineCtor()
		{
			Assert.Throw(() => { new AudioEngine(null).Dispose(); });
			Assert.Throw(() => { new AudioEngine("").Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'X', (byte) 'G', (byte) 'S' });
			Assert.Throw(() => { new AudioEngine(Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'X', (byte) 'G', (byte) 'S', (byte) 'G' });
			Assert.Throw(() => { new AudioEngine(Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'X', (byte) 'G', (byte) 'S', (byte) 'F' });
			Assert.Throw(() => { new AudioEngine(Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'X', (byte) 'G', (byte) 'S', (byte) 'F', 0 });
			Assert.Throw(() => { new AudioEngine(Assert.TMP).Dispose(); });
		}

		[TestException]
		public static void WaveBankCtor()
		{
			AudioEngine audioEngine = new AudioEngine("Content/Audio Engine.xgs");

			Assert.Throw(() => { new WaveBank(null, null).Dispose(); });
			Assert.Throw(() => { new WaveBank(audioEngine, null).Dispose(); });
			Assert.Throw(() => { new WaveBank(audioEngine, "").Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'E' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'D' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'D', 0 });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP).Dispose(); });

			Assert.Throw(() => { new WaveBank(null, null, 0, 0).Dispose(); });
			Assert.Throw(() => { new WaveBank(audioEngine, null, 0, 0).Dispose(); });
			Assert.Throw(() => { new WaveBank(audioEngine, "", 0, 0).Dispose(); });
			File.WriteAllBytes(Assert.TMP + 1, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP + 1, 0, 0).Dispose(); });
			File.WriteAllBytes(Assert.TMP + 2, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'E' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP + 2, 0, 0).Dispose(); });
			File.WriteAllBytes(Assert.TMP + 3, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'D' });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP + 3, 0, 0).Dispose(); });
			File.WriteAllBytes(Assert.TMP + 4, new byte[] { (byte) 'W', (byte) 'B', (byte) 'N', (byte) 'D', 0 });
			Assert.Throw(() => { new WaveBank(audioEngine, Assert.TMP + 4, 0, 0).Dispose(); });

			audioEngine.Dispose();
		}

		[TestException]
		public static void SoundBankCtor()
		{
			AudioEngine audioEngine = new AudioEngine("Content/Audio Engine.xgs");
			Assert.Throw(() => { new SoundBank(null, null).Dispose(); });
			Assert.Throw(() => { new SoundBank(audioEngine, null).Dispose(); });
			Assert.Throw(() => { new SoundBank(audioEngine, "").Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'S', (byte) 'D', (byte) 'B' });
			Assert.Throw(() => { new SoundBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'S', (byte) 'D', (byte) 'B', (byte) 'L' });
			Assert.Throw(() => { new SoundBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'S', (byte) 'D', (byte) 'B', (byte) 'K' });
			Assert.Throw(() => { new SoundBank(audioEngine, Assert.TMP).Dispose(); });
			File.WriteAllBytes(Assert.TMP, new byte[] { (byte) 'S', (byte) 'D', (byte) 'B', (byte) 'K', 0 });
			Assert.Throw(() => { new SoundBank(audioEngine, Assert.TMP).Dispose(); });
			audioEngine.Dispose();
		}

		[TestException]
		public static void TestException()
		{
			AudioEngine audioEngine = new AudioEngine("Content/Audio Engine.xgs");
			WaveBank waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
			SoundBank soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
			Cue cue = soundBank.GetCue("New Cue");

			Assert.Throw(() => { cue.Stop((AudioStopOptions) 2); });

			Assert.Throw(() => { soundBank.GetCue(null); });
			Assert.Throw(() => { soundBank.GetCue(""); });
			Assert.Throw(() => { soundBank.GetCue("a"); });
			Assert.Throw(() => { soundBank.PlayCue(null); });
			Assert.Throw(() => { soundBank.PlayCue(""); });
			Assert.Throw(() => { soundBank.PlayCue("a"); });

			Assert.Throw(() => { audioEngine.GetCategory(null); });
			Assert.Throw(() => { audioEngine.GetCategory(""); });
			Assert.Throw(() => { audioEngine.GetCategory("a"); });

			Assert.Throw(() => { cue.GetVariable(null); });
			Assert.Throw(() => { cue.GetVariable(""); });
			Assert.Throw(() => { cue.GetVariable("a"); });
			Assert.Throw(() => { cue.SetVariable(null, 0f); });
			Assert.Throw(() => { cue.SetVariable("", 0f); });
			Assert.Throw(() => { cue.SetVariable("a", 0f); });

			Assert.Throw(() => { audioEngine.GetGlobalVariable(null); });
			Assert.Throw(() => { audioEngine.GetGlobalVariable(""); });
			Assert.Throw(() => { audioEngine.GetGlobalVariable("a"); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable(null, 0f); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable("", 0f); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable("a", 0f); });

			cue.Dispose();
			soundBank.Dispose();
			waveBank.Dispose();
			audioEngine.Dispose();

			Assert.Throw(() => { cue.Play(); });
			Assert.Throw(() => { cue.Pause(); });
			Assert.Throw(() => { cue.Resume(); });
			Assert.Throw(() => { cue.Stop((AudioStopOptions) (-1)); });
			Assert.Throw(() => { cue.Stop((AudioStopOptions) 2); });

			Assert.Throw(() => { soundBank.GetCue(null); });
			Assert.Throw(() => { soundBank.GetCue(""); });
			Assert.Throw(() => { soundBank.GetCue("New Cue"); });
			Assert.Throw(() => { soundBank.PlayCue(null); });
			Assert.Throw(() => { soundBank.PlayCue(""); });
			Assert.Throw(() => { soundBank.PlayCue("New Cue"); });

			Assert.Throw(() => { audioEngine.GetCategory(null); });
			Assert.Throw(() => { audioEngine.GetCategory(""); });
			Assert.Throw(() => { audioEngine.GetCategory("Music"); });

			Assert.Throw(() => { cue.GetVariable(null); });
			Assert.Throw(() => { cue.GetVariable(""); });
			Assert.Throw(() => { cue.GetVariable("Distance"); });
			Assert.Throw(() => { cue.SetVariable(null, 0f); });
			Assert.Throw(() => { cue.SetVariable("", 0f); });
			Assert.Throw(() => { cue.SetVariable("Distance", 0f); });

			Assert.Throw(() => { audioEngine.GetGlobalVariable(null); });
			Assert.Throw(() => { audioEngine.GetGlobalVariable(""); });
			Assert.Throw(() => { audioEngine.GetGlobalVariable("SpeedOfSound"); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable(null, 0f); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable("", 0f); });
			Assert.Throw(() => { audioEngine.SetGlobalVariable("SpeedOfSound", 0f); });
		}
	}
}
