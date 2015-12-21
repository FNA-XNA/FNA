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
	public class SamplerState : GraphicsResource
	{
		#region Public Properties

		public TextureAddressMode AddressU
		{
			get;
			set;
		}

		public TextureAddressMode AddressV
		{
			get;
			set;
		}

		public TextureAddressMode AddressW
		{
			get;
			set;
		}

		public TextureFilter Filter
		{
			get;
			set;
		}

		public int MaxAnisotropy
		{
			get;
			set;
		}

		public int MaxMipLevel
		{
			get;
			set;
		}

		public float MipMapLevelOfDetailBias
		{
			get;
			set;
		}

		#endregion

		#region Public SamplerState Presets

		public static readonly SamplerState AnisotropicClamp = new SamplerState(
			"SamplerState.AnisotropicClamp",
			TextureFilter.Anisotropic,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp
		);

		public static readonly SamplerState AnisotropicWrap = new SamplerState(
			"SamplerState.AnisotropicWrap",
			TextureFilter.Anisotropic,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap
		);

		public static readonly SamplerState LinearClamp = new SamplerState(
			"SamplerState.LinearClamp",
			TextureFilter.Linear,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp
		);

		public static readonly SamplerState LinearWrap = new SamplerState(
			"SamplerState.LinearWrap",
			TextureFilter.Linear,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap
		);

		public static readonly SamplerState PointClamp = new SamplerState(
			"SamplerState.PointClamp",
			TextureFilter.Point,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp
		);

		public static readonly SamplerState PointWrap = new SamplerState(
			"SamplerState.PointWrap",
			TextureFilter.Point,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap
		);

		#endregion

		#region Public Constructor

		public SamplerState()
		{
			Filter = TextureFilter.Linear;
			AddressU = TextureAddressMode.Wrap;
			AddressV = TextureAddressMode.Wrap;
			AddressW = TextureAddressMode.Wrap;
			MaxAnisotropy = 4;
			MaxMipLevel = 0;
			MipMapLevelOfDetailBias = 0.0f;
		}

		#endregion

		#region Private Constructor

		private SamplerState(
			string name,
			TextureFilter filter,
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			TextureAddressMode addressW
		) : this() {
			Name = name;
			Filter = filter;
			AddressU = addressU;
			AddressV = addressV;
			AddressW = addressW;
		}

		#endregion
	}
}
