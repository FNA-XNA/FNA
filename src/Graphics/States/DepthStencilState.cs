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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class DepthStencilState : GraphicsResource
	{
		#region Public Properties

		public bool DepthBufferEnable
		{
			get
			{
				return state.depthBufferEnable == 1;
			}
			set
			{
				state.depthBufferEnable = (byte) (value ? 1 : 0);
			}
		}

		public bool DepthBufferWriteEnable
		{
			get
			{
				return state.depthBufferWriteEnable == 1;
			}
			set
			{
				state.depthBufferWriteEnable = (byte) (value ? 1 : 0);
			}
		}

		public StencilOperation CounterClockwiseStencilDepthBufferFail
		{
			get
			{
				return state.ccwStencilDepthBufferFail;
			}
			set
			{
				state.ccwStencilDepthBufferFail = value;
			}
		}

		public StencilOperation CounterClockwiseStencilFail	
		{
			get
			{
				return state.ccwStencilFail;
			}
			set
			{
				state.ccwStencilFail = value;
			}
		}

		public CompareFunction CounterClockwiseStencilFunction	
		{
			get
			{
				return state.ccwStencilFunction;
			}
			set
			{
				state.ccwStencilFunction = value;
			}
		}

		public StencilOperation CounterClockwiseStencilPass	
		{
			get
			{
				return state.ccwStencilPass;
			}
			set
			{
				state.ccwStencilPass = value;
			}
		}

		public CompareFunction DepthBufferFunction	
		{
			get
			{
				return state.depthBufferFunction;
			}
			set
			{
				state.depthBufferFunction = value;
			}
		}

		public int ReferenceStencil	
		{
			get
			{
				return state.referenceStencil;
			}
			set
			{
				state.referenceStencil = value;
			}
		}

		public StencilOperation StencilDepthBufferFail	
		{
			get
			{
				return state.stencilDepthBufferFail;
			}
			set
			{
				state.stencilDepthBufferFail = value;
			}
		}

		public bool StencilEnable	
		{
			get
			{
				return state.stencilEnable == 1;
			}
			set
			{
				state.stencilEnable = (byte) (value ? 1 : 0);
			}
		}

		public StencilOperation StencilFail	
		{
			get
			{
				return state.stencilFail;
			}
			set
			{
				state.stencilFail = value;
			}
		}

		public CompareFunction StencilFunction	
		{
			get
			{
				return state.stencilFunction;
			}
			set
			{
				state.stencilFunction = value;
			}
		}

		public int StencilMask	
		{
			get
			{
				return state.stencilMask;
			}
			set
			{
				state.stencilMask = value;
			}
		}

		public StencilOperation StencilPass	
		{
			get
			{
				return state.stencilPass;
			}
			set
			{
				state.stencilPass = value;
			}
		}

		public int StencilWriteMask	
		{
			get
			{
				return state.stencilWriteMask;
			}
			set
			{
				state.stencilWriteMask = value;
			}
		}

		public bool TwoSidedStencilMode	
		{
			get
			{
				return state.twoSidedStencilMode == 1;
			}
			set
			{
				state.twoSidedStencilMode = (byte) (value ? 1 : 0);
			}
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

		internal protected override bool IsHarmlessToLeakInstance
		{
			get
			{
				return true;
			}
		}

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
			StencilMask = Int32.MaxValue;
			StencilWriteMask = Int32.MaxValue;
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
