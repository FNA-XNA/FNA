#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Diagnostics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public static class MediaPlayer
	{
		#region Public Static Properties

		public static bool GameHasControl
		{
			get
			{
				/* This is based on whether or not the player is playing custom
				 * music, rather than yours.
				 * -flibit
				 */
				return true;
			}
		}

		public static bool IsMuted
		{
			get
			{
				return INTERNAL_isMuted;
			}

			set
			{
				INTERNAL_isMuted = value;
				FAudio.XNA_SetSongVolume(
					INTERNAL_isMuted ?
						0.0f :
						INTERNAL_volume
				);
			}
		}

		public static bool IsRepeating
		{
			get;
			set;
		}

		public static bool IsShuffled
		{
			get;
			set;
		}

		public static TimeSpan PlayPosition
		{
			get
			{
				return timer.Elapsed;
			}
		}

		public static MediaQueue Queue 
		{
			get;
			private set;
		}

		public static MediaState State
		{
			get
			{
				return INTERNAL_state;
			}

			private set
			{
				if (INTERNAL_state != value)
				{
					INTERNAL_state = value;
					FrameworkDispatcher.MediaStateChanged = true;
				}
			}
		}

		public static float Volume
		{
			get
			{
				return INTERNAL_volume;
			}
			set
			{
				INTERNAL_volume = MathHelper.Clamp(
					value,
					0.0f,
					1.0f
				);
				FAudio.XNA_SetSongVolume(
					IsMuted ?
					0.0f :
					INTERNAL_volume
				);
			}
		}

		public static bool IsVisualizationEnabled
		{
			get;
			set;
		}

		#endregion

		#region Public Static Variables

		public static event EventHandler<EventArgs> ActiveSongChanged;
		public static event EventHandler<EventArgs> MediaStateChanged;

		#endregion

		#region Private Static Variables

		private static bool INTERNAL_isMuted = false;
		private static MediaState INTERNAL_state = MediaState.Stopped;
		private static float INTERNAL_volume = 1.0f;

		private static bool initialized = false;

		/* Need to hold onto this to keep track of how many songs
		 * have played when in shuffle mode.
		 */
		private static int numSongsInQueuePlayed = 0;

		/* FIXME: Ideally we'd be using the stream offset to track position,
		 * but usually you end up with a bit of stairstepping...
		 *
		 * For now, just use a timer. It's not 100% accurate, but it'll at
		 * least be consistent.
		 * -flibit
		 */
		private static Stopwatch timer = new Stopwatch();

		#endregion

		#region Static Constructor

		static MediaPlayer()
		{
			Queue = new MediaQueue();
			IsVisualizationEnabled = false;
		}

		#endregion

		#region Public Static Methods

		public static void MoveNext()
		{
			NextSong(1);
		}

		public static void MovePrevious()
		{
			NextSong(-1);
		}

		public static void Pause()
		{
			if (State != MediaState.Playing || Queue.ActiveSong == null)
			{
				return;
			}

			FAudio.XNA_PauseSong();
			timer.Stop();

			State = MediaState.Paused;
		}

		/// <summary>
		/// The Play method clears the current playback queue and queues the specified song
		/// for playback. Playback starts immediately at the beginning of the song.
		/// </summary>
		public static void Play(Song song)
		{
			Song previousSong = Queue.Count > 0 ? Queue[0] : null;

			Queue.Clear();
			numSongsInQueuePlayed = 0;
			LoadSong(song);
			Queue.ActiveSongIndex = 0;

			PlaySong(song);

			if (previousSong != song)
			{
				FrameworkDispatcher.ActiveSongChanged = true;
			}
		}

		public static void Play(SongCollection songs)
		{
			Play(songs, 0);
		}

		public static void Play(SongCollection songs, int index)
		{
			Queue.Clear();
			numSongsInQueuePlayed = 0;

			foreach (Song song in songs)
			{
				LoadSong(song);
			}

			Queue.ActiveSongIndex = index;

			PlaySong(Queue.ActiveSong);
		}

		public static void Resume()
		{
			if (State != MediaState.Paused)
			{
				return;
			}

			FAudio.XNA_ResumeSong();
			timer.Start();
			State = MediaState.Playing;
		}

		public static void Stop()
		{
			if (State == MediaState.Stopped)
			{
				return;
			}

			FAudio.XNA_StopSong();
			timer.Stop();
			timer.Reset();

			foreach (Song song in Queue.Songs)
			{
				song.PlayCount = 0;
			}

			State = MediaState.Stopped;
		}

		public static void GetVisualizationData(VisualizationData data)
		{
			if (IsVisualizationEnabled)
			{
				data.CalculateData(Queue.ActiveSong);
			}
		}

		#endregion

		#region Internal Static Methods

		internal static void Update()
		{
			if (	Queue == null ||
				Queue.ActiveSong == null ||
				FAudio.XNA_GetSongEnded() == 0	)
			{
				// Nothing to do... yet...
				return;
			}

			numSongsInQueuePlayed += 1;

			if (numSongsInQueuePlayed >= Queue.Count)
			{
				numSongsInQueuePlayed = 0;
				if (!IsRepeating)
				{
					Stop();

					FrameworkDispatcher.ActiveSongChanged = true;

					return;
				}
			}

			MoveNext();
		}

		internal static void DisposeIfNecessary()
		{
			if (initialized)
			{
				FAudio.XNA_SongQuit();
				initialized = false;
			}
		}

		internal static void OnActiveSongChanged()
		{
			if (ActiveSongChanged != null)
			{
				ActiveSongChanged(null, EventArgs.Empty);
			}
		}

		internal static void OnMediaStateChanged()
		{
			if (MediaStateChanged != null)
			{
				MediaStateChanged(null, EventArgs.Empty);
			}
		}

		#endregion

		#region Private Static Methods

		private static void LoadSong(Song song)
		{
			/* Believe it or not, XNA duplicates the Song object
			 * and then assigns a bunch of stuff to it at Play time.
			 * -flibit
			 */
			Queue.Add(new Song(song.handle, song.Name));
		}

		private static void NextSong(int direction)
		{
			Stop();
			if (IsRepeating && Queue.ActiveSongIndex >= Queue.Count - 1)
			{
				Queue.ActiveSongIndex = 0;

				/* Setting direction to 0 will force the first song
				 * in the queue to be played.
				 * if we're on "shuffle", then it'll pick a random one
				 * anyway, regardless of the "direction".
				 */
				direction = 0;
			}

			Song nextSong = Queue.GetNextSong(direction, IsShuffled);

			if (nextSong != null)
			{
				PlaySong(nextSong);
			}

			FrameworkDispatcher.ActiveSongChanged = true;
		}

		private static void PlaySong(Song song)
		{
			if (!initialized)
			{
				FAudio.XNA_SongInit();
				initialized =  true;
			}
			song.Duration = TimeSpan.FromSeconds(FAudio.XNA_PlaySong(song.handle));
			timer.Start();
			State = MediaState.Playing;
		}

		#endregion

	}
}
