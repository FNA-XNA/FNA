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
			File.WriteAllBytes("tmp", new byte[] { (byte) 'F', (byte) 'S', (byte) 'G' });
			Assert.Throw(() => { new AudioEngine("tmp").Dispose(); });
		}
	}
}
