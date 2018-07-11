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

		internal readonly List<WeakReference> cueList;

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
			dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
			dspSettings.SrcChannelCount = 1;
			dspSettings.DstChannelCount = engine.channels;
			dspSettings.pMatrixCoefficients = Marshal.AllocHGlobal(
				4 *
				(int) dspSettings.SrcChannelCount *
				(int) dspSettings.DstChannelCount
			);
			IsDisposed = false;

			/* We have to manage our XACT resources, lest we get the GC and the
			 * API thread fighting with one another... hoo boy.
			 */
			cueList = new List<WeakReference>();
			selfReference = new WeakReference(this);
			engine.sbList.Add(selfReference);
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

					while (cueList.Count > 0)
					{
						if (cueList[0].Target != null)
						{
							((Cue) cueList[0].Target).Dispose();
						}
						else
						{
							cueList.RemoveAt(0);
						}
					}

					// If this is disposed, stop leaking memory!
					if (!engine.IsDisposed)
					{
						engine.sbList.Remove(selfReference);
						FAudio.FACTSoundBank_Destroy(handle);
						Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
					}
					IsDisposed = true;
					handle = IntPtr.Zero;
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
	}
}
