#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
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
				MojoShader.MOJOSHADER_effectSetTechnique(
					glEffect.EffectData,
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

		#region Internal Variables

		internal IGLEffect glEffect;

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

		private IntPtr stateChangesPtr;

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

		private static readonly MojoShader.MOJOSHADER_textureFilterType[] XNAMag =
			new MojoShader.MOJOSHADER_textureFilterType[]
		{
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.Linear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.LinearMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.PointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.MinPointMagLinearMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR		// TextureFilter.MinPointMagLinearMipPoint
		};

		private static readonly MojoShader.MOJOSHADER_textureFilterType[] XNAMin =
			new MojoShader.MOJOSHADER_textureFilterType[]
		{
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.Linear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.LinearMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.PointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.MinLinearMagPointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.MinLinearMagPointMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinPointMagLinearMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT		// TextureFilter.MinPointMagLinearMipPoint
		};

		private static readonly MojoShader.MOJOSHADER_textureFilterType[] XNAMip =
			new MojoShader.MOJOSHADER_textureFilterType[]
		{
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.Linear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.Point
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,	// TextureFilter.Anisotropic
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.LinearMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.PointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.MinLinearMagPointMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT,		// TextureFilter.MinLinearMagPointMipPoint
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR,	// TextureFilter.MinPointMagLinearMipLinear
			MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT		// TextureFilter.MinPointMagLinearMipPoint
		};

		#endregion

		#region Public Constructor

		public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
		{
			GraphicsDevice = graphicsDevice;

			// Send the blob to the GLDevice to be parsed/compiled
			glEffect = graphicsDevice.GLDevice.CreateEffect(effectCode);

			// This is where it gets ugly...
			INTERNAL_parseEffectStruct();

			// The default technique is the first technique.
			CurrentTechnique = Techniques[0];

			// Use native memory for changes, .NET loves moving this around
			unsafe
			{
				stateChangesPtr = Marshal.AllocHGlobal(
					sizeof(MojoShader.MOJOSHADER_effectStateChanges)
				);
				MojoShader.MOJOSHADER_effectStateChanges *stateChanges =
					(MojoShader.MOJOSHADER_effectStateChanges*) stateChangesPtr;
				stateChanges->render_state_change_count = 0;
				stateChanges->sampler_state_change_count = 0;
				stateChanges->vertex_sampler_state_change_count = 0;
			}
		}

		#endregion

		#region Protected Constructor

		protected Effect(Effect cloneSource)
		{
			GraphicsDevice = cloneSource.GraphicsDevice;

			// Send the parsed data to be cloned and recompiled by MojoShader
			glEffect = GraphicsDevice.GLDevice.CloneEffect(
				cloneSource.glEffect
			);

			// Double the ugly, double the fun!
			INTERNAL_parseEffectStruct();

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

			// Use native memory for changes, .NET loves moving this around
			unsafe
			{
				stateChangesPtr = Marshal.AllocHGlobal(
					sizeof(MojoShader.MOJOSHADER_effectStateChanges)
				);
				MojoShader.MOJOSHADER_effectStateChanges *stateChanges =
					(MojoShader.MOJOSHADER_effectStateChanges*) stateChangesPtr;
				stateChanges->render_state_change_count = 0;
				stateChanges->sampler_state_change_count = 0;
				stateChanges->vertex_sampler_state_change_count = 0;
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
				if (glEffect != null)
				{
					GraphicsDevice.GLDevice.AddDisposeEffect(glEffect);
				}
				if (stateChangesPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(stateChangesPtr);
					stateChangesPtr = IntPtr.Zero;
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
			GraphicsDevice.GLDevice.ApplyEffect(
				glEffect,
				CurrentTechnique.TechniquePointer,
				pass,
				stateChangesPtr
			);
			MojoShader.MOJOSHADER_effectStateChanges *stateChanges =
				(MojoShader.MOJOSHADER_effectStateChanges*) stateChangesPtr;
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

				MojoShader.MOJOSHADER_effectState* states = (MojoShader.MOJOSHADER_effectState*) stateChanges->render_state_changes;
				for (int i = 0; i < stateChanges->render_state_change_count; i += 1)
				{
					MojoShader.MOJOSHADER_renderStateType type = states[i].type;
					if (	type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_VERTEXSHADER ||
						type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_PIXELSHADER	)
					{
						// Skip shader states
						continue;
					}

					if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZENABLE)
					{
						MojoShader.MOJOSHADER_zBufferType* val = (MojoShader.MOJOSHADER_zBufferType*) states[i].value.values;
						pipelineCache.DepthBufferEnable =
							(*val == MojoShader.MOJOSHADER_zBufferType.MOJOSHADER_ZB_TRUE);
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_FILLMODE)
					{
						MojoShader.MOJOSHADER_fillMode* val = (MojoShader.MOJOSHADER_fillMode*) states[i].value.values;
						if (*val == MojoShader.MOJOSHADER_fillMode.MOJOSHADER_FILL_SOLID)
						{
							pipelineCache.FillMode = FillMode.Solid;
						}
						else if (*val == MojoShader.MOJOSHADER_fillMode.MOJOSHADER_FILL_WIREFRAME)
						{
							pipelineCache.FillMode = FillMode.WireFrame;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.DepthBufferWriteEnable = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLEND)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.ColorSourceBlend = XNABlend[(int) *val];
						if (!pipelineCache.SeparateAlphaBlend)
						{
							pipelineCache.AlphaSourceBlend = XNABlend[(int) *val];
						}
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLEND)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.ColorDestinationBlend = XNABlend[(int) *val];
						if (!pipelineCache.SeparateAlphaBlend)
						{
							pipelineCache.AlphaDestinationBlend = XNABlend[(int) *val];
						}
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CULLMODE)
					{
						MojoShader.MOJOSHADER_cullMode* val = (MojoShader.MOJOSHADER_cullMode*) states[i].value.values;
						if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_NONE)
						{
							pipelineCache.CullMode = CullMode.None;
						}
						else if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_CW)
						{
							pipelineCache.CullMode = CullMode.CullClockwiseFace;
						}
						else if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_CCW)
						{
							pipelineCache.CullMode = CullMode.CullCounterClockwiseFace;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.DepthBufferFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ALPHABLENDENABLE)
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
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilEnable = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILZFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilDepthBufferFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILPASS)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.StencilPass = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.StencilFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILREF)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ReferenceStencil = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILWRITEMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.StencilWriteMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEANTIALIAS)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.MultiSampleAntiAlias = (*val == 1);
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEMASK)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.MultiSampleMask = *val;
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOP)
					{
						MojoShader.MOJOSHADER_blendOp* val = (MojoShader.MOJOSHADER_blendOp*) states[i].value.values;
						pipelineCache.ColorBlendFunction = XNABlendOp[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SCISSORTESTENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ScissorTestEnable = (*val == 1);
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SLOPESCALEDEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						pipelineCache.SlopeScaleDepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_TWOSIDEDSTENCILMODE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.TwoSidedStencilMode = (*val == 1);
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILZFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilDepthBufferFail = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILPASS)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						pipelineCache.CCWStencilPass = XNAStencilOp[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						pipelineCache.CCWStencilFunction = XNACompare[(int) *val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE1)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels1 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE2)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels2 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE3)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.ColorWriteChannels3 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDFACTOR)
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
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						pipelineCache.DepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SEPARATEALPHABLENDENABLE)
					{
						int* val = (int*) states[i].value.values;
						pipelineCache.SeparateAlphaBlend = (*val == 1);
						// FIXME: Do we want to update the state for this...? -flibit
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLENDALPHA)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.AlphaSourceBlend = XNABlend[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLENDALPHA)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						pipelineCache.AlphaDestinationBlend = XNABlend[(int) *val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOPALPHA)
					{
						MojoShader.MOJOSHADER_blendOp* val = (MojoShader.MOJOSHADER_blendOp*) states[i].value.values;
						pipelineCache.AlphaBlendFunction = XNABlendOp[(int) *val];
						blendStateChanged = true;
					}
					else if (type == (MojoShader.MOJOSHADER_renderStateType) 178)
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
					(MojoShader.MOJOSHADER_samplerStateRegister*) stateChanges->sampler_state_changes,
					GraphicsDevice.Textures,
					GraphicsDevice.SamplerStates
				);
			}
			if (stateChanges->vertex_sampler_state_change_count > 0)
			{
				INTERNAL_updateSamplers(
					stateChanges->vertex_sampler_state_change_count,
					(MojoShader.MOJOSHADER_samplerStateRegister*) stateChanges->vertex_sampler_state_changes,
					GraphicsDevice.VertexTextures,
					GraphicsDevice.VertexSamplerStates
				);
			}
		}

		private unsafe void INTERNAL_updateSamplers(
			uint changeCount,
			MojoShader.MOJOSHADER_samplerStateRegister* registers,
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
				MojoShader.MOJOSHADER_textureFilterType magFilter = XNAMag[(int) filter];
				MojoShader.MOJOSHADER_textureFilterType minFilter = XNAMin[(int) filter];
				MojoShader.MOJOSHADER_textureFilterType mipFilter = XNAMip[(int) filter];

				MojoShader.MOJOSHADER_effectSamplerState* states = (MojoShader.MOJOSHADER_effectSamplerState*) registers[i].sampler_states;
				for (int j = 0; j < registers[i].sampler_state_count; j += 1)
				{
					MojoShader.MOJOSHADER_samplerStateType type = states[j].type;
					if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_TEXTURE)
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
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSU)
					{
						MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressU = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSV)
					{
						MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressV = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSW)
					{
						MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
						pipelineCache.AddressW = XNAAddress[(int) *val];
						samplerChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAGFILTER)
					{
						MojoShader.MOJOSHADER_textureFilterType* val = (MojoShader.MOJOSHADER_textureFilterType*) states[j].value.values;
						magFilter = *val;
						filterChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MINFILTER)
					{
						MojoShader.MOJOSHADER_textureFilterType* val = (MojoShader.MOJOSHADER_textureFilterType*) states[j].value.values;
						minFilter = *val;
						filterChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPFILTER)
					{
						MojoShader.MOJOSHADER_textureFilterType* val = (MojoShader.MOJOSHADER_textureFilterType*) states[j].value.values;
						mipFilter = *val;
						filterChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPMAPLODBIAS)
					{
						float* val = (float*) states[j].value.values;
						pipelineCache.MipMapLODBias = *val;
						samplerChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXMIPLEVEL)
					{
						int* val = (int*) states[j].value.values;
						pipelineCache.MaxMipLevel = *val;
						samplerChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXANISOTROPY)
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
					if (magFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
					{
						if (minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
						{
							if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.Point;
							}
							else if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.PointMipLinear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else if (	minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
								minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
						{
							if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.MinLinearMagPointMipPoint;
							}
							else if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
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
					else if (	magFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
							magFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
					{
						if (minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT)
						{
							if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.MinPointMagLinearMipPoint;
							}
							else if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
							{
								pipelineCache.Filter = TextureFilter.MinPointMagLinearMipLinear;
							}
							else
							{
								throw new NotImplementedException("Unhandled mipfilter type! " + mipFilter);
							}
						}
						else if (	minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
								minFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
						{
							if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE ||
								mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT	)
							{
								pipelineCache.Filter = TextureFilter.LinearMipPoint;
							}
							else if (	mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR ||
									mipFilter == MojoShader.MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC	)
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

		private unsafe void INTERNAL_parseEffectStruct()
		{
			MojoShader.MOJOSHADER_effect* effectPtr = (MojoShader.MOJOSHADER_effect*) glEffect.EffectData;

			// Set up Parameters
			MojoShader.MOJOSHADER_effectParam* paramPtr = (MojoShader.MOJOSHADER_effectParam*) effectPtr->parameters;
			List<EffectParameter> parameters = new List<EffectParameter>();
			for (int i = 0; i < effectPtr->param_count; i += 1)
			{
				MojoShader.MOJOSHADER_effectParam param = paramPtr[i];
				if (	param.value.type.parameter_type == MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VERTEXSHADER ||
					param.value.type.parameter_type == MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_PIXELSHADER	)
				{
					// Skip shader objects...
					continue;
				}
				else if (	param.value.type.parameter_type >= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER &&
						param.value.type.parameter_type <= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLERCUBE	)
				{
					string textureName = String.Empty;
					MojoShader.MOJOSHADER_effectSamplerState* states = (MojoShader.MOJOSHADER_effectSamplerState*) param.value.values;
					for (int j = 0; j < param.value.value_count; j += 1)
					{
						if (	states[j].value.type.parameter_type >= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE &&
							states[j].value.type.parameter_type <= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURECUBE	)
						{
							MojoShader.MOJOSHADER_effectObject *objectPtr = (MojoShader.MOJOSHADER_effectObject*) effectPtr->objects;
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

				EffectParameterCollection structMembers = null;
				if (param.value.type.member_count > 0)
				{
					List<EffectParameter> memList = new List<EffectParameter>();
					unsafe
					{
						MojoShader.MOJOSHADER_symbolStructMember* mem = (MojoShader.MOJOSHADER_symbolStructMember*) param.value.type.members;
						IntPtr curOffset = IntPtr.Zero;
						for (int j = 0; j < param.value.type.member_count; j += 1)
						{
							uint memSize = mem[j].info.rows * mem[j].info.columns;
							if (mem[j].info.elements > 0)
							{
								memSize *= mem[j].info.elements;
							}
							memList.Add(new EffectParameter(
								Marshal.PtrToStringAnsi(mem[j].name),
								null,
								(int) mem[j].info.rows,
								(int) mem[j].info.columns,
								(int) mem[j].info.elements,
								XNAClass[(int) mem[j].info.parameter_class],
								XNAType[(int) mem[j].info.parameter_type],
								null, // FIXME: Nested structs! -flibit
								null,
								param.value.values + curOffset.ToInt32(),
								memSize * 4
							));
							curOffset += (int) memSize * 4;
						}
					}
					structMembers = new EffectParameterCollection(memList);
				}

				parameters.Add(new EffectParameter(
					Marshal.PtrToStringAnsi(param.value.name),
					Marshal.PtrToStringAnsi(param.value.semantic),
					(int) param.value.type.rows,
					(int) param.value.type.columns,
					(int) param.value.type.elements,
					XNAClass[(int) param.value.type.parameter_class],
					XNAType[(int) param.value.type.parameter_type],
					structMembers,
					INTERNAL_readAnnotations(
						param.annotations,
						param.annotation_count
					),
					param.value.values,
					param.value.value_count * sizeof(float)
				));
			}
			Parameters = new EffectParameterCollection(parameters);

			// Set up Techniques
			MojoShader.MOJOSHADER_effectTechnique* techPtr = (MojoShader.MOJOSHADER_effectTechnique*) effectPtr->techniques;
			List<EffectTechnique> techniques = new List<EffectTechnique>(effectPtr->technique_count);
			for (int i = 0; i < techniques.Capacity; i += 1, techPtr += 1)
			{
				// Set up Passes
				MojoShader.MOJOSHADER_effectPass* passPtr = (MojoShader.MOJOSHADER_effectPass*) techPtr->passes;
				List<EffectPass> passes = new List<EffectPass>((int) techPtr->pass_count);
				for (int j = 0; j < passes.Capacity; j += 1)
				{
					MojoShader.MOJOSHADER_effectPass pass = passPtr[j];
					passes.Add(new EffectPass(
						Marshal.PtrToStringAnsi(pass.name),
						INTERNAL_readAnnotations(
							pass.annotations,
							pass.annotation_count
						),
						this,
						(IntPtr) techPtr,
						(uint) j
					));
				}

				techniques.Add(new EffectTechnique(
					Marshal.PtrToStringAnsi(techPtr->name),
					(IntPtr) techPtr,
					new EffectPassCollection(passes),
					INTERNAL_readAnnotations(
						techPtr->annotations,
						techPtr->annotation_count
					)
				));
			}
			Techniques = new EffectTechniqueCollection(techniques);
		}

		private unsafe EffectAnnotationCollection INTERNAL_readAnnotations(
			IntPtr rawAnnotations,
			uint numAnnotations
		) {
			MojoShader.MOJOSHADER_effectAnnotation* annoPtr = (MojoShader.MOJOSHADER_effectAnnotation*) rawAnnotations;
			List<EffectAnnotation> annotations = new List<EffectAnnotation>((int) numAnnotations);
			for (int i = 0; i < numAnnotations; i += 1)
			{
				MojoShader.MOJOSHADER_effectAnnotation anno = annoPtr[i];
				annotations.Add(new EffectAnnotation(
					Marshal.PtrToStringAnsi(anno.name),
					Marshal.PtrToStringAnsi(anno.semantic),
					(int) anno.type.rows,
					(int) anno.type.columns,
					XNAClass[(int) anno.type.parameter_class],
					XNAType[(int) anno.type.parameter_type],
					anno.values
				));
			}
			return new EffectAnnotationCollection(annotations);
		}

		#endregion
	}
}
