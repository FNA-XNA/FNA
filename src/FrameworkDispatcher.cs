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
		internal static List<SoundEffectInstance> FireAndForgetInstances = new List<SoundEffectInstance>();
		internal static List<DynamicSoundEffectInstance> Streams = new List<DynamicSoundEffectInstance>();

		#endregion

		#region Public Methods

		public static void Update()
		{
			/* Updates the status of various framework components
			 * (such as power state and media), and raises related events.
			 */
			for (int i = 0; i < FireAndForgetInstances.Count; i += 1)
			{
				SoundEffectInstance sfi = FireAndForgetInstances[i];
				if (sfi.State == SoundState.Stopped)
				{
					sfi.Dispose();
					FireAndForgetInstances.RemoveAt(i);
					i -= 1;
				}
			}
			foreach (DynamicSoundEffectInstance stream in Streams)
			{
				stream.Update();
			}
			if (Microphone.micList != null)
			{
				for (int i = 0; i < Microphone.micList.Count; i += 1)
				{
					Microphone.micList[i].CheckBuffer();
				}
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
