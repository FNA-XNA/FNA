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
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal class ProfileCapabilities
	{
		#region Internal Variables

		internal GraphicsProfile Profile;

		internal uint VertexShaderVersion;

		internal uint PixelShaderVersion;

		internal bool OcclusionQuery;

		internal bool GetBackBufferData;

		internal bool SeparateAlphaBlend;

		internal bool DestBlendSrcAlphaSat;

		internal bool MinMaxSrcDestBlend;

		internal int MaxPrimitiveCount;

		internal bool IndexElementSize32;

		internal int MaxVertexStreams;

		internal int MaxStreamStride;

		internal int MaxVertexBufferSize;

		internal int MaxIndexBufferSize;

		internal int MaxTextureSize;

		internal int MaxCubeSize;

		internal int MaxVolumeExtent;

		internal int MaxTextureAspectRatio;

		internal int MaxSamplers;

		internal int MaxVertexSamplers;

		internal int MaxRenderTargets;

		internal bool NonPow2Unconditional;

		internal bool NonPow2Cube;

		internal bool NonPow2Volume;

		internal List<SurfaceFormat> ValidTextureFormats;

		internal List<SurfaceFormat> ValidCubeFormats;

		internal List<SurfaceFormat> ValidVolumeFormats;

		internal List<SurfaceFormat> ValidVertexTextureFormats;

		internal List<SurfaceFormat> InvalidFilterFormats;

		internal List<SurfaceFormat> InvalidBlendFormats;

		internal List<DepthFormat> ValidDepthFormats;

		internal List<VertexElementFormat> ValidVertexFormats;

		#endregion

		#region Internal Static Variables

		internal static ProfileCapabilities Reach;
		internal static ProfileCapabilities HiDef;

		#endregion

		#region Static Constructor

		static ProfileCapabilities()
		{
			/* This data mostly came from Shawn Hargreaves...
			 * https://www.shawnhargreaves.com/blog/reach-vs-hidef.html
			 * ... but the rest came from just getting the variables
			 * from XNA and printing their contents. As far as I
			 * know, these are 100% static. Tested on a box with a
			 * GTX 770 and a VMware Fusion instance.
			 * -flibit
			 */

			Reach = new ProfileCapabilities();
			Reach.Profile = GraphicsProfile.Reach;
			Reach.VertexShaderVersion =	0x200;
			Reach.PixelShaderVersion =	0x200;
			Reach.OcclusionQuery =		false;
			Reach.GetBackBufferData =	false;
			Reach.SeparateAlphaBlend =	false;
			Reach.DestBlendSrcAlphaSat =	false;
			Reach.MinMaxSrcDestBlend =	false;
			Reach.MaxPrimitiveCount =	65535;
			Reach.IndexElementSize32 =	false;
			Reach.MaxVertexStreams =	16;
			Reach.MaxStreamStride =		255;
			Reach.MaxVertexBufferSize =	0x3FFFFFF;
			Reach.MaxIndexBufferSize =	0x3FFFFFF;
			Reach.MaxTextureSize =		2048;
			Reach.MaxCubeSize =		512;
			Reach.MaxVolumeExtent =		0;
			Reach.MaxTextureAspectRatio =	2048;
			Reach.MaxSamplers =		16;
			Reach.MaxVertexSamplers =	0;
			Reach.MaxRenderTargets =	1;
			Reach.NonPow2Unconditional =	false;
			Reach.NonPow2Cube =		false;
			Reach.NonPow2Volume =		false;
			Reach.ValidTextureFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Color,
				SurfaceFormat.Bgr565,
				SurfaceFormat.Bgra5551,
				SurfaceFormat.Bgra4444,
				SurfaceFormat.Dxt1,
				SurfaceFormat.Dxt3,
				SurfaceFormat.Dxt5,
				SurfaceFormat.NormalizedByte2,
				SurfaceFormat.NormalizedByte4
			};
			Reach.ValidCubeFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Color,
				SurfaceFormat.Bgr565,
				SurfaceFormat.Bgra5551,
				SurfaceFormat.Bgra4444,
				SurfaceFormat.Dxt1,
				SurfaceFormat.Dxt3,
				SurfaceFormat.Dxt5
			};
			Reach.ValidVolumeFormats = new List<SurfaceFormat>();
			Reach.ValidVertexTextureFormats = new List<SurfaceFormat>();
			Reach.InvalidFilterFormats = new List<SurfaceFormat>();
			Reach.InvalidBlendFormats = new List<SurfaceFormat>();
			Reach.ValidDepthFormats = new List<DepthFormat>()
			{
				DepthFormat.Depth16,
				DepthFormat.Depth24,
				DepthFormat.Depth24Stencil8
			};
			Reach.ValidVertexFormats = new List<VertexElementFormat>()
			{
				VertexElementFormat.Color,
				VertexElementFormat.Single,
				VertexElementFormat.Vector2,
				VertexElementFormat.Vector3,
				VertexElementFormat.Vector4,
				VertexElementFormat.Byte4,
				VertexElementFormat.Short2,
				VertexElementFormat.Short4,
				VertexElementFormat.NormalizedShort2,
				VertexElementFormat.NormalizedShort4
			};

			HiDef = new ProfileCapabilities();
			HiDef.Profile = GraphicsProfile.HiDef;
			HiDef.VertexShaderVersion =	0x300;
			HiDef.PixelShaderVersion =	0x300;
			HiDef.OcclusionQuery =		true;
			HiDef.GetBackBufferData =	true;
			HiDef.SeparateAlphaBlend =	true;
			HiDef.DestBlendSrcAlphaSat =	true;
			HiDef.MinMaxSrcDestBlend =	true;
			HiDef.MaxPrimitiveCount =	1048575;
			HiDef.IndexElementSize32 =	true;
			HiDef.MaxVertexStreams =	16;
			HiDef.MaxStreamStride =		255;
			HiDef.MaxVertexBufferSize =	0x3FFFFFF;
			HiDef.MaxIndexBufferSize =	0x3FFFFFF;
			HiDef.MaxTextureSize =		8192; /* DX10 min spec */
			HiDef.MaxCubeSize =		8192; /* DX10 min spec */
			HiDef.MaxVolumeExtent =		2048; /* DX10 min spec */
			HiDef.MaxTextureAspectRatio =	2048;
			HiDef.MaxSamplers =		16;
			HiDef.MaxVertexSamplers =	4;
			HiDef.MaxRenderTargets =	4;
			HiDef.NonPow2Unconditional =	true;
			HiDef.NonPow2Cube =		true;
			HiDef.NonPow2Volume =		true;
			HiDef.ValidTextureFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Color,
				SurfaceFormat.Bgr565,
				SurfaceFormat.Bgra5551,
				SurfaceFormat.Bgra4444,
				SurfaceFormat.Dxt1,
				SurfaceFormat.Dxt3,
				SurfaceFormat.Dxt5,
				SurfaceFormat.NormalizedByte2,
				SurfaceFormat.NormalizedByte4,
				SurfaceFormat.Rgba1010102,
				SurfaceFormat.Rg32,
				SurfaceFormat.Rgba64,
				SurfaceFormat.Alpha8,
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.ValidCubeFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Color,
				SurfaceFormat.Bgr565,
				SurfaceFormat.Bgra5551,
				SurfaceFormat.Bgra4444,
				SurfaceFormat.Dxt1,
				SurfaceFormat.Dxt3,
				SurfaceFormat.Dxt5,
				SurfaceFormat.Rgba1010102,
				SurfaceFormat.Rg32,
				SurfaceFormat.Rgba64,
				SurfaceFormat.Alpha8,
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.ValidVolumeFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Color,
				SurfaceFormat.Bgr565,
				SurfaceFormat.Bgra5551,
				SurfaceFormat.Bgra4444,
				SurfaceFormat.Rgba1010102,
				SurfaceFormat.Rg32,
				SurfaceFormat.Rgba64,
				SurfaceFormat.Alpha8,
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.ValidVertexTextureFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.InvalidFilterFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.InvalidBlendFormats = new List<SurfaceFormat>()
			{
				SurfaceFormat.Single,
				SurfaceFormat.Vector2,
				SurfaceFormat.Vector4,
				SurfaceFormat.HalfSingle,
				SurfaceFormat.HalfVector2,
				SurfaceFormat.HalfVector4,
				SurfaceFormat.HdrBlendable
			};
			HiDef.ValidDepthFormats = new List<DepthFormat>()
			{
				DepthFormat.Depth16,
				DepthFormat.Depth24,
				DepthFormat.Depth24Stencil8
			};
			HiDef.ValidVertexFormats = new List<VertexElementFormat>()
			{
				VertexElementFormat.Color,
				VertexElementFormat.Single,
				VertexElementFormat.Vector2,
				VertexElementFormat.Vector3,
				VertexElementFormat.Vector4,
				VertexElementFormat.Byte4,
				VertexElementFormat.Short2,
				VertexElementFormat.Short4,
				VertexElementFormat.NormalizedShort2,
				VertexElementFormat.NormalizedShort4,
				VertexElementFormat.HalfVector2,
				VertexElementFormat.HalfVector4
			};
		}

		#endregion

		#region Internal Methods

		internal void ThrowNotSupportedException(string message)
		{
			throw new NotSupportedException(message);
		}

		internal void ThrowNotSupportedException(string message, object obj)
		{
			throw new NotSupportedException(
				message +
				" " + obj.ToString() // FIXME: WTF?
			);
		}

		internal void ThrowNotSupportedException(string message, object obj1, object obj2)
		{
			throw new NotSupportedException(
				message +
				" " + obj1.ToString() + // FIXME: WTF?
				" " + obj2.ToString() // FIXME: WTF?
			);
		}
		
		#endregion

		#region Internal Static Methods

		internal static ProfileCapabilities GetInstance(GraphicsProfile profile)
		{
			if (profile == GraphicsProfile.Reach)
			{
				return Reach;
			}
			if (profile == GraphicsProfile.HiDef)
			{
				return HiDef;
			}
			throw new ArgumentException("profile");
		}

		#endregion
	}
}
