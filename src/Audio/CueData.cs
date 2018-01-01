#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		internal XACTClip[] INTERNAL_clips;

		public double Volume
		{
			get;
			private set;
		}

		public short Pitch
		{
			get;
			private set;
		}

		public ushort Category
		{
			get;
			private set;
		}

		public readonly bool HasSoundRpcs;

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

		private List<XACTSoundInstance> instances;

		public XACTSound(ushort track, byte waveBank)
		{
			INTERNAL_clips = new XACTClip[1];
			INTERNAL_clips[0] = new XACTClip(track, waveBank);
			Category = 0;
			Volume = 0.0;

			instances = new List<XACTSoundInstance>();
		}

		public XACTSound(BinaryReader reader)
		{
			instances = new List<XACTSoundInstance>();

			// Sound Effect Flags
			byte soundFlags = reader.ReadByte();
			bool complex = (soundFlags & 0x01) != 0;
			
			// Indicates that the data contains RPC codes targeting the sound itself.
			HasSoundRpcs = (soundFlags & 0x02) == 0x02;

			// AudioCategory Index
			Category = reader.ReadUInt16();

			// Sound Volume
			Volume = XACTCalculator.ParseDecibel(reader.ReadByte());

			// Sound Pitch
			Pitch = reader.ReadInt16();

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

			// Parse Sound Clips
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
		}

		public void GatherEvents(List<XACTEvent> eventList)
		{
			foreach (XACTClip curClip in INTERNAL_clips)
			{
				eventList.AddRange(curClip.Events);
			}
		}

		public XACTSoundInstance GenInstance(
			AudioEngine audioEngine,
			List<string> waveBankNames
		) {
			XACTSoundInstance result = new XACTSoundInstance(this);
			if (instances.Count == 0)
			{
				foreach (XACTClip curClip in INTERNAL_clips)
				{
					curClip.LoadEvents(audioEngine, waveBankNames);
				}
			}
			instances.Add(result);
			return result;
		}

		public void DisposeInstance(
			XACTSoundInstance instance,
			AudioEngine audioEngine,
			List<string> waveBankNames
		) {
			instances.Remove(instance);
			if (instances.Count == 0)
			{
				foreach (XACTClip curClip in INTERNAL_clips)
				{
					curClip.UnloadEvents(audioEngine, waveBankNames);
				}
			}
		}
	}

	internal enum CueProperty
	{
		Volume,
		Pitch
	}

	internal class XACTClip
	{
		internal enum EventTypeCode
		{
			Stop = 0,
			PlayWave = 1,
			PlayWaveWithTrackVariation = 3,
			PlayWaveWithEffectVariation = 4,
			PlayWaveWithTrackAndEffectVariation = 6,
			Pitch = 7,
			Volume = 8,
			Marker = 9,
			PitchRepeating = 16,
			VolumeRepeating = 17,
			MarkerRepeating = 18
		}

		internal enum StopEventScope
		{
			Track = 0x00,
			Cue = 0x02
		}

		internal enum XactEventSettingType
		{
			Equation = 0x00,
			Ramp = 0x01
		}

		internal enum XactEventEquationType
		{
			Value = 0x04,
			Random = 0x08
		}

		internal enum XactEventOp
		{
			Replace = 0x00,
			Add = 0x01
		}

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
				0, // FIXME: Is there such a thing as a random offset for a "simple" instance?
				new ushort[] { track },
				new byte[] { waveBank },
				0,
				0,
				0.0,
				0.0,
				0xFF,
				0,
				false,
				false,
				false,
				false,
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
				EventTypeCode eventType = (EventTypeCode) (eventInfo & 0x0000001F);
				uint eventTimestamp = (eventInfo >> 5) & 0x0000FFFF;
				// uint eventUnknown = eventInfo >> 21;

				ushort randomOffset = reader.ReadUInt16();

				// Unused byte (separator?)
				byte separator = reader.ReadByte();
				Debug.Assert(separator == 0xFF);

				// Load the Event
				XACTEvent evt = null;
				if (eventType == EventTypeCode.Stop)
				{
					evt = StopEvent.ParseMarkerEvent(
						reader,
						eventTimestamp,
						randomOffset
					);
				}
				else if (eventType == EventTypeCode.PlayWave)
				{
					evt = PlayWaveEvent.ParsePlayWaveEvent(
						reader,
						eventTimestamp,
						randomOffset,
						clipVolume,
						filterType
					);
				}
				else if (eventType == EventTypeCode.PlayWaveWithTrackVariation)
				{
					evt = PlayWaveEvent.ParsePlayWaveWithTrackVariation(
						reader,
						eventTimestamp,
						randomOffset,
						clipVolume,
						filterType
					);
				}
				else if (eventType == EventTypeCode.PlayWaveWithEffectVariation)
				{
					evt = PlayWaveEvent.ParsePlayWaveWithEffectVariation(
						reader,
						eventTimestamp,
						randomOffset,
						clipVolume,
						filterType
					);
				}
				else if (eventType == EventTypeCode.PlayWaveWithTrackAndEffectVariation)
				{
					evt = PlayWaveEvent.ParsePlayWaveWithTrackAndEffectVariation(
						reader,
						eventTimestamp,
						randomOffset,
						clipVolume,
						filterType
					);
				}
				else if (eventType == EventTypeCode.Pitch)
				{
					evt = ParseVolumeOrPitchEvent(
						reader,
						eventTimestamp,
						randomOffset,
						CueProperty.Pitch,
						false
					);
				}
				else if (eventType == EventTypeCode.PitchRepeating)
				{
					evt = ParseVolumeOrPitchEvent(
						reader,
						eventTimestamp,
						randomOffset,
						CueProperty.Pitch,
						true
					);
				}
				else if (eventType == EventTypeCode.Volume)
				{
					evt = ParseVolumeOrPitchEvent(
						reader,
						eventTimestamp,
						randomOffset,
						CueProperty.Volume,
						false
					);
				}
				else if (eventType == EventTypeCode.VolumeRepeating)
				{
					evt = ParseVolumeOrPitchEvent(
						reader,
						eventTimestamp,
						randomOffset,
						CueProperty.Volume,
						true
					);
				}
				else if (eventType == EventTypeCode.Marker)
				{
					evt = MarkerEvent.ParseMarkerEvent(
						reader,
						eventTimestamp,
						randomOffset,
						false
					);
				}
				else if (eventType == EventTypeCode.MarkerRepeating)
				{
					evt = MarkerEvent.ParseMarkerEvent(
						reader,
						eventTimestamp,
						randomOffset,
						true
					);
				}
				else
				{
					throw new NotImplementedException(
						"EVENT TYPE " + eventType.ToString() + " NOT IMPLEMENTED!"
					);
				}

				Events[i] = evt;
			}
		}

		private static XACTEvent ParseVolumeOrPitchEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			CueProperty property,
			bool repeating
		) {
			// Read and convert the event setting type (Equation or Ramp).
			XactEventSettingType settingType =
				(XactEventSettingType) (reader.ReadByte() & 0x01);

			if (settingType == XactEventSettingType.Equation)
			{
				/* Event Flags
				 * bit0   - 0=Replace 1=Add
				 * bit1   - Unknown
				 * bit2-3 - 01=Value 10=Random
				 */
				byte eventFlags = reader.ReadByte();
				XactEventEquationType equationType =
					(XactEventEquationType) (eventFlags & (0x04 | 0x08));
				XactEventOp operation = (XactEventOp) (eventFlags & 0x01);

				if (equationType == XactEventEquationType.Value)
				{
					return SetValueEvent.ParseSetValueEvent(
						reader,
						eventTimestamp,
						randomOffset,
						property,
						repeating,
						operation
					);
				}
				if (equationType == XactEventEquationType.Random)
				{
					return SetRandomValueEvent.ParseSetRandomValueEvent(
						reader,
						eventTimestamp,
						randomOffset,
						property,
						repeating,
						operation
					);
				}
				throw new NotImplementedException(
					"Encountered event unexpected equation type."
				);
			}
			if (settingType == XactEventSettingType.Ramp)
			{
				return SetRampValueEvent.ParseSetRampValueEvent(
					reader,
					eventTimestamp,
					randomOffset,
					property
				);
			}

			throw new NotImplementedException("Unknown setting type!");
		}

		public void LoadEvents(AudioEngine audioEngine, List<string> waveBankNames)
		{
			foreach (XACTEvent curEvent in Events)
			{
				if (curEvent is PlayWaveEvent)
				{
					((PlayWaveEvent) curEvent).LoadWaves(
						audioEngine,
						waveBankNames
					);
				}
			}
		}

		public void UnloadEvents(AudioEngine audioEngine, List<string> waveBankNames)
		{
			foreach (XACTEvent curEvent in Events)
			{
				if (curEvent is PlayWaveEvent)
				{
					((PlayWaveEvent) curEvent).UnloadWaves(
						audioEngine,
						waveBankNames
					);
				}
			}
		}
	}

	internal abstract class XACTEvent
	{
		public uint Timestamp
		{
			get;
			private set;
		}

		/* FIXME: This needs to be used when processing events.
		 * Event instances should take the time stamp, apply a random
		 * offset bounded by this, and use it as the instance timestamp.
		 */
		public ushort RandomOffset
		{
			get;
			private set;
		}

		public int LoopCount
		{
			get;
			private set;
		}

		public float Frequency
		{
			get;
			private set;
		}

		public static readonly Random Random = new Random();

		public XACTEvent(uint timestamp, ushort randomOffset)
			: this(timestamp, randomOffset, 0, 0)
		{
		}

		protected XACTEvent(
			uint timestamp,
			ushort randomOffset,
			int loopCount,
			float frequency
		) {
			Timestamp = timestamp;
			RandomOffset = randomOffset;
			LoopCount = loopCount;
			Frequency = frequency;
		}

		public static void ReadRecurrenceData(
			BinaryReader reader,
			out int count,
			out float frequency
		) {
			count = reader.ReadUInt16();
			frequency = reader.ReadUInt16() / 1000.0f;
		}
	}

	internal class StopEvent : XACTEvent
	{
		public readonly AudioStopOptions StopOptions;
		public readonly XACTClip.StopEventScope Scope;

		public StopEvent(
			uint timestamp,
			ushort randomOffset,
			AudioStopOptions stopOptions,
			XACTClip.StopEventScope scope
		) : base(timestamp, randomOffset) {
			StopOptions = stopOptions;
			Scope = scope;
		}

		public static StopEvent ParseMarkerEvent(
			BinaryReader reader,
			uint timestamp,
			ushort randomOffset
		) {
			/* Event Flags
			 * bit0   - Play Release (0), Immediate (1)
			 * bit1   - Stop Track (0), Stop Cue (1)
			 * bit2-7 - Unused
			 */
			byte eventFlags = reader.ReadByte();
			AudioStopOptions stopOptions = ((eventFlags & 0x1) == 1)
				? AudioStopOptions.Immediate
				: AudioStopOptions.AsAuthored;
			XACTClip.StopEventScope scope = (XACTClip.StopEventScope) (eventFlags & 0x02);

			return new StopEvent(timestamp, randomOffset, stopOptions, scope);
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

		private bool INTERNAL_pitchVariationOnLoop;
		private bool INTERNAL_pitchVariationAdd;
		private bool INTERNAL_volumeVariationOnLoop;
		private bool INTERNAL_volumeVariationAdd;
		private VariationPlaylistType INTERNAL_trackVariationType;
		private bool INTERNAL_trackVariationOnLoop;
		private byte[] INTERNAL_weights;
		private int INTERNAL_curWave;

		private SoundEffect[] INTERNAL_waves;

		public PlayWaveEvent(
			uint timestamp,
			ushort randomOffset,
			ushort[] tracks,
			byte[] waveBanks,
			short minPitch,
			short maxPitch,
			double minVolume,
			double maxVolume,
			byte filterType,
			byte loopCount,
			bool pitchVariationOnLoop,
			bool pitchVariationAdd,
			bool volumeVariationOnLoop,
			bool volumeVariationAdd,
			ushort trackVariationType,
			bool trackVariationOnLoop,
			byte[] weights
		) : base(timestamp, randomOffset) {
			INTERNAL_tracks = tracks;
			INTERNAL_waveBanks = waveBanks;
			INTERNAL_minPitch = minPitch;
			INTERNAL_maxPitch = maxPitch;
			INTERNAL_minVolume = minVolume;
			INTERNAL_maxVolume = maxVolume;
			INTERNAL_filterType = filterType;
			INTERNAL_loopCount = loopCount;
			INTERNAL_pitchVariationOnLoop = pitchVariationOnLoop;
			INTERNAL_pitchVariationAdd = pitchVariationAdd;
			INTERNAL_volumeVariationOnLoop = volumeVariationOnLoop;
			INTERNAL_volumeVariationAdd = volumeVariationAdd;
			INTERNAL_trackVariationType = (VariationPlaylistType) trackVariationType;
			INTERNAL_trackVariationOnLoop = trackVariationOnLoop;
			INTERNAL_weights = weights;
			INTERNAL_waves = new SoundEffect[tracks.Length];
			INTERNAL_curWave = -1;
		}

		public void LoadWaves(AudioEngine audioEngine, List<string> waveBankNames)
		{
			for (int i = 0; i < INTERNAL_waves.Length; i += 1)
			{
				INTERNAL_waves[i] = audioEngine.INTERNAL_getWaveBankTrack(
					waveBankNames[INTERNAL_waveBanks[i]],
					INTERNAL_tracks[i]
				);
			}
		}

		public void UnloadWaves(AudioEngine audioEngine, List<string> waveBankNames)
		{
			for (int i = 0; i < INTERNAL_waves.Length; i += 1)
			{
				audioEngine.INTERNAL_dropWaveBankTrack(
					waveBankNames[INTERNAL_waveBanks[i]],
					INTERNAL_tracks[i]
				);
			}
			Array.Clear(INTERNAL_waves, 0, INTERNAL_waves.Length);
		}

		public SoundEffectInstance GenerateInstance(
			double soundVolume,
			short soundPitch,
			int currentLoop,
			double? prevVolume,
			short? prevPitch,
			out double finalVolume,
			out short finalPitch
		) {
			if (currentLoop > INTERNAL_loopCount && INTERNAL_loopCount != 255)
			{
				// We've finished all the loops!
				finalVolume = 0.0;
				finalPitch = 0;
				return null;
			}
			INTERNAL_getNextSound();
			SoundEffectInstance result = INTERNAL_waves[INTERNAL_curWave].CreateInstance();
			result.INTERNAL_isXACTSource = true;

			finalVolume = (
				Random.NextDouble() *
				(INTERNAL_maxVolume - INTERNAL_minVolume)
			) + INTERNAL_minVolume;
			if (INTERNAL_volumeVariationAdd && currentLoop > 0)
			{
				finalVolume += prevVolume.Value;
			}
			else
			{
				finalVolume += soundVolume;
			}
			result.Volume = XACTCalculator.CalculateAmplitudeRatio(finalVolume);

			finalPitch = (short) Random.Next(
				INTERNAL_minPitch,
				INTERNAL_maxPitch
			);
			if (INTERNAL_pitchVariationAdd && currentLoop > 0)
			{
				finalPitch += prevPitch.Value;
			}
			else
			{
				finalPitch += soundPitch;
			}
			result.Pitch = finalPitch / 1200.0f;
			
			result.FilterType = INTERNAL_filterType;
			result.IsLooped = (
				(INTERNAL_loopCount == 255) &&
				!INTERNAL_trackVariationOnLoop &&
				!INTERNAL_volumeVariationOnLoop &&
				!INTERNAL_pitchVariationOnLoop
			);
			return result;
		}

		private void INTERNAL_getNextSound()
		{
			if (INTERNAL_trackVariationType == VariationPlaylistType.Ordered)
			{
				INTERNAL_curWave += 1;
				if (INTERNAL_curWave >= INTERNAL_waves.Length)
				{
					INTERNAL_curWave = 0;
				}
			}
			else if (INTERNAL_trackVariationType == VariationPlaylistType.OrderedFromRandom)
			{
				// FIXME: It seems like XACT organizes this for us?
				INTERNAL_curWave += 1;
				if (INTERNAL_curWave >= INTERNAL_waves.Length)
				{
					INTERNAL_curWave = 0;
				}
			}
			else if (INTERNAL_trackVariationType == VariationPlaylistType.Random)
			{
				double max = 0.0;
				for (int i = 0; i < INTERNAL_weights.Length; i += 1)
				{
					max += INTERNAL_weights[i];
				}
				double next = Random.NextDouble() * max;
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
			else if (	INTERNAL_trackVariationType == VariationPlaylistType.RandomNoImmediateRepeats ||
					INTERNAL_trackVariationType == VariationPlaylistType.Shuffle	)
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
				double next = Random.NextDouble() * max;
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
				throw new NotImplementedException(
					"Variation Playlist Type unhandled: " +
					INTERNAL_trackVariationType.ToString()
				);
			}
		}


		public static XACTEvent ParsePlayWaveEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			double clipVolume,
			byte filterType
		) {
			// Play Wave Header
			byte waveBank;
			byte loopCount;
			ushort position;
			ushort angle;
			ushort track;
			ParsePlayWaveBasicHeader(
				reader,
				out track,
				out waveBank,
				out loopCount,
				out position,
				out angle
			);

			return new PlayWaveEvent(
				eventTimestamp,
				randomOffset,
				new ushort[] { track },
				new byte[] { waveBank },
				0,
				0,
				clipVolume,
				clipVolume,
				filterType,
				loopCount,
				false,
				false,
				false,
				false,
				0,
				false,
				new byte[] { 0xFF }
			);
		}

		public static XACTEvent ParsePlayWaveWithTrackVariation(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			double clipVolume,
			byte filterType
		) {
			// Play Wave Header
			byte loopCount;
			ushort position;
			ushort angle;
			ParsePlayWaveComplexHeader(
				reader,
				out loopCount,
				out position,
				out angle
			);

			// Track Variation Block
			ushort numTracks;
			ushort variationValues;
			ushort variationType;
			bool variationOnLoop;
			ushort[] tracks;
			byte[] waveBanks;
			byte[] weights;
			ParseTrackVariation(
				reader,
				out variationType,
				out numTracks,
				out variationValues,
				out variationOnLoop,
				out tracks,
				out waveBanks,
				out weights
			);

			return new PlayWaveEvent(
				eventTimestamp,
				randomOffset,
				tracks,
				waveBanks,
				0,
				0,
				clipVolume,
				clipVolume,
				filterType,
				loopCount,
				false,
				false,
				false,
				false,
				variationType,
				variationOnLoop,
				weights
			);
		}

		public static XACTEvent ParsePlayWaveWithEffectVariation(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			double clipVolume,
			byte filterType
		) {
			// Play Wave Header
			byte waveBank;
			byte loopCount;
			ushort position;
			ushort angle;
			ushort track;
			ParsePlayWaveBasicHeader(
				reader,
				out track,
				out waveBank,
				out loopCount,
				out position,
				out angle
			);

			// Effect Variation Block
			short minPitch;
			short maxPitch;
			double minVolume;
			double maxVolume;
			ushort varFlags;
			bool pitchVarLoop;
			bool volumeVarLoop;
			bool pitchVarAdd;
			bool volumeVarAdd;
			ParseEffectVariation(
				reader,
				clipVolume,
				out minPitch,
				out maxPitch,
				out minVolume,
				out maxVolume,
				out varFlags,
				out pitchVarLoop,
				out volumeVarLoop,
				out pitchVarAdd,
				out volumeVarAdd
			);

			return new PlayWaveEvent(
				eventTimestamp,
				randomOffset,
				new ushort[] { track },
				new byte[] { waveBank },
				minPitch,
				maxPitch,
				minVolume,
				maxVolume,
				filterType,
				loopCount,
				pitchVarLoop,
				pitchVarAdd,
				volumeVarLoop,
				volumeVarAdd,
				0,
				false,
				new byte[] { 0xFF }
			);
		}

		public static XACTEvent ParsePlayWaveWithTrackAndEffectVariation(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			double clipVolume,
			byte filterType
		) {
			// Play Wave Header
			byte loopCount;
			ushort position;
			ushort angle;
			ParsePlayWaveComplexHeader(
				reader,
				out loopCount,
				out position,
				out angle
			);

			// Effect Variation Block
			short minPitch;
			short maxPitch;
			double minVolume;
			double maxVolume;
			ushort varFlags;
			bool pitchVarLoop;
			bool volumeVarLoop;
			bool pitchVarAdd;
			bool volumeVarAdd;
			ParseEffectVariation(
				reader,
				clipVolume,
				out minPitch,
				out maxPitch,
				out minVolume,
				out maxVolume,
				out varFlags,
				out pitchVarLoop,
				out volumeVarLoop,
				out pitchVarAdd,
				out volumeVarAdd
			);

			// Track Variation Block
			ushort numTracks;
			ushort variationValues;
			ushort variationType;
			bool variationOnLoop;
			ushort[] tracks;
			byte[] waveBanks;
			byte[] weights;
			ParseTrackVariation(
				reader,
				out variationType,
				out numTracks,
				out variationValues,
				out variationOnLoop,
				out tracks,
				out waveBanks,
				out weights
			);

			return new PlayWaveEvent(
				eventTimestamp,
				randomOffset,
				tracks,
				waveBanks,
				minPitch,
				maxPitch,
				minVolume,
				maxVolume,
				filterType,
				loopCount,
				pitchVarLoop,
				pitchVarAdd,
				volumeVarLoop,
				volumeVarAdd,
				variationType,
				variationOnLoop,
				weights
			);
		}


		private static void ParsePlayWaveBasicHeader(
			BinaryReader reader,
			out ushort track,
			out byte waveBank,
			out byte loopCount,
			out ushort position,
			out ushort angle
		) {
			/* Event Flags
			 * 0x01 = Break Loop
			 * 0x02 = Use Speaker Position
			 * 0x04 = Use Center Speaker
			 * 0x08 = New Speaker Position On Loop
			 */
			reader.ReadByte();

			// WaveBank Track Index
			track = reader.ReadUInt16();

			// WaveBank Index
			waveBank = reader.ReadByte();

			// Number of times to loop wave (255 is infinite)
			loopCount = reader.ReadByte();

			// Speaker position angle/arc, unused
			position = reader.ReadUInt16();
			angle = reader.ReadUInt16();
		}

		private static void ParsePlayWaveComplexHeader(
			BinaryReader reader,
			out byte loopCount,
			out ushort position,
			out ushort angle
		) {
			/* Event Flags
			 * 0x01 = Break Loop
			 * 0x02 = Use Speaker Position
			 * 0x04 = Use Center Speaker
			 * 0x08 = New Speaker Position On Loop
			 */
			reader.ReadByte();

			// Number of times to loop wave (255 is infinite)
			loopCount = reader.ReadByte();

			// Speaker position angle/arc, unused
			position = reader.ReadUInt16();
			angle = reader.ReadUInt16();
		}

		private static void ParseTrackVariation(
			BinaryReader reader,
			out ushort variationType,
			out ushort numTracks,
			out ushort variationValues,
			out bool variationOnLoop,
			out ushort[] tracks,
			out byte[] waveBanks,
			out byte[] weights
		) {
			// Number of WaveBank tracks
			numTracks = reader.ReadUInt16();

			/* Variation Playlist Type.
			 * First 4 bytes indicates Variation Type.
			 * Next 4 bytes appear to indicate New Variation On Loop.
			 * The rest is currently unknown.
			 * -flibit
			 */
			variationValues = reader.ReadUInt16();
			variationType = (ushort) (variationValues & 0x000F);
			variationOnLoop = (variationValues & 0x00F0) > 0;

			// Unknown values
			reader.ReadBytes(4);

			// Obtain WaveBank track information
			tracks = new ushort[numTracks];
			waveBanks = new byte[numTracks];
			weights = new byte[numTracks];
			for (ushort j = 0; j < numTracks; j += 1)
			{
				tracks[j] = reader.ReadUInt16();
				waveBanks[j] = reader.ReadByte();
				byte minWeight = reader.ReadByte();
				byte maxWeight = reader.ReadByte();
				weights[j] = (byte) (maxWeight - minWeight);
			}
		}

		private static void ParseEffectVariation(
			BinaryReader reader,
			double clipVolume,
			out short minPitch,
			out short maxPitch,
			out double minVolume,
			out double maxVolume,
			out ushort varFlags,
			out bool pitchVarLoop,
			out bool volumeVarLoop,
			out bool pitchVarAdd,
			out bool volumeVarAdd
		) {
			// Pitch Variation
			minPitch = reader.ReadInt16();
			maxPitch = reader.ReadInt16();

			// Volume Variation
			minVolume = XACTCalculator.ParseDecibel(reader.ReadByte());
			maxVolume = XACTCalculator.ParseDecibel(reader.ReadByte());

			// Frequency Variation, unsued
			reader.ReadSingle();
			reader.ReadSingle();

			// Q Factor Variation, unused
			reader.ReadSingle();
			reader.ReadSingle();

			// Variation On Loop flags
			varFlags = reader.ReadUInt16();
			if ((varFlags & 0x1000) == 0)
			{
				minPitch = 0;
				maxPitch = 0;
			}
			if ((varFlags & 0x2000) == 0)
			{
				minVolume = clipVolume;
				maxVolume = clipVolume;
			}
			// varFlags & 0xC000 is freq/qfactor, always together
			pitchVarLoop = (varFlags & 0x0100) > 0;
			volumeVarLoop = (varFlags & 0x0200) > 0;
			// varFlags & 0x0C00 is freq/qfactor loop, always together
			pitchVarAdd = (varFlags & 0x0004) > 0;
			volumeVarAdd = (varFlags & 0x0001) > 0;
			// varFlags & 0x0050 is freq/qfactor add, can be separate
		}
	}

	internal class SetValueEvent : XACTEvent
	{
		public readonly float Value;
		public readonly CueProperty Property;
		public readonly XACTClip.XactEventOp Operation;

		public SetValueEvent(
			uint timestamp,
			ushort randomOffset,
			float value,
			CueProperty property,
			XACTClip.XactEventOp operation,
			int loopCount = 0,
			float frequency = 0
		) : base(
			timestamp,
			randomOffset,
			loopCount,
			frequency
		) {
			Value = value;
			Property = property;
			Operation = operation;
		}

		public double GetVolume(double currentVolume)
		{
			switch (Operation)
			{
				case XACTClip.XactEventOp.Replace:
					return Value;
				case XACTClip.XactEventOp.Add:
					return currentVolume + Value;
				default:
					return currentVolume;
			}
		}

		public float GetPitch(float currentPitch)
		{
			switch (Operation)
			{
				case XACTClip.XactEventOp.Replace:
					return Value;
				case XACTClip.XactEventOp.Add:
					return currentPitch + Value;
				default:
					return currentPitch;
			}
		}

		public static XACTEvent ParseSetValueEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			CueProperty property,
			bool repeating,
			XACTClip.XactEventOp operation
		) {
			// Absolute or relative value to set to.
			float eventValue = reader.ReadSingle();

			// Unused/unknown trailing bytes.
			reader.ReadBytes(9);

			// Is this is a recurrence event?
			if (repeating)
			{
				int count;
				float frequency;
				XACTEvent.ReadRecurrenceData(reader, out count, out frequency);

				return new SetValueEvent(
					eventTimestamp,
					randomOffset,
					eventValue,
					property,
					operation,
					count,
					frequency
				);
			}
			else
			{
				return new SetValueEvent(
					eventTimestamp,
					randomOffset,
					eventValue,
					property,
					operation
				);
			}
		}
	}

	internal class SetRandomValueEvent : XACTEvent
	{
		public readonly float Min;
		public readonly float Max;
		public readonly CueProperty Property;
		public readonly XACTClip.XactEventOp Operation;

		public SetRandomValueEvent(
			uint timestamp,
			ushort randomOffset,
			float min,
			float max,
			CueProperty property,
			XACTClip.XactEventOp operation,
			int loopCount = 0,
			float frequency = 0
		) : base(
			timestamp,
			randomOffset,
			loopCount,
			frequency
		) {
			Min = min;
			Max = max;
			Property = property;
			Operation = operation;
		}

		public double GetVolume(double currentVolume)
		{
			double randomVolume = Min + (Random.NextDouble() * (Max - Min));
			switch (Operation)
			{
				case XACTClip.XactEventOp.Replace:
					return randomVolume;
				case XACTClip.XactEventOp.Add:
					return currentVolume + randomVolume;
				default:
					return currentVolume;
			}
		}

		public float GetPitch(float currentPitch)
		{
			float randomPitch = Min + (float) (Random.NextDouble() * (Max - Min));
			switch (Operation)
			{
				case XACTClip.XactEventOp.Replace:
					return randomPitch;
				case XACTClip.XactEventOp.Add:
					return currentPitch + randomPitch;
				default:
					return currentPitch;
			}
		}

		public static XACTEvent ParseSetRandomValueEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			CueProperty property,
			bool repeating,
			XACTClip.XactEventOp operation)
		{
			// Random min/max.
			float eventMin = reader.ReadSingle();
			float eventMax = reader.ReadSingle();

			// Unused/unknown trailing bytes.
			reader.ReadBytes(5);

			// Is this is a recurrence event?
			if (repeating)
			{
				int count;
				float frequency;
				ReadRecurrenceData(reader, out count, out frequency);

				return new SetRandomValueEvent(
					eventTimestamp,
					randomOffset,
					eventMin,
					eventMax,
					property,
					operation,
					count,
					frequency
				);
			}
			else
			{
				return new SetRandomValueEvent(
					eventTimestamp,
					randomOffset,
					eventMin,
					eventMax,
					property,
					operation
				);
			}
		}
	}

	internal class SetRampValueEvent : XACTEvent
	{
		public readonly float InitialValue;
		public readonly float InitialSlope;
		public readonly float SlopeDelta;
		public readonly float Duration;
		public readonly CueProperty Property;

		public SetRampValueEvent(
			uint timestamp,
			ushort randomOffset,
			float initialValue,
			float initialSlope,
			float slopeDelta,
			float duration,
			CueProperty property,
			int loopCount = 0,
			float frequency = 0
		) : base(
			timestamp,
			randomOffset,
			loopCount,
			frequency
		) {
			InitialValue = initialValue;
			InitialSlope = initialSlope;
			SlopeDelta = slopeDelta;
			Duration = duration;
			Property = property;
		}

		public static XACTEvent ParseSetRampValueEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			CueProperty property
		) {
			// Ramp type.
			float initialValue = reader.ReadSingle();

			// Slope appears to be encoded as (endValue - startValue) / duration;
			float initialSlope = reader.ReadSingle() * 100.0f;
			float slopeDelta = reader.ReadSingle() * 100.0f;

			// Duration of the ramp in seconds.
			float duration = reader.ReadUInt16() / 1000.0f;

			// Number of slices to break up the duration.
			// const float slices = 10;
			// float endValue = initialSlope * duration * slices + initialValue;

			return new SetRampValueEvent(
				eventTimestamp,
				randomOffset,
				initialValue,
				initialSlope,
				slopeDelta,
				duration,
				property
			);
		}
	}

	internal class MarkerEvent : XACTEvent
	{
		// private readonly int markerData;

		public MarkerEvent(
			uint timestamp,
			ushort randomOffset,
			int markerData,
			int loopCount = 0,
			float frequency = 0
		) : base(
			timestamp,
			randomOffset,
			loopCount,
			frequency
		) {
			// FIXME: this.markerData = markerData;
		}

		public static MarkerEvent ParseMarkerEvent(
			BinaryReader reader,
			uint eventTimestamp,
			ushort randomOffset,
			bool repeating
		) {
			// Data value for the marker (0-999)
			int markerData = reader.ReadInt32();

			// Is this is a recurrence marker event?
			if (repeating)
			{
				int count;
				float frequency;
				XACTEvent.ReadRecurrenceData(reader, out count, out frequency);

				return new MarkerEvent(
					eventTimestamp,
					randomOffset,
					markerData,
					count,
					frequency
				);
			}
			else
			{
				return new MarkerEvent(eventTimestamp, randomOffset, markerData);
			}
		}
	}
}
