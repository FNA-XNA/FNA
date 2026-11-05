using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	public static class AudioEngineTest
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
	}
}
