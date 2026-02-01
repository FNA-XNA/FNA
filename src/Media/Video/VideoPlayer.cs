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
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class VideoPlayer : IDisposable
	{
		#region Public Member Data: XNA VideoPlayer Implementation

		public bool IsDisposed
		{
			get;
			private set;
		}

		private bool backing_islooped;
		public bool IsLooped
		{
			get
			{
				return backing_islooped;
			}
			set
			{
				backing_islooped = value;
				if (impl != null)
				{
					impl.IsLooped = value;
				}
			}
		}

		private bool backing_ismuted;
		public bool IsMuted
		{
			get
			{
				return backing_ismuted;
			}
			set
			{
				backing_ismuted = value;
				if (impl != null)
				{
					impl.IsMuted = value;
				}
			}
		}

		public TimeSpan PlayPosition
		{
			get
			{
				return impl != null ? impl.PlayPosition : TimeSpan.Zero;
			}
		}

		public MediaState State
		{
			get
			{
				return impl != null ? impl.State : MediaState.Stopped;
			}
		}

		public Video Video
		{
			get
			{
				return impl != null ? impl.Video : null;
			}
		}

		private float backing_volume;
		public float Volume
		{
			get
			{
				return backing_volume;
			}
			set
			{
				if (value > 1.0f)
				{
					backing_volume = 1.0f;
				}
				else if (value < 0.0f)
				{
					backing_volume = 0.0f;
				}
				else
				{
					backing_volume = value;
				}

				if (impl != null)
				{
					impl.Volume = backing_volume;
				}
			}
		}

		#endregion

		#region Private Member Data: Implementation

		private IVideoPlayerCodec impl;

		internal static Dictionary<string, string> codecExtensions =
			new Dictionary<string, string>
			{
				{ "obu", "AV1" },
				{ "av1", "AV1" },
				{ "ogv", "Theora" }
			};

		internal static Dictionary<string, Func<string, VideoInfo>> codecInfoReaders =
			new Dictionary<string, Func<string, VideoInfo>>
			{
				{ "AV1", VideoPlayerAV1.ReadInfo },
				{ "Theora", VideoPlayerTheora.ReadInfo }
			};

		internal static Dictionary<string, Func<IVideoPlayerCodec>> codecPlayers =
			new Dictionary<string, Func<IVideoPlayerCodec>>
			{
				{ "AV1", () => new VideoPlayerAV1() },
				{ "Theora", () => new VideoPlayerTheora() },
			};

		#endregion

		#region Private Methods: XNA VideoPlayer Implementation

		private void checkDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("VideoPlayer");
			}
		}

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		public VideoPlayer()
		{
			// Initialize public members.
			IsDisposed = false;
			IsLooped = false;
			IsMuted = false;
			Volume = 1.0f;
		}

		public void Dispose()
		{
			checkDisposed();

			if (impl != null)
			{
				impl.Dispose();
			}

			impl = null;
			IsDisposed = true;
		}

		public Texture2D GetTexture()
		{
			checkDisposed();

			return impl.GetTexture();
		}

		public void Play(Video video)
		{
			checkDisposed();

			if (impl != null)
			{
				impl.Dispose();
				impl = null;
			}

			impl = codecPlayers[video.Codec]();
			impl.IsLooped = IsLooped;
			impl.IsMuted = IsMuted;
			impl.Volume = Volume;
			impl.Play(video);
		}

		public void Stop()
		{
			checkDisposed();

			if (impl != null)
			{
				impl.Stop();
			}
		}

		public void Pause()
		{
			checkDisposed();

			if (impl != null)
			{
				impl.Pause();
			}
		}

		public void Resume()
		{
			checkDisposed();

			if (impl != null)
			{
				impl.Resume();
			}
		}

		#endregion

		#region Public Extensions

		// FIXME: Maybe store these to carry over to future videos?

		public void SetAudioTrackEXT(int track)
		{
			if (impl != null)
			{
				impl.SetAudioTrackEXT(track);
			}
		}

		public void SetVideoTrackEXT(int track)
		{
			if (impl != null)
			{
				impl.SetVideoTrackEXT(track);
			}
		}

		#endregion

		#region Structs for Internal Use

		public struct VideoInfo
		{
			public int width;
			public int height;
			public double fps;
		}

		#endregion
	}
}
