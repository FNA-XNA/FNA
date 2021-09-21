#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input.Touch;
using __MediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class FrameworkDispatcher
	{
		#region Internal Variables

		internal static bool ActiveSongChanged = false;
		internal static bool MediaStateChanged = false;
		internal static List<DynamicSoundEffectInstance> Streams = new List<DynamicSoundEffectInstance>();

		#endregion

		#region Public Methods

		public static void Update()
		{
			/* Updates the status of various framework components
			 * (such as power state and media), and raises related events.
			 */
			lock (Streams)
			{
				for (int i = 0; i < Streams.Count; i += 1)
				{
					DynamicSoundEffectInstance dsfi = Streams[i];
					dsfi.Update();
					if (dsfi.IsDisposed)
					{
						i -= 1;
					}
				}
			}
			if (Microphone.micList != null)
			{
				for (int i = 0; i < Microphone.micList.Count; i += 1)
				{
					Microphone.micList[i].CheckBuffer();
				}
			}

			__MediaPlayer.Update();
			if (ActiveSongChanged)
			{
				__MediaPlayer.OnActiveSongChanged();
				ActiveSongChanged = false;
			}
			if (MediaStateChanged)
			{
				__MediaPlayer.OnMediaStateChanged();
				MediaStateChanged = false;
			}

			if (TouchPanel.TouchDeviceExists)
			{
				TouchPanel.Update();
			}
		}

		#endregion
	}
}
