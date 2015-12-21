#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class RasterizerState : GraphicsResource
	{
		#region Public Properties

		public CullMode CullMode
		{
			get;
			set;
		}

		public float DepthBias
		{
			get;
			set;
		}

		public FillMode FillMode
		{
			get;
			set;
		}

		public bool MultiSampleAntiAlias
		{
			get;
			set;
		}

		public bool ScissorTestEnable
		{
			get;
			set;
		}

		public float SlopeScaleDepthBias
		{
			get;
			set;
		}

		#endregion

		#region Public RasterizerState Presets

		public static readonly RasterizerState CullClockwise = new RasterizerState(
			"RasterizerState.CullClockwise",
			CullMode.CullClockwiseFace
		);

		public static readonly RasterizerState CullCounterClockwise = new RasterizerState(
			"RasterizerState.CullCounterClockwise",
			CullMode.CullCounterClockwiseFace
		);

		public static readonly RasterizerState CullNone = new RasterizerState(
			"RasterizerState.CullNone",
			CullMode.None
		);

		#endregion

		#region Public Constructor

		public RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
		}

		#endregion

		#region Private Constructor

		private RasterizerState(
			string name,
			CullMode cullMode
		) : this() {
			Name = name;
			CullMode = cullMode;
		}

		#endregion
	}
}
