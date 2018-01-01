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
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	internal static class AudioDevice
	{
		#region Public Constants

		// Per XAudio2. Yes, it's seriously this high. -flibit
		public const float MAX_GAIN_VALUE = 16777216.0f;

		#endregion

		#region SoundEffect Master Properties

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
				if (ALDevice != null)
				{
					ALDevice.SetMasterVolume(value);
				}
			}
		}

		public static float DistanceScale = 1.0f;

		private static float INTERNAL_dopplerScale = 1.0f;
		public static float DopplerScale
		{
			get
			{
				return INTERNAL_dopplerScale;
			}
			set
			{
				INTERNAL_dopplerScale = value;
				if (ALDevice != null)
				{
					ALDevice.SetDopplerScale(value);
				}
			}
		}

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
				if (ALDevice != null)
				{
					ALDevice.SetSpeedOfSound(value);
				}
			}
		}

		#endregion

		#region RendererDetail List

		public static ReadOnlyCollection<RendererDetail> Renderers;

		#endregion

		#region Internal AL Device

		// FIXME: readonly? -flibit
		public static IALDevice ALDevice;

		#endregion

		#region SoundEffect Management Variables

		// FIXME: readonly? -flibit

		// Used to store SoundEffectInstances generated internally.
		public static List<SoundEffectInstance> InstancePool;

		// Used to store all DynamicSoundEffectInstances, to check buffer counts.
		public static List<DynamicSoundEffectInstance> DynamicInstancePool;

		#endregion

		#region Microphone Management Variables

		// FIXME: readonly? -flibit

		// Used to store Microphones that are currently recording
		public static List<Microphone> ActiveMics;

		#endregion

		#region Public Static Initialize Method

		public static void Initialize()
		{
			// We should only have one of these!
			if (ALDevice != null)
			{
				FNALoggerEXT.LogWarn("ALDevice already exists, overwriting!");
			}

			bool disableSound = Environment.GetEnvironmentVariable(
				"FNA_AUDIO_DISABLE_SOUND"
			) == "1";

			if (disableSound)
			{
				ALDevice = new NullALDevice();
			}
			else
			{
				ALDevice = FNAPlatform.CreateALDevice();
			}

			// Populate device info
			if (ALDevice != null)
			{
				ALDevice.SetMasterVolume(MasterVolume);
				ALDevice.SetDopplerScale(DopplerScale);
				ALDevice.SetSpeedOfSound(SpeedOfSound);

				Renderers = ALDevice.GetDevices();
				Microphone.All = ALDevice.GetCaptureDevices();

				InstancePool = new List<SoundEffectInstance>();
				DynamicInstancePool = new List<DynamicSoundEffectInstance>();
				ActiveMics = new List<Microphone>();
				AppDomain.CurrentDomain.ProcessExit += Dispose;
			}
			else
			{
				Renderers = new ReadOnlyCollection<RendererDetail>(new List<RendererDetail>());
				Microphone.All = new ReadOnlyCollection<Microphone>(new List<Microphone>());
			}
		}

		#endregion

		#region Private Static Dispose Method

		private static void Dispose(object sender, EventArgs e)
		{
			InstancePool.Clear();
			DynamicInstancePool.Clear();
			ALDevice.Dispose();
		}

		#endregion

		#region Public Static Update Methods

		public static void Update()
		{
			ALDevice.Update();

			for (int i = 0; i < InstancePool.Count; i += 1)
			{
				if (InstancePool[i].State == SoundState.Stopped)
				{
					InstancePool[i].Dispose();
					InstancePool.RemoveAt(i);
					i -= 1;
				}
			}

			for (int i = 0; i < DynamicInstancePool.Count; i += 1)
			{
				DynamicSoundEffectInstance sfi = DynamicInstancePool[i];
				sfi.Update();
				if (sfi.State == SoundState.Stopped)
				{
					i -= 1;
				}
			}

			foreach (Microphone mic in ActiveMics)
			{
				mic.CheckBuffer();
			}
		}

		#endregion

		#region Public Static Buffer Methods

		public static IALBuffer GenBuffer(int sampleRate, AudioChannels channels)
		{
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenBuffer(sampleRate, channels);
		}

		public static IALBuffer GenBuffer(
			byte[] data,
			uint sampleRate,
			uint channels,
			uint loopStart,
			uint loopEnd,
			bool isADPCM,
			uint formatParameter
		) {
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenBuffer(
				data,
				sampleRate,
				channels,
				loopStart,
				loopEnd,
				isADPCM,
				formatParameter
			);
		}

		#endregion

		#region Public Static Reverb Methods

		public static IALReverb GenReverb(DSPParameter[] parameters)
		{
			if (ALDevice == null)
			{
				throw new NoAudioHardwareException();
			}
			return ALDevice.GenReverb(parameters);
		}

		#endregion
	}
}
