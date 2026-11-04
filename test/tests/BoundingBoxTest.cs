namespace Microsoft.Xna.Framework
{
	public static class BoundingBoxTest
	{
		[TestException]
		public static void BoundingFrustum()
		{
			BoundingBox box = new BoundingBox();
			Assert.Throw(() => { box.Contains(null); });
			Assert.Throw(() => { box.Intersects(null); });
		}

		[TestException]
		public static void GetCorners()
		{
			Assert.Throw(() => { new BoundingBox().GetCorners(null); });
			Assert.Throw(() => { new BoundingBox().GetCorners(new Vector3[7]); });
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

		[Argument(float.NaN, float.NaN)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void CreateMerged(float f1, float f2)
		{
			Vector3 v1 = new Vector3(f1);
			Vector3 v2 = new Vector3(f2);
			BoundingBox box1 = new BoundingBox(v1, v2);
			BoundingBox box2 = new BoundingBox(v2, v1);
			Assert.Result(BoundingBox.CreateMerged(box1, box2));
			BoundingBox result;
			BoundingBox.CreateMerged(ref box1, ref box2, out result);
			Assert.Result(result);
		}
	}
}
