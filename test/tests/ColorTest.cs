using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework
{
	public static class ColorTest
	{
		[Argument(1.1f, -1.1f, 1.1f)]
		[Argument(0.49f, 0.47f, 0.49f)]
		public static Color Color1(float r, float g, float b)
		{
			return new Color(r, g, b);
		}
		[Argument(1.1f, -1.1f, 1.1f)]
		[Argument(0.49f, 0.47f, 0.49f)]
		public static Color Color2(float r, float g, float b)
		{
			return new Color(new Vector3(r, g, b));
		}

		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static Color Color1(float r, float g, float b, float a)
		{
			return new Color(r, g, b, a);
		}
		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static Color Color2(float r, float g, float b, float a)
		{
			return new Color(new Vector4(r, g, b, a));
		}
		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static Color Color3(float r, float g, float b, float a)
		{
			Color color = new Color();
			((IPackedVector) color).PackFromVector4(new Vector4(r, g, b, a));
			return color;
		}

		[Argument(255, 0.4f)]
		public static Color Multiply(int value, float scale)
		{
			return Color.Multiply(new Color(value, value, value), scale);
		}
	}
}

