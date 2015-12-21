#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class VideoPlayer : IDisposable
	{
		#region Hardware-accelerated YUV -> RGBA

		private Effect shaderProgram;
		private MojoShader.MOJOSHADER_effectStateChanges changes = new MojoShader.MOJOSHADER_effectStateChanges();
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
		private Texture[] oldTextures= new Texture[3];
		private SamplerState[] oldSamplers = new SamplerState[3];
		private RenderTargetBinding[] oldTargets;
		private VertexBufferBinding[] oldBuffers;
		private BlendState prevBlend;
		private DepthStencilState prevDepthStencil;
		private RasterizerState prevRasterizer;
		private Viewport prevViewport;

		private void GL_initialize()
		{
			// Load the YUV->RGBA Effect
			shaderProgram = new Effect(
				currentDevice,
				Resources.YUVToRGBAEffect
			);

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
			// Delete the Effect
			shaderProgram.Dispose();

			// Delete the vertex buffer
			vertBuffer.VertexBuffer.Dispose();

			// Delete the textures if they exist
			for (int i = 0; i < 3; i += 1)
			{
				if (yuvTextures[i] != null)
				{
					yuvTextures[i].Dispose();
				}
			}
		}

		private void GL_setupTextures(int width, int height)
		{
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
				width,
				height,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[1] = new Texture2D(
				currentDevice,
				width / 2,
				height / 2,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[2] = new Texture2D(
				currentDevice,
				width / 2,
				height / 2,
				false,
				SurfaceFormat.Alpha8
			);

			// Precalculate the viewport
			viewport = new Viewport(0, 0, width, height);
		}

		private void GL_pushState()
		{
			// Begin the effect, flagging to restore previous state on end
			currentDevice.GLDevice.BeginPassRestore(shaderProgram.glEffect, ref changes);

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
			oldTargets = currentDevice.GetRenderTargets();
			currentDevice.GLDevice.SetRenderTargets(
				videoTexture,
				null,
				DepthFormat.None
			);

			// Prep render state
			prevBlend = currentDevice.BlendState;
			prevDepthStencil = currentDevice.DepthStencilState;
			prevRasterizer = currentDevice.RasterizerState;
			currentDevice.BlendState = BlendState.Opaque;
			currentDevice.DepthStencilState = DepthStencilState.None;
			currentDevice.RasterizerState = RasterizerState.CullNone;

			// Prep viewport
			prevViewport = currentDevice.Viewport;
			currentDevice.GLDevice.SetViewport(viewport, true);
		}

		private void GL_popState()
		{
			// End the effect, restoring the previous shader state
			currentDevice.GLDevice.EndPassRestore(shaderProgram.glEffect);

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
				currentDevice.GLDevice.SetRenderTargets(
					null,
					null,
					DepthFormat.None
				);
			}
			else
			{
				IRenderTarget oldTarget = oldTargets[0].RenderTarget as IRenderTarget;
				currentDevice.GLDevice.SetRenderTargets(
					oldTargets,
					oldTarget.DepthStencilBuffer,
					oldTarget.DepthStencilFormat
				);
			}
			oldTargets = null;

			// Set viewport AFTER setting targets!
			currentDevice.GLDevice.SetViewport(
				prevViewport,
				currentDevice.RenderTargetCount > 0
			);

			// Restore buffers
			currentDevice.SetVertexBuffers(oldBuffers);
			oldBuffers = null;

			// Restore samplers
			for (int i = 0; i < 3; i += 1)
			{
				currentDevice.Textures[i] = oldTextures[i];
				currentDevice.SamplerStates[i] = oldSamplers[i];
				oldTextures[i] = null;
				oldSamplers[i] = null;
			}
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

		#region Private Member Data: TheoraPlay

		// Grabbed from the Video streams.
		private TheoraPlay.THEORAPLAY_VideoFrame currentVideo;
		private TheoraPlay.THEORAPLAY_VideoFrame nextVideo;
		private IntPtr previousFrame;

		#endregion

		#region Private Member Data: OpenAL

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

		#region Private Methods: OpenAL

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
				 * Figure out how MasterVolume actually applies to instances,
				 * then deal with this accordingly.
				 * -flibit
				 */
				audioStream.Volume = Volume * (1.0f / SoundEffect.MasterVolume);
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
			State = MediaState.Stopped;
			Volume = 1.0f;

			// Initialize private members.
			timer = new Stopwatch();

			// The VideoPlayer will use the GraphicsDevice that is set now.
			currentDevice = Game.Instance.GraphicsDevice;

			// Initialize this here to prevent null GetTexture returns.
			videoTexture = new RenderTargetBinding[1];
			videoTexture[0] = new RenderTargetBinding(
				new RenderTarget2D(
					currentDevice,
					1280,
					720,
					false,
					SurfaceFormat.Color,
					DepthFormat.None,
					0,
					RenderTargetUsage.PreserveContents
				)
			);

			// Initialize the other GL bits.
			GL_initialize();
		}

		public void Dispose()
		{
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
			videoTexture[0].RenderTarget.Dispose();

			// Okay, we out.
			IsDisposed = true;
		}

		public Texture2D GetTexture()
		{
			checkDisposed();

			// Be sure we can even get something from TheoraPlay...
			if (	State == MediaState.Stopped ||
				Video.theoraDecoder == IntPtr.Zero ||
				TheoraPlay.THEORAPLAY_isInitialized(Video.theoraDecoder) == 0 ||
				TheoraPlay.THEORAPLAY_hasVideoStream(Video.theoraDecoder) == 0	)
			{
				 // Screw it, give them the old one.
				return videoTexture[0].RenderTarget as Texture2D;
			}

			// Get the latest video frames.
			bool missedFrame = false;
			while (nextVideo.playms <= timer.ElapsedMilliseconds && !missedFrame)
			{
				currentVideo = nextVideo;
				IntPtr nextFrame = TheoraPlay.THEORAPLAY_getVideo(Video.theoraDecoder);
				if (nextFrame != IntPtr.Zero)
				{
					TheoraPlay.THEORAPLAY_freeVideo(previousFrame);
					previousFrame = Video.videoStream;
					Video.videoStream = nextFrame;
					nextVideo = TheoraPlay.getVideoFrame(Video.videoStream);
					missedFrame = false;
				}
				else
				{
					// Don't mind me, just ignoring that complete failure above!
					missedFrame = true;
				}

				if (TheoraPlay.THEORAPLAY_isDecoding(Video.theoraDecoder) == 0)
				{
					// FIXME: This is part of the Duration hack!
					Video.Duration = new TimeSpan(0, 0, 0, 0, (int) currentVideo.playms);

					// Stop and reset the timer. If we're looping, the loop will start it again.
					timer.Stop();
					timer.Reset();

					// If looping, go back to the start. Otherwise, we'll be exiting.
					if (IsLooped && State == MediaState.Playing)
					{
						// Kill the audio, no matter what.
						if (audioStream != null)
						{
							audioStream.Stop();
							audioStream.Dispose();
							audioStream = null;
						}

						// Free everything and start over.
						TheoraPlay.THEORAPLAY_freeVideo(previousFrame);
						previousFrame = IntPtr.Zero;
						Video.AttachedToPlayer = false;
						Video.Dispose();
						Video.AttachedToPlayer = true;
						Video.Initialize();

						// Grab the initial audio again.
						if (TheoraPlay.THEORAPLAY_hasAudioStream(Video.theoraDecoder) != 0)
						{
							InitAudioStream();
						}

						// Grab the initial video again.
						if (TheoraPlay.THEORAPLAY_hasVideoStream(Video.theoraDecoder) != 0)
						{
							currentVideo = TheoraPlay.getVideoFrame(Video.videoStream);
							previousFrame = Video.videoStream;
							do
							{
								// The decoder miiight not be ready yet.
								Video.videoStream = TheoraPlay.THEORAPLAY_getVideo(Video.theoraDecoder);
							} while (Video.videoStream == IntPtr.Zero);
							nextVideo = TheoraPlay.getVideoFrame(Video.videoStream);
						}

						// Start! Again!
						timer.Start();
						if (audioStream != null)
						{
							audioStream.Play();
						}
					}
					else
					{
						// Stop everything, clean up. We out.
						State = MediaState.Stopped;
						if (audioStream != null)
						{
							audioStream.Stop();
							audioStream.Dispose();
							audioStream = null;
						}
						TheoraPlay.THEORAPLAY_freeVideo(previousFrame);
						Video.AttachedToPlayer = false;
						Video.Dispose();

						// We're done, so give them the last frame.
						return videoTexture[0].RenderTarget as Texture2D;
					}
				}
			}

			// Set up an environment to muck about in.
			GL_pushState();

			// Prepare YUV GL textures with our current frame data
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[0],
				currentVideo.pixels
			);
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[1],
				new IntPtr(
					currentVideo.pixels.ToInt64() +
					(currentVideo.width * currentVideo.height)
				)
			);
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[2],
				new IntPtr(
					currentVideo.pixels.ToInt64() +
					(currentVideo.width * currentVideo.height) +
					(currentVideo.width / 2 * currentVideo.height / 2)
				)
			);

			// Draw the YUV textures to the framebuffer with our shader.
			currentDevice.DrawPrimitives(
				PrimitiveType.TriangleStrip,
				0,
				2
			);

			// Clean up after ourselves.
			GL_popState();

			// Finally.
			return videoTexture[0].RenderTarget as Texture2D;
		}

		public void Play(Video video)
		{
			checkDisposed();

			// We need to assign this regardless of what happens next.
			Video = video;
			video.AttachedToPlayer = true;

			// FIXME: This is a part of the Duration hack!
			Video.Duration = TimeSpan.MaxValue;

			// Check the player state before attempting anything.
			if (State != MediaState.Stopped)
			{
				return;
			}

			// Update the player state now, for the thread we're about to make.
			State = MediaState.Playing;

			// Start the video if it hasn't been yet.
			if (Video.IsDisposed)
			{
				video.Initialize();
			}

			// Grab the first bit of audio. We're trying to start the decoding ASAP.
			if (TheoraPlay.THEORAPLAY_hasAudioStream(Video.theoraDecoder) != 0)
			{
				InitAudioStream();
			}

			// Grab the first bit of video, set up the texture.
			if (TheoraPlay.THEORAPLAY_hasVideoStream(Video.theoraDecoder) != 0)
			{
				currentVideo = TheoraPlay.getVideoFrame(Video.videoStream);
				previousFrame = Video.videoStream;
				do
				{
					// The decoder miiight not be ready yet.
					Video.videoStream = TheoraPlay.THEORAPLAY_getVideo(Video.theoraDecoder);
				} while (Video.videoStream == IntPtr.Zero);
				nextVideo = TheoraPlay.getVideoFrame(Video.videoStream);

				RenderTargetBinding overlap = videoTexture[0];
				videoTexture[0] = new RenderTargetBinding(
					new RenderTarget2D(
						currentDevice,
						(int) currentVideo.width,
						(int) currentVideo.height,
						false,
						SurfaceFormat.Color,
						DepthFormat.None,
						0,
						RenderTargetUsage.PreserveContents
					)
				);
				overlap.RenderTarget.Dispose();
				GL_setupTextures(
					(int) currentVideo.width,
					(int) currentVideo.height
				);
			}

			// Initialize the thread!
			System.Console.Write("Starting Theora player...");
			timer.Start();
			if (audioStream != null)
			{
				audioStream.Play();
			}
			System.Console.WriteLine(" Done!");
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
			System.Console.Write("Signaled Theora player to stop, waiting...");
			timer.Stop();
			timer.Reset();
			if (audioStream != null)
			{
				audioStream.Stop();
				audioStream.Dispose();
				audioStream = null;
			}
			if (previousFrame != IntPtr.Zero)
			{
				TheoraPlay.THEORAPLAY_freeVideo(previousFrame);
			}
			Video.AttachedToPlayer = false;
			Video.Dispose();
			System.Console.WriteLine(" Done!");
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

		#region Private Theora Audio Stream Methods

		private bool StreamAudio()
		{
			// The size of our abstracted buffer.
			const int BUFFER_SIZE = 4096 * 2;

			// Store our abstracted buffer into here.
			List<float> data = new List<float>();

			// We'll store this here, so alBufferData can use it too.
			TheoraPlay.THEORAPLAY_AudioPacket currentAudio;
			currentAudio.channels = 0;
			currentAudio.freq = 0;

			// There might be an initial period of silence, so forcibly push through.
			while (	audioStream.State == SoundState.Stopped &&
				TheoraPlay.THEORAPLAY_availableAudio(Video.theoraDecoder) == 0	);

			// Add to the buffer from the decoder until it's large enough.
			while (	data.Count < BUFFER_SIZE &&
				TheoraPlay.THEORAPLAY_availableAudio(Video.theoraDecoder) > 0	)
			{
				IntPtr audioPtr = TheoraPlay.THEORAPLAY_getAudio(Video.theoraDecoder);
				currentAudio = TheoraPlay.getAudioPacket(audioPtr);
				data.AddRange(
					TheoraPlay.getSamples(
						currentAudio.samples,
						currentAudio.frames * currentAudio.channels
					)
				);
				TheoraPlay.THEORAPLAY_freeAudio(audioPtr);
			}

			// If we actually got data, buffer it into OpenAL.
			if (data.Count > 0)
			{
				audioStream.SubmitFloatBufferEXT(data.ToArray());
				return true;
			}
			return false;
		}

		private void OnBufferRequest(object sender, EventArgs args)
		{
			if (!StreamAudio())
			{
				// Okay, we ran out. No need for this!
				audioStream.BufferNeeded -= OnBufferRequest;
			}
		}

		private void InitAudioStream()
		{
			// The number of buffers to queue into the source.
			const int NUM_BUFFERS = 4;

			// Generate the source.
			IntPtr audioPtr = IntPtr.Zero;
			do
			{
				audioPtr = TheoraPlay.THEORAPLAY_getAudio(Video.theoraDecoder);
			} while (audioPtr == IntPtr.Zero);
			TheoraPlay.THEORAPLAY_AudioPacket packet = TheoraPlay.getAudioPacket(audioPtr);
			audioStream = new DynamicSoundEffectInstance(
				packet.freq,
				(AudioChannels) packet.channels
			);
			audioStream.BufferNeeded += OnBufferRequest;
			UpdateVolume();

			// Fill and queue the buffers.
			for (int i = 0; i < NUM_BUFFERS; i += 1)
			{
				if (!StreamAudio())
				{
					break;
				}
			}
		}

		#endregion
	}
}
