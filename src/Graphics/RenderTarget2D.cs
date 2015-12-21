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
		IGLRenderbuffer IRenderTarget.DepthStencilBuffer
		{
			get
			{
				return glDepthStencilBuffer;
			}
		}

		/// <inheritdoc/>
		IGLRenderbuffer IRenderTarget.ColorBuffer
		{
			get
			{
				return glColorBuffer;
			}
		}

		#endregion

		#region Private Variables

		private IGLRenderbuffer glDepthStencilBuffer;
		private IGLRenderbuffer glColorBuffer;

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
			MultiSampleCount = Math.Min(
				MathHelper.ClosestMSAAPower(preferredMultiSampleCount),
				graphicsDevice.GLDevice.MaxMultiSampleCount
			);
			RenderTargetUsage = usage;

			if (MultiSampleCount > 0)
			{
				glColorBuffer = graphicsDevice.GLDevice.GenRenderbuffer(
					width,
					height,
					Format,
					MultiSampleCount
				);
			}

			// If we don't need a depth buffer then we're done.
			if (preferredDepthFormat == DepthFormat.None)
			{
				return;
			}

			glDepthStencilBuffer = graphicsDevice.GLDevice.GenRenderbuffer(
				width,
				height,
				preferredDepthFormat,
				MultiSampleCount
			);
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (glDepthStencilBuffer != null)
				{
					GraphicsDevice.GLDevice.AddDisposeRenderbuffer(glDepthStencilBuffer);
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
