#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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
			get
			{
				return state.addressU;
			}
			set
			{
				state.addressU = value;
			}
		}

		public TextureAddressMode AddressV
		{
			get
			{
				return state.addressV;
			}
			set
			{
				state.addressV = value;
			}
		}

		public TextureAddressMode AddressW
		{
			get
			{
				return state.addressW;
			}
			set
			{
				state.addressW = value;
			}
		}

		public TextureFilter Filter
		{
			get
			{
				return state.filter;
			}
			set
			{
				state.filter = value;
			}
		}

		public int MaxAnisotropy
		{
			get
			{
				return state.maxAnisotropy;
			}
			set
			{
				state.maxAnisotropy = value;
			}
		}

		public int MaxMipLevel
		{
			get
			{
				return state.maxMipLevel;
			}
			set
			{
				state.maxMipLevel = value;
			}
		}

		public float MipMapLevelOfDetailBias
		{
			get
			{
				return state.mipMapLevelOfDetailBias;
			}
			set
			{
				state.mipMapLevelOfDetailBias = value;
			}
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

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_SamplerState state;

		internal protected override bool IsHarmlessToLeakInstance
		{
			get
			{
				return true;
			}
		}

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
