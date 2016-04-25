#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Microsoft.Xna.Framework
{
	public static class FrameworkDispatcher
	{
		#region Internal Variables

		internal static bool ActiveSongChanged;
		internal static bool MediaStateChanged;

		#endregion

		#region Public Methods

		public static void Update()
		{
			/* Updates the status of various framework components
			 * (such as power state and media), and raises related events.
			 */
			if (AudioDevice.ALDevice == null)
			{
				AudioDevice.Initialize();
			}
			else
			{
				/* This has to be in an 'else', otherwise we hit
				 * NoAudioHardwareException in the wrong place.
				 * -flibit
				 */

				// Checks and cleans instances, fires events accordingly
				AudioDevice.Update();
			}
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
