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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.cue.aspx
	public sealed class Cue : IDisposable
	{
		#region Public Properties

		public bool IsCreated
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_CREATED) != 0;
			}
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
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_PAUSED) != 0;
			}
		}

		public bool IsPlaying
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_PLAYING) != 0;
			}
		}

		public bool IsPrepared
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_PREPARED) != 0;
			}
		}

		public bool IsPreparing
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_PREPARING) != 0;
			}
		}

		public bool IsStopped
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_STOPPED) != 0;
			}
		}

		public bool IsStopping
		{
			get
			{
				uint state;
				FAudio.FACTCue_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_STOPPING) != 0;
			}
		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private IntPtr handle;
		private SoundBank bank;
		private WeakReference selfReference;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Internal Constructor

		internal Cue(IntPtr cue, string name, SoundBank soundBank)
		{
			handle = cue;
			Name = name;
			bank = soundBank;

			selfReference = new WeakReference(this, true);
			bank.engine.RegisterPointer(handle, selfReference);
		}

		#endregion

		#region Destructor

		~Cue()
		{
			if (AudioEngine.ProgramExiting)
			{
				return;
			}

			if (!IsDisposed && IsPlaying)
			{
				// STOP LEAKING YOUR CUES, ARGH
				GC.ReRegisterForFinalize(this);
				return;
			}
			Dispose(false);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Public Methods

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

			emitter.emitterData.ChannelCount = bank.dspSettings.SrcChannelCount;
			emitter.emitterData.CurveDistanceScaler = float.MaxValue;
			FAudio.FACT3DCalculate(
				bank.engine.handle3D,
				ref listener.listenerData,
				ref emitter.emitterData,
				ref bank.dspSettings
			);
			FAudio.FACT3DApply(ref bank.dspSettings, handle);
		}

		public float GetVariable(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort variable = FAudio.FACTCue_GetVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid variable name!"
				);
			}

			float result;
			FAudio.FACTCue_GetVariable(
				handle,
				variable,
				out result
			);
			return result;
		}

		public void Pause()
		{
			FAudio.FACTCue_Pause(handle, 1);
		}

		public void Play()
		{
			FAudio.FACTCue_Play(handle);
		}

		public void Resume()
		{
			FAudio.FACTCue_Pause(handle, 0);
		}

		public void SetVariable(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort variable = FAudio.FACTCue_GetVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid variable name!"
				);
			}

			FAudio.FACTCue_SetVariable(
				handle,
				variable,
				value
			);
		}

		public void Stop(AudioStopOptions options)
		{
			FAudio.FACTCue_Stop(
				handle,
				(options == AudioStopOptions.Immediate) ?
					FAudio.FACT_FLAG_STOP_IMMEDIATE :
					FAudio.FACT_FLAG_STOP_RELEASE
			);
		}

		#endregion

		#region Internal Methods

		internal void OnCueDestroyed()
		{
			IsDisposed = true;
			handle = IntPtr.Zero;
			selfReference = null;
		}

		#endregion

		#region Private Methods

		private void Dispose(bool disposing)
		{
			lock (bank.engine.gcSync)
			{
				if (!IsDisposed)
				{
					if (Disposing != null)
					{
						Disposing.Invoke(this, null);
					}

					// If this is Disposed, stop leaking memory!
					if (!bank.engine.IsDisposed)
					{
						FAudio.FACTCue_Destroy(handle);
					}
					OnCueDestroyed();
				}
			}
		}

		#endregion
	}
}
