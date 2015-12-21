#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region NO_STREAM_THREAD Option
// #define NO_STREAM_THREAD
/* We use a thread to stream Songs specifically, because if the game hangs, the
 * Song needs to keep playing, as Windows Media Player would in XNA4.
 *
 * If you know that won't happen and want to cut out a thread, or you want to
 * use this with XNA4 somehow, you can uncomment this define.
 *
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Audio;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Song : IEquatable<Song>, IDisposable
	{
		#region Public Metadata Properties

		// TODO: vorbis_comment TITLE
		public string Name
		{
			get;
			private set;
		}

		// TODO: vorbis_comment TRACKNUMBER
		public int TrackNumber
		{
			get;
			private set;
		}

		#endregion

		#region Public Stream Properties

		public TimeSpan Duration
		{
			get;
			private set;
		}

		#endregion

		#region Public MediaPlayer Properties

		public bool IsProtected
		{
			get
			{
				return false;
			}
		}

		public bool IsRated
		{
			get
			{
				return false;
			}
		}

		public int PlayCount
		{
			get;
			private set;
		}

		public int Rating
		{
			get
			{
				return 0;
			}
		}

		#endregion

		#region Public IDisposable Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		// TODO: Track the ov_reads and stream position
		internal TimeSpan Position
		{
			get
			{
				return timer.Elapsed;
			}
		}

		internal float Volume
		{
			set
			{
				/* FIXME: Works around MasterVolume only temporarily!
				 * Figure out how MasterVolume actually applies to instances,
				 * then deal with this accordingly.
				 * -flibit
				 */
				soundStream.Volume = value * (1.0f / SoundEffect.MasterVolume);
			}
		}

		#endregion

		#region Internal Variables

		internal float[] chunk;
		internal int chunkSize;
		internal int chunkStep;

		#endregion

		#region Private Variables

		/* FIXME: Ideally we'd be using the Vorbis offsets to track position,
		 * but usually you end up with a bit of stairstepping...
		 *
		 * For now, just use a timer. It's not 100% accurate, but it'll at
		 * least be consistent.
		 * -flibit
		 */
		private Stopwatch timer = new Stopwatch();

		private DynamicSoundEffectInstance soundStream;
		private IntPtr vorbisFile;
		private bool eof;

		private const int MAX_SAMPLES = 2 * 2 * 48000;
		private static byte[] vorbisBuffer = new byte[MAX_SAMPLES];
		private static GCHandle bufferHandle = GCHandle.Alloc(vorbisBuffer, GCHandleType.Pinned);
		private static IntPtr bufferPtr = bufferHandle.AddrOfPinnedObject();

#if !NO_STREAM_THREAD
		private Thread songThread;
		private bool exitThread;
#endif

		#endregion

		#region Constructors, Deconstructor, Dispose()

		internal Song(string fileName)
		{
			Vorbisfile.ov_fopen(fileName, out vorbisFile);
			Vorbisfile.vorbis_info fileInfo = Vorbisfile.ov_info(
				vorbisFile,
				0
			);

			// TODO: ov_comment() -flibit
			Name = Path.GetFileNameWithoutExtension(fileName);
			TrackNumber = 0;

			Duration = TimeSpan.FromSeconds(
				Vorbisfile.ov_time_total(vorbisFile, 0)
			);

			soundStream = new DynamicSoundEffectInstance(
				(int) fileInfo.rate,
				(AudioChannels) fileInfo.channels
			);

			// FIXME: 60 is arbitrary for a 60Hz game -flibit
			chunkSize = (int) (fileInfo.rate * fileInfo.channels / 60);
			chunkStep = chunkSize / VisualizationData.Size;

			IsDisposed = false;
		}

		internal Song(string fileName, int durationMS) : this(fileName)
		{
			/* If you got here, you've still got the XNB file! Well done!
			 * Except if you're running FNA, you're not using the WMA anymore.
			 * But surely it's the same song, right...?
			 * Well, consider this a check more than anything. If this bothers
			 * you, just remove the XNB file and we'll read the OGG straight up.
			 * -flibit
			 */
			if (Math.Abs(Duration.Milliseconds - durationMS) > 1000)
			{
				throw new Exception("XNB/OGG duration mismatch!");
			}
		}

		~Song()
		{
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				soundStream.Dispose();
				soundStream = null;
				Vorbisfile.ov_clear(ref vorbisFile);
			}
			IsDisposed = true;
		}

		#endregion

		#region Internal Playback Methods

		internal void Play()
		{
			eof = false;
			soundStream.BufferNeeded += QueueBuffer;

#if NO_STREAM_THREAD
			soundStream.Play();
#else
			soundStream.Play(false);

			exitThread = false;
			songThread = new Thread(SongThread);
			songThread.IsBackground = true;
			songThread.Start();
#endif

			timer.Start();

			PlayCount += 1;
		}

		internal void Resume()
		{
			soundStream.Resume();
			timer.Start();
		}

		internal void Pause()
		{
			soundStream.Pause();
			timer.Stop();
		}

		internal void Stop()
		{
			PlayCount = 0;

#if !NO_STREAM_THREAD
			exitThread = true;
			if (songThread != null && Thread.CurrentThread != songThread)
			{
				songThread.Join();
			}
#endif

			timer.Stop();
			timer.Reset();

			soundStream.Stop();
			soundStream.BufferNeeded -= QueueBuffer;
			Vorbisfile.ov_time_seek(vorbisFile, 0.0);
		}

		internal float[] GetSamples()
		{
			if (chunk == null)
			{
				chunk = new float[chunkSize];
			}
			soundStream.GetSamples(chunk);
			return chunk;
		}

		#endregion

		#region Internal Event Handler Methods

		internal void QueueBuffer(object sender, EventArgs args)
		{
			int bs;
			int cur = 0;
			int total = 0;
			do
			{
				cur = (int) Vorbisfile.ov_read(
					vorbisFile,
					bufferPtr + total,
					4096,
					0,
					2,
					1,
					out bs
				);
				total += cur;
			} while (cur > 0 && total < (MAX_SAMPLES - 4096));

			// If we're at the end of the file, stop!
			if (total == 0)
			{
				eof = true;
				soundStream.BufferNeeded -= QueueBuffer;
				return;
			}

			// Send the filled buffer to the stream.
			soundStream.SubmitBuffer(
				vorbisBuffer,
				0,
				total
			);
		}

		#endregion

		#region Private Song Update Thread

#if !NO_STREAM_THREAD
		private void SongThread()
		{
			while (!exitThread)
			{
				soundStream.Update();
				if (eof && soundStream.PendingBufferCount == 0)
				{
					soundStream.Stop();
					exitThread = true;
				}
				// Arbitrarily 1 frame in a 15Hz game -flibit
				Thread.Sleep(67);
			}
			if (PlayCount > 0)
			{
				/* If PlayCount is 0 at this point, then we were stopped by the
				 * calling application, and this event shouldn't happen.
				 * -flibit
				 */
				MediaPlayer.OnSongFinishedPlaying(null, null);
			}
		}
#endif

		#endregion

		#region Public Comparison Methods/Operators

		public bool Equals(Song song) 
		{
			return (((object) song) != null) && (Name == song.Name);
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return Equals(obj as Song);
		}

		public static bool operator ==(Song song1, Song song2)
		{
			if (((object) song1) == null)
			{
				return ((object) song2) == null;
			}
			return song1.Equals(song2);
		}

		public static bool operator !=(Song song1, Song song2)
		{
			return !(song1 == song2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Constructs a new Song object based on the specified URI.
		/// </summary>
		/// <remarks>
		/// This method matches the signature of the one in XNA4, however we currently can't play remote songs, so
		/// the URI is required to be a file name and the code uses the LocalPath property only.
		/// </remarks>
		/// <param name="name">Name of the song.</param>
		/// <param name="uri">Uri object that represents the URI.</param>
		/// <returns>Song object that can be used to play the song.</returns>
		public static Song FromUri(string name, Uri uri)
		{
			if (!uri.IsFile)
			{
				throw new InvalidOperationException("Only local file URIs are supported for now");
			}

			return new Song(uri.LocalPath)
			{
				Name = name
			};
		}

		#endregion
	}
}
