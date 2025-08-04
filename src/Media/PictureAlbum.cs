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
	public sealed class PictureAlbum : IEquatable<PictureAlbum>, IDisposable
	{
		#region Internal Constructor
		internal PictureAlbum() { throw new NotImplementedException(); }
		#endregion

		#region Equality Implementation
		public bool Equals(PictureAlbum other) { throw new NotImplementedException(); }
		public override bool Equals(Object obj) { throw new NotImplementedException(); }
		public override int GetHashCode() { throw new NotImplementedException(); }
		public static bool operator==(PictureAlbum first, PictureAlbum second) { throw new NotImplementedException(); }
		public static bool operator!=(PictureAlbum first, PictureAlbum second) { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public void Dispose() { throw new NotImplementedException(); }
		public override string ToString() { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public PictureAlbumCollection Albums { get { throw new NotImplementedException(); } }
		public bool IsDisposed { get { throw new NotImplementedException(); } }
		public string Name { get { throw new NotImplementedException(); } }
		public PictureAlbum Parent { get { throw new NotImplementedException(); } }
		public PictureCollection Pictures { get { throw new NotImplementedException(); } }
		#endregion
	}
}
