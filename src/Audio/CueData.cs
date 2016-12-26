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

		public ushort ReleaseMS
		{
			get;
			internal set;
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
			ReleaseMS = 0;
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
			ReleaseMS = 0;
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
			Pitch = reader.ReadInt16() / 100.0f;

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

			HasLoadedTracks = false;
		}

		public void LoadTracks(AudioEngine audioEngine, List<string> waveBankNames)
		{
			foreach (XACTClip curClip in INTERNAL_clips)
			{
				curClip.LoadEvents(audioEngine, waveBankNames);
			}
			HasLoadedTracks = true;
		}
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
				//uint eventUnknown = eventInfo >> 21;

				ushort randomOffset = reader.ReadUInt16();

				// Unused byte (separator?)
				byte separator = reader.ReadByte();
				Debug.Assert(separator == 0xFF);

				// Load the Event
				if (eventType == EventTypeCode.Stop)
				{
					Events[i] = new StopEvent(eventTimestamp, randomOffset, reader);
				}
				else if (eventType == EventTypeCode.PlayWave)
				{
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
						out angle);

					// Finally.
					Events[i] = new PlayWaveEvent(
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
				else if (eventType == EventTypeCode.PlayWaveWithTrackVariation)
				{
					byte loopCount;
					ushort position;
					ushort angle;
					ushort numTracks;
					ushort variationValues;
					ushort variationType;
					bool variationOnLoop;
					ushort[] tracks;
					byte[] waveBanks;
					byte[] weights;
					ParsePlayWaveTracks(
						out loopCount,
						reader,
						out position,
						out angle,
						out numTracks,
						out variationValues,
						out variationType,
						out variationOnLoop,
						out tracks,
						out waveBanks,
						out weights);

					// Finally.
					Events[i] = new PlayWaveEvent(
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
				else if (eventType == EventTypeCode.PlayWaveWithEffectVariation)
				{
					// Play Wave Basic
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
						out angle);

					// Effects variation block.
					float minPitch;
					float maxPitch;
					double minVolume;
					double maxVolume;
					ushort varFlags;
					bool pitchVarLoop;
					bool volumeVarLoop;
					bool pitchVarAdd;
					bool volumeVarAdd;
					ParseEffectVariation(
						out minPitch,
						reader,
						clipVolume,
						out maxPitch,
						out minVolume,
						out maxVolume,
						out varFlags,
						out pitchVarLoop,
						out volumeVarLoop,
						out pitchVarAdd,
						out volumeVarAdd);

					// Finally.
					Events[i] = new PlayWaveEvent(
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
				else if (eventType == EventTypeCode.PlayWaveWithTrackAndEffectVariation)
				{
					// Play Wave Header
					byte loopCount;
					ushort position;
					ushort angle;
					ParsePlayWaveComplexHeader(out loopCount, reader, out position, out angle);

					// Effects variation block.
					float minPitch;
					float maxPitch;
					double minVolume;
					double maxVolume;
					ushort varFlags;
					bool pitchVarLoop;
					bool volumeVarLoop;
					bool pitchVarAdd;
					bool volumeVarAdd;
					ParseEffectVariation(
						out minPitch,
						reader,
						clipVolume,
						out maxPitch,
						out minVolume,
						out maxVolume,
						out varFlags,
						out pitchVarLoop,
						out volumeVarLoop,
						out pitchVarAdd,
						out volumeVarAdd);

					// Track variation block.
					ushort numTracks;
					ushort variationValues;
					ushort variationType;
					bool variationOnLoop;
					ushort[] tracks;
					byte[] waveBanks;
					byte[] weights;
					ParseTrackVariation(
						out variationType,
						reader,
						out numTracks,
						out variationValues,
						out variationOnLoop,
						out tracks,
						out waveBanks,
						out weights);

					// Finally.
					Events[i] = new PlayWaveEvent(
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
				else if (eventType == EventTypeCode.Pitch)
				{
					ParseVolumeOrPitchEvent(
						reader,
						i,
						eventTimestamp,
						randomOffset,
						false,
						CreateRandomPitchEvent,
						CreateEquationPitchEvent,
						CreateRampPitchEvent);
				}
				else if (eventType == EventTypeCode.PitchRepeating)
				{
					ParseVolumeOrPitchEvent(
						reader,
						i,
						eventTimestamp,
						randomOffset,
						true,
						CreateRandomPitchEvent,
						CreateEquationPitchEvent,
						CreateRampPitchEvent);
				}
				else if (eventType == EventTypeCode.Volume)
				{
					ParseVolumeOrPitchEvent(
						reader,
						i,
						eventTimestamp,
						randomOffset,
						false,
						CreateRandomVolumeEvent,
						CreateEquationVolumeEvent,
						CreateRampVolumeEvent);
				}
				else if (eventType == EventTypeCode.VolumeRepeating)
				{
					ParseVolumeOrPitchEvent(
						reader,
						i,
						eventTimestamp,
						randomOffset,
						true,
						CreateRandomVolumeEvent,
						CreateEquationVolumeEvent,
						CreateRampVolumeEvent);
				}
				else if (eventType == EventTypeCode.Marker)
				{
					ParseMarkerEvent(reader, i, eventTimestamp, randomOffset, false);
				}
				else if (eventType == EventTypeCode.MarkerRepeating)
				{
					ParseMarkerEvent(reader, i, eventTimestamp, randomOffset, true);
				}
				else
				{
					throw new NotImplementedException(
						"EVENT TYPE " + eventType.ToString() + " NOT IMPLEMENTED!"
					);
				}
			}
		}

		delegate XACTEvent CreateSetEquationEvent(
			uint timestamp,
			ushort randomOffset,
			float value,
			XactEventOp operation,
			int count = 0,
			float frequency = 0);

		XACTEvent CreateEquationVolumeEvent(
			uint timestamp,
			ushort randomOffset,
			float value,
			XactEventOp operation,
			int count = 0,
			float frequency = 0)
		{
			return new SetValueEvent(
				timestamp,
				randomOffset,
				value,
				CueProperty.Volume,
				operation,
				count,
				frequency);
		}

		XACTEvent CreateEquationPitchEvent(
			uint timestamp,
			ushort randomOffset,
			float value,
			XactEventOp operation,
			int count = 0,
			float frequency = 0)
		{
			return new SetValueEvent(
				timestamp,
				randomOffset,
				value,
				CueProperty.Pitch,
				operation,
				count,
				frequency);
		}

		delegate XACTEvent CreateRandomEvent(
			uint timestamp,
			ushort randomOffset,
			float min,
			float max,
			XactEventOp operation,
			int count = 0,
			float frequency = 0);

		XACTEvent CreateRandomVolumeEvent(
			uint timestamp,
			ushort randomOffset,
			float min,
			float max,
			XactEventOp operation,
			int count = 0,
			float frequency = 0)
		{
			return new SetRandomValueEvent(
				timestamp,
				randomOffset,
				min,
				max,
				CueProperty.Volume,
				operation,
				count,
				frequency);
		}

		XACTEvent CreateRandomPitchEvent(
			uint timestamp,
			ushort randomOffset,
			float min,
			float max,
			XactEventOp operation,
			int count = 0,
			float frequency = 0)
		{
			return new SetRandomValueEvent(
				timestamp,
				randomOffset,
				min,
				max,
				CueProperty.Pitch,
				operation,
				count,
				frequency);
		}

		delegate XACTEvent CreateRampEvent(
			uint timestamp,
			ushort randomOffset,
			float initialValue,
			float initialSlope,
			float slopeDelta,
			float duration);

		XACTEvent CreateRampVolumeEvent(
			uint timestamp,
			ushort randomOffset,
			float initialValue,
			float initialSlope,
			float slopeDelta,
			float duration)
		{
			return new SetRampValueEvent(
				timestamp,
				randomOffset,
				initialValue,
				initialSlope,
				slopeDelta,
				duration,
				CueProperty.Volume);
		}

		XACTEvent CreateRampPitchEvent(
			uint timestamp,
			ushort randomOffset,
			float initialValue,
			float initialSlope,
			float slopeDelta,
			float duration)
		{
			return new SetRampValueEvent(
				timestamp,
				randomOffset,
				initialValue,
				initialSlope,
				slopeDelta,
				duration,
				CueProperty.Pitch);
		}

		private void ParseVolumeOrPitchEvent(
			BinaryReader reader,
			int i,
			uint eventTimestamp,
			ushort randomOffset,
			bool repeating,
			CreateRandomEvent createRandomEvent,
			CreateSetEquationEvent createEquationEvent,
			CreateRampEvent createRampEvent)
		{
			// Read and convert the event setting type (Equation or Ramp).
			XactEventSettingType settingType =
				(XactEventSettingType) (reader.ReadByte() & 0x01);

			switch (settingType)
			{
				case XactEventSettingType.Equation:
					/* Event Flags
							 * bit0   - 0=Replace 1=Add
							 * bit1   - Unknown
							 * bit2-3 - 01=Value 10=Random
							*/
					byte eventFlags = reader.ReadByte();
					XactEventEquationType equationType =
						(XactEventEquationType) (eventFlags & (0x04 | 0x08));
					XactEventOp operation = (XactEventOp) (eventFlags & 0x01);

					switch (equationType)
					{
						case XactEventEquationType.Value:
							// Absolute or relative value to set to.
							float eventValue = reader.ReadSingle() / 100.0f;

							// Unused/unknown trailing bytes.
							reader.ReadBytes(9);

							// Is this is a recurrence event?
							if (repeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(reader, out count, out frequency);

								Events[i] = createEquationEvent(
									eventTimestamp,
									randomOffset,
									eventValue,
									operation,
									count,
									frequency);
							}
							else
							{
								Events[i] = createEquationEvent(
									eventTimestamp,
									randomOffset,
									eventValue,
									operation);
							}
							break;
						case XactEventEquationType.Random:
							// Random min/max.
							float eventMin = reader.ReadSingle() / 100.0f;
							float eventMax = reader.ReadSingle() / 100.0f;

							// Unused/unknown trailing bytes.
							reader.ReadBytes(5);

							// Is this is a recurrence event?
							if (repeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(reader, out count, out frequency);

								Events[i] = createRandomEvent(
									eventTimestamp,
									randomOffset,
									eventMin,
									eventMax,
									operation,
									count,
									frequency);
							}
							else
							{
								Events[i] = createRandomEvent(
									eventTimestamp,
									randomOffset,
									eventMin,
									eventMax,
									operation);
							}
							break;
						default:
							throw new NotImplementedException(
								"Encountered event unexpected equation type.");
					}
					break;
				case XactEventSettingType.Ramp:
					// Ramp type.

					float initialValue = reader.ReadSingle() / 100.0f;

					// Slope appears to be encoded as (endValue - startValue) / duration;
					float initialSlope = reader.ReadSingle();
					float slopeDelta = reader.ReadSingle();

					// Duration of the ramp in seconds.

					float duration = reader.ReadUInt16() / 1000.0f;

					// Number of slices to break up the duration.
					const float slices = 10;
					float endValue = initialSlope * duration * slices + initialValue;

					Events[i] = createRampEvent(
						eventTimestamp,
						randomOffset,
						initialValue,
						initialSlope,
						slopeDelta,
						duration);
					break;
			}
		}

		private void ParseMarkerEvent(
			BinaryReader reader,
			int i,
			uint eventTimestamp,
			ushort randomOffset,
			bool repeating)
		{
			// Data value for the marker (0-999)
			int markerData = reader.ReadInt32();

			// Is this is a recurrence marker event?
			if (repeating)
			{
				int count;
				float frequency;
				ReadRecurrenceData(reader, out count, out frequency);

				Events[i] = new MarkerEvent(
					eventTimestamp,
					randomOffset,
					markerData,
					count,
					frequency);
			}
			else
			{
				Events[i] = new MarkerEvent(eventTimestamp, randomOffset, markerData);
			}
		}

		private static void ParseEffectVariation(
			out float minPitch,
			BinaryReader reader,
			double clipVolume,
			out float maxPitch,
			out double minVolume,
			out double maxVolume,
			out ushort varFlags,
			out bool pitchVarLoop,
			out bool volumeVarLoop,
			out bool pitchVarAdd,
			out bool volumeVarAdd)
		{
			// Pitch Variation
			minPitch = reader.ReadInt16() / 100.0f;
			maxPitch = reader.ReadInt16() / 100.0f;

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

		private static void ParseTrackVariation(
			out ushort variationType,
			BinaryReader reader,
			out ushort numTracks,
			out ushort variationValues,
			out bool variationOnLoop,
			out ushort[] tracks,
			out byte[] waveBanks,
			out byte[] weights)
		{
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

		private static void ParsePlayWaveTracks(
			out byte loopCount,
			BinaryReader reader,
			out ushort position,
			out ushort angle,
			out ushort numTracks,
			out ushort variationValues,
			out ushort variationType,
			out bool variationOnLoop,
			out ushort[] tracks,
			out byte[] waveBanks,
			out byte[] weights)
		{
			ParsePlayWaveComplexHeader(out loopCount, reader, out position, out angle);

			ParseTrackVariation(
				out variationType,
				reader,
				out numTracks,
				out variationValues,
				out variationOnLoop,
				out tracks,
				out waveBanks,
				out weights);
		}

		private static void ParsePlayWaveComplexHeader(
			out byte loopCount,
			BinaryReader reader,
			out ushort position,
			out ushort angle)
		{
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

		private static void ParsePlayWaveBasicHeader(
			BinaryReader reader,
			out ushort track,
			out byte waveBank,
			out byte loopCount,
			out ushort position,
			out ushort angle)
		{
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

		private static void ReadRecurrenceData(
			BinaryReader reader,
			out int count,
			out float frequency
		) {
			count = reader.ReadUInt16();
			frequency = reader.ReadUInt16() / 1000.0f;
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
	}

	internal abstract class XACTEvent
	{
		public uint Timestamp
		{
			get;
			private set;
		}

		// FIXME: This needs to be used when processing events. Event instances
		// should take the time stamp, apply a random offset bounded by this, 
		// and use it as the instance timestamp.
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
			float frequency)
		{
			Timestamp = timestamp;
			RandomOffset = randomOffset;
			LoopCount = loopCount;
			Frequency = frequency;
		}
	}

	internal class StopEvent : XACTEvent
	{
		public readonly AudioStopOptions StopOptions;
		public readonly XACTClip.StopEventScope Scope;

		public StopEvent(
			uint timestamp,
			ushort randomOffset,
			BinaryReader reader
		) : base(timestamp, randomOffset) {
			/* Event Flags
			 * bit0   - Play Release (0), Immediate (1)
			 * bit1   - Stop Track (0), Stop Cue (1)
			 * bit2-7 - Unused
			 */
			byte eventFlags = reader.ReadByte();
			StopOptions = ((eventFlags & 0x1) == 1) ?
				AudioStopOptions.Immediate :
				AudioStopOptions.AsAuthored;
			Scope = (XACTClip.StopEventScope) (eventFlags & 0x02);
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

		private float INTERNAL_minPitch;
		private float INTERNAL_maxPitch;

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
			float minPitch,
			float maxPitch,
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
			byte[] weights)
			: base(timestamp, randomOffset)
		{
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
				INTERNAL_waves[i] =
					audioEngine.INTERNAL_getWaveBankTrack(
						waveBankNames[INTERNAL_waveBanks[i]],
						INTERNAL_tracks[i]);
			}
		}

		public SoundEffectInstance GenerateInstance(
			double soundVolume,
			float soundPitch,
			int currentLoop,
			double? prevVolume,
			float? prevPitch,
			out double finalVolume,
			out float finalPitch
		) {
			if (currentLoop > INTERNAL_loopCount
					&& INTERNAL_loopCount != 255)
			{
				// We've finished all the loops!
				finalVolume = 0.0;
				finalPitch = 0;
				return null;
			}
			INTERNAL_getNextSound();
			SoundEffectInstance result =
				INTERNAL_waves[INTERNAL_curWave].CreateInstance();
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

			finalPitch = (float)(Random.NextDouble() 
				* (INTERNAL_maxPitch - INTERNAL_minPitch
			) + INTERNAL_minPitch);
			if (INTERNAL_pitchVariationAdd && currentLoop > 0)
			{
				finalPitch += prevPitch.Value;
			}
			else
			{
				finalPitch += soundPitch;
			}
			result.Pitch = finalPitch / 12.0f;

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
	}

	internal class SetValueEvent : XACTEvent
	{
		public readonly float Value;
		public readonly XACTClip.XactEventOp Operation;
		public readonly CueProperty Property;

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
		)
		{
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
	}

	internal enum CueProperty
	{
		Volume,
		Pitch
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
		)
		{
			InitialValue = initialValue;
			InitialSlope = initialSlope;
			SlopeDelta = slopeDelta;
			Duration = duration;
			Property = property;
		}
	}

	internal class MarkerEvent : XACTEvent
	{
		//private readonly int markerData;

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
	}
}
