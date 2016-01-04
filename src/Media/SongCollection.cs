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
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class SongCollection : IEnumerable<Song>, IEnumerable, IDisposable
	{
		#region Public Properties

		public Song this[int index]
		{
			get
			{
				return innerlist[index];
			}
		}

		public int Count
		{
			get
			{
				return innerlist.Count;
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private List<Song> innerlist;

		#endregion

		#region Internal Constructor

		internal SongCollection(List<Song> songs)
		{
			innerlist = songs;
			IsDisposed = false;
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			innerlist.Clear();
			IsDisposed = true;
		}

		#endregion

		#region IEnumerable Methods

		public IEnumerator<Song> GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		#endregion
	}
}
