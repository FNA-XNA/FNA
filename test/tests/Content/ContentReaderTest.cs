using System;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content
{
	class NotMatchReader : ContentTypeReader
	{
		public NotMatchReader() : base(typeof(ContentSerializerIgnoreAttribute)) { }
		protected override object Read(ContentReader input, object existingInstance)
		{
			return new ContentSerializerIgnoreAttribute();
		}
	}

	class TestDataReader : ContentTypeReader
	{
		public TestDataReader() : base(typeof(int)) { }
		protected override object Read(ContentReader input, object existingInstance)
		{
			Assert.Throw(() => { input.ReadSharedResource<int>(null); });
			Assert.Throw(() => { input.ReadRawObject(new NotMatchReader(), new ContentSerializerIgnoreAttribute()); });
			return true;
		}
	}

	public static class ContentReaderTest
	{
		static ContentReaderTest()
		{
			AppDomain.CurrentDomain.TypeResolve += AppDomain_TypeResolve;
		}

		private static Assembly AppDomain_TypeResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name == "Microsoft.Xna.Framework.Content.TestDataReader")
			{
				return Assembly.GetExecutingAssembly();
			}
			return null;
		}

		[TestException]
		public static void TestException()
		{
			ContentManager contentManager = new ContentManager(new GameServiceContainer());
			Assert.Throw(() => { contentManager.Load<int>("Content/TestData"); });
		}
	}
}
