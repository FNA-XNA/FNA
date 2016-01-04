#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
			get;
			set;
		}

		public Blend AlphaDestinationBlend
		{
			get;
			set;
		}

		public Blend AlphaSourceBlend
		{
			get;
			set;
		}

		public BlendFunction ColorBlendFunction
		{
			get;
			set;
		}

		public Blend ColorDestinationBlend
		{
			get;
			set;
		}

		public Blend ColorSourceBlend
		{
			get;
			set;
		}

		public ColorWriteChannels ColorWriteChannels
		{
			get;
			set;
		}

		public ColorWriteChannels ColorWriteChannels1
		{
			get;
			set;
		}

		public ColorWriteChannels ColorWriteChannels2
		{
			get;
			set;
		}

		public ColorWriteChannels ColorWriteChannels3
		{
			get;
			set;
		}

		public Color BlendFactor
		{
			get;
			set;
		}

		public int MultiSampleMask
		{
			get;
			set;
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
