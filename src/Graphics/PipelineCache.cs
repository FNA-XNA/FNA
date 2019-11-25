#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	#region Internal PSO Hash Struct

	[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 128)]
	internal struct StateHash : IEquatable<StateHash>
	{
		readonly int i1;
		readonly int i2;
		readonly int i3;
		readonly int i4;

		public StateHash(int i1, int i2, int i3, int i4)
		{
			this.i1 = i1;
			this.i2 = i2;
			this.i3 = i3;
			this.i4 = i4;
		}

		public override string ToString()
		{
			return	Convert.ToString(i1, 2).PadLeft(32, '0') +
				Convert.ToString(i2, 2).PadLeft(32, '0') +
				Convert.ToString(i3, 2).PadLeft(32, '0') +
				Convert.ToString(i4, 2).PadLeft(32, '0');
		}

		bool IEquatable<StateHash>.Equals(StateHash hash)
		{
			return i1 == hash.i1 && i2 == hash.i2 && i3 == hash.i3 && i4 == hash.i4;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}

			StateHash hash = (StateHash) obj;
			return i1 == hash.i1 && i2 == hash.i2 && i3 == hash.i3 && i4 == hash.i4;
		}

		public override int GetHashCode()
		{
			return unchecked(i1 + i2 + i3 + i4);
		}
	}

	#endregion

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

		private Dictionary<StateHash, BlendState> blendCache =
			new Dictionary<StateHash, BlendState>();

		/* Private Hashing Functions */

		private static StateHash GetBlendHash(
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
			int funcs = ((int) alphaBlendFunc << 4) | ((int) colorBlendFunc);
			int blendsAndColorWriteChannels =
				  ((int) alphaDestBlend	<< (32 - 4))
				| ((int) alphaSrcBlend	<< (32 - 8))
				| ((int) colorDestBlend	<< (32 - 12))
				| ((int) colorSrcBlend	<< (32 - 16))
				| ((int) channels	<< (32 - 20))
				| ((int) channels1	<< (32 - 24))
				| ((int) channels2	<< (32 - 28))
				| ((int) channels3);

			return new StateHash(
				funcs,
				blendsAndColorWriteChannels,
				(int) blendFactor.PackedValue,
				multisampleMask
			);
		}

		/* Public Functions */

		public static StateHash GetBlendHash(BlendState state)
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
			StateHash hash = GetBlendHash(
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

		private Dictionary<StateHash, DepthStencilState> depthStencilCache =
			new Dictionary<StateHash, DepthStencilState>();

		/* Private Hashing Functions */

		private static StateHash GetDepthStencilHash(
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
			// Bool -> Int32 conversion
			int zEnable = depthBufferEnable ? 1 : 0;
			int zWriteEnable = depthWriteEnable ? 1 : 0;
			int sEnable = stencilEnable ? 1 : 0;
			int twoSided = twoSidedStencil ? 1 : 0;

			int packedProperties =
				  ((int) zEnable		<< 32 - 2)
				| ((int) zWriteEnable		<< 32 - 3)
				| ((int) sEnable		<< 32 - 4)
				| ((int) twoSided		<< 32 - 5)
				| ((int) depthFunc		<< 32 - 8)
				| ((int) stencilFunc		<< 32 - 11)
				| ((int) ccwStencilFunc		<< 32 - 14)
				| ((int) stencilPass		<< 32 - 17)
				| ((int) stencilFail		<< 32 - 20)
				| ((int) stencilDepthFail	<< 32 - 23)
				| ((int) ccwStencilFail		<< 32 - 26)
				| ((int) ccwStencilPass		<< 32 - 29)
				| ((int) ccwStencilDepthFail);

			return new StateHash(
				packedProperties,
				stencilMask,
				stencilWriteMask,
				referenceStencil
			);
		}

		/* Public Functions */

		public static StateHash GetDepthStencilHash(DepthStencilState state)
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
			StateHash hash = GetDepthStencilHash(
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

		private Dictionary<StateHash, RasterizerState> rasterizerCache =
			new Dictionary<StateHash, RasterizerState>();

		/* Private Hashing Functions */

		private static StateHash GetRasterizerHash(
			CullMode cullMode,
			FillMode fillMode,
			float depthBias,
			bool msaa,
			bool scissor,
			float slopeScaleDepthBias
		) {
			// Bool -> Int32 conversion
			int multiSampleAntiAlias = (msaa ? 1 : 0);
			int scissorTestEnable = (scissor ? 1 : 0);

			int packedProperties =
				  ((int) multiSampleAntiAlias	<< 4)
				| ((int) scissorTestEnable	<< 3)
				| ((int) cullMode		<< 1)
				| ((int) fillMode);

			return new StateHash(
				0,
				packedProperties,
				FloatToInt32(depthBias),
				FloatToInt32(slopeScaleDepthBias)
			);
		}

		/* Public Functions */

		public static StateHash GetRasterizerHash(RasterizerState state)
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
			StateHash hash = GetRasterizerHash(
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

		private Dictionary<StateHash, SamplerState> samplerCache =
			new Dictionary<StateHash, SamplerState>();

		/* Private Hashing Functions */

		private static StateHash GetSamplerHash(
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			TextureAddressMode addressW,
			int maxAnisotropy,
			int maxMipLevel,
			float mipLODBias,
			TextureFilter filter
		) {
			int filterAndAddresses =
				  ((int) filter		<< 6)
				| ((int) addressU	<< 4)
				| ((int) addressV	<< 2)
				| ((int) addressW);

			return new StateHash(
				filterAndAddresses,
				maxAnisotropy,
				maxMipLevel,
				FloatToInt32(mipLODBias)
			);
		}

		/* Public Functions */

		public static StateHash GetSamplerHash(SamplerState state)
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
			StateHash hash = GetSamplerHash(
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

		private static unsafe int FloatToInt32(float f)
		{
			return *((int *) &f);
		}

		#endregion
	}
}
