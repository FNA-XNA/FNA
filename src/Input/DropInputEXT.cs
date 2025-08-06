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
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public static class DropInputEXT
	{
		#region Event
		public static event Action<string> FileDroped;

		public static event Action<string> TextDroped;
		#endregion


		#region Internal Event Access Method

		internal static void OnFileDrop(string fileName)
		{
			if (FileDroped != null)
			{
				FileDroped(fileName);
			}
		}

		internal static void OnTextDrop(string text)
		{
			if (TextDroped != null)
			{
				TextDroped(text);
			}
		}

		#endregion
	}
}
