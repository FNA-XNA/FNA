namespace Microsoft.Xna.Framework.Content
{
	public static class ContentSerializerCollectionItemNameAttributeTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new ContentSerializerCollectionItemNameAttribute(null); });
			Assert.Throw(() => { new ContentSerializerCollectionItemNameAttribute(""); });
		}
	}
}
