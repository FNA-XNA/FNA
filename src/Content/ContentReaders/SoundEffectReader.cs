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
			/* Swap endian - this is one of the very few places requiring this!
			 * Note: This only affects the fmt chunk that's glued into the file.
			 */
			bool se = input.platform == 'x';

			// Format block length
			int formatLength = input.ReadInt32();

			byte[] format = input.ReadBytes(formatLength);

			// Wavedata
			byte[] data = input.ReadBytes(input.ReadInt32());

			// Loop information
			int loopStart = input.ReadInt32();
			int loopLength = input.ReadInt32();

			// Sound duration in milliseconds, unused
			input.ReadUInt32();

			IntPtr formatPtr = FNAPlatform.Malloc(format.Length);
			Marshal.Copy(format, 0, formatPtr, format.Length);

			if (format.Length >= 16)
			{
				FAudio.FAudioWaveFormatEx* wfx = (FAudio.FAudioWaveFormatEx*) formatPtr;
				wfx->wFormatTag = Swap(se, wfx->wFormatTag);
				wfx->nChannels = Swap(se, wfx->nChannels);
				wfx->nSamplesPerSec = Swap(se, wfx->nSamplesPerSec);
				wfx->nAvgBytesPerSec = Swap(se, wfx->nAvgBytesPerSec);
				wfx->nBlockAlign = Swap(se, wfx->nBlockAlign);
				wfx->wBitsPerSample = Swap(se, wfx->wBitsPerSample);
				if (format.Length >= 18)
				{
					wfx->cbSize = Swap(se, wfx->cbSize);
					if (format.Length >= 18 + 34 && wfx->wFormatTag == 0x166 && wfx->cbSize == 34)
					{
						FAudio.FAudioXMA2WaveFormatEx* xma2format = (FAudio.FAudioXMA2WaveFormatEx*) formatPtr;
						xma2format->wNumStreams = Swap(se, xma2format->wNumStreams);
						xma2format->dwChannelMask = Swap(se, xma2format->dwChannelMask);
						xma2format->dwSamplesEncoded = Swap(se, xma2format->dwSamplesEncoded);
						xma2format->dwBytesPerBlock = Swap(se, xma2format->dwBytesPerBlock);
						xma2format->dwPlayBegin = Swap(se, xma2format->dwPlayBegin);
						xma2format->dwPlayLength = Swap(se, xma2format->dwPlayLength);
						xma2format->dwLoopBegin = Swap(se, xma2format->dwLoopBegin);
						xma2format->dwLoopLength = Swap(se, xma2format->dwLoopLength);
						xma2format->wBlockCount = Swap(se, xma2format->wBlockCount);
					}
				}
			}

			return new SoundEffect().FromBuffer(
				input.AssetName,
				data,
				0,
				data.Length,
				formatPtr,
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
