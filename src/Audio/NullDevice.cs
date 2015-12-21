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
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	/* This is a device that deliberately does as little as possible, allowing
	 * for no sound without throwing NoAudioHardwareExceptions. This is not a
	 * part of the XNA4 spec, however, so behavior here is entirely undefined!
	 * -flibit
	 */
	internal class NullDevice : IALDevice
	{
		private class NullBuffer : IALBuffer
		{
			public TimeSpan Duration
			{
				get
				{
					// FIXME: Maybe read the PCM/ADPCM lengths? -flibit
					return TimeSpan.Zero;
				}
			}
		}

		private class NullSource : IALSource
		{
		}

		private class NullReverb : IALReverb
		{
		}

		public void Update()
		{
			// No-op, duh.
		}

		public void Dispose()
		{
			// No-op, duh.
		}

		public ReadOnlyCollection<RendererDetail> GetDevices()
		{
			return new ReadOnlyCollection<RendererDetail>(
				new List<RendererDetail>()
			);
		}

		public ReadOnlyCollection<Microphone> GetCaptureDevices()
		{
			return new ReadOnlyCollection<Microphone>(
				new List<Microphone>()
			);
		}

		public IALBuffer GenBuffer()
		{
			return new NullBuffer();
		}

		public IALBuffer GenBuffer(
			byte[] data,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		) {
			return new NullBuffer();
		}

		public void DeleteBuffer(IALBuffer buffer)
		{
			// No-op, duh.
		}

		public void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			byte[] data,
			int count,
			int sampleRate
		) {
			// No-op, duh.
		}

		public void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			float[] data,
			int sampleRate
		) {
			// No-op, duh.
		}

		public IALSource GenSource()
		{
			return new NullSource();
		}

		public IALSource GenSource(IALBuffer buffer)
		{
			return new NullSource();
		}

		public void StopAndDisposeSource(IALSource source)
		{
			// No-op, duh.
		}

		public void PlaySource(IALSource source)
		{
			// No-op, duh.
		}

		public void PauseSource(IALSource source)
		{
			// No-op, duh.
		}

		public void ResumeSource(IALSource source)
		{
			// No-op, duh.
		}

		public SoundState GetSourceState(IALSource source)
		{
			/* FIXME: This return value is highly volatile!
			 * You can't necessarily do Stopped, because then stuff like Song
			 * explodes, but SoundState.Playing doesn't make a whole lot of
			 * sense either. This at least prevents annoyances like Song errors
			 * from happening and, for the most part, claims to be "playing"
			 * depending on how you ask for a source's state.
			 * -flibit
			 */
			return SoundState.Paused;
		}

		public void SetSourceVolume(IALSource source, float volume)
		{
			// No-op, duh.
		}

		public void SetSourceLooped(IALSource source, bool looped)
		{
			// No-op, duh.
		}

		public void SetSourcePan(IALSource source, float pan)
		{
			// No-op, duh.
		}

		public void SetSourcePosition(IALSource source, Vector3 pos)
		{
			// No-op, duh.
		}

		public void SetSourcePitch(IALSource source, float pitch, bool clamp)
		{
			// No-op, duh.
		}

		public void SetSourceReverb(IALSource source, IALReverb reverb)
		{
			// No-op, duh.
		}

		public void SetSourceLowPassFilter(IALSource source, float hfGain)
		{
			// No-op, duh.
		}

		public void SetSourceHighPassFilter(IALSource source, float lfGain)
		{
			// No-op, duh.
		}

		public void SetSourceBandPassFilter(IALSource source, float hfGain, float lfGain)
		{
			// No-op, duh.
		}

		public void QueueSourceBuffer(IALSource source, IALBuffer buffer)
		{
			// No-op, duh.
		}

		public void DequeueSourceBuffers(
			IALSource source,
			int buffersToDequeue,
			Queue<IALBuffer> errorCheck
		) {
			// No-op, duh.
		}

		public int CheckProcessedBuffers(IALSource source)
		{
			return 0;
		}

		public void GetBufferData(
			IALSource source,
			IALBuffer[] buffer,
			float[] samples,
			AudioChannels channels
		) {
			// No-op, duh.
		}

		public IALReverb GenReverb(DSPParameter[] parameters)
		{
			return new NullReverb();
		}

		public void DeleteReverb(IALReverb reverb)
		{
			// No-op, duh.
		}

		public void CommitReverbChanges(IALReverb reverb)
		{
			// No-op, duh.
		}

		public void SetReverbReflectionsDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionLeft(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionRight(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionLeftMatrix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbPositionRightMatrix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbEarlyDiffusion(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLateDiffusion(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLowEQGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbLowEQCutoff(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbHighEQGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbHighEQCutoff(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRearDelay(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterFrequency(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterMain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbReflectionsGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbGain(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDecayTime(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbDensity(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbRoomSize(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public void SetReverbWetDryMix(IALReverb reverb, float value)
		{
			// No-op, duh.
		}

		public IntPtr StartDeviceCapture(string name, int sampleRate, int bufSize)
		{
			return IntPtr.Zero;
		}

		public void StopDeviceCapture(IntPtr handle)
		{
			// No-op, duh.
		}

		public int CaptureSamples(IntPtr handle, IntPtr buffer, int count)
		{
			return 0;
		}

		public bool CaptureHasSamples(IntPtr handle)
		{
			return false;
		}
	}
}
