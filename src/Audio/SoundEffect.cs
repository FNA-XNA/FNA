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
using System.IO;
using System.Collections.Generic;
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
				return INTERNAL_buffer.Duration;
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

		// FIXME: This should affect all sounds! alListener? -flibit
		private static float INTERNAL_masterVolume = 1.0f;
		public static float MasterVolume
		{
			get
			{
				return INTERNAL_masterVolume;
			}
			set
			{
				INTERNAL_masterVolume = value;
			}
		}

		// FIXME: How does this affect OpenAL? -flibit
		private static float INTERNAL_distanceScale = 1.0f;
		public static float DistanceScale
		{
			get
			{
				return INTERNAL_distanceScale;
			}
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException("value of DistanceScale");
				}
				INTERNAL_distanceScale = value;
			}
		}

		// FIXME: How does this affect OpenAL? -flibit
		private static float INTERNAL_dopplerScale = 1.0f;
		public static float DopplerScale
		{
			get
			{
				return INTERNAL_dopplerScale;
			}
			set
			{
				if (value <= 0.0f)
				{
					throw new ArgumentOutOfRangeException("value of DopplerScale");
				}
				INTERNAL_dopplerScale = value;
			}
		}

		// FIXME: How does this affect OpenAL? -flibit
		private static float INTERNAL_speedOfSound = 343.5f;
		public static float SpeedOfSound
		{
			get
			{
				return INTERNAL_speedOfSound;
			}
			set
			{
				INTERNAL_speedOfSound = value;
			}
		}

		#endregion

		#region Internal Variables

		internal List<WeakReference> Instances = new List<WeakReference>();
		internal IALBuffer INTERNAL_buffer;

		#endregion

		#region Public Constructors

		public SoundEffect(
			byte[] buffer,
			int sampleRate,
			AudioChannels channels
		) {
			INTERNAL_buffer = AudioDevice.GenBuffer(
				buffer,
				(uint) sampleRate,
				(uint) channels,
				0,
				0,
				false,
				1
			);
		}

		public SoundEffect(
			byte[] buffer,
			int offset,
			int count,
			int sampleRate,
			AudioChannels channels,
			int loopStart,
			int loopLength
		) {
			byte[] sendBuf;
			if (offset != 0 || count != buffer.Length)
			{
				// I kind of hate this. -flibit
				sendBuf = new byte[count];
				Array.Copy(buffer, offset, sendBuf, 0, count);
			}
			else
			{
				sendBuf = buffer;
			}

			INTERNAL_buffer = AudioDevice.GenBuffer(
				sendBuf,
				(uint) sampleRate,
				(uint) channels,
				(uint) loopStart,
				(uint) (loopStart + loopLength),
				false,
				1
			);
		}

		#endregion

		#region Internal Constructors

		internal SoundEffect(Stream s)
		{
			INTERNAL_loadAudioStream(s);
		}

		internal SoundEffect(
			string name,
			byte[] buffer,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopLength,
			bool isADPCM,
			uint formatParameter
		) {
			Name = name;
			INTERNAL_buffer = AudioDevice.GenBuffer(
				buffer,
				sampleRate,
				channels,
				loopStart,
				loopStart + loopLength,
				isADPCM,
				formatParameter
			);
		}

		#endregion

		#region Destructor

		~SoundEffect()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Method

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
				if (INTERNAL_buffer != null)
				{
					AudioDevice.ALDevice.DeleteBuffer(INTERNAL_buffer);
				}
				IsDisposed = true;
			}
		}

		#endregion

		#region Additional SoundEffect/SoundEffectInstance Creation Methods

		public SoundEffectInstance CreateInstance()
		{
			return new SoundEffectInstance(this);
		}

		public static SoundEffect FromStream(Stream stream)
		{
			return new SoundEffect(stream);
		}

		#endregion

		#region Public Play Methods

		public bool Play()
		{
			// FIXME: Perhaps MasterVolume should be applied to alListener? -flibit
			return Play(MasterVolume, 0.0f, 0.0f);
		}

		public bool Play(float volume, float pitch, float pan)
		{
			SoundEffectInstance instance = CreateInstance();
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
			AudioDevice.InstancePool.Add(instance);
			return true;
		}

		#endregion

		#region Private WAV Loading Method

		private void INTERNAL_loadAudioStream(Stream s)
		{
			byte[] data;
			uint sampleRate = 0;
			uint numChannels = 0;
			bool isADPCM = false;
			uint formatParameter = 0;

			using (BinaryReader reader = new BinaryReader(s))
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
				uint audio_format = reader.ReadUInt16();	// 2
				numChannels = reader.ReadUInt16();		// 4
				sampleRate = reader.ReadUInt32();		// 8
				reader.ReadUInt32();				// 12, Byte Rate
				ushort blockAlign = reader.ReadUInt16();	// 14, Block Align
				ushort bitDepth = reader.ReadUInt16();		// 16, Bits Per Sample

				if (audio_format == 1)
				{
					System.Diagnostics.Debug.Assert(bitDepth == 8 || bitDepth == 16);
					formatParameter = (uint) (bitDepth / 16); // 1 for 16, 0 for 8
				}
				else if (audio_format != 2)
				{
					isADPCM = true;
					formatParameter = (((blockAlign / numChannels) - 6) * 2);
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

			INTERNAL_buffer = AudioDevice.GenBuffer(
				data,
				sampleRate,
				numChannels,
				0,
				0,
				isADPCM,
				formatParameter
			);
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

		#endregion
	}
}
