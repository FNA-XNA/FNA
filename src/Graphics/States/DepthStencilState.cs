#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
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
	public class DepthStencilState : GraphicsResource
	{
		#region Public Properties

		public bool DepthBufferEnable
		{
			get => state.depthBufferEnable == 1;
            set => state.depthBufferEnable = (byte) (value ? 1 : 0);
        }

		public bool DepthBufferWriteEnable
		{
			get => state.depthBufferWriteEnable == 1;
            set => state.depthBufferWriteEnable = (byte) (value ? 1 : 0);
        }

		public StencilOperation CounterClockwiseStencilDepthBufferFail
		{
			get => state.ccwStencilDepthBufferFail;
            set => state.ccwStencilDepthBufferFail = value;
        }

		public StencilOperation CounterClockwiseStencilFail	
		{
			get => state.ccwStencilFail;
            set => state.ccwStencilFail = value;
        }

		public CompareFunction CounterClockwiseStencilFunction	
		{
			get => state.ccwStencilFunction;
            set => state.ccwStencilFunction = value;
        }

		public StencilOperation CounterClockwiseStencilPass	
		{
			get => state.ccwStencilPass;
            set => state.ccwStencilPass = value;
        }

		public CompareFunction DepthBufferFunction	
		{
			get => state.depthBufferFunction;
            set => state.depthBufferFunction = value;
        }

		public int ReferenceStencil	
		{
			get => state.referenceStencil;
            set => state.referenceStencil = value;
        }

		public StencilOperation StencilDepthBufferFail	
		{
			get => state.stencilDepthBufferFail;
            set => state.stencilDepthBufferFail = value;
        }

		public bool StencilEnable	
		{
			get => state.stencilEnable == 1;
            set => state.stencilEnable = (byte) (value ? 1 : 0);
        }

		public StencilOperation StencilFail	
		{
			get => state.stencilFail;
            set => state.stencilFail = value;
        }

		public CompareFunction StencilFunction	
		{
			get => state.stencilFunction;
            set => state.stencilFunction = value;
        }

		public int StencilMask	
		{
			get => state.stencilMask;
            set => state.stencilMask = value;
        }

		public StencilOperation StencilPass	
		{
			get => state.stencilPass;
            set => state.stencilPass = value;
        }

		public int StencilWriteMask	
		{
			get => state.stencilWriteMask;
            set => state.stencilWriteMask = value;
        }

		public bool TwoSidedStencilMode	
		{
			get => state.twoSidedStencilMode == 1;
            set => state.twoSidedStencilMode = (byte) (value ? 1 : 0);
        }

		#endregion

		#region Public DepthStencilState Presets

		public static readonly DepthStencilState Default = new DepthStencilState(
			"DepthStencilState.Default",
			true,
			true
		);

		public static readonly DepthStencilState DepthRead = new DepthStencilState(
			"DepthStencilState.DepthRead",
			true,
			false
		);

		public static readonly DepthStencilState None = new DepthStencilState(
			"DepthStencilState.None",
			false,
			false
		);

		#endregion

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_DepthStencilState state;

		#endregion

		#region Public Constructor

		public DepthStencilState()
		{
			DepthBufferEnable = true;
			DepthBufferWriteEnable = true;
			DepthBufferFunction = CompareFunction.LessEqual;
			StencilEnable = false;
			StencilFunction = CompareFunction.Always;
			StencilPass = StencilOperation.Keep;
			StencilFail = StencilOperation.Keep;
			StencilDepthBufferFail = StencilOperation.Keep;
			TwoSidedStencilMode = false;
			CounterClockwiseStencilFunction = CompareFunction.Always;
			CounterClockwiseStencilFail = StencilOperation.Keep;
			CounterClockwiseStencilPass = StencilOperation.Keep;
			CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
			StencilMask = int.MaxValue;
			StencilWriteMask = int.MaxValue;
			ReferenceStencil = 0;
		}

		#endregion

		#region Private Constructor

		private DepthStencilState(
			string name,
			bool depthBufferEnable,
			bool depthBufferWriteEnable
		) : this() {
			Name = name;
			DepthBufferEnable = depthBufferEnable;
			DepthBufferWriteEnable = depthBufferWriteEnable;
		}

		#endregion
	}
}
