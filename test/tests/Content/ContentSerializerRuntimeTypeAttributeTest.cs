namespace Microsoft.Xna.Framework.Content
{
	public static class ContentSerializerRuntimeTypeAttributeTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new ContentSerializerRuntimeTypeAttribute(null); });
			Assert.Throw(() => { new ContentSerializerRuntimeTypeAttribute(""); });
		}
	}
}
