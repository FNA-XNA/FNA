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
	public sealed class Artist : IEquatable<Artist>, IDisposable
	{
		#region Internal Constructor
		internal Artist() { throw new NotImplementedException(); }
		#endregion

		#region Equality Implementation
		public bool Equals(Artist other) { throw new NotImplementedException(); }
		public override bool Equals(Object obj) { throw new NotImplementedException(); }
		public override int GetHashCode() { throw new NotImplementedException(); }
		public static bool operator==(Artist first, Artist second) { throw new NotImplementedException(); }
		public static bool operator!=(Artist first, Artist second) { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public void Dispose() { throw new NotImplementedException(); }
		public override string ToString() { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public AlbumCollection Albums { get { throw new NotImplementedException(); } }
		public bool IsDisposed { get { throw new NotImplementedException(); } }
		public string Name { get { throw new NotImplementedException(); } }
		public SongCollection Songs { get { throw new NotImplementedException(); } }
		#endregion
	}
}
