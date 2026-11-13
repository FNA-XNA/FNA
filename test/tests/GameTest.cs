namespace Microsoft.Xna.Framework
{
	public static class GameTest
	{
		internal static Game game = new Game();
		internal static GraphicsDeviceManager gdm = new GraphicsDeviceManager(game) { GraphicsProfile = Graphics.GraphicsProfile.HiDef };
		static GameTest()
		{
			((IGraphicsDeviceManager) gdm).CreateDevice();
		}

		[TestException]
		public static void Time()
		{
			Assert.Throw(() => { game.InactiveSleepTime = new System.TimeSpan(-1); });
			Assert.Throw(() => { game.TargetElapsedTime = new System.TimeSpan(0); });
		}

		[TestException]
		public static void Content()
		{
			Assert.Throw(() => { game.Content = null; });
		}
	}
}
