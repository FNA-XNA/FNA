#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class FrameworkDispatcher
	{
		#region Internal Variables

		internal static bool ActiveSongChanged = false;
		internal static bool MediaStateChanged = false;

		static SpinLock _deadSoundsLock = new SpinLock();
		static readonly Queue<SoundEffectInstance> DeadSounds = new Queue<SoundEffectInstance>();
		internal static List<DynamicSoundEffectInstance> Streams = new List<DynamicSoundEffectInstance>();

		#endregion

		#region Public Methods

		public static void EnqueueSoundEffectInstanceStop(SoundEffectInstance soundEffectInstance) {
			var lockTaken = false;
			_deadSoundsLock.Enter(ref lockTaken);
			try
            {
				DeadSounds.Enqueue(soundEffectInstance);
			}
			finally
            {
				if (lockTaken)
					_deadSoundsLock.Exit();
			}
		}

		public static void Update()
		{
			/* Updates the status of various framework components
			 * (such as power state and media), and raises related events.
			 */
			while (true)
			{
				SoundEffectInstance soundEffectInstance;
				var lockTaken = false;
				_deadSoundsLock.Enter(ref lockTaken);
				try
				{
					if (DeadSounds.Count == 0)
						break;
					soundEffectInstance = DeadSounds.Dequeue();
				}
				finally
				{
					if (lockTaken)
						_deadSoundsLock.Exit();
				}
				soundEffectInstance.Stop(true);
			}
			foreach (DynamicSoundEffectInstance stream in Streams)
			{
				stream.Update();
			}

			MediaPlayer.Update();
			if (ActiveSongChanged)
			{
				MediaPlayer.OnActiveSongChanged();
				ActiveSongChanged = false;
			}
			if (MediaStateChanged)
			{
				MediaPlayer.OnMediaStateChanged();
				MediaStateChanged = false;
			}
		}

		#endregion
	}
}
