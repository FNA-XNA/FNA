#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal class PipelineCache
	{
		#region Private Variables

		private GraphicsDevice device;

		#endregion

		#region Public Constructor

		public PipelineCache(GraphicsDevice graphicsDevice)
		{
			device = graphicsDevice;
		}

		#endregion

		#region BlendState Cache

		/* Public Variables */

		public BlendFunction AlphaBlendFunction;
		public Blend AlphaDestinationBlend;
		public Blend AlphaSourceBlend;
		public BlendFunction ColorBlendFunction;
		public Blend ColorDestinationBlend;
		public Blend ColorSourceBlend;
		public ColorWriteChannels ColorWriteChannels;
		public ColorWriteChannels ColorWriteChannels1;
		public ColorWriteChannels ColorWriteChannels2;
		public ColorWriteChannels ColorWriteChannels3;
		public Color BlendFactor;
		public int MultiSampleMask;

		/* FIXME: Do we actually care about this calculation, or do we
		 * just assume false each time?
		 * -flibit
		 */
		public bool SeparateAlphaBlend;

		/* Private Cache Storage */

		private Dictionary<BlendStateHash, BlendState> blendCache =
			new Dictionary<BlendStateHash, BlendState>();

		private struct BlendStateHash
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

		/* Public Functions */

		public void BeginApplyBlend()
		{
			BlendState oldBlendState = device.BlendState;

			AlphaBlendFunction = oldBlendState.AlphaBlendFunction;
			AlphaDestinationBlend = oldBlendState.AlphaDestinationBlend;
			AlphaSourceBlend = oldBlendState.AlphaSourceBlend;
			ColorBlendFunction = oldBlendState.ColorBlendFunction;
			ColorDestinationBlend = oldBlendState.ColorDestinationBlend;
			ColorSourceBlend = oldBlendState.ColorSourceBlend;
			ColorWriteChannels = oldBlendState.ColorWriteChannels;
			ColorWriteChannels1 = oldBlendState.ColorWriteChannels1;
			ColorWriteChannels2 = oldBlendState.ColorWriteChannels2;
			ColorWriteChannels3 = oldBlendState.ColorWriteChannels3;
			BlendFactor = oldBlendState.BlendFactor;
			MultiSampleMask = oldBlendState.MultiSampleMask;
			SeparateAlphaBlend = (
				ColorBlendFunction != AlphaBlendFunction ||
				ColorDestinationBlend != AlphaDestinationBlend
			);
		}

		public void EndApplyBlend()
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

			BlendState newBlend;
			if (!blendCache.TryGetValue(hash, out newBlend))
			{
				newBlend = new BlendState();

				newBlend.AlphaBlendFunction = AlphaBlendFunction;
				newBlend.AlphaDestinationBlend = AlphaDestinationBlend;
				newBlend.AlphaSourceBlend = AlphaSourceBlend;
				newBlend.ColorBlendFunction = ColorBlendFunction;
				newBlend.ColorDestinationBlend = ColorDestinationBlend;
				newBlend.ColorSourceBlend = ColorSourceBlend;
				newBlend.ColorWriteChannels = ColorWriteChannels;
				newBlend.ColorWriteChannels1 = ColorWriteChannels1;
				newBlend.ColorWriteChannels2 = ColorWriteChannels2;
				newBlend.ColorWriteChannels3 = ColorWriteChannels3;
				newBlend.BlendFactor = BlendFactor;
				newBlend.MultiSampleMask = MultiSampleMask;

				blendCache.Add(hash, newBlend);
				FNALoggerEXT.LogInfo("New BlendState added to pipeline cache");
			}

			device.BlendState = newBlend;
		}

		#endregion

		#region DepthStencilState Cache

		/* Public Variables */

		public bool DepthBufferEnable;
		public bool DepthBufferWriteEnable;
		public CompareFunction DepthBufferFunction;
		public bool StencilEnable;
		public CompareFunction StencilFunction;
		public StencilOperation StencilPass;
		public StencilOperation StencilFail;
		public StencilOperation StencilDepthBufferFail;
		public bool TwoSidedStencilMode;
		public CompareFunction CCWStencilFunction;
		public StencilOperation CCWStencilFail;
		public StencilOperation CCWStencilPass;
		public StencilOperation CCWStencilDepthBufferFail;
		public int StencilMask;
		public int StencilWriteMask;
		public int ReferenceStencil;

		/* Private Cache Storage */

		private Dictionary<DepthStencilStateHash, DepthStencilState> depthStencilCache =
			new Dictionary<DepthStencilStateHash, DepthStencilState>();

		private struct DepthStencilStateHash
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

		/* Public Functions */

		public void BeginApplyDepthStencil()
		{
			DepthStencilState oldDepthStencilState = device.DepthStencilState;

			DepthBufferEnable = oldDepthStencilState.DepthBufferEnable;
			DepthBufferWriteEnable = oldDepthStencilState.DepthBufferWriteEnable;
			DepthBufferFunction = oldDepthStencilState.DepthBufferFunction;
			StencilEnable = oldDepthStencilState.StencilEnable;
			StencilFunction = oldDepthStencilState.StencilFunction;
			StencilPass = oldDepthStencilState.StencilPass;
			StencilFail = oldDepthStencilState.StencilFail;
			StencilDepthBufferFail = oldDepthStencilState.StencilDepthBufferFail;
			TwoSidedStencilMode = oldDepthStencilState.TwoSidedStencilMode;
			CCWStencilFunction = oldDepthStencilState.CounterClockwiseStencilFunction;
			CCWStencilFail = oldDepthStencilState.CounterClockwiseStencilFail;
			CCWStencilPass = oldDepthStencilState.CounterClockwiseStencilPass;
			CCWStencilDepthBufferFail = oldDepthStencilState.CounterClockwiseStencilDepthBufferFail;
			StencilMask = oldDepthStencilState.StencilMask;
			StencilWriteMask = oldDepthStencilState.StencilWriteMask;
			ReferenceStencil = oldDepthStencilState.ReferenceStencil;
		}

		public void EndApplyDepthStencil()
		{
			DepthStencilStateHash hash = new DepthStencilStateHash();

			// Bool -> Int32 conversion
			int depthBufferEnable = DepthBufferEnable ? 1 : 0;
			int depthBufferWriteEnable = DepthBufferWriteEnable ? 1 : 0;
			int stencilEnable = StencilEnable ? 1 : 0;
			int twoSidedStencilMode = TwoSidedStencilMode ? 1 : 0;

			hash.packedProperties =
				  ((int) depthBufferEnable	<< 32 - 2)
				| ((int) depthBufferWriteEnable	<< 32 - 3)
				| ((int) stencilEnable		<< 32 - 4)
				| ((int) twoSidedStencilMode	<< 32 - 5)
				| ((int) DepthBufferFunction	<< 32 - 8)
				| ((int) StencilFunction	<< 32 - 11)
				| ((int) CCWStencilFunction	<< 32 - 14)
				| ((int) StencilPass		<< 32 - 17)
				| ((int) StencilFail		<< 32 - 20)
				| ((int) StencilDepthBufferFail	<< 32 - 23)
				| ((int) CCWStencilFail		<< 32 - 26)
				| ((int) CCWStencilPass		<< 32 - 29)
				| ((int) CCWStencilDepthBufferFail);
			hash.stencilMask = StencilMask;
			hash.stencilWriteMask = StencilWriteMask;
			hash.referenceStencil = ReferenceStencil;

			DepthStencilState newDepthStencil;
			if (!depthStencilCache.TryGetValue(hash, out newDepthStencil))
			{
				newDepthStencil = new DepthStencilState();

				newDepthStencil.DepthBufferEnable = DepthBufferEnable;
				newDepthStencil.DepthBufferWriteEnable = DepthBufferWriteEnable;
				newDepthStencil.DepthBufferFunction = DepthBufferFunction;
				newDepthStencil.StencilEnable = StencilEnable;
				newDepthStencil.StencilFunction = StencilFunction;
				newDepthStencil.StencilPass = StencilPass;
				newDepthStencil.StencilFail = StencilFail;
				newDepthStencil.StencilDepthBufferFail = StencilDepthBufferFail;
				newDepthStencil.TwoSidedStencilMode = TwoSidedStencilMode;
				newDepthStencil.CounterClockwiseStencilFunction = CCWStencilFunction;
				newDepthStencil.CounterClockwiseStencilFail = CCWStencilFail;
				newDepthStencil.CounterClockwiseStencilPass = CCWStencilPass;
				newDepthStencil.CounterClockwiseStencilDepthBufferFail = CCWStencilDepthBufferFail;
				newDepthStencil.StencilMask = StencilMask;
				newDepthStencil.StencilWriteMask = StencilWriteMask;
				newDepthStencil.ReferenceStencil = ReferenceStencil;

				depthStencilCache.Add(hash, newDepthStencil);
				FNALoggerEXT.LogInfo("New DepthStencilState added to pipeline cache");
			}

			device.DepthStencilState = newDepthStencil;
		}

		#endregion

		#region RasterizerState Cache

		/* Public Variables */

		public CullMode CullMode;
		public FillMode FillMode;
		public float DepthBias;
		public bool MultiSampleAntiAlias;
		public bool ScissorTestEnable;
		public float SlopeScaleDepthBias;

		/* Private Cache Storage */

		private Dictionary<RasterizerStateHash, RasterizerState> rasterizerCache =
			new Dictionary<RasterizerStateHash, RasterizerState>();

		private struct RasterizerStateHash
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

		/* Public Functions */

		public void BeginApplyRasterizer()
		{
			RasterizerState oldRasterizerState = device.RasterizerState;

			CullMode = oldRasterizerState.CullMode;
			FillMode = oldRasterizerState.FillMode;
			DepthBias = oldRasterizerState.DepthBias;
			MultiSampleAntiAlias = oldRasterizerState.MultiSampleAntiAlias;
			ScissorTestEnable = oldRasterizerState.ScissorTestEnable;
			SlopeScaleDepthBias = oldRasterizerState.SlopeScaleDepthBias;
		}

		public void EndApplyRasterizer()
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

			RasterizerState newRasterizer;
			if (!rasterizerCache.TryGetValue(hash, out newRasterizer))
			{
				newRasterizer = new RasterizerState();

				newRasterizer.CullMode = CullMode;
				newRasterizer.FillMode = FillMode;
				newRasterizer.DepthBias = DepthBias;
				newRasterizer.MultiSampleAntiAlias = MultiSampleAntiAlias;
				newRasterizer.ScissorTestEnable = ScissorTestEnable;
				newRasterizer.SlopeScaleDepthBias = SlopeScaleDepthBias;

				rasterizerCache.Add(hash, newRasterizer);
				FNALoggerEXT.LogInfo("New RasterizerState added to pipeline cache");
			}

			device.RasterizerState = newRasterizer;
		}

		#endregion

		#region SamplerState Cache

		/* Public Variables */

		public TextureAddressMode AddressU;
		public TextureAddressMode AddressV;
		public TextureAddressMode AddressW;
		public int MaxAnisotropy;
		public int MaxMipLevel;
		public float MipMapLODBias;
		public TextureFilter Filter;

		/* Private Cache Storage */

		private Dictionary<SamplerStateHash, SamplerState> samplerCache =
			new Dictionary<SamplerStateHash, SamplerState>();

		private struct SamplerStateHash
		{
			internal int filterAndAddresses;
			internal int maxAnisotropy;
			internal int maxMipLevel;
			internal float mipMapLevelOfDetailBias;

			public override string ToString()
			{
				string binary =   System.Convert.ToString(filterAndAddresses, 2).PadLeft(32, '0')
						+ System.Convert.ToString(maxAnisotropy, 2).PadLeft(32, '0')
						+ System.Convert.ToString(maxMipLevel, 2).PadLeft(32, '0');

				foreach (byte b in System.BitConverter.GetBytes(mipMapLevelOfDetailBias))
				{
					binary += System.Convert.ToString(b, 2).PadLeft(8, '0');
				}

				return binary;
			}
		}

		/* Public Functions */

		public void BeginApplySampler(SamplerStateCollection samplers, int register)
		{
			SamplerState oldSampler = samplers[register];

			AddressU = oldSampler.AddressU;
			AddressV = oldSampler.AddressV;
			AddressW = oldSampler.AddressW;
			MaxAnisotropy = oldSampler.MaxAnisotropy;
			MaxMipLevel = oldSampler.MaxMipLevel;
			MipMapLODBias = oldSampler.MipMapLevelOfDetailBias;
			Filter = oldSampler.Filter;
		}

		public void EndApplySampler(SamplerStateCollection samplers, int register)
		{
			SamplerStateHash hash = new SamplerStateHash();
			hash.filterAndAddresses =
				  ((int) Filter		<< 6)
				| ((int) AddressU	<< 4)
				| ((int) AddressV	<< 2)
				| ((int) AddressW);
			hash.maxAnisotropy = MaxAnisotropy;
			hash.maxMipLevel = MaxMipLevel;
			hash.mipMapLevelOfDetailBias = MipMapLODBias;

			SamplerState newSampler;
			if (!samplerCache.TryGetValue(hash, out newSampler))
			{
				newSampler = new SamplerState();

				newSampler.Filter = Filter;
				newSampler.AddressU = AddressU;
				newSampler.AddressV = AddressV;
				newSampler.AddressW = AddressW;
				newSampler.MaxAnisotropy = MaxAnisotropy;
				newSampler.MaxMipLevel = MaxMipLevel;
				newSampler.MipMapLevelOfDetailBias = MipMapLODBias;

				samplerCache.Add(hash, newSampler);
				FNALoggerEXT.LogInfo("New SamplerState added to pipeline cache");
			}

			samplers[register] = newSampler;
		}

		#endregion
	}
}
