using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework
{
	public static class ColorTest
	{
		[Argument(1.1f, -1.1f, 1.1f)]
		[Argument(0.49f, 0.47f, 0.49f)]
		public static void ColorCtor(float r, float g, float b)
		{
			Assert.Result(new Color(r, g, b));
			Assert.Result(new Color(new Vector3(r, g, b)));
		}

		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static void ColorCtor(float r, float g, float b, float a)
		{
			Assert.Result(new Color(r, g, b, a));
			Assert.Result(new Color(new Vector4(r, g, b, a)));
		}

		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static void PackFromVector4(float r, float g, float b, float a)
		{
			Color color = new Color();
			((IPackedVector) color).PackFromVector4(new Vector4(r, g, b, a));
			Assert.Result(color);
		}

		[Argument(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
		public static void FromNonPremultiplied(int r, int g, int b, int a)
		{
			Assert.Result(Color.FromNonPremultiplied(r, g, b, a));
		}

		[Argument(255, 0.4f)]
		public static void Multiply(int value, float scale)
		{
			Assert.Result(Color.Multiply(new Color(value, value, value), scale));
		}
	}
}

