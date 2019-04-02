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

		#region Internal Hash Function

		internal BlendStateHash GetHash()
		{
			BlendStateHash hash = new BlendStateHash();

			hash.funcs = ((int) AlphaBlendFunction << 4) | ((int) ColorBlendFunction);
			hash.blendsAndColorWriteChannels =
				  ((int) AlphaDestinationBlend	<< (32 - 4))
				| ((int) AlphaSourceBlend	<< (32 - 8))
				| ((int) ColorDestinationBlend	<< (32 - 12))
				| ((int) ColorSourceBlend	<< (32 - 16))
				| ((int) ColorWriteChannels	<< (32 - 20))
				| ((int) ColorWriteChannels1	<< (32 - 24))
				| ((int) ColorWriteChannels2	<< (32 - 28))
				| ((int) ColorWriteChannels3);
			hash.blendFactor = BlendFactor.PackedValue;
			hash.multiSampleMask = MultiSampleMask;

			return hash;
		}

		#endregion
	}

	internal struct BlendStateHash
	{
		internal int funcs;
		internal int blendsAndColorWriteChannels;
		internal uint blendFactor;
		internal int multiSampleMask;

		public override string ToString()
		{
			return    System.Convert.ToString(funcs, 2).PadLeft(32, '0')
				+ System.Convert.ToString(blendsAndColorWriteChannels, 2).PadLeft(32, '0')
				+ System.Convert.ToString(blendFactor, 2).PadLeft(32, '0')
				+ System.Convert.ToString(multiSampleMask, 2).PadLeft(32, '0');
		}
	}
}
