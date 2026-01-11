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
	public abstract class BaseYUVPlayer : IDisposable
	{
		#region Hardware-accelerated YUV -> RGBA

		protected Effect shaderProgram;
		private IntPtr stateChangesPtr;
		protected Texture2D[] yuvTextures = new Texture2D[3];
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

		protected void GL_initialize(byte[] shaderProgramBytes)
		{
			// Load the YUV->RGBA Effect
			shaderProgram = new Effect(
				currentDevice,
				shaderProgramBytes
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

		protected void GL_dispose()
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

		protected void GL_setupTextures(
			int yWidth,
			int yHeight,
			int uvWidth,
			int uvHeight,
			SurfaceFormat surfaceFormat
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
				surfaceFormat
			);
			yuvTextures[1] = new Texture2D(
				currentDevice,
				uvWidth,
				uvHeight,
				false,
				surfaceFormat
			);
			yuvTextures[2] = new Texture2D(
				currentDevice,
				uvWidth,
				uvHeight,
				false,
				surfaceFormat
			);

			// Precalculate the viewport
			viewport = new Viewport(0, 0, yWidth, yHeight);
		}

		protected void GL_pushState()
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

		protected void GL_popState()
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

		#endregion

		#region Private Member Data: XNA VideoPlayer Implementation

		// Store this to optimize things on our end.
		protected RenderTargetBinding[] videoTexture;

		// We need to access the GraphicsDevice frequently.
		protected GraphicsDevice currentDevice;

		#endregion

		#region Private Methods: XNA VideoPlayer Implementation

		protected void checkDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("VideoPlayer");
			}
		}

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		protected BaseYUVPlayer()
		{
			// Initialize public members.
			IsDisposed = false;

			// Initialize private members.
			videoTexture = new RenderTargetBinding[1];
		}

		public virtual void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			// Destroy the other GL bits.
			GL_dispose();

			// Dispose the Texture.
			if (videoTexture[0].RenderTarget != null)
			{
				videoTexture[0].RenderTarget.Dispose();
			}

			// Okay, we out.
			IsDisposed = true;
		}

		#endregion
	}
}
