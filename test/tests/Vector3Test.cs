namespace Microsoft.Xna.Framework
{
	public class Vector3Test
	{
		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector3 Max(float value1, float value2)
		{
			return Vector3.Max(new Vector3(value1), new Vector3(value2));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector3 Min(float value1, float value2)
		{
			return Vector3.Min(new Vector3(value1), new Vector3(value2));
		}
	}
}
