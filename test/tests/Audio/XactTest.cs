using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	public static class XactTest
	{
		[TestException]
		public static void Ctor()
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
