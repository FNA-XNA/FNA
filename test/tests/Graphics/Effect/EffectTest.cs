namespace Microsoft.Xna.Framework.Graphics
{
	public static class EffectTest
	{
		class CloneEffect : Effect
		{
			internal CloneEffect(Effect cloneSource) : base(cloneSource) { }
		}

		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new Effect(null, new byte[0]).Dispose(); });
			Assert.Throw(() => { new Effect(null, new byte[1]).Dispose(); });
			Assert.Throw(() => { new Effect(null, new byte[4]).Dispose(); });

			Assert.Throw(() => { new CloneEffect(null).Dispose(); });
		}
	}
}
