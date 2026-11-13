namespace Microsoft.Xna.Framework.Audio
{
	public static class AudioCategoryTest
	{
		[TestException]
		public static void TestException()
		{
			AudioCategory audioCategory = new AudioCategory();
			Assert.Throw(() => { audioCategory.Pause(); });
			Assert.Throw(() => { audioCategory.Resume(); });
			Assert.Throw(() => { audioCategory.SetVolume(0f); });
			Assert.Throw(() => { audioCategory.SetVolume(-float.Epsilon); });
			Assert.Throw(() => { audioCategory.Stop((AudioStopOptions) 2); });
		}
	}
}
