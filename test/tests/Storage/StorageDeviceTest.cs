namespace Microsoft.Xna.Framework.Storage
{
	public static class StorageDeviceTest
	{
		[TestException]
		public static void BeginShowSelector()
		{
			Assert.Throw(() => { StorageDevice.BeginShowSelector((PlayerIndex) (-1), -1, 0, null, null); });
			Assert.Throw(() => { StorageDevice.BeginShowSelector(PlayerIndex.One, -1, 0, null, null); });
		}
	}
}
