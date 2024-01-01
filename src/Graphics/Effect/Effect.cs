#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
	{
		#region Public Properties

		private EffectTechnique INTERNAL_currentTechnique;
		public EffectTechnique CurrentTechnique
		{
			get
			{
				return INTERNAL_currentTechnique;
			}
			set
			{
				FNA3D.FNA3D_SetEffectTechnique(
					GraphicsDevice.GLDevice,
					glEffect,
					value.TechniquePointer
				);
				INTERNAL_currentTechnique = value;
			}
		}

		public EffectParameterCollection Parameters
		{
			get;
			private set;
		}

		public EffectTechniqueCollection Techniques
		{
			get;
			private set;
		}

		#endregion

		#region Internal FNA3D Variables

		internal IntPtr glEffect;

		#endregion

		#region Private Variables

		private Dictionary<IntPtr, EffectParameter> samplerMap = new Dictionary<IntPtr, EffectParameter>(new IntPtrBoxlessComparer());
		private class IntPtrBoxlessComparer : IEqualityComparer<IntPtr>
		{
			public bool Equals(IntPtr x, IntPtr y)
			{
				return x == y;
			}

			public int GetHashCode(IntPtr obj)
			{
				return obj.GetHashCode();
			}
		}

		private IntPtr effectData;

		#endregion

		#region Private Static Variables

		private static readonly EffectParameterType[] XNAType = new EffectParameterType[]
		{
			EffectParameterType.Void,	// MOJOSHADER_SYMTYPE_VOID
			EffectParameterType.Bool,	// MOJOSHADER_SYMTYPE_BOOL
			EffectParameterType.Int32,	// MOJOSHADER_SYMTYPE_INT
			EffectParameterType.Single,	// MOJOSHADER_SYMTYPE_FLOAT
			EffectParameterType.String,	// MOJOSHADER_SYMTYPE_STRING
			EffectParameterType.Texture,	// MOJOSHADER_SYMTYPE_TEXTURE
			EffectParameterType.Texture1D,	// MOJOSHADER_SYMTYPE_TEXTURE1D
			EffectParameterType.Texture2D,	// MOJOSHADER_SYMTYPE_TEXTURE2D
			EffectParameterType.Texture3D,	// MOJOSHADER_SYMTYPE_TEXTURE3D
			EffectParameterType.TextureCube	// MOJOSHADER_SYMTYPE_TEXTURECUBE
		};

		private static readonly EffectParameterClass[] XNAClass = new EffectParameterClass[]
		{
			EffectParameterClass.Scalar,	// MOJOSHADER_SYMCLASS_SCALAR
			EffectParameterClass.Vector,	// MOJOSHADER_SYMCLASS_VECTOR
			EffectParameterClass.Matrix,	// MOJOSHADER_SYMCLASS_MATRIX_ROWS
			EffectParameterClass.Matrix,	// MOJOSHADER_SYMCLASS_MATRIX_COLUMNS
			EffectParameterClass.Object,	// MOJOSHADER_SYMCLASS_OBJECT
			EffectParameterClass.Struct	// MOJOSHADER_SYMCLASS_STRUCT
		};

		private static readonly Blend[] XNABlend = new Blend[]
		{
			(Blend) (-1),			// NOPE
			Blend.Zero,			// MOJOSHADER_BLEND_ZERO
			Blend.One,			// MOJOSHADER_BLEND_ONE
			Blend.SourceColor,		// MOJOSHADER_BLEND_SRCCOLOR
			Blend.InverseSourceColor,	// MOJOSHADER_BLEND_INVSRCCOLOR
			Blend.SourceAlpha,		// MOJOSHADER_BLEND_SRCALPHA
			Blend.InverseSourceAlpha,	// MOJOSHADER_BLEND_INVSRCALPHA
			Blend.DestinationAlpha,		// MOJOSHADER_BLEND_DESTALPHA
			Blend.InverseDestinationAlpha,	// MOJOSHADER_BLEND_INVDESTALPHA
			Blend.DestinationColor,		// MOJOSHADER_BLEND_DESTCOLOR
			Blend.InverseDestinationColor,	// MOJOSHADER_BLEND_INVDESTCOLOR
			Blend.SourceAlphaSaturation,	// MOJOSHADER_BLEND_SRCALPHASAT
			(Blend) (-1),			// NOPE
			(Blend) (-1),			// NOPE
			Blend.BlendFactor,		// MOJOSHADER_BLEND_BLENDFACTOR
			Blend.InverseBlendFactor	// MOJOSHADER_BLEND_INVBLENDFACTOR
		};

		private static readonly BlendFunction[] XNABlendOp = new BlendFunction[]
		{
			(BlendFunction) (-1),		// NOPE
			BlendFunction.Add,		// MOJOSHADER_BLENDOP_ADD
			BlendFunction.Subtract,		// MOJOSHADER_BLENDOP_SUBTRACT
			BlendFunction.ReverseSubtract,	// MOJOSHADER_BLENDOP_REVSUBTRACT
			BlendFunction.Min,		// MOJOSHADER_BLENDOP_MIN
			BlendFunction.Max		// MOJOSHADER_BLENDOP_MAX
		};

		private static readonly CompareFunction[] XNACompare = new CompareFunction[]
		{
			(CompareFunction) (-1),		// NOPE
			CompareFunction.Never,		// MOJOSHADER_CMP_NEVER
			CompareFunction.Less,		// MOJOSHADER_CMP_LESS
			CompareFunction.Equal,		// MOJOSHADER_CMP_EQUAL
			CompareFunction.LessEqual,	// MOJOSHADER_CMP_LESSEQUAL
			CompareFunction.Greater,	// MOJOSHADER_CMP_GREATER
			CompareFunction.NotEqual,	// MOJOSHADER_CMP_NOTEQUAL
			CompareFunction.GreaterEqual,	// MOJOSHADER_CMP_GREATEREQUAL
			CompareFunction.Always		// MOJOSHADER_CMP_ALWAYS
		};

		private static readonly StencilOperation[] XNAStencilOp = new StencilOperation[]
		{
			(StencilOperation) (-1),		// NOPE
			StencilOperation.Keep,			// MOJOSHADER_STENCILOP_KEEP
			StencilOperation.Zero,			// MOJOSHADER_STENCILOP_ZERO
			StencilOperation.Replace,		// MOJOSHADER_STENCILOP_REPLACE
			StencilOperation.IncrementSaturation,	// MOJOSHADER_STENCILOP_INCRSAT
			StencilOperation.DecrementSaturation,	// MOJOSHADER_STENCILOP_DECRSAT
			StencilOperation.Invert,		// MOJOSHADER_STENCILOP_INVERT
			StencilOperation.Increment,		// MOJOSHADER_STENCILOP_INCR
			StencilOperation.Decrement		// MOJOSHADER_STENCILOP_DECR
		};

		private static readonly TextureAddressMode[] XNAAddress = new TextureAddressMode[]
		{
			(TextureAddressMode) (-1),	// NOPE
			TextureAddressMode.Wrap,	// MOJOSHADER_TADDRESS_WRAP
			TextureAddressMode.Mirror,	// MOJOSHADER_TADDRESS_MIRROR
			TextureAddressMode.Clamp	// MOJOSHADER_TADDRESS_CLAMP
		};

		private static readonly MOJOSHADER_textureFilterType[] XNAMag =
			new MOJOSHADER_textureFilterType[]
		{
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.Linear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.LinearMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.PointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.MinPointMagLinearMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR		// TextureFilter.MinPointMagLinearMipPoint
		};

		private static readonly MOJOSHADER_textureFilterType[] XNAMin =
			new MOJOSHADER_textureFilterType[]
		{
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.Linear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.LinearMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.PointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.MinLinearMagPointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.MinLinearMagPointMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinPointMagLinearMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT		// TextureFilter.MinPointMagLinearMipPoint
		};

		private static readonly MOJOSHADER_textureFilterType[] XNAMip =
			new MOJOSHADER_textureFilterType[]
		{
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.Linear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.LinearMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.PointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.MinLinearMagPointMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipPoint
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,		// TextureFilter.MinPointMagLinearMipLinear
			MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT		// TextureFilter.MinPointMagLinearMipPoint
		};

		#endregion

		#region Public Constructor

		public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
		{
			GraphicsDevice = graphicsDevice;

			// Send the blob to the GLDevice to be parsed/compiled
			IntPtr effectData;
			FNA3D.FNA3D_CreateEffect(
				graphicsDevice.GLDevice,
				effectCode,
				effectCode.Length,
				out glEffect,
				out effectData
			);

			this.effectData = effectData;

			// This is where it gets ugly...
			INTERNAL_parseEffectStruct(effectData);

			// The default technique is the first technique.
			CurrentTechnique = Techniques[0];
		}

		#endregion

		#region Protected Constructor

		protected Effect(Effect cloneSource)
		{
			GraphicsDevice = cloneSource.GraphicsDevice;

			// Send the parsed data to be cloned and recompiled by MojoShader
			IntPtr effectData;
			FNA3D.FNA3D_CloneEffect(
				GraphicsDevice.GLDevice,
				cloneSource.glEffect,
				out glEffect,
				out effectData
			);

			this.effectData = effectData;

			// Double the ugly, double the fun!
			INTERNAL_parseEffectStruct(effectData);

			// Copy texture parameters, if applicable
			for (int i = 0; i < cloneSource.Parameters.Count; i += 1)
			{
				Parameters[i].texture = cloneSource.Parameters[i].texture;
			}

			// The default technique is whatever the current technique was.
			for (int i = 0; i < cloneSource.Techniques.Count; i += 1)
			{
				if (cloneSource.Techniques[i] == cloneSource.CurrentTechnique)
				{
					CurrentTechnique = Techniques[i];
				}
			}
		}

		#endregion

		#region Public Methods

		public virtual Effect Clone()
		{
			return new Effect(this);
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IntPtr toDispose = Interlocked.Exchange(ref glEffect, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeEffect(
						GraphicsDevice.GLDevice,
						toDispose
					);
				}
			}
			base.Dispose(disposing);
		}

		protected internal virtual void OnApply()
		{
		}

		#endregion

		#region Internal Methods

		internal unsafe void INTERNAL_applyEffect(uint pass)
		{
			FNA3D.FNA3D_ApplyEffect(
				GraphicsDevice.GLDevice,
				glEffect,
				pass,
				GraphicsDevice.effectStateChangesPtr
			);
			MOJOSHADER_effectStateChanges *stateChanges =
				(MOJOSHADER_effectStateChanges*) GraphicsDevice.effectStateChangesPtr;
			if (stateChanges->render_state_change_count > 0)
			{
				PipelineCache pipelineCache = GraphicsDevice.PipelineCache;
				pipelineCache.BeginApplyBlend();
				pipelineCache.BeginApplyDepthStencil();
				pipelineCache.BeginApplyRasterizer();

				// Used to avoid redundant device state application
				bool blendStateChanged = false;
				bool depthStencilStateChanged = false;
				bool rasterizerStateChanged = false;

				MOJOSHADER_effectState* states = (MOJOSHADER_effectState*) stateChanges->render_state_changes;
				for (int i = 0; i < stateChanges->render_state_change_count; i += 1)
				{
					MOJOSHADER_renderStateType type = states[i].type;
					if (	type == MOJOSHADER_renderStateType.MOJOSHADER_RS_VERTEXSHADER ||
						type == MOJOSHADER_renderStateType.MOJOSHADER_RS_PIXELSHADER	)
					{
						// Skip shader states
						continue;
					}

					if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_ZENABLE)
					{
						MOJOSHADER_zBufferType* val = (MOJOSHADER_zBufferType*) states[i].value.values;
						pipelineCache.DepthBufferEnable =
							(*val == MOJOSHADER_zBufferType.MOJOSHADER_ZB_TRUE);
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_FILLMODE)
					{
						MOJOSHADER_fillMode* val = (MOJOSHADER_fillMode*) states[i].value.values;
						if (*val == MOJOSHADER_fillMode.MOJOSHADER_FILL_SOLID)
						{
							pipelineCache.FillMode = FillMode.Solid;
						}
						else if (*val == MOJOSHADER_fillMode.MOJOSHADER_FILL_WIREFRAME)
						{
							pipelineCache.FillMode = FillMode.WireFrame;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_ZWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.DepthBufferWriteEnable = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLEND)
					{
						MOJOSHADER_blendMode* val = (MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.ColorSourceBlend = XNABlend[(int) *val];
						if (!pipelineCache.SeparateAlphaBlend)
						{
							pipelineCache.AlphaSourceBlend = XNABlend[(int) *val];
						}
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLEND)
					{
						MOJOSHADER_blendMode* val = (MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.ColorDestinationBlend = XNABlend[(int) *val];
						if (!pipelineCache.SeparateAlphaBlend)
						{
							pipelineCache.AlphaDestinationBlend = XNABlend[(int) *val];
						}
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_CULLMODE)
					{
						MOJOSHADER_cullMode* val = (MOJOSHADER_cullMode*) states[i].value.values;
						if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_NONE)
						{
							pipelineCache.CullMode = CullMode.None;
						}
						else if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_CW)
						{
							pipelineCache.CullMode = CullMode.CullClockwiseFace;
						}
						else if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_CCW)
						{
							pipelineCache.CullMode = CullMode.CullCounterClockwiseFace;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_ZFUNC)
					{
						MOJOSHADER_compareFunc* val = (MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.DepthBufferFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_ALPHABLENDENABLE)
					{
						// FIXME: Assuming no other blend calls are made in the effect! -flibit
						int* val = (int*) states[i].value.values;
						if (*val == 0)
						{
							pipelineCache.ColorSourceBlend = Blend.One;
							pipelineCache.ColorDestinationBlend = Blend.Zero;
							pipelineCache.AlphaSourceBlend = Blend.One;
							pipelineCache.AlphaDestinationBlend = Blend.Zero;
							blendStateChanged = true;
						}
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilEnable = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFAIL)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILZFAIL)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilDepthBufferFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILPASS)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilPass = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFUNC)
					{
						MOJOSHADER_compareFunc* val = (MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.StencilFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILREF)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ReferenceStencil = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILWRITEMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilWriteMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEANTIALIAS)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.MultiSampleAntiAlias = (*val == 1);
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.MultiSampleMask = *val;
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOP)
					{
						MOJOSHADER_blendOp* val = (MOJOSHADER_blendOp*) states[i].value.values;
						pipelineCache.ColorBlendFunction = XNABlendOp[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_SCISSORTESTENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ScissorTestEnable = (*val == 1);
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_SLOPESCALEDEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						pipelineCache.SlopeScaleDepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_TWOSIDEDSTENCILMODE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.TwoSidedStencilMode = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFAIL)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILZFAIL)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilDepthBufferFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILPASS)
					{
						MOJOSHADER_stencilOp* val = (MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilPass = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFUNC)
					{
						MOJOSHADER_compareFunc* val = (MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.CCWStencilFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE1)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels1 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE2)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels2 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE3)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels3 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDFACTOR)
					{
						// FIXME: RGBA? -flibit
						int* val = (int*) states[i].value.values;
						pipelineCache.BlendFactor = new Color(
							(*val >> 24) & 0xFF,
							(*val >> 16) & 0xFF,
							(*val >> 8) & 0xFF,
							*val & 0xFF
						);
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_DEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						pipelineCache.DepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_SEPARATEALPHABLENDENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.SeparateAlphaBlend = (*val == 1);
						// FIXME: Do we want to update the state for this...? -flibit
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLENDALPHA)
					{
						MOJOSHADER_blendMode* val = (MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.AlphaSourceBlend = XNABlend[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLENDALPHA)
					{
						MOJOSHADER_blendMode* val = (MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.AlphaDestinationBlend = XNABlend[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOPALPHA)
					{
						MOJOSHADER_blendOp* val = (MOJOSHADER_blendOp*) states[i].value.values;
						pipelineCache.AlphaBlendFunction = XNABlendOp[(int) *val];
						blendStateChanged = true;
					}
					else if (type == (MOJOSHADER_renderStateType) 178)
					{
						/* Apparently this is "SetSampler"? */
					}
					else
					{
						throw new NotImplementedException("Unhandled render state! " + type);
					}
				}
				if (blendStateChanged)
				{
					pipelineCache.EndApplyBlend();
				}
				if (depthStencilStateChanged)
				{
					pipelineCache.EndApplyDepthStencil();
				}
				if (rasterizerStateChanged)
				{
					pipelineCache.EndApplyRasterizer();
				}
			}
			if (stateChanges->sampler_state_change_count > 0)
			{
				INTERNAL_updateSamplers(
					stateChanges->sampler_state_change_count,
					(MOJOSHADER_samplerStateRegister*) stateChanges->sampler_state_changes,
					GraphicsDevice.Textures,
					GraphicsDevice.SamplerStates
				);
			}
			if (stateChanges->vertex_sampler_state_change_count > 0)
			{
				INTERNAL_updateSamplers(
					stateChanges->vertex_sampler_state_change_count,
					(MOJOSHADER_samplerStateRegister*) stateChanges->vertex_sampler_state_changes,
					GraphicsDevice.VertexTextures,
					GraphicsDevice.VertexSamplerStates
				);
			}
		}

		private unsafe void INTERNAL_updateSamplers(
			uint changeCount,
			MOJOSHADER_samplerStateRegister* registers,
			TextureCollection textures,
			SamplerStateCollection samplers
		) {
			for (int i = 0; i < changeCount; i += 1)
			{
				if (registers[i].sampler_state_count == 0)
				{
					// Nothing to do
					continue;
				}

				int register = (int) registers[i].sampler_register;

				PipelineCache pipelineCache = GraphicsDevice.PipelineCache;
				pipelineCache.BeginApplySampler(samplers, register);

				// Used to prevent redundant sampler changes
				bool samplerChanged = false;
				bool filterChanged = false;

				// Current sampler filter
				TextureFilter filter = pipelineCache.Filter;
				MOJOSHADER_textureFilterType magFilter = XNAMag[(int) filter];
				MOJOSHADER_textureFilterType minFilter = XNAMin[(int) filter];
				MOJOSHADER_textureFilterType mipFilter = XNAMip[(int) filter];

				MOJOSHADER_effectSamplerState* states = (MOJOSHADER_effectSamplerState*) registers[i].sampler_states;
				for (int j = 0; j < registers[i].sampler_state_count; j += 1)
				{
					MOJOSHADER_samplerStateType type = states[j].type;
					if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_TEXTURE)
					{
						EffectParameter texParam;
						if (samplerMap.TryGetValue(registers[i].sampler_name, out texParam))
						{
							Texture texture = texParam.texture;
							if (texture != null)
							{
								textures[register] = texture;
							}
						}
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSU)
					{
						MOJOSHADER_textureAddress* val = (MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressU = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSV)
					{
						MOJOSHADER_textureAddress* val = (MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressV = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSW)
					{
						MOJOSHADER_textureAddress* val = (MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressW = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAGFILTER)
					{
						MOJOSHADER_textureFilterType* val = (MOJOSHADER_textureFilterType*) states[j].value.values;
						magFilter = *val;
						filterChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MINFILTER)
					{
						MOJOSHADER_textureFilterType* val = (MOJOSHADER_textureFilterType*) states[j].value.values;
						minFilter = *val;
						filterChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPFILTER)
					{
						MOJOSHADER_textureFilterType* val = (MOJOSHADER_textureFilterType*) states[j].value.values;
						mipFilter = *val;
						filterChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPMAPLODBIAS)
					{
						float* val = (float*) states[j].value.values;
						pipelineCache.MipMapLODBias = *val;
						samplerChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXMIPLEVEL)
					{
						int* val = (int*) states[j].value.values;
						pipelineCache.MaxMipLevel = *val;
						samplerChanged = true;
					}
					else if (type == MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXANISOTROPY)
					{
						int* val = (int*) states[j].value.values;
						pipelineCache.MaxAnisotropy = *val;
						samplerChanged = true;
					}
					else
					{
						throw new NotImplementedException("Unhandled sampler state! " + type);
					}
				}
				if (filterChanged)
				{
					if (magFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
					{
						if (minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
						{
							if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.Point;
							}
							else if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.PointMipLinear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else if (	minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
								minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
						{
							if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.MinLinearMagPointMipPoint;
							}
							else if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.MinLinearMagPointMipLinear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else
						{
							throw new NotImplementedException("Unhandled minfilter type! " + minFilter);
						}
					}
					else if (	magFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
							magFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
					{
						if (minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
						{
							if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.MinPointMagLinearMipPoint;
							}
							else if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.MinPointMagLinearMipLinear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else if (	minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
								minFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
						{
							if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.LinearMipPoint;
							}
							else if (	mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.Linear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else
						{
							throw new NotImplementedException("Unhandled minfilter type! " + minFilter);
						}
					}
					else
					{
						throw new NotImplementedException("Unhandled magfilter type! " + magFilter);
					}
					samplerChanged = true;
				}

				if (samplerChanged)
				{
					pipelineCache.EndApplySampler(samplers, register);
				}
			}
		}

		#endregion

		#region Private Methods

		private unsafe void INTERNAL_parseEffectStruct(IntPtr effectData)
		{
			MOJOSHADER_effect* effectPtr = (MOJOSHADER_effect*) effectData;

			// Set up Parameters
			MOJOSHADER_effectParam* paramPtr = (MOJOSHADER_effectParam*) effectPtr->parameters;
			List<EffectParameter> parameters = new List<EffectParameter>();
			for (int i = 0; i < effectPtr->param_count; i += 1)
			{
				MOJOSHADER_effectParam param = paramPtr[i];
				if (	param.value.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VERTEXSHADER ||
					param.value.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_PIXELSHADER	)
				{
					// Skip shader objects...
					continue;
				}
				else if (	param.value.type.parameter_type >= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER &&
						param.value.type.parameter_type <= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLERCUBE	)
				{
					string textureName = String.Empty;
					MOJOSHADER_effectSamplerState* states = (MOJOSHADER_effectSamplerState*) param.value.values;
					for (int j = 0; j < param.value.value_count; j += 1)
					{
						if (	states[j].value.type.parameter_type >= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE &&
							states[j].value.type.parameter_type <= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURECUBE	)
						{
							MOJOSHADER_effectObject *objectPtr = (MOJOSHADER_effectObject*) effectPtr->objects;
							int* index = (int*) states[j].value.values;
							textureName = Marshal.PtrToStringAnsi(objectPtr[*index].mapping.name);
							break;
						}
					}
					/* Because textures have to be declared before the sampler,
					 * we can assume that it will always be in the list by the
					 * time we get to this point.
					 * -flibit
					 */
					for (int j = 0; j < parameters.Count; j += 1)
					{
						if (textureName.Equals(parameters[j].Name))
						{
							samplerMap[param.value.name] = parameters[j];
							break;
						}
					}
					continue;
				}

				EffectParameter toAdd = new EffectParameter(
					MarshalHelper.PtrToInternedStringAnsi(param.value.name),
					MarshalHelper.PtrToInternedStringAnsi(param.value.semantic),
					(int) param.value.type.rows,
					(int) param.value.type.columns,
					(int) param.value.type.elements,
					XNAClass[(int) param.value.type.parameter_class],
					XNAType[(int) param.value.type.parameter_type],
					new IntPtr(&paramPtr[i].value.type),
					INTERNAL_readAnnotations(
						param.annotations,
						param.annotation_count
					),
					param.value.values,
					param.value.value_count * sizeof(float),
					this
				);
				if (param.value.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_STRING)
				{
					int* index = (int*) param.value.values;
					toAdd.cachedString = INTERNAL_GetStringFromObjectTable(*index);
				}
				parameters.Add(toAdd);

			}
			Parameters = new EffectParameterCollection(parameters);

			// Set up Techniques
			MOJOSHADER_effectTechnique* techPtr = (MOJOSHADER_effectTechnique*) effectPtr->techniques;
			List<EffectTechnique> techniques = new List<EffectTechnique>(effectPtr->technique_count);
			for (int i = 0; i < techniques.Capacity; i += 1, techPtr += 1)
			{
				// Set up Passes
				MOJOSHADER_effectPass* passPtr = (MOJOSHADER_effectPass*) techPtr->passes;
				EffectPassCollection passes;
				if (techPtr->pass_count == 1)
				{
					passes = new EffectPassCollection(INTERNAL_readPass(
						ref passPtr[0], (IntPtr) techPtr, 0
					));
				}
				else
				{
					List<EffectPass> passList = new List<EffectPass>((int) techPtr->pass_count);
					for (int j = 0; j < passList.Capacity; j += 1)
					{
						passList.Add(INTERNAL_readPass(ref passPtr[j], (IntPtr) techPtr, (uint) j));
					}
					passes = new EffectPassCollection(passList);
				}

				techniques.Add(new EffectTechnique(
					MarshalHelper.PtrToInternedStringAnsi(techPtr->name),
					(IntPtr) techPtr,
					passes,
					INTERNAL_readAnnotations(
						techPtr->annotations,
						techPtr->annotation_count
					)
				));
			}
			Techniques = new EffectTechniqueCollection(techniques);
		}

		internal unsafe static EffectParameterCollection INTERNAL_readEffectParameterStructureMembers(
			EffectParameter parameter,
			IntPtr _type,
			Effect outer
		) {
			if (_type == IntPtr.Zero)
			{
				return null;
			}

			var type = *(MOJOSHADER_symbolTypeInfo*) _type;
			EffectParameterCollection structMembers = null;
			if (type.member_count > 0)
			{
				List<EffectParameter> memList = new List<EffectParameter>();
				unsafe
				{
					MOJOSHADER_symbolStructMember* mem = (MOJOSHADER_symbolStructMember*) type.members;
					IntPtr curOffset = IntPtr.Zero;
					for (int j = 0; j < type.member_count; j += 1)
					{
						uint memSize = mem[j].info.rows * mem[j].info.columns;
						if (mem[j].info.elements > 0)
						{
							memSize *= mem[j].info.elements;
						}
						EffectParameter toAdd = new EffectParameter(
							MarshalHelper.PtrToInternedStringAnsi(mem[j].name),
							null,
							(int) mem[j].info.rows,
							(int) mem[j].info.columns,
							(int) mem[j].info.elements,
							XNAClass[(int) mem[j].info.parameter_class],
							XNAType[(int) mem[j].info.parameter_type],
							null, // FIXME: Nested structs! -flibit
							null,
							parameter.values + curOffset.ToInt32(),
							memSize * 4,
							outer
						);

						if (mem[j].info.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_STRING)
						{
							int* index = (int*) (parameter.values + curOffset.ToInt32());
							toAdd.cachedString = outer.INTERNAL_GetStringFromObjectTable(*index);
						}
						memList.Add(toAdd);
						curOffset += (int) memSize * 4;
					}
				}
				structMembers = new EffectParameterCollection(memList);
			}

			return structMembers;
		}

		private unsafe string INTERNAL_GetStringFromObjectTable(int index)
		{
			MOJOSHADER_effect* effectPtr = (MOJOSHADER_effect*) effectData;
			MOJOSHADER_effectObject* objectsPtr = (MOJOSHADER_effectObject*) effectPtr->objects;
			if (index < effectPtr->object_count)
			{
				return Marshal.PtrToStringAnsi(objectsPtr[index].stringvalue.stringvalue);
			}
			else
			{
				throw new InvalidOperationException("Invalid effect object index");
			}
		}

		private unsafe EffectPass INTERNAL_readPass(
			ref MOJOSHADER_effectPass pass,
			IntPtr techPtr, uint index
		) {
			return new EffectPass(
				MarshalHelper.PtrToInternedStringAnsi(pass.name),
				INTERNAL_readAnnotations(
					pass.annotations,
					pass.annotation_count
				),
				this,
				techPtr,
				index
			);
		}

		private unsafe EffectAnnotationCollection INTERNAL_readAnnotations(
			IntPtr rawAnnotations,
			uint numAnnotations
		) {
			if (numAnnotations == 0)
			{
				return EffectAnnotationCollection.Empty;
			}

			MOJOSHADER_effectAnnotation* annoPtr = (MOJOSHADER_effectAnnotation*) rawAnnotations;
			List<EffectAnnotation> annotations = new List<EffectAnnotation>((int) numAnnotations);
			for (int i = 0; i < numAnnotations; i += 1)
			{
				MOJOSHADER_effectAnnotation anno = annoPtr[i];

				EffectAnnotation toAdd = new EffectAnnotation(
					MarshalHelper.PtrToInternedStringAnsi(anno.name),
					MarshalHelper.PtrToInternedStringAnsi(anno.semantic),
					(int) anno.type.rows,
					(int) anno.type.columns,
					XNAClass[(int) anno.type.parameter_class],
					XNAType[(int) anno.type.parameter_type],
					anno.values
				);
				if (anno.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_STRING)
				{
					int* index = (int*) anno.values;
					toAdd.cachedString = INTERNAL_GetStringFromObjectTable(*index);
				}
				annotations.Add(toAdd);
			}
			return new EffectAnnotationCollection(annotations);
		}

		#endregion

		#region MojoShader Interop

		/* Shader Parse Interface */

		private enum MOJOSHADER_symbolClass
		{
			MOJOSHADER_SYMCLASS_SCALAR = 0,
			MOJOSHADER_SYMCLASS_VECTOR,
			MOJOSHADER_SYMCLASS_MATRIX_ROWS,
			MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
			MOJOSHADER_SYMCLASS_OBJECT,
			MOJOSHADER_SYMCLASS_STRUCT,
			MOJOSHADER_SYMCLASS_TOTAL
		}

		private enum MOJOSHADER_symbolType
		{
			MOJOSHADER_SYMTYPE_VOID = 0,
			MOJOSHADER_SYMTYPE_BOOL,
			MOJOSHADER_SYMTYPE_INT,
			MOJOSHADER_SYMTYPE_FLOAT,
			MOJOSHADER_SYMTYPE_STRING,
			MOJOSHADER_SYMTYPE_TEXTURE,
			MOJOSHADER_SYMTYPE_TEXTURE1D,
			MOJOSHADER_SYMTYPE_TEXTURE2D,
			MOJOSHADER_SYMTYPE_TEXTURE3D,
			MOJOSHADER_SYMTYPE_TEXTURECUBE,
			MOJOSHADER_SYMTYPE_SAMPLER,
			MOJOSHADER_SYMTYPE_SAMPLER1D,
			MOJOSHADER_SYMTYPE_SAMPLER2D,
			MOJOSHADER_SYMTYPE_SAMPLER3D,
			MOJOSHADER_SYMTYPE_SAMPLERCUBE,
			MOJOSHADER_SYMTYPE_PIXELSHADER,
			MOJOSHADER_SYMTYPE_VERTEXSHADER,
			MOJOSHADER_SYMTYPE_PIXELFRAGMENT,
			MOJOSHADER_SYMTYPE_VERTEXFRAGMENT,
			MOJOSHADER_SYMTYPE_UNSUPPORTED,
			MOJOSHADER_SYMTYPE_TOTAL
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_symbolTypeInfo
		{
			public MOJOSHADER_symbolClass parameter_class;
			public MOJOSHADER_symbolType parameter_type;
			public uint rows;
			public uint columns;
			public uint elements;
			public uint member_count;
			public IntPtr members; // MOJOSHADER_symbolStructMember*
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_symbolStructMember
		{
			public IntPtr name; //const char*
			public MOJOSHADER_symbolTypeInfo info;
		}

		/* MOJOSHADER_effectState types... */

		private enum MOJOSHADER_renderStateType
		{
			MOJOSHADER_RS_ZENABLE,
			MOJOSHADER_RS_FILLMODE,
			MOJOSHADER_RS_SHADEMODE,
			MOJOSHADER_RS_ZWRITEENABLE,
			MOJOSHADER_RS_ALPHATESTENABLE,
			MOJOSHADER_RS_LASTPIXEL,
			MOJOSHADER_RS_SRCBLEND,
			MOJOSHADER_RS_DESTBLEND,
			MOJOSHADER_RS_CULLMODE,
			MOJOSHADER_RS_ZFUNC,
			MOJOSHADER_RS_ALPHAREF,
			MOJOSHADER_RS_ALPHAFUNC,
			MOJOSHADER_RS_DITHERENABLE,
			MOJOSHADER_RS_ALPHABLENDENABLE,
			MOJOSHADER_RS_FOGENABLE,
			MOJOSHADER_RS_SPECULARENABLE,
			MOJOSHADER_RS_FOGCOLOR,
			MOJOSHADER_RS_FOGTABLEMODE,
			MOJOSHADER_RS_FOGSTART,
			MOJOSHADER_RS_FOGEND,
			MOJOSHADER_RS_FOGDENSITY,
			MOJOSHADER_RS_RANGEFOGENABLE,
			MOJOSHADER_RS_STENCILENABLE,
			MOJOSHADER_RS_STENCILFAIL,
			MOJOSHADER_RS_STENCILZFAIL,
			MOJOSHADER_RS_STENCILPASS,
			MOJOSHADER_RS_STENCILFUNC,
			MOJOSHADER_RS_STENCILREF,
			MOJOSHADER_RS_STENCILMASK,
			MOJOSHADER_RS_STENCILWRITEMASK,
			MOJOSHADER_RS_TEXTUREFACTOR,
			MOJOSHADER_RS_WRAP0,
			MOJOSHADER_RS_WRAP1,
			MOJOSHADER_RS_WRAP2,
			MOJOSHADER_RS_WRAP3,
			MOJOSHADER_RS_WRAP4,
			MOJOSHADER_RS_WRAP5,
			MOJOSHADER_RS_WRAP6,
			MOJOSHADER_RS_WRAP7,
			MOJOSHADER_RS_WRAP8,
			MOJOSHADER_RS_WRAP9,
			MOJOSHADER_RS_WRAP10,
			MOJOSHADER_RS_WRAP11,
			MOJOSHADER_RS_WRAP12,
			MOJOSHADER_RS_WRAP13,
			MOJOSHADER_RS_WRAP14,
			MOJOSHADER_RS_WRAP15,
			MOJOSHADER_RS_CLIPPING,
			MOJOSHADER_RS_LIGHTING,
			MOJOSHADER_RS_AMBIENT,
			MOJOSHADER_RS_FOGVERTEXMODE,
			MOJOSHADER_RS_COLORVERTEX,
			MOJOSHADER_RS_LOCALVIEWER,
			MOJOSHADER_RS_NORMALIZENORMALS,
			MOJOSHADER_RS_DIFFUSEMATERIALSOURCE,
			MOJOSHADER_RS_SPECULARMATERIALSOURCE,
			MOJOSHADER_RS_AMBIENTMATERIALSOURCE,
			MOJOSHADER_RS_EMISSIVEMATERIALSOURCE,
			MOJOSHADER_RS_VERTEXBLEND,
			MOJOSHADER_RS_CLIPPLANEENABLE,
			MOJOSHADER_RS_POINTSIZE,
			MOJOSHADER_RS_POINTSIZE_MIN,
			MOJOSHADER_RS_POINTSPRITEENABLE,
			MOJOSHADER_RS_POINTSCALEENABLE,
			MOJOSHADER_RS_POINTSCALE_A,
			MOJOSHADER_RS_POINTSCALE_B,
			MOJOSHADER_RS_POINTSCALE_C,
			MOJOSHADER_RS_MULTISAMPLEANTIALIAS,
			MOJOSHADER_RS_MULTISAMPLEMASK,
			MOJOSHADER_RS_PATCHEDGESTYLE,
			MOJOSHADER_RS_DEBUGMONITORTOKEN,
			MOJOSHADER_RS_POINTSIZE_MAX,
			MOJOSHADER_RS_INDEXEDVERTEXBLENDENABLE,
			MOJOSHADER_RS_COLORWRITEENABLE,
			MOJOSHADER_RS_TWEENFACTOR,
			MOJOSHADER_RS_BLENDOP,
			MOJOSHADER_RS_POSITIONDEGREE,
			MOJOSHADER_RS_NORMALDEGREE,
			MOJOSHADER_RS_SCISSORTESTENABLE,
			MOJOSHADER_RS_SLOPESCALEDEPTHBIAS,
			MOJOSHADER_RS_ANTIALIASEDLINEENABLE,
			MOJOSHADER_RS_MINTESSELLATIONLEVEL,
			MOJOSHADER_RS_MAXTESSELLATIONLEVEL,
			MOJOSHADER_RS_ADAPTIVETESS_X,
			MOJOSHADER_RS_ADAPTIVETESS_Y,
			MOJOSHADER_RS_ADAPTIVETESS_Z,
			MOJOSHADER_RS_ADAPTIVETESS_W,
			MOJOSHADER_RS_ENABLEADAPTIVETESSELLATION,
			MOJOSHADER_RS_TWOSIDEDSTENCILMODE,
			MOJOSHADER_RS_CCW_STENCILFAIL,
			MOJOSHADER_RS_CCW_STENCILZFAIL,
			MOJOSHADER_RS_CCW_STENCILPASS,
			MOJOSHADER_RS_CCW_STENCILFUNC,
			MOJOSHADER_RS_COLORWRITEENABLE1,
			MOJOSHADER_RS_COLORWRITEENABLE2,
			MOJOSHADER_RS_COLORWRITEENABLE3,
			MOJOSHADER_RS_BLENDFACTOR,
			MOJOSHADER_RS_SRGBWRITEENABLE,
			MOJOSHADER_RS_DEPTHBIAS,
			MOJOSHADER_RS_SEPARATEALPHABLENDENABLE,
			MOJOSHADER_RS_SRCBLENDALPHA,
			MOJOSHADER_RS_DESTBLENDALPHA,
			MOJOSHADER_RS_BLENDOPALPHA,
			MOJOSHADER_RS_VERTEXSHADER = 146,
			MOJOSHADER_RS_PIXELSHADER = 147
		}

		private enum MOJOSHADER_zBufferType
		{
			MOJOSHADER_ZB_FALSE,
			MOJOSHADER_ZB_TRUE,
			MOJOSHADER_ZB_USEW
		}

		private enum MOJOSHADER_fillMode
		{
			MOJOSHADER_FILL_POINT		= 1,
			MOJOSHADER_FILL_WIREFRAME	= 2,
			MOJOSHADER_FILL_SOLID		= 3
		}

		private enum MOJOSHADER_blendMode
		{
			MOJOSHADER_BLEND_ZERO			= 1,
			MOJOSHADER_BLEND_ONE			= 2,
			MOJOSHADER_BLEND_SRCCOLOR		= 3,
			MOJOSHADER_BLEND_INVSRCCOLOR		= 4,
			MOJOSHADER_BLEND_SRCALPHA		= 5,
			MOJOSHADER_BLEND_INVSRCALPHA		= 6,
			MOJOSHADER_BLEND_DESTALPHA		= 7,
			MOJOSHADER_BLEND_INVDESTALPHA		= 8,
			MOJOSHADER_BLEND_DESTCOLOR		= 9,
			MOJOSHADER_BLEND_INVDESTCOLOR		= 10,
			MOJOSHADER_BLEND_SRCALPHASAT		= 11,
			MOJOSHADER_BLEND_BOTHSRCALPHA		= 12,
			MOJOSHADER_BLEND_BOTHINVSRCALPHA	= 13,
			MOJOSHADER_BLEND_BLENDFACTOR		= 14,
			MOJOSHADER_BLEND_INVBLENDFACTOR		= 15,
			MOJOSHADER_BLEND_SRCCOLOR2		= 16,
			MOJOSHADER_BLEND_INVSRCCOLOR2		= 17
		}

		private enum MOJOSHADER_cullMode
		{
			MOJOSHADER_CULL_NONE	= 1,
			MOJOSHADER_CULL_CW	= 2,
			MOJOSHADER_CULL_CCW	= 3
		}

		private enum MOJOSHADER_compareFunc
		{
			MOJOSHADER_CMP_NEVER		= 1,
			MOJOSHADER_CMP_LESS		= 2,
			MOJOSHADER_CMP_EQUAL		= 3,
			MOJOSHADER_CMP_LESSEQUAL	= 4,
			MOJOSHADER_CMP_GREATER		= 5,
			MOJOSHADER_CMP_NOTEQUAL		= 6,
			MOJOSHADER_CMP_GREATEREQUAL	= 7,
			MOJOSHADER_CMP_ALWAYS		= 8
		}

		private enum MOJOSHADER_stencilOp
		{
			MOJOSHADER_STENCILOP_KEEP	= 1,
			MOJOSHADER_STENCILOP_ZERO	= 2,
			MOJOSHADER_STENCILOP_REPLACE	= 3,
			MOJOSHADER_STENCILOP_INCRSAT	= 4,
			MOJOSHADER_STENCILOP_DECRSAT	= 5,
			MOJOSHADER_STENCILOP_INVERT	= 6,
			MOJOSHADER_STENCILOP_INCR	= 7,
			MOJOSHADER_STENCILOP_DECR	= 8
		}

		private enum MOJOSHADER_blendOp
		{
			MOJOSHADER_BLENDOP_ADD		= 1,
			MOJOSHADER_BLENDOP_SUBTRACT	= 2,
			MOJOSHADER_BLENDOP_REVSUBTRACT	= 3,
			MOJOSHADER_BLENDOP_MIN		= 4,
			MOJOSHADER_BLENDOP_MAX		= 5
		}

		/* MOJOSHADER_effectSamplerState types... */

		private enum MOJOSHADER_samplerStateType
		{
			MOJOSHADER_SAMP_UNKNOWN0	= 0,
			MOJOSHADER_SAMP_UNKNOWN1	= 1,
			MOJOSHADER_SAMP_UNKNOWN2	= 2,
			MOJOSHADER_SAMP_UNKNOWN3	= 3,
			MOJOSHADER_SAMP_TEXTURE		= 4,
			MOJOSHADER_SAMP_ADDRESSU	= 5,
			MOJOSHADER_SAMP_ADDRESSV	= 6,
			MOJOSHADER_SAMP_ADDRESSW	= 7,
			MOJOSHADER_SAMP_BORDERCOLOR	= 8,
			MOJOSHADER_SAMP_MAGFILTER	= 9,
			MOJOSHADER_SAMP_MINFILTER	= 10,
			MOJOSHADER_SAMP_MIPFILTER	= 11,
			MOJOSHADER_SAMP_MIPMAPLODBIAS	= 12,
			MOJOSHADER_SAMP_MAXMIPLEVEL	= 13,
			MOJOSHADER_SAMP_MAXANISOTROPY	= 14,
			MOJOSHADER_SAMP_SRGBTEXTURE	= 15,
			MOJOSHADER_SAMP_ELEMENTINDEX	= 16,
			MOJOSHADER_SAMP_DMAPOFFSET	= 17
		}

		private enum MOJOSHADER_textureAddress
		{
			MOJOSHADER_TADDRESS_WRAP	= 1,
			MOJOSHADER_TADDRESS_MIRROR	= 2,
			MOJOSHADER_TADDRESS_CLAMP	= 3,
			MOJOSHADER_TADDRESS_BORDER	= 4,
			MOJOSHADER_TADDRESS_MIRRORONCE	= 5
		}

		private enum MOJOSHADER_textureFilterType
		{
			MOJOSHADER_TEXTUREFILTER_NONE,
			MOJOSHADER_TEXTUREFILTER_POINT,
			MOJOSHADER_TEXTUREFILTER_LINEAR,
			MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,
			MOJOSHADER_TEXTUREFILTER_PYRAMIDALQUAD,
			MOJOSHADER_TEXTUREFILTER_GAUSSIANQUAD,
			MOJOSHADER_TEXTUREFILTER_CONVOLUTIONMONO
		}

		/* Effect value types... */

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectValue
		{
			public IntPtr name; // const char*
			public IntPtr semantic; // const char*
			public MOJOSHADER_symbolTypeInfo type;
			public uint value_count;
			public IntPtr values; // You know what, just look at the C header...
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectState
		{
			public MOJOSHADER_renderStateType type;
			public MOJOSHADER_effectValue value;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectSamplerState
		{
			public MOJOSHADER_samplerStateType type;
			public MOJOSHADER_effectValue value;
		}

		/* typedef MOJOSHADER_effectValue MOJOSHADER_effectAnnotation; */
		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectAnnotation
		{
			public IntPtr name; // const char*
			public IntPtr semantic; // const char*
			public MOJOSHADER_symbolTypeInfo type;
			public uint value_count;
			public IntPtr values; // You know what, just look at the C header...
		}

		/* Effect interface structures... */

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectParam
		{
			public MOJOSHADER_effectValue value;
			public uint annotation_count;
			public IntPtr annotations; // MOJOSHADER_effectAnnotations*
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectPass
		{
			public IntPtr name; // const char*
			public uint state_count;
			public IntPtr states; // MOJOSHADER_effectState*
			public uint annotation_count;
			public IntPtr annotations; // MOJOSHADER_effectAnnotations*
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectTechnique
		{
			public IntPtr name; // const char*
			public uint pass_count;
			public IntPtr passes; // MOJOSHADER_effectPass*
			public uint annotation_count;
			public IntPtr annotations; // MOJOSHADER_effectAnnotations*
		}

		/* Effect "objects"... */

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectShader
		{
			public MOJOSHADER_symbolType type;
			public uint technique;
			public uint pass;
			public uint is_preshader;
			public uint preshader_param_count;
			public IntPtr preshader_params; // unsigned int*
			public uint param_count;
			public IntPtr parameters; // unsigned int*
			public uint sampler_count;
			public IntPtr samplers; // MOJOSHADER_samplerStateRegister*
			public IntPtr shader; // *shader/*preshader union
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectSamplerMap
		{
			public MOJOSHADER_symbolType type;
			public IntPtr name; // const char*
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectString
		{
			public MOJOSHADER_symbolType type;
			public IntPtr stringvalue; // const char*
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effectTexture
		{
			public MOJOSHADER_symbolType type;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct MOJOSHADER_effectObject
		{
			[FieldOffset(0)]
			public MOJOSHADER_symbolType type;
			[FieldOffset(0)]
			public MOJOSHADER_effectShader shader;
			[FieldOffset(0)]
			public MOJOSHADER_effectSamplerMap mapping;
			[FieldOffset(0)]
			public MOJOSHADER_effectString stringvalue;
			[FieldOffset(0)]
			public MOJOSHADER_effectTexture texture;
		}

		/* Effect state change types... */

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_samplerStateRegister
		{
			public IntPtr sampler_name; // const char*
			public uint sampler_register;
			public uint sampler_state_count;
			public IntPtr sampler_states; // const MOJOSHADER_effectSamplerState*
		}

		// Needed by VideoPlayer...
		[StructLayout(LayoutKind.Sequential)]
		internal struct MOJOSHADER_effectStateChanges
		{
			public uint render_state_change_count;
			public IntPtr render_state_changes; // const MOJOSHADER_effectState*
			public uint sampler_state_change_count;
			public IntPtr sampler_state_changes; // const MOJOSHADER_samplerStateRegister*
			public uint vertex_sampler_state_change_count;
			public IntPtr vertex_sampler_state_changes; // const MOJOSHADER_samplerStateRegister*
		}

		/* Effect parsing interface... this is a partial struct! */

		[StructLayout(LayoutKind.Sequential)]
		private struct MOJOSHADER_effect
		{
			public int error_count;
			public IntPtr errors; // MOJOSHADER_error*
			public int param_count;
			public IntPtr parameters; // MOJOSHADER_effectParam* params, lolC#
			public int technique_count;
			public IntPtr techniques; // MOJOSHADER_effectTechnique*
			public int object_count;
			public IntPtr objects; // MOJOSHADER_effectObject*
		}

		#endregion
	}
}
