using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
	public static class StorageDeviceTest
	{
		[TestException]
		public static void TestException()
		{
			Assert.Throw(() => { StorageDevice.BeginShowSelector((PlayerIndex) (-1), -1, 0, null, null); });
			Assert.Throw(() => { StorageDevice.BeginShowSelector(PlayerIndex.One, -1, 0, null, null); });

			Assert.Throw(() => { StorageDevice.EndShowSelector(null); });
			Assert.Throw(() => { StorageDevice.EndShowSelector(new Task(Action)); });

			IAsyncResult asyncResult1 = StorageDevice.BeginShowSelector(PlayerIndex.One, 0, 0, null, null);
			IAsyncResult asyncResult2 = StorageDevice.BeginShowSelector(PlayerIndex.One, 0, 0, null, null);
			IAsyncResult asyncResult3 = StorageDevice.BeginShowSelector(PlayerIndex.One, 0, 0, null, null);
			StorageDevice storageDevice1 = StorageDevice.EndShowSelector(asyncResult1);
			StorageDevice storageDevice2 = StorageDevice.EndShowSelector(asyncResult2);
			StorageDevice storageDevice3 = StorageDevice.EndShowSelector(asyncResult3);

			Assert.Throw(() => { StorageDevice.EndShowSelector(asyncResult1); });

			Assert.Throw(() => { storageDevice1.EndOpenContainer(null); });
			Assert.Throw(() => { storageDevice1.EndOpenContainer(new Task(Action)); });

			asyncResult1 = storageDevice1.BeginOpenContainer(null, null, null);
			asyncResult2 = storageDevice2.BeginOpenContainer("", null, null);
			asyncResult3 = storageDevice3.BeginOpenContainer("", null, null);

			Assert.Throw(() => { storageDevice2.EndOpenContainer(asyncResult3); });
			Assert.Throw(() => { storageDevice2.EndOpenContainer(asyncResult3); });

			Assert.Throw(() => { storageDevice1.EndOpenContainer(asyncResult1); });
			Assert.Throw(() => { storageDevice2.EndOpenContainer(asyncResult2); });
		}

		private static void Action() { }
	}
}
