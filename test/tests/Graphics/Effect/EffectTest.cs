namespace Microsoft.Xna.Framework.Graphics
{
	public static class EffectTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new Effect(null, new byte[0]).Dispose(); });
			Assert.Throw(() => { new Effect(null, new byte[1]).Dispose(); });
			Assert.Throw(() => { new Effect(null, new byte[4]).Dispose(); });
		}
	}
}
