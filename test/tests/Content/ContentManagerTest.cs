using System;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
	public static class ContentManagerTest
	{
		class MyContentManager : ContentManager
		{
			public MyContentManager(IServiceProvider serviceProvider) : base(serviceProvider) { }
			internal Stream MyOpenStream(string assetName)
			{
				return base.OpenStream(assetName);
			}
			internal void ReadAsset(string assetName, Action<IDisposable> recordDisposableObject)
			{
				base.ReadAsset<int>(assetName, recordDisposableObject);
			}
		}

		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new ContentManager(null); });
			Assert.Throw(() => { new ContentManager(null, null); });
			Assert.Throw(() => { new ContentManager(new GameServiceContainer(), null); });
		}

		[Test]
		public static void Default()
		{
			ContentManager contentManager = new ContentManager(new GameServiceContainer());
			Assert.Result(contentManager.RootDirectory);
			contentManager.Dispose();
		}

		[TestException]
		public static void Load()
		{
			MyContentManager contentManager = new MyContentManager(new GameServiceContainer());
			Assert.Throw(() => { contentManager.Load<int>(null); });
			Assert.Throw(() => { contentManager.Load<int>(""); });
			contentManager.Dispose();
			Assert.Throw(() => { contentManager.Load<int>(null); });
		}

		[TestException]
		public static void OpenStream()
		{
			MyContentManager contentManager = new MyContentManager(new GameServiceContainer());
			Assert.Throw(() => { contentManager.MyOpenStream(null); });
			Assert.Throw(() => { contentManager.MyOpenStream("NotExist"); });
			contentManager.Dispose();
		}

		[TestException]
		public static void ReadAsset()
		{
			MyContentManager contentManager = new MyContentManager(new GameServiceContainer());
			Assert.Throw(() => { contentManager.ReadAsset(null, null); });
			Assert.Throw(() => { contentManager.ReadAsset("", null); });
			contentManager.Dispose();
			Assert.Throw(() => { contentManager.ReadAsset(null, null); });
		}

		[TestException]
		public static void RootDirectory()
		{
			ContentManager contentManager = new ContentManager(new GameServiceContainer());
			Assert.Throw(() => { contentManager.RootDirectory = null; });
			contentManager.Dispose();
		}
	}
}
