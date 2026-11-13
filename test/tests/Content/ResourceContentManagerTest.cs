using System;
using System.Resources;

namespace Microsoft.Xna.Framework.Content
{
	class MyResourceContentManager : ResourceContentManager
	{
		public MyResourceContentManager(IServiceProvider servicesProvider, ResourceManager resource) : base(servicesProvider, resource) { }
		internal new void OpenStream(string assetName)
		{
			base.OpenStream(assetName);
		}
	}
	public static class ResourceContentManagerTest
	{
		[TestException]
		public static void Ctor()
		{
			Assert.Throw(() => { new ResourceContentManager(null, null); });
			Assert.Throw(() => { new ResourceContentManager(new GameServiceContainer(), null); });
		}

		[TestException]
		public static void OpenStream()
		{
			MyResourceContentManager resourceContentManager = new MyResourceContentManager(new GameServiceContainer(), new ResourceManager(typeof(int)));
			Assert.Throw(() => { resourceContentManager.OpenStream("NotExist"); });
			resourceContentManager.Dispose();
		}
	}
}
