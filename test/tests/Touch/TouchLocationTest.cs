namespace Microsoft.Xna.Framework.Input.Touch
{
	public static class TouchLocationTest
	{
		[Test]
		public static void Test()
		{
			TouchLocation touchLocation = new TouchLocation(3, TouchLocationState.Pressed, new Vector2(1f, 1f));
			TouchLocation result;
			touchLocation.TryGetPreviousLocation(out result);
			Assert.Result(result.Position);
		}
	}
}
