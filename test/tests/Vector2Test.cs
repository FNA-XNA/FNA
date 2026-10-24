namespace Microsoft.Xna.Framework
{
	public class Vector2Test
	{
		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector2 Max(float value1, float value2)
		{
			return Vector2.Max(new Vector2(value1), new Vector2(value2));
		}

		[Argument(1f, 2f)]
		[Argument(float.NaN, 1f)]
		[Argument(1f, float.NaN)]
		public static Vector2 Min(float value1, float value2)
		{
			return Vector2.Min(new Vector2(value1), new Vector2(value2));
		}
	}
}
