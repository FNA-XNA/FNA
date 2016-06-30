#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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

using Microsoft.Xna.Framework.Utilities;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class TitleContainer
	{
		#region Internal Static Properties

		static internal string Location
		{
			get;
			private set;
		}

		#endregion

		#region Static Constructor

		static TitleContainer()
		{
			Location = AppDomain.CurrentDomain.BaseDirectory;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns an open stream to an exsiting file in the title storage area.
		/// </summary>
		/// <param name="name">The filepath relative to the title storage area.</param>
		/// <returns>A open stream or null if the file is not found.</returns>
		public static Stream OpenStream(string name)
		{
			string safeName = FileHelpers.NormalizeFilePathSeparators(name);

#if CASE_SENSITIVITY_HACK
			if (Path.IsPathRooted(safeName))
			{
				return OpenStreamCase(safeName);
			}
			return OpenStreamCase(Path.Combine(Location, safeName));
#else
			if (Path.IsPathRooted(safeName))
			{
				return File.OpenRead(safeName);
			}
			return File.OpenRead(Path.Combine(Location, safeName));
#endif
		}

		#endregion

		#region Private Static fcaseopen Method

#if CASE_SENSITIVITY_HACK
		private static Stream OpenStreamCase(string name)
		{
			string[] splits = name.Split(Path.DirectorySeparatorChar);
			splits[0] = "/";
			splits[1] = "/" + splits[1];
			int i;
			for (i = 1; i < splits.Length - 1; i += 1)
			{
				string[] dirs = Directory.GetDirectories(splits[0]);
				foreach (string d in dirs)
				{
					string di = d.Substring(d.LastIndexOf("/") + 1);
					if (splits[i].ToLower().Equals(di.ToLower()))
					{
						splits[i] = di;
						break;
					}
				}
				splits[0] += Path.DirectorySeparatorChar + splits[i];
			}
			string[] files = Directory.GetFiles(splits[0]);
			foreach (string f in files)
			{
				string fi = f.Substring(f.LastIndexOf("/") + 1);
				if (splits[i].ToLower().Equals(fi.ToLower()))
				{
					splits[i] = fi;
					break;
				}
			}
			splits[0] += Path.DirectorySeparatorChar + splits[i];
			return File.OpenRead(splits[0]);
		}
#endif

		#endregion
	}
}

