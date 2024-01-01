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
	internal class VideoReader : ContentTypeReader<Video>
	{
		#region Internal Static Supported File Extensions Variable

		internal static readonly string[] supportedExtensions = new string[] { ".ogv", ".ogg" };

		#endregion

		#region Protected Read Method

		protected internal override Video Read(
			ContentReader input,
			Video existingInstance
		) {
			string path = MonoGame.Utilities.FileHelpers.ResolveRelativePath(
				Path.Combine(
					input.ContentManager.RootDirectoryFullPath,
					input.AssetName
				),
				input.ReadObject<string>()
			);

			/* The path string includes the ".wmv" extension. Let's see if this
			 * file exists in a format we actually support...
			 */
			path = Normalize(path.Substring(0, path.Length - 4));
			if (String.IsNullOrEmpty(path))
			{
				throw new ContentLoadException();
			}

			int durationMS = input.ReadObject<int>();
			int width = input.ReadObject<int>();
			int height = input.ReadObject<int>();
			float framesPerSecond = input.ReadObject<float>();
			VideoSoundtrackType soundTrackType = (VideoSoundtrackType) input.ReadObject<int>();

			return new Video(
				path,
				input.ContentManager.GetGraphicsDevice(),
				durationMS,
				width,
				height,
				framesPerSecond,
				soundTrackType
			);
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
