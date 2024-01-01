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

		#region Private Variables

		private List<Song> songs = new List<Song>();

		#endregion

		#region Internal Constructor

		internal MediaQueue()
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
			songs.Clear();
		}

		#endregion

	}
}
