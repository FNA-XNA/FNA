#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
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

		#region Internal Hash Function

		internal RasterizerStateHash GetHash()
		{
			RasterizerStateHash hash = new RasterizerStateHash();

			// Bool -> Int32 conversion
			int multiSampleAntiAlias = (MultiSampleAntiAlias ? 1 : 0);
			int scissorTestEnable = (ScissorTestEnable ? 1 : 0);

			hash.packedProperties =
				  ((int) multiSampleAntiAlias	<< 4)
				| ((int) scissorTestEnable	<< 3)
				| ((int) CullMode		<< 1)
				| ((int) FillMode);
			hash.depthBias = DepthBias;
			hash.slopeScaleDepthBias = SlopeScaleDepthBias;

			return hash;
		}

		#endregion
	}

	internal struct RasterizerStateHash
	{
		internal int packedProperties;
		internal float depthBias;
		internal float slopeScaleDepthBias;

		public override string ToString()
		{
			string binary = System.Convert.ToString(packedProperties, 2).PadLeft(32, '0');

			foreach (byte b in System.BitConverter.GetBytes(depthBias))
			{
				binary += System.Convert.ToString(b, 2).PadLeft(8, '0');
			}

			foreach (byte b in System.BitConverter.GetBytes(slopeScaleDepthBias))
			{
				binary += System.Convert.ToString(b, 2).PadLeft(8, '0');
			}

			return binary;
		}
	}
}
