namespace Microsoft.Xna.Framework
{
	public class Vector3Test
	{
		[Test]
		public static void Default()
		{
			Assert.Result(Vector3.Backward);
			Assert.Result(Vector3.Down);
			Assert.Result(Vector3.Forward);
			Assert.Result(Vector3.Left);
			Assert.Result(Vector3.One);
			Assert.Result(Vector3.Right);
			Assert.Result(Vector3.UnitX);
			Assert.Result(Vector3.UnitY);
			Assert.Result(Vector3.UnitZ);
			Assert.Result(Vector3.Up);
			Assert.Result(Vector3.Zero);
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Max(float value1, float value2)
		{
			Assert.Result(Vector3.Max(new Vector3(value1), new Vector3(value2)));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static void Min(float value1, float value2)
		{
			Assert.Result(Vector3.Min(new Vector3(value1), new Vector3(value2)));
		}
	}
}
