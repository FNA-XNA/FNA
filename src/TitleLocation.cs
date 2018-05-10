#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class TitleLocation
	{
		#region Public Static Properties

		public static string Path
		{
			get
			{
				return FNAPlatform.TitleLocation;
			}
		}

		#endregion
	}
}

