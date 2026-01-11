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
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class VideoPlayerTheora : IVideoPlayerImpl, IDisposable
	{
		#region Hardware-accelerated YUV -> RGBA

		private Effect shaderProgram;
		private IntPtr stateChangesPtr;
		private Texture2D[] yuvTextures = new Texture2D[3];
		private Viewport viewport;

		private static VertexPositionTexture[] vertices = new VertexPositionTexture[]
		{
			new VertexPositionTexture(
				new Vector3(-1.0f, -1.0f, 0.0f),
				new Vector2(0.0f, 1.0f)
			),
			new VertexPositionTexture(
				new Vector3(1.0f, -1.0f, 0.0f),
				new Vector2(1.0f, 1.0f)
			),
			new VertexPositionTexture(
				new Vector3(-1.0f, 1.0f, 0.0f),
				new Vector2(0.0f, 0.0f)
			),
			new VertexPositionTexture(
				new Vector3(1.0f, 1.0f, 0.0f),
				new Vector2(1.0f, 0.0f)
			)
		};
		private VertexBufferBinding vertBuffer;

		// Used to restore our previous GL state.
		private Texture[] oldTextures = new Texture[3];
		private SamplerState[] oldSamplers = new SamplerState[3];
		private RenderTargetBinding[] oldTargets;
		private VertexBufferBinding[] oldBuffers;
		private BlendState prevBlend;
		private DepthStencilState prevDepthStencil;
		private RasterizerState prevRasterizer;
		private Viewport prevViewport;
		private FNA3D.FNA3D_RenderTargetBinding[] nativeVideoTexture =
			new FNA3D.FNA3D_RenderTargetBinding[3];
		private FNA3D.FNA3D_RenderTargetBinding[] nativeOldTargets =
			new FNA3D.FNA3D_RenderTargetBinding[GraphicsDevice.MAX_RENDERTARGET_BINDINGS];

		private void GL_initialize()
		{
			// Load the YUV->RGBA Effect
			shaderProgram = new Effect(
				currentDevice,
				Resources.YUVToRGBAEffect
			);
			unsafe
			{
				stateChangesPtr = FNAPlatform.Malloc(
					sizeof(Effect.MOJOSHADER_effectStateChanges)
				);
			}

			// Allocate the vertex buffer
			vertBuffer = new VertexBufferBinding(
				new VertexBuffer(
					currentDevice,
					VertexPositionTexture.VertexDeclaration,
					4,
					BufferUsage.WriteOnly
				)
			);
			vertBuffer.VertexBuffer.SetData(vertices);
		}

		private void GL_dispose()
		{
			if (currentDevice == null)
			{
				// We never initialized to begin with...
				return;
			}
			currentDevice = null;

			// Delete the Effect
			if (shaderProgram != null)
			{
				shaderProgram.Dispose();
			}
			if (stateChangesPtr != IntPtr.Zero)
			{
				FNAPlatform.Free(stateChangesPtr);
			}

			// Delete the vertex buffer
			if (vertBuffer.VertexBuffer != null)
			{
				vertBuffer.VertexBuffer.Dispose();
			}

			// Delete the textures if they exist
			for (int i = 0; i < 3; i += 1)
			{
				if (yuvTextures[i] != null)
				{
					yuvTextures[i].Dispose();
				}
			}
		}

		private void GL_setupTextures(
			int yWidth,
			int yHeight,
			int uvWidth,
			int uvHeight
		) {
			// Allocate YUV GL textures
			for (int i = 0; i < 3; i += 1)
			{
				if (yuvTextures[i] != null)
				{
					yuvTextures[i].Dispose();
				}
			}
			yuvTextures[0] = new Texture2D(
				currentDevice,
				yWidth,
				yHeight,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[1] = new Texture2D(
				currentDevice,
				uvWidth,
				uvHeight,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[2] = new Texture2D(
				currentDevice,
				uvWidth,
				uvHeight,
				false,
				SurfaceFormat.Alpha8
			);

			// Precalculate the viewport
			viewport = new Viewport(0, 0, yWidth, yHeight);
		}

		private void GL_pushState()
		{
			// Begin the effect, flagging to restore previous state on end
			FNA3D.FNA3D_BeginPassRestore(
				currentDevice.GLDevice,
				shaderProgram.glEffect,
				stateChangesPtr
			);

			// Prep our samplers
			for (int i = 0; i < 3; i += 1)
			{
				oldTextures[i] = currentDevice.Textures[i];
				oldSamplers[i] = currentDevice.SamplerStates[i];
				currentDevice.Textures[i] = yuvTextures[i];
				currentDevice.SamplerStates[i] = SamplerState.LinearClamp;
			}

			// Prep buffers
			oldBuffers = currentDevice.GetVertexBuffers();
			currentDevice.SetVertexBuffers(vertBuffer);

			// Prep target bindings
			int oldTargetCount = currentDevice.GetRenderTargetsNoAllocEXT(null);
			Array.Resize(ref oldTargets, oldTargetCount);
			currentDevice.GetRenderTargetsNoAllocEXT(oldTargets);

			unsafe
			{
				fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &nativeVideoTexture[0])
				{
					GraphicsDevice.PrepareRenderTargetBindings(
						rt,
						videoTexture
					);
					FNA3D.FNA3D_SetRenderTargets(
						currentDevice.GLDevice,
						rt,
						videoTexture.Length,
						IntPtr.Zero,
						DepthFormat.None,
						0
					);
				}
			}

			// Prep render state
			prevBlend = currentDevice.BlendState;
			prevDepthStencil = currentDevice.DepthStencilState;
			prevRasterizer = currentDevice.RasterizerState;
			currentDevice.BlendState = BlendState.Opaque;
			currentDevice.DepthStencilState = DepthStencilState.None;
			currentDevice.RasterizerState = RasterizerState.CullNone;

			// Prep viewport
			prevViewport = currentDevice.Viewport;
			FNA3D.FNA3D_SetViewport(
				currentDevice.GLDevice,
				ref viewport.viewport
			);
		}

		private void GL_popState()
		{
			// End the effect, restoring the previous shader state
			FNA3D.FNA3D_EndPassRestore(
				currentDevice.GLDevice,
				shaderProgram.glEffect
			);

			// Restore GL state
			currentDevice.BlendState = prevBlend;
			currentDevice.DepthStencilState = prevDepthStencil;
			currentDevice.RasterizerState = prevRasterizer;
			prevBlend = null;
			prevDepthStencil = null;
			prevRasterizer = null;

			/* Restore targets using GLDevice directly.
			 * This prevents accidental clearing of previously bound targets.
			 */
			if (oldTargets == null || oldTargets.Length == 0)
			{
				FNA3D.FNA3D_SetRenderTargets(
					currentDevice.GLDevice,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					DepthFormat.None,
					0
				);
			}
			else
			{
				IRenderTarget oldTarget = oldTargets[0].RenderTarget as IRenderTarget;

				unsafe
				{
					fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &nativeOldTargets[0])
					{
						GraphicsDevice.PrepareRenderTargetBindings(
							rt,
							oldTargets
						);
						FNA3D.FNA3D_SetRenderTargets(
							currentDevice.GLDevice,
							rt,
							oldTargets.Length,
							oldTarget.DepthStencilBuffer,
							oldTarget.DepthStencilFormat,
							(byte) (oldTarget.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0) /* lol c# */
						);
					}
				}
			}
			oldTargets = null;

			// Set viewport AFTER setting targets!
			FNA3D.FNA3D_SetViewport(
				currentDevice.GLDevice,
				ref prevViewport.viewport
			);

			// Restore buffers
			currentDevice.SetVertexBuffers(oldBuffers);
			oldBuffers = null;

			// Restore samplers
			currentDevice.Textures.ignoreTargets = true;
			for (int i = 0; i < 3; i += 1)
			{
				/* The application may have set a texture ages
				 * ago, only to not unset after disposing. We
				 * have to avoid an ObjectDisposedException!
				 */
				if (oldTextures[i] == null || !oldTextures[i].IsDisposed)
				{
					currentDevice.Textures[i] = oldTextures[i];
				}
				currentDevice.SamplerStates[i] = oldSamplers[i];
				oldTextures[i] = null;
				oldSamplers[i] = null;
			}
			currentDevice.Textures.ignoreTargets = false;
		}

		#endregion

		#region Public Member Data: XNA VideoPlayer Implementation

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsLooped
		{
			get;
			set;
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
				UpdateVolume();
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
				UpdateVolume();
			}
		}

		#endregion

		#region Private Member Data: XNA VideoPlayer Implementation

		// We use this to update our PlayPosition.
		private Stopwatch timer;

		// Store this to optimize things on our end.
		private RenderTargetBinding[] videoTexture;

		// We need to access the GraphicsDevice frequently.
		private GraphicsDevice currentDevice;

		#endregion

		#region Private Member Data: Theorafile

		private IntPtr theora;
		private double fps;

		private IntPtr yuvData;
		private int yuvDataLen;
		private int currentFrame;

		private const int AUDIO_BUFFER_SIZE = 4096 * 2;
		private static readonly float[] audioData = new float[AUDIO_BUFFER_SIZE];
		private static GCHandle audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
		private IntPtr audioDataPtr = audioHandle.AddrOfPinnedObject();

		#endregion

		#region Private Member Data: Audio Stream

		private DynamicSoundEffectInstance audioStream;

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

		#region Private Methods: Audio Stream

		private void UpdateVolume()
		{
			if (audioStream == null)
			{
				return;
			}
			if (IsMuted)
			{
				audioStream.Volume = 0.0f;
			}
			else
			{
				/* FIXME: Works around MasterVolume only temporarily!
				 * We need to detach this source from the AL listener properties.
				 * -flibit
				 */
				audioStream.Volume = Volume * (1.0f / SoundEffect.MasterVolume);
			}
		}

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		public VideoPlayerTheora()
		{
			// Initialize public members.
			IsDisposed = false;
			IsLooped = false;
			IsMuted = false;
			State = MediaState.Stopped;
			Volume = 1.0f;

			// Initialize private members.
			timer = new Stopwatch();
			videoTexture = new RenderTargetBinding[1];
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			// Stop the VideoPlayer. This gets almost everything...
			Stop();

			// Destroy the other GL bits.
			GL_dispose();

			// Dispose the DynamicSoundEffectInstance
			if (audioStream != null)
			{
				audioStream.Dispose();
				audioStream = null;
			}

			// Dispose the Texture.
			if (videoTexture[0].RenderTarget != null)
			{
				videoTexture[0].RenderTarget.Dispose();
			}

			// Free the YUV buffer
			if (yuvData != IntPtr.Zero)
			{
				FNAPlatform.Free(yuvData);
				yuvData = IntPtr.Zero;
			}

			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref theora);
			}

			// Okay, we out.
			IsDisposed = true;
		}

		public Texture2D GetTexture()
		{
			checkDisposed();

			if (Video == null)
			{
				throw new InvalidOperationException();
			}

			// Be sure we can even get something from Theorafile...
			if (	State == MediaState.Stopped ||
				theora == IntPtr.Zero ||
				Theorafile.tf_hasvideo(theora) == 0	)
			{
				 // Screw it, give them the old one.
				return videoTexture[0].RenderTarget as Texture2D;
			}

			int thisFrame = (int) (timer.Elapsed.TotalMilliseconds / (1000.0 / fps));
			if (thisFrame > currentFrame)
			{
				// Only update the textures if we need to!
				if (Theorafile.tf_readvideo(
					theora,
					yuvData,
					thisFrame - currentFrame
				) == 1 || currentFrame == -1) {
					UpdateTexture();
				}
				currentFrame = thisFrame;
			}

			// Check for the end...
			bool ended = Theorafile.tf_eos(theora) == 1;
			if (audioStream != null)
			{
				ended &= audioStream.PendingBufferCount == 0;
			}
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

				// Kill whatever audio/video we've got
				if (audioStream != null)
				{
					audioStream.Stop();
					audioStream.Dispose();
					audioStream = null;
				}

				// Reset the stream no matter what happens next
				Theorafile.tf_reset(theora);

				// If looping, go back to the start. Otherwise, we'll be exiting.
				if (IsLooped)
				{
					// Starting over!
					InitializeTheoraStream();

					// Start! Again!
					timer.Start();
					if (audioStream != null)
					{
						audioStream.Play();
					}
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
			if (theora != IntPtr.Zero)
			{
				Theorafile.tf_close(ref theora);
				theora = IntPtr.Zero;
			}

			// FIXME: This is a part of the Duration hack!
			if (Video.needsDurationHack)
			{
				Video.Duration = TimeSpan.MaxValue;
			}

			Theorafile.th_pixel_fmt fmt;
			int yWidth;
			int yHeight;
			int uvWidth;
			int uvHeight;

			Theorafile.tf_fopen(Video.handle, out theora);
			if (theora == IntPtr.Zero)
			{
				throw new System.IO.FileNotFoundException(Video.handle);
			}
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

			// Sanity checks for video metadata
			if (Video.Width != yWidth || Video.Height != yHeight)
			{
				throw new InvalidOperationException(
					"XNB/OGV width/height mismatch!" +
					" Width: " + Video.Width.ToString() +
					" Height: " + Video.Height.ToString()
				);
			}
			if (Math.Abs(Video.FramesPerSecond - fps) >= 1.0f)
			{
				throw new InvalidOperationException(
					"XNB/OGV framesPerSecond mismatch!" +
					" FPS: " + Video.FramesPerSecond.ToString()
				);
			}

			// Per-video track settings should always take priority
			if (Video.audioTrack >= 0)
			{
				SetAudioTrackEXT(Video.audioTrack);
			}
			if (Video.videoTrack >= 0)
			{
				SetVideoTrackEXT(Video.videoTrack);
			}

			// Check the player state before attempting anything.
			if (State != MediaState.Stopped)
			{
				return;
			}

			// Update the player state now, before initializing
			State = MediaState.Playing;

			// Carve out YUV buffer before doing any decoder work
			if (yuvData != IntPtr.Zero)
			{
				FNAPlatform.Free(yuvData);
			}
			yuvDataLen = (
				(yWidth * yHeight) +
				(uvWidth * uvHeight * 2)
			);
			yuvData = FNAPlatform.Malloc(yuvDataLen);

			// Hook up the decoder to this player
			InitializeTheoraStream();

			// Set up the texture data
			if (Theorafile.tf_hasvideo(theora) == 1)
			{
				// The VideoPlayer will use the GraphicsDevice that is set now.
				if (currentDevice != Video.GraphicsDevice)
				{
					GL_dispose();
					currentDevice = Video.GraphicsDevice;
					GL_initialize();
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
					uvHeight
				);
			}

			// The player can finally start now!
			timer.Start();
			if (audioStream != null)
			{
				audioStream.Play();
			}
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
			if (audioStream != null)
			{
				audioStream.Stop();
				audioStream.Dispose();
				audioStream = null;
			}
			Theorafile.tf_reset(theora);
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

			// Pause timer, audio.
			timer.Stop();
			if (audioStream != null)
			{
				audioStream.Pause();
			}
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

			// Unpause timer, audio.
			timer.Start();
			if (audioStream != null)
			{
				audioStream.Resume();
			}
		}

		#endregion

		#region Public Extensions

		// FIXME: Maybe store these to carry over to future videos?

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

		#region Private Theora Audio Stream Methods

		private void OnBufferRequest(object sender, EventArgs args)
		{
			int samples = Theorafile.tf_readaudio(
				theora,
				audioDataPtr,
				AUDIO_BUFFER_SIZE
			);
			if (samples > 0)
			{
				audioStream.SubmitFloatBufferEXT(
					audioData,
					0,
					samples
				);
			}
			else if (Theorafile.tf_eos(theora) == 1)
			{
				// Okay, we ran out. No need for this!
				audioStream.BufferNeeded -= OnBufferRequest;
			}
		}

		#endregion

		#region Private Theora Video Stream Methods

		private void UpdateTexture()
		{
			// Prepare YUV GL textures with our current frame data
			FNA3D.FNA3D_SetTextureDataYUV(
				currentDevice.GLDevice,
				yuvTextures[0].texture,
				yuvTextures[1].texture,
				yuvTextures[2].texture,
				yuvTextures[0].Width,
				yuvTextures[0].Height,
				yuvTextures[1].Width,
				yuvTextures[1].Height,
				yuvData,
				yuvDataLen
			);

			// Draw the YUV textures to the framebuffer with our shader.
			GL_pushState();
			currentDevice.DrawPrimitives(
				PrimitiveType.TriangleStrip,
				0,
				2
			);
			GL_popState();
		}

		#endregion

		#region Theora Decoder Hookup Method

		private void InitializeTheoraStream()
		{
			// Grab the first video frame ASAP.
			while (Theorafile.tf_readvideo(theora, yuvData, 1) == 0);

			// Grab the first bit of audio. We're trying to start the decoding ASAP.
			if (Theorafile.tf_hasaudio(theora) == 1)
			{
				int channels, samplerate;
				Theorafile.tf_audioinfo(theora, out channels, out samplerate);
				audioStream = new DynamicSoundEffectInstance(
					samplerate,
					(AudioChannels) channels
				);
				audioStream.BufferNeeded += OnBufferRequest;
				UpdateVolume();

				// Fill and queue the buffers.
				for (int i = 0; i < 4; i += 1)
				{
					OnBufferRequest(audioStream, EventArgs.Empty);
					if (audioStream.PendingBufferCount == i)
					{
						break;
					}
				}
			}

			currentFrame = -1;
		}

		#endregion


		#region Video support methods

		internal static VideoPlayer.VideoInfo ReadInfo(string fileName)
		{
			IntPtr theora;
			int width, height;
			double fps;
			Theorafile.th_pixel_fmt fmt;
			Theorafile.tf_fopen(fileName, out theora);
			Theorafile.tf_videoinfo(
				theora,
				out width,
				out height,
				out fps,
				out fmt
			);
			Theorafile.tf_close(ref theora);

			return new VideoPlayer.VideoInfo() { fps = fps, width = width, height = height };
		}

		#endregion
	}
}
