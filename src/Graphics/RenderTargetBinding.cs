#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
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
	// RenderTargetBinding structure: http://msdn.microsoft.com/en-us/library/ff434403.aspx
	public struct RenderTargetBinding
	{
		#region Public Properties

		public Texture RenderTarget
		{
			get
			{
				return renderTarget;
			}
		}

		public CubeMapFace CubeMapFace
		{
			get
			{
				return cubeMapFace;
			}
		}

		#endregion

		#region Private Variables

		private readonly Texture renderTarget;
		private readonly CubeMapFace cubeMapFace;

		#endregion

		#region Public Constructors

		public RenderTargetBinding(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
			{
				throw new ArgumentNullException("renderTarget");
			}

			this.renderTarget = renderTarget;
			cubeMapFace = CubeMapFace.PositiveX;
		}

		public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
		{
			if (renderTarget == null)
			{
				throw new ArgumentNullException("renderTarget");
			}
			if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
			{
				throw new ArgumentOutOfRangeException("cubeMapFace");
			}

			this.renderTarget = renderTarget;
			this.cubeMapFace = cubeMapFace;
		}

		#endregion

		#region Public Static Conversion Operator

		public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
		{
			return new RenderTargetBinding(renderTarget);
		}

		#endregion

		#region Internal FNA3D Conversion

		internal FNA3D.FNA3D_RenderTargetBinding ToFNA3D()
		{
			if (renderTarget is RenderTarget2D)
			{
				RenderTarget2D rt = renderTarget as RenderTarget2D;
				return new FNA3D.FNA3D_RenderTargetBinding
				{
					type = 0,
					format = rt.Format,
					levelCount = rt.LevelCount,
					texture = rt.texture,
					width = rt.Width,
					height = rt.Height,
					renderTargetUsage = rt.RenderTargetUsage,
					colorBuffer = (rt as IRenderTarget).ColorBuffer,
					depthStencilFormat = rt.DepthStencilFormat,
					multiSampleCount = rt.MultiSampleCount,
					cubeMapFace = CubeMapFace.PositiveX
				};
			}
			else
			{
				RenderTargetCube rt = renderTarget as RenderTargetCube;
				return new FNA3D.FNA3D_RenderTargetBinding
				{
					type = 1,
					format = rt.Format,
					levelCount = rt.LevelCount,
					texture = rt.texture,
					width = rt.Size,
					height = rt.Size,
					renderTargetUsage = rt.RenderTargetUsage,
					colorBuffer = (rt as IRenderTarget).ColorBuffer,
					depthStencilFormat = rt.DepthStencilFormat,
					multiSampleCount = rt.MultiSampleCount,
					cubeMapFace = this.cubeMapFace
				};
			}
		}

		#endregion
	}
}
