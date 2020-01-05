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
				if (	value.Milliseconds < 100 ||
					value.Milliseconds > 1000 ||
					value.Milliseconds % 10 != 0	)
				{
					throw new ArgumentOutOfRangeException();
				}
				bufferDuration = value;
			}
		}

		public bool IsHeadset
		{
			get
			{
				// FIXME: I think this is just for Windows Phone? -flibit
				return false;
			}
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
		private uint handle;

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

		internal Microphone(uint id, string name)
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
			if (buffer == null)
			{
				throw new ArgumentException("buffer is null!");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentException("offset");
			}
			if (count <= 0 || (offset + count) > buffer.Length)
			{
				throw new ArgumentException("count");
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
			return SoundEffect.GetSampleDuration(
				sizeInBytes,
				SampleRate,
				AudioChannels.Mono
			);
		}

		public int GetSampleSizeInBytes(TimeSpan duration)
		{
			return SoundEffect.GetSampleSizeInBytes(
				duration,
				SampleRate,
				AudioChannels.Mono
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
