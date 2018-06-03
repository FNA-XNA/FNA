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
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundbank.aspx
	public class SoundBank : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsInUse
		{
			get
			{
				uint state;
				FAudio.FACTSoundBank_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_INUSE) != 0;
			}
		}

		#endregion

		#region Internal Variables

		internal AudioEngine engine;
		internal FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

		#endregion

		#region Private Variables

		private IntPtr handle;
		private WeakReference selfReference;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructor

		public SoundBank(AudioEngine audioEngine, string filename)
		{
			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}

			byte[] buffer = TitleContainer.ReadAllBytes(filename);
			GCHandle pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			FAudio.FACTAudioEngine_CreateSoundBank(
				audioEngine.handle,
				pin.AddrOfPinnedObject(),
				(uint) buffer.Length,
				0,
				0,
				out handle
			);

			pin.Free();
			buffer = null;

			engine = audioEngine;
			selfReference = new WeakReference(this);
			dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
			dspSettings.SrcChannelCount = 1;
			dspSettings.DstChannelCount = engine.channels;
			dspSettings.pMatrixCoefficients = Marshal.AllocHGlobal(
				4 *
				(int) dspSettings.SrcChannelCount *
				(int) dspSettings.DstChannelCount
			);
			engine.RegisterSoundBank(handle, selfReference);
			IsDisposed = false;
		}

		#endregion

		#region Destructor

		~SoundBank()
		{
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

		#region Protected Dispose Method

		protected void Dispose(bool disposing)
		{
			lock (engine.gcSync)
			{
				if (!IsDisposed && disposing)
				{
					if (Disposing != null)
					{
						Disposing.Invoke(this, null);
					}

					if (engine.IsDisposed)
					{
						// If you got here, stop leaking memory!
						IsDisposed = true;
					}
					else
					{
						FAudio.FACTSoundBank_Destroy(handle);
						Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
					}
				}
			}
		}

		#endregion

		#region Public Methods

		public Cue GetCue(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort cue = FAudio.FACTSoundBank_GetCueIndex(
				handle,
				name
			);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid cue name!"
				);
			}

			IntPtr result;
			FAudio.FACTSoundBank_Prepare(
				handle,
				cue,
				0,
				0,
				out result
			);
			return new Cue(result, name, this);
		}

		public void PlayCue(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort cue = FAudio.FACTSoundBank_GetCueIndex(
				handle,
				name
			);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid cue name!"
				);
			}

			FAudio.FACTSoundBank_Play(
				handle,
				cue,
				0,
				0,
				IntPtr.Zero
			);
		}

		public void PlayCue(
			string name,
			AudioListener listener,
			AudioEmitter emitter
		) {
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			if (emitter == null)
			{
				throw new ArgumentNullException("emitter");
			}

			ushort cue = FAudio.FACTSoundBank_GetCueIndex(
				handle,
				name
			);

			if (cue == FAudio.FACTINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid cue name!"
				);
			}

			emitter.emitterData.ChannelCount = dspSettings.SrcChannelCount;
			emitter.emitterData.CurveDistanceScaler = float.MaxValue;
			FAudio.FACT3DCalculate(
				engine.handle3D,
				ref listener.listenerData,
				ref emitter.emitterData,
				ref dspSettings
			);
			FAudio.FACTSoundBank_Play3D(
				handle,
				cue,
				0,
				0,
				ref dspSettings,
				IntPtr.Zero
			);
		}

		#endregion

		#region Internal Methods

		internal void OnSoundBankDestroyed()
		{
			IsDisposed = true;
			selfReference = null;
		}

		#endregion
	}
}
