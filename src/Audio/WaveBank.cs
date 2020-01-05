#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.wavebank.aspx
	public class WaveBank : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsPrepared
		{
			get
			{
				uint state;
				FAudio.FACTWaveBank_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_PREPARED) != 0;
			}
		}

		public bool IsInUse
		{
			get
			{
				uint state;
				FAudio.FACTWaveBank_GetState(handle, out state);
				return (state & FAudio.FACT_STATE_INUSE) != 0;
			}
		}

		#endregion

		#region Private Variables

		private IntPtr handle;

		private AudioEngine engine;
		private WeakReference selfReference;

		// Non-streaming WaveBanks
		private byte[] buffer;
		private GCHandle pin;

		// Streaming WaveBanks
		private IntPtr ioStream;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructors

		public WaveBank(
			AudioEngine audioEngine,
			string nonStreamingWaveBankFilename
		) {
			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(nonStreamingWaveBankFilename))
			{
				throw new ArgumentNullException("nonStreamingWaveBankFilename");
			}

			buffer = TitleContainer.ReadAllBytes(
				nonStreamingWaveBankFilename
			);
			pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			FAudio.FACTAudioEngine_CreateInMemoryWaveBank(
				audioEngine.handle,
				pin.AddrOfPinnedObject(),
				(uint) buffer.Length,
				0,
				0,
				out handle
			);

			engine = audioEngine;
			selfReference = new WeakReference(this, true);
			engine.RegisterWaveBank(handle, selfReference);
			IsDisposed = false;
		}

		public WaveBank(
			AudioEngine audioEngine,
			string streamingWaveBankFilename,
			int offset,
			short packetsize
		) {
			if (audioEngine == null)
			{
				throw new ArgumentNullException("audioEngine");
			}
			if (String.IsNullOrEmpty(streamingWaveBankFilename))
			{
				throw new ArgumentNullException("streamingWaveBankFilename");
			}

			string safeName = MonoGame.Utilities.FileHelpers.NormalizeFilePathSeparators(
				streamingWaveBankFilename
			);
			if (!Path.IsPathRooted(safeName))
			{
				safeName = Path.Combine(
					TitleLocation.Path,
					safeName
				);
			}
			ioStream = FAudio.FAudio_fopen(safeName);

			FAudio.FACTStreamingParameters settings = new FAudio.FACTStreamingParameters();
			settings.file = ioStream;
			FAudio.FACTAudioEngine_CreateStreamingWaveBank(
				audioEngine.handle,
				ref settings,
				out handle
			);

			engine = audioEngine;
			selfReference = new WeakReference(this, true);
			engine.RegisterWaveBank(handle, selfReference);
			IsDisposed = false;
		}

		#endregion

		#region Destructor

		~WaveBank()
		{
			if (AudioEngine.ProgramExiting)
			{
				return;
			}

			if (!IsDisposed && IsInUse)
			{
				// STOP LEAKING YOUR BANKS, ARGH
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

		#region Protected Dispose Method

		protected virtual void Dispose(bool disposing)
		{
			lock (engine.gcSync)
			{
				if (!IsDisposed)
				{
					if (Disposing != null)
					{
						Disposing.Invoke(this, null);
					}

					// If this is disposed, stop leaking memory!
					if (!engine.IsDisposed)
					{
						engine.UnregisterWaveBank(handle);
						FAudio.FACTWaveBank_Destroy(handle);
					}
					OnWaveBankDestroyed();
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void OnWaveBankDestroyed()
		{
			IsDisposed = true;
			if (buffer != null)
			{
				pin.Free();
				buffer = null;
			}
			else if (ioStream != IntPtr.Zero)
			{
				// FACT frees this pointer!
				ioStream = IntPtr.Zero;
			}
			handle = IntPtr.Zero;
			selfReference = null;
		}

		#endregion
	}
}
