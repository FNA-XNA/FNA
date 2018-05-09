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
				/* This property was previously prepared with the help
				 * of a class (static) constructor and a private setter.
				 * 
				 * Unfortunately, it caused issues when the game reflected
				 * into TitleLocation.Path before FNAPlatform had a chance
				 * to initialize the platform, which itself requires the path.
				 * 
				 * This "lazy property" fixes the issue.
				 * -ade
				 */
				if (INTERNAL_Path != null)
					return INTERNAL_Path;
				return INTERNAL_Path = FNAPlatform.GetBaseDirectory();
			}
		}

		#endregion

		#region Private Static Fields

		private static string INTERNAL_Path;

		#endregion
	}
}

