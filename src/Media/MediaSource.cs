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
	public sealed class MediaSource
	{
		#region Internal Constructor
		internal MediaSource() { throw new NotImplementedException(); }
		#endregion

		#region Public Static Methods
		public static IList<MediaSource> GetAvailableMediaSources() { throw new NotImplementedException(); }
		#endregion

		#region Public Methods
		public override string ToString() { throw new NotImplementedException(); }
		#endregion

		#region Public Properties
		public MediaSourceType MediaSourceType { get { throw new NotImplementedException(); } }
		public string Name { get { throw new NotImplementedException(); } }
		#endregion
	}
}
