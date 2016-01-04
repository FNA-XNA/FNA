#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;

using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Utilities;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class VideoReader : ContentTypeReader<Video>
	{
		#region Private Supported File Extensions Variable

		static string[] supportedExtensions = new string[] { ".ogv", ".ogg" };

		#endregion

		#region Internal Filename Normalizer Method

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		#endregion

		#region Protected Read Method

		protected internal override Video Read(
			ContentReader input,
			Video existingInstance
		) {
			string path = FileHelpers.ResolveRelativePath(
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

			return new Video(path, durationMS, width, height, framesPerSecond, soundTrackType);
		}

		#endregion
	}
}
