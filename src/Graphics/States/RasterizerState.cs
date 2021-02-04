#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
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
			get => state.cullMode;
            set => state.cullMode = value;
        }

		public float DepthBias
		{
			get => state.depthBias;
            set => state.depthBias = value;
        }

		public FillMode FillMode
		{
			get => state.fillMode;
            set => state.fillMode = value;
        }

		public bool MultiSampleAntiAlias
		{
			get => state.multiSampleAntiAlias == 1;
            set => state.multiSampleAntiAlias = (byte) (value ? 1 : 0);
        }

		public bool ScissorTestEnable
		{
			get => state.scissorTestEnable == 1;
            set => state.scissorTestEnable = (byte) (value ? 1 : 0);
        }

		public float SlopeScaleDepthBias
		{
			get => state.slopeScaleDepthBias;
            set => state.slopeScaleDepthBias = value;
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

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_RasterizerState state;

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
