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
	/// <summary>
	/// Represents a texture cube that can be used as a render target.
	/// </summary>
	public class RenderTargetCube : TextureCube, IRenderTarget
	{
		#region Public Properties

		/// <summary>
		/// Gets the depth-stencil buffer format of this render target.
		/// </summary>
		/// <value>The format of the depth-stencil buffer.</value>
		public DepthFormat DepthStencilFormat
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of multisample locations.
		/// </summary>
		/// <value>The number of multisample locations.</value>
		public int MultiSampleCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the usage mode of this render target.
		/// </summary>
		/// <value>The usage mode of the render target.</value>
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
		int IRenderTarget.Width
		{
			get
			{
				return Size;
			}
		}

		/// <inheritdoc/>
		int IRenderTarget.Height
		{
			get
			{
				return Size;
			}
		}

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

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <param name="size">The width and height of a texture cube face in pixels.</param>
		/// <param name="mipMap">
		/// <see langword="true"/> to generate a full mipmap chain; otherwise <see langword="false"/>.
		/// </param>
		/// <param name="preferredFormat">The preferred format of the surface.</param>
		/// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
		public RenderTargetCube(
			GraphicsDevice graphicsDevice,
			int size,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat
		) : this(
			graphicsDevice,
			size,
			mipMap,
			preferredFormat,
			preferredDepthFormat,
			0,
			RenderTargetUsage.DiscardContents
		) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <param name="size">The width and height of a texture cube face in pixels.</param>
		/// <param name="mipMap">
		/// <see langword="true"/> to generate a full mipmap chain; otherwise <see langword="false"/>.
		/// </param>
		/// <param name="preferredFormat">The preferred format of the surface.</param>
		/// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
		/// <param name="preferredMultiSampleCount">The preferred number of multisample locations.</param>
		/// <param name="usage">The usage mode of the render target.</param>
		public RenderTargetCube(
			GraphicsDevice graphicsDevice,
			int size,
			bool mipMap,
			SurfaceFormat preferredFormat,
			DepthFormat preferredDepthFormat,
			int preferredMultiSampleCount,
			RenderTargetUsage usage
		) : base(
			graphicsDevice,
			size,
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
					size,
					size,
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
				size,
				size,
				preferredDepthFormat,
				MultiSampleCount
			);
		}

		#endregion

		#region Protected Dispose Method

		/// <summary>
		/// Releases the unmanaged resources used by an instance of the
		/// <see cref="RenderTargetCube"/> class and optionally releases the managed
		/// resources.
		/// </summary>
		/// <param name="disposing">
		/// <see langword="true"/> to release both managed and unmanaged resources;
		/// <see langword="false"/> to release only unmanaged resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (glDepthStencilBuffer != null)
				{
					GraphicsDevice.GLDevice.AddDisposeRenderbuffer(glDepthStencilBuffer);
				}
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
