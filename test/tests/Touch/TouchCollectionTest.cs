namespace Microsoft.Xna.Framework.Input.Touch
{
	public static class TouchCollectionTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new TouchCollection(null); });
			Assert.Throw(() => { new TouchCollection(new TouchLocation[7]); });
		}

		[TestException]
		public static void CopyTo()
		{
			TouchCollection touchLocations = new TouchCollection(new TouchLocation[8]);
			Assert.Throw(() => { touchLocations.CopyTo(null, -1); });
		}
	}
}
