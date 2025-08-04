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
	public sealed class Picture : IEquatable<Picture>, IDisposable
	{
		#region Internal Constructor
		internal Picture() { throw new NotImplementedException(); }
		#endregion

		#region Equality Implementation
		public bool Equals(Picture other) { throw new NotImplementedException(); }
		public override bool Equals(Object obj) { throw new NotImplementedException(); }
		public override int GetHashCode() { throw new NotImplementedException(); }
		public static bool operator==(Picture first, Picture second) { throw new NotImplementedException(); }
		public static bool operator!=(Picture first, Picture second) { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public void Dispose() { throw new NotImplementedException(); }
		public Stream GetImage() { throw new NotImplementedException(); }
		public Stream GetThumbnail() { throw new NotImplementedException(); }
		public override string ToString() { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public PictureAlbum Album { get { throw new NotImplementedException(); } }
		public DateTime Date { get { throw new NotImplementedException(); } }
		public int Height { get { throw new NotImplementedException(); } }
		public bool IsDisposed { get { throw new NotImplementedException(); } }
		public string Name { get { throw new NotImplementedException(); } }
		public int Width { get { throw new NotImplementedException(); } }
		#endregion
	}
}
