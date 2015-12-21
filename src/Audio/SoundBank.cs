#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundbank.aspx
	public class SoundBank : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsInUse
		{
			get
			{
				throw new NotImplementedException("Bank Cue instance count tracking!");
			}
		}

		#endregion

		#region Private Variables

		private AudioEngine INTERNAL_baseEngine;

		private List<string> INTERNAL_waveBankNames;
		private Dictionary<string, CueData> INTERNAL_cueData;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructor

		public SoundBank(AudioEngine audioEngine, string filename)
		{
			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}

			INTERNAL_baseEngine = audioEngine;

			using (Stream soundBankStream = TitleContainer.OpenStream(filename))
			using (BinaryReader reader = new BinaryReader(soundBankStream))
			{
				// Check the file header. Should be 'SDBK'
				if (reader.ReadUInt32() != 0x4B424453)
				{
					throw new ArgumentException("SDBK format not recognized!");
				}

				// Check the content version. Assuming XNA4 Refresh.
				if (reader.ReadUInt16() != AudioEngine.ContentVersion)
				{
					throw new ArgumentException("SDBK Content version!");
				}

				// Check the tool version. Assuming XNA4 Refresh.
				if (reader.ReadUInt16() != 43)
				{
					throw new ArgumentException("SDBK Tool version!");
				}

				// CRC, unused
				reader.ReadUInt16();

				// Last modified, unused
				reader.ReadUInt64();

				// Unknown value, Internet suggests platform
				reader.ReadByte();

				// Cue Counts
				ushort numCueSimple = reader.ReadUInt16();
				ushort numCueComplex = reader.ReadUInt16();

				// Unknown value
				reader.ReadUInt16();

				// Total Cues, unused
				reader.ReadUInt16();

				// Number of associated WaveBanks
				byte numWaveBanks = reader.ReadByte();

				// Unknown, Internet suggest number of "sounds"
				reader.ReadUInt16();

				// Cue Name Table Length
				ushort cueNameTableLength = reader.ReadUInt16();

				// Unknown value
				reader.ReadUInt16();

				// Cue Offsets
				uint cueSimpleOffset = reader.ReadUInt32();
				uint cueComplexOffset = reader.ReadUInt32();

				// Cue Name Table Offset
				uint cueNameTableOffset = reader.ReadUInt32();

				// Unknown value
				reader.ReadUInt32();

				// Variable Tables Offset, unused
				reader.ReadUInt32();

				// Unknown value
				reader.ReadUInt32();

				// WaveBank Name Table Offset
				uint waveBankNameTableOffset = reader.ReadUInt32();

				// Cue Name Hash Offsets, unused
				reader.ReadUInt32();
				reader.ReadUInt32();

				// Unknown value, Internet suggest "sounds" offset
				reader.ReadUInt32();

				// SoundBank Name, unused
				reader.ReadBytes(64);

				// Parse WaveBank names
				soundBankStream.Seek(waveBankNameTableOffset, SeekOrigin.Begin);
				INTERNAL_waveBankNames = new List<string>();
				for (byte i = 0; i < numWaveBanks; i += 1)
				{
					INTERNAL_waveBankNames.Add(
						System.Text.Encoding.UTF8.GetString(
							reader.ReadBytes(64), 0, 64
						).Replace("\0", "")
					);
				}

				// Parse Cue name list
				soundBankStream.Seek(cueNameTableOffset, SeekOrigin.Begin);
				string[] cueNames = System.Text.Encoding.UTF8.GetString(
					reader.ReadBytes(cueNameTableLength),
					0,
					cueNameTableLength
				).Split('\0');

				// Create our CueData Dictionary
				INTERNAL_cueData = new Dictionary<string, CueData>();

				// Parse Simple Cues
				soundBankStream.Seek(cueSimpleOffset, SeekOrigin.Begin);
				for (ushort i = 0; i < numCueSimple; i += 1)
				{
					// Cue flags, unused
					reader.ReadByte();

					// Cue Sound Offset
					uint offset = reader.ReadUInt32();

					// Store this for when we're done reading the sound.
					long curPos = reader.BaseStream.Position;

					// Go to the sound in the Bank.
					reader.BaseStream.Seek(offset, SeekOrigin.Begin);

					// Parse the Sound
					INTERNAL_cueData.Add(
						cueNames[i],
						new CueData(new XACTSound(reader))
					);

					// Back to where we were...
					reader.BaseStream.Seek(curPos, SeekOrigin.Begin);
				}

				// Parse Complex Cues
				soundBankStream.Seek(cueComplexOffset, SeekOrigin.Begin);
				for (ushort i = 0; i < numCueComplex; i += 1)
				{
					// Cue flags
					byte cueFlags = reader.ReadByte();

					if ((cueFlags & 0x04) != 0) // FIXME: ???
					{
						// Cue Sound Offset
						uint offset = reader.ReadUInt32();

						// Unknown value
						reader.ReadUInt32();

						// Store this for when we're done reading the sound.
						long curPos = reader.BaseStream.Position;

						// Go to the sound in the bank
						reader.BaseStream.Seek(offset, SeekOrigin.Begin);

						// Parse the Sound
						INTERNAL_cueData.Add(
							cueNames[numCueSimple + i],
							new CueData(new XACTSound(reader))
						);

						// Back to where we were...
						reader.BaseStream.Seek(curPos, SeekOrigin.Begin);
					}
					else
					{
						// Variation Table Offset for this Cue
						uint offset = reader.ReadUInt32();

						// Transition Table Offset for this Cue, unused
						reader.ReadUInt32();

						// Store this for when we're done reading the Variation Table
						long curPos = reader.BaseStream.Position;

						// Seek to the Variation Table in the file
						reader.BaseStream.Seek(offset, SeekOrigin.Begin);

						// Number of Variations in the Table
						ushort numVariations = reader.ReadUInt16();

						// Variation Table Flags
						ushort varTableFlags = reader.ReadUInt16();

						// Unknown value
						reader.ReadUInt16();

						// Probability Control Variable, if applicable
						ushort variable = reader.ReadUInt16();

						// Create data for the CueData
						XACTSound[] cueSounds = new XACTSound[numVariations];
						float[,] cueProbs = new float[numVariations, 2];

						// Used to determine Variation storage format
						int varTableType = (varTableFlags >> 3) & 0x0007;

						for (ushort j = 0; j < numVariations; j += 1)
						{
							if (varTableType == 0)
							{
								// Wave with byte min/max
								ushort track = reader.ReadUInt16();
								byte waveBank = reader.ReadByte();
								byte wMin = reader.ReadByte();
								byte wMax = reader.ReadByte();

								// Create the Sound
								cueSounds[j] = new XACTSound(track, waveBank);

								// Calculate probability based on weight
								cueProbs[j, 0] = wMax / 255.0f;
								cueProbs[j, 1] = wMin / 255.0f;
							}
							else if (varTableType == 1)
							{
								// Complex with byte min/max
								uint varOffset = reader.ReadUInt32();
								byte wMin = reader.ReadByte();
								byte wMax = reader.ReadByte();

								// Store for sound read
								long varPos = reader.BaseStream.Position;

								// Seek to the sound in the Bank
								reader.BaseStream.Seek(varOffset, SeekOrigin.Begin);

								// Read the sound
								cueSounds[j] = new XACTSound(reader);

								// Back to where we were...
								reader.BaseStream.Seek(varPos, SeekOrigin.Begin);

								// Calculate probability based on weight
								cueProbs[j, 0] = wMax / 255.0f;
								cueProbs[j, 1] = wMin / 255.0f;
							}
							else if (varTableType == 3)
							{
								// Complex with float min/max
								uint varOffset = reader.ReadUInt32();
								float wMin = reader.ReadSingle();
								float wMax = reader.ReadSingle();

								// Unknown value
								reader.ReadUInt32();

								// Store for sound read
								long varPos = reader.BaseStream.Position;

								// Seek to the sound in the Bank
								reader.BaseStream.Seek(varOffset, SeekOrigin.Begin);

								// Read the sound
								cueSounds[j] = new XACTSound(reader);

								// Back to where we were...
								reader.BaseStream.Seek(varPos, SeekOrigin.Begin);

								// Calculate probability based on weight
								cueProbs[j, 0] = wMax;
								cueProbs[j, 1] = wMin;
							}
							else if (varTableType == 4)
							{
								// Compact Wave
								ushort track = reader.ReadUInt16();
								byte waveBank = reader.ReadByte();

								// Create the Sound
								cueSounds[j] = new XACTSound(track, waveBank);

								// FIXME: Assume Sound weight is 100%
								cueProbs[j, 0] = 1.0f;
								cueProbs[j, 1] = 0.0f;
							}
							else
							{
								throw new NotSupportedException();
							}
						}

						// Back to where we were...
						reader.BaseStream.Seek(curPos, SeekOrigin.Begin);

						// Add Built CueData to Dictionary
						INTERNAL_cueData.Add(
							cueNames[numCueSimple + i],
							new CueData(
								cueSounds,
								cueProbs,
								(varTableType == 3) ? INTERNAL_baseEngine.INTERNAL_getVariableName(variable) : String.Empty
							)
						);
					}

					// Cue instance limit
					byte instanceLimit = reader.ReadByte();

					// Fade In/Out
					ushort fadeIn = reader.ReadUInt16();
					ushort fadeOut = reader.ReadUInt16();

					// Cue max instance behavior
					byte behavior = reader.ReadByte();

					INTERNAL_cueData[cueNames[numCueSimple + i]].SetLimit(
						instanceLimit,
						behavior,
						fadeIn,
						fadeOut
					);
				}
			}
			IsDisposed = false;
		}

		#endregion

		#region Destructor

		~SoundBank()
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

		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (Disposing != null)
				{
					Disposing.Invoke(this, null);
				}
				INTERNAL_waveBankNames.Clear();
				INTERNAL_cueData.Clear();
				IsDisposed = true;
			}
		}

		#endregion

		#region Public Methods

		public Cue GetCue(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (!INTERNAL_cueData.ContainsKey(name))
			{
				throw new ArgumentException("Cue name not found: " + name);
			}
			return new Cue(
				INTERNAL_baseEngine,
				INTERNAL_waveBankNames,
				name,
				INTERNAL_cueData[name],
				false
			);
		}

		public void PlayCue(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (!INTERNAL_cueData.ContainsKey(name))
			{
				throw new InvalidOperationException("name not found!");
			}
			Cue newCue = new Cue(
				INTERNAL_baseEngine,
				INTERNAL_waveBankNames,
				name,
				INTERNAL_cueData[name],
				true
			);
			newCue.Play();
		}

		public void PlayCue(
			string name,
			AudioListener listener,
			AudioEmitter emitter
		) {
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (!INTERNAL_cueData.ContainsKey(name))
			{
				throw new InvalidOperationException("name not found!");
			}
			Cue newCue = new Cue(
				INTERNAL_baseEngine,
				INTERNAL_waveBankNames,
				name,
				INTERNAL_cueData[name],
				true
			);
			newCue.Apply3D(listener, emitter);
			newCue.Play();
		}

		#endregion
	}
}
