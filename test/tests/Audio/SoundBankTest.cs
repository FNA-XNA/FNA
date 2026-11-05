namespace Microsoft.Xna.Framework.Audio
{
	public static class SoundBankTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new SoundBank(null, null).Dispose(); });
		}
	}
}
