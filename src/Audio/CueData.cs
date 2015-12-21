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
	internal class CueData
	{
		public XACTSound[] Sounds
		{
			get;
			private set;
		}

		public ushort Category
		{
			get;
			private set;
		}

		public float[,] Probabilities
		{
			get;
			private set;
		}

		public bool IsUserControlled
		{
			get;
			private set;
		}

		public string UserControlVariable
		{
			get;
			private set;
		}

		public byte InstanceLimit
		{
			get;
			private set;
		}

		public MaxInstanceBehavior MaxCueBehavior
		{
			get;
			private set;
		}

		public ushort FadeInMS
		{
			get;
			private set;
		}

		public ushort FadeOutMS
		{
			get;
			private set;
		}

		public CueData(XACTSound sound)
		{
			Sounds = new XACTSound[1];
			Probabilities = new float[1, 2];

			Sounds[0] = sound;
			Category = sound.Category;
			Probabilities[0, 0] = 1.0f;
			Probabilities[0, 1] = 0.0f;
			IsUserControlled = false;

			// Assume we can have max instances, for now.
			InstanceLimit = 255;
			MaxCueBehavior = MaxInstanceBehavior.ReplaceOldest;
			FadeInMS = 0;
			FadeOutMS = 0;
		}

		public CueData(
			XACTSound[] sounds,
			float[,] probabilities,
			string controlVariable
		) {
			Sounds = sounds;
			Category = Sounds[0].Category; // FIXME: Assumption!
			Probabilities = probabilities;
			IsUserControlled = !String.IsNullOrEmpty(controlVariable);
			UserControlVariable = controlVariable;

			// Assume we can have max instances, for now.
			InstanceLimit = 255;
			MaxCueBehavior = MaxInstanceBehavior.ReplaceOldest;
			FadeInMS = 0;
			FadeOutMS = 0;
		}

		public void SetLimit(
			byte instanceLimit,
			byte behavior,
			ushort fadeIn,
			ushort fadeOut
		) {
			InstanceLimit = instanceLimit;
			MaxCueBehavior = (MaxInstanceBehavior) (behavior >> 3);
			FadeInMS = fadeIn;
			FadeOutMS = fadeOut;
		}
	}

	internal class XACTSound
	{
		private XACTClip[] INTERNAL_clips;

		public double Volume
		{
			get;
			private set;
		}

		public float Pitch
		{
			get;
			private set;
		}

		public ushort Category
		{
			get;
			private set;
		}

		public bool HasLoadedTracks
		{
			get;
			private set;
		}

		public List<uint[]> RPCCodes
		{
			get;
			private set;
		}

		public uint[] DSPCodes
		{
			get;
			private set;
		}

		public XACTSound(ushort track, byte waveBank)
		{
			INTERNAL_clips = new XACTClip[1];
			INTERNAL_clips[0] = new XACTClip(track, waveBank);
			Category = 0;
			Volume = 0.0;
			HasLoadedTracks = false;
		}

		public XACTSound(BinaryReader reader)
		{
			// Sound Effect Flags
			byte soundFlags = reader.ReadByte();
			bool complex = (soundFlags & 0x01) != 0;

			// AudioCategory Index
			Category = reader.ReadUInt16();

			// Sound Volume
			Volume = XACTCalculator.ParseDecibel(reader.ReadByte());

			// Sound Pitch
			Pitch = (reader.ReadInt16() / 1000.0f);

			// Unknown value
			reader.ReadByte();

			// Length of Sound Entry, unused
			reader.ReadUInt16();

			// Number of Sound Clips
			if (complex)
			{
				INTERNAL_clips = new XACTClip[reader.ReadByte()];
			}
			else
			{
				// Simple Sounds always have 1 PlayWaveEvent.
				INTERNAL_clips = new XACTClip[1];
				ushort track = reader.ReadUInt16();
				byte waveBank = reader.ReadByte();
				INTERNAL_clips[0] = new XACTClip(track, waveBank);
			}

			// Parse RPC Properties
			RPCCodes = new List<uint[]>();
			if ((soundFlags & 0x0E) != 0)
			{
				// RPC data length
				ushort rpcDataLength = reader.ReadUInt16();
				ushort totalDataRead = 2;

				while (totalDataRead < rpcDataLength)
				{
					// Number of RPC Presets (for this track)
					uint[] codeList = new uint[reader.ReadByte()];

					// Obtain RPC curve codes (in this block)
					for (int i = 0; i < codeList.Length; i += 1)
					{
						codeList[i] = reader.ReadUInt32();
					}

					// Add this track's code list to the master list
					RPCCodes.Add(codeList);

					totalDataRead += (ushort) (1 + (4 * codeList.Length));
				}
			}

			// Parse DSP Presets
			DSPCodes = new uint[0]; // Eww... -flibit
			if ((soundFlags & 0x10) != 0)
			{
				// DSP Presets Length, unused
				reader.ReadUInt16();

				// Number of DSP Presets
				DSPCodes = new uint[reader.ReadByte()];

				// Obtain DSP Preset codes
				for (byte j = 0; j < DSPCodes.Length; j += 1)
				{
					DSPCodes[j] = reader.ReadUInt32();
				}
			}

			// Parse Sound Events
			if (complex)
			{
				for (int i = 0; i < INTERNAL_clips.Length; i += 1)
				{
					// XACT Clip volume
					double clipVolume = XACTCalculator.ParseDecibel(reader.ReadByte());

					// XACT Clip Offset in Bank
					uint offset = reader.ReadUInt32();

					// XACT Clip filter
					byte filterFlags = reader.ReadByte();
					byte filterType;
					if ((filterFlags & 0x01) == 0x01)
					{
						filterType = (byte) ((filterFlags >> 1) & 0x02);
					}
					else
					{
						filterType = 0xFF;
					}
					reader.ReadByte(); // QFactor?
					reader.ReadUInt16(); // Frequency

					// Store this for when we're done reading the clip.
					long curPos = reader.BaseStream.Position;

					// Go to the Clip in the Bank.
					reader.BaseStream.Seek(offset, SeekOrigin.Begin);

					// Parse the Clip.
					INTERNAL_clips[i] = new XACTClip(reader, clipVolume, filterType);

					// Back to where we were...
					reader.BaseStream.Seek(curPos, SeekOrigin.Begin);
				}
			}

			HasLoadedTracks = false;
		}

		public void LoadTracks(AudioEngine audioEngine, List<string> waveBankNames)
		{
			foreach (XACTClip curClip in INTERNAL_clips)
			{
				curClip.LoadTracks(audioEngine, waveBankNames);
			}
			HasLoadedTracks = true;
		}

		public void GatherEvents(List<XACTEvent> eventList)
		{
			foreach (XACTClip curClip in INTERNAL_clips)
			{
				eventList.AddRange(curClip.Events);
			}
		}
	}

	internal class XACTClip
	{
		public XACTEvent[] Events
		{
			get;
			private set;
		}

		public XACTClip(ushort track, byte waveBank)
		{
			Events = new XACTEvent[1];
			Events[0] = new PlayWaveEvent(
				0,
				new ushort[] { track },
				new byte[] { waveBank },
				0,
				0,
				1.0,
				1.0,
				0xFF,
				0,
				0,
				false,
				new byte[] { 0xFF }
			);
		}

		public XACTClip(BinaryReader reader, double clipVolume, byte filterType)
		{
			// Number of XACT Events
			Events = new XACTEvent[reader.ReadByte()];

			for (int i = 0; i < Events.Length; i += 1)
			{
				// Full Event information
				uint eventInfo = reader.ReadUInt32();

				// XACT Event Type, Timestamp
				uint eventType = eventInfo & 0x0000001F;
				uint eventTimestamp = (eventInfo >> 5) & 0x0000FFFF;
				// uint eventUnknown = eventInfo >> 21;

				// Random offset, unused
				reader.ReadUInt16();

				// Load the Event
				if (eventType == 0)
				{
					// TODO: Codename OhGodNo
					// Stop Event
				}
				else if (eventType == 1)
				{
					// Unknown value
					reader.ReadByte();

					/* Event Flags
					 * 0x01 = Break Loop
					 * 0x02 = Use Speaker Position
					 * 0x04 = Use Center Speaker
					 * 0x08 = New Speaker Position On Loop
					 */
					reader.ReadByte();

					// WaveBank Track Index
					ushort track = reader.ReadUInt16();

					// WaveBank Index
					byte waveBank = reader.ReadByte();

					// Number of times to loop wave (255 is infinite)
					byte loopCount = reader.ReadByte();

					// Speaker position angle/arc, unused
					reader.ReadUInt16();
					reader.ReadUInt16();

					// Finally.
					Events[i] = new PlayWaveEvent(
						eventTimestamp,
						new ushort[] { track },
						new byte[] { waveBank },
						0,
						0,
						clipVolume,
						clipVolume,
						filterType,
						loopCount,
						0,
						false,
						new byte[] { 0xFF }
					);
				}
				else if (eventType == 3)
				{
					// Unknown value
					reader.ReadByte();

					/* Event Flags
					 * 0x01 = Break Loop
					 * 0x02 = Use Speaker Position
					 * 0x04 = Use Center Speaker
					 * 0x08 = New Speaker Position On Loop
					 */
					reader.ReadByte();

					// Number of times to loop wave (255 is infinite)
					byte loopCount = reader.ReadByte();

					// Speaker position angle/arc, unused
					reader.ReadUInt16();
					reader.ReadUInt16();

					// Number of WaveBank tracks
					ushort numTracks = reader.ReadUInt16();

					/* Variation Playlist Type.
					 * First 4 bytes indicates Variation Type.
					 * Next 4 bytes appear to indicate New Variation On Loop.
					 * The rest is currently unknown.
					 * -flibit
					 */
					ushort variationValues = reader.ReadUInt16();
					ushort variationType = (ushort)(variationValues & 0x000F);
					bool variationOnLoop = (variationValues & 0x00F0) > 0;

					// Unknown values
					reader.ReadBytes(4);

					// Obtain WaveBank track information
					ushort[] tracks = new ushort[numTracks];
					byte[] waveBanks = new byte[numTracks];
					byte[] weights = new byte[numTracks];
					for (ushort j = 0; j < numTracks; j += 1)
					{
						tracks[j] = reader.ReadUInt16();
						waveBanks[j] = reader.ReadByte();
						byte minWeight = reader.ReadByte();
						byte maxWeight = reader.ReadByte();
						weights[j] = (byte) (maxWeight - minWeight);
					}

					// Finally.
					Events[i] = new PlayWaveEvent(
						eventTimestamp,
						tracks,
						waveBanks,
						0,
						0,
						clipVolume,
						clipVolume,
						filterType,
						loopCount,
						variationType,
						variationOnLoop,
						weights
					);
				}
				else if (eventType == 4)
				{
					// Unknown value
					reader.ReadByte();

					/* Event Flags
					 * 0x01 = Break Loop
					 * 0x02 = Use Speaker Position
					 * 0x04 = Use Center Speaker
					 * 0x08 = New Speaker Position On Loop
					 */
					reader.ReadByte();
					
					// WaveBank track
					ushort track = reader.ReadUInt16();
					
					// WaveBank index, unconfirmed
					byte waveBank = reader.ReadByte();
					
					// Loop Count, unconfirmed
					byte loopCount = reader.ReadByte();
					
					// Speaker position angle/arc, unused
					reader.ReadUInt16();
					reader.ReadUInt16();
					
					// Pitch Variation
					short minPitch = reader.ReadInt16();
					short maxPitch = reader.ReadInt16();
					
					// Volume Variation
					double minVolume = XACTCalculator.ParseDecibel(reader.ReadByte());
					double maxVolume = XACTCalculator.ParseDecibel(reader.ReadByte());

					// Frequency Variation, unusued
					reader.ReadSingle();
					reader.ReadSingle();

					// Q Factor Variation, unused
					reader.ReadSingle();
					reader.ReadSingle();

					// Unknown value
					reader.ReadUInt16();
					
					// Finally.
					Events[i] = new PlayWaveEvent(
						eventTimestamp,
						new ushort[] { track },
						new byte[] { waveBank },
						minPitch,
						maxPitch,
						minVolume,
						maxVolume,
						filterType,
						loopCount,
						0,
						false,
						new byte[] { 0xFF }
					);
				}
				else if (eventType == 6)
				{
					// Unknown value
					reader.ReadByte();

					/* Event Flags
					 * 0x01 = Break Loop
					 * 0x02 = Use Speaker Position
					 * 0x04 = Use Center Speaker
					 * 0x08 = New Speaker Position On Loop
					 */
					reader.ReadByte();

					// Number of times to loop wave (255 is infinite)
					byte loopCount = reader.ReadByte();

					// Speaker position angle/arc, unused
					reader.ReadUInt16();
					reader.ReadUInt16();

					// Pitch variation
					short minPitch = reader.ReadInt16();
					short maxPitch = reader.ReadInt16();

					// Volume variation
					double minVolume = XACTCalculator.ParseDecibel(reader.ReadByte());
					double maxVolume = XACTCalculator.ParseDecibel(reader.ReadByte());

					// Frequency Variation, unusued
					reader.ReadSingle();
					reader.ReadSingle();

					// Q Factor Variation, unused
					reader.ReadSingle();
					reader.ReadSingle();

					// Unknown value
					reader.ReadByte();

					// Variation flags
					// FIXME: There's probably more to these flags...
					byte varFlags = reader.ReadByte();
					if ((varFlags & 0x20) != 0x20)
					{
						// Throw out the volume variation.
						minVolume = clipVolume;
						maxVolume = clipVolume;
					}
					if ((varFlags & 0x10) != 0x10)
					{
						// Throw out the pitch variation
						minPitch = 0;
						maxPitch = 0;
					}

					// Number of WaveBank tracks
					ushort numTracks = reader.ReadUInt16();

					/* Variation Playlist Type.
					 * First 4 bytes indicates Variation Type.
					 * Next 4 bytes appear to indicate New Variation On Loop.
					 * The rest is currently unknown.
					 * -flibit
					 */
					ushort variationValues = reader.ReadUInt16();
					ushort variationType = (ushort)(variationValues & 0x000F);
					bool variationOnLoop = (variationValues & 0x00F0) > 0;

					// Unknown values
					reader.ReadBytes(4);

					// Obtain WaveBank track information
					ushort[] tracks = new ushort[numTracks];
					byte[] waveBanks = new byte[numTracks];
					byte[] weights = new byte[numTracks];
					for (ushort j = 0; j < numTracks; j += 1)
					{
						tracks[j] = reader.ReadUInt16();
						waveBanks[j] = reader.ReadByte();
						byte minWeight = reader.ReadByte();
						byte maxWeight = reader.ReadByte();
						weights[j] = (byte) (maxWeight - minWeight);
					}

					// Finally.
					Events[i] = new PlayWaveEvent(
						eventTimestamp,
						tracks,
						waveBanks,
						minPitch,
						maxPitch,
						minVolume,
						maxVolume,
						filterType,
						loopCount,
						variationType,
						variationOnLoop,
						weights
					);
				}
				else if (eventType == 7)
				{
					// Unknown values
					reader.ReadBytes(2);

					/* Event Flags
					 * 0x08 = Min/Max Values
					 * Rest is unknown
					 */
					bool minMax = (reader.ReadByte() & 0x08) == 0x08;

					// Min/Max Random
					float min = reader.ReadSingle() / 1000.0f;
					float max;
					if (minMax)
					{
						max = reader.ReadSingle() / 1000.0f;
					}
					else
					{
						max = min;
					}

					// FIXME: Any more...? -flibit

					Events[i] = new SetPitchEvent(
						eventTimestamp,
						min,
						max
					);
				}
				else if (eventType == 8)
				{
					// Unknown values
					reader.ReadBytes(2);

					/* Event Flags
					 * 0x08 = Min/Max Values
					 * 0x01 = Add, rather than replace
					 * Rest is unknown
					 */
					byte flags = reader.ReadByte();
					bool addVolume = (flags & 0x01) == 0x01;
					bool minMax = (flags & 0x08) == 0x08;

					// Operand Constant
					float min = reader.ReadSingle() / 100.0f;
					float max;
					if (minMax)
					{
						max = reader.ReadSingle() / 100.0f;

						// Unknown bytes
						reader.ReadBytes(5);
					}
					else
					{
						max = min;

						// Unknown values
						reader.ReadBytes(8);
					}
					if (addVolume)
					{
						min += (float) clipVolume;
						max += (float) clipVolume;
					}

					Events[i] = new SetVolumeEvent(
						eventTimestamp,
						XACTCalculator.CalculateAmplitudeRatio(min),
						XACTCalculator.CalculateAmplitudeRatio(max)
					);
				}
				else if (eventType == 15)
				{
					// TODO: Codename OhGodNo -flibit
					// Unknown Event!
				}
				else if (eventType == 17)
				{
					// TODO: Codename OhGodNo -flibit
					// Volume Repeat Event
				}
				else
				{
					/* TODO: All XACT Events.
					 * The following type information is based on
					 * third-party contributions:
					 * Type 9 - Marker Event
					 * -flibit
					 */
					throw new Exception(
						"EVENT TYPE " + eventType.ToString() + " NOT IMPLEMENTED!"
					);
				}
			}
		}

		public void LoadTracks(AudioEngine audioEngine, List<string> waveBankNames)
		{
			foreach (XACTEvent curEvent in Events)
			{
				if (curEvent.Type == 1)
				{
					((PlayWaveEvent) curEvent).LoadTracks(
						audioEngine,
						waveBankNames
					);
				}
			}
		}
	}

	internal abstract class XACTEvent
	{
		public uint Type
		{
			get;
			private set;
		}

		public uint Timestamp
		{
			get;
			private set;
		}

		protected static Random random = new Random();

		public XACTEvent(uint type, uint timestamp)
		{
			Type = type;
			Timestamp = timestamp;
		}
	}

	internal class PlayWaveEvent : XACTEvent
	{
		private enum VariationPlaylistType : ushort
		{
			Ordered,
			OrderedFromRandom,
			Random,
			RandomNoImmediateRepeats,
			Shuffle
		}

		private ushort[] INTERNAL_tracks;
		private byte[] INTERNAL_waveBanks;

		private short INTERNAL_minPitch;
		private short INTERNAL_maxPitch;

		private double INTERNAL_minVolume;
		private double INTERNAL_maxVolume;

		private byte INTERNAL_filterType;

		private byte INTERNAL_loopCount;

		private VariationPlaylistType INTERNAL_variationType;
		private bool INTERNAL_variationOnLoop;
		private byte[] INTERNAL_weights;
		private int INTERNAL_curWave;

		private SoundEffect[] INTERNAL_waves;

		public PlayWaveEvent(
			uint timestamp,
			ushort[] tracks,
			byte[] waveBanks,
			short minPitch,
			short maxPitch,
			double minVolume,
			double maxVolume,
			byte filterType,
			byte loopCount,
			ushort variationType,
			bool variationOnLoop,
			byte[] weights
		) : base(1, timestamp) {
			INTERNAL_tracks = tracks;
			INTERNAL_waveBanks = waveBanks;
			INTERNAL_minPitch = minPitch;
			INTERNAL_maxPitch = maxPitch;
			INTERNAL_minVolume = minVolume;
			INTERNAL_maxVolume = maxVolume;
			INTERNAL_filterType = filterType;
			INTERNAL_loopCount = loopCount;
			INTERNAL_variationType = (VariationPlaylistType) variationType;
			INTERNAL_variationOnLoop = variationOnLoop;
			INTERNAL_weights = weights;
			INTERNAL_waves = new SoundEffect[tracks.Length];
			INTERNAL_curWave = -1;
		}

		public void LoadTracks(AudioEngine audioEngine, List<string> waveBankNames)
		{
			for (int i = 0; i < INTERNAL_waves.Length; i += 1)
			{
				INTERNAL_waves[i] = audioEngine.INTERNAL_getWaveBankTrack(
					waveBankNames[INTERNAL_waveBanks[i]],
					INTERNAL_tracks[i]
				);
			}
		}

		public SoundEffectInstance GenerateInstance(
			double soundVolume,
			float soundPitch,
			int currentLoop
		) {
			if (currentLoop > INTERNAL_loopCount && INTERNAL_loopCount != 255)
			{
				// We've finished all the loops!
				return null;
			}
			INTERNAL_getNextSound();
			SoundEffectInstance result = INTERNAL_waves[INTERNAL_curWave].CreateInstance();
			result.INTERNAL_isXACTSource = true;
			result.Volume = XACTCalculator.CalculateAmplitudeRatio(
				soundVolume + (
					random.NextDouble() *
					(INTERNAL_maxVolume - INTERNAL_minVolume)
				) + INTERNAL_minVolume
			);
			result.Pitch = (
				random.Next(
					INTERNAL_minPitch,
					INTERNAL_maxPitch
				) / 1000.0f
			) + soundPitch;
			result.FilterType = INTERNAL_filterType;
			result.IsLooped = !INTERNAL_variationOnLoop && (INTERNAL_loopCount == 255);
			return result;
		}

		private void INTERNAL_getNextSound()
		{
			if (INTERNAL_variationType == VariationPlaylistType.Ordered)
			{
				INTERNAL_curWave += 1;
				if (INTERNAL_curWave >= INTERNAL_waves.Length)
				{
					INTERNAL_curWave = 0;
				}
			}
			else if (INTERNAL_variationType == VariationPlaylistType.OrderedFromRandom)
			{
				// FIXME: It seems like XACT organizes this for us?
				INTERNAL_curWave += 1;
				if (INTERNAL_curWave >= INTERNAL_waves.Length)
				{
					INTERNAL_curWave = 0;
				}
			}
			else if (INTERNAL_variationType == VariationPlaylistType.Random)
			{
				double max = 0.0;
				for (int i = 0; i < INTERNAL_weights.Length; i += 1)
				{
					max += INTERNAL_weights[i];
				}
				double next = random.NextDouble() * max;
				for (int i = INTERNAL_weights.Length - 1; i >= 0; i -= 1)
				{
					if (next > max - INTERNAL_weights[i])
					{
						INTERNAL_curWave = i;
						return;
					}
					max -= INTERNAL_weights[i];
				}
			}
			else if (	INTERNAL_variationType == VariationPlaylistType.RandomNoImmediateRepeats ||
					INTERNAL_variationType == VariationPlaylistType.Shuffle	)
			{
				// FIXME: Is Shuffle really any different from this?
				double max = 0.0;
				for (int i = 0; i < INTERNAL_weights.Length; i += 1)
				{
					if (i == INTERNAL_curWave)
					{
						continue;
					}
					max += INTERNAL_weights[i];
				}
				double next = random.NextDouble() * max;
				for (int i = INTERNAL_weights.Length - 1; i >= 0; i -= 1)
				{
					if (i == INTERNAL_curWave)
					{
						continue;
					}
					if (next > max - INTERNAL_weights[i])
					{
						INTERNAL_curWave = i;
						return;
					}
					max -= INTERNAL_weights[i];
				}
			}
			else
			{
				throw new Exception(
					"Variation Playlist Type unhandled: " +
					INTERNAL_variationType.ToString()
				);
			}
		}
	}

	internal class SetVolumeEvent : XACTEvent
	{
		private float INTERNAL_min;
		private float INTERNAL_max;

		public SetVolumeEvent(
			uint timestamp,
			float min,
			float max
		) : base(2, timestamp) {
			INTERNAL_min = min;
			INTERNAL_max = max;
		}

		public float GetVolume()
		{
			return INTERNAL_min + (float) (
				random.NextDouble() * (INTERNAL_max - INTERNAL_min)
			);
		}
	}

	internal class SetPitchEvent : XACTEvent
	{
		private float INTERNAL_min;
		private float INTERNAL_max;

		public SetPitchEvent(
			uint timestamp,
			float min,
			float max
		) : base(3, timestamp) {
			INTERNAL_min = min;
			INTERNAL_max = max;
		}

		public float GetPitch()
		{
			return INTERNAL_min + (float) (
				random.NextDouble() * (INTERNAL_max - INTERNAL_min)
			);
		}
	}
}
