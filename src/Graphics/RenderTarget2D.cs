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
using System.Threading;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class RenderTarget2D : Texture2D, IRenderTarget
	{
		#region Public Properties

		public DepthFormat DepthStencilFormat
		{
			get;
			private set;
		}

		public int MultiSampleCount
		{
			get;
			private set;
		}

		public RenderTargetUsage RenderTargetUsage
		{
			get;
			private set;
		}

		public bool IsContentLost
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IRenderTarget Properties

		/// <inheritdoc/>
		IntPtr IRenderTarget.DepthStencilBuffer
		{
			get
			{
				return glDepthStencilBuffer;
			}
		}

		/// <inheritdoc/>
		IntPtr IRenderTarget.ColorBuffer
		{
			get
			{
				return glColorBuffer;
			}
		}

		#endregion

		#region Private FNA3D Variables

		private IntPtr glDepthStencilBuffer;
		private IntPtr glColorBuffer;

		#endregion

		#region ContentLost Event

#pragma warning disable 0067
		// We never lose data, but lol XNA4 compliance -flibit
		public event EventHandler<EventArgs> ContentLost;
#pragma warning restore 0067

		#endregion

		#region Public Constructors

		public RenderTarget2D(
			GraphicsDevice graphicsDevice,
			int width,
			int height
		) : this(
			graphicsDevice,
			width,
			height,
			false,
			SurfaceFormat.Color,
			DepthFormat.None,
			0,
			RenderTargetUsage.DiscardContents
		) {
		}

		public RenderTarget2D(
			GraphicsDevice graphicsDevice,
			int width,
			int height,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat
		) : this(
			graphicsDevice,
			width,
			height,
			mipMap,
			preferredFormat,
			preferredDepthFormat,
			0,
			RenderTargetUsage.DiscardContents
		) {
		}

		public RenderTarget2D(
			GraphicsDevice graphicsDevice,
			int width,
			int height,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat,
			int preferredMultiSampleCount,
			RenderTargetUsage usage
		) : base(
			graphicsDevice,
			width,
			height,
			mipMap,
			preferredFormat
		) {
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
				graphicsDevice.GLDevice,
				Format,
				MathHelper.ClosestMSAAPower(preferredMultiSampleCount)
			);
			RenderTargetUsage = usage;

			if (MultiSampleCount > 0)
			{
				glColorBuffer = FNA3D.FNA3D_GenColorRenderbuffer(
					graphicsDevice.GLDevice,
					Width,
					Height,
					Format,
					MultiSampleCount,
					texture
				);
			}

			// If we don't need a depth buffer then we're done.
			if (DepthStencilFormat == DepthFormat.None)
			{
				return;
			}

			glDepthStencilBuffer = FNA3D.FNA3D_GenDepthStencilRenderbuffer(
				graphicsDevice.GLDevice,
				Width,
				Height,
				DepthStencilFormat,
				MultiSampleCount
			);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IntPtr toDispose = Interlocked.Exchange(ref glColorBuffer, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeRenderbuffer(
						GraphicsDevice.GLDevice,
						toDispose
					);
				}

				toDispose = Interlocked.Exchange(ref glDepthStencilBuffer, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeRenderbuffer(
						GraphicsDevice.GLDevice,
						toDispose
					);
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Internal Context Reset Method

		protected internal override void GraphicsDeviceResetting()
		{
			base.GraphicsDeviceResetting();
		}

		#endregion
	}
}
