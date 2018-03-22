#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region CASE_SENSITIVITY_HACK Option
// #define CASE_SENSITIVITY_HACK
/* On Linux, the file system is case sensitive.
 * This means that unless you really focused on it, there's a good chance that
 * your filenames are not actually accurate! The result: File/DirectoryNotFound.
 * This is a quick alternative to MONO_IOMAP=all, but the point is that you
 * should NOT depend on either of these two things. PLEASE fix your paths!
 * -flibit
 */
#endregion

#region Using Statements
using System;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class TitleContainer
	{
		#region Public Static Methods

		/// <summary>
		/// Returns an open stream to an exsiting file in the title storage area.
		/// </summary>
		/// <param name="name">The filepath relative to the title storage area.</param>
		/// <returns>A open stream or null if the file is not found.</returns>
		public static Stream OpenStream(string name)
		{
			string safeName = MonoGame.Utilities.FileHelpers.NormalizeFilePathSeparators(name);

#if CASE_SENSITIVITY_HACK
			if (Path.IsPathRooted(safeName))
			{
				return OpenStreamCase(safeName);
			}
			return OpenStreamCase(Path.Combine(TitleLocation.Path, safeName));
#else
			if (Path.IsPathRooted(safeName))
			{
				return File.OpenRead(safeName);
			}
			return File.OpenRead(Path.Combine(TitleLocation.Path, safeName));
#endif
		}

		#endregion

		#region Private Static fcaseopen Method

#if CASE_SENSITIVITY_HACK
		private static Stream OpenStreamCase(string name)
		{
			if (File.Exists(name))
			{
				return File.OpenRead(name);
			}

			string[] splits = name.Split(Path.DirectorySeparatorChar);
			splits[0] = "/";
			int i;

			// The directories...
			for (i = 1; i < splits.Length - 1; i += 1)
			{
				splits[0] += SearchCase(
					splits[i],
					Directory.GetDirectories(splits[0])
				);
			}

			// The file...
			splits[0] += SearchCase(
				splits[i],
				Directory.GetFiles(splits[0])
			);

			// Finally.
			splits[0] = splits[0].Remove(0, 1);
			FNALoggerEXT.LogError(
				"Case sensitivity!\n\t" +
				name.Substring(TitleLocation.Path.Length) + "\n\t" +
				splits[0].Substring(TitleLocation.Path.Length)
			);
			return File.OpenRead(splits[0]);
		}

		private static string SearchCase(string name, string[] list)
		{
			foreach (string l in list)
			{
				string li = l.Substring(l.LastIndexOf("/") + 1);
				if (name.ToLower().Equals(li.ToLower()))
				{
					return Path.DirectorySeparatorChar + li;
				}
			}
			// If you got here, get ready to crash!
			return Path.DirectorySeparatorChar + name;
		}
#endif

		#endregion
	}
}

