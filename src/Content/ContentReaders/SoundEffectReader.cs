#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.IO;

using Microsoft.Xna.Framework.Audio;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class SoundEffectReader : ContentTypeReader<SoundEffect>
	{
		#region Private Supported File Extensions Variable

		static string[] supportedExtensions = new string[] { ".wav" };

		#endregion

		#region Internal Filename Normalizer Method

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		#endregion

		#region Protected Read Method

		protected internal override SoundEffect Read(
			ContentReader input,
			SoundEffect existingInstance
		) {
			// Format block length
			uint formatLength = input.ReadUInt32();

			// WaveFormatEx data
			ushort wFormatTag = input.ReadUInt16();
			ushort nChannels = input.ReadUInt16();
			uint nSamplesPerSec = input.ReadUInt32();
			uint nAvgBytesPerSec = input.ReadUInt32();
			ushort nBlockAlign = input.ReadUInt16();
			ushort wBitsPerSample = input.ReadUInt16();
			/* ushort cbSize =*/ input.ReadUInt16();

			// Seek past the rest of this crap (cannot seek though!)
			input.ReadBytes((int) (formatLength - 18));

			// Wavedata
			byte[] data = input.ReadBytes(input.ReadInt32());

			// Loop information
			int loopStart = input.ReadInt32();
			int loopLength = input.ReadInt32();

			// Sound duration in milliseconds, unused
			input.ReadUInt32();

			return new SoundEffect(
				input.AssetName,
				data,
				0,
				data.Length,
				wFormatTag,
				nChannels,
				nSamplesPerSec,
				nAvgBytesPerSec,
				nBlockAlign,
				wBitsPerSample,
				loopStart,
				loopLength
			);
		}

		#endregion
	}
}
