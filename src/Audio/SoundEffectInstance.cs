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
				INTERNAL_looped = value;
				if (INTERNAL_alSource != null)
				{
					AudioDevice.ALDevice.SetSourceLooped(
						INTERNAL_alSource,
						value
					);
				}
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
				if (INTERNAL_alSource != null)
				{
					AudioDevice.ALDevice.SetSourcePan(
						INTERNAL_alSource,
						value
					);
				}
			}
		}

		private float INTERNAL_pitch = 0f;
		public float Pitch
		{
			get
			{
				return INTERNAL_pitch;
			}
			set
			{
				if (INTERNAL_isXACTSource)
				{
					value = MathHelper.Clamp(value, -2.0f, 2.0f);
				}
				else
				{
					value = MathHelper.Clamp(value, -1.0f, 1.0f);
				}
				INTERNAL_pitch = value;
				if (INTERNAL_alSource != null)
				{
					AudioDevice.ALDevice.SetSourcePitch(
						INTERNAL_alSource,
						value,
						!INTERNAL_isXACTSource
					);
				}
			}
		}

		public SoundState State
		{
			get
			{
				if (INTERNAL_alSource == null)
				{
					return SoundState.Stopped;
				}
				SoundState result = AudioDevice.ALDevice.GetSourceState(
					INTERNAL_alSource
				);
				if (result == SoundState.Stopped && isDynamic)
				{
					// Force playing at all times for DSFI!
					return SoundState.Playing;
				}
				return result;
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
				if (!INTERNAL_isXACTSource)
				{
					value = MathHelper.Clamp(value, 0.0f, 1.0f);
				}
				INTERNAL_volume = value;
				if (INTERNAL_alSource != null)
				{
					AudioDevice.ALDevice.SetSourceVolume(
						INTERNAL_alSource,
						value
					);
				}
			}
		}

		#endregion

		#region Internal Variables: 3D Audio

		internal bool INTERNAL_positionalAudio = false;
		internal Vector3 position = new Vector3(0.0f, 0.0f, 0.1f);

		#endregion

		#region Internal Variables: XACT Filters

		internal byte FilterType;

		#endregion

		#region Private Variables: XNA Implementation

		private SoundEffect INTERNAL_parentEffect;
		private WeakReference selfReference;

		internal bool isDynamic;

		/* FNA' XACT runtime wraps around SoundEffect for audio output.
		 * Only problem: XACT pitch has no boundaries, SoundEffect does.
		 * So, we're going to use this to tell the pitch clamp to STFU.
		 * -flibit
		 */
		internal bool INTERNAL_isXACTSource = false;

		#endregion

		#region Private Variables: AL Source, EffectSlot

		internal IALSource INTERNAL_alSource;
		private IALReverb INTERNAL_alReverb;

		#endregion

		#region Internal Constructor

		internal SoundEffectInstance(SoundEffect parent)
		{
			INTERNAL_parentEffect = parent;
			if (INTERNAL_parentEffect != null)
			{
				selfReference = new WeakReference(this);
				INTERNAL_parentEffect.Instances.Add(selfReference);
			}
			isDynamic = false;
		}

		#endregion

		#region Destructor

		~SoundEffectInstance()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Method

		public virtual void Dispose()
		{
			if (!IsDisposed)
			{
				Stop(true);
				if (INTERNAL_parentEffect != null)
				{
					INTERNAL_parentEffect.Instances.Remove(selfReference);
					selfReference = null;
				}
				IsDisposed = true;
			}
		}

		#endregion

		#region Public 3D Audio Methods

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			// We positional now
			if (!INTERNAL_positionalAudio)
			{
				// Do we need to convert a stereo buffer to mono?
				if (	!isDynamic &&
					INTERNAL_parentEffect.INTERNAL_buffer.Channels == 2 &&
					INTERNAL_parentEffect.INTERNAL_monoBuffer == null	)
				{
					INTERNAL_parentEffect.INTERNAL_monoBuffer = AudioDevice.ALDevice.ConvertStereoToMono(
						INTERNAL_parentEffect.INTERNAL_buffer
					);
				}

				// K, we really positional now
				INTERNAL_positionalAudio = true;
			}

			// Set up our final position according to orientation of listener
			position = Vector3.Transform(
				emitter.Position - listener.Position,
				Matrix.CreateWorld(Vector3.Zero, listener.Forward, listener.Up)
			);

			// XACT doesn't do automated attenuation!
			if (INTERNAL_isXACTSource && position != Vector3.Zero)
			{
				position.Normalize();
			}

			// This can get called before Play()...
			if (INTERNAL_alSource != null)
			{
				// Finally.
				AudioDevice.ALDevice.SetSourcePosition(
					INTERNAL_alSource,
					position
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

		#endregion

		#region Public Playback Methods

		public virtual void Play()
		{
			if (State != SoundState.Stopped)
			{
				return;
			}

			if (INTERNAL_alSource != null)
			{
				// The sound has stopped, but hasn't cleaned up yet...
				AudioDevice.ALDevice.StopAndDisposeSource(INTERNAL_alSource);
				INTERNAL_alSource = null;
			}

			IALBuffer srcBuf;
			if (INTERNAL_positionalAudio && INTERNAL_parentEffect.INTERNAL_monoBuffer != null)
			{
				srcBuf = INTERNAL_parentEffect.INTERNAL_monoBuffer;
			}
			else
			{
				srcBuf = INTERNAL_parentEffect.INTERNAL_buffer;
			}
			INTERNAL_alSource = AudioDevice.ALDevice.GenSource(
				srcBuf,
				INTERNAL_isXACTSource
			);
			if (INTERNAL_alSource == null)
			{
				FNALoggerEXT.LogWarn("AL SOURCE WAS NOT AVAILABLE, SKIPPING.");
				return;
			}

			// Apply Pan/Position
			if (INTERNAL_positionalAudio)
			{
				AudioDevice.ALDevice.SetSourcePosition(
					INTERNAL_alSource,
					position
				);
			}
			else
			{
				Pan = Pan;
			}

			// Reassign Properties, in case the AL properties need to be applied.
			Volume = Volume;
			IsLooped = IsLooped;
			Pitch = Pitch;

			// Apply EFX
			if (INTERNAL_alReverb != null)
			{
				AudioDevice.ALDevice.SetSourceReverb(
					INTERNAL_alSource,
					INTERNAL_alReverb
				);
			}

			AudioDevice.ALDevice.PlaySource(INTERNAL_alSource);
		}

		public void Pause()
		{
			if (INTERNAL_alSource != null && State == SoundState.Playing)
			{
				AudioDevice.ALDevice.PauseSource(INTERNAL_alSource);
			}
		}

		public void Resume()
		{
			if (INTERNAL_alSource == null)
			{
				// XNA4 just plays if we've not started yet.
				Play();
			}
			else if (State == SoundState.Paused)
			{
				AudioDevice.ALDevice.ResumeSource(INTERNAL_alSource);
			}
		}

		public void Stop()
		{
			if (INTERNAL_alSource != null)
			{
				// TODO: GraphicsResource-like reference management -flibit
				if (AudioDevice.ALDevice != null)
				{
					AudioDevice.ALDevice.StopAndDisposeSource(INTERNAL_alSource);
					DynamicSoundEffectInstance dsfi = this as DynamicSoundEffectInstance;
					if (dsfi != null && AudioDevice.DynamicInstancePool.Contains(dsfi))
					{
						AudioDevice.DynamicInstancePool.Remove(dsfi);
					}
				}
				INTERNAL_alSource = null;
			}
		}

		public void Stop(bool immediate)
		{
			Stop();
		}

		#endregion

		#region Internal Effects Methods

		internal void INTERNAL_applyReverb(IALReverb reverb)
		{
			INTERNAL_alReverb = reverb;
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.SetSourceReverb(
					INTERNAL_alSource,
					INTERNAL_alReverb
				);
			}
		}

		internal void INTERNAL_applyLowPassFilter(float hfGain)
		{
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.SetSourceLowPassFilter(INTERNAL_alSource, hfGain);
			}
		}

		internal void INTERNAL_applyHighPassFilter(float lfGain)
		{
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.SetSourceHighPassFilter(INTERNAL_alSource, lfGain);
			}
		}

		internal void INTERNAL_applyBandPassFilter(float hfGain, float lfGain)
		{
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.SetSourceBandPassFilter(INTERNAL_alSource, hfGain, lfGain);
			}
		}

		#endregion
	}
}
