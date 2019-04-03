#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
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
			get;
			set;
		}

		public bool DepthBufferWriteEnable
		{
			get;
			set;
		}

		public StencilOperation CounterClockwiseStencilDepthBufferFail
		{
			get;
			set;
		}

		public StencilOperation CounterClockwiseStencilFail	
		{
			get;
			set;
		}

		public CompareFunction CounterClockwiseStencilFunction	
		{
			get;
			set;
		}

		public StencilOperation CounterClockwiseStencilPass	
		{
			get;
			set;
		}

		public CompareFunction DepthBufferFunction	
		{
			get;
			set;
		}

		public int ReferenceStencil	
		{
			get;
			set;
		}

		public StencilOperation StencilDepthBufferFail	
		{
			get;
			set;
		}

		public bool StencilEnable	
		{
			get;
			set;
		}

		public StencilOperation StencilFail	
		{
			get;
			set;
		}

		public CompareFunction StencilFunction	
		{
			get;
			set;
		}

		public int StencilMask	
		{
			get;
			set;
		}

		public StencilOperation StencilPass	
		{
			get;
			set;
		}

		public int StencilWriteMask	
		{
			get;
			set;
		}

		public bool TwoSidedStencilMode	
		{
			get;
			set;
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

		#region Internal Hash Function

		internal DepthStencilStateHash GetHash()
		{
			DepthStencilStateHash hash = new DepthStencilStateHash();

			// Bool -> Int32 conversion
			int depthBufferEnable = DepthBufferEnable ? 1 : 0;
			int depthBufferWriteEnable = DepthBufferWriteEnable ? 1 : 0;
			int stencilEnable = StencilEnable ? 1 : 0;
			int twoSidedStencilMode = TwoSidedStencilMode ? 1 : 0;

			hash.packedProperties =
				  ((int) depthBufferEnable		<< 32 - 2)
				| ((int) depthBufferWriteEnable		<< 32 - 3)
				| ((int) stencilEnable			<< 32 - 4)
				| ((int) twoSidedStencilMode		<< 32 - 5)
				| ((int) DepthBufferFunction		<< 32 - 8)
				| ((int) StencilFunction		<< 32 - 11)
				| ((int) CounterClockwiseStencilFunction << 32 - 14)
				| ((int) StencilPass			<< 32 - 17)
				| ((int) StencilFail			<< 32 - 20)
				| ((int) StencilDepthBufferFail		<< 32 - 23)
				| ((int) CounterClockwiseStencilFail	<< 32 - 26)
				| ((int) CounterClockwiseStencilPass	<< 32 - 29)
				| ((int) CounterClockwiseStencilDepthBufferFail);
			hash.stencilMask = StencilMask;
			hash.stencilWriteMask = StencilWriteMask;
			hash.referenceStencil = ReferenceStencil;

			return hash;
		}

		#endregion
	}

	internal struct DepthStencilStateHash
	{
		internal int packedProperties;
		internal int stencilMask;
		internal int stencilWriteMask;
		internal int referenceStencil;

		public override string ToString()
		{
			return    System.Convert.ToString(packedProperties, 2).PadLeft(32, '0')
				+ System.Convert.ToString(stencilMask, 2).PadLeft(32, '0')
				+ System.Convert.ToString(stencilWriteMask, 2).PadLeft(32, '0')
				+ System.Convert.ToString(referenceStencil, 2).PadLeft(32, '0');
		}
	}
}
