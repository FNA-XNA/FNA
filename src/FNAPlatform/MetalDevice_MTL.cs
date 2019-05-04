#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
using SDL2;
#endregion

/* =============================
 * Objective-C Runtime Reference
 * =============================
 * A function call takes the form of objc_msgSend(<receiver>, <message>, <args>)
 * 
 * To send the "quack" message to a "duck" instance, we need two things:
 * 1) A pointer to the duck instance. This is the receiver.
 * 2) A Selector (read: char* with extra metadata) for the "quack" message.
 * 
 * To create a "quack" selector, we have to call sel_registerName() with a
 * parameter of "quack" turned into an array of UTF8 bytes. This is tedious,
 * so we do this here by calling the convenience method Selector("quack").
 * 
 * To get a property of obj: objc_msgSend(obj, Selector("propertyName"))
 * To set a property of obj: objc_msgSend(obj, Selector("setPropertyName"), val)
 * 
 * ===========
 * Other notes
 * ===========
 * The size of NSUInteger varies depending on CPU arch (32 bit / 64 bit).
 * Since all modern Apple devices are 64 bit, we can just use ulong (UInt64)
 * instead of uint (UInt32). This makes the structs the correct size.
 */

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class MetalDevice : IGLDevice
	{
		#region Private ObjC Runtime Entry Points

		const string objcLibrary = "/usr/lib/libobjc.A.dylib";

		// Void

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLPixelFormat arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, double arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLViewport viewport);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLScissorRect scissorRect);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLClearColor color);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, float arg1, float arg2, float arg3, float arg4);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLRegion region, ulong level, IntPtr bytes, ulong bytesPerRow);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr srcTexture, ulong srcSlice, ulong srcLevel, MTLOrigin srcOrigin, MTLSize srcSize, IntPtr dstTexture, ulong dstSlice, ulong dstLevel, MTLOrigin dstOrigin);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong primitiveType, ulong indexCount, ulong indexType, IntPtr indexBuffer, ulong indexBufferOffset, ulong instanceCount, int baseVertex, ulong baseInstance);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong primitiveType, ulong indexCount, ulong indexType, IntPtr indexBuffer, ulong indexBufferOffset);

		// IntPtr

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg1, IntPtr arg2);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, MTLPixelFormat arg1, ulong arg2, ulong arg3, bool arg4);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

		// ulong

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern ulong ulong_objc_msgSend(IntPtr receiver, IntPtr selector);

		// Bool

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		// Other

		[DllImport(objcLibrary)]
		private static extern IntPtr objc_getClass(string name);

		[DllImport(objcLibrary)]
		private static extern IntPtr sel_registerName(byte[] name);

		#endregion

		#region Private Metal Entry Points

		const string metalLibrary = "/System/Library/Frameworks/Metal.framework/Metal";

		[DllImport(metalLibrary)]
		private static extern IntPtr MTLCreateSystemDefaultDevice();

		#endregion

		#region Private MojoShader Interop

		// FIXME: Move all this to MojoShader.cs

		const string mojoshader = "mojoshader";

		[DllImport(mojoshader)]
		private static extern IntPtr MOJOSHADER_mtlCompileEffect(
			IntPtr effect, // MOJOSHADER_effect*
			IntPtr mtlDevice // MTLDevice*
		);

		[DllImport(mojoshader)]
		private static extern string MOJOSHADER_mtlGetError();

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlDeleteEffect(IntPtr mtlEffect);

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlEffectBegin(
			IntPtr mtlEffect,
			out uint numPasses,
			int saveShaderState,
			IntPtr stateChanges,
			IntPtr curVert,
			IntPtr curFrag
		);

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlEffectBeginPass(
			IntPtr mtlEffect,
			uint pass,
			IntPtr curVert,
			IntPtr curFrag,
			out IntPtr newVert_out,
			out IntPtr newFrag_out
		);

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlEffectCommitChanges(
			IntPtr mtlEffect,
			out IntPtr newVert_out,
			out IntPtr newFrag_out
		);

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlEffectEndPass(IntPtr mtlEffect);

		[DllImport(mojoshader)]
		private static extern void MOJOSHADER_mtlEffectEnd(
			IntPtr mtlEffect,
			out IntPtr newVert_out,
			out IntPtr newFrag_out
		);

		#endregion

		#region Private MTL Enums

		private enum MTLLoadAction : ulong
		{
			DontCare	= 0,
			Load		= 1,
			Clear		= 2
		}

		private enum MTLPrimitiveType : ulong
		{
			Point			= 0,
			Line			= 1,
			LineStrip		= 2,
			Triangle		= 3,
			TriangleStrip	= 4
		}

		private enum MTLIndexType : ulong
		{
			ulong16 = 0,
			ulong32 = 1
		}

		private enum MTLPixelFormat : ulong
		{
			Invalid				= 0,
			A8Unorm      		   	= 1,
			R16Float     			= 25,
			RG8Snorm			= 32,
			B5G6R5Unorm 			= 40,
			ABGR4Unorm			= 42,
			BGR5A1Unorm			= 43,
			R32Float			= 55,
			RG16Unorm			= 60,
			RG16Snorm			= 62,
			RG16Float			= 65,
			RGBA8Unorm			= 70,
			RGBA8Snorm			= 72,
			BGRA8Unorm			= 80,
			RGB10A2Unorm			= 90,
			RG32Float			= 105,
			RGBA16Unorm			= 110,
			RGBA16Float			= 115,
			RGBA32Float			= 125,
			BC1_RGBA			= 130,
			BC2_RGBA			= 132,
			BC3_RGBA			= 134,
			Depth32Float			= 252,
			Stencil8			= 253,
			Depth24Unorm_Stencil8		= 255,
			Depth32Float_Stencil8		= 260,
		}

		private enum MTLSamplerMinMagFilter
		{
			Nearest = 0,
			Linear = 1
		}

		private enum MTLTextureUsage
		{
			Unknown = 0,
			ShaderRead = 1,
			ShaderWrite = 2,
			RenderTarget = 4
		}

		private enum MTLTextureType
		{
			Texture2D = 2,
			Multisample2D = 4
		}

		private enum MTLResourceStorageMode
		{
			Shared = 0,
			Managed = 1, /* macOS only */
			Private = 2
		}

		#endregion

		#region Private MTL Structs

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLClearColor
		{
			public double red;
			public double green;
			public double blue;
			public double alpha;

			public MTLClearColor(
				double red,
				double green,
				double blue,
				double alpha
			) {
				this.red = red;
				this.green = green;
				this.blue = blue;
				this.alpha = alpha;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLViewport
		{
			public double originX;
			public double originY;
			public double width;
			public double height;
			public double znear;
			public double zfar;

			public MTLViewport(
				double originX,
				double originY,
				double width,
				double height,
				double znear,
				double zfar
			) {
				this.originX = originX;
				this.originY = originY;
				this.width = width;
				this.height = height;
				this.znear = znear;
				this.zfar = zfar;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLScissorRect
		{
			ulong x;
			ulong y;
			ulong width;
			ulong height;

			public MTLScissorRect(
				ulong x,
				ulong y,
				ulong width,
				ulong height
			) {
				this.x = x;
				this.y = y;
				this.width = width;
				this.height = height;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLOrigin
		{
			ulong x;
			ulong y;
			ulong z;

			public MTLOrigin(ulong x, ulong y, ulong z)
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLSize
		{
			ulong width;
			ulong height;
			ulong depth;

			public MTLSize(ulong width, ulong height, ulong depth)
			{
				this.width = width;
				this.height = height;
				this.depth = depth;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLRegion
		{
			MTLOrigin origin;
			MTLSize size;

			public MTLRegion(MTLOrigin origin, MTLSize size)
			{
				this.origin = origin;
				this.size = size;
			}
		}

		#endregion

		#region Selectors

		private static IntPtr Selector(string name)
		{
			return sel_registerName(System.Text.Encoding.UTF8.GetBytes(name));
		}

		// FIXME: Clean up this huge mess

		private static IntPtr selCommandBuffer = Selector("commandBuffer");
		private static IntPtr selCommandQueue = Selector("newCommandQueue");
		private static IntPtr selCommit = Selector("commit");
		private static IntPtr selContents = Selector("contents");
		private static IntPtr selName = Selector("name");
		private static IntPtr selPresentDrawable = Selector("presentDrawable:");
		private static IntPtr selNewBufferWithLength = Selector("newBufferWithLength:options:");
		private static IntPtr selRenderCommandEncoder = Selector("renderCommandEncoderWithDescriptor:");
		private static IntPtr selSupportsSampleCount = Selector("supportsSampleCount:");
		private static IntPtr selNewTextureWithDescriptor = Selector("newTextureWithDescriptor:");

		private static IntPtr selClearColor = Selector("clearColor");
		private static IntPtr selSetClearColor = Selector("setClearColor:");
		private static IntPtr selLoadAction = Selector("loadAction");
		private static IntPtr selSetLoadAction = Selector("setLoadAction:");
		private static IntPtr selTexture = Selector("texture");
		private static IntPtr selSetTexture = Selector("setTexture:");
		private static IntPtr selSetPixelFormat = Selector("setPixelFormat:");

		private static IntPtr selColorAttachments = Selector("colorAttachments");
		private static IntPtr selObjectAtIndexedSubscript = Selector("objectAtIndexedSubscript:");
		private static IntPtr selRenderPassDescriptor = Selector("renderPassDescriptor");
		private static IntPtr selSetViewport = Selector("setViewport:");
		private static IntPtr selSetScissorRect = Selector("setScissorRect:");
		private static IntPtr selEndEncoding = Selector("endEncoding");
		private static IntPtr selNextDrawable = Selector("nextDrawable");
		private static IntPtr selTexture2DDescriptor = Selector("texture2DDescriptorWithPixelFormat:width:height:mipmapped:");
		private static IntPtr selReplaceRegion = Selector("replaceRegion:mipmapLevel:withBytes:bytesPerRow:");
		private static IntPtr selBlitCommandEncoder = Selector("blitCommandEncoder");
		private static IntPtr selCopyFromTexture = Selector("copyFromTexture:sourceSlice:sourceLevel:sourceOrigin:sourceSize:toTexture:destinationSlice:destinationLevel:destinationOrigin:");
		private static IntPtr selSetBlendColor = Selector("setBlendColorRed:green:blue:alpha:");
		private static IntPtr selDrawIndexedPrimitives = Selector("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:instanceCount:baseVertex:baseInstance:");

		private static IntPtr selSetVertexFunction = Selector("setVertexFunction:");
		private static IntPtr selSetFragmentFunction = Selector("setFragmentFunction:");
		private static IntPtr selSetVertexDescriptor = Selector("setVertexDescriptor:");
		private static IntPtr selNewRenderPipelineState = Selector("newRenderPipelineStateWithDescriptor:error:");

		private static IntPtr selSetStencilReference = Selector("setStencilReferenceValue:");

		private static IntPtr selLocalizedDescription = Selector("localizedDescription");

		private static IntPtr selSetUsage = Selector("setUsage:");
		private static IntPtr selSetTextureType = Selector("setTextureType:");
		private static IntPtr selSetSampleCount = Selector("setSampleCount:");
		private static IntPtr selSetWidth = Selector("setWidth:");
		private static IntPtr selSetHeight = Selector("setHeight:");

		private static IntPtr selDepthAttachment = Selector("depthAttachment");
		private static IntPtr selSetClearDepth = Selector("setClearDepth:");
		private static IntPtr selStencilAttachment = Selector("stencilAttachment");
		private static IntPtr selSetClearStencil = Selector("setClearStencil:");

		private static IntPtr selSetFramebufferOnly = Selector("setFramebufferOnly:");
		private static IntPtr selSetStorageMode = Selector("setStorageMode:");

		private static IntPtr selPixelFormat = Selector("pixelFormat");

		private static IntPtr selNew = Selector("new");
		private static IntPtr selRelease = Selector("release");
		private static IntPtr selRetain = Selector("retain");
		private static IntPtr selDrain = Selector("drain");

		#endregion

		#region ObjC Class References

		private static IntPtr classTextureDescriptor = objc_getClass("MTLTextureDescriptor");
		private static IntPtr classRenderPassDescriptor = objc_getClass("MTLRenderPassDescriptor");
		private static IntPtr classRenderPipelineDescriptor = objc_getClass("MTLRenderPipelineDescriptor");
		private static IntPtr classNSAutoreleasePool = objc_getClass("NSAutoreleasePool");

		#endregion

		#region NSString -> C# String

		private static IntPtr selUtf8 = Selector("UTF8String");
		private static string NSStringToUTF8(IntPtr nsstr)
		{
			return Marshal.PtrToStringAnsi(
				intptr_objc_msgSend(nsstr, selUtf8)
			);
		}

		#endregion

		#region Objective-C Memory Management Utilities

		private static void ObjCRelease(IntPtr obj)
		{
			objc_msgSend(obj, selRelease);
		}

		private static void ObjCRetain(IntPtr obj)
		{
			objc_msgSend(obj, selRetain);
		}

		private static IntPtr StartAutoreleasePool()
		{
			return intptr_objc_msgSend(classNSAutoreleasePool, selNew);
		}

		private static void DrainAutoreleasePool(IntPtr pool)
		{
			objc_msgSend(pool, selDrain);
		}

		#endregion

		#region MTLDevice

		private static string mtlGetDeviceName(IntPtr device)
		{
			return NSStringToUTF8(intptr_objc_msgSend(device, selName));
		}

		private static bool mtlSupportsSampleCount(IntPtr device, ulong count)
		{
			return bool_objc_msgSend(device, selSupportsSampleCount, count);
		}

		private static IntPtr mtlMakeCommandQueue(IntPtr device)
		{
			return intptr_objc_msgSend(device, selCommandQueue);
		}

		private static IntPtr mtlNewBufferWithLength(IntPtr device, ulong length)
		{
			return intptr_objc_msgSend(
				device,
				selNewBufferWithLength,
				length,
				IntPtr.Zero // FIXME: Do we need this?
			);
		}

		private static IntPtr mtlNewTextureWithDescriptor(IntPtr device, IntPtr texDesc)
		{
			return intptr_objc_msgSend(
				device,
				selNewTextureWithDescriptor,
				texDesc
			);
		}

		#endregion

		#region MTLBuffer

		private static IntPtr mtlGetBufferContentsPtr(IntPtr buffer)
		{
			return intptr_objc_msgSend(buffer, selContents);
		}

		#endregion

		#region MTLCommandBuffer

		private static IntPtr mtlMakeRenderCommandEncoder(
			IntPtr commandBuffer,
			IntPtr renderPassDesc
		) {
			return intptr_objc_msgSend(
				commandBuffer,
				selRenderCommandEncoder,
				renderPassDesc
			);
		}

		private static void mtlPresentDrawable(
			IntPtr commandBuffer,
			IntPtr drawable
		) {
			objc_msgSend(
				commandBuffer,
				selPresentDrawable,
				drawable
			);
		}

		private static void mtlCommitCommandBuffer(IntPtr commandBuffer)
		{
			objc_msgSend(commandBuffer, selCommit);
		}

		#endregion

		#region MTLCommandQueue

		private static IntPtr mtlMakeCommandBuffer(IntPtr queue)
		{
			return intptr_objc_msgSend(queue, selCommandBuffer);
		}

		#endregion

		#region Generic Color Attachment Methods (RenderPass or RenderPipelineState)

		private static IntPtr mtlGetColorAttachment(
			IntPtr desc,
			ulong index
		) {
			IntPtr attachments = intptr_objc_msgSend(
				desc,
				selColorAttachments
			);

			return intptr_objc_msgSend(
				attachments, 
				selObjectAtIndexedSubscript,
				index
			);
		}

		private static void mtlSetColorAttachmentClearColor(
			IntPtr colorAttachment,
			float r,
			float g,
			float b,
			float a
		) {
			MTLClearColor clearColor = new MTLClearColor(r, g, b, a);
			objc_msgSend(colorAttachment, selSetClearColor, clearColor);
		}

		private static void mtlSetAttachmentLoadAction(
			IntPtr colorAttachment,
			MTLLoadAction loadAction
		) {
			objc_msgSend(colorAttachment, selSetLoadAction, (ulong) loadAction);
		}

		private static void mtlSetAttachmentTexture(
			IntPtr colorAttachment,
			IntPtr texture
		) {
			objc_msgSend(colorAttachment, selSetTexture, texture);
		}

		private static void mtlSetColorAttachmentPixelFormat(
			IntPtr colorAttachment,
			MTLPixelFormat pixelFormat
		) {
			objc_msgSend(colorAttachment, selSetPixelFormat, pixelFormat);
		}

		#endregion

		#region Depth/Stencil Attachments

		private static void mtlSetDepthAttachmentClearDepth(
			IntPtr depthAttachment,
			float clearDepth
		) {
			objc_msgSend(depthAttachment, selSetClearDepth, clearDepth);
		}

		private static void mtlSetStencilAttachmentClearStencil(
			IntPtr stencilAttachment,
			int clearStencil
		) {
			objc_msgSend(stencilAttachment, selSetClearStencil, (ulong) clearStencil);
		}

		#endregion

		#region MTLRenderPassDescriptor

		private static IntPtr mtlMakeRenderPassDescriptor()
		{
			return intptr_objc_msgSend(classRenderPassDescriptor, selRenderPassDescriptor);
		}

		private static IntPtr mtlGetDepthAttachment(IntPtr pass)
		{
			return intptr_objc_msgSend(pass, selDepthAttachment);
		}

		private static IntPtr mtlGetStencilAttachment(IntPtr pass)
		{
			return intptr_objc_msgSend(pass, selStencilAttachment);
		}

		#endregion

		#region MTLRenderCommandEncoder

		private static void mtlSetBlendColor(
			IntPtr renderCommandEncoder,
			float red,
			float green,
			float blue,
			float alpha
		) {
			objc_msgSend(renderCommandEncoder, selSetBlendColor, red, green, blue, alpha);
		}

		private static void mtlSetStencilReferenceValue(
			IntPtr renderCommandEncoder,
			ulong referenceValue
		) {
			objc_msgSend(renderCommandEncoder, selSetStencilReference, referenceValue);
		}

		private static void mtlSetViewport(
			IntPtr renderCommandEncoder,
			int x,
			int y,
			int w,
			int h,
			double minDepth,
			double maxDepth
		) {
			MTLViewport viewport = new MTLViewport(x, y, w, h, minDepth, maxDepth);
			objc_msgSend(renderCommandEncoder, selSetViewport, viewport);
		}

		private static void mtlSetScissorRect(
			IntPtr renderCommandEncoder,
			ulong x,
			ulong y,
			ulong w,
			ulong h
		) {
			MTLScissorRect rect = new MTLScissorRect(x, y, w, h);
			objc_msgSend(renderCommandEncoder, selSetScissorRect, rect);
		}

		private static void mtlEndEncoding(IntPtr commandEncoder)
		{
			objc_msgSend(commandEncoder, selEndEncoding);
		}

		private static void mtlDrawIndexedPrimitives(
			IntPtr renderCommandEncoder,
			MTLPrimitiveType primitiveType,
			ulong indexCount,
			MTLIndexType indexType,
			IntPtr indexBuffer,
			ulong indexBufferOffset,
			ulong instanceCount,
			int baseVertex,
			ulong baseInstance
		) {
			Console.WriteLine("Encoder: " + renderCommandEncoder);
			Console.WriteLine("Primitive Type: " + primitiveType);
			Console.WriteLine("indexCount: " + indexCount);
			Console.WriteLine("indexType: " + indexType);
			Console.WriteLine("index buffer: " + indexBuffer);
			Console.WriteLine("index buffer offset: " + indexBufferOffset);
			Console.WriteLine("instanceCount: " + instanceCount);
			Console.WriteLine("base vertex: " + baseVertex);
			Console.WriteLine("baseInstance: " + baseInstance);
			objc_msgSend(
				renderCommandEncoder,
				selDrawIndexedPrimitives,
				(ulong) primitiveType,
				indexCount,
				(ulong) indexType,
				indexBuffer,
				indexBufferOffset,
				instanceCount,
				baseVertex,
				baseInstance
			);
		}

		#endregion

		#region CAMetalLayer

		private static IntPtr mtlNextDrawable(IntPtr layer)
		{
			return intptr_objc_msgSend(layer, selNextDrawable);
		}

		private static MTLPixelFormat mtlGetLayerPixelFormat(IntPtr layer)
		{
			return (MTLPixelFormat) ulong_objc_msgSend(layer, selPixelFormat);
		}

		#endregion

		#region CAMetalDrawable

		private static IntPtr mtlGetTextureFromDrawable(IntPtr drawable)
		{
			return intptr_objc_msgSend(drawable, selTexture);
		}

		private static void mtlSetLayerFramebufferOnly(
			IntPtr layer,
			bool framebufferOnly
		) {
			objc_msgSend(layer, selSetFramebufferOnly, framebufferOnly ? 1 : 0);
		}

		#endregion

		#region MTLTextureDescriptor

		private static IntPtr mtlMakeTexture2DDescriptor(
			MTLPixelFormat pixelFormat,
			ulong width,
			ulong height,
			bool mipmapped
		) {
			return intptr_objc_msgSend(
				classTextureDescriptor,
				selTexture2DDescriptor,
				pixelFormat,
				width,
				height,
				mipmapped
			);
		}

		private static void mtlSetTextureUsage(
			IntPtr texDesc,
			MTLTextureUsage usage
		) {
			objc_msgSend(texDesc, selSetUsage, (ulong) usage);
		}

		private static void mtlSetTextureType(
			IntPtr texDesc,
			MTLTextureType type
		) {
			objc_msgSend(texDesc, selSetTextureType, (ulong) type);
		}

		private static void mtlSetTextureSampleCount(
			IntPtr texDesc,
			int sampleCount
		) {
			objc_msgSend(texDesc, selSetSampleCount, (ulong) sampleCount);
		}

		private static void mtlSetTexturePixelFormat(
			IntPtr texDesc,
			MTLPixelFormat format
		) {
			objc_msgSend(texDesc, selSetPixelFormat, format);
		}

		private static void mtlSetTextureWidth(
			IntPtr texDesc,
			int width
		) {
			objc_msgSend(texDesc, selSetWidth, (ulong) width);
		}

		private static void mtlSetTextureHeight(
			IntPtr texDesc,
			int height
		) {
			objc_msgSend(texDesc, selSetHeight, (ulong) height);
		}

		#endregion

		#region MTLTexture

		private static void mtlReplaceRegion(
			IntPtr texture,
			MTLRegion region,
			ulong level,
			IntPtr pixelBytes,
			ulong bytesPerRow
		) {
			objc_msgSend(
				texture,
				selReplaceRegion,
				region,
				level,
				pixelBytes,
				bytesPerRow
			);
		}

		#region MTLShader

		private static string mtlGetShaderName(IntPtr mtlShader)
		{
			return NSStringToUTF8(intptr_objc_msgSend(mtlShader, selName));
		}

		#endregion

		#region MTLBlitCommandEncoder

		private static IntPtr mtlMakeBlitCommandEncoder(IntPtr commandBuffer)
		{
			return intptr_objc_msgSend(commandBuffer, selBlitCommandEncoder);
		}

		private static void mtlBlitTextureToTexture(
			IntPtr blitCommandEncoder,
			IntPtr srcTexture,
			ulong srcSlice,
			ulong srcLevel,
			MTLOrigin srcOrigin,
			MTLSize srcSize,
			IntPtr dstTexture,
			ulong dstSlice,
			ulong dstLevel,
			MTLOrigin dstOrigin
		) {
			objc_msgSend(
				blitCommandEncoder,
				selCopyFromTexture,
				srcTexture,
				srcSlice,
				srcLevel,
				srcOrigin,
				srcSize,
				dstTexture,
				dstSlice,
				dstLevel,
				dstOrigin
			);
		}

		#endregion

		#region MTLRenderPipelineState

		private static IntPtr mtlMakeRenderPipelineDescriptor()
		{
			return intptr_objc_msgSend(classRenderPipelineDescriptor, selNew);
		}

		private static void mtlSetPipelineVertexFunction(
			IntPtr pipelineDescriptor,
			IntPtr vertexFunction
		) {
			objc_msgSend(pipelineDescriptor, selSetVertexFunction, vertexFunction);
		}

		private static void mtlSetPipelineFragmentFunction(
			IntPtr pipelineDescriptor,
			IntPtr fragmentFunction
		) {
			objc_msgSend(pipelineDescriptor, selSetFragmentFunction, fragmentFunction);
		}

		private static void mtlSetPipelineVertexDescriptor(
			IntPtr pipelineDescriptor,
			IntPtr vertexDescriptor
		) {
			objc_msgSend(pipelineDescriptor, selSetVertexDescriptor, vertexDescriptor);
		}

		private static IntPtr mtlNewRenderPipelineStateWithDescriptor(
			IntPtr device,
			IntPtr pipelineDescriptor
		) {
			IntPtr error = IntPtr.Zero;
			IntPtr pipeline = intptr_objc_msgSend(device, selNewRenderPipelineState, pipelineDescriptor, error);
			if (error != IntPtr.Zero)
			{
				throw new Exception("Metal Error: " + GetNSErrorDescription(error));
			}
			return pipeline;
		}

		#endregion

		#region Storage Modes

		private static void mtlSetStorageMode(
			IntPtr resource,
			MTLResourceStorageMode mode
		) {
			objc_msgSend(resource, selSetStorageMode, (ulong) mode);
		}

		#endregion

		#region Error Handling

		private static IntPtr mtlGetErrorLocalizedDescription(IntPtr error)
		{
			return intptr_objc_msgSend(error, selLocalizedDescription);
		}

		private static string GetNSErrorDescription(IntPtr error)
		{
			return NSStringToUTF8(mtlGetErrorLocalizedDescription(error));
		}

		#endregion

		#endregion
	}
}