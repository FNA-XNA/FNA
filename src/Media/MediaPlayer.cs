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

				if (Queue.Count == 0)
				{
					return;
				}

				Queue.SetVolume(value ? 0.0f : Volume);
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
				if (Queue.ActiveSong == null)
				{
					return TimeSpan.Zero;
				}

				return Queue.ActiveSong.Position;
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
					if (MediaStateChanged != null)
					{
						MediaStateChanged(null, EventArgs.Empty);
					}
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
				INTERNAL_volume = MathHelper.Clamp(value, 0.0f, 1.0f);

				if (Queue.ActiveSong == null)
				{
					return;
				}

				Queue.SetVolume(IsMuted ? 0.0f : value);
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

		/* Need to hold onto this to keep track of how many songs
		 * have played when in shuffle mode.
		 */
		private static int numSongsInQueuePlayed = 0;

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

			Queue.ActiveSong.Pause();

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
			Queue.Add(song);
			Queue.ActiveSongIndex = 0;

			PlaySong(song);

			if (previousSong != song && ActiveSongChanged != null)
			{
				ActiveSongChanged.Invoke(null, EventArgs.Empty);
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
				Queue.Add(song);
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

			Queue.ActiveSong.Resume();
			State = MediaState.Playing;
		}

		public static void Stop()
		{
			if (State == MediaState.Stopped)
			{
				return;
			}

			// Loop through so that we reset the PlayCount as well.
			foreach (Song song in Queue.Songs)
			{
				Queue.ActiveSong.Stop();
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

		internal static void OnSongFinishedPlaying(object sender, EventArgs args)
		{
			// TODO: Check args to see if song sucessfully played.
			numSongsInQueuePlayed += 1;

			if (numSongsInQueuePlayed >= Queue.Count)
			{
				numSongsInQueuePlayed = 0;
				if (!IsRepeating)
				{
					Stop();

					if (ActiveSongChanged != null)
					{
						ActiveSongChanged.Invoke(null, null);
					}

					return;
				}
			}

			MoveNext();
		}

		#endregion

		#region Private Static Methods

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

			if (ActiveSongChanged != null)
			{
				ActiveSongChanged.Invoke(null, null);
			}
		}

		private static void PlaySong(Song song)
		{
			song.Volume = IsMuted ? 0.0f : Volume;
			song.Play();
			State = MediaState.Playing;
		}

		#endregion

	}
}
