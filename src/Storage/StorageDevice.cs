#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
#endregion

namespace Microsoft.Xna.Framework.Storage
{
	/// <summary>
	/// Exposes a storage device for storing user data.
	/// </summary>
	/// <remarks>
	/// MSDN documentation contains related conceptual article:
	/// http://msdn.microsoft.com/en-us/library/bb200105.aspx
	/// </remarks>
	public sealed class StorageDevice
	{
		#region Public Properties

		/// <summary>
		/// Returns the amount of free space.
		/// </summary>
		public long FreeSpace
		{
			get
			{
				try
				{
					return new DriveInfo(storageRoot).AvailableFreeSpace;
				}
				catch(Exception e)
				{
					// Storage root was invalid or unavailable.
					throw new StorageDeviceNotConnectedException(
						"The storage device bound to the container is not connected.",
						e
					);
				}
			}
		}

		/// <summary>
		/// Returns true if this StorageDevice path is accessible, false otherwise.
		/// </summary>
		public bool IsConnected
		{
			get
			{
				if (	Game.Instance.Platform.OSVersion.Equals("Linux") ||
					Game.Instance.Platform.OSVersion.Equals("Mac OS X")	)
				{
					/* Linux and Mac use locally connected storage in the user's
					 * home location, which should always be "connected".
					 */
					return true;
				}
				else if (Game.Instance.Platform.OSVersion.Equals("Windows"))
				{
					try
					{
						return new DriveInfo(storageRoot).IsReady;
					}
					catch
					{
						// The storageRoot path is invalid / has been removed.
						return false;
					}
				}
				throw new Exception("StorageDevice: Platform.OSVersion not handled!");
			}
		}

		/// <summary>
		/// Returns the total size of device.
		/// </summary>
		public long TotalSpace
		{
			get
			{
				try
				{
					return new DriveInfo(storageRoot).TotalSize;
				}
				catch(Exception e)
				{
					// Storage root was invalid or unavailable.
					throw new StorageDeviceNotConnectedException(
						"The storage device bound to the container is not connected.",
						e
					);
				}
			}
		}

		#endregion

		#region Private Variables

		private PlayerIndex? devicePlayer;

		private StorageContainer deviceContainer;

		#endregion

		#region Private Static Variables

		private static readonly string storageRoot = GetStorageRoot();

		#endregion

		#region Events

		/// <summary>
		/// Fired when a device is removed or inserted.
		/// </summary>
		public static event EventHandler<EventArgs> DeviceChanged;

		private void OnDeviceChanged()
		{
			if (DeviceChanged != null)
			{
				DeviceChanged(this, null);
			}
		}

		#endregion

		#region Private Delegates

		private delegate StorageDevice ShowSelectorAsynchronous(
			PlayerIndex? player,
			int sizeInBytes,
			int directoryCount
		);

		private delegate StorageContainer OpenContainerAsynchronous(string displayName);

		#endregion

		#region Internal Constructors

		/// <summary>
		/// Creates a new <see cref="StorageDevice"/> instance.
		/// </summary>
		/// <param name="player">The playerIndex of the player.</param>
		/// <param name="sizeInBytes">Size of the storage device.</param>
		/// <param name="directoryCount"></param>
		internal StorageDevice(PlayerIndex? player, int sizeInBytes, int directoryCount)
		{
			devicePlayer = player;
		}

		#endregion

		#region Public OpenContainer Methods

		/// <summary>
		/// Begins the open for a StorageContainer.
		/// </summary>
		/// <returns>The open StorageContainer.</returns>
		/// <param name="displayName">Name of file.</param>
		/// <param name="callback">Method to call on completion.</param>
		/// <param name="state">Request identifier object for callback (can be null).</param>
		public IAsyncResult BeginOpenContainer(
			string displayName,
			AsyncCallback callback,
			object state
		) {
			try
			{
				OpenContainerAsynchronous AsynchronousOpen = new OpenContainerAsynchronous(Open);
				return AsynchronousOpen.BeginInvoke(displayName, callback, state);
			}
			finally
			{
				// TODO:  No resources to clean up?  Remove this finally block?
			}
		}

		/// <summary>
		/// Ends the open container process.
		/// </summary>
		/// <returns>The open StorageContainer.</returns>
		/// <param name="result">Result of BeginOpenContainer.</param>
		public StorageContainer EndOpenContainer(IAsyncResult result)
		{
			StorageContainer returnValue = null;
			try
			{
				// Retrieve the delegate.
				AsyncResult asyncResult = result as AsyncResult;
				if (asyncResult != null)
				{
					OpenContainerAsynchronous asyncDelegate = asyncResult.AsyncDelegate
						as OpenContainerAsynchronous;

					// Wait for the WaitHandle to become signaled.
					result.AsyncWaitHandle.WaitOne();

					// Call EndInvoke to retrieve the results.
					if (asyncDelegate != null)
					{
						returnValue = asyncDelegate.EndInvoke(result);
					}
				}
			}
			finally
			{
				// Close the wait handle.
				result.AsyncWaitHandle.Dispose();
			}

			return returnValue;
		}

		#endregion

		#region Public ShowSelector Methods

		/// <summary>
		/// Begin process to display the StorageDevice selector UI.
		/// </summary>
		/// <returns>The show selector.</returns>
		/// <param name="callback">Method to invoke when device is selected by player.</param>
		/// <param name="state">Request identifier object for callback (can be null).</param>
		public static IAsyncResult BeginShowSelector(
			AsyncCallback callback,
			object state
		) {
			return BeginShowSelector(
				0,
				0,
				callback,
				state
			);
		}

		/// <summary>
		/// Begin process to display the StorageDevice selector UI.
		/// </summary>
		/// <returns>The show selector.</returns>
		/// <param name="player">The PlayerIndex. Only PlayerIndex.One is valid on Windows.</param>
		/// <param name="callback">Method to invoke when device is selected by player.</param>
		/// <param name="state">Request identifier object for callback (can be null).</param>
		public static IAsyncResult BeginShowSelector(
			PlayerIndex player,
			AsyncCallback callback,
			object state
		) {
			return BeginShowSelector(
				player,
				0,
				0,
				callback,
				state
			);
		}

		/// <summary>
		/// Begin process to display the StorageDevice selector UI.
		/// </summary>
		/// <returns>The show selector.</returns>
		/// <param name="sizeInBytes">Size (in bytes) of data to write.</param>
		/// <param name="directoryCount">Number of directories to write.</param>
		/// <param name="callback">Method to invoke when device is selected by player.</param>
		/// <param name="state">Request identifier object for callback (can be null).</param>
		public static IAsyncResult BeginShowSelector(
			int sizeInBytes,
			int directoryCount,
			AsyncCallback callback,
			object state
		) {
			ShowSelectorAsynchronous del = new ShowSelectorAsynchronous(Show);
			return del.BeginInvoke(null, sizeInBytes, directoryCount, callback, state);
		}

		/// <summary>
		/// Begin process to display the StorageDevice selector UI.
		/// </summary>
		/// <returns>The show selector.</returns>
		/// <param name="player">The PlayerIndex. Only PlayerIndex.One is valid on Windows.</param>
		/// <param name="sizeInBytes">Size (in bytes) of data to write.</param>
		/// <param name="directoryCount">Number of directories to write.</param>
		/// <param name="callback">Method to invoke when device is selected by player.</param>
		/// <param name="state">Request identifier object for callback (can be null).</param>
		public static IAsyncResult BeginShowSelector(
			PlayerIndex player,
			int sizeInBytes,
			int directoryCount,
			AsyncCallback callback,
			object state
		) {
			ShowSelectorAsynchronous del = new ShowSelectorAsynchronous(Show);
			return del.BeginInvoke(player, sizeInBytes, directoryCount, callback, state);
		}

		/// <summary>
		/// Ends the show selector user interface display.
		/// </summary>
		/// <returns>The storage device.</returns>
		/// <param name="result">The result of BeginShowSelector.</param>
		public static StorageDevice EndShowSelector(IAsyncResult result)
		{
			if (!result.IsCompleted)
			{
				// Wait for the WaitHandle to become signaled.
				try
				{
					result.AsyncWaitHandle.WaitOne();
				}
				finally
				{
				}
			}

			// Retrieve the delegate.
			AsyncResult asyncResult = (AsyncResult) result;

			ShowSelectorAsynchronous del = asyncResult.AsyncDelegate as ShowSelectorAsynchronous;

			if (del != null)
			{
				return del.EndInvoke(result);
			}

			throw new ArgumentException("result");
		}

		#endregion

		#region Public StorageContainer Delete Method

		public void DeleteContainer(string titleName)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Private OpenContainer Async Method

		// Private method to handle the creation of the StorageDevice.
		private StorageContainer Open(string displayName)
		{
			deviceContainer = new StorageContainer(
				this,
				displayName,
				storageRoot,
				devicePlayer
			);
			return deviceContainer;
		}

		#endregion

		#region Private ShowSelector Async Method

		// Private method to handle the creation of the StorageDevice.
		private static StorageDevice Show(PlayerIndex? player, int sizeInBytes, int directoryCount)
		{
			return new StorageDevice(player, sizeInBytes, directoryCount);
		}

		#endregion

		#region Private Static OS User Directory Path Method

		private static string GetStorageRoot()
		{
			if (Game.Instance.Platform.OSVersion.Equals("Windows"))
			{
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"SavedGames"
				);
			}
			if (Game.Instance.Platform.OSVersion.Equals("Mac OS X"))
			{
				string osConfigDir = Environment.GetEnvironmentVariable("HOME");
				if (String.IsNullOrEmpty(osConfigDir))
				{
					return "."; // Oh well.
				}
				osConfigDir += "/Library/Application Support";
				return osConfigDir;
			}
			if (Game.Instance.Platform.OSVersion.Equals("Linux"))
			{
				// Assuming a non-OSX Unix platform will follow the XDG. Which it should.
				string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (String.IsNullOrEmpty(osConfigDir))
				{
					osConfigDir = Environment.GetEnvironmentVariable("HOME");
					if (String.IsNullOrEmpty(osConfigDir))
					{
						return ".";	// Oh well.
					}
					osConfigDir += "/.local/share";
				}
				return osConfigDir;
			}
			throw new Exception("StorageDevice: Platform.OSVersion not handled!");
		}

		#endregion
	}
}
