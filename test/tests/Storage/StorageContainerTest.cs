using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
	public static class StorageContainerTest
	{
		[TestException]
		public static void TestException()
		{
			IAsyncResult asyncResult = StorageDevice.BeginShowSelector(PlayerIndex.One, 0, 0, null, null);
			StorageDevice storageDevice = StorageDevice.EndShowSelector(asyncResult);
			asyncResult = storageDevice.BeginOpenContainer("DisplayName", null, null);
			StorageContainer storageContainer = storageDevice.EndOpenContainer(asyncResult);

			Assert.Throw(() => { storageContainer.CreateDirectory(null); });
			Assert.Throw(() => { storageContainer.CreateDirectory(""); });
			Assert.Throw(() => { storageContainer.CreateDirectory("../"); });
			Assert.Throw(() => { storageContainer.CreateFile(null); });
			Assert.Throw(() => { storageContainer.CreateFile(""); });
			Assert.Throw(() => { storageContainer.CreateFile("../"); });
			Assert.Throw(() => { storageContainer.DeleteDirectory(null); });
			Assert.Throw(() => { storageContainer.DeleteDirectory(""); });
			Assert.Throw(() => { storageContainer.DeleteDirectory("../"); });
			Assert.Throw(() => { storageContainer.DeleteFile(null); });
			Assert.Throw(() => { storageContainer.DeleteFile(""); });
			Assert.Throw(() => { storageContainer.DeleteFile("../"); });
			Assert.Throw(() => { storageContainer.DirectoryExists(null); });
			Assert.Throw(() => { storageContainer.DirectoryExists(""); });
			Assert.Throw(() => { storageContainer.DirectoryExists("../"); });
			Assert.Throw(() => { storageContainer.FileExists(null); });
			Assert.Throw(() => { storageContainer.FileExists(""); });
			Assert.Throw(() => { storageContainer.FileExists("../"); });
			Assert.Throw(() => { storageContainer.OpenFile(null, FileMode.Open, FileAccess.Read, FileShare.None); });
			Assert.Throw(() => { storageContainer.OpenFile("", FileMode.Open, FileAccess.Read, FileShare.None); });
			Assert.Throw(() => { storageContainer.OpenFile("../", FileMode.Open, FileAccess.Read, FileShare.None); });
			storageContainer.Dispose();
			Assert.Throw(() => { var _ = storageContainer.DisplayName; });
			Assert.Throw(() => { var _ = storageContainer.StorageDevice; });
			Assert.Throw(() => { storageContainer.GetDirectoryNames(null); });
			Assert.Throw(() => { storageContainer.GetFileNames(null); });

			Assert.Throw(() => { storageContainer.CreateDirectory(null); });
			Assert.Throw(() => { storageContainer.CreateFile(null); });
			Assert.Throw(() => { storageContainer.DeleteDirectory(null); });
			Assert.Throw(() => { storageContainer.DeleteFile(null); });
			Assert.Throw(() => { storageContainer.DirectoryExists(null); });
			Assert.Throw(() => { storageContainer.FileExists(null); });
			Assert.Throw(() => { storageContainer.OpenFile(null, FileMode.Open, FileAccess.Read, FileShare.None); });
		}
	}
}
