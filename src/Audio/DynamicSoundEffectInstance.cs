#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.dynamicsoundeffectinstance.aspx
	public sealed class DynamicSoundEffectInstance : SoundEffectInstance
	{
		#region Public Properties

		public int PendingBufferCount
		{
			get;
			private set;
		}

		public override bool IsLooped
		{
			get
			{
				return false;
			}
			set
			{
				// No-op, DynamicSoundEffectInstance cannot be looped!
			}
		}

		#endregion

		#region Private XNA Variables

		private int sampleRate;
		private AudioChannels channels;

		private const int MINIMUM_BUFFER_CHECK = 3;

		#endregion

		#region Private AL Variables

		private Queue<IALBuffer> queuedBuffers;
		private Queue<IALBuffer> buffersToQueue;
		private Queue<IALBuffer> availableBuffers;

		#endregion

		#region BufferNeeded Event

		public event EventHandler<EventArgs> BufferNeeded;

		#endregion

		#region Public Constructor

		public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels) : base(null)
		{
			this.sampleRate = sampleRate;
			this.channels = channels;

			PendingBufferCount = 0;

			isDynamic = true;
			queuedBuffers = new Queue<IALBuffer>();
			buffersToQueue = new Queue<IALBuffer>();
			availableBuffers = new Queue<IALBuffer>();
		}

		#endregion

		#region Destructor

		~DynamicSoundEffectInstance()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Method

		public override void Dispose()
		{
			if (!IsDisposed)
			{
				base.Dispose(); // Will call Stop(true);

				// Delete all known buffer objects
				while (queuedBuffers.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(queuedBuffers.Dequeue());
				}
				queuedBuffers = null;
				while (availableBuffers.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(availableBuffers.Dequeue());
				}
				availableBuffers = null;
				while (buffersToQueue.Count > 0)
				{
					AudioDevice.ALDevice.DeleteBuffer(buffersToQueue.Dequeue());
				}
				buffersToQueue = null;

				IsDisposed = true;
			}
		}

		#endregion

		#region Public Time/Sample Information Methods

		public TimeSpan GetSampleDuration(int sizeInBytes)
		{
			return SoundEffect.GetSampleDuration(
				sizeInBytes,
				sampleRate,
				channels
			);
		}

		public int GetSampleSizeInBytes(TimeSpan duration)
		{
			return SoundEffect.GetSampleSizeInBytes(
				duration,
				sampleRate,
				channels
			);
		}

		#endregion

		#region Public SubmitBuffer Methods

		public void SubmitBuffer(byte[] buffer)
		{
			this.SubmitBuffer(buffer, 0, buffer.Length);
		}

		public void SubmitBuffer(byte[] buffer, int offset, int count)
		{
			// Generate a buffer if we don't have any to use.
			if (availableBuffers.Count == 0)
			{
				availableBuffers.Enqueue(
					AudioDevice.ALDevice.GenBuffer()
				);
			}

			// Push the data to OpenAL.
			IALBuffer newBuf = availableBuffers.Dequeue();
			AudioDevice.ALDevice.SetBufferData(
				newBuf,
				channels,
				buffer, // TODO: offset -flibit
				count,
				sampleRate
			);

			// If we're already playing, queue immediately.
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.QueueSourceBuffer(
					INTERNAL_alSource,
					newBuf
				);
				queuedBuffers.Enqueue(newBuf);

				// If the source stopped, reboot it now.
				if (AudioDevice.ALDevice.GetSourceState(INTERNAL_alSource) == SoundState.Stopped)
				{
					AudioDevice.ALDevice.PlaySource(INTERNAL_alSource);
				}
			}
			else
			{
				buffersToQueue.Enqueue(newBuf);
			}

			PendingBufferCount += 1;
		}

		#endregion

		#region Public Play Method

		public override void Play()
		{
			Play(true);
		}

		#endregion

		#region Internal Play Method

		internal void Play(bool isManaged)
		{
			if (State != SoundState.Stopped)
			{
				return; // No-op if we're already playing.
			}

			if (INTERNAL_alSource != null)
			{
				// The sound has stopped, but hasn't cleaned up yet...
				AudioDevice.ALDevice.StopAndDisposeSource(INTERNAL_alSource);
				INTERNAL_alSource = null;
			}
			while (queuedBuffers.Count > 0)
			{
				availableBuffers.Enqueue(queuedBuffers.Dequeue());
				PendingBufferCount -= 1;
			}

			INTERNAL_alSource = AudioDevice.ALDevice.GenSource();
			if (INTERNAL_alSource == null)
			{
				System.Console.WriteLine("WARNING: AL SOURCE WAS NOT AVAILABLE. SKIPPING.");
				return;
			}

			// Queue the buffers to this source
			while (buffersToQueue.Count > 0)
			{
				IALBuffer nextBuf = buffersToQueue.Dequeue();
				queuedBuffers.Enqueue(nextBuf);
				AudioDevice.ALDevice.QueueSourceBuffer(INTERNAL_alSource, nextBuf);
			}

			// Apply Pan/Position
			if (INTERNAL_positionalAudio)
			{
				INTERNAL_positionalAudio = false;
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
			Pitch = Pitch;

			// ... but wait! What if we need moar buffers?
			for (
				int i = MINIMUM_BUFFER_CHECK - PendingBufferCount;
				(i > 0) && BufferNeeded != null;
				i -= 1
			) {
				BufferNeeded(this, null);
			}

			// Finally.
			AudioDevice.ALDevice.PlaySource(INTERNAL_alSource);
			if (isManaged)
			{
				AudioDevice.DynamicInstancePool.Add(this);
			}
		}

		#endregion

		#region Internal Update Method

		internal void Update()
		{
			// Get the number of processed buffers.
			int finishedBuffers = AudioDevice.ALDevice.CheckProcessedBuffers(
				INTERNAL_alSource
			);
			if (finishedBuffers == 0)
			{
				// Nothing to do... yet.
				return;
			}

			// Dequeue the processed buffers, error checking as needed.
			AudioDevice.ALDevice.DequeueSourceBuffers(
				INTERNAL_alSource,
				finishedBuffers,
				queuedBuffers
			);

			// The processed buffers are now available.
			for (int i = 0; i < finishedBuffers; i += 1)
			{
				availableBuffers.Enqueue(queuedBuffers.Dequeue());
			}

			// PendingBufferCount changed during playback, trigger now!
			PendingBufferCount -= finishedBuffers;
			if (BufferNeeded != null)
			{
				BufferNeeded(this, null);
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

		#region Internal Sample Data Retrieval Method

		internal void GetSamples(float[] samples)
		{
			if (INTERNAL_alSource != null && queuedBuffers.Count > 0)
			{
				AudioDevice.ALDevice.GetBufferData(
					INTERNAL_alSource,
					queuedBuffers.ToArray(), // FIXME: Blech -flibit
					samples,
					channels
				);
			}
			else
			{
				Array.Clear(samples, 0, samples.Length);
			}
		}

		#endregion

		#region Public FNA Extension Methods

		/* THIS IS AN EXTENSION OF THE XNA4 API! */
		public void SubmitFloatBufferEXT(float[] buffer)
		{
			/* Float samples are the typical format received from decoders.
			 * We currently use this for the VideoPlayer.
			 * -flibit
			 */

			// Generate a buffer if we don't have any to use.
			if (availableBuffers.Count == 0)
			{
				availableBuffers.Enqueue(AudioDevice.ALDevice.GenBuffer());
			}

			// Push buffer to the AL.
			IALBuffer newBuf = availableBuffers.Dequeue();
			AudioDevice.ALDevice.SetBufferData(
				newBuf,
				channels,
				buffer,
				sampleRate
			);

			// If we're already playing, queue immediately.
			if (INTERNAL_alSource != null)
			{
				AudioDevice.ALDevice.QueueSourceBuffer(
					INTERNAL_alSource,
					newBuf
				);
				queuedBuffers.Enqueue(newBuf);

				// If the source stopped, reboot it now.
				if (AudioDevice.ALDevice.GetSourceState(INTERNAL_alSource) == SoundState.Stopped)
				{
					AudioDevice.ALDevice.PlaySource(INTERNAL_alSource);
				}
			}
			else
			{
				buffersToQueue.Enqueue(newBuf);
			}

			PendingBufferCount += 1;
		}

		#endregion
	}
}
