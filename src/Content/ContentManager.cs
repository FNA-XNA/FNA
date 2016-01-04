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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Utilities;
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
				return Path.Combine(TitleContainer.Location, RootDirectory);
			}
		}

		#endregion

		#region Private Variables

		private IGraphicsDeviceService graphicsDeviceService;
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

			// FIXME: Should this block be here? -flibit
			if (graphicsDeviceService == null)
			{
				graphicsDeviceService = ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
				if (graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}

			Stream stream = null;
			string modifiedAssetName = String.Empty; // Will be used if we have to guess a filename
			try
			{
				stream = OpenStream(assetName);
			}
			catch (Exception e)
			{
				// Okay, so we couldn't open it. Maybe it needs a different extension?
				// FIXME: This only works for files on the disk, what about custom streams? -flibit
				modifiedAssetName = FileHelpers.NormalizeFilePathSeparators(
					Path.Combine(RootDirectoryFullPath, assetName)
				);
				if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
				{
					modifiedAssetName = Texture2DReader.Normalize(modifiedAssetName);
				}
				else if ((typeof(T) == typeof(SoundEffect)))
				{
					modifiedAssetName = SoundEffectReader.Normalize(modifiedAssetName);
				}
				else if ((typeof(T) == typeof(Effect)))
				{
					modifiedAssetName = EffectReader.Normalize(modifiedAssetName);
				}
				else if ((typeof(T) == typeof(Song)))
				{
					modifiedAssetName = SongReader.Normalize(modifiedAssetName);
				}
				else if ((typeof(T) == typeof(Video)))
				{
					modifiedAssetName = VideoReader.Normalize(modifiedAssetName);
				}

				// Did we get anything...?
				if (String.IsNullOrEmpty(modifiedAssetName))
				{
					// Nope, nothing we're aware of!
					throw new ContentLoadException(
						"Could not load asset " + assetName + "! Error: " + e.Message,
						e
					);
				}

				stream = TitleContainer.OpenStream(modifiedAssetName);
			}

			// Check for XNB header
			stream.Read(xnbHeader, 0, xnbHeader.Length);
			if (	xnbHeader[0] == 'X' &&
				xnbHeader[1] == 'N' &&
				xnbHeader[2] == 'B' &&
				targetPlatformIdentifiers.Contains((char) xnbHeader[3]) )
			{
				using (BinaryReader xnbReader = new BinaryReader(stream))
				using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, recordDisposableObject))
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
					Texture2D texture = Texture2D.FromStream(
						graphicsDeviceService.GraphicsDevice,
						stream
					);
					texture.Name = assetName;
					result = texture;
				}
				else if ((typeof(T) == typeof(SoundEffect)))
				{
					result = SoundEffect.FromStream(stream);
				}
				else if ((typeof(T) == typeof(Effect)))
				{
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, (int) stream.Length);
					result = new Effect(graphicsDeviceService.GraphicsDevice, data);
				}
				else if ((typeof(T) == typeof(Song)))
				{
					// FIXME: Not using the stream! -flibit
					result = new Song(modifiedAssetName);
				}
				else if ((typeof(T) == typeof(Video)))
				{
					// FIXME: Not using the stream! -flibit
					result = new Video(modifiedAssetName);
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

		#endregion

		#region Private Methods

		private ContentReader GetContentReaderFromXnb(string originalAssetName, ref Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
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
				MemoryStream decompressedStream = new MemoryStream(
					new byte[decompressedSize],
					0,
					decompressedSize,
					true,
					true // This MUST be true! We may need GetBuffer()!
				);
				// Default window size for XNB encoded files is 64Kb (need 16 bits to represent it)
				LzxDecoder dec = new LzxDecoder(16);
				int decodedBytes = 0;
				long startPos = stream.Position;
				long pos = startPos;

				while (pos - startPos < compressedSize)
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
					int hi = stream.ReadByte();
					int lo = stream.ReadByte();
					int block_size = (hi << 8) | lo;
					int frame_size = 0x8000; // Frame size is 32kB by default
					// Does this block define a frame size?
					if (hi == 0xFF)
					{
						hi = lo;
						lo = (byte) stream.ReadByte();
						frame_size = (hi << 8) | lo;
						hi = (byte) stream.ReadByte();
						lo = (byte) stream.ReadByte();
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
					dec.Decompress(stream, block_size, decompressedStream, frame_size);
					pos += block_size;
					decodedBytes += frame_size;
					/* Reset the position of the input just in case the bit
					 * buffer read in some unused bytes.
					 */
					stream.Seek(pos, SeekOrigin.Begin);
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
					graphicsDeviceService.GraphicsDevice,
					originalAssetName,
					version,
					recordDisposableObject
				);
			}
			else
			{
				reader = new ContentReader(
					this,
					stream,
					graphicsDeviceService.GraphicsDevice,
					originalAssetName,
					version,
					recordDisposableObject
				);
			}
			return reader;
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
