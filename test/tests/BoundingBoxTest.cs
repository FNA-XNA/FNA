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

		[Test]
		public static void Contains()
		{
			Vector3 vector3;
			ContainmentType containmentType;
			BoundingBox box1, box2;

			vector3 = new Vector3(float.NaN);
			box1 = new BoundingBox(vector3, vector3);
			box2 = new BoundingBox(vector3, vector3);
			Assert.Result(box1.Contains(box2));
			box1.Contains(ref box2, out containmentType);
			Assert.Result(containmentType);
		}
	}
}
