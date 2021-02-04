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
	public class BlendState : GraphicsResource
	{
		#region Public Properties

		public BlendFunction AlphaBlendFunction
		{
			get => state.alphaBlendFunction;
            set => state.alphaBlendFunction = value;
        }

		public Blend AlphaDestinationBlend
		{
			get => state.alphaDestinationBlend;
            set => state.alphaDestinationBlend = value;
        }

		public Blend AlphaSourceBlend
		{
			get => state.alphaSourceBlend;
            set => state.alphaSourceBlend = value;
        }

		public BlendFunction ColorBlendFunction
		{
			get => state.colorBlendFunction;
            set => state.colorBlendFunction = value;
        }

		public Blend ColorDestinationBlend
		{
			get => state.colorDestinationBlend;
            set => state.colorDestinationBlend = value;
        }

		public Blend ColorSourceBlend
		{
			get => state.colorSourceBlend;
            set => state.colorSourceBlend = value;
        }

		public ColorWriteChannels ColorWriteChannels
		{
			get => state.colorWriteEnable;
            set => state.colorWriteEnable = value;
        }

		public ColorWriteChannels ColorWriteChannels1
		{
			get => state.colorWriteEnable1;
            set => state.colorWriteEnable1 = value;
        }

		public ColorWriteChannels ColorWriteChannels2
		{
			get => state.colorWriteEnable2;
            set => state.colorWriteEnable2 = value;
        }

		public ColorWriteChannels ColorWriteChannels3
		{
			get => state.colorWriteEnable3;
            set => state.colorWriteEnable3 = value;
        }

		public Color BlendFactor
		{
			get => state.blendFactor;
            set => state.blendFactor = value;
        }

		public int MultiSampleMask
		{
			get => state.multiSampleMask;
            set => state.multiSampleMask = value;
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

		public BlendState()
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
