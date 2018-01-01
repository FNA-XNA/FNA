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
using System.Reflection;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal static class ContentExtensions
	{
		#region Public Static Constructor Extractor Method

		public static ConstructorInfo GetDefaultConstructor(this Type type)
		{
			return type.GetConstructor(
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				null,
				new Type[0],
				null
			);
		}

		#endregion
	}
}
