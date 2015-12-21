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
	internal interface IALDevice
	{
		void Update();
		void Dispose();
		ReadOnlyCollection<RendererDetail> GetDevices();
		ReadOnlyCollection<Microphone> GetCaptureDevices();

		IALBuffer GenBuffer();
		IALBuffer GenBuffer(
			byte[] data,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		);
		void DeleteBuffer(IALBuffer buffer);
		void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			byte[] data,
			int count,
			int sampleRate
		);
		void SetBufferData(
			IALBuffer buffer,
			AudioChannels channels,
			float[] data,
			int sampleRate
		);

		IALSource GenSource();
		IALSource GenSource(IALBuffer buffer);
		void StopAndDisposeSource(IALSource source);
		void PlaySource(IALSource source);
		void PauseSource(IALSource source);
		void ResumeSource(IALSource source);
		SoundState GetSourceState(IALSource source);
		void SetSourceVolume(IALSource source, float volume);
		void SetSourceLooped(IALSource source, bool looped);
		void SetSourcePan(IALSource source, float pan);
		void SetSourcePosition(IALSource source, Vector3 pos);
		void SetSourcePitch(IALSource source, float pitch, bool clamp);
		void SetSourceReverb(IALSource source, IALReverb reverb);
		void SetSourceLowPassFilter(IALSource source, float hfGain);
		void SetSourceHighPassFilter(IALSource source, float lfGain);
		void SetSourceBandPassFilter(IALSource source, float hfGain, float lfGain);
		void QueueSourceBuffer(IALSource source, IALBuffer buffer);
		void DequeueSourceBuffers(
			IALSource source,
			int buffersToDequeue,
			Queue<IALBuffer> errorCheck
		);
		int CheckProcessedBuffers(IALSource source);
		void GetBufferData(
			IALSource source,
			IALBuffer[] buffer,
			float[] samples,
			AudioChannels channels
		);

		IALReverb GenReverb(DSPParameter[] parameters);
		void DeleteReverb(IALReverb reverb);
		void CommitReverbChanges(IALReverb reverb);
		void SetReverbReflectionsDelay(IALReverb reverb, float value);
		void SetReverbDelay(IALReverb reverb, float value);
		void SetReverbPositionLeft(IALReverb reverb, float value);
		void SetReverbPositionRight(IALReverb reverb, float value);
		void SetReverbPositionLeftMatrix(IALReverb reverb, float value);
		void SetReverbPositionRightMatrix(IALReverb reverb, float value);
		void SetReverbEarlyDiffusion(IALReverb reverb, float value);
		void SetReverbLateDiffusion(IALReverb reverb, float value);
		void SetReverbLowEQGain(IALReverb reverb, float value);
		void SetReverbLowEQCutoff(IALReverb reverb, float value);
		void SetReverbHighEQGain(IALReverb reverb, float value);
		void SetReverbHighEQCutoff(IALReverb reverb, float value);
		void SetReverbRearDelay(IALReverb reverb, float value);
		void SetReverbRoomFilterFrequency(IALReverb reverb, float value);
		void SetReverbRoomFilterMain(IALReverb reverb, float value);
		void SetReverbRoomFilterHighFrequency(IALReverb reverb, float value);
		void SetReverbReflectionsGain(IALReverb reverb, float value);
		void SetReverbGain(IALReverb reverb, float value);
		void SetReverbDecayTime(IALReverb reverb, float value);
		void SetReverbDensity(IALReverb reverb, float value);
		void SetReverbRoomSize(IALReverb reverb, float value);
		void SetReverbWetDryMix(IALReverb reverb, float value);

		IntPtr StartDeviceCapture(string name, int sampleRate, int bufSize);
		void StopDeviceCapture(IntPtr handle);
		int CaptureSamples(IntPtr handle, IntPtr buffer, int count);
		bool CaptureHasSamples(IntPtr handle);
	}

	internal interface IALBuffer
	{
		TimeSpan Duration
		{
			get;
		}
	}

	internal interface IALSource
	{
	}

	internal interface IALReverb
	{
	}
}
