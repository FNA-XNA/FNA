#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region VERBOSE_PIPELINECACHE Option
// #define VERBOSE_PIPELINECACHE
/* If you want to debug the PipelineCache to make sure it's interpreting your
 * Effects' render state changes properly, you can enable this and get a bunch
 * of messages logged to FNALoggerEXT.
 * -flibit
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

		#region Public Hashing Constants

		/* The hashing algorithm in this class
		 * is based on an implementation from
		 * Josh Bloch's "Effective Java".
		 * (https://stackoverflow.com/a/113600)
		 *
		 * -caleb
		 */

		public const uint HASH_START = 17;
		public const uint HASH_FACTOR = 37;

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

		private Dictionary<uint, BlendState> blendCache =
			new Dictionary<uint, BlendState>();

		/* Private Hashing Functions */

		private static uint GetBlendHash(
			BlendFunction alphaBlendFunc,
			Blend alphaDestBlend,
			Blend alphaSrcBlend,
			BlendFunction colorBlendFunc,
			Blend colorDestBlend,
			Blend colorSrcBlend,
			ColorWriteChannels channels,
			ColorWriteChannels channels1,
			ColorWriteChannels channels2,
			ColorWriteChannels channels3,
			Color blendFactor,
			int multisampleMask
		) {
			uint hash = HASH_START;
			hash = hash * HASH_FACTOR + (uint) alphaBlendFunc;
			hash = hash * HASH_FACTOR + (uint) alphaDestBlend;
			hash = hash * HASH_FACTOR + (uint) alphaSrcBlend;
			hash = hash * HASH_FACTOR + (uint) colorBlendFunc;
			hash = hash * HASH_FACTOR + (uint) colorSrcBlend;
			hash = hash * HASH_FACTOR + (uint) colorDestBlend;
			hash = hash * HASH_FACTOR + (uint) channels;
			hash = hash * HASH_FACTOR + (uint) channels1;
			hash = hash * HASH_FACTOR + (uint) channels2;
			hash = hash * HASH_FACTOR + (uint) channels3;
			hash = hash * HASH_FACTOR + (uint) blendFactor.GetHashCode();
			hash = hash * HASH_FACTOR + (uint) multisampleMask;
			return hash;
		}

		/* Public Functions */

		public static uint GetBlendHash(BlendState state)
		{
			return GetBlendHash(
				state.AlphaBlendFunction,
				state.AlphaDestinationBlend,
				state.AlphaSourceBlend,
				state.ColorBlendFunction,
				state.ColorDestinationBlend,
				state.ColorSourceBlend,
				state.ColorWriteChannels,
				state.ColorWriteChannels1,
				state.ColorWriteChannels2,
				state.ColorWriteChannels3,
				state.BlendFactor,
				state.MultiSampleMask
			);
		}

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
			uint hash = GetBlendHash(
				AlphaBlendFunction,
				AlphaDestinationBlend,
				AlphaSourceBlend,
				ColorBlendFunction,
				ColorDestinationBlend,
				ColorSourceBlend,
				ColorWriteChannels,
				ColorWriteChannels1,
				ColorWriteChannels2,
				ColorWriteChannels3,
				BlendFactor,
				MultiSampleMask
			);
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
#if VERBOSE_PIPELINECACHE
				FNALoggerEXT.LogInfo(
					"New BlendState added to pipeline cache with hash:\n" +
					hash.ToString()
				);
				FNALoggerEXT.LogInfo(
					"Updated size of BlendState cache: " +
					blendCache.Count
				);
			}
			else
			{
				FNALoggerEXT.LogInfo(
					"Retrieved BlendState from pipeline cache with hash:\n" +
					hash.ToString()
				);
#endif
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

		private Dictionary<uint, DepthStencilState> depthStencilCache =
			new Dictionary<uint, DepthStencilState>();

		/* Private Hashing Functions */

		private static uint GetDepthStencilHash(
			bool depthBufferEnable,
			bool depthWriteEnable,
			CompareFunction depthFunc,
			bool stencilEnable,
			CompareFunction stencilFunc,
			StencilOperation stencilPass,
			StencilOperation stencilFail,
			StencilOperation stencilDepthFail,
			bool twoSidedStencil,
			CompareFunction ccwStencilFunc,
			StencilOperation ccwStencilPass,
			StencilOperation ccwStencilFail,
			StencilOperation ccwStencilDepthFail,
			int stencilMask,
			int stencilWriteMask,
			int referenceStencil
		) {
			uint hash = HASH_START;
			hash = hash * HASH_FACTOR + (uint) (depthBufferEnable ? 1 : 0);
			hash = hash * HASH_FACTOR + (uint) (depthWriteEnable ? 1 : 0);
			hash = hash * HASH_FACTOR + (uint) depthFunc;
			hash = hash * HASH_FACTOR + (uint) (stencilEnable ? 1 : 0);
			hash = hash * HASH_FACTOR + (uint) stencilFunc;
			hash = hash * HASH_FACTOR + (uint) stencilPass;
			hash = hash * HASH_FACTOR + (uint) stencilFail;
			hash = hash * HASH_FACTOR + (uint) stencilDepthFail;
			hash = hash * HASH_FACTOR + (uint) (twoSidedStencil ? 1 : 0);
			hash = hash * HASH_FACTOR + (uint) ccwStencilFunc;
			hash = hash * HASH_FACTOR + (uint) ccwStencilPass;
			hash = hash * HASH_FACTOR + (uint) ccwStencilFail;
			hash = hash * HASH_FACTOR + (uint) ccwStencilDepthFail;
			hash = hash * HASH_FACTOR + (uint) stencilMask;
			hash = hash * HASH_FACTOR + (uint) stencilWriteMask;
			hash = hash * HASH_FACTOR + (uint) referenceStencil;
			return hash;
		}

		/* Public Functions */

		public static uint GetDepthStencilHash(DepthStencilState state)
		{
			return GetDepthStencilHash(
				state.DepthBufferEnable,
				state.DepthBufferWriteEnable,
				state.DepthBufferFunction,
				state.StencilEnable,
				state.StencilFunction,
				state.StencilPass,
				state.StencilFail,
				state.StencilDepthBufferFail,
				state.TwoSidedStencilMode,
				state.CounterClockwiseStencilFunction,
				state.CounterClockwiseStencilPass,
				state.CounterClockwiseStencilFail,
				state.CounterClockwiseStencilDepthBufferFail,
				state.StencilMask,
				state.StencilWriteMask,
				state.ReferenceStencil
			);
		}

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
			uint hash = GetDepthStencilHash(
				DepthBufferEnable,
				DepthBufferWriteEnable,
				DepthBufferFunction,
				StencilEnable,
				StencilFunction,
				StencilPass,
				StencilFail,
				StencilDepthBufferFail,
				TwoSidedStencilMode,
				CCWStencilFunction,
				CCWStencilPass,
				CCWStencilFail,
				CCWStencilDepthBufferFail,
				StencilMask,
				StencilWriteMask,
				ReferenceStencil
			);
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
#if VERBOSE_PIPELINECACHE
				FNALoggerEXT.LogInfo(
					"New DepthStencilState added to pipeline cache with hash:\n" +
					hash.ToString()
				);
				FNALoggerEXT.LogInfo(
					"Updated size of DepthStencilState cache: " +
					depthStencilCache.Count
				);
			}
			else
			{
				FNALoggerEXT.LogInfo(
					"Retrieved DepthStencilState from pipeline cache with hash:\n" +
					hash.ToString()
				);
#endif
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

		private Dictionary<uint, RasterizerState> rasterizerCache =
			new Dictionary<uint, RasterizerState>();

		/* Private Hashing Functions */

		private static uint GetRasterizerHash(
			CullMode cullMode,
			FillMode fillMode,
			float depthBias,
			bool msaa,
			bool scissor,
			float slopeScaleDepthBias
		) {
			uint hash = HASH_START;
			hash = hash * HASH_FACTOR + (uint) cullMode;
			hash = hash * HASH_FACTOR + (uint) fillMode;
			hash = hash * HASH_FACTOR + FloatToUInt(depthBias);
			hash = hash * HASH_FACTOR + (uint) (msaa ? 1 : 0);
			hash = hash * HASH_FACTOR + (uint) (scissor ? 1 : 0);
			hash = hash * HASH_FACTOR + FloatToUInt(slopeScaleDepthBias);
			return hash;
		}

		/* Public Functions */

		public static uint GetRasterizerHash(RasterizerState state)
		{
			return GetRasterizerHash(
				state.CullMode,
				state.FillMode,
				state.DepthBias,
				state.MultiSampleAntiAlias,
				state.ScissorTestEnable,
				state.SlopeScaleDepthBias
			);
		}

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
			uint hash = GetRasterizerHash(
				CullMode,
				FillMode,
				DepthBias,
				MultiSampleAntiAlias,
				ScissorTestEnable,
				SlopeScaleDepthBias
			);
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
#if VERBOSE_PIPELINECACHE
				FNALoggerEXT.LogInfo(
					"New RasterizerState added to pipeline cache with hash:\n" +
					hash.ToString()
				);
				FNALoggerEXT.LogInfo(
					"Updated size of RasterizerState cache: " +
					rasterizerCache.Count
				);
			}
			else
			{
				FNALoggerEXT.LogInfo(
					"Retrieved RasterizerState from pipeline cache with hash:\n" +
					hash.ToString()
				);
#endif
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

		private Dictionary<uint, SamplerState> samplerCache =
			new Dictionary<uint, SamplerState>();

		/* Private Hashing Functions */

		private static uint GetSamplerHash(
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			TextureAddressMode addressW,
			int maxAnisotropy,
			int maxMipLevel,
			float mipLODBias,
			TextureFilter filter
		) {
			uint hash = HASH_START;
			hash = hash * HASH_FACTOR + (uint) addressU;
			hash = hash * HASH_FACTOR + (uint) addressV;
			hash = hash * HASH_FACTOR + (uint) addressW;
			hash = hash * HASH_FACTOR + (uint) maxAnisotropy;
			hash = hash * HASH_FACTOR + (uint) maxMipLevel;
			hash = hash * HASH_FACTOR + FloatToUInt(mipLODBias);
			hash = hash * HASH_FACTOR + (uint) filter;
			return hash;
		}

		/* Public Functions */

		public static uint GetSamplerHash(SamplerState state)
		{
			return GetSamplerHash(
				state.AddressU,
				state.AddressV,
				state.AddressW,
				state.MaxAnisotropy,
				state.MaxMipLevel,
				state.MipMapLevelOfDetailBias,
				state.Filter
			);
		}

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
			uint hash = GetSamplerHash(
				AddressU,
				AddressV,
				AddressW,
				MaxAnisotropy,
				MaxMipLevel,
				MipMapLODBias,
				Filter
			);
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
#if VERBOSE_PIPELINECACHE
				FNALoggerEXT.LogInfo(
					"New SamplerState added to pipeline cache with hash:\n" +
					hash.ToString()
				);
				FNALoggerEXT.LogInfo(
					"Updated size of SamplerState cache: " +
					samplerCache.Count
				);
			}
			else
			{
				FNALoggerEXT.LogInfo(
					"Retrieved SamplerState from pipeline cache with hash:\n" +
					hash.ToString()
				);
#endif
			}

			samplers[register] = newSampler;
		}

		#endregion

		#region Private Helper Methods

		private static unsafe uint FloatToUInt(float f)
		{
			return *((uint *) &f);
		}

		#endregion
	}
}
