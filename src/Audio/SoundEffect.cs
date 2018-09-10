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
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.soundeffect.aspx
	public sealed class SoundEffect : IDisposable
	{
		#region Public Properties

		public TimeSpan Duration
		{
			get
			{
				return TimeSpan.FromSeconds(
					(double) handle.PlayLength /
					(double) format.nSamplesPerSec
				);
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			set;
		}

		#endregion

		#region Public Static Properties

		public static float MasterVolume
		{
			get
			{
				float result;
				FAudio.FAudioVoice_GetVolume(
					Device().MasterVoice,
					out result
				);
				return result;
			}
			set
			{
				FAudio.FAudioVoice_SetVolume(
					Device().MasterVoice,
					value,
					0
				);
			}
		}

		public static float DistanceScale
		{
			get
			{
				return Device().CurveDistanceScaler;
			}
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException("value <= 0.0f");
				}
				Device().CurveDistanceScaler = value;
			}
		}

		public static float DopplerScale
		{
			get
			{
				return Device().DopplerScale;
			}
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException("value <= 0.0f");
				}
				Device().DopplerScale = value;
			}
		}

		public static float SpeedOfSound
		{
			get
			{
				return Device().SpeedOfSound;
			}
			set
			{
				FAudioContext dev = Device();
				dev.SpeedOfSound = value;
				FAudio.F3DAudioInitialize(
					dev.DeviceDetails.OutputFormat.dwChannelMask,
					dev.SpeedOfSound,
					dev.Handle3D
				);
			}
		}

		#endregion

		#region Internal Variables

		internal List<WeakReference> Instances = new List<WeakReference>();
		internal FAudio.FAudioBuffer handle;
		internal FAudio.FAudioWaveFormatEx format;
		internal uint loopStart;
		internal uint loopLength;

		#endregion

		#region Public Constructors

		public SoundEffect(
			byte[] buffer,
			int sampleRate,
			AudioChannels channels
		) : this(
			null,
			buffer,
			0,
			buffer.Length,
			1,
			(ushort) channels,
			(uint) sampleRate,
			(uint) (sampleRate * ((ushort) channels * 2)),
			(ushort) ((ushort) channels * 2),
			16,
			0,
			0
		) {
		}

		public SoundEffect(
			byte[] buffer,
			int offset,
			int count,
			int sampleRate,
			AudioChannels channels,
			int loopStart,
			int loopLength
		) : this(
			null,
			buffer,
			offset,
			count,
			1,
			(ushort) channels,
			(uint) sampleRate,
			(uint) (sampleRate * ((ushort) channels * 2)),
			(ushort) ((ushort) channels * 2),
			16,
			loopStart,
			loopLength
		) {
		}

		#endregion

		#region Internal Constructor

		internal SoundEffect(
			string name,
			byte[] buffer,
			int offset,
			int count,
			ushort wFormatTag,
			ushort nChannels,
			uint nSamplesPerSec,
			uint nAvgBytesPerSec,
			ushort nBlockAlign,
			ushort wBitsPerSample,
			int loopStart,
			int loopLength
		) {
			Device();
			Name = name;
			this.loopStart = (uint) loopStart;
			this.loopLength = (uint) loopLength;

			/* Buffer format */
			format = new FAudio.FAudioWaveFormatEx();
			format.wFormatTag = wFormatTag;
			format.nChannels = nChannels;
			format.nSamplesPerSec = nSamplesPerSec;
			format.nAvgBytesPerSec = nAvgBytesPerSec;
			format.nBlockAlign = nBlockAlign;
			format.wBitsPerSample = wBitsPerSample;
			format.cbSize = 0; /* May be needed for ADPCM? */

			/* Easy stuff */
			handle = new FAudio.FAudioBuffer();
			handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
			handle.pContext = IntPtr.Zero;

			/* Buffer data */
			handle.AudioBytes = (uint) count;
			handle.pAudioData = Marshal.AllocHGlobal(count);
			Marshal.Copy(
				buffer,
				offset,
				handle.pAudioData,
				count
			);

			/* Play regions */
			handle.PlayBegin = 0;
			if (wFormatTag == 1)
			{
				handle.PlayLength = (uint) (
					count /
					nChannels /
					(wBitsPerSample / 8)
				);
			}
			else if (wFormatTag == 2)
			{
				handle.PlayLength = (uint) (
					count /
					nBlockAlign *
					(((nBlockAlign / nChannels) - 6) * 2)
				);
			}

			/* Set by Instances! */
			handle.LoopBegin = 0;
			handle.LoopLength = 0;
			handle.LoopCount = 0;
		}

		#endregion

		#region Destructor

		~SoundEffect()
		{
			Dispose();
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			if (!IsDisposed)
			{
				/* FIXME: Is it ironic that we're generating
				 * garbage with ToArray while cleaning up after
				 * the program's leaks?
				 * -flibit
				 */
				foreach (WeakReference instance in Instances.ToArray())
				{
					object target = instance.Target;
					if (target != null)
					{
						(target as IDisposable).Dispose();
					}
				}
				Instances.Clear();
				Marshal.FreeHGlobal(handle.pAudioData);
				IsDisposed = true;
			}
		}

		public bool Play()
		{
			return Play(1.0f, 0.0f, 0.0f);
		}

		public bool Play(float volume, float pitch, float pan)
		{
			SoundEffectInstance instance = new SoundEffectInstance(this);
			instance.Volume = volume;
			instance.Pitch = pitch;
			instance.Pan = pan;
			instance.Play();
			if (instance.State != SoundState.Playing)
			{
				// Ran out of AL sources, probably.
				instance.Dispose();
				return false;
			}
			return true;
		}

		public SoundEffectInstance CreateInstance()
		{
			return new SoundEffectInstance(this);
		}

		#endregion

		#region Public Static Methods

		public static TimeSpan GetSampleDuration(
			int sizeInBytes,
			int sampleRate,
			AudioChannels channels
		) {
			sizeInBytes /= 2; // 16-bit PCM!
			int ms = (int) (
				(sizeInBytes / (int) channels) /
				(sampleRate / 1000.0f)
			);
			return new TimeSpan(0, 0, 0, 0, ms);
		}

		public static int GetSampleSizeInBytes(
			TimeSpan duration,
			int sampleRate,
			AudioChannels channels
		) {
			return (int) (
				duration.TotalSeconds *
				sampleRate *
				(int) channels *
				2 // 16-bit PCM!
			);
		}

		public static SoundEffect FromStream(Stream stream)
		{
			// Sample data
			byte[] data;

			// WaveFormatEx data
			ushort wFormatTag;
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			ushort wBitsPerSample;
			// ushort cbSize;

			using (BinaryReader reader = new BinaryReader(stream))
			{
				// RIFF Signature
				string signature = new string(reader.ReadChars(4));
				if (signature != "RIFF")
				{
					throw new NotSupportedException("Specified stream is not a wave file.");
				}

				reader.ReadUInt32(); // Riff Chunk Size

				string wformat = new string(reader.ReadChars(4));
				if (wformat != "WAVE")
				{
					throw new NotSupportedException("Specified stream is not a wave file.");
				}

				// WAVE Header
				string format_signature = new string(reader.ReadChars(4));
				while (format_signature != "fmt ")
				{
					reader.ReadBytes(reader.ReadInt32());
					format_signature = new string(reader.ReadChars(4));
				}

				int format_chunk_size = reader.ReadInt32();

				wFormatTag = reader.ReadUInt16();
				nChannels = reader.ReadUInt16();
				nSamplesPerSec = reader.ReadUInt32();
				nAvgBytesPerSec = reader.ReadUInt32();
				nBlockAlign = reader.ReadUInt16();
				wBitsPerSample = reader.ReadUInt16();

				// Reads residual bytes
				if (format_chunk_size > 16)
				{
					reader.ReadBytes(format_chunk_size - 16);
				}

				// data Signature
				string data_signature = new string(reader.ReadChars(4));
				while (data_signature.ToLowerInvariant() != "data")
				{
					reader.ReadBytes(reader.ReadInt32());
					data_signature = new string(reader.ReadChars(4));
				}
				if (data_signature != "data")
				{
					throw new NotSupportedException("Specified wave file is not supported.");
				}

				int waveDataLength = reader.ReadInt32();
				data = reader.ReadBytes(waveDataLength);
			}

			return new SoundEffect(
				null,
				data,
				0,
				data.Length,
				wFormatTag,
				nChannels,
				nSamplesPerSec,
				nAvgBytesPerSec,
				nBlockAlign,
				wBitsPerSample,
				0,
				0
			);
		}

		#endregion

		#region FAudio Context

		internal class FAudioContext
		{
			public static FAudioContext Context = null;

			public readonly IntPtr Handle;
			public readonly byte[] Handle3D;
			public readonly IntPtr MasterVoice;
			public readonly FAudio.FAudioDeviceDetails DeviceDetails;

			public float CurveDistanceScaler;
			public float DopplerScale;
			public float SpeedOfSound;

			public IntPtr ReverbVoice;
			private FAudio.FAudioVoiceSends reverbSends;

			private FAudioContext(IntPtr ctx, uint devices)
			{
				Handle = ctx;

				uint i;
				for (i = 0; i < devices; i += 1)
				{
					FAudio.FAudio_GetDeviceDetails(
						Handle,
						i,
						out DeviceDetails
					);
					if ((DeviceDetails.Role & FAudio.FAudioDeviceRole.FAudioDefaultGameDevice) == FAudio.FAudioDeviceRole.FAudioDefaultGameDevice)
					{
						break;
					}
				}
				if (i == devices)
				{
					i = 0; /* Oh well. */
					FAudio.FAudio_GetDeviceDetails(
						Handle,
						i,
						out DeviceDetails
					);
				}
				FAudio.FAudio_CreateMasteringVoice(
					Handle,
					out MasterVoice,
					FAudio.FAUDIO_DEFAULT_CHANNELS,
					FAudio.FAUDIO_DEFAULT_SAMPLERATE,
					0,
					i,
					IntPtr.Zero
				);

				CurveDistanceScaler = 1.0f;
				DopplerScale = 1.0f;
				SpeedOfSound = 343.5f;
				Handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
				FAudio.F3DAudioInitialize(
					DeviceDetails.OutputFormat.dwChannelMask,
					SpeedOfSound,
					Handle3D
				);

				Context = this;
			}

			public void Dispose()
			{
				if (ReverbVoice != IntPtr.Zero)
				{
					FAudio.FAudioVoice_DestroyVoice(ReverbVoice);
					ReverbVoice = IntPtr.Zero;
					Marshal.FreeHGlobal(reverbSends.pSends);
				}
				FAudio.FAudioVoice_DestroyVoice(MasterVoice);
				FAudio.FAudio_Release(Handle);
				Context = null;
			}

			public unsafe void AttachReverb(IntPtr voice)
			{
				// Only create a reverb voice if they ask for it!
				if (ReverbVoice == IntPtr.Zero)
				{
					IntPtr reverb;
					FAudio.FAudioCreateReverb(out reverb, 0);

					IntPtr chainPtr;
					chainPtr = Marshal.AllocHGlobal(
						Marshal.SizeOf(typeof(FAudio.FAudioEffectChain))
					);
					FAudio.FAudioEffectChain* reverbChain = (FAudio.FAudioEffectChain*) chainPtr;
					reverbChain->EffectCount = 1;
					reverbChain->pEffectDescriptors = Marshal.AllocHGlobal(
						Marshal.SizeOf(typeof(FAudio.FAudioEffectDescriptor))
					);

					FAudio.FAudioEffectDescriptor* reverbDesc =
						(FAudio.FAudioEffectDescriptor*) reverbChain->pEffectDescriptors;
					reverbDesc->InitialState = 1;
					reverbDesc->OutputChannels = (uint) (
						(DeviceDetails.OutputFormat.Format.nChannels == 6) ? 6 : 1
					);
					reverbDesc->pEffect = reverb;

					FAudio.FAudio_CreateSubmixVoice(
						Handle,
						out ReverbVoice,
						1, /* Reverb will be omnidirectional */
						DeviceDetails.OutputFormat.Format.nSamplesPerSec,
						0,
						0,
						IntPtr.Zero,
						chainPtr
					);
					FAudio.FAPOBase_Release(reverb);

					Marshal.FreeHGlobal(reverbChain->pEffectDescriptors);
					Marshal.FreeHGlobal(chainPtr);

					// Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC
					IntPtr rvbParamsPtr = Marshal.AllocHGlobal(
						Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters))
					);
					FAudio.FAudioFXReverbParameters* rvbParams = (FAudio.FAudioFXReverbParameters*) rvbParamsPtr;
					rvbParams->WetDryMix = 100.0f;
					rvbParams->ReflectionsDelay = 7;
					rvbParams->ReverbDelay = 11;
					rvbParams->RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY;
					rvbParams->PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
					rvbParams->PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
					rvbParams->PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
					rvbParams->PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
					rvbParams->EarlyDiffusion = 15;
					rvbParams->LateDiffusion = 15;
					rvbParams->LowEQGain = 8;
					rvbParams->LowEQCutoff = 4;
					rvbParams->HighEQGain = 8;
					rvbParams->HighEQCutoff = 6;
					rvbParams->RoomFilterFreq = 5000f;
					rvbParams->RoomFilterMain = -10f;
					rvbParams->RoomFilterHF = -1f;
					rvbParams->ReflectionsGain = -26.0200005f;
					rvbParams->ReverbGain = 10.0f;
					rvbParams->DecayTime = 1.49000001f;
					rvbParams->Density = 100.0f;
					rvbParams->RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE;
					FAudio.FAudioVoice_SetEffectParameters(
						ReverbVoice,
						0,
						rvbParamsPtr,
						(uint) Marshal.SizeOf(typeof(FAudio.FAudioFXReverbParameters)),
						0
					);
					Marshal.FreeHGlobal(rvbParamsPtr);

					reverbSends = new FAudio.FAudioVoiceSends();
					reverbSends.SendCount = 2;
					reverbSends.pSends = Marshal.AllocHGlobal(
						2 * Marshal.SizeOf(typeof(FAudio.FAudioSendDescriptor))
					);
					FAudio.FAudioSendDescriptor* sendDesc = (FAudio.FAudioSendDescriptor*) reverbSends.pSends;
					sendDesc[0].Flags = 0;
					sendDesc[0].pOutputVoice = MasterVoice;
					sendDesc[1].Flags = 0;
					sendDesc[1].pOutputVoice = ReverbVoice;
				}

				// Oh hey here's where we actually attach it
				FAudio.FAudioVoice_SetOutputVoices(
					voice,
					ref reverbSends
				);
			}

			public static void Create()
			{
				IntPtr ctx;
				try
				{
					FAudio.FAudioCreate(
						out ctx,
						0,
						FAudio.FAUDIO_DEFAULT_PROCESSOR
					);
				}
				catch
				{
					/* FAudio is missing, bail! */
					return;
				}

				uint devices;
				FAudio.FAudio_GetDeviceCount(
					ctx,
					out devices
				);
				if (devices == 0)
				{
					/* No sound cards, bail! */
					FAudio.FAudio_Release(ctx);
					return;
				}

				Context = new FAudioContext(ctx, devices);
			}
		}

		internal static FAudioContext Device()
		{
			if (FAudioContext.Context != null)
			{
				return FAudioContext.Context;
			}
			FAudioContext.Create();
			if (FAudioContext.Context == null)
			{
				throw new NoAudioHardwareException();
			}
			return FAudioContext.Context;
		}

		#endregion
	}
}
