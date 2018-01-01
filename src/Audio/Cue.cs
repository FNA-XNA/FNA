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
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.cue.aspx
	public sealed class Cue : IDisposable
	{
		#region Public Properties

		// FIXME: What is this...? -flibit
		public bool IsCreated
		{
			get;
			private set;
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsPaused
		{
			get
			{
				return (	!INTERNAL_timer.IsRunning &&
						INTERNAL_timer.ElapsedTicks > 0	);
			}
		}

		public bool IsPlaying
		{
			get
			{
				return (	INTERNAL_timer.IsRunning ||
						INTERNAL_timer.ElapsedTicks > 0	) && !IsStopping;
			}
		}

		// FIXME: Is this just to load WaveBank tracks? -flibit
		public bool IsPrepared
		{
			get;
			private set;
		}

		// FIXME: Is this just to load WaveBank tracks? -flibit
		public bool IsPreparing
		{
			get;
			private set;
		}

		public bool IsStopped
		{
			get;
			private set;
		}

		public bool IsStopping
		{
			get
			{
				return (	INTERNAL_fadeMode == FadeMode.FadeOut ||
						INTERNAL_fadeMode == FadeMode.ReleaseRpc	);
			}
		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		private ulong elapsedFrames;
		internal bool JustStarted
		{
			get
			{
				return elapsedFrames < 2;
			}
		}

		#endregion

		#region Private Variables

		private AudioEngine INTERNAL_baseEngine;

		// Cue information parsed from the SoundBank
		private List<string> INTERNAL_waveBankNames;
		private CueData INTERNAL_data;

		// Current sound and its events
		private XACTSoundInstance INTERNAL_activeSound;

		private Dictionary<SoundEffectInstance, PlayWaveEventInstance>
			INTERNAL_playWaveEventBySound;

		// Used for event timestamps
		private Stopwatch INTERNAL_timer;

		// Sound list
		private List<SoundEffectInstance> INTERNAL_instancePool;
		private List<double> INTERNAL_instanceVolumes;
		private List<short> INTERNAL_instancePitches;

		// RPC data list
		private List<float> INTERNAL_rpcTrackVolumes;
		private List<float> INTERNAL_rpcTrackPitches;
		private ushort INTERNAL_maxRpcReleaseTime;

		// Events can control volume/pitch as well!
		internal double eventVolume;
		internal float eventPitch;

		// User-controlled sounds require a bit more trickery.
		private bool INTERNAL_userControlledPlaying;
		private float INTERNAL_controlledValue;

		// 3D audio variables
		private bool INTERNAL_isPositional;
		private AudioListener INTERNAL_listener;
		private AudioEmitter INTERNAL_emitter;

		// XACT instance variables
		private List<Variable> INTERNAL_variables;

		// Category managing this Cue, and whether or not it's user-managed
		private AudioCategory INTERNAL_category;
		private bool INTERNAL_isManaged;

		// Fading
		private enum FadeMode
		{
			None,
			FadeOut,
			FadeIn,
			ReleaseRpc
		}
		private long INTERNAL_fadeStart;
		private long INTERNAL_fadeEnd;
		private FadeMode INTERNAL_fadeMode = FadeMode.None;

		#endregion

		#region Private Static Random Number Generator

		private static Random random = new Random();

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Internal Constructor

		internal Cue(
			AudioEngine audioEngine,
			List<string> waveBankNames,
			string name,
			CueData data,
			bool managed
		) {
			INTERNAL_baseEngine = audioEngine;
			INTERNAL_waveBankNames = waveBankNames;

			Name = name;

			INTERNAL_data = data;
			IsPrepared = false;
			IsPreparing = true;

			INTERNAL_maxRpcReleaseTime = 0;

			foreach (XACTSound curSound in data.Sounds)
			{
				/* Determine the release times per track, if any, to be used to extend
				 * the sound when playing the release.
				 */
				{
					ushort maxReleaseMS = 0;

					// Loop over tracks.
					for (int i = 0; i < curSound.RPCCodes.Count; i += 1)
					{
						// Loop over curves.
						foreach (uint curCode in curSound.RPCCodes[i])
						{
							RPC curRPC = INTERNAL_baseEngine.INTERNAL_getRPC(curCode);
							if (!INTERNAL_baseEngine.INTERNAL_isGlobalVariable(curRPC.Variable))
							{
								// Only release times applied to volume are considered.
								if (curRPC.Variable.Equals("ReleaseTime") && curRPC.Parameter == RPCParameter.Volume)
								{
									maxReleaseMS = Math.Max((ushort)curRPC.LastPoint.X, maxReleaseMS);
								}
							}
						}
					}

					// Keep track of the maximum release time to extend the sound.
					INTERNAL_maxRpcReleaseTime = maxReleaseMS;
				}
			}

			IsPrepared = true;
			IsPreparing = false;

			IsStopped = false;

			INTERNAL_isManaged = managed;

			INTERNAL_category = INTERNAL_baseEngine.INTERNAL_initCue(
				this,
				data.Category
			);

			eventVolume = 0.0;
			eventPitch = 0.0f;

			INTERNAL_userControlledPlaying = false;
			INTERNAL_isPositional = false;

			INTERNAL_playWaveEventBySound =
				new Dictionary<SoundEffectInstance, PlayWaveEventInstance>();

			INTERNAL_timer = new Stopwatch();

			INTERNAL_instancePool = new List<SoundEffectInstance>();
			INTERNAL_instanceVolumes = new List<double>();
			INTERNAL_instancePitches = new List<short>();

			INTERNAL_rpcTrackVolumes = new List<float>();
			INTERNAL_rpcTrackPitches = new List<float>();
		}

		#endregion

		#region Destructor

		~Cue()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			if (!IsDisposed)
			{
				if (Disposing != null)
				{
					Disposing.Invoke(this, null);
				}
				if (INTERNAL_instancePool != null)
				{
					foreach (SoundEffectInstance sfi in INTERNAL_instancePool)
					{
						sfi.Dispose();
					}
					INTERNAL_instancePool.Clear();
					INTERNAL_instanceVolumes.Clear();
					INTERNAL_instancePitches.Clear();
					INTERNAL_rpcTrackVolumes.Clear();
					INTERNAL_rpcTrackPitches.Clear();
				}

				KillCue();

				IsDisposed = true;

				// IXACTCue* no longer exists, these should all be false
				IsStopped = false;
				IsCreated = false;
				IsPrepared = false;
			}
		}

		#endregion

		#region Public Methods

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			if ((IsPlaying || IsStopping) && !INTERNAL_isPositional)
			{
				throw new InvalidOperationException("Apply3D call after Play!");
			}
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			if (emitter == null)
			{
				throw new ArgumentNullException("emitter");
			}
			INTERNAL_listener = listener;
			INTERNAL_emitter = emitter;

			// Set Apply3D-related Variables
			Vector3 emitterToListener = listener.Position - emitter.Position;
			float distance = emitterToListener.Length();
			SetVariable("Distance", distance);
			SetVariable(
				"DopplerPitchScalar",
				INTERNAL_calculateDoppler(emitterToListener, distance)
			);
			SetVariable(
				"OrientationAngle",
				MathHelper.ToDegrees((float) Math.Acos(
					Vector3.Dot(
						emitterToListener / distance, // Direction...
						listener.Forward
					) // Slope...
				)) // Angle!
			);

			INTERNAL_isPositional = true;
		}

		public float GetVariable(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (name.Equals("NumCueInstances"))
			{
				return INTERNAL_category.INTERNAL_cueInstanceCount(Name);
			}
			foreach (Variable curVar in INTERNAL_variables)
			{
				if (name.Equals(curVar.Name))
				{
					return curVar.GetValue();
				}
			}
			throw new ArgumentException("Instance variable not found!");
		}

		public void Pause()
		{
			if (IsPlaying)
			{
				INTERNAL_timer.Stop();
				foreach (SoundEffectInstance sfi in INTERNAL_instancePool)
				{
					sfi.Pause();
				}
			}
		}

		public void Play()
		{
			if (IsPlaying || IsStopping)
			{
				throw new InvalidOperationException("Cue already playing!");
			}

			// Instance limiting
			if (INTERNAL_category.INTERNAL_cueInstanceCount(Name) >= INTERNAL_data.InstanceLimit)
			{
				if (INTERNAL_data.MaxCueBehavior == MaxInstanceBehavior.Fail)
				{
					return; // Just ignore us...
				}
				else if (INTERNAL_data.MaxCueBehavior == MaxInstanceBehavior.Queue)
				{
					throw new NotImplementedException("Cue Queueing not handled!");
				}
				else if (INTERNAL_data.MaxCueBehavior == MaxInstanceBehavior.ReplaceOldest)
				{
					if (!INTERNAL_category.INTERNAL_removeOldestCue(Name))
					{
						return; // Just ignore us...
					}
				}
				else if (INTERNAL_data.MaxCueBehavior == MaxInstanceBehavior.ReplaceQuietest)
				{
					if (!INTERNAL_category.INTERNAL_removeQuietestCue(Name))
					{
						return; // Just ignore us...
					}
				}
				else if (INTERNAL_data.MaxCueBehavior == MaxInstanceBehavior.ReplaceLowestPriority)
				{
					// FIXME: Priority?
					if (!INTERNAL_category.INTERNAL_removeOldestCue(Name))
					{
						return; // Just ignore us...
					}
				}
			}

			if (!INTERNAL_category.INTERNAL_addCue(this))
			{
				Debug.Assert(false);
				return;
			}

			elapsedFrames = 0;
			INTERNAL_timer.Start();
			if (INTERNAL_data.FadeInMS > 0)
			{
				INTERNAL_startFadeIn(INTERNAL_data.FadeInMS);
			}

			if (!INTERNAL_calculateNextSound())
			{
				return;
			}

			INTERNAL_activeSound.InitializeClips();

			IsPrepared = false;
		}

		public void Resume()
		{
			if (IsPaused)
			{
				INTERNAL_timer.Start();
				foreach (SoundEffectInstance sfi in INTERNAL_instancePool)
				{
					sfi.Resume();
				}
			}
		}

		public void SetVariable(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			foreach (Variable curVar in INTERNAL_variables)
			{
				if (name.Equals(curVar.Name))
				{
					if (curVar.IsGlobal)
					{
						throw new ArgumentException("Global variables cannot be set on a cue instance!");				    
					}

					if (!curVar.IsPublic)
					{
						throw new ArgumentException("Private variables cannot be set!");				    
					}

					if (curVar.IsReadOnly)
					{
						throw new ArgumentException("Readonly variables cannot be set!");  
					}

					curVar.SetValue(value);
					return;
				}
			}
			throw new ArgumentException("Instance variable not found!");
		}

		public void Stop(AudioStopOptions options)
		{
			if (IsPlaying || IsStopping)
			{
				if (!IsPaused)
				{
					if (options == AudioStopOptions.AsAuthored)
					{
						if (INTERNAL_data.FadeOutMS > 0)
						{
							INTERNAL_startFadeOut(INTERNAL_data.FadeOutMS);
							return;
						}
						else if (INTERNAL_maxRpcReleaseTime > 0)
						{
							INTERNAL_startReleaseRpc(INTERNAL_maxRpcReleaseTime);
							return;
						}
					}
				}
				foreach (SoundEffectInstance sfi in INTERNAL_instancePool)
				{
					sfi.Stop();
					sfi.Dispose();
				}
				INTERNAL_instancePool.Clear();
				INTERNAL_instanceVolumes.Clear();
				INTERNAL_instancePitches.Clear();
				INTERNAL_rpcTrackVolumes.Clear();
				INTERNAL_rpcTrackPitches.Clear();
				INTERNAL_userControlledPlaying = false;

				KillCue();

				IsStopped = true;

				// If this is a managed Cue, we're done here.
				if (INTERNAL_isManaged)
				{
					Dispose();
				}
			}
		}

		#endregion

		#region Internal Methods

		internal bool INTERNAL_update()
		{
			// If we're not running, save some instructions...
			if (!INTERNAL_timer.IsRunning)
			{
				return true;
			}
			elapsedFrames += 1;

			// User control updates
			if (INTERNAL_data.IsUserControlled)
			{
				string varName = INTERNAL_data.UserControlVariable;
				if (	INTERNAL_userControlledPlaying &&
					(INTERNAL_baseEngine.INTERNAL_isGlobalVariable(varName) ?
						!MathHelper.WithinEpsilon(INTERNAL_controlledValue, INTERNAL_baseEngine.GetGlobalVariable(varName)) :
						!MathHelper.WithinEpsilon(INTERNAL_controlledValue, GetVariable(INTERNAL_data.UserControlVariable)))	)
				{
					// TODO: Crossfading
					foreach (SoundEffectInstance sfi in INTERNAL_instancePool)
					{
						sfi.Stop();
						sfi.Dispose();
					}
					INTERNAL_instancePool.Clear();
					INTERNAL_instanceVolumes.Clear();
					INTERNAL_instancePitches.Clear();
					INTERNAL_rpcTrackVolumes.Clear();
					INTERNAL_rpcTrackPitches.Clear();
					if (!INTERNAL_calculateNextSound())
					{
						// Nothing to play, bail.
						return true;
					}
					INTERNAL_activeSound.InitializeClips();
					INTERNAL_timer.Stop();
					INTERNAL_timer.Reset();
					INTERNAL_timer.Start();
				}

				if (INTERNAL_activeSound == null)
				{
					return INTERNAL_userControlledPlaying;
				}
			}

			// Trigger events for each track
			foreach (XACTClipInstance clip in INTERNAL_activeSound.Clips)
			{
				// Play events when the timestamp has been hit.
				for (int i = 0; i < clip.Events.Count; i += 1)
				{
					EventInstance evt = clip.Events[i];

					if (	!evt.Played &&
						INTERNAL_timer.ElapsedMilliseconds > evt.Timestamp	)
					{
						evt.Apply(
							this,
							null,
							INTERNAL_timer.ElapsedMilliseconds / 1000.0f
						);
					}
				}
			}


			// Clear out sound effect instances as they finish
			for (int i = 0; i < INTERNAL_instancePool.Count; i += 1)
			{
				if (INTERNAL_instancePool[i].State == SoundState.Stopped)
				{
					// Get the event that spawned this instance...
					PlayWaveEventInstance evtInstance =
						INTERNAL_playWaveEventBySound[INTERNAL_instancePool[i]];
					double prevVolume = INTERNAL_instanceVolumes[i];
					short prevPitch = INTERNAL_instancePitches[i];

					// Then delete all the guff
					INTERNAL_playWaveEventBySound.Remove(INTERNAL_instancePool[i]);
					INTERNAL_instancePool[i].Dispose();
					INTERNAL_instancePool.RemoveAt(i);
					INTERNAL_instanceVolumes.RemoveAt(i);
					INTERNAL_instancePitches.RemoveAt(i);
					INTERNAL_rpcTrackVolumes.RemoveAt(i);
					INTERNAL_rpcTrackPitches.RemoveAt(i);

					// Increment the loop counter, try to get another loop
					evtInstance.LoopCount += 1;
					PlayWave(evtInstance, prevVolume, prevPitch);

					// Removed a wave, have to step back...
					i -= 1;
				}
			}

			// Fade in/out
			float fadePerc = 1.0f;
			if (INTERNAL_fadeMode != FadeMode.None)
			{
				if (INTERNAL_fadeMode == FadeMode.FadeOut)
				{
					if (INTERNAL_category.crossfadeType == CrossfadeType.Linear)
					{
						fadePerc = (
							INTERNAL_fadeEnd -
							(
								INTERNAL_timer.ElapsedMilliseconds -
								INTERNAL_fadeStart
							)
						) / (float) INTERNAL_fadeEnd;
					}
					else
					{
						throw new NotImplementedException("Unhandled CrossfadeType!");
					}
					if (fadePerc <= 0.0f)
					{
						Stop(AudioStopOptions.Immediate);
						INTERNAL_fadeMode = FadeMode.None;
						return false;
					}
				}
				else if (INTERNAL_fadeMode == FadeMode.FadeIn)
				{
					if (INTERNAL_category.crossfadeType == CrossfadeType.Linear)
					{
						fadePerc = INTERNAL_timer.ElapsedMilliseconds / (float) INTERNAL_fadeEnd;
					}
					else
					{
						throw new NotImplementedException("Unhandled CrossfadeType!");
					}
					if (fadePerc > 1.0f)
					{
						fadePerc = 1.0f;
						INTERNAL_fadeMode = FadeMode.None;
					}
				}
				else if (INTERNAL_fadeMode == FadeMode.ReleaseRpc)
				{
					float releasePerc = (
						INTERNAL_timer.ElapsedMilliseconds -
						INTERNAL_fadeStart
					) / (float) INTERNAL_maxRpcReleaseTime;
					if (releasePerc > 1.0f)
					{
						Stop(AudioStopOptions.Immediate);
						INTERNAL_fadeMode = FadeMode.None;
						return false;
					}
				}
				else
				{
					throw new NotImplementedException("Unsupported FadeMode!");
				}
			}

			// If everything has been played and finished, we're done here.
			if (INTERNAL_instancePool.Count == 0)
			{
				bool allPlayed = true;
				foreach (XACTClipInstance clipInstance in INTERNAL_activeSound.Clips)
				{
					foreach (EventInstance evt in clipInstance.Events)
					{
						if (!evt.Played)
						{
							allPlayed = false;
							break;
						}
					}
				}
				if (allPlayed)
				{
					// If this is managed, we're done completely.
					if (INTERNAL_isManaged)
					{
						Dispose();
					}
					else
					{
						KillCue();
					}
					if (INTERNAL_userControlledPlaying)
					{
						// We're "still" "playing" right now...
						return true;
					}
					IsStopped = true;
					return false;
				}
			}

			// RPC updates
			float rpcVolume = 0.0f;
			float rpcPitch = 0.0f;
			float hfGain = 1.0f;
			float lfGain = 1.0f;
			for (int i = 0; i < INTERNAL_activeSound.Sound.RPCCodes.Count; i += 1)
			{
				// Are we processing an RPC targeting the sound itself rather than a track?
				bool isSoundRpc = i == 0 && INTERNAL_activeSound.Sound.HasSoundRpcs;

				// If there is an RPC targeting the sound instance itself, it is handled in rpcVolume/rpcPitch, and the first track is at i-1.
				int trackRpcIndex = INTERNAL_activeSound.Sound.HasSoundRpcs ? i - 1 : i;

				// If this RPC Code is for a track that is not active yet, we have nothing to do.
				if (trackRpcIndex >= INTERNAL_instancePool.Count)
				{
					// FIXME: This presumes that tracks start in order, which doesn't have to be true.
					break;
				}
				if (!isSoundRpc)
				{
					INTERNAL_rpcTrackVolumes[trackRpcIndex] = 0.0f;
					INTERNAL_rpcTrackPitches[trackRpcIndex] = 0.0f;
				}

				foreach (uint curCode in INTERNAL_activeSound.Sound.RPCCodes[i])
				{
					RPC curRPC = INTERNAL_baseEngine.INTERNAL_getRPC(curCode);
					float result;
					if (!INTERNAL_baseEngine.INTERNAL_isGlobalVariable(curRPC.Variable))
					{
						float variableValue;

						if (curRPC.Variable.Equals("AttackTime"))
						{
							PlayWaveEvent playWaveEvent =
								(PlayWaveEvent) INTERNAL_activeSound.Sound.INTERNAL_clips[trackRpcIndex].Events[0];

							long elapsedFromPlay = INTERNAL_timer.ElapsedMilliseconds
								- playWaveEvent.Timestamp;
							variableValue = elapsedFromPlay;
						}
						else if (curRPC.Variable.Equals("ReleaseTime"))
						{
							if (INTERNAL_fadeMode == FadeMode.ReleaseRpc)
							{
								long elapsedFromStop = INTERNAL_timer.ElapsedMilliseconds - INTERNAL_fadeStart;
								variableValue = elapsedFromStop;
							}
							else
							{
								variableValue = 0.0f;
							}
						}
						else
						{
							variableValue = GetVariable(curRPC.Variable);
						}

						result = curRPC.CalculateRPC(variableValue);
					}
					else
					{
						// It's a global variable we're looking for!
						result = curRPC.CalculateRPC(
							INTERNAL_baseEngine.GetGlobalVariable(
								curRPC.Variable
							)
						);
					}
					if (curRPC.Parameter == RPCParameter.Volume)
					{
						// If this RPC targets the sound instance itself then apply to the dedicated variable.
						if (isSoundRpc)
						{
							rpcVolume += result;
						}
						else
						{
							INTERNAL_rpcTrackVolumes[trackRpcIndex] += result;
						}
					}
					else if (curRPC.Parameter == RPCParameter.Pitch)
					{
						float pitch = result;
						if (isSoundRpc)
						{
							rpcPitch += pitch;
						}
						else
						{
							INTERNAL_rpcTrackPitches[trackRpcIndex] += pitch;
						}
					}
					else if (curRPC.Parameter == RPCParameter.FilterFrequency)
					{
						// FIXME: Just listening to the last RPC!
						float hf = result / 20000.0f;
						float lf = 1.0f - hf;
						if (isSoundRpc)
						{
							hfGain = hf;
							lfGain = lf;
						}
						else
						{
							throw new NotImplementedException("Per-track filter RPCs!");
						}
					}
					else
					{
						throw new NotImplementedException(
							"RPC Parameter Type: " + curRPC.Parameter.ToString()
						);
					}
				}
			}

			// Sound effect instance updates
			for (int i = 0; i < INTERNAL_instancePool.Count; i += 1)
			{
				/* The final volume should be the combination of the
				 * authored volume, category volume, RPC sound/track
				 * volumes, event volumes, and fade.
				 */
				INTERNAL_instancePool[i].Volume = XACTCalculator.CalculateAmplitudeRatio(
					INTERNAL_instanceVolumes[i] +
					rpcVolume +
					INTERNAL_rpcTrackVolumes[i] +
					eventVolume
				) * INTERNAL_category.INTERNAL_volume.Value * fadePerc;

				/* The final pitch should be the combination of the
				 * authored pitch, RPC sound/track pitches, and event
				 * pitch.
				 *
				 * XACT uses -1200 to 1200 (+/- 12 semitones),
				 * XNA uses -1.0f to 1.0f (+/- 1 octave).
				 */
				INTERNAL_instancePool[i].Pitch = (
					INTERNAL_instancePitches[i] +
					rpcPitch +
					INTERNAL_rpcTrackPitches[i] +
					eventPitch
				) / 1200.0f;

				/* The final filter is determined by the instance's filter type,
				 * in addition to our calculation of the HF/LF gain values.
				 */
				byte fType = INTERNAL_instancePool[i].FilterType;
				if (fType == 0xFF)
				{
					// No-op, no filter!
				}
				else if (fType == 0)
				{
					INTERNAL_instancePool[i].INTERNAL_applyLowPassFilter(hfGain);
				}
				else if (fType == 1)
				{
					INTERNAL_instancePool[i].INTERNAL_applyHighPassFilter(lfGain);
				}
				else if (fType == 2)
				{
					INTERNAL_instancePool[i].INTERNAL_applyBandPassFilter(hfGain, lfGain);
				}
				else
				{
					throw new InvalidOperationException("Unhandled filter type!");
				}

				// Update 3D position, if applicable
				if (INTERNAL_isPositional)
				{
					INTERNAL_instancePool[i].Apply3D(
						INTERNAL_listener,
						INTERNAL_emitter
					);
				}
			}

			return true;
		}

		internal void INTERNAL_genVariables(List<Variable> cueVariables)
		{
			INTERNAL_variables = cueVariables;
		}

		internal float INTERNAL_calculateVolume()
		{
			float retval = 0.0f;
			for (int i = 0; i < INTERNAL_activeSound.Sound.RPCCodes.Count; i += 1)
			foreach (uint curCode in INTERNAL_activeSound.Sound.RPCCodes[i])
			{
				RPC curRPC = INTERNAL_baseEngine.INTERNAL_getRPC(curCode);
				if (curRPC.Parameter != RPCParameter.Volume)
				{
					continue;
				}
				float result;
				if (!INTERNAL_baseEngine.INTERNAL_isGlobalVariable(curRPC.Variable))
				{
					float variableValue;

					if (curRPC.Variable.Equals("AttackTime"))
					{
						long elapsedFromPlay = INTERNAL_timer.ElapsedMilliseconds;
						variableValue = elapsedFromPlay;
					}
					else if (curRPC.Variable.Equals("ReleaseTime"))
					{
						if (INTERNAL_fadeMode == FadeMode.ReleaseRpc)
						{
							long elapsedFromStop = INTERNAL_timer.ElapsedMilliseconds - INTERNAL_fadeStart;
							variableValue = elapsedFromStop;
						}
						else
						{
							variableValue = 0.0f;
						}
					}
					else
					{
						variableValue = GetVariable(curRPC.Variable);
					}

					result = curRPC.CalculateRPC(variableValue);
				}
				else
				{
					// It's a global variable we're looking for!
					result = curRPC.CalculateRPC(
						INTERNAL_baseEngine.GetGlobalVariable(
							curRPC.Variable
						)
					);
				}
				retval += result;
			}
			return retval;
		}

		internal void INTERNAL_startFadeIn(ushort ms)
		{
			// start is not used, since it's always 0 anyway -flibit
			INTERNAL_fadeEnd = ms;
			INTERNAL_fadeMode = FadeMode.FadeIn;
		}

		internal void INTERNAL_startFadeOut(ushort ms)
		{
			if (INTERNAL_fadeMode == FadeMode.FadeOut)
			{
				return; // Already in the middle of something...
			}
			INTERNAL_fadeStart = INTERNAL_timer.ElapsedMilliseconds;
			INTERNAL_fadeEnd = ms;
			INTERNAL_fadeMode = FadeMode.FadeOut;
			INTERNAL_category.INTERNAL_moveToDying(this);
		}

		internal void INTERNAL_startReleaseRpc(ushort ms)
		{
			INTERNAL_fadeStart = INTERNAL_timer.ElapsedMilliseconds;
			INTERNAL_fadeEnd = ms;
			INTERNAL_fadeMode = FadeMode.ReleaseRpc;
		}

		#endregion

		#region Private Methods

		private bool INTERNAL_calculateNextSound()
		{
			if (INTERNAL_activeSound != null)
			{
				INTERNAL_activeSound.Dispose(
					INTERNAL_baseEngine,
					INTERNAL_waveBankNames
				);
				INTERNAL_activeSound = null;
			}

			INTERNAL_playWaveEventBySound.Clear();

			// Pick a sound based on a Cue instance variable
			if (INTERNAL_data.IsUserControlled)
			{
				INTERNAL_userControlledPlaying = true;
				if (INTERNAL_baseEngine.INTERNAL_isGlobalVariable(INTERNAL_data.UserControlVariable))
				{
					INTERNAL_controlledValue = INTERNAL_baseEngine.GetGlobalVariable(
						INTERNAL_data.UserControlVariable
					);
				}
				else
				{
					INTERNAL_controlledValue = GetVariable(
						INTERNAL_data.UserControlVariable
					);
				}
				for (int i = 0; i < INTERNAL_data.Probabilities.Length / 2; i += 1)
				{
					if (	INTERNAL_controlledValue <= INTERNAL_data.Probabilities[i, 0] &&
						INTERNAL_controlledValue >= INTERNAL_data.Probabilities[i, 1]	)
					{
						INTERNAL_activeSound = INTERNAL_data.Sounds[i].GenInstance(
							INTERNAL_baseEngine,
							INTERNAL_waveBankNames
						);
						return true;
					}
				}

				/* This should only happen when the
				 * UserControlVariable is none of the sound
				 * probabilities, in which case we are just
				 * silent. But, we are still claiming to be
				 * "playing" in the meantime.
				 * -flibit
				 */
				return false;
			}

			// Randomly pick a sound
			double max = 0.0;
			for (int i = 0; i < INTERNAL_data.Probabilities.GetLength(0); i += 1)
			{
				max += INTERNAL_data.Probabilities[i, 0] - INTERNAL_data.Probabilities[i, 1];
			}
			double next = random.NextDouble() * max;

			for (int i = INTERNAL_data.Probabilities.GetLength(0) - 1; i >= 0; i -= 1)
			{
				if (next > max - (INTERNAL_data.Probabilities[i, 0] - INTERNAL_data.Probabilities[i, 1]))
				{
					INTERNAL_activeSound = INTERNAL_data.Sounds[i].GenInstance(
						INTERNAL_baseEngine,
						INTERNAL_waveBankNames
					);
					break;
				}
				max -= INTERNAL_data.Probabilities[i, 0] - INTERNAL_data.Probabilities[i, 1];
			}

			return true;
		}

		private void KillCue()
		{
			INTERNAL_timer.Stop();
			INTERNAL_timer.Reset();
			if (INTERNAL_activeSound != null)
			{
				if (INTERNAL_waveBankNames.Count > 0) // AKA !SoundBank.IsDisposed
				{
					INTERNAL_activeSound.Dispose(
						INTERNAL_baseEngine,
						INTERNAL_waveBankNames
					);
				}
				INTERNAL_activeSound = null;
			}
			INTERNAL_category.INTERNAL_removeActiveCue(this);
		}

		internal void PlayWave(
			EventInstance eventInstance,
			double? prevVolume = null,
			short? prevPitch = null
		) {
			PlayWaveEventInstance playWaveEventInstance =
				(PlayWaveEventInstance) eventInstance;
			PlayWaveEvent evt = (PlayWaveEvent) eventInstance.Event;

			double finalVolume;
			short finalPitch;
			SoundEffectInstance sfi = evt.GenerateInstance(
				INTERNAL_activeSound.Sound.Volume,
				INTERNAL_activeSound.Sound.Pitch,
				playWaveEventInstance.LoopCount,
				prevVolume,
				prevPitch,
				out finalVolume,
				out finalPitch
			);
			if (sfi != null)
			{
				if (INTERNAL_isPositional)
				{
					sfi.Apply3D(INTERNAL_listener, INTERNAL_emitter);
				}
				foreach (uint curDSP in INTERNAL_activeSound.Sound.DSPCodes)
				{
					// FIXME: This only applies the last DSP!
					sfi.INTERNAL_applyReverb(
						INTERNAL_baseEngine.INTERNAL_getDSP(curDSP)
					);
				}
				INTERNAL_instancePool.Add(sfi);
				INTERNAL_instanceVolumes.Add(finalVolume);
				INTERNAL_instancePitches.Add(finalPitch);

				INTERNAL_playWaveEventBySound.Add(sfi, playWaveEventInstance);
				INTERNAL_rpcTrackVolumes.Add(0.0f);
				INTERNAL_rpcTrackPitches.Add(0.0f);
				sfi.Play();
			}
		}

		private float INTERNAL_calculateDoppler(Vector3 emitterToListener, float distance)
		{
			/* Adapted from algorithm published as a part of the webaudio specification:
			 * https://dvcs.w3.org/hg/audio/raw-file/tip/webaudio/specification.html#Spatialization-doppler-shift
			 * -Chad
			 */

			float dopplerShift = 1.0f;

			float dopplerFactor = INTERNAL_emitter.DopplerScale;
			if (dopplerFactor > 0.0f)
			{
				float speedOfSound = INTERNAL_baseEngine.GetGlobalVariable("SpeedOfSound");
				float scaledSpeedOfSound = speedOfSound / dopplerFactor;

				// Project the velocities along the emitter to listener vector.
				float projectedListenerVelocity = Vector3.Dot(
					emitterToListener,
					INTERNAL_listener.Velocity
				) / distance;
				float projectedEmitterVelocity = Vector3.Dot(
					emitterToListener,
					INTERNAL_emitter.Velocity
				) / distance;

				// Clamp to the speed of the medium.
				projectedListenerVelocity = Math.Min(
					projectedListenerVelocity,
					scaledSpeedOfSound
				);
				projectedEmitterVelocity = Math.Min(
					projectedEmitterVelocity,
					scaledSpeedOfSound
				);

				// Apply doppler effect.
				dopplerShift = (
					speedOfSound - dopplerFactor * projectedListenerVelocity
				) / (
					speedOfSound - dopplerFactor * projectedEmitterVelocity
				);
				if (float.IsNaN(dopplerShift))
				{
					dopplerShift = 1.0f;
				}

				// Limit the pitch shifting to 2 octaves up and 1 octaves down per XACT behavior.
				dopplerShift = MathHelper.Clamp(dopplerShift, 0.5f, 4.0f);
			}

			return dopplerShift;
		}

		#endregion
	}

	internal class XACTSoundInstance
	{
		public readonly XACTSound Sound;
		public readonly List<XACTClipInstance> Clips = new List<XACTClipInstance>();

		public XACTSoundInstance(XACTSound sound)
		{
			Sound = sound;
		}

		public void Dispose(AudioEngine audioEngine, List<string> waveBankNames)
		{
			Clips.Clear();
			Sound.DisposeInstance(this, audioEngine, waveBankNames);
		}

		internal void InitializeClips()
		{
			// Create clip instances for each clip (track).
			foreach (XACTClip curClip in Sound.INTERNAL_clips)
			{
				XACTClipInstance clipInstance = new XACTClipInstance(curClip);
				Clips.Add(clipInstance);
			}
		}
	}

	internal class XACTClipInstance
	{
		public readonly XACTClip Clip;
		public readonly List<EventInstance> Events = new List<EventInstance>();

		public XACTClipInstance(XACTClip clip)
		{
			Clip = clip;

			// Create event instances for each event.
			foreach (XACTEvent evt in Clip.Events)
			{
				// TODO: How best to eliminate this switch? Factory template method? Table of delegates?
				EventInstance eventInstance = null;
				if (evt is PlayWaveEvent)
				{
					eventInstance = new PlayWaveEventInstance((PlayWaveEvent) evt);
				}
				else if (evt is StopEvent)
				{
					eventInstance = new StopEventInstance((StopEvent) evt);
				}
				else if (evt is SetValueEvent)
				{
					eventInstance = new SetValueEventInstance((SetValueEvent) evt);
				}
				else if (evt is SetRandomValueEvent)
				{
					eventInstance = new SetRandomValueEventInstance((SetRandomValueEvent) evt);
				}
				else if (evt is SetRampValueEvent)
				{
					eventInstance = new SetRampValueEventInstance((SetRampValueEvent) evt);
				}
				else if (evt is MarkerEvent)
				{
					eventInstance = new MarkerEventInstance((MarkerEvent) evt);
				}

				Debug.Assert(eventInstance != null);
				Events.Add(eventInstance);
			}
		}
	}

	internal abstract class EventInstance
	{
		public readonly XACTEvent Event;
		public float Timestamp;
		public int LoopCount;
		public bool Played;

		public EventInstance(XACTEvent evt)
		{
			Event = evt;
			Timestamp = (
				Event.Timestamp +
				XACTEvent.Random.Next(0, Event.RandomOffset)
			);
			LoopCount = Event.LoopCount;
			Played = false;
		}

		public abstract void Apply(Cue cue, XACTClip track, float elapsedTime);

		protected void HandleRepeating()
		{
			if (LoopCount > 0)
			{
				// If not set to infinite looping.
				if (Event.LoopCount != 65535)
				{
					LoopCount = LoopCount - 1;
				}

				// FIXME: Use Frequency Units (Seconds / Beats per Minute) instead of constant of seconds.
				Timestamp = Timestamp + Event.Frequency * 1000.0f;
			}
			else
			{
				Played = true;
			}
		}
	}

	internal class PlayWaveEventInstance : EventInstance
	{
		public PlayWaveEventInstance(PlayWaveEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			// Only actually play if we are not in the process of stopping.
			if (!cue.IsStopping)
			{
				cue.PlayWave(this);
			}
			Played = true;
		}
	}

	internal class StopEventInstance : EventInstance
	{
		public StopEventInstance(StopEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			StopEvent evt = (StopEvent) Event;

			AudioStopOptions stopOptions = evt.StopOptions;

			switch (evt.Scope)
			{
				case XACTClip.StopEventScope.Cue:
					cue.Stop(stopOptions);
					break;
				case XACTClip.StopEventScope.Track:
					/* FIXME: Need to stop this and ONLY this track
					 * track.Stop(stopOptions);
					 */
					break;
			}

			Played = true;
		}
	}

	internal class SetValueEventInstance : EventInstance
	{
		public SetValueEventInstance(SetValueEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			SetValueEvent evt = (SetValueEvent) Event;
			switch (evt.Property)
			{
				case CueProperty.Volume:
					cue.eventVolume = evt.GetVolume(cue.eventVolume);
					break;
				case CueProperty.Pitch:
					cue.eventPitch = evt.GetPitch(cue.eventPitch);
					break;
			}

			HandleRepeating();
		}
	}

	internal class SetRandomValueEventInstance : EventInstance
	{
		public SetRandomValueEventInstance(SetRandomValueEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			SetRandomValueEvent evt = (SetRandomValueEvent) Event;
			switch (evt.Property)
			{
				case CueProperty.Volume:
					cue.eventVolume = evt.GetVolume(cue.eventVolume);
					break;
				case CueProperty.Pitch:
					cue.eventPitch = evt.GetPitch(cue.eventPitch);
					break;
			}

			HandleRepeating();
		}
	}

	internal class SetRampValueEventInstance : EventInstance
	{
		public SetRampValueEventInstance(SetRampValueEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			SetRampValueEvent evt = (SetRampValueEvent) Event;
			if (elapsedTime <= Timestamp / 1000.0f + evt.Duration)
			{
				switch (evt.Property)
				{
					case CueProperty.Volume:
						cue.eventVolume = GetValue(evt, elapsedTime);
						break;
					case CueProperty.Pitch:
						cue.eventPitch = GetValue(evt, elapsedTime);
						break;
				}
			}
			else
			{
				HandleRepeating();
			}
		}

		private float GetValue(SetRampValueEvent x, float elapsedTime)
		{
			// Number of slices to break up the duration.
			const float slices = 10;
			float endValue = x.InitialSlope * x.Duration * slices + x.InitialValue;

			// FIXME: Incorporate 2nd derivative into the interpolated pitch.

			float amount = MathHelper.Clamp(
				(elapsedTime - Timestamp / 1000.0f) / x.Duration,
				0.0f,
				1.0f
			);
			return MathHelper.Lerp(x.InitialValue, endValue, amount);
		}
	}

	internal class MarkerEventInstance : EventInstance
	{
		public MarkerEventInstance(MarkerEvent evt)
			: base(evt)
		{
		}

		public override void Apply(Cue cue, XACTClip track, float elapsedTime)
		{
			// FIXME: Implement action for a marker event. Some kind of callback?

			HandleRepeating();
		}
	}
}
