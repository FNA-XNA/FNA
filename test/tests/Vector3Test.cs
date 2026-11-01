namespace Microsoft.Xna.Framework
{
	public class Vector3Test
	{
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
