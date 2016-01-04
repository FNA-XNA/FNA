#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Storage
{
	/// <summary>
	/// Contains a logical collection of files used for user-data storage.
	/// </summary>
	/// <remarks>
	/// MSDN documentation contains related conceptual article:
	/// http://msdn.microsoft.com/en-us/library/bb200105.aspx#ID4EDB
	/// </remarks>
	public class StorageContainer : IDisposable
	{
		#region Public Properties

		/// <summary>
		/// The title's (i.e. "game's") filename.
		/// </summary>
		public string DisplayName
		{
			get;
			private set;
		}

		/// <summary>
		/// A bool value indicating whether the instance has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get;
			private set;
		}

		/// <summary>
		/// The <see cref="StorageDevice"/> that holds logical files for the container.
		/// </summary>
		public StorageDevice StorageDevice
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		private readonly string storagePath;

		#endregion

		#region Events

		/// <summary>
		/// Fired when <see cref="Dispose"/> is called or object is finalized or collected
		/// by the garbage collector.
		/// </summary>
		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Internal Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageContainer"/> class.
		/// </summary>
		/// <param name='device'>The attached storage-device.</param>
		/// <param name='name'>The title's filename.</param>
		/// <param name='rootPath'>The path of the storage root folder</param>
		/// <param name='playerIndex'>
		/// The index of the player whose data is being saved, or null if data is for all
		/// players.
		/// </param>
		internal StorageContainer(
			StorageDevice device,
			string name,
			string rootPath,
			PlayerIndex? playerIndex
		) {
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("A title name has to be provided in parameter name.");
			}

			StorageDevice = device;
			DisplayName = name;

			// Generate the path of the game's savefolder
			storagePath = Path.Combine(
				rootPath,
				Path.GetFileNameWithoutExtension(
					AppDomain.CurrentDomain.FriendlyName
				)
			);

			// Create the root folder for all titles, if needed.
			if (!Directory.Exists(storagePath))
			{
				Directory.CreateDirectory(storagePath);
			}

			storagePath = Path.Combine(storagePath, name);

			// Create the sub-folder for this container/title's files, if needed.
			if (!Directory.Exists(storagePath))
			{
				Directory.CreateDirectory(storagePath);
			}

			/* There are two types of subfolders within a StorageContainer.
			 * The first is a PlayerX folder, X being a specified PlayerIndex.
			 * The second is AllPlayers, when PlayerIndex is NOT specified.
			 * Basically, you should NEVER expect to have ANY file in the root
			 * game save folder.
			 * -flibit
			 */
			if (playerIndex.HasValue)
			{
				storagePath = Path.Combine(storagePath, "Player" + ((int) playerIndex.Value + 1).ToString());
			}
			else
			{
				storagePath = Path.Combine(storagePath, "AllPlayers");
			}

			// Create the player folder, if needed.
			if (!Directory.Exists(storagePath))
			{
				Directory.CreateDirectory(storagePath);
			}
		}

		#endregion

		#region Public Dispose Method

		/// <summary>
		/// Disposes un-managed objects referenced by this object.
		/// </summary>
		public void Dispose()
		{
			if (Disposing != null)
			{
				Disposing(this, null);
			}
			IsDisposed = true;
		}

		#endregion

		#region Public Create Methods

		/// <summary>
		/// Creates a new directory in the storage-container.
		/// </summary>
		/// <param name="directory">Relative path of the directory to be created.</param>
		public void CreateDirectory(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException("Parameter directory must contain a value.");
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(storagePath, directory);

			// Now let's try to create it.
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		/// <summary>
		/// Creates a file in the storage-container.
		/// </summary>
		/// <param name="file">Relative path of the file to be created.</param>
		/// <returns>Returns <see cref="Stream"/> for the created file.</returns>
		public Stream CreateFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("Parameter file must contain a value.");
			}

			// File name is relative, so combine with our path.
			string filePath = Path.Combine(storagePath, file);

			// Return a new file with read/write access.
			return File.Create(filePath);
		}

		#endregion

		#region Public Delete Methods

		/// <summary>
		/// Deletes specified directory from the storage-container.
		/// </summary>
		/// <param name="directory">The relative path of the directory to be deleted.</param>
		public void DeleteDirectory(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException("Parameter directory must contain a value.");
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(storagePath, directory);

			// Now let's try to delete it.
			Directory.Delete(dirPath);
		}

		/// <summary>
		/// Deletes a file from the storage-container.
		/// </summary>
		/// <param name="file">The relative path of the file to be deleted.</param>
		public void DeleteFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("Parameter file must contain a value.");
			}

			// Relative, so combine with our path.
			string filePath = Path.Combine(storagePath, file);

			// Now let's try to delete it.
			File.Delete(filePath);
		}

		#endregion

		#region Public Exists Methods

		/// <summary>
		/// Returns true if specified path exists in the storage-container, false otherwise.
		/// </summary>
		/// <param name="directory">The relative path of the directory to query for.</param>
		/// <returns>True if the directory path exists, false otherwise.</returns>
		public bool DirectoryExists(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException("Parameter directory must contain a value.");
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(storagePath, directory);

			return Directory.Exists(dirPath);
		}

		/// <summary>
		/// Returns true if the specified file exists in the storage-container, false otherwise.
		/// </summary>
		/// <param name="file">The relative path of the file to query for.</param>
		/// <returns>True if file exists, false otherwise.</returns>
		public bool FileExists(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("Parameter file must contain a value.");
			}

			// File name is relative, so combine with our path.
			string filePath = Path.Combine(storagePath, file);

			// Return a new file with read/write access.
			return File.Exists(filePath);
		}

		#endregion

		#region Public GetNames Methods

		/// <summary>
		/// Returns an array of the directory names in the storage-container.
		/// </summary>
		/// <returns>Array of directory names.</returns>
		public string[] GetDirectoryNames()
		{
			string[] names = Directory.GetDirectories(storagePath);
			for (int i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(storagePath.Length + 1);
			}
			return names;
		}

		/// <summary>
		/// Returns an array of directory names with given search pattern.
		/// </summary>
		/// <param name="searchPattern">
		/// A search pattern that supports single-character ("?") and multicharacter ("*")
		/// wildcards.
		/// </param>
		/// <returns>Array of matched directory names.</returns>
		public string[] GetDirectoryNames(string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
			{
				throw new ArgumentNullException("Parameter searchPattern must contain a value.");
			}

			string[] names = Directory.GetDirectories(storagePath, searchPattern);
			for (int i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(storagePath.Length + 1);
			}
			return names;
		}

		/// <summary>
		/// Returns an array of file names in the storage-container.
		/// </summary>
		/// <returns>Array of file names.</returns>
		public string[] GetFileNames()
		{
			string[] names = Directory.GetFiles(storagePath);
			for (int i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(storagePath.Length + 1);
			}
			return names;
		}

		/// <summary>
		/// Returns an array of file names with given search pattern.
		/// </summary>
		/// <param name="searchPattern">
		/// A search pattern that supports single-character ("?") and multicharacter ("*")
		/// wildcards.
		/// </param>
		/// <returns>Array of matched file names.</returns>
		public string[] GetFileNames(string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
			{
				throw new ArgumentNullException("Parameter searchPattern must contain a value.");
			}

			string[] names = Directory.GetFiles(storagePath, searchPattern);
			for (int i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(storagePath.Length + 1);
			}
			return names;
		}

		#endregion

		#region Public OpenFile Methods

		/// <summary>
		/// Opens a file contained in storage-container.
		/// </summary>
		/// <param name="file">Relative path of the file.</param>
		/// <param name="fileMode">
		/// <see cref="FileMode"/> that specifies how the file is to be opened.
		/// </param>
		/// <returns><see cref="Stream"/> object for the opened file.</returns>
		public Stream OpenFile(
			string file,
			FileMode fileMode
		) {
			return OpenFile(
				file,
				fileMode,
				FileAccess.ReadWrite,
				FileShare.ReadWrite
			);
		}

		/// <summary>
		/// Opens a file contained in storage-container.
		/// </summary>
		/// <param name="file">Relative path of the file.</param>
		/// <param name="fileMode">
		/// <see cref="FileMode"/> that specifies how the file is to be opened.
		/// </param>
		/// <param name="fileAccess">
		/// <see cref="FileAccess"/> that specifies access mode.
		/// </param>
		/// <returns><see cref="Stream"/> object for the opened file.</returns>
		public Stream OpenFile(
			string file,
			FileMode fileMode,
			FileAccess fileAccess
		) {
			return OpenFile(
				file,
				fileMode,
				fileAccess,
				FileShare.ReadWrite
			);
		}

		/// <summary>
		/// Opens a file contained in storage-container.
		/// </summary>
		/// <param name="file">Relative path of the file.</param>
		/// <param name="fileMode">
		/// <see cref="FileMode"/> that specifies how the file is to be opened.
		/// </param>
		/// <param name="fileAccess">
		/// <see cref="FileAccess"/> that specifies access mode.
		/// </param>
		/// <param name="fileShare">A bitwise combination of <see cref="FileShare"/>
		/// enumeration values that specifies access modes for other stream objects.</param>
		/// <returns><see cref="Stream"/> object for the opened file.</returns>
		public Stream OpenFile(
			string file,
			FileMode fileMode,
			FileAccess fileAccess,
			FileShare fileShare
		) {
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("Parameter file must contain a value.");
			}

			// Filename is relative, so combine with our path.
			string filePath = Path.Combine(storagePath, file);

			return File.Open(filePath, fileMode, fileAccess, fileShare);
		}

		#endregion
	}
}
