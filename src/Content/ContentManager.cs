#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public partial class ContentManager : IDisposable
	{
		#region Public ServiceProvider Property

		public IServiceProvider ServiceProvider
		{
			get;
			private set;
		}

		#endregion

		#region Public RootDirectory Property

		public string RootDirectory
		{
			get;
			set;
		}

		#endregion

		#region Internal Root Directory Path Property

		internal string RootDirectoryFullPath
		{
			get
			{
				if (Path.IsPathRooted(RootDirectory))
				{
					return RootDirectory;
				}
				return Path.Combine(TitleLocation.Path, RootDirectory);
			}
		}

		#endregion

		#region Private Variables

		private GraphicsDevice graphicsDevice;
		private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private List<IDisposable> disposableAssets = new List<IDisposable>();
		private bool disposed;

		#endregion

		#region Private Static Variables

		private static object ContentManagerLock = new object();
		private static List<WeakReference> ContentManagers = new List<WeakReference>();

		private static readonly byte[] xnbHeader = new byte[4];
		private static List<char> targetPlatformIdentifiers = new List<char>()
		{
			'w', // Windows (DirectX)
			'x', // Xbox360
			'm', // WindowsPhone
			'i', // iOS
			'a', // Android
			'd', // DesktopGL
			'X', // MacOSX
			'W', // WindowsStoreApp
			'n', // NativeClient
			'u', // Ouya
			'p', // PlayStationMobile
			'M', // WindowsPhone8
			'r', // RaspberryPi
			'P', // Playstation 4
			'g', // WindowsGL (deprecated for DesktopGL)
			'l', // Linux (deprecated for DesktopGL)
		};

		// Note: These may not be in alphabetical order, for performance reasons
		private static readonly string[] effectExtensions = new string[] { ".fxb" };
		private static readonly string[] texture2DExtensions = new string[]
		{
			".png", ".jpg", ".jpeg", ".dds", ".qoi", ".bmp", ".gif", ".tga", ".tif", ".tiff"
		};
		private static readonly string[] textureCubeExtensions = new string[] {	".dds" };
		private static readonly string[] soundEffectExtensions = new string[] { ".wav" };

		#endregion

		#region Public Constructors

		public ContentManager(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			ServiceProvider = serviceProvider;
			RootDirectory = string.Empty;
			AddContentManager(this);
		}

		public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (rootDirectory == null)
			{
				throw new ArgumentNullException("rootDirectory");
			}
			ServiceProvider = serviceProvider;
			RootDirectory = rootDirectory;
			AddContentManager(this);
		}

		#endregion

		#region Destructor

		/* Use C# destructor syntax for finalization code.
		 * This destructor will run only if the Dispose method
		 * does not get called.
		 * It gives your base class the opportunity to finalize.
		 * Do not provide destructors in types derived from this class.
		 */
		~ContentManager()
		{
			/* Do not re-create Dispose clean-up code here.
			 * Calling Dispose(false) is optimal in terms of
			 * readability and maintainability.
			 */
			Dispose(false);
		}

		#endregion

		#region Dispose Methods

		public void Dispose()
		{
			Dispose(true);
			/* Tell the garbage collector not to call the finalizer
			 * since all the cleanup will already be done.
			 */
			GC.SuppressFinalize(this);
			// Once disposed, content manager wont be used again
			RemoveContentManager(this);
		}

		/* If disposing is true, it was called explicitly and we should dispose managed
		 * objects. If disposing is false, it was called by the finalizer and managed
		 * objects should not be disposed.
		 */
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Unload();
				}
				disposed = true;
			}
		}

		#endregion

		#region Public Methods

		public virtual T Load<T>(string assetName)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}
			T result = default(T);

			/* On some platforms, name and slash direction matter.
			 * We store the asset by a /-separating key rather than
			 * how the path to the file was passed to us to avoid
			 * loading "content/asset1.xnb" and "content\\ASSET1.xnb"
			 * as if they were two different files. this matches
			 * stock XNA behavior. The Dictionary will ignore case
			 * differences.
			 */
			string key = assetName.Replace('\\', '/');

			// Check for a previously loaded asset first
			object asset = null;
			if (loadedAssets.TryGetValue(key, out asset))
			{
				if (asset is T)
				{
					return (T) asset;
				}
			}
			// Load the asset.
			result = ReadAsset<T>(assetName, null);
			loadedAssets[key] = result;
			return result;
		}

		public virtual void Unload()
		{
			// Look for disposable assets.
			foreach (IDisposable disposable in disposableAssets)
			{
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			disposableAssets.Clear();
			loadedAssets.Clear();
		}

		#endregion

		#region Protected Methods

		protected virtual Stream OpenStream(string assetName)
		{
			Stream stream;
			try
			{
				stream = TitleContainer.OpenStream(
					Path.Combine(RootDirectory, assetName) + ".xnb"
				);
			}
			catch (FileNotFoundException fileNotFound)
			{
				throw new ContentLoadException("The content file was not found.", fileNotFound);
			}
			catch (DirectoryNotFoundException directoryNotFound)
			{
				throw new ContentLoadException("The directory was not found.", directoryNotFound);
			}
			catch (Exception exception)
			{
				throw new ContentLoadException("Opening stream error.", exception);
			}
			return stream;
		}

		protected T ReadAsset<T>(string assetName, Action<IDisposable> recordDisposableObject)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}

			object result = null;
			Stream stream = null;
			try
			{
				stream = OpenStream(assetName);
			}
			catch (Exception e)
			{
				// Okay, so we couldn't open it. Maybe it needs a different extension?
				stream = OpenStreamRaw<T>(assetName);
				if (stream == null)
				{
					throw new ContentLoadException(
						"Could not load asset " + assetName + "! Error: " + e.Message,
						e
					);
				}
			}

			// Check for XNB header
			stream.Read(xnbHeader, 0, xnbHeader.Length);
			if (	xnbHeader[0] == 'X' &&
				xnbHeader[1] == 'N' &&
				xnbHeader[2] == 'B' &&
				targetPlatformIdentifiers.Contains((char) xnbHeader[3]) )
			{
				using (BinaryReader xnbReader = new BinaryReader(stream))
				using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, (char) xnbHeader[3], recordDisposableObject))
				{
					result = reader.ReadAsset<T>();
					GraphicsResource resource = result as GraphicsResource;
					if (resource != null)
					{
						resource.Name = assetName;
					}
				}
			}
			else
			{
				// It's not an XNB file. Try to load as a raw asset instead.

				// FIXME: Assuming seekable streams! -flibit
				stream.Seek(0, SeekOrigin.Begin);

				if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
				{
					Texture2D texture;
					if (	xnbHeader[0] == 'D' &&
						xnbHeader[1] == 'D' &&
						xnbHeader[2] == 'S' &&
						xnbHeader[3] == ' '	)
					{
						texture = Texture2D.DDSFromStreamEXT(
							GetGraphicsDevice(),
							stream
						);
					}
					else
					{
						texture = Texture2D.FromStream(
							GetGraphicsDevice(),
							stream
						);
					}
					texture.Name = assetName;
					result = texture;
				}
				else if (typeof(T) == typeof(TextureCube))
				{
					TextureCube texture = TextureCube.DDSFromStreamEXT(
						GetGraphicsDevice(),
						stream
					);
					texture.Name = assetName;
					result = texture;
				}
				else if (typeof(T) == typeof(SoundEffect))
				{
					SoundEffect effect = SoundEffect.FromStream(stream);
					effect.Name = assetName;
					result = effect;
				}
				else if (typeof(T) == typeof(Effect))
				{
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, (int) stream.Length);
					Effect effect = new Effect(GetGraphicsDevice(), data);
					effect.Name = assetName;
					result = effect;
				}
				else if (typeof(T) == typeof(Song))
				{
					// Song can't use the stream, get the file name and free the handle
					string fileName = (stream as FileStream).Name;
					stream.Close();

					result = new Song(fileName);
				}
				else if (typeof(T) == typeof(Video))
				{
					// Video can't use the stream, get the file name and free the handle
					string fileName = (stream as FileStream).Name;
					stream.Close();

					result = new Video(fileName, GetGraphicsDevice());
					FNALoggerEXT.LogWarn(
						"Video " +
						fileName +
						" does not have an XNB file! Hacking Duration property!"
					);
				}
				else
				{
					stream.Close();
					throw new ContentLoadException("Could not load " + assetName + " asset!");
				}

				/* Because Raw Assets skip the ContentReader step, they need to have their
				 * disposables recorded here. Doing it outside of this catch will
				 * result in disposables being logged twice.
				 */
				IDisposable disposableResult = result as IDisposable;
				if (disposableResult != null)
				{
					if (recordDisposableObject != null)
					{
						recordDisposableObject(disposableResult);
					}
					else
					{
						disposableAssets.Add(disposableResult);
					}
				}

				/* Because we're not using a BinaryReader for raw assets, we
				 * need to close the stream ourselves.
				 * -flibit
				 */
				stream.Close();
			}

			return (T) result;
		}

		#endregion

		#region Internal Methods

		internal void RecordDisposable(IDisposable disposable)
		{
			Debug.Assert(disposable != null, "The disposable is null!");

			/* Avoid recording disposable objects twice. ReloadAsset will try to record
			 * the disposables again. We don't know which asset recorded which
			 * disposable so just guard against storing multiple of the same instance.
			 */
			if (!disposableAssets.Contains(disposable))
			{
				disposableAssets.Add(disposable);
			}
		}

		internal GraphicsDevice GetGraphicsDevice()
		{
			if (graphicsDevice == null)
			{
				IGraphicsDeviceService result = ServiceProvider.GetService(
					typeof(IGraphicsDeviceService)
				) as IGraphicsDeviceService;
				if (result == null)
				{
					throw new ContentLoadException("No Graphics Device Service");
				}
				graphicsDevice = result.GraphicsDevice;
			}
			return graphicsDevice;
		}

		#endregion

		#region Private Methods

		private ContentReader GetContentReaderFromXnb(string originalAssetName, ref Stream stream, BinaryReader xnbReader, char platform, Action<IDisposable> recordDisposableObject)
		{
			byte version = xnbReader.ReadByte();
			byte flags = xnbReader.ReadByte();
			bool compressed = (flags & 0x80) != 0;
			if (version != 5 && version != 4)
			{
				throw new ContentLoadException("Invalid XNB version");
			}
			// The next int32 is the length of the XNB file
			int xnbLength = xnbReader.ReadInt32();
			ContentReader reader;
			if (compressed)
			{
				/* Decompress the XNB
				 * Thanks to ShinAli (https://bitbucket.org/alisci01/xnbdecompressor)
				 */
				int compressedSize = xnbLength - 14;
				int decompressedSize = xnbReader.ReadInt32();

				// This will replace the XNB stream at the end
				MemoryStream decompressedStream = new MemoryStream(
					new byte[decompressedSize],
					0,
					decompressedSize,
					true,
					true // This MUST be true! Readers may need GetBuffer()!
				);

				/* Read in the whole XNB file at once, into a temp buffer.
				 * For slow disks, the extra malloc is more than worth the
				 * performance improvement from not constantly fread()ing!
				 */
				MemoryStream compressedStream = new MemoryStream(
					new byte[compressedSize],
					0,
					compressedSize,
					true,
					true
				);
				stream.Read(compressedStream.GetBuffer(), 0, compressedSize);

				// Default window size for XNB encoded files is 64Kb (need 16 bits to represent it)
				LzxDecoder dec = new LzxDecoder(16);
				int decodedBytes = 0;
				long pos = 0;

				while (pos < compressedSize)
				{
					/* The compressed stream is separated into blocks that will
					 * decompress into 32kB or some other size if specified.
					 * Normal, 32kB output blocks will have a short indicating
					 * the size of the block before the block starts. Blocks
					 * that have a defined output will be preceded by a byte of
					 * value 0xFF (255), then a short indicating the output size
					 * and another for the block size. All shorts for these
					 * cases are encoded in big endian order.
					 */
					int hi = compressedStream.ReadByte();
					int lo = compressedStream.ReadByte();
					int block_size = (hi << 8) | lo;
					int frame_size = 0x8000; // Frame size is 32kB by default
					// Does this block define a frame size?
					if (hi == 0xFF)
					{
						hi = lo;
						lo = (byte) compressedStream.ReadByte();
						frame_size = (hi << 8) | lo;
						hi = (byte) compressedStream.ReadByte();
						lo = (byte) compressedStream.ReadByte();
						block_size = (hi << 8) | lo;
						pos += 5;
					}
					else
					{
						pos += 2;
					}
					// Either says there is nothing to decode
					if (block_size == 0 || frame_size == 0)
					{
						break;
					}
					dec.Decompress(compressedStream, block_size, decompressedStream, frame_size);
					pos += block_size;
					decodedBytes += frame_size;
					/* Reset the position of the input just in case the bit
					 * buffer read in some unused bytes.
					 */
					compressedStream.Seek(pos, SeekOrigin.Begin);
				}
				if (decompressedStream.Position != decompressedSize)
				{
					throw new ContentLoadException(
						"Decompression of " + originalAssetName + " failed. "
					);
				}
				decompressedStream.Seek(0, SeekOrigin.Begin);
				reader = new ContentReader(
					this,
					decompressedStream,
					originalAssetName,
					version,
					platform,
					recordDisposableObject
				);
			}
			else
			{
				reader = new ContentReader(
					this,
					stream,
					originalAssetName,
					version,
					platform,
					recordDisposableObject
				);
			}
			return reader;
		}

		private Stream CheckRawExtensions(string assetName, string[] extensions)
		{
			// Start with the fastest path...
			string fileName = MonoGame.Utilities.FileHelpers.NormalizeFilePathSeparators(
				Path.Combine(RootDirectoryFullPath, assetName)
			);
			if (File.Exists(fileName))
			{
				return TitleContainer.OpenStream(fileName);
			}
			foreach (string ext in extensions)
			{
				// Concatenate the file name with valid extensions.
				string fileNamePlusExt = fileName + ext;
				if (File.Exists(fileNamePlusExt))
				{
					return TitleContainer.OpenStream(fileNamePlusExt);
				}
			}

			// If we got here, we need to try the slower path :(
			fileName = MonoGame.Utilities.FileHelpers.NormalizeFilePathSeparators(
				assetName
			);
			try
			{
				return OpenStream(fileName);
			}
			catch
			{
				foreach (string ext in extensions)
				{
					// Concatenate the file name with valid extensions.
					string fileNamePlusExt = fileName + ext;
					try
					{
						return OpenStream(fileNamePlusExt);
					}
					catch
					{
						continue;
					}
				}
			}

			// No idea what we're looking at here!
			return null;
		}

		private Stream OpenStreamRaw<T>(string assetName)
		{
			if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
			{
				return CheckRawExtensions(assetName, texture2DExtensions);
			}
			else if (typeof(T) == typeof(TextureCube))
			{
				return CheckRawExtensions(assetName, textureCubeExtensions);
			}
			else if (typeof(T) == typeof(SoundEffect))
			{
				return CheckRawExtensions(assetName, soundEffectExtensions);
			}
			else if (typeof(T) == typeof(Effect))
			{
				return CheckRawExtensions(assetName, effectExtensions);
			}
			else if (typeof(T) == typeof(Song))
			{
				return CheckRawExtensions(assetName, SongReader.supportedExtensions);
			}
			else if (typeof(T) == typeof(Video))
			{
				return CheckRawExtensions(assetName, VideoReader.supportedExtensions);
			}
			return null;
		}

		#endregion

		#region Private Static Methods

		private static void AddContentManager(ContentManager contentManager)
		{
			lock (ContentManagerLock)
			{
				/* Check if the list contains this content manager already. Also take
				 * the opportunity to prune the list of any finalized content managers.
				 */
				bool contains = false;
				for (int i = ContentManagers.Count - 1; i >= 0; i -= 1)
				{
					WeakReference contentRef = ContentManagers[i];
					if (ReferenceEquals(contentRef.Target, contentManager))
					{
						contains = true;
					}
					if (!contentRef.IsAlive)
					{
						ContentManagers.RemoveAt(i);
					}
				}
				if (!contains)
				{
					ContentManagers.Add(new WeakReference(contentManager));
				}
			}
		}

		private static void RemoveContentManager(ContentManager contentManager)
		{
			lock (ContentManagerLock)
			{
				/* Check if the list contains this content manager and remove it. Also
				 * take the opportunity to prune the list of any finalized content managers.
				 */
				for (int i = ContentManagers.Count - 1; i >= 0; i -= 1)
				{
					WeakReference contentRef = ContentManagers[i];
					if (!contentRef.IsAlive || ReferenceEquals(contentRef.Target, contentManager))
					{
						ContentManagers.RemoveAt(i);
					}
				}
			}
		}

		#endregion
	}
}
