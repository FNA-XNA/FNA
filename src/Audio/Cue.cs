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
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.cue.aspx
	public sealed class Cue : IDisposable
	{
		#region Public Properties

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
						INTERNAL_timer.ElapsedTicks > 0	);
			}
		}

		public bool IsPrepared
		{
			get;
			private set;
		}

		public bool IsPreparing
		{
			get;
			private set;
		}

		public bool IsStopped
		{
			get
			{
				return !IsPlaying;
			}
		}

		public bool IsStopping
		{
			get
			{
				return INTERNAL_fadeMode == FadeMode.FadeOut;
			}
		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		internal bool JustStarted
		{
			get
			{
				// Arbitrarily 1/12 of a second, with some wiggle room -flibit
				return INTERNAL_timer.ElapsedMilliseconds < 80;
			}
		}

		#endregion

		#region Private Variables

		private AudioEngine INTERNAL_baseEngine;

		// Cue information parsed from the SoundBank
		private CueData INTERNAL_data;

		// Current sound and its events
		private XACTSound INTERNAL_activeSound;
		private List<XACTEvent> INTERNAL_eventList;
		private List<bool> INTERNAL_eventPlayed;
		private Dictionary<XACTEvent, int> INTERNAL_eventLoops;
		private Dictionary<SoundEffectInstance, XACTEvent> INTERNAL_waveEventSounds;

		// Used for event timestamps
		private Stopwatch INTERNAL_timer;

		// Sound list
		private List<SoundEffectInstance> INTERNAL_instancePool;
		private List<float> INTERNAL_instanceVolumes;
		private List<float> INTERNAL_instancePitches;

		// RPC data list
		private List<float> INTERNAL_rpcTrackVolumes;
		private List<float> INTERNAL_rpcTrackPitches;

		// Events can control volume/pitch as well!
		private float eventVolume;
		private float eventPitch;

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
			FadeIn
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

			Name = name;

			INTERNAL_data = data;
			foreach (XACTSound curSound in data.Sounds)
			{
				if (!curSound.HasLoadedTracks)
				{
					curSound.LoadTracks(
						INTERNAL_baseEngine,
						waveBankNames
					);
				}
			}

			INTERNAL_isManaged = managed;

			INTERNAL_category = INTERNAL_baseEngine.INTERNAL_initCue(
				this,
				data.Category
			);

			eventVolume = 1.0f;
			eventPitch = 0.0f;

			INTERNAL_userControlledPlaying = false;
			INTERNAL_isPositional = false;

			INTERNAL_eventList = new List<XACTEvent>();
			INTERNAL_eventPlayed = new List<bool>();
			INTERNAL_eventLoops = new Dictionary<XACTEvent, int>();
			INTERNAL_waveEventSounds = new Dictionary<SoundEffectInstance, XACTEvent>();

			INTERNAL_timer =  new Stopwatch();

			INTERNAL_instancePool = new List<SoundEffectInstance>();
			INTERNAL_instanceVolumes = new List<float>();
			INTERNAL_instancePitches = new List<float>();

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
					INTERNAL_timer.Stop();
				}
				INTERNAL_category.INTERNAL_removeActiveCue(this);
				IsDisposed = true;
			}
		}

		#endregion

		#region Public Methods

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			if (IsPlaying && !INTERNAL_isPositional)
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
			SetVariable(
				"Distance",
				Vector3.Distance(
					INTERNAL_emitter.Position,
					INTERNAL_listener.Position
				)
			);
			// TODO: DopplerPitchScaler, OrientationAngle
			INTERNAL_isPositional = true;
		}

		public float GetVariable(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
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
			if (IsPlaying)
			{
				throw new InvalidOperationException("Cue already playing!");
			}

			INTERNAL_category.INTERNAL_initCue(this);

			if (GetVariable("NumCueInstances") >= INTERNAL_data.InstanceLimit)
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
				return;
			}

			INTERNAL_timer.Start();
			if (INTERNAL_data.FadeInMS > 0)
			{
				INTERNAL_startFadeIn(INTERNAL_data.FadeInMS);
			}

			if (!INTERNAL_calculateNextSound())
			{
				return;
			}

			INTERNAL_activeSound.GatherEvents(INTERNAL_eventList);
			foreach (XACTEvent evt in INTERNAL_eventList)
			{
				INTERNAL_eventPlayed.Add(false);
				INTERNAL_eventLoops.Add(evt, 0);
			}
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
					curVar.SetValue(value);
					return;
				}
			}
			throw new ArgumentException("Instance variable not found!");
		}

		public void Stop(AudioStopOptions options)
		{
			if (IsPlaying)
			{
				if (	options == AudioStopOptions.AsAuthored &&
					INTERNAL_data.FadeOutMS > 0	)
				{
					INTERNAL_startFadeOut(INTERNAL_data.FadeOutMS);
					return;
				}
				INTERNAL_timer.Stop();
				INTERNAL_timer.Reset();
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
				INTERNAL_category.INTERNAL_removeActiveCue(this);

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

			// Play events when the timestamp has been hit.
			for (int i = 0; i < INTERNAL_eventList.Count; i += 1)
			{
				if (	!INTERNAL_eventPlayed[i] &&
					INTERNAL_timer.ElapsedMilliseconds > INTERNAL_eventList[i].Timestamp	)
				{
					uint type = INTERNAL_eventList[i].Type;
					if (type == 1)
					{
						PlayWave((PlayWaveEvent) INTERNAL_eventList[i]);
					}
					else if (type == 2)
					{
						eventVolume = ((SetVolumeEvent) INTERNAL_eventList[i]).GetVolume();
					}
					else if (type == 3)
					{
						eventPitch = ((SetPitchEvent) INTERNAL_eventList[i]).GetPitch();
					}
					else
					{
						throw new NotImplementedException("Unhandled XACTEvent type!");
					}
					INTERNAL_eventPlayed[i] = true;
				}
			}

			// Clear out sound effect instances as they finish
			for (int i = 0; i < INTERNAL_instancePool.Count; i += 1)
			{
				if (INTERNAL_instancePool[i].State == SoundState.Stopped)
				{
					// Get the event that spawned this instance...
					PlayWaveEvent evt = (PlayWaveEvent) INTERNAL_waveEventSounds[INTERNAL_instancePool[i]];

					// Then delete all the guff
					INTERNAL_waveEventSounds.Remove(INTERNAL_instancePool[i]);
					INTERNAL_instancePool[i].Dispose();
					INTERNAL_instancePool.RemoveAt(i);
					INTERNAL_instanceVolumes.RemoveAt(i);
					INTERNAL_instancePitches.RemoveAt(i);
					INTERNAL_rpcTrackVolumes.RemoveAt(i);
					INTERNAL_rpcTrackPitches.RemoveAt(i);

					// Increment the loop counter, try to get another loop
					INTERNAL_eventLoops[evt] += 1;
					PlayWave(evt);

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
						fadePerc = (INTERNAL_fadeEnd - (INTERNAL_timer.ElapsedMilliseconds - INTERNAL_fadeStart)) / (float) INTERNAL_fadeEnd;
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
				else
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
			}

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
					INTERNAL_activeSound.GatherEvents(INTERNAL_eventList);
					foreach (XACTEvent evt in INTERNAL_eventList)
					{
						INTERNAL_eventPlayed.Add(false);
						INTERNAL_eventLoops.Add(evt, 0);
					}
					INTERNAL_timer.Stop();
					INTERNAL_timer.Reset();
					INTERNAL_timer.Start();
				}

				if (INTERNAL_activeSound == null)
				{
					return INTERNAL_userControlledPlaying;
				}
			}

			// If everything has been played and finished, we're done here.
			if (INTERNAL_instancePool.Count == 0)
			{
				bool allPlayed = true;
				foreach (bool played in INTERNAL_eventPlayed)
				{
					if (!played)
					{
						allPlayed = false;
						break;
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
						INTERNAL_timer.Stop();
						INTERNAL_timer.Reset();
						INTERNAL_category.INTERNAL_removeActiveCue(this);
					}
					return INTERNAL_userControlledPlaying;
				}
			}

			// RPC updates
			float rpcVolume = 1.0f;
			float rpcPitch = 0.0f;
			float hfGain = 1.0f;
			float lfGain = 1.0f;
			for (int i = 0; i < INTERNAL_activeSound.RPCCodes.Count; i += 1)
			{
				if (i > INTERNAL_instancePool.Count)
				{
					break;
				}
				if (i > 0)
				{
					INTERNAL_rpcTrackVolumes[i - 1] = 1.0f;
					INTERNAL_rpcTrackPitches[i - 1] = 0.0f;
				}
				foreach (uint curCode in INTERNAL_activeSound.RPCCodes[i])
				{
					RPC curRPC = INTERNAL_baseEngine.INTERNAL_getRPC(curCode);
					float result;
					if (!INTERNAL_baseEngine.INTERNAL_isGlobalVariable(curRPC.Variable))
					{
						result = curRPC.CalculateRPC(GetVariable(curRPC.Variable));
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
						float vol = XACTCalculator.CalculateAmplitudeRatio(result / 100.0);
						if (i == 0)
						{
							rpcVolume *= vol;
						}
						else
						{
							INTERNAL_rpcTrackVolumes[i - 1] *= vol;
						}
					}
					else if (curRPC.Parameter == RPCParameter.Pitch)
					{
						float pitch = result / 1000.0f;
						if (i == 0)
						{
							rpcPitch += pitch;
						}
						else
						{
							INTERNAL_rpcTrackPitches[i - 1] += pitch;
						}
					}
					else if (curRPC.Parameter == RPCParameter.FilterFrequency)
					{
						// FIXME: Just listening to the last RPC!
						float hf = result / 20000.0f;
						float lf = 1.0f - hf;
						if (i == 0)
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
						throw new NotImplementedException("RPC Parameter Type: " + curRPC.Parameter.ToString());
					}
				}
			}

			// Sound effect instance updates
			for (int i = 0; i < INTERNAL_instancePool.Count; i += 1)
			{
				/* The final volume should be the combination of the
				 * authored volume, category volume, RPC/Event volumes, and fade.
				 */
				INTERNAL_instancePool[i].Volume = (
					INTERNAL_instanceVolumes[i] *
					INTERNAL_category.INTERNAL_volume.Value *
					rpcVolume *
					INTERNAL_rpcTrackVolumes[i] *
					eventVolume *
					fadePerc
				);

				/* The final pitch should be the combination of the
				 * authored pitch and RPC/Event pitch results.
				 */
				INTERNAL_instancePool[i].Pitch = (
					INTERNAL_instancePitches[i] +
					rpcPitch +
					eventPitch +
					INTERNAL_rpcTrackPitches[i]
				);

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
			float retval = 1.0f;
			for (int i = 0; i < INTERNAL_activeSound.RPCCodes.Count; i += 1)
			foreach (uint curCode in INTERNAL_activeSound.RPCCodes[i])
			{
				RPC curRPC = INTERNAL_baseEngine.INTERNAL_getRPC(curCode);
				if (curRPC.Parameter != RPCParameter.Volume)
				{
					continue;
				}
				float result;
				if (!INTERNAL_baseEngine.INTERNAL_isGlobalVariable(curRPC.Variable))
				{
					result = curRPC.CalculateRPC(GetVariable(curRPC.Variable));
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
				retval *= XACTCalculator.CalculateAmplitudeRatio(result / 100.0);
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
			INTERNAL_fadeStart = INTERNAL_timer.ElapsedMilliseconds;
			INTERNAL_fadeEnd = ms;
			INTERNAL_fadeMode = FadeMode.FadeOut;
		}

		#endregion

		#region Private Methods

		private bool INTERNAL_calculateNextSound()
		{
			INTERNAL_activeSound = null;
			INTERNAL_eventList.Clear();
			INTERNAL_eventPlayed.Clear();
			INTERNAL_eventLoops.Clear();
			INTERNAL_waveEventSounds.Clear();

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
						INTERNAL_activeSound = INTERNAL_data.Sounds[i];
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
					INTERNAL_activeSound = INTERNAL_data.Sounds[i];
					break;
				}
				max -= INTERNAL_data.Probabilities[i, 0] - INTERNAL_data.Probabilities[i, 1];
			}

			return true;
		}

		private void PlayWave(PlayWaveEvent evt)
		{
			SoundEffectInstance sfi = evt.GenerateInstance(
				INTERNAL_activeSound.Volume,
				INTERNAL_activeSound.Pitch,
				INTERNAL_eventLoops[evt]
			);
			if (sfi != null)
			{
				if (INTERNAL_isPositional)
				{
					sfi.Apply3D(INTERNAL_listener, INTERNAL_emitter);
				}
				foreach (uint curDSP in INTERNAL_activeSound.DSPCodes)
				{
					// FIXME: This only applies the last DSP!
					sfi.INTERNAL_applyReverb(
						INTERNAL_baseEngine.INTERNAL_getDSP(curDSP)
					);
				}
				INTERNAL_instancePool.Add(sfi);
				INTERNAL_instanceVolumes.Add(sfi.Volume);
				INTERNAL_instancePitches.Add(sfi.Pitch);
				INTERNAL_waveEventSounds.Add(sfi, evt);
				INTERNAL_rpcTrackVolumes.Add(1.0f);
				INTERNAL_rpcTrackPitches.Add(0.0f);
				sfi.Play();
			}
		}

		#endregion
	}
}
