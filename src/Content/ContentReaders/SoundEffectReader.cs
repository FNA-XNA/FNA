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
using System.Runtime.InteropServices;
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

			// Wavedata format
			ushort format = input.ReadUInt16();

			// Number of channels
			ushort channels = input.ReadUInt16();

			// Sample rate
			uint sampleRate = input.ReadUInt32();

			// Averate bytes per second, unused
			input.ReadUInt32();

			// Block alignment, needed for MSADPCM
			ushort blockAlign = input.ReadUInt16();

			// Bit depth
			ushort bitDepth = input.ReadUInt16();

			// cbSize, unused
			input.ReadUInt16();

			// Seek past the rest of this crap (cannot seek though!)
			input.ReadBytes((int) (formatLength - 18));

			// Wavedata
			byte[] data = input.ReadBytes(input.ReadInt32());

			// Loop information
			uint loopStart = input.ReadUInt32();
			uint loopLength = input.ReadUInt32();

			// Sound duration in milliseconds, unused
			input.ReadUInt32();

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var result = new SoundEffect(
				input.AssetName,
				handle.AddrOfPinnedObject(),
				data.Length,
				sampleRate,
				channels,
				loopStart,
				loopLength,
				format == 2,
				(uint) ((format == 2) ? (((blockAlign / channels) - 6) * 2) : (bitDepth / 16))
			);
			handle.Free();
			return result;
		}

		#endregion
	}
}
