#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public interface IVideoPlayerImpl : IDisposable
	{
		#region Public Member Data: XNA VideoPlayer Implementation

		bool IsLooped
		{
			get;
			set;
		}

		bool IsMuted
		{
			get;
			set;
		}

		TimeSpan PlayPosition
		{
			get;
		}

		MediaState State
		{
			get;
		}

		Video Video
		{
			get;
		}

		float Volume
		{
			get;
			set;
		}

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		Texture2D GetTexture();
		void Play(Video video);
		void Stop();
		void Pause();
		void Resume();
		void SetAudioTrackEXT(int track);
		void SetVideoTrackEXT(int track);

		#endregion
	}
}
