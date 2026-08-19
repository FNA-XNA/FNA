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
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	public class Microphone
	{
		#region Public Static Properties

		public static ReadOnlyCollection<Microphone> All
		{
			get
			{
				if (micList == null)
				{
					micList = new ReadOnlyCollection<Microphone>(
						FNAPlatform.GetMicrophones()
					);
				}
				return micList;
			}
		}

		public static Microphone Default
		{
			get
			{
				if (All.Count == 0)
				{
					return null;
				}
				return All[0];
			}
		}

		#endregion

		#region Public Properties

		public TimeSpan BufferDuration
		{
			get
			{
				return bufferDuration;
			}
			set
			{
				if (	value.TotalMilliseconds < 100 ||
					value.TotalMilliseconds > 1000 ||
					value.TotalMilliseconds % 10 != 0	)
				{
					throw new ArgumentOutOfRangeException("value", "Microphone buffer duration must be between 100ms and 1sec and  10ms aligned.");
				}
				bufferDuration = value;
			}
		}

		public bool IsHeadset
		{
			get { return true; }
		}

		public int SampleRate
		{
			get
			{
				return SAMPLERATE;
			}
		}

		public MicrophoneState State
		{
			get;
			private set;
		}

		#endregion

		#region Public Variables

		public readonly string Name;

		#endregion

		#region Private Variables

		private TimeSpan bufferDuration;
		private readonly IntPtr handle;

		#endregion

		#region Internal Static Variables

		internal static ReadOnlyCollection<Microphone> micList;

		#endregion

		#region Events

		public event EventHandler<EventArgs> BufferReady;

		#endregion

		#region Internal Constants

		/* FIXME: This is what XNA4 aims for, but it _could_ be lower.
		 * Something worth looking at is falling back to lower sample
		 * rates in powers of two, i.e. 44100, 22050, 11025, etc.
		 * -flibit
		 */
		internal const int SAMPLERATE = 44100;

		#endregion

		#region Internal Constructor

		internal Microphone(IntPtr id, string name)
		{
			handle = id;
			Name = name;
			bufferDuration = TimeSpan.FromSeconds(1.0);
			State = MicrophoneState.Stopped;
		}

		#endregion

		#region Public Methods

		public int GetData(byte[] buffer)
		{
			return GetData(buffer, 0, buffer.Length);
		}

		public int GetData(byte[] buffer, int offset, int count)
		{
			// SDL_AUDIO_BYTESIZE(SDL_AUDIO_S16) = 2
			if (buffer == null || buffer.Length == 0 || buffer.Length % 2 != 0)
			{
				throw new ArgumentException("Buffer is invalid. Ensure that the buffer length is non-zero and meets the block alignment requirements for the audio format.");
			}
			if (unchecked((uint) offset >= (uint) buffer.Length) || offset % 2 != 0)
			{
				throw new ArgumentException("Byte offset is invalid. Ensure that it falls within the buffer and meets the block alignment requirements for the audio format.");
			}
			if (count <= 0 || unchecked((uint) (offset + count) > (uint) buffer.Length) || count % 2 != 0)
			{
				throw new ArgumentException("Number of samples to play is invalid. Ensure that it meets the block alignment requirements for the audio format.");
			}
			return FNAPlatform.GetMicrophoneSamples(
				handle,
				buffer,
				offset,
				count
			);
		}

		public TimeSpan GetSampleDuration(int sizeInBytes)
		{
			if (sizeInBytes < 0)
			{
				throw new ArgumentException("Buffer size cannot be negative.");
			}
			return SoundEffect.INTERNAL_GetSampleDuration(
				sizeInBytes,
				SampleRate,
				2 // 16-bit PCM!
			);
		}

		public int GetSampleSizeInBytes(TimeSpan duration)
		{
			if (duration.TotalMilliseconds < 0.0 || duration.TotalMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("duration");
			}
			return SoundEffect.INTERNAL_GetSampleSizeInBytes(
				duration,
				SampleRate,
				2 // 16-bit PCM!
			);
		}

		public void Start()
		{
			FNAPlatform.StartMicrophone(handle);
			State = MicrophoneState.Started;
		}

		public void Stop()
		{
			FNAPlatform.StopMicrophone(handle);
			State = MicrophoneState.Stopped;
		}

		#endregion

		#region Internal Methods

		internal void CheckBuffer()
		{
			if (	BufferReady != null &&
				GetSampleDuration(FNAPlatform.GetMicrophoneQueuedBytes(handle)) > bufferDuration	)
			{
				BufferReady(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
