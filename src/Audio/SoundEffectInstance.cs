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
using System.Runtime.InteropServices;
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
				if (hasStarted)
				{
					throw new InvalidOperationException();
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
				INTERNAL_pan = value;
				if (is3D)
				{
					return;
				}

				SetPanMatrixCoefficients();
				if (handle != IntPtr.Zero)
				{
					FAudio.FAudioVoice_SetOutputMatrix(
						handle,
						IntPtr.Zero,
						dspSettings.SrcChannelCount,
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
				INTERNAL_pitch = MathHelper.Clamp(value, -1.0f, 1.0f);
				if (handle != IntPtr.Zero)
				{
					UpdatePitch();
				}
			}
		}

		public SoundState State
		{
			get;
			private set;
		}

		private float INTERNAL_volume = 1.0f;
		public float Volume
		{
			get
			{
				return INTERNAL_volume;
			}
			set
			{
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

		#region Internal Variables

		internal IntPtr handle;
		internal IntPtr callbacks;
		internal bool isDynamic;

		#endregion

		#region Private Variables

		private SoundEffect parentEffect;
		private WeakReference selfReference;
		private bool hasStarted;
		private bool is3D;
		private FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;
		private FAudio.OnStreamEndFunc OnStreamEndFunc;

		#endregion

		#region Internal Constructor

		internal SoundEffectInstance(
			SoundEffect parent = null,
			bool fireAndForget = false
		) {
			SoundEffect.Device();

			parentEffect = parent;
			if (parentEffect != null)
			{
				if (fireAndForget)
				{
					selfReference = null;
					parentEffect.FireAndForgetInstances.Add(this);
				}
				else
				{
					selfReference = new WeakReference(this);
					parentEffect.Instances.Add(selfReference);
				}
			}
			isDynamic = this is DynamicSoundEffectInstance;
			hasStarted = false;
			is3D = false;
			OnStreamEndFunc = OnStreamEnd;
			unsafe
			{
				callbacks = Marshal.AllocHGlobal(
					sizeof(FAudio.FAudioVoiceCallback)
				);
				FAudio.FAudioVoiceCallback* cb = (FAudio.FAudioVoiceCallback*) callbacks;
				cb->OnBufferEnd = IntPtr.Zero;
				cb->OnBufferStart = IntPtr.Zero;
				cb->OnLoopEnd = IntPtr.Zero;
				cb->OnStreamEnd = Marshal.GetFunctionPointerForDelegate(OnStreamEndFunc);
				cb->OnVoiceError = IntPtr.Zero;
				cb->OnVoiceProcessingPassEnd = IntPtr.Zero;
				cb->OnVoiceProcessingPassStart = IntPtr.Zero;
			}
			State = SoundState.Stopped;

			if (!isDynamic)
			{
				InitDSPSettings(parentEffect.format.nChannels);
			}
		}

		#endregion

		#region Destructor

		~SoundEffectInstance()
		{
			Dispose();
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			Dispose(true);
		}

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			if (emitter == null)
			{
				throw new ArgumentNullException("emitter");
			}

			is3D = true;
			SoundEffect.FAudioContext dev = SoundEffect.Device();
			emitter.emitterData.CurveDistanceScaler = dev.CurveDistanceScaler;
			emitter.emitterData.ChannelCount = dspSettings.SrcChannelCount;
			FAudio.F3DAudioCalculate(
				dev.Handle3D,
				ref listener.listenerData,
				ref emitter.emitterData,
				(
					FAudio.F3DAUDIO_CALCULATE_MATRIX |
					FAudio.F3DAUDIO_CALCULATE_DOPPLER
				),
				ref dspSettings
			);
			if (handle != IntPtr.Zero)
			{
				UpdatePitch();
				FAudio.FAudioVoice_SetOutputMatrix(
					handle,
					IntPtr.Zero,
					dspSettings.SrcChannelCount,
					dspSettings.DstChannelCount,
					dspSettings.pMatrixCoefficients,
					0
				);
			}
		}

		public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
		{
			if (listeners == null)
			{
				throw new ArgumentNullException("listeners");
			}
			if (listeners.Length == 1)
			{
				Apply3D(listeners[0], emitter);
				return;
			}
			throw new NotSupportedException("Only one listener is supported.");
		}

		public virtual void Play()
		{
			if (State != SoundState.Stopped)
			{
				return;
			}

			SoundEffect.FAudioContext dev = SoundEffect.Device();

			/* Create handle */
			FAudio.FAudioWaveFormatEx fmt = isDynamic ?
				(this as DynamicSoundEffectInstance).format :
				parentEffect.format;
			FAudio.FAudio_CreateSourceVoice(
				dev.Handle,
				out handle,
				ref fmt,
				FAudio.FAUDIO_VOICE_USEFILTER,
				FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
				callbacks,
				IntPtr.Zero,
				IntPtr.Zero
			);
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
					IntPtr.Zero,
					dspSettings.SrcChannelCount,
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
			State = SoundState.Playing;
			hasStarted = true;
		}

		public void Pause()
		{
			if (handle != IntPtr.Zero && State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(handle, 0, 0);
				State = SoundState.Paused;
			}
		}

		public void Resume()
		{
			if (handle == IntPtr.Zero)
			{
				// XNA4 just plays if we've not started yet.
				Play();
			}
			else if (State == SoundState.Paused)
			{
				FAudio.FAudioSourceVoice_Start(handle, 0, 0);
				State = SoundState.Playing;
			}
		}

		public void Stop()
		{
			Stop(true);
		}

		public void Stop(bool immediate)
		{
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
				State = SoundState.Stopped;

				if (isDynamic)
				{
					FrameworkDispatcher.Streams.Remove(
						this as DynamicSoundEffectInstance
					);
					(this as DynamicSoundEffectInstance).ClearBuffers();
				}
				if (parentEffect != null && selfReference == null)
				{
					Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
					Marshal.FreeHGlobal(callbacks);
					IsDisposed = true;
					parentEffect.FireAndForgetInstances.Remove(this);
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
				if (parentEffect != null && selfReference != null)
				{
					parentEffect.Instances.Remove(selfReference);
					selfReference = null;
				}
				Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
				Marshal.FreeHGlobal(callbacks);
				IsDisposed = true;
			}
		}

		#endregion

		#region Internal Methods

		internal void InitDSPSettings(uint srcChannels)
		{
			dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
			dspSettings.DopplerFactor = 1.0f;
			dspSettings.SrcChannelCount = srcChannels;
			dspSettings.DstChannelCount = SoundEffect.Device().DeviceDetails.OutputFormat.Format.nChannels;
			dspSettings.pMatrixCoefficients = Marshal.AllocHGlobal(
				4 *
				(int) dspSettings.SrcChannelCount *
				(int) dspSettings.DstChannelCount
			);
		}

		internal void INTERNAL_applyReverb(float rvGain)
		{
			// TODO
		}

		internal void INTERNAL_applyLowPassFilter(float cutoff)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
			p.Type = FAudio.FAudioFilterType.LowPassFilter;
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
			p.Type = FAudio.FAudioFilterType.HighPassFilter;
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
			p.Type = FAudio.FAudioFilterType.LowPassFilter;
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

		private void OnStreamEnd(IntPtr callback)
		{
			FAudio.FAudioSourceVoice_Stop(handle, 0, 0);
			State = SoundState.Stopped;
			FrameworkDispatcher.DeadSounds.Enqueue(this);
		}

		private void UpdatePitch()
		{
			float doppler;
			float dopplerScale = SoundEffect.Device().DopplerScale;
			if (!is3D || dopplerScale == 0.0f)
			{
				doppler = 1.0f;
			}
			else
			{
				doppler = dspSettings.DopplerFactor * dopplerScale;
			}

			FAudio.FAudioSourceVoice_SetFrequencyRatio(
				handle,
				(float) Math.Pow(2.0, INTERNAL_pitch) * doppler,
				0
			);
		}

		private unsafe void SetPanMatrixCoefficients()
		{
			float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
			FAudio.FAudioWaveFormatEx fmt = isDynamic ?
				(this as DynamicSoundEffectInstance).format :
				parentEffect.format;

			float left = (INTERNAL_pan > 0.0f) ? (1.0f - INTERNAL_pan) : 1.0f;
			float right = (INTERNAL_pan < 0.0f) ? (1.0f  + INTERNAL_pan) : 1.0f;

			uint dwChannelMask = SoundEffect.Device().DeviceDetails.OutputFormat.dwChannelMask;
			if (fmt.nChannels == 1)
			{
				if (dwChannelMask == FAudio.SPEAKER_MONO)
				{
					outputMatrix[0] = 1.0f;
					return;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_LEFT) != 0)
				{
					*outputMatrix++ = left;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_RIGHT) != 0)
				{
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_CENTER) != 0)
				{
					outputMatrix++;
				}
				if ((dwChannelMask & FAudio.SPEAKER_LOW_FREQUENCY) != 0)
				{
					outputMatrix++;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_LEFT) != 0)
				{
					*outputMatrix++ = left;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_RIGHT) != 0)
				{
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_LEFT_OF_CENTER) != 0)
				{
					*outputMatrix++ = left;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_RIGHT_OF_CENTER) != 0)
				{
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_CENTER) != 0)
				{
					outputMatrix++;
				}
				if ((dwChannelMask & FAudio.SPEAKER_SIDE_LEFT) != 0)
				{
					*outputMatrix++ = left;
				}
				if ((dwChannelMask & FAudio.SPEAKER_SIDE_RIGHT) != 0)
				{
					*outputMatrix++ = right;
				}
			}
			else
			{
				if (dwChannelMask == FAudio.SPEAKER_MONO)
				{
					outputMatrix[0] = 1.0f;
					outputMatrix[1] = 1.0f;
					return;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_LEFT) != 0)
				{
					*outputMatrix++ = left;
					*outputMatrix++ = 0.0f;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_RIGHT) != 0)
				{
					*outputMatrix++ = 0.0f;
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_CENTER) != 0)
				{
					outputMatrix += 2;
				}
				if ((dwChannelMask & FAudio.SPEAKER_LOW_FREQUENCY) != 0)
				{
					outputMatrix += 2;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_LEFT) != 0)
				{
					*outputMatrix++ = left;
					*outputMatrix++ = 0.0f;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_RIGHT) != 0)
				{
					*outputMatrix++ = 0.0f;
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_LEFT_OF_CENTER) != 0)
				{
					*outputMatrix++ = left;
					*outputMatrix++ = 0.0f;
				}
				if ((dwChannelMask & FAudio.SPEAKER_FRONT_RIGHT_OF_CENTER) != 0)
				{
					*outputMatrix++ = 0.0f;
					*outputMatrix++ = right;
				}
				if ((dwChannelMask & FAudio.SPEAKER_BACK_CENTER) != 0)
				{
					outputMatrix += 2;
				}
				if ((dwChannelMask & FAudio.SPEAKER_SIDE_LEFT) != 0)
				{
					*outputMatrix++ = left;
					*outputMatrix++ = 0.0f;
				}
				if ((dwChannelMask & FAudio.SPEAKER_SIDE_RIGHT) != 0)
				{
					*outputMatrix++ = 0.0f;
					*outputMatrix++ = right;
				}
			}
		}

		#endregion
	}
}
