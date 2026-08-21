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
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.dynamicsoundeffectinstance.aspx
	public sealed class DynamicSoundEffectInstance : SoundEffectInstance
	{
		#region Public Properties

		public int PendingBufferCount
		{
			get
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				return queuedBuffers.Count;
			}
		}

		public override bool IsLooped
		{
			get
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				return false;
			}
			set
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
				}
				// DynamicSoundEffectInstance cannot be looped!
				if (value)
				{
					throw new InvalidOperationException("The method call is invalid.");
				}
			}
		}

		#endregion

		#region Internal Variables

		internal FAudio.FAudioWaveFormatEx format;

		#endregion

		#region Private Variables

		private List<IntPtr> queuedBuffers;
		private List<uint> queuedSizes;

		#endregion

		#region Private Constants

		private const int MINIMUM_BUFFER_CHECK = 3;

		#endregion

		#region BufferNeeded Event

		public event EventHandler<EventArgs> BufferNeeded;

		#endregion

		#region Public Constructor

		public DynamicSoundEffectInstance(
			int sampleRate,
			AudioChannels channels
		) : base(null) {
			if (sampleRate < FAudio.FAUDIO_MIN_SAMPLE_RATE || sampleRate > FAudio.FAUDIO_MAX_SAMPLE_RATE) // XNA: sampleRate < 8000 || sampleRate > 48000
			{
				throw new ArgumentOutOfRangeException("sampleRate");
			}
			if (channels < AudioChannels.Mono || channels > AudioChannels.Stereo)
			{
				throw new ArgumentOutOfRangeException("channels");
			}
			FAudio.FAudio_AddRef(SoundEffect.Device().Handle);

			format.wFormatTag = 1;
			format.nChannels = (ushort) channels;
			format.nSamplesPerSec = (uint) sampleRate;
			format.wBitsPerSample = 16;
			format.nBlockAlign = (ushort) (2 * format.nChannels);
			format.nAvgBytesPerSec = format.nBlockAlign * format.nSamplesPerSec;
			format.cbSize = 0;

			queuedBuffers = new List<IntPtr>();
			queuedSizes = new List<uint>();

			InitDSPSettings(format.nChannels);
		}

		#endregion

		#region Public Methods

		public TimeSpan GetSampleDuration(int sizeInBytes)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (sizeInBytes < 0)
			{
				throw new ArgumentException("Buffer size cannot be negative.");
			}
			return SoundEffect.INTERNAL_GetSampleDuration(
				sizeInBytes,
				(int) format.nSamplesPerSec,
				format.nBlockAlign
			);
		}

		public int GetSampleSizeInBytes(TimeSpan duration)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (duration.TotalMilliseconds < 0.0 || duration.TotalMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("duration");
			}
			return SoundEffect.INTERNAL_GetSampleSizeInBytes(
				duration,
				(int) format.nSamplesPerSec,
				format.nBlockAlign
			);
		}

		public override void Play()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			// Wait! What if we need moar buffers?
			Update();

			// Okay we're good
			base.Play();
			lock (FrameworkDispatcher.Streams)
			{
				if (!FrameworkDispatcher.Streams.Contains(this))
				{
					FrameworkDispatcher.Streams.Add(this);
				}
			}
		}

		public void SubmitBuffer(byte[] buffer)
		{
			this.SubmitBuffer(buffer, 0, buffer.Length);
		}

		public void SubmitBuffer(byte[] buffer, int offset, int count)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "This object has already been disposed.");
			}
			if (buffer == null || buffer.Length == 0 || buffer.Length % format.nBlockAlign != 0)
			{
				throw new ArgumentException("Buffer is invalid. Ensure that the buffer length is non-zero and meets the block alignment requirements for the audio format.");
			}
			if (unchecked((uint) offset >= (uint) buffer.Length) || offset % format.nBlockAlign != 0)
			{
				throw new ArgumentException("Byte offset is invalid. Ensure that it falls within the buffer and meets the block alignment requirements for the audio format.");
			}
			if (count <= 0 || unchecked((uint) (offset + count) > (uint) buffer.Length) || count % format.nBlockAlign != 0)
			{
				throw new ArgumentException("Number of samples to play is invalid. Ensure that it meets the block alignment requirements for the audio format.");
			}
			IntPtr next = FNAPlatform.Malloc(count);
			Marshal.Copy(buffer, offset, next, count);
			lock (queuedBuffers)
			{
				queuedBuffers.Add(next);
				if (State != SoundState.Stopped)
				{
					FAudio.FAudioBuffer buf = new FAudio.FAudioBuffer();
					buf.AudioBytes = (uint) count;
					buf.pAudioData = next;
					buf.PlayLength = buf.AudioBytes / format.nBlockAlign;
					FAudio.FAudioSourceVoice_SubmitSourceBuffer(
						handle,
						ref buf,
						IntPtr.Zero
					);
				}
				else
				{
					queuedSizes.Add((uint) count);
				}
			}
		}

		public void SubmitFloatBufferEXT(float[] buffer)
		{
			SubmitFloatBufferEXT(buffer, 0, buffer.Length);
		}

		public void SubmitFloatBufferEXT(float[] buffer, int offset, int count)
		{
			/* Float samples are the typical format received from decoders.
			 * We currently use this for the VideoPlayer.
			 * -flibit
			 */
			if (State != SoundState.Stopped && format.wFormatTag == 1)
			{
				throw new InvalidOperationException(
					"Submit a float buffer before Playing!"
				);
			}
			format.wFormatTag = 3;
			format.wBitsPerSample = 32;
			format.nBlockAlign = (ushort) (4 * format.nChannels);
			format.nAvgBytesPerSec = format.nBlockAlign * format.nSamplesPerSec;

			IntPtr next = FNAPlatform.Malloc(count * sizeof(float));
			Marshal.Copy(buffer, offset, next, count);
			lock (queuedBuffers)
			{
				queuedBuffers.Add(next);
				if (State != SoundState.Stopped)
				{
					FAudio.FAudioBuffer buf = new FAudio.FAudioBuffer();
					buf.AudioBytes = (uint) count * sizeof(float);
					buf.pAudioData = next;
					buf.PlayLength = buf.AudioBytes / format.nBlockAlign;
					FAudio.FAudioSourceVoice_SubmitSourceBuffer(
						handle,
						ref buf,
						IntPtr.Zero
					);
				}
				else
				{
					queuedSizes.Add((uint) count * sizeof(float));
				}
			}
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			bool needsRelease = !IsDisposed;

			base.Dispose(disposing);

			if (needsRelease)
			{
				FAudio.FAudio_Release(SoundEffect.Device().Handle);
			}
		}

		#endregion

		#region Internal Methods

		internal void QueueInitialBuffers()
		{
			FAudio.FAudioBuffer buffer = new FAudio.FAudioBuffer();
			lock (queuedBuffers)
			{
				for (int i = 0; i < queuedBuffers.Count; i += 1)
				{
					buffer.AudioBytes = queuedSizes[i];
					buffer.pAudioData = queuedBuffers[i];
					buffer.PlayLength = buffer.AudioBytes / format.nBlockAlign;
					FAudio.FAudioSourceVoice_SubmitSourceBuffer(
						handle,
						ref buffer,
						IntPtr.Zero
					);
				}
				queuedSizes.Clear();
			}
		}

		internal void ClearBuffers()
		{
			lock (queuedBuffers)
			{
				foreach (IntPtr buf in queuedBuffers)
				{
					FNAPlatform.Free(buf);
				}
				queuedBuffers.Clear();
				queuedSizes.Clear();
			}
		}

		internal void Update()
		{
			if (State != SoundState.Playing)
			{
				// Shh, we don't need you right now...
				return;
			}

			if (handle != IntPtr.Zero)
			{
				FAudio.FAudioVoiceState state;
				FAudio.FAudioSourceVoice_GetState(
					handle,
					out state,
					FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
				);
				while (PendingBufferCount > state.BuffersQueued)
				lock (queuedBuffers)
				{
					FNAPlatform.Free(queuedBuffers[0]);
					queuedBuffers.RemoveAt(0);
				}
			}

			// Do we need even moar buffers?
			for (
				int i = MINIMUM_BUFFER_CHECK - PendingBufferCount;
				(i > 0) && BufferNeeded != null;
				i -= 1
			) {
				BufferNeeded(this, null);
			}
		}

		#endregion
	}
}
