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
using System.IO;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Video
	{
		#region Public Properties

		public int Width
		{
			get
			{
				return yWidth;
			}
		}

		public int Height
		{
			get
			{
				return yHeight;
			}
		}

		public float FramesPerSecond
		{
			get
			{
				return (float) fps;
			}
		}

		public VideoSoundtrackType VideoSoundtrackType
		{
			get;
			private set;
		}

		// FIXME: This is hacked, look up "This is a part of the Duration hack!"
		public TimeSpan Duration
		{
			get;
			internal set;
		}

		#endregion

		#region Internal Properties

		internal GraphicsDevice GraphicsDevice
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables: Theorafile

		internal IntPtr theora;
		internal int yWidth;
		internal int yHeight;
		internal int uvWidth;
		internal int uvHeight;
		internal double fps;
		internal bool needsDurationHack;

		#endregion

		#region Internal Constructors

		internal Video(string fileName, GraphicsDevice device)
		{
			GraphicsDevice = device;

			Theorafile.th_pixel_fmt fmt;
			Theorafile.tf_fopen(fileName, out theora);
			Theorafile.tf_videoinfo(
				theora,
				out yWidth,
				out yHeight,
				out fps,
				out fmt
			);
			if (fmt == Theorafile.th_pixel_fmt.TH_PF_420)
			{
				uvWidth = yWidth / 2;
				uvHeight = yHeight / 2;
			}
			else if (fmt == Theorafile.th_pixel_fmt.TH_PF_422)
			{
				uvWidth = yWidth / 2;
				uvHeight = yHeight;
			}
			else if (fmt == Theorafile.th_pixel_fmt.TH_PF_444)
			{
				uvWidth = yWidth;
				uvHeight = yHeight;
			}
			else
			{
				throw new NotSupportedException(
					"Unrecognized YUV format!"
				);
			}

			// FIXME: This is a part of the Duration hack!
			Duration = TimeSpan.MaxValue;
			needsDurationHack = true;
		}

		internal Video(
			string fileName,
			GraphicsDevice device,
			int durationMS,
			int width,
			int height,
			float framesPerSecond,
			VideoSoundtrackType soundtrackType
		) : this(fileName, device) {
			/* If you got here, you've still got the XNB file! Well done!
			 * Except if you're running FNA, you're not using the WMV anymore.
			 * But surely it's the same video, right...?
			 * Well, consider this a check more than anything. If this bothers
			 * you, just remove the XNB file and we'll read the OGV straight up.
			 * -flibit
			 */
			if (width != Width || height != Height)
			{
				throw new InvalidOperationException(
					"XNB/OGV width/height mismatch!" +
					" Width: " + Width.ToString() +
					" Height: " + Height.ToString()
				);
			}
			if (Math.Abs(FramesPerSecond - framesPerSecond) >= 1.0f)
			{
				throw new InvalidOperationException(
					"XNB/OGV framesPerSecond mismatch!" +
					" FPS: " + FramesPerSecond.ToString()
				);
			}

			// FIXME: Oh, hey! I wish we had this info in Theora!
			Duration = TimeSpan.FromMilliseconds(durationMS);
			needsDurationHack = false;

			VideoSoundtrackType = soundtrackType;
		}

		#endregion

		#region Public Extensions
		
		public static Video FromUriEXT(Uri uri, GraphicsDevice graphicsDevice)
		{
			string path;
			if (uri.IsAbsoluteUri)
			{
				// If it's absolute, be sure we can actually get it...
				if (!uri.IsFile)
				{
					throw new InvalidOperationException(
						"Only local file URIs are supported for now"
					);
				}
				path = uri.LocalPath;
			}
			else
			{
				path = Path.Combine(
					TitleLocation.Path,
					uri.ToString()
				);
			}

			return new Video(path, graphicsDevice);
		}

		public void SetAudioTrackEXT(int track)
		{
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_setaudiotrack(theora, track);
			}
		}

		public void SetVideoTrackEXT(int track)
		{
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_setvideotrack(theora, track);
			}
		}

		#endregion

		#region Destructor

		~Video()
		{
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref theora);
			}
		}

		#endregion
	}
}
