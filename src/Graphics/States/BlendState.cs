#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class BlendState : GraphicsResource
	{
		#region Public Properties

		public BlendFunction AlphaBlendFunction
		{
			get
			{
				return state.blendFuncAlpha;
			}
			set
			{
				state.blendFuncAlpha = value;
			}
		}

		public Blend AlphaDestinationBlend
		{
			get
			{
				return state.dstBlendAlpha;
			}
			set
			{
				state.dstBlendAlpha = value;
			}
		}

		public Blend AlphaSourceBlend
		{
			get
			{
				return state.srcBlendAlpha;
			}
			set
			{
				state.srcBlendAlpha = value;
			}
		}

		public BlendFunction ColorBlendFunction
		{
			get
			{
				return state.blendFunc;
			}
			set
			{
				state.blendFunc = value;
			}
		}

		public Blend ColorDestinationBlend
		{
			get
			{
				return state.dstBlend;
			}
			set
			{
				state.dstBlend = value;
			}
		}

		public Blend ColorSourceBlend
		{
			get
			{
				return state.srcBlend;
			}
			set
			{
				state.srcBlend = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels
		{
			get
			{
				return state.colorWriteEnable;
			}
			set
			{
				state.colorWriteEnable = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels1
		{
			get
			{
				return state.colorWriteEnable1;
			}
			set
			{
				state.colorWriteEnable1 = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels2
		{
			get
			{
				return state.colorWriteEnable2;
			}
			set
			{
				state.colorWriteEnable2 = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels3
		{
			get
			{
				return state.colorWriteEnable3;
			}
			set
			{
				state.colorWriteEnable3 = value;
			}
		}

		public Color BlendFactor
		{
			get
			{
				return state.blendColor;
			}
			set
			{
				state.blendColor = value;
			}
		}

		public int MultiSampleMask
		{
			get
			{
				return state.multisampleMask;
			}
			set
			{
				state.multisampleMask = value;
			}
		}

		#endregion

		#region Public BlendState Presets

		public static readonly BlendState Additive = new BlendState(
			"BlendState.Additive",
			Blend.SourceAlpha,
			Blend.SourceAlpha,
			Blend.One,
			Blend.One
		);

		public static readonly BlendState AlphaBlend = new BlendState(
			"BlendState.AlphaBlend",
			Blend.One,
			Blend.One,
			Blend.InverseSourceAlpha,
			Blend.InverseSourceAlpha
		);

		public static readonly BlendState NonPremultiplied = new BlendState(
			"BlendState.NonPremultiplied",
			Blend.SourceAlpha,
			Blend.SourceAlpha,
			Blend.InverseSourceAlpha,
			Blend.InverseSourceAlpha
		);

		public static readonly BlendState Opaque = new BlendState(
			"BlendState.Opaque",
			Blend.One,
			Blend.One,
			Blend.Zero,
			Blend.Zero
		);

		#endregion

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_BlendState state;

		#endregion

		#region Public Constructor

		public BlendState ()
		{
			AlphaBlendFunction = BlendFunction.Add;
			AlphaDestinationBlend = Blend.Zero;
			AlphaSourceBlend = Blend.One;
			ColorBlendFunction = BlendFunction.Add;
			ColorDestinationBlend = Blend.Zero;
			ColorSourceBlend = Blend.One;
			ColorWriteChannels = ColorWriteChannels.All;
			ColorWriteChannels1 = ColorWriteChannels.All;
			ColorWriteChannels2 = ColorWriteChannels.All;
			ColorWriteChannels3 = ColorWriteChannels.All;
			BlendFactor = Color.White;
			MultiSampleMask = -1; // AKA 0xFFFFFFFF
		}

		#endregion

		#region Private Constructor

		private BlendState(
			string name,
			Blend colorSourceBlend,
			Blend alphaSourceBlend,
			Blend colorDestBlend,
			Blend alphaDestBlend
		) : this() {
			Name = name;
			ColorSourceBlend = colorSourceBlend;
			AlphaSourceBlend = alphaSourceBlend;
			ColorDestinationBlend = colorDestBlend;
			AlphaDestinationBlend = alphaDestBlend;
		}

		#endregion
	}
}
