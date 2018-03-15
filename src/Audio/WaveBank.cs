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
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Utilities;
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

#pragma warning disable 0414
		// We just hold this to make the GC do the right thing
		private AudioEngine engine;
#pragma warning restore 0414

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

			string safeName = FileHelpers.NormalizeFilePathSeparators(
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
			IsDisposed = false;
		}

		#endregion

		#region Destructor

		~WaveBank()
		{
			Dispose(true);
		}

		#endregion

		#region Public Dispose Method

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

		#region Protected Dispose Method

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (Disposing != null)
				{
					Disposing.Invoke(this, null);
				}

				if (!engine.IsDisposed) // Just FYI, this is really bad
				{
					FAudio.FACTWaveBank_Destroy(handle);
				}

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
				engine = null;

				IsDisposed = true;
			}
		}

		#endregion
	}
}
