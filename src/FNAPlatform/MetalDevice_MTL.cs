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
using System.Runtime.InteropServices;
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

		[DllImport(objcLibrary)]
		private static extern IntPtr objc_getClass(string name);

		[DllImport(objcLibrary)]
		private static extern void objc_release(IntPtr obj);

		[DllImport(objcLibrary)]
		private static extern IntPtr objc_retain(IntPtr obj);

		[DllImport(objcLibrary)]
		private static extern IntPtr objc_autoreleasePoolPush();

		[DllImport(objcLibrary)]
		private static extern void objc_autoreleasePoolPop(IntPtr pool);

		[DllImport(objcLibrary)]
		private static extern IntPtr sel_registerName(byte[] name);

		/* Here come the obj_msgSend overloads... */

		// Void

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, ulong arg2);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, ulong arg2, ulong arg3);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg1, ulong arg2, ulong arg3);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg1, ulong arg2);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, uint arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, bool arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, double arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLViewport viewport);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLScissorRect scissorRect);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLClearColor color);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, NSRange range);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, float arg1, float arg2, float arg3);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, float arg1, float arg2, float arg3, float arg4);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, MTLRegion region, ulong level, ulong slice, IntPtr bytes, ulong bytesPerRow, ulong bytesPerImage);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr srcTexture, ulong srcSlice, ulong srcLevel, MTLOrigin srcOrigin, MTLSize srcSize, IntPtr dstTexture, ulong dstSlice, ulong dstLevel, MTLOrigin dstOrigin);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, ulong primitiveType, ulong indexCount, ulong indexType, IntPtr indexBuffer, ulong indexBufferOffset, ulong instanceCount);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr pixelBytes, ulong bytesPerRow, ulong bytesPerImage, MTLRegion region, ulong level, ulong slice);

		// IntPtr

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, string arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg1, ulong arg2);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, MTLPixelFormat arg1, ulong arg2, bool arg3);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, MTLPixelFormat arg1, ulong arg2, ulong arg3, bool arg4);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, out IntPtr arg2);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern IntPtr intptr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, out IntPtr arg3);

		// Ulong

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern ulong ulong_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern ulong ulong_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		// Bool

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector, NSOperatingSystemVersion arg);

		// CGSize

		[DllImport(objcLibrary, EntryPoint = "objc_msgSend")]
		private static extern CGSize cgsize_objc_msgSend(IntPtr receiver, IntPtr selector);

		#endregion

		#region C-Style Metal Function Imports

		const string metalLibrary = "/System/Library/Frameworks/Metal.framework/Metal";

		[DllImport(metalLibrary)]
		internal static extern IntPtr MTLCreateSystemDefaultDevice();

		#endregion

		#region Private MTL Enums

		private enum MTLLoadAction : ulong
		{
			DontCare = 0,
			Load = 1,
			Clear = 2
		}

		private enum MTLStoreAction : ulong
		{
			DontCare = 0,
			Store = 1,
			MultisampleResolve = 2
		}

		private enum MTLPrimitiveType : ulong
		{
			Point = 0,
			Line = 1,
			LineStrip = 2,
			Triangle = 3,
			TriangleStrip = 4
		}

		private enum MTLIndexType : ulong
		{
			UInt16 = 0,
			UInt32 = 1
		}

		private enum MTLPixelFormat : ulong
		{
			Invalid			= 0,
			A8Unorm			= 1,
			R16Float     		= 25,
			RG8Snorm		= 32,
			B5G6R5Unorm 		= 40,
			ABGR4Unorm		= 42,
			BGR5A1Unorm		= 43,
			R32Float		= 55,
			RG16Unorm		= 60,
			RG16Snorm		= 62,
			RG16Float		= 65,
			RGBA8Unorm		= 70,
			BGRA8Unorm		= 80,
			RGB10A2Unorm		= 90,
			RG32Float		= 105,
			RGBA16Unorm		= 110,
			RGBA16Float		= 115,
			RGBA32Float		= 125,
			BC1_RGBA		= 130,
			BC2_RGBA		= 132,
			BC3_RGBA		= 134,
			Depth16Unorm		= 250,
			Depth32Float		= 252,
			Depth24Unorm_Stencil8	= 255,
			Depth32Float_Stencil8	= 260,
		}

		private enum MTLSamplerMinMagFilter
		{
			Nearest = 0,
			Linear = 1
		}

		private enum MTLTextureUsage
		{
			ShaderRead = 1,
			RenderTarget = 4
		}

		private enum MTLTextureType
		{
			Multisample2D = 4,
			Texture3D = 7
		}

		private enum MTLStorageMode
		{
			Private = 2
		}

		private enum MTLBlendFactor
		{
			Zero = 0,
			One = 1,
			SourceColor = 2,
			OneMinusSourceColor = 3,
			SourceAlpha = 4,
			OneMinusSourceAlpha = 5,
			DestinationColor = 6,
			OneMinusDestinationColor = 7,
			DestinationAlpha = 8,
			OneMinusDestinationAlpha = 9,
			SourceAlphaSaturated = 10,
			BlendColor = 11,
			OneMinusBlendColor = 12
		}

		private enum MTLBlendOperation
		{
			Add = 0,
			Subtract = 1,
			ReverseSubtract = 2,
			Min = 3,
			Max = 4
		}

		private enum MTLCullMode
		{
			None = 0,
			Front = 1,
			Back = 2
		}

		private enum MTLTriangleFillMode
		{
			Fill = 0,
			Lines = 1
		}

		private enum MTLSamplerAddressMode
		{
			ClampToEdge = 0,
			Repeat = 2,
			MirrorRepeat = 3
		}

		private enum MTLSamplerMipFilter
		{
			Nearest = 1,
			Linear = 2
		}

		private enum MTLVertexFormat
		{
			UChar4 = 3,
			UChar4Normalized = 9,
			Short2 = 16,
			Short4 = 18,
			Short2Normalized = 22,
			Short4Normalized = 24,
			Half2 = 25,
			Half4 = 27,
			Float = 28,
			Float2 = 29,
			Float3 = 30,
			Float4 = 31
		}

		private enum MTLVertexStepFunction
		{
			PerInstance = 2
		}

		private enum MTLCompareFunction
		{
			Never = 0,
			Less = 1,
			Equal = 2,
			LessEqual = 3,
			Greater = 4,
			NotEqual = 5,
			GreaterEqual = 6,
			Always = 7
		}

		private enum MTLStencilOperation
		{
			Keep = 0,
			Zero = 1,
			Replace = 2,
			IncrementClamp = 3,
			DecrementClamp = 4,
			Invert = 5,
			IncrementWrap = 6,
			DecrementWrap = 7
		}

		private enum MTLVisibilityResultMode
		{
			Disabled = 0,
			Counting = 2
		}

		private enum MTLResourceOptions
		{
			CPUCacheModeDefaultCache = 0,
			CPUCacheModeWriteCombined = 1
		}

		private enum MTLPurgeableState
		{
			NonVolatile = 2,
			Empty = 4
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
				int x,
				int y,
				int width,
				int height
			) {
				this.x = (ulong) x;
				this.y = (ulong) y;
				this.width = (ulong) width;
				this.height = (ulong) height;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLOrigin
		{
			ulong x;
			ulong y;
			ulong z;

			public MTLOrigin(int x, int y, int z)
			{
				this.x = (ulong) x;
				this.y = (ulong) y;
				this.z = (ulong) z;
			}

			public static MTLOrigin Zero = new MTLOrigin(0, 0, 0);
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MTLSize
		{
			ulong width;
			ulong height;
			ulong depth;

			public MTLSize(int width, int height, int depth)
			{
				this.width = (ulong) width;
				this.height = (ulong) height;
				this.depth = (ulong) depth;
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

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct CGSize
		{
			public double width;
			public double height;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct NSRange
		{
			public ulong loc;
			public ulong len;

			public NSRange(int loc, int len)
			{
				this.loc = (ulong) loc;
				this.len = (ulong) len;
			}
		}

		#endregion

		#region Selectors

		private static IntPtr Selector(string name)
		{
			name += Char.MinValue; // null terminator
			return sel_registerName(System.Text.Encoding.UTF8.GetBytes(name));
		}

		private static IntPtr selRespondsToSelector = Selector("respondsToSelector:");
		private static bool RespondsToSelector(IntPtr obj, IntPtr selector)
		{
			return bool_objc_msgSend(obj, selRespondsToSelector, selector);
		}

		#endregion

		#region ObjC Class References

		private static IntPtr classTextureDescriptor = objc_getClass("MTLTextureDescriptor");
		private static IntPtr classRenderPassDescriptor = objc_getClass("MTLRenderPassDescriptor");
		private static IntPtr classRenderPipelineDescriptor = objc_getClass("MTLRenderPipelineDescriptor");
		private static IntPtr classDepthStencilDescriptor = objc_getClass("MTLDepthStencilDescriptor");
		private static IntPtr classStencilDescriptor = objc_getClass("MTLStencilDescriptor");
		private static IntPtr classMTLSamplerDescriptor = objc_getClass("MTLSamplerDescriptor");
		private static IntPtr classMTLVertexDescriptor = objc_getClass("MTLVertexDescriptor");
		private static IntPtr classNSProcessInfo = objc_getClass("NSProcessInfo");
		private static IntPtr classNSString = objc_getClass("NSString");

		#endregion

		#region NSString <-> C# String

		private static IntPtr selUtf8 = Selector("UTF8String");
		private static string NSStringToUTF8(IntPtr nsstr)
		{
			return Marshal.PtrToStringAnsi(
				intptr_objc_msgSend(nsstr, selUtf8)
			);
		}

		private static IntPtr selAlloc = Selector("alloc");
		private static IntPtr selInitWithUtf8 = Selector("initWithUTF8String:");
		private static IntPtr UTF8ToNSString(string str)
		{
			return intptr_objc_msgSend(
				intptr_objc_msgSend(classNSString, selAlloc),
				selInitWithUtf8,
				str
			);
		}

		#endregion

		#region iOS/tvOS GPU Check

		private static IntPtr selSupportsFamily = Selector("supportsFamily:");
		private static IntPtr selSupportsFeatureSet = Selector("supportsFeatureSet:");
		private bool HasModernAppleGPU()
		{
			// "Modern" = A9 or later
			const ulong GPUFamilyCommon2 = 3002;
			const ulong iOS_GPUFamily3_v1 = 4;
			const ulong tvOS_GPUFamily2_v1 = 30003;

			// Can we use the GPUFamily API?
			if (RespondsToSelector(device, selSupportsFamily))
			{
				return bool_objc_msgSend(
					device,
					selSupportsFamily,
					GPUFamilyCommon2
				);
			}

			// Fall back to checking FeatureSets...
			bool iosCompat = bool_objc_msgSend(
				device,
				selSupportsFeatureSet,
				iOS_GPUFamily3_v1
			);
			bool tvosCompat = bool_objc_msgSend(
				device,
				selSupportsFeatureSet,
				tvOS_GPUFamily2_v1
			);
			return iosCompat || tvosCompat;
		}

		#endregion

		#region OS Version Check

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct NSOperatingSystemVersion
		{
			public long major;
			public long minor;
			public long patch;
		}

		private static IntPtr selOperatingSystemAtLeast = Selector("isOperatingSystemAtLeastVersion:");
		private static IntPtr selProcessInfo = Selector("processInfo");
		private static bool OperatingSystemAtLeast(long major, long minor, long patch)
		{
			NSOperatingSystemVersion version = new NSOperatingSystemVersion
			{
				major = major,
				minor = minor,
				patch = patch
			};
			IntPtr processInfo = intptr_objc_msgSend(
				classNSProcessInfo,
				selProcessInfo
			);
			return bool_objc_msgSend(
				processInfo,
				selOperatingSystemAtLeast,
				version
			);
		}

		#endregion

		#region MTLDevice

		private static IntPtr selName = Selector("name");
		private static string mtlGetDeviceName(IntPtr device)
		{
			return NSStringToUTF8(intptr_objc_msgSend(device, selName));
		}

		private static IntPtr selSupportsSampleCount = Selector("supportsSampleCount:");
		private static bool mtlSupportsSampleCount(IntPtr device, int count)
		{
			return bool_objc_msgSend(device, selSupportsSampleCount, (ulong) count);
		}

		private static IntPtr selCommandQueue = Selector("newCommandQueue");
		private static IntPtr mtlNewCommandQueue(IntPtr device)
		{
			return intptr_objc_msgSend(device, selCommandQueue);
		}

		private static IntPtr selNewBufferWithLength = Selector("newBufferWithLength:options:");
		private static IntPtr mtlNewBufferWithLength(IntPtr device, int length, MTLResourceOptions options)
		{
			return intptr_objc_msgSend(
				device,
				selNewBufferWithLength,
				(ulong) length,
				(ulong) options
			);
		}

		private static IntPtr selNewTextureWithDescriptor = Selector("newTextureWithDescriptor:");
		private static IntPtr mtlNewTextureWithDescriptor(IntPtr device, IntPtr texDesc)
		{
			return intptr_objc_msgSend(
				device,
				selNewTextureWithDescriptor,
				texDesc
			);
		}

		private static IntPtr selNewSamplerStateWithDescriptor = Selector("newSamplerStateWithDescriptor:");
		private static IntPtr mtlNewSamplerStateWithDescriptor(IntPtr device, IntPtr sampDesc)
		{
			return intptr_objc_msgSend(
				device,
				selNewSamplerStateWithDescriptor,
				sampDesc
			);
		}

		private static IntPtr selSupportsDepth24Stencil8 = Selector("isDepth24Stencil8PixelFormatSupported");
		private static bool mtlSupportsDepth24Stencil8(IntPtr device)
		{
			return bool_objc_msgSend(device, selSupportsDepth24Stencil8);
		}

		#endregion

		#region MTLBuffer

		private static IntPtr selContents = Selector("contents");
		private static IntPtr mtlGetBufferContentsPtr(IntPtr buffer)
		{
			return intptr_objc_msgSend(buffer, selContents);
		}

		#endregion

		#region MTLCommandBuffer

		private static IntPtr selRenderCommandEncoder = Selector("renderCommandEncoderWithDescriptor:");
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

		private static IntPtr selPresentDrawable = Selector("presentDrawable:");
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

		private static IntPtr selCommit = Selector("commit");
		private static void mtlCommitCommandBuffer(IntPtr commandBuffer)
		{
			objc_msgSend(commandBuffer, selCommit);
		}

		private static IntPtr selWaitUntilCompleted = Selector("waitUntilCompleted");
		private static void mtlCommandBufferWaitUntilCompleted(IntPtr commandBuffer)
		{
			objc_msgSend(commandBuffer, selWaitUntilCompleted);
		}

		#endregion

		#region MTLCommandQueue

		private static IntPtr selCommandBuffer = Selector("commandBuffer");
		private static IntPtr mtlMakeCommandBuffer(IntPtr queue)
		{
			return intptr_objc_msgSend(queue, selCommandBuffer);
		}

		#endregion

		#region Attachment Methods

		private static IntPtr selColorAttachments = Selector("colorAttachments");
		private static IntPtr selObjectAtIndexedSubscript = Selector("objectAtIndexedSubscript:");
		private static IntPtr mtlGetColorAttachment(IntPtr desc, int index)
		{
			IntPtr attachments = intptr_objc_msgSend(
				desc,
				selColorAttachments
			);

			return intptr_objc_msgSend(
				attachments, 
				selObjectAtIndexedSubscript,
				(ulong) index
			);
		}

		private static IntPtr selDepthAttachment = Selector("depthAttachment");
		private static IntPtr mtlGetDepthAttachment(IntPtr desc)
		{
			return intptr_objc_msgSend(desc, selDepthAttachment);
		}

		private static IntPtr selStencilAttachment = Selector("stencilAttachment");
		private static IntPtr mtlGetStencilAttachment(IntPtr desc)
		{
			return intptr_objc_msgSend(desc, selStencilAttachment);
		}

		private static IntPtr selSetLoadAction = Selector("setLoadAction:");
		private static void mtlSetAttachmentLoadAction(
			IntPtr attachment,
			MTLLoadAction loadAction
		) {
			objc_msgSend(attachment, selSetLoadAction, (ulong) loadAction);
		}

		private static IntPtr selSetStoreAction = Selector("setStoreAction:");
		private static void mtlSetAttachmentStoreAction(
			IntPtr attachment,
			MTLStoreAction storeAction
		) {
			objc_msgSend(attachment, selSetStoreAction, (ulong) storeAction);
		}

		private static IntPtr selSetTexture = Selector("setTexture:");
		private static void mtlSetAttachmentTexture(
			IntPtr attachment,
			IntPtr texture
		) {
			objc_msgSend(attachment, selSetTexture, texture);
		}

		private static IntPtr selSetSlice = Selector("setSlice:");
		private static void mtlSetAttachmentSlice(
			IntPtr attachment,
			int slice
		) {
			objc_msgSend(attachment, selSetSlice, (ulong) slice);
		}

		private static IntPtr selSetPixelFormat = Selector("setPixelFormat:");
		private static void mtlSetAttachmentPixelFormat(
			IntPtr attachment,
			MTLPixelFormat pixelFormat
		) {
			objc_msgSend(attachment, selSetPixelFormat, (ulong) pixelFormat);
		}

		private static IntPtr selSetResolveTexture = Selector("setResolveTexture:");
		private static void mtlSetAttachmentResolveTexture(
			IntPtr attachment,
			IntPtr resolveTexture
		) {
			objc_msgSend(attachment, selSetResolveTexture, resolveTexture);
		}

		private static IntPtr selSetResolveSlice = Selector("setResolveSlice:");
		private static void mtlSetAttachmentResolveSlice(
			IntPtr attachment,
			int resolveSlice
		) {
			objc_msgSend(attachment, selSetResolveSlice, (ulong) resolveSlice);
		}

		private static IntPtr selSetClearColor = Selector("setClearColor:");
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

		private static IntPtr selSetClearDepth = Selector("setClearDepth:");
		private static void mtlSetDepthAttachmentClearDepth(
			IntPtr depthAttachment,
			float clearDepth
		) {
			objc_msgSend(depthAttachment, selSetClearDepth, clearDepth);
		}

		private static IntPtr selSetClearStencil = Selector("setClearStencil:");
		private static void mtlSetStencilAttachmentClearStencil(
			IntPtr stencilAttachment,
			int clearStencil
		) {
			objc_msgSend(stencilAttachment, selSetClearStencil, (ulong) clearStencil);
		}

		private static IntPtr selBlendingEnabled = Selector("setBlendingEnabled:");
		private static void mtlSetAttachmentBlendingEnabled(
			IntPtr colorAttachment,
			bool enabled
		) {
			objc_msgSend(colorAttachment, selBlendingEnabled, enabled);
		}

		private static IntPtr selSetAlphaBlendOperation = Selector("setAlphaBlendOperation:");
		private static void mtlSetAttachmentAlphaBlendOperation(
			IntPtr colorAttachment,
			MTLBlendOperation op
		) {
			objc_msgSend(colorAttachment, selSetAlphaBlendOperation, (ulong) op);
		}

		private static IntPtr selSetRGBBlendOperation = Selector("setRgbBlendOperation:");
		private static void mtlSetAttachmentRGBBlendOperation(
			IntPtr colorAttachment,
			MTLBlendOperation op
		) {
			objc_msgSend(colorAttachment, selSetRGBBlendOperation, (ulong) op);
		}

		private static IntPtr selSetDestinationAlphaBlendFactor = Selector("setDestinationAlphaBlendFactor:");
		private static void mtlSetAttachmentDestinationAlphaBlendFactor(
			IntPtr colorAttachment,
			MTLBlendFactor blend
		) {
			objc_msgSend(colorAttachment, selSetDestinationAlphaBlendFactor, (ulong) blend);
		}

		private static IntPtr selSetDestinationRGBBlendFactor = Selector("setDestinationRGBBlendFactor:");
		private static void mtlSetAttachmentDestinationRGBBlendFactor(
			IntPtr colorAttachment,
			MTLBlendFactor blend
		) {
			objc_msgSend(colorAttachment, selSetDestinationRGBBlendFactor, (ulong) blend);
		}

		private static IntPtr selSetSourceAlphaBlendFactor = Selector("setSourceAlphaBlendFactor:");
		private static void mtlSetAttachmentSourceAlphaBlendFactor(
			IntPtr colorAttachment,
			MTLBlendFactor blend
		) {
			objc_msgSend(colorAttachment, selSetSourceAlphaBlendFactor, (ulong) blend);
		}

		private static IntPtr selSetSourceRGBBlendFactor = Selector("setSourceRGBBlendFactor:");
		private static void mtlSetAttachmentSourceRGBBlendFactor(
			IntPtr colorAttachment,
			MTLBlendFactor blend
		) {
			objc_msgSend(colorAttachment, selSetSourceRGBBlendFactor, (ulong) blend);
		}

		private static IntPtr selSetWriteMask = Selector("setWriteMask:");
		private static void mtlSetAttachmentWriteMask(
			IntPtr colorAttachment,
			int mask
		) {
			objc_msgSend(colorAttachment, selSetWriteMask, (ulong) mask);
		}

		#endregion

		#region MTLRenderPassDescriptor

		private static IntPtr selRenderPassDescriptor = Selector("renderPassDescriptor");
		private static IntPtr mtlMakeRenderPassDescriptor()
		{
			return intptr_objc_msgSend(classRenderPassDescriptor, selRenderPassDescriptor);
		}

		private static IntPtr selSetVisibilityBuffer = Selector("setVisibilityResultBuffer:");
		private static void mtlSetVisibilityResultBuffer(
			IntPtr renderPassDesc,
			IntPtr buffer
		) {
			objc_msgSend(renderPassDesc, selSetVisibilityBuffer, buffer);
		}

		#endregion

		#region MTLRenderCommandEncoder

		private static IntPtr selSetBlendColor = Selector("setBlendColorRed:green:blue:alpha:");
		private static void mtlSetBlendColor(
			IntPtr renderCommandEncoder,
			float red,
			float green,
			float blue,
			float alpha
		) {
			objc_msgSend(renderCommandEncoder, selSetBlendColor, red, green, blue, alpha);
		}

		private static IntPtr selSetStencilReference = Selector("setStencilReferenceValue:");
		private static void mtlSetStencilReferenceValue(
			IntPtr renderCommandEncoder,
			uint referenceValue
		) {
			objc_msgSend(renderCommandEncoder, selSetStencilReference, referenceValue);
		}

		private static IntPtr selSetViewport = Selector("setViewport:");
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

		private static IntPtr selSetScissorRect = Selector("setScissorRect:");
		private static void mtlSetScissorRect(
			IntPtr renderCommandEncoder,
			int x,
			int y,
			int w,
			int h
		) {
			MTLScissorRect rect = new MTLScissorRect(x, y, w, h);
			objc_msgSend(renderCommandEncoder, selSetScissorRect, rect);
		}

		private static IntPtr selEndEncoding = Selector("endEncoding");
		private static void mtlEndEncoding(IntPtr commandEncoder)
		{
			objc_msgSend(commandEncoder, selEndEncoding);
		}

		private static IntPtr selDrawIndexedPrimitives = Selector("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:instanceCount:");
		private static void mtlDrawIndexedPrimitives(
			IntPtr renderCommandEncoder,
			MTLPrimitiveType primitiveType,
			int indexCount,
			MTLIndexType indexType,
			IntPtr indexBuffer,
			int indexBufferOffset,
			int instanceCount
		) {
			objc_msgSend(
				renderCommandEncoder,
				selDrawIndexedPrimitives,
				(ulong) primitiveType,
				(ulong) indexCount,
				(ulong) indexType,
				indexBuffer,
				(ulong) indexBufferOffset,
				(ulong) instanceCount
			);
		}

		private static IntPtr selDrawPrimitives = Selector("drawPrimitives:vertexStart:vertexCount:");
		private static void mtlDrawPrimitives(
			IntPtr renderCommandEncoder,
			MTLPrimitiveType primitive,
			int vertexStart,
			int vertexCount
		) {
			objc_msgSend(
				renderCommandEncoder,
				selDrawPrimitives,
				(ulong) primitive,
				(ulong) vertexStart,
				(ulong) vertexCount
			);
		}

		private static IntPtr selSetCullMode = Selector("setCullMode:");
		private static void mtlSetCullMode(
			IntPtr renderCommandEncoder,
			MTLCullMode cullMode
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetCullMode,
				(ulong) cullMode
			);
		}

		private static IntPtr selSetTriangleFillMode = Selector("setTriangleFillMode:");
		private static void mtlSetTriangleFillMode(
			IntPtr renderCommandEncoder,
			MTLTriangleFillMode fillMode
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetTriangleFillMode,
				(ulong) fillMode
			);
		}

		private static IntPtr selSetDepthBias = Selector("setDepthBias:slopeScale:clamp:");
		private static void mtlSetDepthBias(
			IntPtr renderCommandEncoder,
			float depthBias,
			float slopeScaleDepthBias,
			float clamp
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetDepthBias,
				depthBias,
				slopeScaleDepthBias,
				clamp
			);
		}

		private static IntPtr selSetDepthStencilState = Selector("setDepthStencilState:");
		private static void mtlSetDepthStencilState(
			IntPtr renderCommandEncoder,
			IntPtr depthStencilState
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetDepthStencilState,
				depthStencilState
			);
		}

		private static IntPtr selInsertDebugSignpost = Selector("insertDebugSignpost:");
		private static void mtlInsertDebugSignpost(
			IntPtr renderCommandEncoder,
			string message
		) {
			objc_msgSend(
				renderCommandEncoder,
				selInsertDebugSignpost,
				UTF8ToNSString(message)
			);
		}

		private static IntPtr selSetRenderPipelineState = Selector("setRenderPipelineState:");
		private static void mtlSetRenderPipelineState(
			IntPtr renderCommandEncoder,
			IntPtr pipelineState
		) {
			objc_msgSend(renderCommandEncoder, selSetRenderPipelineState, pipelineState);
		}

		private static IntPtr selSetVertexBuffer = Selector("setVertexBuffer:offset:atIndex:");
		private static void mtlSetVertexBuffer(
			IntPtr renderCommandEncoder,
			IntPtr vertexBuffer,
			int offset,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetVertexBuffer,
				vertexBuffer,
				(ulong) offset,
				(ulong) index
			);
		}

		private static IntPtr selSetVertexBufferOffset = Selector("setVertexBufferOffset:atIndex:");
		private static void mtlSetVertexBufferOffset(
			IntPtr renderCommandEncoder,
			int offset,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetVertexBufferOffset,
				(ulong) offset,
				(ulong) index
			);
		}

		private static IntPtr selSetFragmentBuffer = Selector("setFragmentBuffer:offset:atIndex:");
		private static void mtlSetFragmentBuffer(
			IntPtr renderCommandEncoder,
			IntPtr fragmentBuffer,
			int offset,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetFragmentBuffer,
				fragmentBuffer,
				(ulong) offset,
				(ulong) index
			);
		}

		private static IntPtr selSetFragmentBufferOffset = Selector("setFragmentBufferOffset:atIndex:");
		private static void mtlSetFragmentBufferOffset(
			IntPtr renderCommandEncoder,
			int offset,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetFragmentBufferOffset,
				(ulong) offset,
				(ulong) index
			);
		}

		private static IntPtr selSetFragmentTexture = Selector("setFragmentTexture:atIndex:");
		private static void mtlSetFragmentTexture(
			IntPtr renderCommandEncoder,
			IntPtr fragmentTexture,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetFragmentTexture,
				fragmentTexture,
				(ulong) index
			);
		}

		private static IntPtr selSetFragmentSamplerState = Selector("setFragmentSamplerState:atIndex:");
		private static void mtlSetFragmentSamplerState(
			IntPtr renderCommandEncoder,
			IntPtr samplerState,
			int index
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetFragmentSamplerState,
				samplerState,
				(ulong) index
			);
		}

		private static IntPtr selSetVisibilityResultMode = Selector("setVisibilityResultMode:offset:");
		private static void mtlSetVisibilityResultMode(
			IntPtr renderCommandEncoder,
			MTLVisibilityResultMode mode,
			int offset
		) {
			objc_msgSend(
				renderCommandEncoder,
				selSetVisibilityResultMode,
				(ulong) mode,
				(ulong) offset
			);
		}

		#endregion

		#region CAMetalLayer

		private static IntPtr selLayer = Selector("layer");
		private static IntPtr mtlGetLayer(IntPtr view)
		{
			return intptr_objc_msgSend(view, selLayer);
		}

		private static IntPtr selSetDevice = Selector("setDevice:");
		private static void mtlSetLayerDevice(IntPtr layer, IntPtr device)
		{
			objc_msgSend(layer, selSetDevice, device);
		}

		private static IntPtr selNextDrawable = Selector("nextDrawable");
		private static IntPtr mtlNextDrawable(IntPtr layer)
		{
			return intptr_objc_msgSend(layer, selNextDrawable);
		}

		private static IntPtr selPixelFormat = Selector("pixelFormat");
		private static MTLPixelFormat mtlGetLayerPixelFormat(IntPtr layer)
		{
			return (MTLPixelFormat) ulong_objc_msgSend(layer, selPixelFormat);
		}

		private static IntPtr selDrawableSize = Selector("drawableSize");
		private static CGSize mtlGetDrawableSize(IntPtr layer)
		{
			return (CGSize) cgsize_objc_msgSend(layer, selDrawableSize);
		}

		private static IntPtr selDisplaySyncEnabled = Selector("setDisplaySyncEnabled:");
		private static void mtlSetDisplaySyncEnabled(
			IntPtr layer,
			bool enabled
		) {
			objc_msgSend(layer, selDisplaySyncEnabled, enabled);
		}

		private static IntPtr selSetFramebufferOnly = Selector("setFramebufferOnly:");
		private static void mtlSetLayerFramebufferOnly(
			IntPtr layer,
			bool framebufferOnly
		) {
			objc_msgSend(layer, selSetFramebufferOnly, framebufferOnly);
		}

		private static IntPtr selSetMagnificationFilter = Selector("setMagnificationFilter:");
		private static void mtlSetLayerMagnificationFilter(
			IntPtr layer,
			IntPtr val
		) {
			objc_msgSend(layer, selSetMagnificationFilter, val);
		}

		#endregion

		#region CAMetalDrawable

		private static IntPtr selTexture = Selector("texture");
		private static IntPtr mtlGetTextureFromDrawable(IntPtr drawable)
		{
			return intptr_objc_msgSend(drawable, selTexture);
		}

		#endregion

		#region MTLTextureDescriptor

		private static IntPtr selTexture2DDescriptor = Selector("texture2DDescriptorWithPixelFormat:width:height:mipmapped:");
		private static IntPtr mtlMakeTexture2DDescriptor(
			MTLPixelFormat pixelFormat,
			int width,
			int height,
			bool mipmapped
		) {
			return intptr_objc_msgSend(
				classTextureDescriptor,
				selTexture2DDescriptor,
				pixelFormat,
				(ulong) width,
				(ulong) height,
				mipmapped
			);
		}

		private static IntPtr selTextureCubeDescriptor = Selector("textureCubeDescriptorWithPixelFormat:size:mipmapped:");
		private static IntPtr mtlMakeTextureCubeDescriptor(
			MTLPixelFormat pixelFormat,
			int size,
			bool mipmapped
		) {
			return intptr_objc_msgSend(
				classTextureDescriptor,
				selTextureCubeDescriptor,
				pixelFormat,
				(ulong) size,
				mipmapped
			);
		}

		private static IntPtr selSetUsage = Selector("setUsage:");
		private static void mtlSetTextureUsage(
			IntPtr texDesc,
			MTLTextureUsage usage
		) {
			objc_msgSend(texDesc, selSetUsage, (ulong) usage);
		}

		private static IntPtr selSetTextureType = Selector("setTextureType:");
		private static void mtlSetTextureType(
			IntPtr texDesc,
			MTLTextureType type
		) {
			objc_msgSend(texDesc, selSetTextureType, (ulong) type);
		}

		private static IntPtr selSetSampleCount = Selector("setSampleCount:");
		private static void mtlSetTextureSampleCount(
			IntPtr texDesc,
			int sampleCount
		) {
			objc_msgSend(texDesc, selSetSampleCount, (ulong) sampleCount);
		}

		private static IntPtr selSetDepth = Selector("setDepth:");
		private static void mtlSetTextureDepth(
			IntPtr texDesc,
			int depth
		) {
			objc_msgSend(texDesc, selSetDepth, (ulong) depth);
		}

		#endregion

		#region MTLTexture

		private static IntPtr selReplaceRegion = Selector("replaceRegion:mipmapLevel:slice:withBytes:bytesPerRow:bytesPerImage:");
		private static void mtlReplaceRegion(
			IntPtr texture,
			MTLRegion region,
			int level,
			int slice,
			IntPtr pixelBytes,
			int bytesPerRow,
			int bytesPerImage
		) {
			objc_msgSend(
				texture,
				selReplaceRegion,
				region,
				(ulong) level,
				(ulong) slice,
				pixelBytes,
				(ulong) bytesPerRow,
				(ulong) bytesPerImage
			);
		}

		private static IntPtr selGetBytes = Selector("getBytes:bytesPerRow:bytesPerImage:fromRegion:mipmapLevel:slice:");
		private static void mtlGetTextureBytes(
			IntPtr texture,
			IntPtr pixelBytes,
			int bytesPerRow,
			int bytesPerImage,
			MTLRegion region,
			int level,
			int slice
		) {
			objc_msgSend(
				texture,
				selGetBytes,
				pixelBytes,
				(ulong) bytesPerRow,
				(ulong) bytesPerImage,
				region,
				(ulong) level,
				(ulong) slice
			);
		}

		private static IntPtr selWidth = Selector("width");
		private static ulong mtlGetTextureWidth(IntPtr texture)
		{
			return ulong_objc_msgSend(texture, selWidth);
		}

		private static IntPtr selHeight = Selector("height");
		private static ulong mtlGetTextureHeight(IntPtr texture)
		{
			return ulong_objc_msgSend(texture, selHeight);
		}

		private static IntPtr selSetPurgeableState = Selector("setPurgeableState:");
		private static MTLPurgeableState mtlSetPurgeableState(
			IntPtr resource,
			MTLPurgeableState state
		) {
			return (MTLPurgeableState) ulong_objc_msgSend(
				resource,
				selSetPurgeableState,
				(ulong) state
			);
		}

		#region MTLBlitCommandEncoder

		private static IntPtr selBlitCommandEncoder = Selector("blitCommandEncoder");
		private static IntPtr mtlMakeBlitCommandEncoder(IntPtr commandBuffer)
		{
			return intptr_objc_msgSend(commandBuffer, selBlitCommandEncoder);
		}

		private static IntPtr selCopyFromTexture = Selector("copyFromTexture:sourceSlice:sourceLevel:sourceOrigin:sourceSize:toTexture:destinationSlice:destinationLevel:destinationOrigin:");
		private static void mtlBlitTextureToTexture(
			IntPtr blitCommandEncoder,
			IntPtr srcTexture,
			int srcSlice,
			int srcLevel,
			MTLOrigin srcOrigin,
			MTLSize srcSize,
			IntPtr dstTexture,
			int dstSlice,
			int dstLevel,
			MTLOrigin dstOrigin
		) {
			objc_msgSend(
				blitCommandEncoder,
				selCopyFromTexture,
				srcTexture,
				(ulong) srcSlice,
				(ulong) srcLevel,
				srcOrigin,
				srcSize,
				dstTexture,
				(ulong) dstSlice,
				(ulong) dstLevel,
				dstOrigin
			);
		}

		private static IntPtr selSynchronizeResource = Selector("synchronizeResource:");
		private static void mtlSynchronizeResource(
			IntPtr blitCommandEncoder,
			IntPtr resource
		) {
			objc_msgSend(blitCommandEncoder, selSynchronizeResource, resource);
		}

		private static IntPtr selGenerateMipmaps = Selector("generateMipmapsForTexture:");
		private static void mtlGenerateMipmapsForTexture(
			IntPtr blitCommandEncoder,
			IntPtr texture
		) {
			objc_msgSend(blitCommandEncoder, selGenerateMipmaps, texture);
		}

		#endregion

		#region MTLRenderPipelineState

		private static IntPtr selNew = Selector("new");
		private static IntPtr mtlNewRenderPipelineDescriptor()
		{
			return intptr_objc_msgSend(classRenderPipelineDescriptor, selNew);
		}

		private static IntPtr selSetVertexFunction = Selector("setVertexFunction:");
		private static void mtlSetPipelineVertexFunction(
			IntPtr pipelineDescriptor,
			IntPtr vertexFunction
		) {
			objc_msgSend(pipelineDescriptor, selSetVertexFunction, vertexFunction);
		}

		private static IntPtr selSetFragmentFunction = Selector("setFragmentFunction:");
		private static void mtlSetPipelineFragmentFunction(
			IntPtr pipelineDescriptor,
			IntPtr fragmentFunction
		) {
			objc_msgSend(pipelineDescriptor, selSetFragmentFunction, fragmentFunction);
		}

		private static IntPtr selSetVertexDescriptor = Selector("setVertexDescriptor:");
		private static void mtlSetPipelineVertexDescriptor(
			IntPtr pipelineDescriptor,
			IntPtr vertexDescriptor
		) {
			objc_msgSend(pipelineDescriptor, selSetVertexDescriptor, vertexDescriptor);
		}

		private static IntPtr selNewRenderPipelineState = Selector("newRenderPipelineStateWithDescriptor:error:");
		private static IntPtr mtlNewRenderPipelineStateWithDescriptor(
			IntPtr device,
			IntPtr pipelineDescriptor
		) {
			IntPtr error = IntPtr.Zero;
			IntPtr pipeline = intptr_objc_msgSend(
				device,
				selNewRenderPipelineState,
				pipelineDescriptor,
				out error
			);
			if (error != IntPtr.Zero)
			{
				throw new Exception("Metal Error: " + GetNSErrorDescription(error));
			}
			return pipeline;
		}

		private static IntPtr selSetDepthAttachmentPixelFormat = Selector("setDepthAttachmentPixelFormat:");
		private static void mtlSetDepthAttachmentPixelFormat(
			IntPtr pipelineDescriptor,
			MTLPixelFormat format
		) {
			objc_msgSend(pipelineDescriptor, selSetDepthAttachmentPixelFormat, (ulong) format);
		}

		private static IntPtr selSetStencilAttachmentPixelFormat = Selector("setStencilAttachmentPixelFormat:");
		private static void mtlSetStencilAttachmentPixelFormat(
			IntPtr pipelineDescriptor,
			MTLPixelFormat format
		) {
			objc_msgSend(pipelineDescriptor, selSetStencilAttachmentPixelFormat, (ulong) format);
		}

		private static void mtlSetPipelineSampleCount(
			IntPtr pipelineDescriptor,
			int sampleCount
		) {
			objc_msgSend(pipelineDescriptor, selSetSampleCount, (ulong) sampleCount);
		}

		#endregion

		#region Sampler Descriptor

		// selNew already defined
		private static IntPtr mtlNewSamplerDescriptor()
		{
			return intptr_objc_msgSend(classMTLSamplerDescriptor, selNew);
		}

		private static IntPtr selSetMinFilter = Selector("setMinFilter:");
		private static void mtlSetSamplerMinFilter(
			IntPtr samplerDesc,
			MTLSamplerMinMagFilter filter
		) {
			objc_msgSend(samplerDesc, selSetMinFilter, (uint) filter);
		}

		private static IntPtr selSetMagFilter = Selector("setMagFilter:");
		private static void mtlSetSamplerMagFilter(
			IntPtr samplerDesc,
			MTLSamplerMinMagFilter filter
		) {
			objc_msgSend(samplerDesc, selSetMagFilter, (uint) filter);
		}

		private static IntPtr selSetMipFilter = Selector("setMipFilter:");
		private static void mtlSetSamplerMipFilter(
			IntPtr samplerDesc,
			MTLSamplerMipFilter filter
		) {
			objc_msgSend(samplerDesc, selSetMipFilter, (uint) filter);
		}

		private static IntPtr selSetLodMinClamp = Selector("setLodMinClamp:");
		private static void mtlSetSamplerLodMinClamp(
			IntPtr samplerDesc,
			float clamp
		) {
			objc_msgSend(samplerDesc, selSetLodMinClamp, clamp);
		}

		private static IntPtr selSetMaxAnisotropy = Selector("setMaxAnisotropy:");
		private static void mtlSetSamplerMaxAnisotropy(
			IntPtr samplerDesc,
			int maxAnisotropy
		) {
			objc_msgSend(samplerDesc, selSetMaxAnisotropy, (ulong) maxAnisotropy);
		}

		private static IntPtr selSetRAddressMode = Selector("setRAddressMode:");
		private static void mtlSetSampler_rAddressMode(
			IntPtr samplerDesc,
			MTLSamplerAddressMode mode
		) {
			objc_msgSend(samplerDesc, selSetRAddressMode, (ulong) mode);
		}

		private static IntPtr selSetSAddressMode = Selector("setSAddressMode:");
		private static void mtlSetSampler_sAddressMode(
			IntPtr samplerDesc,
			MTLSamplerAddressMode mode
		) {
			objc_msgSend(samplerDesc, selSetSAddressMode, (ulong) mode);
		}
		
		private static IntPtr selSetTAddressMode = Selector("setTAddressMode:");
		private static void mtlSetSampler_tAddressMode(
			IntPtr samplerDesc,
			MTLSamplerAddressMode mode
		) {
			objc_msgSend(samplerDesc, selSetTAddressMode, (ulong) mode);
		}

		#endregion

		#region Vertex Descriptor

		private static IntPtr selVertexDescriptor = Selector("vertexDescriptor");
		private static IntPtr mtlMakeVertexDescriptor()
		{
			return intptr_objc_msgSend(classMTLVertexDescriptor, selVertexDescriptor);
		}

		private static IntPtr selAttributes = Selector("attributes");
		private static IntPtr mtlGetVertexAttributeDescriptor(
			IntPtr vertexDesc,
			int index
		) {
			IntPtr attributes = intptr_objc_msgSend(
				vertexDesc,
				selAttributes
			);

			return intptr_objc_msgSend(
				attributes, 
				selObjectAtIndexedSubscript,
				(ulong) index
			);
		}

		private static IntPtr selSetFormat = Selector("setFormat:");
		private static void mtlSetVertexAttributeFormat(
			IntPtr vertexAttribute,
			MTLVertexFormat format
		) {
			objc_msgSend(vertexAttribute, selSetFormat, (ulong) format);
		}

		private static IntPtr selSetOffset = Selector("setOffset:");
		private static void mtlSetVertexAttributeOffset(
			IntPtr vertexAttribute,
			int offset
		) {
			objc_msgSend(vertexAttribute, selSetOffset, (ulong) offset);
		}

		private static IntPtr selSetBufferIndex = Selector("setBufferIndex:");
		private static void mtlSetVertexAttributeBufferIndex(
			IntPtr vertexAttribute,
			int bufferIndex
		) {
			objc_msgSend(vertexAttribute, selSetBufferIndex, (ulong) bufferIndex);
		}

		private static IntPtr selLayouts = Selector("layouts");
		private static IntPtr mtlGetVertexBufferLayoutDescriptor(
			IntPtr vertexDesc,
			int index
		) {
			IntPtr layouts = intptr_objc_msgSend(
				vertexDesc,
				selLayouts
			);

			return intptr_objc_msgSend(
				layouts, 
				selObjectAtIndexedSubscript,
				(ulong) index
			);
		}

		private static IntPtr selSetStride = Selector("setStride:");
		private static void mtlSetVertexBufferLayoutStride(
			IntPtr vertexBufferLayout,
			int stride
		) {
			objc_msgSend(vertexBufferLayout, selSetStride, (ulong) stride);
		}

		private static IntPtr selSetStepFunction = Selector("setStepFunction:");
		private static void mtlSetVertexBufferLayoutStepFunction(
			IntPtr vertexBufferLayout,
			MTLVertexStepFunction stepFunc
		) {
			objc_msgSend(vertexBufferLayout, selSetStepFunction, (ulong) stepFunc);
		}

		private static IntPtr selSetStepRate = Selector("setStepRate:");
		private static void mtlSetVertexBufferLayoutStepRate(
			IntPtr vertexBufferLayout,
			int stepRate
		) {
			objc_msgSend(vertexBufferLayout, selSetStepRate, (ulong) stepRate);
		}

		#endregion

		#region Storage Modes

		private static IntPtr selSetStorageMode = Selector("setStorageMode:");
		private static void mtlSetStorageMode(
			IntPtr resource,
			MTLStorageMode mode
		) {
			objc_msgSend(resource, selSetStorageMode, (ulong) mode);
		}

		#endregion

		#region MTLLibrary

		private static IntPtr selNewLibraryWithSource = Selector("newLibraryWithSource:options:error:");
		private static IntPtr mtlNewLibraryWithSource(
			IntPtr device,
			IntPtr shaderSourceNSString,
			IntPtr compileOptions
		) {
			IntPtr error = IntPtr.Zero;
			IntPtr library = intptr_objc_msgSend(
				device,
				selNewLibraryWithSource,
				shaderSourceNSString,
				compileOptions,
				out error
			);
			if (error != IntPtr.Zero)
			{
				throw new Exception("Metal Error: " + GetNSErrorDescription(error));
			}
			return library;
		}

		private static IntPtr selNewFunctionWithName = Selector("newFunctionWithName:");
		private static IntPtr mtlNewFunctionWithName(
			IntPtr library,
			IntPtr shaderNameNSString
		) {
			return intptr_objc_msgSend(
				library,
				selNewFunctionWithName,
				shaderNameNSString
			);
		}

		#endregion

		#region Depth-Stencil State

		private static IntPtr mtlNewDepthStencilDescriptor()
		{
			return intptr_objc_msgSend(classDepthStencilDescriptor, selNew);
		}

		private static IntPtr mtlNewStencilDescriptor()
		{
			return intptr_objc_msgSend(classStencilDescriptor, selNew);
		}

		private static IntPtr selSetDepthCompareFunction = Selector("setDepthCompareFunction:");
		private static void mtlSetDepthCompareFunction(
			IntPtr depthStencilDescriptor,
			MTLCompareFunction func
		) {
			objc_msgSend(depthStencilDescriptor, selSetDepthCompareFunction, (ulong) func);
		}

		private static IntPtr selSetDepthWriteEnabled = Selector("setDepthWriteEnabled:");
		private static void mtlSetDepthWriteEnabled(
			IntPtr depthStencilDescriptor,
			bool enabled
		) {
			objc_msgSend(depthStencilDescriptor, selSetDepthWriteEnabled, enabled);
		}

		private static IntPtr selSetBackFaceStencil = Selector("setBackFaceStencil:");
		private static void mtlSetBackFaceStencil(
			IntPtr depthStencilDescriptor,
			IntPtr stencilDescriptor
		) {
			objc_msgSend(depthStencilDescriptor, selSetBackFaceStencil, stencilDescriptor);
		}

		private static IntPtr selSetFrontFaceStencil = Selector("setFrontFaceStencil:");
		private static void mtlSetFrontFaceStencil(
			IntPtr depthStencilDescriptor,
			IntPtr stencilDescriptor
		) {
			objc_msgSend(depthStencilDescriptor, selSetFrontFaceStencil, stencilDescriptor);
		}

		private static IntPtr selNewDepthStencilStateWithDescriptor = Selector("newDepthStencilStateWithDescriptor:");
		private static IntPtr mtlNewDepthStencilStateWithDescriptor(
			IntPtr device,
			IntPtr descriptor
		) {
			return intptr_objc_msgSend(
				device,
				selNewDepthStencilStateWithDescriptor,
				descriptor
			);
		}

		private static IntPtr selSetStencilFailureOperation = Selector("setStencilFailureOperation:");
		private static void mtlSetStencilFailureOperation(
			IntPtr stencilDescriptor,
			MTLStencilOperation op
		) {
			objc_msgSend(stencilDescriptor, selSetStencilFailureOperation, (ulong) op);
		}

		private static IntPtr selSetDepthFailureOperation = Selector("setDepthFailureOperation:");
		private static void mtlSetDepthFailureOperation(
			IntPtr stencilDescriptor,
			MTLStencilOperation op
		) {
			objc_msgSend(stencilDescriptor, selSetDepthFailureOperation, (ulong) op);
		}

		private static IntPtr selSetDepthStencilPassOperation = Selector("setDepthStencilPassOperation:");
		private static void mtlSetDepthStencilPassOperation(
			IntPtr stencilDescriptor,
			MTLStencilOperation op
		) {
			objc_msgSend(stencilDescriptor, selSetDepthStencilPassOperation, (ulong) op);
		}

		private static IntPtr selSetStencilCompareFunction = Selector("setStencilCompareFunction:");
		private static void mtlSetStencilCompareFunction(
			IntPtr stencilDescriptor,
			MTLCompareFunction func
		) {
			objc_msgSend(stencilDescriptor, selSetStencilCompareFunction, (ulong) func);
		}

		private static IntPtr selSetStencilReadMask = Selector("setReadMask:");
		private static void mtlSetStencilReadMask(
			IntPtr stencilDescriptor,
			uint mask
		) {
			objc_msgSend(stencilDescriptor, selSetStencilReadMask, mask);
		}

		private static IntPtr selSetStencilWriteMask = Selector("setWriteMask:");
		private static void mtlSetStencilWriteMask(
			IntPtr stencilDescriptor,
			uint mask
		) {
			objc_msgSend(stencilDescriptor, selSetStencilWriteMask, mask);
		}

		#endregion

		#region Error Handling

		private static IntPtr selLocalizedDescription = Selector("localizedDescription");
		private static string GetNSErrorDescription(IntPtr error)
		{
			return NSStringToUTF8(intptr_objc_msgSend(error, selLocalizedDescription));
		}

		#endregion

		#endregion
	}
}