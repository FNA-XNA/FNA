namespace Microsoft.Xna.Framework.Content
{
	public static class ContentSerializerAttributeTest
	{
		[Test]
		public static void Default()
		{
			ContentSerializerAttribute contentSerializerAttribute = new ContentSerializerAttribute();
			Assert.Result(contentSerializerAttribute.AllowNull);
			Assert.Result(contentSerializerAttribute.CollectionItemName);
			Assert.Result(contentSerializerAttribute.ElementName);
			Assert.Result(contentSerializerAttribute.FlattenContent);
			Assert.Result(contentSerializerAttribute.Optional);
			Assert.Result(contentSerializerAttribute.SharedResource);
		}

		[TestException]
		public static void CollectionItemName()
		{
			ContentSerializerAttribute contentSerializerAttribute = new ContentSerializerAttribute();
			Assert.Throw(() => { contentSerializerAttribute.CollectionItemName = null; });
			Assert.Throw(() => { contentSerializerAttribute.CollectionItemName = ""; });
		}
	}
}
