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
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaQueue
	{
		#region Public Properties

		public Song ActiveSong
		{
			get
			{
				if (songs.Count == 0 || ActiveSongIndex < 0)
				{
					return null;
				}

				return songs[ActiveSongIndex];
			}
		}

		public int ActiveSongIndex
		{
			get;
			set;
		}

		public int Count
		{
			get
			{
				return songs.Count;
			}
		}

		public Song this[int index]
		{
			get
			{
				return songs[index];
			}
		}

		#endregion

		#region Internal Properties

		internal IEnumerable<Song> Songs
		{
			get
			{
				return songs;
			}
		}

		internal Song GetNextSong(int direction, bool shuffle)
		{
			if (shuffle)
			{
				ActiveSongIndex = random.Next(songs.Count);
			}
			else
			{
				ActiveSongIndex = (int) MathHelper.Clamp(
					ActiveSongIndex + direction,
					0,
					songs.Count - 1
				);
			}

			return songs[ActiveSongIndex];
		}

		#endregion

		#region Private Variables

		private List<Song> songs = new List<Song>();
		private Random random = new Random();

		#endregion

		#region Public Constructor

		public MediaQueue()
		{
			ActiveSongIndex = -1;
		}

		#endregion

		#region Internal Methods

		internal void Add(Song song)
		{
			songs.Add(song);
		}

		internal void Clear()
		{
			Song song;
			while (songs.Count > 0)
			{
				song = songs[0];
				song.Stop();
				songs.Remove(song);
			}
		}

		internal void SetVolume(float volume)
		{
			int count = songs.Count;
			for (int i = 0; i < count; i += 1)
			{
				songs[i].Volume = volume;
			}
		}

		internal void Stop()
		{
			int count = songs.Count;
			for (int i = 0; i < count; i += 1)
			{
				songs[i].Stop();
			}
		}

		#endregion

	}
}
