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
				if (value > 1.0f || value < -1.0f)
				{
					throw new ArgumentOutOfRangeException("value");
				}
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
						SoundEffect.Device().MasterVoice,
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

		private SoundState INTERNAL_state = SoundState.Stopped;
		public SoundState State
		{
			get
			{
				if (	!isDynamic &&
					handle != IntPtr.Zero &&
					INTERNAL_state == SoundState.Playing	)
				{
					FAudio.FAudioVoiceState state;
					FAudio.FAudioSourceVoice_GetState(
						handle,
						out state,
						FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
					);
					if (state.BuffersQueued == 0)
					{
						Stop(true);
					}
				}
				return INTERNAL_state;
			}
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
		internal bool isDynamic;

		#endregion

		#region Private Variables

		private SoundEffect parentEffect;
		private WeakReference selfReference;
		private bool hasStarted;
		private bool is3D;
		private bool usingReverb;
		private FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

		#endregion

		#region memset Entry Point

		[DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr memset(IntPtr ptr, int value, IntPtr num);

		#endregion

		#region Internal Constructor

		internal SoundEffectInstance(SoundEffect parent = null)
		{
			SoundEffect.Device();

			selfReference = new WeakReference(this, true);
			parentEffect = parent;
			isDynamic = this is DynamicSoundEffectInstance;
			hasStarted = false;
			is3D = false;
			usingReverb = false;
			INTERNAL_state = SoundState.Stopped;

			if (!isDynamic)
			{
				InitDSPSettings(parentEffect.format.nChannels);
			}
			if (parentEffect != null)
			{
				parentEffect.Instances.Add(selfReference);
			}
		}

		#endregion

		#region Destructor

		~SoundEffectInstance()
		{
			if (!IsDisposed && State == SoundState.Playing)
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
					SoundEffect.Device().MasterVoice,
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
				IntPtr.Zero,
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
					SoundEffect.Device().MasterVoice,
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
			INTERNAL_state = SoundState.Playing;
			hasStarted = true;
		}

		public void Pause()
		{
			if (handle != IntPtr.Zero && State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(handle, 0, 0);
				INTERNAL_state = SoundState.Paused;
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
				INTERNAL_state = SoundState.Playing;
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
				usingReverb = false;
				INTERNAL_state = SoundState.Stopped;

				if (isDynamic)
				{
					FrameworkDispatcher.Streams.Remove(
						this as DynamicSoundEffectInstance
					);
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
				Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
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
			memset(
				dspSettings.pMatrixCoefficients,
				'\0',
				(IntPtr) (4 * dspSettings.SrcChannelCount * dspSettings.DstChannelCount)
			);
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
			if (dspSettings.SrcChannelCount == 2)
			{
				outputMatrix[1] = rvGain;
			}
			FAudio.FAudioVoice_SetOutputMatrix(
				handle,
				SoundEffect.Device().ReverbVoice,
				dspSettings.SrcChannelCount,
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
			p.Type = FAudio.FAudioFilterType.FAudioLowPassFilter;
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
			/* Two major things to notice:
			 * 1. The spec assumes any speaker count >= 2 has Front Left/Right.
			 * 2. Stereo panning is WAY more complicated than you think.
			 *    The main thing is that hard panning does NOT eliminate an
			 *    entire channel; the two channels are blended on each side.
			 * Aside from that, XNA is pretty naive about the output matrix.
			 * -flibit
			 */
			float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
			if (dspSettings.SrcChannelCount == 1)
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
