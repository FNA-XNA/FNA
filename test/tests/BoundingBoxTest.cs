namespace Microsoft.Xna.Framework
{
	public static class BoundingBoxTest
	{
		[TestException]
		public static void GetCorners()
		{
			Assert.Throw(() => { new BoundingBox().GetCorners(null); });
			Assert.Throw(() => { new BoundingBox().GetCorners(new Vector3[0]); });
		}
	}
}
