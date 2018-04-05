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
				return Device().DSPSettings.DopplerFactor;
			}
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException("value <= 0.0f");
				}
				Device().DSPSettings.DopplerFactor = value;
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
			sampleRate,
			(ushort) channels,
			0,
			0,
			1,
			16
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
			sampleRate,
			(ushort) channels,
			loopStart,
			loopLength,
			1,
			16
		) {
		}

		#endregion

		#region Internal Constructor

		internal SoundEffect(
			string name,
			byte[] buffer,
			int offset,
			int count,
			int sampleRate,
			ushort channels,
			int loopStart,
			int loopLength,
			ushort formatTag,
			ushort formatParameter
		) {
			Device();
			Name = name;

			/* Buffer format */
			format = new FAudio.FAudioWaveFormatEx();
			format.wFormatTag = formatTag;
			format.nChannels = channels;
			format.nSamplesPerSec = (uint) sampleRate;
			format.nAvgBytesPerSec = 0; /* FIXME */

			/* Lazily assigning formatParameter... */
			format.nBlockAlign = formatParameter;
			format.wBitsPerSample = formatParameter;

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
			if (formatTag == 1)
			{
				handle.PlayLength = (uint) (
					count /
					format.nChannels /
					(format.wBitsPerSample / 8)
				);
			}
			else if (formatTag == 2)
			{
				handle.PlayLength = (uint) (
					count /
					formatParameter *
					(((formatParameter / channels) - 6) * 2)
				);
			}
			handle.LoopBegin = (uint) loopStart;
			handle.LoopLength = (uint) loopLength;
			handle.LoopCount = 0; /* Set by Instances! */

			/* TODO: Might be needed for ADPCMWaveFormat accuracy */
			format.cbSize = 0;
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
			SoundEffectInstance instance = new SoundEffectInstance(
				this,
				true
			);
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
			byte[] data;
			int sampleRate = 0;
			ushort numChannels = 0;
			ushort format = 0;
			ushort formatParameter = 0;

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

				// Header Information
				format = reader.ReadUInt16();			// 2
				numChannels = reader.ReadUInt16();		// 4
				sampleRate = reader.ReadInt32();		// 8
				reader.ReadUInt32();				// 12, Byte Rate
				ushort blockAlign = reader.ReadUInt16();	// 14, Block Align
				ushort bitDepth = reader.ReadUInt16();		// 16, Bits Per Sample

				if (format == 1)
				{
					System.Diagnostics.Debug.Assert(bitDepth == 8 || bitDepth == 16);
					formatParameter = bitDepth;
				}
				else if (format == 2)
				{
					formatParameter = blockAlign;
				}
				else
				{
					throw new NotSupportedException("Wave format is not supported.");
				}

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
				sampleRate,
				numChannels,
				0,
				0,
				format,
				formatParameter
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

			public FAudio.F3DAUDIO_DSP_SETTINGS DSPSettings;
			public float CurveDistanceScaler;
			public float SpeedOfSound;

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
					if ((DeviceDetails.Role & FAudio.FAudioDeviceRole.DefaultGameDevice) == FAudio.FAudioDeviceRole.DefaultGameDevice)
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
					48000, /* Should be 0, but SDL... */
					0,
					i,
					IntPtr.Zero
				);

				DSPSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
				DSPSettings.DopplerFactor = 1.0f;
				CurveDistanceScaler = 1.0f;
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
				FAudio.FAudioVoice_DestroyVoice(MasterVoice);
				FAudio.FAudio_Release(Handle);
				Context = null;
			}

			public static void Create()
			{
				/* TODO: Remove the FNA variable! */
				if (Environment.GetEnvironmentVariable(
					"FNA_AUDIO_DISABLE_SOUND"
				) == "1") {
					Environment.SetEnvironmentVariable(
						"SDL_AUDIODRIVER",
						"dummy"
					);
				}

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
