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
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class PictureAlbumCollection : IEnumerable<PictureAlbum>, IEnumerable, IDisposable
	{
		#region Internal Constructor
		internal PictureAlbumCollection() { throw new NotImplementedException(); }
		#endregion

		#region Enumerable Implementation
		public IEnumerator<PictureAlbum> GetEnumerator() { throw new NotImplementedException(); }
		IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public void Dispose() { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public int Count { get { throw new NotImplementedException(); } }
		public bool IsDisposed { get { throw new NotImplementedException(); } }
		public PictureAlbum this[int index] { get { throw new NotImplementedException(); } }
		#endregion
	}
}
