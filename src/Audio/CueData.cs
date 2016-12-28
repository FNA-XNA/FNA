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
				curClip.LoadEvents(audioEngine, waveBankNames);
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

				// Random offset, unused
				reader.ReadUInt16();

				// Load the Event
				if (eventType == EventTypeCode.Stop)
				{
					// Unknown value
					reader.ReadByte();

					/* Event Flags
					 * bit0   - Play Release (0), Immediate (1)
					 * bit1   - Stop Track (0), Stop Cue (1)
					 * bit2-7 - Unused
					 */
					byte eventFlags = reader.ReadByte();
					AudioStopOptions options = ((eventFlags & 0x1) == 1) ?
						AudioStopOptions.Immediate :
						AudioStopOptions.AsAuthored;
					StopEventScope scope = (StopEventScope) (eventFlags & 0x02);

					Events[i] = new StopEvent(eventTimestamp, options, scope);
				}
				else if (eventType == EventTypeCode.PlayWave)
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
					ushort variationType = (ushort) (variationValues & 0x000F);
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

					// Variation On Loop flags
					ushort varFlags = reader.ReadUInt16();
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
					bool pitchVarLoop = (varFlags & 0x0100) > 0;
					bool volumeVarLoop = (varFlags & 0x0200) > 0;
					// varFlags & 0x0C00 is freq/qfactor loop, always together
					bool pitchVarAdd = (varFlags & 0x0004) > 0;
					bool volumeVarAdd = (varFlags & 0x0001) > 0;
					// varFlags & 0x0050 is freq/qfactor add, can be separate

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

					// Frequency Variation, unused
					reader.ReadSingle();
					reader.ReadSingle();

					// Q Factor Variation, unused
					reader.ReadSingle();
					reader.ReadSingle();

					// Variation On Loop flags
					ushort varFlags = reader.ReadUInt16();
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
					bool pitchVarLoop = (varFlags & 0x0100) > 0;
					bool volumeVarLoop = (varFlags & 0x0200) > 0;
					// varFlags & 0x0C00 is freq/qfactor loop, always together
					bool pitchVarAdd = (varFlags & 0x0004) > 0;
					bool volumeVarAdd = (varFlags & 0x0001) > 0;
					// varFlags & 0x0050 is freq/qfactor add, can be separate

					// Number of WaveBank tracks
					ushort numTracks = reader.ReadUInt16();

					/* Variation Playlist Type.
					 * First 4 bytes indicates Variation Type.
					 * Next 4 bytes appear to indicate New Variation On Loop.
					 * The rest is currently unknown.
					 * -flibit
					 */
					ushort variationValues = reader.ReadUInt16();
					ushort variationType = (ushort) (variationValues & 0x000F);
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
						pitchVarLoop,
						pitchVarAdd,
						volumeVarLoop,
						volumeVarAdd,
						variationType,
						variationOnLoop,
						weights
					);
				}
				else if (	eventType == EventTypeCode.Pitch ||
						eventType == EventTypeCode.PitchRepeating	)
				{
					// Unused byte (separator?)
					byte separator = reader.ReadByte();
					Debug.Assert(separator == 0xFF);

					// Read and convert the event setting type (Equation or Ramp).
					XactEventSettingType settingType = (XactEventSettingType) (reader.ReadByte() & 0x01);

					if (settingType == XactEventSettingType.Equation)
					{
						/* Event Flags
						 * bit0   - 0=Replace 1=Add
						 * bit1   - Unknown
						 * bit2-3 - 01=Value 10=Random
						*/
						byte eventFlags = reader.ReadByte();
						XactEventEquationType equationType = (XactEventEquationType) (eventFlags & (0x04 | 0x08));
						XactEventOp operation = (XactEventOp) (eventFlags & 0x01);

						if (equationType == XactEventEquationType.Value)
						{
							// Absolute or relative value to set the pitch to.
							float eventValue = reader.ReadSingle();

							// Unused/unknown trailing bytes.
							reader.ReadBytes(9);

							// Is this is a recurrence pitch event?
							if (eventType == EventTypeCode.PitchRepeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(
									reader,
									out count,
									out frequency
								);

								Events[i] = new SetEquationPitchEvent(
									eventTimestamp,
									eventValue,
									operation,
									count,
									frequency
								);
							}
							else
							{
								Events[i] = new SetEquationPitchEvent(
									eventTimestamp,
									eventValue,
									operation
								);
							}
						}
						else if (equationType == XactEventEquationType.Random)
						{
							// Random pitch Min/Max.
							float eventMin = reader.ReadSingle();
							float eventMax = reader.ReadSingle();

							// Unused/unknown trailing bytes.
							reader.ReadBytes(5);

							// Is this is a recurrence pitch event?
							if (eventType == EventTypeCode.PitchRepeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(
									reader,
									out count,
									out frequency
								);

								Events[i] = new SetRandomPitchEvent(
									eventTimestamp,
									eventMin,
									eventMax,
									operation,
									count,
									frequency
								);
							}
							else
							{
								Events[i] = new SetRandomPitchEvent(
									eventTimestamp,
									eventMin,
									eventMax,
									operation
								);
							}
						}
						else
						{
							throw new NotImplementedException(
								"Unexpected equation type."
							);
						}
					}
					else if (settingType == XactEventSettingType.Ramp)
					{
						/*float initialValue =*/ reader.ReadSingle(); // / 100.0f;

						/* Slope appears to be encoded as
						 * (endValue - startValue) / duration;
						 */
						/*float initialSlope =*/ reader.ReadSingle();
						/*float slopeDelta =*/ reader.ReadSingle();

						// Duration of the ramp in seconds.
						/*float duration =*/ reader.ReadUInt16(); // / 1000.0f;

						// Number of slices to break up the duration.
						// const float slices = 10;
						// float endValue = initialSlope * duration * slices + initialValue;

						/* FIXME: Create a Ramp Event type that can operate over
						 * the period from timestamp to timestamp + duration.
						 *
						 * Events[i] = new SetRampPitchEvent(
						 *	eventTimestamp,
						 *	initialValue,
						 *	initialSlope,
						 *	slopeDelta,
						 *	duration
						 * );
						 */
						Events[i] = new NullEvent(eventTimestamp);
					}
				}
				else if (	eventType == EventTypeCode.Volume ||
						eventType == EventTypeCode.VolumeRepeating	)
				{
					// Unused byte (separator?)
					byte separator = reader.ReadByte();
					Debug.Assert(separator == 0xFF);

					// Read and convert the event setting type (Equation or Ramp).
					XactEventSettingType settingType = (XactEventSettingType) (reader.ReadByte() & 0x01);

					if (settingType == XactEventSettingType.Equation)
					{
						/* Event Flags
						 * bit0   - 0=Replace 1=Add
						 * bit1   - Unknown
						 * bit2-3 - 01=Value 10=Random
						*/
						byte eventFlags = reader.ReadByte();
						XactEventEquationType equationType = (XactEventEquationType) (eventFlags & (0x04 | 0x08));
						XactEventOp operation = (XactEventOp) (eventFlags & 0x01);

						if (equationType == XactEventEquationType.Value)
						{
							// Absolute or relative value to set to.
							float eventValue = reader.ReadSingle();

							// Unused/unknown trailing bytes.
							reader.ReadBytes(9);

							// Is this is a recurrence event?
							if (eventType == EventTypeCode.VolumeRepeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(
									reader,
									out count,
									out frequency
								);

								Events[i] = new SetEquationVolumeEvent(
									eventTimestamp,
									eventValue,
									operation,
									count,
									frequency
								);
							}
							else
							{
								Events[i] = new SetEquationVolumeEvent(
									eventTimestamp,
									eventValue,
									operation
								);
							}
						}
						else if (equationType == XactEventEquationType.Random)
						{
							// Random min/max.
							float eventMin = reader.ReadSingle();
							float eventMax = reader.ReadSingle();

							// Unused/unknown trailing bytes.
							reader.ReadBytes(5);

							// Is this is a recurrence event?
							if (eventType == EventTypeCode.VolumeRepeating)
							{
								int count;
								float frequency;
								ReadRecurrenceData(
									reader,
									out count,
									out frequency
								);

								Events[i] = new SetRandomVolumeEvent(
									eventTimestamp,
									eventMin,
									eventMax,
									operation,
									count,
									frequency
								);
							}
							else
							{
								Events[i] = new SetRandomVolumeEvent(
									eventTimestamp,
									eventMin,
									eventMax,
									operation
								);
							}
						}
						else
						{
							throw new NotImplementedException(
								"Unexpected equation type."
							);
						}
					}
					else if (settingType == XactEventSettingType.Ramp)
					{
						/*float initialValue =*/ reader.ReadSingle(); // / 100.0f;

						// Slope appears to be encoded as (endValue - startValue) / duration;
						/*float initialSlope =*/ reader.ReadSingle();
						/*float slopeDelta =*/ reader.ReadSingle();

						// Duration of the ramp in seconds.
						/*float duration =*/ reader.ReadUInt16(); // / 1000.0f;

						// Number of slices to break up the duration.
						// const float slices = 10;
						// float endValue = initialSlope * duration * slices + initialValue;

						/* FIXME: Create a Ramp Event type that can operate over
						 * the period from timestamp to timestamp + duration.
						 *
						 * Events[i] = new SetRampVolumeEvent(
						 *	eventTimestamp,
						 *	initialValue,
						 *	initialSlope,
						 *	slopeDelta,
						 *	duration
						 * );
						 */
						Events[i] = new NullEvent(eventTimestamp);
						break;
					}
				}
				else if (	eventType == EventTypeCode.Marker ||
						eventType == EventTypeCode.MarkerRepeating	)
				{
					// Unused byte (separator?)
					byte separator = reader.ReadByte();
					Debug.Assert(separator == 0xFF);

					// Data value for the marker (0-999)
					int markerData = reader.ReadInt32();

					// Is this is a recurrence marker event?
					if (eventType == EventTypeCode.MarkerRepeating)
					{
						int count;
						float frequency;
						ReadRecurrenceData(reader, out count, out frequency);

						Events[i] = new MarkerEvent(
							eventTimestamp,
							markerData,
							count,
							frequency
						);
					}
					else
					{
						Events[i] = new MarkerEvent(eventTimestamp, markerData);
					}
				}
				else
				{
					// TODO: All XACT Events?
					throw new NotImplementedException(
						"EVENT TYPE " + eventType.ToString() + " NOT IMPLEMENTED!"
					);
				}
			}
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

		public int Count { get; private set; }

		public float Frequency { get; private set; }

		protected static readonly Random random = new Random();

		public XACTEvent(uint timestamp)
			: this(timestamp, 0, 0)
		{
		}

		protected XACTEvent(uint timestamp, int count, float frequency)			
		{
			Timestamp = timestamp;
			Count = count;
			Frequency = frequency;
		}

		public abstract void Apply(Cue cue, XACTClip track);
	}

	internal class StopEvent : XACTEvent
	{
		public readonly AudioStopOptions StopOptions;
		public readonly XACTClip.StopEventScope Scope;

		public StopEvent(
			uint timestamp,
			AudioStopOptions stopOptions,
			XACTClip.StopEventScope scope
		) : base(timestamp) {
			StopOptions = stopOptions;
			Scope = scope;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			AudioStopOptions stopOptions = StopOptions;

			switch (Scope)
			{
				case XACTClip.StopEventScope.Cue:
					cue.Stop(stopOptions);
					break;
				case XACTClip.StopEventScope.Track:
					// FIXME: Need to stop this and ONLY this track
					// track.Stop(stopOptions);
					// break;
					throw new NotImplementedException("Stop events targeting the track are not supported!");
			}
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
		) : base(timestamp) {
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
				random.NextDouble() *
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

			finalPitch = (short) random.Next(
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
				throw new NotImplementedException(
					"Variation Playlist Type unhandled: " +
					INTERNAL_trackVariationType.ToString()
				);
			}
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			cue.PlayWave(this);
		}
	}

	internal class SetEquationVolumeEvent : XACTEvent
	{
		private readonly float value;
		private readonly XACTClip.XactEventOp operation;

		public SetEquationVolumeEvent(
			uint timestamp,
			float value,
			XACTClip.XactEventOp operation,
			int count = 0,
			float frequency = 0
		) : base(
			timestamp,
			count,
			frequency
		) {
			this.value = value;
			this.operation = operation;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			cue.eventVolume = GetVolume(cue.eventVolume);
		}

		private double GetVolume(double currentVolume)
		{
			switch (operation)
			{
				case XACTClip.XactEventOp.Replace:
					return value;
				case XACTClip.XactEventOp.Add:
					return currentVolume + value;
				default:
					return currentVolume;
			}
		}
	}

	internal class SetRandomVolumeEvent : XACTEvent
	{
		private readonly float min;
		private readonly float max;
		private readonly XACTClip.XactEventOp operation;

		public SetRandomVolumeEvent(
			uint timestamp,
			float min,
			float max,
			XACTClip.XactEventOp operation,
			int count = 0,
			float frequency = 0
		) : base(
			timestamp,
			count,
			frequency
		) {
			this.min = min;
			this.max = max;
			this.operation = operation;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			cue.eventVolume = GetVolume(cue.eventVolume);
		}

		private double GetVolume(double currentVolume)
		{
			double randomVolume = min + (random.NextDouble() * (max - min));
			switch (operation)
			{
				case XACTClip.XactEventOp.Replace:
					return randomVolume;
				case XACTClip.XactEventOp.Add:
					return currentVolume + randomVolume;
				default:
					return currentVolume;
			}
		}
	}

	internal class SetEquationPitchEvent : XACTEvent
	{
		private readonly float value;
		private readonly XACTClip.XactEventOp operation;

		public SetEquationPitchEvent(
			uint timestamp,
			float value,
			XACTClip.XactEventOp operation,
			int count = 0,
			float frequency = 0
		) : base(
			timestamp,
			count,
			frequency
		) {
			this.value = value;
			this.operation = operation;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			cue.eventPitch = GetPitch(cue.eventPitch);
		}

		private float GetPitch(float currentPitch)
		{
			switch (operation)
			{
				case XACTClip.XactEventOp.Replace:
					return value;
				case XACTClip.XactEventOp.Add:
					return currentPitch + value;
				default:
					return currentPitch;
			}
		}
	}

	internal class SetRandomPitchEvent : XACTEvent
	{
		private readonly float min;
		private readonly float max;
		private readonly XACTClip.XactEventOp operation;

		public SetRandomPitchEvent(
			uint timestamp,
			float min,
			float max,
			XACTClip.XactEventOp operation,
			int count = 0,
			float frequency = 0
		) : base(
			timestamp,
			count,
			frequency
		) {
			this.min = min;
			this.max = max;
			this.operation = operation;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			cue.eventPitch = GetPitch(cue.eventPitch);
		}

		private float GetPitch(float currentPitch)
		{
			float randomPitch = min + (float) (random.NextDouble() * (max - min));
			switch (operation)
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

	internal class MarkerEvent : XACTEvent
	{
		//private readonly int markerData;

		public MarkerEvent(
			uint timestamp,
			int markerData,
			int count = 0,
			float frequency = 0
		) : base(
			timestamp,
			count,
			frequency
		) {
			// FIXME: this.markerData = markerData;
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			// FIXME: Implement action for a marker event. Some kind of callback?
		}
	}

	internal class NullEvent : XACTEvent
	{
		public NullEvent(
			uint timestamp
		) : base(timestamp)
		{
		}

		public override void Apply(Cue cue, XACTClip track)
		{
			// Do nothing.
		}
	}
}
