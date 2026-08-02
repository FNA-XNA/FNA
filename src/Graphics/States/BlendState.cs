#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class BlendState : GraphicsResource
	{
		#region Public Properties

		public BlendFunction AlphaBlendFunction
		{
			get
			{
				return state.alphaBlendFunction;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.alphaBlendFunction = value;
			}
		}

		public Blend AlphaDestinationBlend
		{
			get
			{
				return state.alphaDestinationBlend;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.alphaDestinationBlend = value;
			}
		}

		public Blend AlphaSourceBlend
		{
			get
			{
				return state.alphaSourceBlend;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.alphaSourceBlend = value;
			}
		}

		public BlendFunction ColorBlendFunction
		{
			get
			{
				return state.colorBlendFunction;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorBlendFunction = value;
			}
		}

		public Blend ColorDestinationBlend
		{
			get
			{
				return state.colorDestinationBlend;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorDestinationBlend = value;
			}
		}

		public Blend ColorSourceBlend
		{
			get
			{
				return state.colorSourceBlend;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorSourceBlend = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels
		{
			get
			{
				return state.colorWriteEnable;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorWriteEnable = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels1
		{
			get
			{
				return state.colorWriteEnable1;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorWriteEnable1 = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels2
		{
			get
			{
				return state.colorWriteEnable2;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorWriteEnable2 = value;
			}
		}

		public ColorWriteChannels ColorWriteChannels3
		{
			get
			{
				return state.colorWriteEnable3;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.colorWriteEnable3 = value;
			}
		}

		public Color BlendFactor
		{
			get
			{
				return state.blendFactor;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.blendFactor = value;
			}
		}

		public int MultiSampleMask
		{
			get
			{
				return state.multiSampleMask;
			}
			set
			{
				ThrowReadonly(typeof(BlendState));
				state.multiSampleMask = value;
			}
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

		#region Internal Variables

		internal bool IsReadonly;

		internal FNA3D.FNA3D_BlendState state;

		internal protected override bool IsHarmlessToLeakInstance
		{
			get
			{
				return true;
			}
		}

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
			IsReadonly = true;
		}

		#endregion

		#region Private Methods

		private void ThrowReadonly(Type type)
		{
			if (IsReadonly)
			{
				string name = type.Name;
				throw new InvalidOperationException("Cannot change read-only " + name + ". State objects become read-only the first time they are bound to a GraphicsDevice. To change property values, create a new " + name + " instance.");
			}
		}

		#endregion
	}
}
