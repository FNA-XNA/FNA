using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Dav1dfile;
using SDL3;

namespace Microsoft.Xna.Framework.Media
{
	public unsafe class VideoPlayerAV1 : BaseYUVPlayer, IVideoPlayerCodec, IDisposable
	{
		#region Public Member Data: XNA VideoPlayer Implementation

		public bool IsLooped
		{
			get;
			set;
		}

		public bool IsMuted
		{
			get
			{
				return false;
			}
			set
			{
				// no-op
			}
		}

		public TimeSpan PlayPosition
		{
			get
			{
				return timer.Elapsed;
			}
		}

		public MediaState State
		{
			get;
			private set;
		}

		public Video Video
		{
			get;
			private set;
		}

		public float Volume
		{
			get
			{
				return 0.0f;
			}
			set
			{
				// no-op
			}
		}

		#endregion

		#region Private Member Data: XNA VideoPlayer Implementation

		// We use this to update our PlayPosition.
		private Stopwatch timer;

		#endregion

		#region Private Member Data: dav1dfile

		private IntPtr context;
		private Bindings.PixelLayout pixelLayout;
		private int bitsPerPixel;

		private int currentFrame;
		private double fps;

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		public VideoPlayerAV1()
		{
			// Initialize public members.
			IsLooped = false;
			IsMuted = true;
			State = MediaState.Stopped;
			Volume = 0.0f;

			// Initialize private members.
			timer = new Stopwatch();
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			// Stop the VideoPlayer. This gets almost everything...
			Stop();

			// Destroy the underlying GL bits.
			base.Dispose();

			if (context != IntPtr.Zero)
			{
				Bindings.df_close(context);
			}
		}

		public Texture2D GetTexture()
		{
			checkDisposed();

			if (Video == null)
			{
				throw new InvalidOperationException();
			}

			// Be sure we can even get something...
			if (	State == MediaState.Stopped ||
				    context == IntPtr.Zero )
			{
				// Screw it, give them the old one.
				return videoTexture[0].RenderTarget as Texture2D;
			}

			int thisFrame = (int) (timer.Elapsed.TotalMilliseconds / (1000.0 / fps));
			if (thisFrame > currentFrame)
			{
				// Only update the textures if we need to!
				if (DecodeAndUpdateFrame(thisFrame - currentFrame) || currentFrame == -1)
				{
					float rescaleFactor;
					if (bitsPerPixel == 12)
					{
						rescaleFactor = (float) (1.0 / (4096 / 65536.0));
					}
					else if (bitsPerPixel == 10)
					{
						rescaleFactor = (float) (1.0 / (1024 / 65536.0));
					}
					else
					{
						rescaleFactor = 1.0f;
					}

					shaderProgram.Parameters["RescaleFactor"]
						.SetValue(new Vector4(rescaleFactor, rescaleFactor, rescaleFactor, 1.0f));

					// Draw the YUV textures to the framebuffer with our shader.
					GL_pushState();
					currentDevice.DrawPrimitives(
						PrimitiveType.TriangleStrip,
						0,
						2
					);
					GL_popState();
				}
				currentFrame = thisFrame;
			}

			// Check for the end...
			bool ended = Bindings.df_eos(context) == 1;
			if (ended)
			{
				// FIXME: This is part of the Duration hack!
				if (Video.needsDurationHack)
				{
					Video.Duration = timer.Elapsed; // FIXME: Frames * FPS? -flibit
				}

				// Stop and reset the timer. If we're looping, the loop will start it again.
				timer.Stop();
				timer.Reset();

				// Reset the stream no matter what happens next
				Bindings.df_reset(context);

				// If looping, go back to the start. Otherwise, we'll be exiting.
				if (IsLooped)
				{
					// Starting over!
					currentFrame = -1;

					// Start! Again!
					timer.Start();
				}
				else
				{
					// We out
					State = MediaState.Stopped;
				}
			}

			// Finally.
			return videoTexture[0].RenderTarget as Texture2D;
		}

		public void Play(Video video)
		{
			checkDisposed();

			// We need to assign this regardless of what happens next.
			Video = video;
			Video.parent = this; // FIXME: Remove this when extension is replaced!!!

			// Again, no matter what happens this should be cleared!
			if (context != IntPtr.Zero)
			{
				Bindings.df_close(context);
				context = IntPtr.Zero;
			}

			// FIXME: This is a part of the Duration hack!
			if (Video.needsDurationHack)
			{
				Video.Duration = TimeSpan.MaxValue;
			}

			int ok = Bindings.df_fopen(Video.handle, out context);
			if (context == IntPtr.Zero || ok == 0)
			{
				throw new System.IO.FileNotFoundException(Video.handle);
			}

			int yWidth, yHeight;
			Bindings.PixelLayout layout;
			try
			{
				byte hbd;
				Bindings.df_videoinfo2(context, out yWidth, out yHeight, out layout, out hbd, out fps);
				if (hbd == 2)
				{
					bitsPerPixel = 12;
				}
				else if (hbd == 1)
				{
					bitsPerPixel = 10;
				}
				else
				{
					bitsPerPixel = 8;
				}
			} catch {
				Bindings.df_videoinfo(context, out yWidth, out yHeight, out layout);
				bitsPerPixel = 8;
				fps = 0;
			}

			int uvWidth, uvHeight;

			switch (layout) {
				case Bindings.PixelLayout.I420:
					uvWidth = yWidth / 2;
					uvHeight = yHeight / 2;
					break;
				case Bindings.PixelLayout.I422:
					uvWidth = yWidth / 2;
					uvHeight = yHeight;
					break;
				case Bindings.PixelLayout.I444:
					uvWidth = yWidth;
					uvHeight = yHeight;
					break;
				default:
					throw new NotSupportedException("Unsupported pixel layout in AV1 file");
			}

			// Sanity checks for video metadata
			if (Video.Width != yWidth || Video.Height != yHeight)
			{
				throw new InvalidOperationException(
					"XNB/OGV width/height mismatch!" +
					" Width: " + Video.Width.ToString() +
					" Height: " + Video.Height.ToString()
				);
			}

			if (fps == 0)
			{
				if (Video.FramesPerSecond == 0)
				{
					throw new InvalidOperationException("Framerate not present in header or manually specified");
				}
				fps = Video.FramesPerSecond;
			}
			else if (Math.Abs(Video.FramesPerSecond - fps) >= 1.0f)
			{
				throw new InvalidOperationException(
					"XNB/OGV framesPerSecond mismatch!" +
					" FPS: " + Video.FramesPerSecond.ToString()
				);
			}

			// Check the player state before attempting anything.
			if (State != MediaState.Stopped)
			{
				return;
			}

			// Update the player state now, before initializing
			State = MediaState.Playing;

			currentFrame = -1;

			// Set up the texture data
			// The VideoPlayer will use the GraphicsDevice that is set now.
			if (currentDevice != Video.GraphicsDevice)
			{
				GL_dispose();
				currentDevice = Video.GraphicsDevice;
				GL_initialize(Resources.YUVToRGBAEffectR);
			}

			RenderTargetBinding overlap = videoTexture[0];
			videoTexture[0] = new RenderTargetBinding(
				new RenderTarget2D(
					currentDevice,
					yWidth,
					yHeight,
					false,
					SurfaceFormat.Color,
					DepthFormat.None,
					0,
					RenderTargetUsage.PreserveContents
				)
			);
			if (overlap.RenderTarget != null)
			{
				overlap.RenderTarget.Dispose();
			}
			GL_setupTextures(
				yWidth,
				yHeight,
				uvWidth,
				uvHeight,
				bitsPerPixel > 8 ? SurfaceFormat.UShortEXT : SurfaceFormat.ByteEXT
			);

			// The player can finally start now!
			timer.Start();
		}

        public void Stop()
        {
	        checkDisposed();

	        // Check the player state before attempting anything.
	        if (State == MediaState.Stopped)
	        {
		        return;
	        }

	        // Update the player state.
	        State = MediaState.Stopped;

	        // Wait for the player to end if it's still going.
	        timer.Stop();
	        timer.Reset();
	        Bindings.df_reset(context);
        }

        public void Pause()
        {
	        checkDisposed();

	        // Check the player state before attempting anything.
	        if (State != MediaState.Playing)
	        {
		        return;
	        }

	        // Update the player state.
	        State = MediaState.Paused;

	        // Pause timer
	        timer.Stop();
        }

        public void Resume()
        {
	        checkDisposed();

	        // Check the player state before attempting anything.
	        if (State != MediaState.Paused)
	        {
		        return;
	        }

	        // Update the player state.
	        State = MediaState.Playing;

	        // Unpause timer
	        timer.Start();
        }

        #endregion

        #region Public Extensions

        // FIXME: Maybe store these to carry over to future videos?

        public void SetAudioTrackEXT(int track)
        {
	        // not available?
        }

        public void SetVideoTrackEXT(int track)
        {
	        // not available?
        }

        #endregion

        #region Private AV1 Video Stream Methods

        private bool DecodeAndUpdateFrame(int frameCount = 1)
		{
			IntPtr yData, uData, vData;
			uint yLength, uvLength;
			uint yStride, uvStride;
			byte[] yScratchBuffer = null, uvScratchBuffer = null;

			int ok = Bindings.df_readvideo(
				context, frameCount,
				out yData, out uData, out vData,
				out yLength, out uvLength,
				out yStride, out uvStride
			);

			if (ok != 1)
			{
				return false;
			}

			UploadDataToTexture(yuvTextures[0], yData, yLength, yStride, ref yScratchBuffer);
			UploadDataToTexture(yuvTextures[1], uData, uvLength, uvStride, ref uvScratchBuffer);
			UploadDataToTexture(yuvTextures[2], vData, uvLength, uvStride, ref uvScratchBuffer);

			return true;
		}

		private void UploadDataToTexture(Texture2D texture, IntPtr data, uint length, uint stride, ref byte[] scratchBuffer)
		{
			int w = texture.Width, h = texture.Height,
				dataH = (int)(length / stride),
				availH = Math.Min(dataH, h),
				eltSize = bitsPerPixel > 8 ? 2 : 1,
				rowSize = eltSize * w;

			if (w == stride)
			{
				texture.SetDataPointerEXT(0, new Rectangle(0, 0, w, availH), data, (int)length);
				return;
			}

			Array.Resize(ref scratchBuffer, w * availH * eltSize);

			fixed (byte* scratch = scratchBuffer) {
				for (int y = 0; y < availH; y++)
				{
					SDL.SDL_memcpy((IntPtr)scratch + (rowSize * y), data + (int)(stride * y), (UIntPtr)rowSize);
				}
				texture.SetDataPointerEXT(0, null, (IntPtr)scratch, scratchBuffer.Length);
			}
		}

		#endregion

		#region Video support methods

		internal static VideoPlayer.VideoInfo ReadInfo(string fileName)
		{
			IntPtr context;
			int width, height;
			Bindings.PixelLayout pixelLayout;
			byte hbd;
			double fps;
			Bindings.df_fopen(fileName, out context);
			Bindings.df_videoinfo2(context, out width, out height, out pixelLayout, out hbd, out fps);
			Bindings.df_close(context);

			return new VideoPlayer.VideoInfo() { fps = fps, width = width, height = height };
		}

		#endregion
	}
}
