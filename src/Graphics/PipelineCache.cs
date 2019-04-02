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
		#region Public BlendState Variables

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

		#endregion

		#region Public DepthStencilState Variables

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

		#endregion

		#region Public RasterizerState Variables

		public CullMode CullMode;
		public FillMode FillMode;
		public float DepthBias;
		public bool MultiSampleAntiAlias;
		public bool ScissorTestEnable;
		public float SlopeScaleDepthBias;

		#endregion

		#region Public SamplerState Variables

		public TextureAddressMode AddressU;
		public TextureAddressMode AddressV;
		public TextureAddressMode AddressW;
		public int MaxAnisotropy;
		public int MaxMipLevel;
		public float MipMapLODBias;
		public TextureFilter Filter;

		#endregion

		#region Private Variables

		private GraphicsDevice device;

		#endregion

		#region Private State Cache Variables

		private Dictionary<BlendStateHash, BlendState> blendCache =
			new Dictionary<BlendStateHash, BlendState>();

		#endregion

		#region Private State Cache Hack Variables

		/* This is a workaround to prevent excessive state allocation.
		 * Well, I say "excessive", but even this allocates a minimum of
		 * 1916 bytes per effect. Just for states.
		 *
		 * Additionally, we depend on our inaccurate behavior of letting you
		 * change the state after it's been bound to the GraphicsDevice.
		 *
		 * More accurate behavior will require hashing the current states,
		 * comparing them to the new state after applying the effect, and
		 * getting a state from a cache, generating a new one if needed.
		 * -flibit
		 */
		private DepthStencilState[] depthStencilCache = new DepthStencilState[2]
		{
			new DepthStencilState(), new DepthStencilState()
		};
		private RasterizerState[] rasterizerCache = new RasterizerState[2]
		{
			new RasterizerState(), new RasterizerState()
		};
		private SamplerState[] samplerCache = GenerateSamplerCache();
		private static SamplerState[] GenerateSamplerCache()
		{
			int numSamplers = 60; // FIXME: Arbitrary! -flibit
			SamplerState[] result = new SamplerState[numSamplers];
			for (int i = 0; i < numSamplers; i += 1)
			{
				result[i] = new SamplerState();
			}
			return result;
		}

		#endregion

		#region Public Constructor

		public PipelineCache(GraphicsDevice graphicsDevice)
		{
			device = graphicsDevice;
		}

		#endregion

		#region Public Methods

		public void BeginApply()
		{
			BlendState oldBlendState = device.BlendState;
			DepthStencilState oldDepthStencilState = device.DepthStencilState;
			RasterizerState oldRasterizerState = device.RasterizerState;

			// Current blend state
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

			// Current depth/stencil state
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

			// Current rasterizer state
			CullMode = oldRasterizerState.CullMode;
			FillMode = oldRasterizerState.FillMode;
			DepthBias = oldRasterizerState.DepthBias;
			MultiSampleAntiAlias = oldRasterizerState.MultiSampleAntiAlias;
			ScissorTestEnable = oldRasterizerState.ScissorTestEnable;
			SlopeScaleDepthBias = oldRasterizerState.SlopeScaleDepthBias;
		}

		public void EndApplyBlend()
		{
			BlendState newBlend;
			BlendStateHash hash = device.BlendState.GetHash();

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
				FNALoggerEXT.LogInfo("New BlendState added to cache");
			}

			device.BlendState = newBlend;
		}

		public void EndApplyDepthStencil()
		{
			// FIXME: This is part of the state cache hack! -flibit
			DepthStencilState newDepthStencil;
			if (device.DepthStencilState == depthStencilCache[0])
			{
				newDepthStencil = depthStencilCache[1];
			}
			else
			{
				newDepthStencil = depthStencilCache[0];
			}
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
			device.DepthStencilState = newDepthStencil;
		}

		public void EndApplyRasterizer()
		{
			// FIXME: This is part of the state cache hack! -flibit
			RasterizerState newRasterizer;
			if (device.RasterizerState == rasterizerCache[0])
			{
				newRasterizer = rasterizerCache[1];
			}
			else
			{
				newRasterizer = rasterizerCache[0];
			}
			newRasterizer.CullMode = CullMode;
			newRasterizer.FillMode = FillMode;
			newRasterizer.DepthBias = DepthBias;
			newRasterizer.MultiSampleAntiAlias = MultiSampleAntiAlias;
			newRasterizer.ScissorTestEnable = ScissorTestEnable;
			newRasterizer.SlopeScaleDepthBias = SlopeScaleDepthBias;
			device.RasterizerState = newRasterizer;
		}

		public void BeginApplySampler(SamplerStateCollection samplers, int register)
		{
			SamplerState oldSampler = samplers[register];

			// Current sampler state
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
			// FIXME: This is part of the state cache hack! -flibit
			SamplerState newSampler;
			if (samplers[register] == samplerCache[register])
			{
				// FIXME: 30 is arbitrary! -flibit
				newSampler = samplerCache[register + 30];
			}
			else
			{
				newSampler = samplerCache[register];
			}
			newSampler.Filter = Filter;
			newSampler.AddressU = AddressU;
			newSampler.AddressV = AddressV;
			newSampler.AddressW = AddressW;
			newSampler.MaxAnisotropy = MaxAnisotropy;
			newSampler.MaxMipLevel = MaxMipLevel;
			newSampler.MipMapLevelOfDetailBias = MipMapLODBias;
			samplers[register] = newSampler;
		}

		#endregion
	}
}
