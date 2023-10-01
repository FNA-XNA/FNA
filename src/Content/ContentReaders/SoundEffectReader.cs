#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2023 Ethan Lee and the MonoGame Team
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
		#region Protected Read Method

		protected internal override SoundEffect Read(
			ContentReader input,
			SoundEffect existingInstance
		) {
			/* Swap endian - this is one of the very few places requiring this!
			 * Note: This only affects the fmt chunk that's glued into the file.
			 */
			bool se = input.platform == 'x';

			// Format block length
			uint formatLength = input.ReadUInt32();

			// WaveFormatEx data
			ushort wFormatTag = Swap(se, input.ReadUInt16());
			ushort nChannels = Swap(se, input.ReadUInt16());
			uint nSamplesPerSec = Swap(se, input.ReadUInt32());
			uint nAvgBytesPerSec = Swap(se, input.ReadUInt32());
			ushort nBlockAlign = Swap(se, input.ReadUInt16());
			ushort wBitsPerSample = Swap(se, input.ReadUInt16());

			byte[] extra = null;
			if (formatLength > 16)
			{
				ushort cbSize = Swap(se, input.ReadUInt16());

				if (wFormatTag == 0x166 && cbSize == 34)
				{
					// XMA2 has got some nice extra crap.
					extra = new byte[34];
					using (MemoryStream extraStream = new MemoryStream(extra))
					using (BinaryWriter extraWriter = new BinaryWriter(extraStream))
					{
						// See FAudio.FAudioXMA2WaveFormatEx for the layout.
						extraWriter.Write(Swap(se, input.ReadUInt16()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(Swap(se, input.ReadUInt32()));
						extraWriter.Write(input.ReadByte());
						extraWriter.Write(input.ReadByte());
						extraWriter.Write(Swap(se, input.ReadUInt16()));
					}
					// Is there any crap that needs skipping? Eh whatever.
					input.ReadBytes((int) (formatLength - 18 - 34));
				}
				else
				{
					// Seek past the rest of this crap (cannot seek though!)
					input.ReadBytes((int) (formatLength - 18));
				}
			}

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
				extra,
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

		#region Internal Static Swapping Methods

		internal static ushort Swap(bool swap, ushort x)
		{
			return !swap ? x : (ushort) (
				((x >> 8)	& 0x00FF) |
				((x << 8)	& 0xFF00)
			);
		}

		internal static uint Swap(bool swap, uint x)
		{
			return !swap ? x : (
				((x >> 24)	& 0x000000FF) |
				((x >> 8)	& 0x0000FF00) |
				((x << 8)	& 0x00FF0000) |
				((x << 24)	& 0xFF000000)
			);
		}

		#endregion

	}
}
