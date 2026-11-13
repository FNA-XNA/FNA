using System;

namespace Microsoft.Xna.Framework
{
	public static class GameTimeTest
	{
		[Test]
		public static void Default()
		{
			GameTime gameTime = new GameTime();
			Assert.Result(gameTime.ElapsedGameTime);
			Assert.Result(gameTime.IsRunningSlowly);
			Assert.Result(gameTime.TotalGameTime);
			gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
			Assert.Result(gameTime.IsRunningSlowly);
		}
	}
}
