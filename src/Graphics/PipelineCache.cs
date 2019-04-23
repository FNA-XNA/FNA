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
	internal class PipelineCache
	{
		#region Private PSO Hash Struct

		[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 128)]
		private struct StateHash : IEquatable<StateHash>
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
				return    Convert.ToString(i1, 2).PadLeft(32, '0')
					+ Convert.ToString(i2, 2).PadLeft(32, '0')
					+ Convert.ToString(i3, 2).PadLeft(32, '0')
					+ Convert.ToString(i4, 2).PadLeft(32, '0');
			}

                        bool IEquatable<StateHash>.Equals(StateHash hash)
                        {
                                return i1 == hash.i1 && i2 == hash.i2 && i3 == hash.i3 && i4 == hash.i4;
                        }

                        override public bool Equals(object obj)
                        {
                                if (obj == null || obj.GetType() != GetType())
                                        return false;

                                StateHash hash = (StateHash) obj;
                                return i1 == hash.i1 && i2 == hash.i2 && i3 == hash.i3 && i4 == hash.i4;
                        }

                        override public int GetHashCode()
                        {
                                return unchecked(i1 + i2 + i3 + i4);
                        }
		}

		#endregion

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
			int funcs = ((int) AlphaBlendFunction << 4) | ((int) ColorBlendFunction);
			int blendsAndColorWriteChannels =
				  ((int) AlphaDestinationBlend	<< (32 - 4))
				| ((int) AlphaSourceBlend	<< (32 - 8))
				| ((int) ColorDestinationBlend	<< (32 - 12))
				| ((int) ColorSourceBlend	<< (32 - 16))
				| ((int) ColorWriteChannels	<< (32 - 20))
				| ((int) ColorWriteChannels1	<< (32 - 24))
				| ((int) ColorWriteChannels2	<< (32 - 28))
				| ((int) ColorWriteChannels3);

			StateHash hash = new StateHash(
				funcs,
				blendsAndColorWriteChannels,
				(int) BlendFactor.PackedValue,
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
				FNALoggerEXT.LogInfo("New BlendState added to pipeline cache with hash:\n"
							+ hash.ToString());
				FNALoggerEXT.LogInfo("Updated size of BlendState cache: "
							+ blendCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo("Retrieved BlendState from pipeline cache with hash:\n"
							+ hash.ToString());
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
			// Bool -> Int32 conversion
			int depthBufferEnable = DepthBufferEnable ? 1 : 0;
			int depthBufferWriteEnable = DepthBufferWriteEnable ? 1 : 0;
			int stencilEnable = StencilEnable ? 1 : 0;
			int twoSidedStencilMode = TwoSidedStencilMode ? 1 : 0;

			int packedProperties =
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

			StateHash hash = new StateHash(
				packedProperties,
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
				FNALoggerEXT.LogInfo("New DepthStencilState added to pipeline cache with hash:\n"
							+ hash.ToString());
				FNALoggerEXT.LogInfo("Updated size of DepthStencilState cache: "
							+ depthStencilCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo("Retrieved DepthStencilState from pipeline cache with hash:\n"
							+ hash.ToString());
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
			// Bool -> Int32 conversion
			int multiSampleAntiAlias = (MultiSampleAntiAlias ? 1 : 0);
			int scissorTestEnable = (ScissorTestEnable ? 1 : 0);

			int packedProperties =
				  ((int) multiSampleAntiAlias	<< 4)
				| ((int) scissorTestEnable	<< 3)
				| ((int) CullMode		<< 1)
				| ((int) FillMode);

			StateHash hash = new StateHash(
				0,
				packedProperties,
				FloatToInt32(DepthBias),
				FloatToInt32(SlopeScaleDepthBias)
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
				FNALoggerEXT.LogInfo("New RasterizerState added to pipeline cache with hash:\n"
							+ hash.ToString());
				FNALoggerEXT.LogInfo("Updated size of RasterizerState cache: "
							+ rasterizerCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo("Retrieved RasterizerState from pipeline cache with hash:\n"
							+ hash.ToString());
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
			int filterAndAddresses =
				  ((int) Filter		<< 6)
				| ((int) AddressU	<< 4)
				| ((int) AddressV	<< 2)
				| ((int) AddressW);

			StateHash hash = new StateHash(
				filterAndAddresses,
				MaxAnisotropy,
				MaxMipLevel,
				FloatToInt32(MipMapLODBias)
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
				FNALoggerEXT.LogInfo("New SamplerState added to pipeline cache with hash:\n"
							+ hash.ToString());
				FNALoggerEXT.LogInfo("Updated size of SamplerState cache: "
							+ samplerCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo("Retrieved SamplerState from pipeline cache with hash:\n"
							+ hash.ToString());
#endif
			}

			samplers[register] = newSampler;
		}

		#endregion

		#region Private Helper Methods

		private unsafe int FloatToInt32(float f)
		{
			return *((int *) &f);
		}

		#endregion
	}
}
