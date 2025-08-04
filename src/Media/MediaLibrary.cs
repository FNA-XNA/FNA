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
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaLibrary : IDisposable
	{
		#region Public Constructors
		public MediaLibrary() { throw new NotImplementedException(); }
		public MediaLibrary(MediaSource mediaSource) { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public void Dispose() { throw new NotImplementedException(); }
		public Picture GetPictureFromToken(string token) { throw new NotImplementedException(); }
		public Picture SavePicture(string name, byte[] imageBuffer) { throw new NotImplementedException(); }
		public Picture SavePicture(string name, Stream source) { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public AlbumCollection Albums { get { throw new NotImplementedException(); } }
		public ArtistCollection Artists { get { throw new NotImplementedException(); } }
		public GenreCollection Genres { get { throw new NotImplementedException(); } }
		public bool IsDisposed { get { throw new NotImplementedException(); } }
		public MediaSource MediaSource { get { throw new NotImplementedException(); } }
		public PictureCollection Pictures { get { throw new NotImplementedException(); } }
		public PlaylistCollection Playlists { get { throw new NotImplementedException(); } }
		public PictureAlbum RootPictureAlbum { get { throw new NotImplementedException(); } }
		public PictureCollection SavedPictures { get { throw new NotImplementedException(); } }
		public SongCollection Songs { get { throw new NotImplementedException(); } }
		#endregion
	}
}
