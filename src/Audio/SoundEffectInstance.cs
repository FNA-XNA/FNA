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
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundeffectinstance.aspx
	public class SoundEffectInstance : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			protected set;
		}

		private bool INTERNAL_looped = false;
		public virtual bool IsLooped
		{
			get
			{
				return INTERNAL_looped;
			}
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				if (hasStarted)
				{
					throw new InvalidOperationException("Loop must be set before the first Play call.");
				}
				INTERNAL_looped = value;
			}
		}

		private float INTERNAL_pan = 0.0f;
		public float Pan
		{
			get
			{
				return INTERNAL_pan;
			}
			set
			{
				if (!(value >= -1f && value <= 1f))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				if (hasStarted && is3D)
				{
					throw new InvalidOperationException("Pan cannot be set on a 3D sound. To ensure a 2D sound avoid calling Apply3D and ensure Pan is set before the first Play call.");
				}
				is3D = false;
				INTERNAL_pan = value;

				SetPanMatrixCoefficients();
				if (handle != IntPtr.Zero)
				{
					FAudio.FAudioVoice_SetOutputMatrix(
						handle,
						SoundEffect.Device().MasterVoice,
						Channels,
						dspSettings.DstChannelCount,
						dspSettings.pMatrixCoefficients,
						0
					);
				}
			}
		}

		private float INTERNAL_pitch = 0.0f;
		public float Pitch
		{
			get
			{
				return INTERNAL_pitch;
			}
			set
			{
				if (!(value >= -1f && value <= 1f))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				INTERNAL_pitch = value;
				if (handle != IntPtr.Zero)
				{
					UpdatePitch();
				}
			}
		}

		private SoundState INTERNAL_state = SoundState.Stopped;
		public SoundState State
		{
			get
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				if (	!isDynamic &&
					handle != IntPtr.Zero &&
					INTERNAL_state == SoundState.Playing	)
				{
					FAudio.FAudioVoiceState state;
					FAudio.FAudioSourceVoice_GetState(handle, out state, 0);
					if (state.BuffersQueued == 0 && state.SamplesPlayed == 0)
					{
						Stop(true);
					}
				}
				return INTERNAL_state;
			}
		}

		private float INTERNAL_volume = 1f;
		public float Volume
		{
			get
			{
				return INTERNAL_volume;
			}
			set
			{
				if (!(value >= -FAudio.FAUDIO_MAX_VOLUME_LEVEL && value <= FAudio.FAUDIO_MAX_VOLUME_LEVEL)) // XNA: !(value >= 0f && value <= 1f)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				INTERNAL_volume = value;
				if (handle != IntPtr.Zero)
				{
					FAudio.FAudioVoice_SetVolume(
						handle,
						INTERNAL_volume,
						0
					);
				}
			}
		}

		#endregion

		#region Internal Property

		internal virtual uint Channels
		{
			get
			{
				return parentEffect.channels;
			}
		}

		#endregion

		#region Internal Variables

		internal IntPtr handle;

		#endregion

		#region Private Variables

		private SoundEffect parentEffect;
		private WeakReference selfReference;
		private bool isDynamic;
		private bool hasStarted;
		private bool is3D;
		private bool usingReverb;
		private FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

		#endregion

		#region Private Static Variables

		private static readonly float maxFreqRatio = Environment.GetEnvironmentVariable(
			"FNA_SOUNDEFFECT_UNCAPPED_PITCH"
		) == "1" ? FAudio.FAUDIO_MAX_FREQ_RATIO : FAudio.FAUDIO_DEFAULT_FREQ_RATIO;

		#endregion

		#region Internal Constructor

		internal SoundEffectInstance(SoundEffect parent)
		{
			SoundEffect.Device();

			selfReference = new WeakReference(this, true);
			parentEffect = parent;
			hasStarted = false;
			is3D = false;
			usingReverb = false;
			INTERNAL_state = SoundState.Stopped;

			if (parentEffect != null)
			{
				InitDSPSettings();
				parentEffect.Instances.Add(selfReference);
			}
			else
			{
				// Only DynamicSoundEffectInstance can avoid sending a SoundEffect base
				isDynamic = true;
			}
		}

		#endregion

		#region Destructor

		~SoundEffectInstance()
		{
			if (!SoundEffect.FAudioContext.ProgramExiting && !IsDisposed && State == SoundState.Playing)
			{
				// STOP LEAKING YOUR INSTANCES, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}
			Dispose();
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public unsafe void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
		{
			if (listeners == null || listeners.Length == 0)
			{
				throw new ArgumentNullException("listeners");
			}
			if (emitter == null)
			{
				throw new ArgumentNullException("emitter");
			}
			AudioListener listener = listeners[0];
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException(
					"SoundEffectInstance"
				);
			}
			if (hasStarted && !is3D)
			{
				throw new InvalidOperationException("The sound is not a 3D sound. Call Apply3D before the first Play call to configure it to be a 3D sound.");
			}
			if (Channels > 2)
			{
				throw new InvalidOperationException("An unexpected error has occurred.");
			}

			is3D = true;
			SoundEffect.FAudioContext dev = SoundEffect.Device();

			FAudio.F3DAUDIO_EMITTER emitterData = emitter.emitterData;
			emitterData.InnerRadius = dev.CurveDistanceScaler;
			emitterData.InnerRadiusAngle = (float) Math.PI / 6f;
			emitterData.ChannelCount = 1;
			emitterData.CurveDistanceScaler = dev.CurveDistanceScaler;
			emitterData.DopplerScaler *= dev.DopplerScale;

			uint flags = FAudio.F3DAUDIO_CALCULATE_MATRIX | FAudio.F3DAUDIO_CALCULATE_ZEROCENTER;
			if (emitterData.DopplerScaler != 0)
			{
				flags |= FAudio.F3DAUDIO_CALCULATE_DOPPLER;
			}

			FAudio.F3DAudioCalculate(
				dev.Handle3D,
				ref listener.listenerData,
				ref emitterData,
				flags,
				ref dspSettings
			);
			if (listeners.Length > 1)
			{
				float* pMatrixCoefficients1 = (float*) dspSettings.pMatrixCoefficients;
				dspSettings.pMatrixCoefficients = FNAPlatform.Malloc((int) (4 * Channels * dspSettings.DstChannelCount));
				float* pMatrixCoefficients2 = (float*) dspSettings.pMatrixCoefficients;

				float DopplerFactor = dspSettings.DopplerFactor;

				for (int num = 1; num < listeners.Length; num++)
				{
					FAudio.F3DAudioCalculate(
						dev.Handle3D,
						ref listeners[num].listenerData,
						ref emitterData,
						flags,
						ref dspSettings
					);
					for (int dstChannelIndex = 0; dstChannelIndex < dspSettings.DstChannelCount; dstChannelIndex++)
					{
						pMatrixCoefficients1[dstChannelIndex] = (pMatrixCoefficients1[dstChannelIndex] * num + pMatrixCoefficients2[dstChannelIndex]) / (num + 1);
					}
					DopplerFactor = (num * DopplerFactor + dspSettings.DopplerFactor) / (num + 1);
				}
				FNAPlatform.Free(dspSettings.pMatrixCoefficients);
				dspSettings.pMatrixCoefficients = (IntPtr) pMatrixCoefficients1;
				dspSettings.DopplerFactor = DopplerFactor;
			}
			if (Channels == 2)
			{
				for (uint i = dspSettings.DstChannelCount - 1; i > 0; i--)
				{
					((float*) dspSettings.pMatrixCoefficients)[2*i+1] = ((float*) dspSettings.pMatrixCoefficients)[i] * 0.70710677f;
					((float*) dspSettings.pMatrixCoefficients)[2*i] = ((float*) dspSettings.pMatrixCoefficients)[i] * 0.70710677f;
				}
			}
			if (handle != IntPtr.Zero)
			{
				if (emitterData.DopplerScaler != 0)
				{
					FAudio.FAudioSourceVoice_SetFrequencyRatio(
						handle,
						dspSettings.DopplerFactor,
						0
					);
				}
				FAudio.FAudioVoice_SetOutputMatrix(
					handle,
					SoundEffect.Device().MasterVoice,
					Channels,
					dspSettings.DstChannelCount,
					dspSettings.pMatrixCoefficients,
					0
				);
			}
		}

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			Apply3D(new AudioListener[1] { listener }, emitter);
		}

		public virtual void Play()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (State == SoundState.Playing)
			{
				return;
			}
			if (State == SoundState.Paused)
			{
				/* Just resume the existing handle */
				FAudio.FAudioSourceVoice_Start(handle, 0, 0);
				INTERNAL_state = SoundState.Playing;
				return;
			}

			SoundEffect.FAudioContext dev = SoundEffect.Device();

			/* Create handle */
			if (isDynamic)
			{
				FAudio.FAudio_CreateSourceVoice(
					dev.Handle,
					out handle,
					ref (this as DynamicSoundEffectInstance).format,
					FAudio.FAUDIO_VOICE_USEFILTER,
					maxFreqRatio,
					IntPtr.Zero,
					IntPtr.Zero,
					IntPtr.Zero
				);
			}
			else
			{
				FAudio.FAudio_CreateSourceVoice(
					dev.Handle,
					out handle,
					parentEffect.formatPtr,
					FAudio.FAUDIO_VOICE_USEFILTER,
					maxFreqRatio,
					IntPtr.Zero,
					IntPtr.Zero,
					IntPtr.Zero
				);
			}
			if (handle == IntPtr.Zero)
			{
				return; /* What */
			}

			/* Apply current properties */
			FAudio.FAudioVoice_SetVolume(handle, INTERNAL_volume, 0);
			UpdatePitch();
			if (is3D || Pan != 0.0f)
			{
				FAudio.FAudioVoice_SetOutputMatrix(
					handle,
					SoundEffect.Device().MasterVoice,
					Channels,
					dspSettings.DstChannelCount,
					dspSettings.pMatrixCoefficients,
					0
				);
			}

			/* For static effects, submit the buffer now */
			if (isDynamic)
			{
				(this as DynamicSoundEffectInstance).QueueInitialBuffers();
			}
			else
			{
				if (IsLooped)
				{
					parentEffect.handle.LoopCount = 255;
					parentEffect.handle.LoopBegin = parentEffect.loopStart;
					parentEffect.handle.LoopLength = parentEffect.loopLength;
				}
				else
				{
					parentEffect.handle.LoopCount = 0;
					parentEffect.handle.LoopBegin = 0;
					parentEffect.handle.LoopLength = 0;
				}
				FAudio.FAudioSourceVoice_SubmitSourceBuffer(
					handle,
					ref parentEffect.handle,
					IntPtr.Zero
				);
			}

			/* Play, finally. */
			FAudio.FAudioSourceVoice_Start(handle, 0, 0);
			INTERNAL_state = SoundState.Playing;
			hasStarted = true;
		}

		public void Pause()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (handle != IntPtr.Zero && State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(handle, 0, 0);
				INTERNAL_state = SoundState.Paused;
			}
		}

		public void Resume()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			SoundState state = State; // Triggers a query, update
			if (handle == IntPtr.Zero)
			{
				// XNA4 just plays if we've not started yet.
				Play();
			}
			else if (state == SoundState.Paused)
			{
				FAudio.FAudioSourceVoice_Start(handle, 0, 0);
				INTERNAL_state = SoundState.Playing;
			}
		}

		public void Stop()
		{
			Stop(true);
		}

		public void Stop(bool immediate)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (handle == IntPtr.Zero)
			{
				return;
			}

			if (immediate)
			{
				FAudio.FAudioSourceVoice_Stop(handle, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(handle);
				FAudio.FAudioVoice_DestroyVoice(handle);
				handle = IntPtr.Zero;
				usingReverb = false;
				INTERNAL_state = SoundState.Stopped;

				if (isDynamic)
				{
					lock (FrameworkDispatcher.Streams)
					{
						FrameworkDispatcher.Streams.Remove(
							this as DynamicSoundEffectInstance
						);
					}
					(this as DynamicSoundEffectInstance).ClearBuffers();
				}
			}
			else
			{
				if (isDynamic)
				{
					throw new InvalidOperationException();
				}
				FAudio.FAudioSourceVoice_ExitLoop(handle, 0);
			}
		}

		#endregion

		#region Protected Methods

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Stop(true);
				if (parentEffect != null)
				{
					parentEffect.Instances.Remove(selfReference);
				}
				selfReference = null;
				FNAPlatform.Free(dspSettings.pMatrixCoefficients);
				IsDisposed = true;
			}
		}

		#endregion

		#region Internal Methods

		internal void InitDSPSettings()
		{
			dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
			dspSettings.DopplerFactor = 1.0f;
			dspSettings.SrcChannelCount = 1;
			dspSettings.DstChannelCount = SoundEffect.Device().DeviceDetails.OutputFormat.Format.nChannels;

			int memsize = (
				4 *
				(int) Channels *
				(int) dspSettings.DstChannelCount
			);
			dspSettings.pMatrixCoefficients = FNAPlatform.Malloc(memsize);
			unsafe
			{
				byte* memPtr = (byte*) dspSettings.pMatrixCoefficients;
				for (int i = 0; i < memsize; i += 1)
				{
					memPtr[i] = 0;
				}
			}
			SetPanMatrixCoefficients();
		}

		internal unsafe void INTERNAL_applyReverb(float rvGain)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			if (!usingReverb)
			{
				SoundEffect.Device().AttachReverb(handle);
				usingReverb = true;
			}

			// Re-using this float array...
			float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
			outputMatrix[0] = rvGain;
			if (Channels == 2)
			{
				outputMatrix[1] = rvGain;
			}
			FAudio.FAudioVoice_SetOutputMatrix(
				handle,
				SoundEffect.Device().ReverbVoice,
				Channels,
				1,
				dspSettings.pMatrixCoefficients,
				0
			);
		}

		internal void INTERNAL_applyLowPassFilter(float cutoff)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
			p.Type = FAudio.FAudioFilterType.FAudioLowPassFilter;
			p.Frequency = cutoff;
			p.OneOverQ = 1.0f;
			FAudio.FAudioVoice_SetFilterParameters(
				handle,
				ref p,
				0
			);
		}

		internal void INTERNAL_applyHighPassFilter(float cutoff)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
			p.Type = FAudio.FAudioFilterType.FAudioHighPassFilter;
			p.Frequency = cutoff;
			p.OneOverQ = 1.0f;
			FAudio.FAudioVoice_SetFilterParameters(
				handle,
				ref p,
				0
			);
		}

		internal void INTERNAL_applyBandPassFilter(float center)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
			p.Type = FAudio.FAudioFilterType.FAudioBandPassFilter;
			p.Frequency = center;
			p.OneOverQ = 1.0f;
			FAudio.FAudioVoice_SetFilterParameters(
				handle,
				ref p,
				0
			);
		}

		#endregion

		#region Private Methods

		private void UpdatePitch()
		{
			float ratio;
			if (!is3D || dspSettings.DopplerFactor == 0.0f)
			{
				ratio = (float) Math.Pow(2.0, INTERNAL_pitch);
			}
			else
			{
				ratio = dspSettings.DopplerFactor;
			}

			FAudio.FAudioSourceVoice_SetFrequencyRatio(
				handle,
				ratio,
				0
			);
		}

		private unsafe void SetPanMatrixCoefficients()
		{
			/* Two major things to notice:
			 * 1. The spec assumes any speaker count >= 2 has Front Left/Right.
			 * 2. Stereo panning is WAY more complicated than you think.
			 *    The main thing is that hard panning does NOT eliminate an
			 *    entire channel; the two channels are blended on each side.
			 * Aside from that, XNA is pretty naive about the output matrix.
			 * -flibit
			 */
			float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
			if (Channels == 1)
			{
				if (dspSettings.DstChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
				}
				else
				{
					outputMatrix[0] = (INTERNAL_pan > 0.0f) ? (1.0f - INTERNAL_pan) : 1.0f;
					outputMatrix[1] = (INTERNAL_pan < 0.0f) ? (1.0f  + INTERNAL_pan) : 1.0f;
				}
			}
			else
			{
				if (dspSettings.DstChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
					outputMatrix[1] = 1.0f;
				}
				else
				{
					if (INTERNAL_pan <= 0.0f)
					{
						// Left speaker blends left/right channels
						outputMatrix[0] = 0.5f * INTERNAL_pan + 1.0f;
						outputMatrix[1] = 0.5f * -INTERNAL_pan;
						// Right speaker gets less of the right channel
						outputMatrix[2] = 0.0f;
						outputMatrix[3] = INTERNAL_pan + 1.0f;
					}
					else
					{
						// Left speaker gets less of the left channel
						outputMatrix[0] = -INTERNAL_pan + 1.0f;
						outputMatrix[1] = 0.0f;
						// Right speaker blends right/left channels
						outputMatrix[2] = 0.5f * INTERNAL_pan;
						outputMatrix[3] = 0.5f * -INTERNAL_pan + 1.0f;
					}
				}
			}
		}

		#endregion
	}
}
