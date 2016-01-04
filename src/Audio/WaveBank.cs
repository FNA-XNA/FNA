#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* The unxwb project, written by Luigi Auriemma, was released in 2006 under the
 * GNU General Public License, version 2.0:
 *
 * http://www.gnu.org/licenses/gpl-2.0.html
 *
 * While the unxwb project was released under the GPL, Luigi has given express
 * permission to the MonoGame project to use code from unxwb under the MonoGame
 * project license. See LICENSE for details.
 *
 * The unxwb website can be found here:
 *
 * http://aluigi.altervista.org/papers.htm#xbox
 */
#endregion

#region Using Statements
using System;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.wavebank.aspx
	public class WaveBank : IDisposable
	{
		#region Private Sound Entry Container Class

		// Used to store sound entry data, mainly for streaming WaveBanks
		private class SoundStreamEntry
		{
			public uint PlayOffset
			{
				get;
				private set;
			}

			public uint PlayLength
			{
				get;
				private set;
			}

			public uint Codec
			{
				get;
				private set;
			}

			public uint Frequency
			{
				get;
				private set;
			}

			public uint Channels
			{
				get;
				private set;
			}

			public uint LoopOffset
			{
				get;
				private set;
			}

			public uint LoopLength
			{
				get;
				private set;
			}

			public uint Alignment
			{
				get;
				private set;
			}

			public uint BitDepth
			{
				get;
				private set;
			}

			public SoundStreamEntry(
				uint playOffset,
				uint playLength,
				uint codec,
				uint frequency,
				uint channels,
				uint loopOffset,
				uint loopLength,
				uint alignment,
				uint bitDepth
			) {
				PlayOffset = playOffset;
				PlayLength = playLength;
				Codec = codec;
				Frequency = frequency;
				Channels = channels;
				LoopOffset = loopOffset;
				LoopLength = loopLength;
				Alignment = alignment;
				BitDepth = bitDepth;
			}
		}

		#endregion

		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsPrepared
		{
			get;
			private set;
		}

		public bool IsInUse
		{
			get
			{
				throw new NotImplementedException("Cue wave entry dependency tracking!");
			}
		}

		#endregion

		#region Private Variables

		// We keep this in order to Dispose ourselves later.
		private AudioEngine INTERNAL_baseEngine;
		private string INTERNAL_name;

		// These are only used for streaming WaveBanks
		private BinaryReader INTERNAL_waveBankReader;
		private SoundStreamEntry[] INTERNAL_soundStreamEntries;

		// Stores the actual wavedata
		private SoundEffect[] INTERNAL_sounds;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructors

		public WaveBank(
			AudioEngine audioEngine,
			string nonStreamingWaveBankFilename
		) {
			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(nonStreamingWaveBankFilename))
			{
				throw new ArgumentNullException("nonStreamingWaveBankFilename");
			}

			using (Stream stream = TitleContainer.OpenStream(nonStreamingWaveBankFilename))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				LoadWaveBank(audioEngine, reader, false);
			}
		}

		public WaveBank(
			AudioEngine audioEngine,
			string streamingWaveBankFilename,
			int offset,
			short packetsize
		) {
			/* Note that offset and packetsize go unused,
			 * because we're frauds and aren't actually streaming.
			 * -flibit
			 */

			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(streamingWaveBankFilename))
			{
				throw new ArgumentNullException("streamingWaveBankFilename");
			}

			INTERNAL_waveBankReader = new BinaryReader(
				TitleContainer.OpenStream(streamingWaveBankFilename)
			);
			LoadWaveBank(audioEngine, INTERNAL_waveBankReader, true);
		}

		#endregion

		#region Destructor

		~WaveBank()
		{
			Dispose(true);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

		#region Protected Dispose Method

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (Disposing != null)
				{
					Disposing.Invoke(this, null);
				}
				foreach (SoundEffect se in INTERNAL_sounds)
				{
					if (se != null)
					{
						se.Dispose();
					}
				}
				INTERNAL_baseEngine.INTERNAL_removeWaveBank(INTERNAL_name);
				INTERNAL_sounds = null;
				if (INTERNAL_waveBankReader != null)
				{
					INTERNAL_waveBankReader.Close();
					INTERNAL_waveBankReader = null;
				}
				IsDisposed = true;
				IsPrepared = false;
			}
		}

		#endregion

		#region Internal Method

		internal SoundEffect INTERNAL_getTrack(ushort track)
		{
			if (INTERNAL_sounds[track] == null)
			{
				LoadWaveEntry(
					INTERNAL_soundStreamEntries[track],
					track,
					INTERNAL_waveBankReader
				);
			}
			return INTERNAL_sounds[track];
		}

		#endregion

		#region Private WaveBank Load Method

		private void LoadWaveBank(AudioEngine audioEngine, BinaryReader reader, bool streaming)
		{
			/* Until we finish the LoadWaveBank process, this WaveBank is NOT
			 * ready to run. For us this doesn't really matter, but the game
			 * could be loading WaveBanks asynchronously, so let's be careful.
			 * -flibit
			 */
			IsPrepared = false;

			INTERNAL_baseEngine = audioEngine;

			// Check the file header. Should be 'WBND'
			if (reader.ReadUInt32() != 0x444E4257)
			{
				throw new ArgumentException("WBND format not recognized!");
			}

			// Check the content version. Assuming XNA4 Refresh.
			if (reader.ReadUInt32() != AudioEngine.ContentVersion)
			{
				throw new ArgumentException("WBND Content version!");
			}

			// Check the tool version. Assuming XNA4 Refresh.
			if (reader.ReadUInt32() != 44)
			{
				throw new ArgumentException("WBND Tool version!");
			}

			// Obtain WaveBank chunk offsets/lengths
			uint[] offsets = new uint[5];
			uint[] lengths = new uint[5];
			for (int i = 0; i < 5; i += 1)
			{
				offsets[i] = reader.ReadUInt32();
				lengths[i] = reader.ReadUInt32();
			}

			// Seek to the first offset, obtain WaveBank info
			reader.BaseStream.Seek(offsets[0], SeekOrigin.Begin);

			// IsStreaming bool, unused
			reader.ReadUInt16();

			// WaveBank Flags
			ushort wavebankFlags = reader.ReadUInt16();
			// bool containsEntryNames =	(wavebankFlags & 0x0001) != 0;
			bool compact =			(wavebankFlags & 0x0002) != 0;
			// bool syncDisabled =		(wavebankFlags & 0x0004) != 0;
			// bool containsSeekTables =	(wavebankFlags & 0x0008) != 0;

			// WaveBank Entry Count
			uint numEntries = reader.ReadUInt32();

			// WaveBank Name
			INTERNAL_name = System.Text.Encoding.UTF8.GetString(
				reader.ReadBytes(64), 0, 64
			).Replace("\0", "");

			// WaveBank entry information
			uint metadataElementSize = reader.ReadUInt32();
			reader.ReadUInt32(); // nameElementSize
			uint alignment = reader.ReadUInt32();

			// Determine the generic play region offset
			uint playRegionOffset = offsets[4];
			if (playRegionOffset == 0)
			{
				playRegionOffset = offsets[1] + (numEntries * metadataElementSize);
			}

			// Entry format. Read early for Compact data
			uint entryFormat = 0;
			if (compact)
			{
				entryFormat = reader.ReadUInt32();
			}

			// Read in the wavedata
			INTERNAL_sounds = new SoundEffect[numEntries];
			if (streaming)
			{
				INTERNAL_soundStreamEntries = new SoundStreamEntry[numEntries];
			}
			uint curOffset = offsets[1];
			for (int curEntry = 0; curEntry < numEntries; curEntry += 1)
			{
				// Seek to the current entry
				reader.BaseStream.Seek(curOffset, SeekOrigin.Begin);

				// Entry Information
				uint entryPlayOffset = 0;
				uint entryPlayLength = 0;
				uint entryLoopOffset = 0;
				uint entryLoopLength = 0;

				// Obtain Entry Information
				if (compact)
				{
					uint entryLength = reader.ReadUInt32();

					entryPlayOffset =
						(entryLength & ((1 << 21) - 1)) *
						alignment;
					entryPlayLength =
						(entryLength >> 21) & ((1 << 11) - 1);

					// FIXME: Deviation Length
					reader.BaseStream.Seek(
						curOffset + metadataElementSize,
						SeekOrigin.Begin
					);

					if (curEntry == (numEntries - 1))
					{
						// Last track, last length.
						entryLength = lengths[4];
					}
					else
					{
						entryLength = (
							(
							reader.ReadUInt32() &
							((1 << 21) - 1)
							) * alignment
						);
					}
					entryPlayLength = entryLength - entryPlayOffset;
				}
				else
				{
					if (metadataElementSize >= 4)
						reader.ReadUInt32(); // Flags/Duration, unused
					if (metadataElementSize >= 8)
						entryFormat = reader.ReadUInt32();
					if (metadataElementSize >= 12)
						entryPlayOffset = reader.ReadUInt32();
					if (metadataElementSize >= 16)
						entryPlayLength = reader.ReadUInt32();
					if (metadataElementSize >= 20)
						entryLoopOffset = reader.ReadUInt32();
					if (metadataElementSize >= 24)
						entryLoopLength = reader.ReadUInt32();
					else
					{
						// FIXME: This is a bit hacky.
						if (entryPlayLength != 0)
						{
							entryPlayLength = lengths[4];
						}
					}
				}

				// Update seek offsets
				curOffset += metadataElementSize;
				entryPlayOffset += playRegionOffset;

				// Parse Format for Wavedata information
				uint entryCodec =	(entryFormat >> 0)		& ((1 << 2) - 1);
				uint entryChannels =	(entryFormat >> 2)		& ((1 << 3) - 1);
				uint entryFrequency =	(entryFormat >> (2 + 3))	& ((1 << 18) - 1);
				uint entryAlignment =	(entryFormat >> (2 + 3 + 18))	& ((1 << 8) - 1);
				uint entryBitDepth =	(entryFormat >> (2 + 3 + 18 + 8));

				if (streaming)
				{
					INTERNAL_soundStreamEntries[curEntry] = new SoundStreamEntry(
						entryPlayOffset,
						entryPlayLength,
						entryCodec,
						entryFrequency,
						entryChannels,
						entryLoopOffset,
						entryLoopLength,
						entryAlignment,
						entryBitDepth
					);
				}
				else
				{
					SoundStreamEntry filler = new SoundStreamEntry(
						entryPlayOffset,
						entryPlayLength,
						entryCodec,
						entryFrequency,
						entryChannels,
						entryLoopOffset,
						entryLoopLength,
						entryAlignment,
						entryBitDepth
					);
					LoadWaveEntry(filler, (ushort) curEntry, reader);
				}
			}

			// Add this WaveBank to the AudioEngine Dictionary
			audioEngine.INTERNAL_addWaveBank(INTERNAL_name, this);

			// Finally.
			IsDisposed = false;
			IsPrepared = true;
		}

		#endregion

		#region Private WaveBank Entry Load Method

		private void LoadWaveEntry(SoundStreamEntry entry, ushort track, BinaryReader reader)
		{
			// Read Wavedata
			reader.BaseStream.Seek(entry.PlayOffset, SeekOrigin.Begin);
			byte[] entryData = reader.ReadBytes((int) entry.PlayLength);

			// Load SoundEffect based on codec
			if (entry.Codec == 0x0) // PCM
			{
				INTERNAL_sounds[track] = new SoundEffect(
					"WaveBank Sound",
					entryData,
					entry.Frequency,
					entry.Channels,
					entry.LoopOffset,
					entry.LoopLength,
					false,
					entry.BitDepth
				);
			}
			else if (entry.Codec == 0x2) // ADPCM
			{
				INTERNAL_sounds[track] = new SoundEffect(
					"WaveBank Sound",
					entryData,
					entry.Frequency,
					entry.Channels,
					entry.LoopOffset,
					entry.LoopLength,
					true,
					(entry.Alignment + 16) * 2
				);
			}
			else // Includes 0x1 - XMA, 0x3 - WMA
			{
				throw new NotSupportedException("Rebuild your WaveBanks with ADPCM!");
			}
		}

		#endregion
	}
}
