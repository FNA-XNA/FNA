using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework
{
	public static class ColorTest
	{
		[Argument(1.1f, -1.1f, 1.1f)]
		[Argument(0.49f, 0.47f, 0.49f)]
		public static void Ctor(float r, float g, float b)
		{
			Assert.Result(new Color(r, g, b));
			Assert.Result(new Color(new Vector3(r, g, b)));
		}

		[Argument(1.1f, -1.1f, 0.49f, 0.47f)]
		public static void Ctor(float r, float g, float b, float a)
		{
			Assert.Result(new Color(r, g, b, a));
			Vector4 vector4 = new Vector4(r, g, b, a);
			Assert.Result(new Color(vector4));
			IPackedVector color = new Color();
			color.PackFromVector4(vector4);
			Assert.Result(color);
		}

		[Argument(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
		public static void FromNonPremultiplied(int r, int g, int b, int a)
		{
			Assert.Result(Color.FromNonPremultiplied(r, g, b, a));
		}

		[Test]
		public static void Lerp()
		{
			Assert.Result(Color.Lerp(new Color(), new Color(5, 10, 15, 20), 0.4f));
		}

		[Argument(255, 0.4f)]
		public static void Multiply(int value, float scale)
		{
			Assert.Result(Color.Multiply(new Color(value, value, value), scale));
		}
	}
}

