namespace Microsoft.Xna.Framework
{
	public static class RectangleTest
	{
		[Argument(1, 2, 3, 4, 5, 6, 7, 8)]
		public static Rectangle Intersect(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
		{
			return Rectangle.Intersect(new Rectangle(x1, y1, width1, height1), new Rectangle(x2, y2, width2, height2));
		}

		[Argument(1, 2, 3, 4, 5, 6, 7, 8)]
		public static Rectangle IntersectRef(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
		{
			Rectangle r1 = new Rectangle(x1, y1, width1, height1);
			Rectangle r2 = new Rectangle(x2, y2, width2, height2);
			Rectangle result;
			Rectangle.Intersect(ref r1, ref r2, out result);
			return result;
		}

		[Argument(1, 2, 3, 4, 5, 6, 7, 8)]
		public static Rectangle Union(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
		{
			return Rectangle.Union(new Rectangle(x1, y1, width1, height1), new Rectangle(x2, y2, width2, height2));
		}

		[Argument(1, 2, 3, 4, 5, 6, 7, 8)]
		public static Rectangle UnionRef(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
		{
			Rectangle r1 = new Rectangle(x1, y1, width1, height1);
			Rectangle r2 = new Rectangle(x2, y2, width2, height2);
			Rectangle result;
			Rectangle.Union(ref r1, ref r2, out result);
			return result;
		}
	}
}
