namespace Microsoft.Xna.Framework.Audio
{
	public static class WaveBankTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new WaveBank(null, null).Dispose(); });
			Assert.Throw(() => { new WaveBank(null, null, 0, 0).Dispose(); });
		}
	}
}
