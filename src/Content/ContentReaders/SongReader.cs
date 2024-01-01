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

using Microsoft.Xna.Framework.Media;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class SongReader : ContentTypeReader<Song>
	{
		#region Private Static Supported File Extensions Variable

		internal static readonly string[] supportedExtensions = new string[] { ".ogg", ".oga", ".qoa" };

		#endregion

		#region Protected Read Method

		protected internal override Song Read(ContentReader input, Song existingInstance)
		{
			string path = MonoGame.Utilities.FileHelpers.ResolveRelativePath(
				Path.Combine(
					input.ContentManager.RootDirectoryFullPath,
					input.AssetName
				),
				input.ReadString()
			);

			/* The path string includes the ".wma" extension. Let's see if this
			 * file exists in a format we actually support...
			 */
			path = Normalize(path.Substring(0, path.Length - 4));
			if (String.IsNullOrEmpty(path))
			{
				throw new ContentLoadException();
			}

			int durationMs = input.ReadInt32();

			return new Song(path, durationMs);
		}

		#endregion

		#region Private Static Extension Check Method

		private static string Normalize(string fileName)
		{
			if (File.Exists(fileName))
			{
				return fileName;
			}
			foreach (string ext in supportedExtensions)
			{
				// Concatenate the file name with valid extensions.
				string fileNamePlusExt = fileName + ext;
				if (File.Exists(fileNamePlusExt))
				{
					return fileNamePlusExt;
				}
			}
			return null;
		}

		#endregion
	}
}
