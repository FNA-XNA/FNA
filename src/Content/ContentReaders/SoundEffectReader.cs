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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class SoundEffectReader : ContentTypeReader<SoundEffect>
	{
		#region Protected Read Method

		protected internal override unsafe SoundEffect Read(
			ContentReader input,
			SoundEffect existingInstance
		) {
			// Format block
			byte[] formatBytes = input.ReadBytes(input.ReadInt32());

			/* Swap endian - this is one of the very few places requiring this!
			 * Note: This only affects the fmt chunk that's glued into the file.
			 */
			if (input.platform == 'x')
			{
				fixed (byte* ptr = formatBytes)
				{
					FAudio.FAudioWaveFormatEx* wfx = (FAudio.FAudioWaveFormatEx*) ptr;
					wfx->wFormatTag = Swap(wfx->wFormatTag);
					wfx->nChannels = Swap(wfx->nChannels);
					wfx->nSamplesPerSec = Swap(wfx->nSamplesPerSec);
					wfx->nAvgBytesPerSec = Swap(wfx->nAvgBytesPerSec);
					wfx->nBlockAlign = Swap(wfx->nBlockAlign);
					wfx->wBitsPerSample = Swap(wfx->wBitsPerSample);
					if (formatBytes.Length > 16)
					{
						wfx->cbSize = Swap(wfx->cbSize);
						if (wfx->wFormatTag == 0x166 && wfx->cbSize == 34)
						{
							FAudio.FAudioXMA2WaveFormatEx* xma2format = (FAudio.FAudioXMA2WaveFormatEx*) ptr;
							xma2format->wNumStreams = Swap(xma2format->wNumStreams);
							xma2format->dwChannelMask = Swap(xma2format->dwChannelMask);
							xma2format->dwSamplesEncoded = Swap(xma2format->dwSamplesEncoded);
							xma2format->dwBytesPerBlock = Swap(xma2format->dwBytesPerBlock);
							xma2format->dwPlayBegin = Swap(xma2format->dwPlayBegin);
							xma2format->dwPlayLength = Swap(xma2format->dwPlayLength);
							xma2format->dwLoopBegin = Swap(xma2format->dwLoopBegin);
							xma2format->dwLoopLength = Swap(xma2format->dwLoopLength);
							xma2format->wBlockCount = Swap(xma2format->wBlockCount);
						}
					}
				}
			}

			// Wavedata
			byte[] data = input.ReadBytes(input.ReadInt32());

			// Loop information
			int loopStart = input.ReadInt32();
			int loopLength = input.ReadInt32();

			// Sound duration in milliseconds, unused
			input.ReadUInt32();

			IntPtr formatPtr = FNAPlatform.Malloc(formatBytes.Length);
			Marshal.Copy(formatBytes, 0, formatPtr, formatBytes.Length);

			return new SoundEffect().FromBuffer(
				input.AssetName,
				formatPtr,
				data,
				0,
				data.Length,
				loopStart,
				loopLength
			);
		}

		#endregion

		#region Internal Static Swapping Methods

		internal static ushort Swap(ushort x)
		{
			return (ushort) (
				((x >> 8)	& 0x00FF) |
				((x << 8)	& 0xFF00)
			);
		}

		internal static uint Swap(uint x)
		{
			return (
				((x >> 24)	& 0x000000FF) |
				((x >> 8)	& 0x0000FF00) |
				((x << 8)	& 0x00FF0000) |
				((x << 24)	& 0xFF000000)
			);
		}

		#endregion
	}
}
