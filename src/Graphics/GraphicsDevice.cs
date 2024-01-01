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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class GraphicsDevice : IDisposable
	{
		#region Internal Constants

		// Per XNA4 General Spec
		internal const int MAX_TEXTURE_SAMPLERS = 16;

		// Per XNA4 HiDef Spec
		internal const int MAX_VERTEX_ATTRIBUTES = 16;
		internal const int MAX_RENDERTARGET_BINDINGS = 4;
		internal const int MAX_VERTEXTEXTURE_SAMPLERS = 4;

		#endregion

		#region Public GraphicsDevice State Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public GraphicsDeviceStatus GraphicsDeviceStatus
		{
			get
			{
				return GraphicsDeviceStatus.Normal;
			}
		}

		public GraphicsAdapter Adapter
		{
			get;
			private set;
		}

		public GraphicsProfile GraphicsProfile
		{
			get;
			private set;
		}

		public PresentationParameters PresentationParameters
		{
			get;
			private set;
		}

		#endregion

		#region Public Graphics Display Properties

		public DisplayMode DisplayMode
		{
			get
			{
				if (PresentationParameters.IsFullScreen)
				{
					int w, h;
					FNA3D.FNA3D_GetBackbufferSize(GLDevice, out w, out h);
					return new DisplayMode(
						w,
						h,
						FNA3D.FNA3D_GetBackbufferSurfaceFormat(GLDevice)
					);
				}
				return Adapter.CurrentDisplayMode;
			}
		}

		#endregion

		#region Public GL State Properties

		public TextureCollection Textures
		{
			get;
			private set;
		}

		public SamplerStateCollection SamplerStates
		{
			get;
			private set;
		}

		public TextureCollection VertexTextures
		{
			get;
			private set;
		}

		public SamplerStateCollection VertexSamplerStates
		{
			get;
			private set;
		}

		public BlendState BlendState
		{
			get
			{
				return nextBlend;
			}
			set
			{
				nextBlend = value;
			}
		}

		public DepthStencilState DepthStencilState
		{
			get
			{
				return nextDepthStencil;
			}
			set
			{
				nextDepthStencil = value;
			}
		}

		public RasterizerState RasterizerState
		{
			get;
			set;
		}

		/* We have to store this internally because we flip the Rectangle for
		 * when we aren't rendering to a target. I'd love to remove this.
		 * -flibit
		 */
		private Rectangle INTERNAL_scissorRectangle;
		public Rectangle ScissorRectangle
		{
			get
			{
				return INTERNAL_scissorRectangle;
			}
			set
			{
				INTERNAL_scissorRectangle = value;
				FNA3D.FNA3D_SetScissorRect(
					GLDevice,
					ref value
				);
			}
		}

		/* We have to store this internally because we flip the Viewport for
		 * when we aren't rendering to a target. I'd love to remove this.
		 * -flibit
		 */
		private Viewport INTERNAL_viewport;
		public Viewport Viewport
		{
			get
			{
				return INTERNAL_viewport;
			}
			set
			{
				INTERNAL_viewport = value;
				FNA3D.FNA3D_SetViewport(
					GLDevice,
					ref value.viewport
				);
			}
		}

		public Color BlendFactor
		{
			get
			{
				Color result;
				FNA3D.FNA3D_GetBlendFactor(GLDevice, out result);
				return result;
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				FNA3D.FNA3D_SetBlendFactor(GLDevice, ref value);
			}
		}

		public int MultiSampleMask
		{
			get
			{
				return FNA3D.FNA3D_GetMultiSampleMask(GLDevice);
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				FNA3D.FNA3D_SetMultiSampleMask(GLDevice, value);
			}
		}

		public int ReferenceStencil
		{
			get
			{
				return FNA3D.FNA3D_GetReferenceStencil(GLDevice);
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * DepthStencilState?
				 * -flibit
				 */
				FNA3D.FNA3D_SetReferenceStencil(GLDevice, value);
			}
		}

		#endregion

		#region Public Buffer Object Properties

		public IndexBuffer Indices
		{
			get;
			set;
		}

		#endregion

		#region Internal FNA3D_Device

		internal readonly IntPtr GLDevice;

		#endregion

		#region Internal Pipeline Cache

		internal readonly PipelineCache PipelineCache;

		#endregion

		#region Private State Shadowing Variables

		private BlendState currentBlend;
		private BlendState nextBlend;
		private DepthStencilState currentDepthStencil;
		private DepthStencilState nextDepthStencil;

		#endregion

		#region Internal Sampler Change Queue

		private readonly bool[] modifiedSamplers = new bool[MAX_TEXTURE_SAMPLERS];
		private readonly bool[] modifiedVertexSamplers = new bool[MAX_VERTEXTEXTURE_SAMPLERS];

		#endregion

		#region Internal State Changes Pointer

		internal IntPtr effectStateChangesPtr;

		#endregion

		#region Private Disposal Variables

		/* 
		 * Use weak GCHandles for the global resources list as we do not
		 * know when a resource may be disposed and collected. We do not
		 * want to prevent a resource from being collected by holding a
		 * strong reference to it in this list. Using the WeakReference
		 * class would produce unnecessary allocations - we don't need
		 * its finalizer or shareability for this scenario since every
		 * GraphicsResource has a finalizer.
		 */
		private readonly List<GCHandle> resources = new List<GCHandle>();
		private readonly object resourcesLock = new object();

		#endregion

		#region Private Clear Variables

		/* On Intel Integrated graphics, there is a fast hw unit for doing
		 * clears to colors where all components are either 0 or 255.
		 * Despite XNA4 using Purple here, we use black (in Release) to avoid
		 * performance warnings on Intel/Mesa.
		 * -sulix
		 *
		 * Also, these are NOT readonly, for weird performance reasons -flibit
		 */
#if DEBUG
		private static Vector4 DiscardColor = new Color(68, 34, 136, 255).ToVector4();
#else
		private static Vector4 DiscardColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
#endif

		#endregion

		#region Private RenderTarget Variables

		// Some of these are internal for validation purposes

		internal readonly RenderTargetBinding[] renderTargetBindings =
			new RenderTargetBinding[MAX_RENDERTARGET_BINDINGS];
		private FNA3D.FNA3D_RenderTargetBinding[] nativeTargetBindings =
			new FNA3D.FNA3D_RenderTargetBinding[MAX_RENDERTARGET_BINDINGS];
		private FNA3D.FNA3D_RenderTargetBinding[] nativeTargetBindingsNext =
			new FNA3D.FNA3D_RenderTargetBinding[MAX_RENDERTARGET_BINDINGS];

		internal int renderTargetCount = 0;

		// Used to prevent allocs on SetRenderTarget()
		private readonly RenderTargetBinding[] singleTargetCache = new RenderTargetBinding[1];

		#endregion

		#region Private Buffer Object Variables

		private readonly VertexBufferBinding[] vertexBufferBindings =
			new VertexBufferBinding[MAX_VERTEX_ATTRIBUTES];
		private readonly FNA3D.FNA3D_VertexBufferBinding[] nativeBufferBindings =
			new FNA3D.FNA3D_VertexBufferBinding[MAX_VERTEX_ATTRIBUTES];
		private int vertexBufferCount = 0;
		private bool vertexBuffersUpdated = false;

		// Used for client arrays
		IntPtr userVertexBuffer, userIndexBuffer;
		int userVertexBufferSize, userIndexBufferSize;

		#endregion

		#region GraphicsDevice Events

#pragma warning disable 0067
		// We never lose devices, but lol XNA4 compliance -flibit
		public event EventHandler<EventArgs> DeviceLost;
#pragma warning restore 0067
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
		public event EventHandler<ResourceCreatedEventArgs> ResourceCreated;
		public event EventHandler<ResourceDestroyedEventArgs> ResourceDestroyed;
		public event EventHandler<EventArgs> Disposing;

		// TODO: Hook this up to GraphicsResource
		internal void OnResourceCreated(object resource)
		{
			if (ResourceCreated != null)
			{
				ResourceCreated(this, new ResourceCreatedEventArgs(resource));
			}
		}

		// TODO: Hook this up to GraphicsResource
		internal void OnResourceDestroyed(string name, object tag)
		{
			if (ResourceDestroyed != null)
			{
				ResourceDestroyed(this, new ResourceDestroyedEventArgs(name, tag));
			}
		}

		#endregion

		#region Constructor, Destructor, Dispose Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
		/// </summary>
		/// <param name="adapter">The graphics adapter.</param>
		/// <param name="graphicsProfile">The graphics profile.</param>
		/// <param name="presentationParameters">The presentation options.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="presentationParameters"/> is <see langword="null"/>.
		/// </exception>
		public GraphicsDevice(
			GraphicsAdapter adapter,
			GraphicsProfile graphicsProfile,
			PresentationParameters presentationParameters
		) {
			if (presentationParameters == null)
			{
				throw new ArgumentNullException("presentationParameters");
			}

			// Set the properties from the constructor parameters.
			Adapter = adapter;
			PresentationParameters = presentationParameters;
			GraphicsProfile = graphicsProfile;
			PresentationParameters.MultiSampleCount = MathHelper.ClosestMSAAPower(
				PresentationParameters.MultiSampleCount
			);

			// Set up the FNA3D Device
			try
			{
				GLDevice = FNA3D.FNA3D_CreateDevice(
					ref PresentationParameters.parameters,
#if DEBUG
					1
#else
					0
#endif
				);
			}
			catch(Exception e)
			{
				throw new NoSuitableGraphicsDeviceException(
					e.Message
				);
			}

			// The mouse needs to know this for faux-backbuffer mouse scaling.
			Input.Mouse.INTERNAL_BackBufferWidth = PresentationParameters.BackBufferWidth;
			Input.Mouse.INTERNAL_BackBufferHeight = PresentationParameters.BackBufferHeight;

			// The Touch Panel needs this too, for the same reason.
			Input.Touch.TouchPanel.DisplayWidth = PresentationParameters.BackBufferWidth;
			Input.Touch.TouchPanel.DisplayHeight = PresentationParameters.BackBufferHeight;

			// Force set the default render states.
			BlendState = BlendState.Opaque;
			DepthStencilState = DepthStencilState.Default;
			RasterizerState = RasterizerState.CullCounterClockwise;

			// Initialize the Texture/Sampler state containers
			int maxTextures, maxVertexTextures;
			FNA3D.FNA3D_GetMaxTextureSlots(
				GLDevice,
				out maxTextures,
				out maxVertexTextures
			);
			Textures = new TextureCollection(
				maxTextures,
				modifiedSamplers
			);
			SamplerStates = new SamplerStateCollection(
				maxTextures,
				modifiedSamplers
			);
			VertexTextures = new TextureCollection(
				maxVertexTextures,
				modifiedVertexSamplers
			);
			VertexSamplerStates = new SamplerStateCollection(
				maxVertexTextures,
				modifiedVertexSamplers
			);

			// Set the default viewport and scissor rect.
			Viewport = new Viewport(PresentationParameters.Bounds);
			ScissorRectangle = Viewport.Bounds;

			// Allocate the pipeline cache to be used by Effects
			PipelineCache = new PipelineCache(this);

			// Set up the effect state changes pointer.
			unsafe
			{
				effectStateChangesPtr = FNAPlatform.Malloc(
					sizeof(Effect.MOJOSHADER_effectStateChanges)
				);
                Effect.MOJOSHADER_effectStateChanges* stateChanges =
					(Effect.MOJOSHADER_effectStateChanges*) effectStateChangesPtr;
				stateChanges->render_state_change_count = 0;
				stateChanges->sampler_state_change_count = 0;
				stateChanges->vertex_sampler_state_change_count = 0;
			}
		}

		~GraphicsDevice()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// We're about to dispose, notify the application.
					if (Disposing != null)
					{
						Disposing(this, EventArgs.Empty);
					}

					/* Dispose of all remaining graphics resources before
					 * disposing of the GraphicsDevice.
					 */
					lock (resourcesLock)
					{
						foreach (GCHandle resource in resources.ToArray())
						{
							object target = resource.Target;
							if (target != null)
							{
								(target as IDisposable).Dispose();
							}
						}
						resources.Clear();
					}

					if (userVertexBuffer != IntPtr.Zero)
					{
						FNA3D.FNA3D_AddDisposeVertexBuffer(
							GLDevice,
							userVertexBuffer
						);
					}
					if (userIndexBuffer != IntPtr.Zero)
					{
						FNA3D.FNA3D_AddDisposeIndexBuffer(
							GLDevice,
							userIndexBuffer
						);
					}

					FNAPlatform.Free(effectStateChangesPtr);

					// Dispose of the GL Device/Context
					FNA3D.FNA3D_DestroyDevice(GLDevice);
				}

				IsDisposed = true;
			}
		}

		#endregion

		#region Internal Resource Management Methods

		internal void AddResourceReference(GCHandle resourceReference)
		{
			lock (resourcesLock)
			{
				resources.Add(resourceReference);
			}
		}

		internal void RemoveResourceReference(GCHandle resourceReference)
		{
			lock (resourcesLock)
			{
				// Scan the list and do value comparisons (List.Remove will box the handles)
				for (int i = 0, c = resources.Count; i < c; i++)
				{
					if (resources[i] != resourceReference)
						continue;

					// Perform an unordered removal, the order of items in this list does not matter
					resources[i] = resources[resources.Count - 1];
					resources.RemoveAt(resources.Count - 1);
					return;
				}
			}
		}

		#endregion

		#region Public Present Method

		public void Present()
		{
			FNA3D.FNA3D_SwapBuffers(
				GLDevice,
				IntPtr.Zero,
				IntPtr.Zero,
				PresentationParameters.DeviceWindowHandle
			);
		}

		public void Present(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			if (overrideWindowHandle == IntPtr.Zero)
			{
				overrideWindowHandle = PresentationParameters.DeviceWindowHandle;
			}
			if (sourceRectangle.HasValue && destinationRectangle.HasValue)
			{
				Rectangle src = sourceRectangle.Value;
				Rectangle dst = destinationRectangle.Value;
				FNA3D.FNA3D_SwapBuffers(
					GLDevice,
					ref src,
					ref dst,
					overrideWindowHandle
				);
			}
			else if (sourceRectangle.HasValue)
			{
				Rectangle src = sourceRectangle.Value;
				FNA3D.FNA3D_SwapBuffers(
					GLDevice,
					ref src,
					IntPtr.Zero,
					overrideWindowHandle
				);
			}
			else if (destinationRectangle.HasValue)
			{
				Rectangle dst = destinationRectangle.Value;
				FNA3D.FNA3D_SwapBuffers(
					GLDevice,
					IntPtr.Zero,
					ref dst,
					overrideWindowHandle
				);
			}
			else
			{
				FNA3D.FNA3D_SwapBuffers(
					GLDevice,
					IntPtr.Zero,
					IntPtr.Zero,
					overrideWindowHandle
				);
			}
		}

		#endregion

		#region Public Reset Methods

		public void Reset()
		{
			Reset(PresentationParameters, Adapter);
		}

		public void Reset(PresentationParameters presentationParameters)
		{
			Reset(presentationParameters, Adapter);
		}

		public void Reset(
			PresentationParameters presentationParameters,
			GraphicsAdapter graphicsAdapter
		) {
			if (presentationParameters == null)
			{
				throw new ArgumentNullException("presentationParameters");
			}
			PresentationParameters = presentationParameters;
			Adapter = graphicsAdapter;

			// Verify MSAA before we really start...
			PresentationParameters.MultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
				GLDevice,
				PresentationParameters.BackBufferFormat,
				MathHelper.ClosestMSAAPower(PresentationParameters.MultiSampleCount)
			);

			// We're about to reset, let the application know.
			if (DeviceResetting != null)
			{
				DeviceResetting(this, EventArgs.Empty);
			}

			/* FIXME: Why are we not doing this...? -flibit
			lock (resourcesLock)
			{
				foreach (WeakReference resource in resources)
				{
					object target = resource.Target;
					if (target != null)
					{
						(target as GraphicsResource).GraphicsDeviceResetting();
					}
				}

				// Remove references to resources that have been garbage collected.
				resources.RemoveAll(wr => !wr.IsAlive);
			}
			*/

			/* Reset the backbuffer first, before doing anything else.
			 * The GLDevice needs to know what we're up to right away.
			 * -flibit
			 */
			FNA3D.FNA3D_ResetBackbuffer(
				GLDevice,
				ref PresentationParameters.parameters
			);

			// The mouse needs to know this for faux-backbuffer mouse scaling.
			Input.Mouse.INTERNAL_BackBufferWidth = PresentationParameters.BackBufferWidth;
			Input.Mouse.INTERNAL_BackBufferHeight = PresentationParameters.BackBufferHeight;

			// The Touch Panel needs this too, for the same reason.
			Input.Touch.TouchPanel.DisplayWidth = PresentationParameters.BackBufferWidth;
			Input.Touch.TouchPanel.DisplayHeight = PresentationParameters.BackBufferHeight;

			// Now, update the viewport
			Viewport = new Viewport(
				0,
				0,
				PresentationParameters.BackBufferWidth,
				PresentationParameters.BackBufferHeight
			);

			// Update the scissor rectangle to our new default target size
			ScissorRectangle = new Rectangle(
				0,
				0,
				PresentationParameters.BackBufferWidth,
				PresentationParameters.BackBufferHeight
			);

			// We just reset, let the application know.
			if (DeviceReset != null)
			{
				DeviceReset(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Public Clear Methods

		public void Clear(Color color)
		{
			Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
				color.ToVector4(),
				Viewport.MaxDepth,
				0
			);
		}

		public void Clear(ClearOptions options, Color color, float depth, int stencil)
		{
			Clear(
				options,
				color.ToVector4(),
				depth,
				stencil
			);
		}

		public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
		{
			DepthFormat dsFormat;
			if (renderTargetCount == 0)
			{
				/* FIXME: PresentationParameters.DepthStencilFormat is probably
				 * a more accurate value here, but the Backbuffer may disagree.
				 * -flibit
				 */
				dsFormat = FNA3D.FNA3D_GetBackbufferDepthFormat(GLDevice);
			}
			else
			{
				dsFormat = (renderTargetBindings[0].RenderTarget as IRenderTarget).DepthStencilFormat;
			}
			if (dsFormat == DepthFormat.None)
			{
				options &= ClearOptions.Target;
			}
			else if (dsFormat != DepthFormat.Depth24Stencil8)
			{
				options &= ~ClearOptions.Stencil;
			}
			FNA3D.FNA3D_Clear(
				GLDevice,
				options,
				ref color,
				depth,
				stencil
			);
		}

		#endregion

		#region Public Backbuffer Methods

		public void GetBackBufferData<T>(T[] data) where T : struct
		{
			GetBackBufferData(null, data, 0, data.Length);
		}

		public void GetBackBufferData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GetBackBufferData(null, data, startIndex, elementCount);
		}

		public void GetBackBufferData<T>(
			Rectangle? rect,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			int x, y, w, h;
			if (rect == null)
			{
				x = 0;
				y = 0;
				FNA3D.FNA3D_GetBackbufferSize(
					GLDevice,
					out w,
					out h
				);
			}
			else
			{
				x = rect.Value.X;
				y = rect.Value.Y;
				w = rect.Value.Width;
				h = rect.Value.Height;
			}

			int elementSizeInBytes = MarshalHelper.SizeOf<T>();
			Texture.ValidateGetDataFormat(
				FNA3D.FNA3D_GetBackbufferSurfaceFormat(GLDevice),
				elementSizeInBytes
			);

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_ReadBackbuffer(
				GLDevice,
				x,
				y,
				w,
				h,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				data.Length * elementSizeInBytes
			);
			handle.Free();
		}

		#endregion

		#region Public RenderTarget Methods

		public void SetRenderTarget(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
			{
				SetRenderTargets(null);
			}
			else
			{
				singleTargetCache[0] = new RenderTargetBinding(renderTarget);
				SetRenderTargets(singleTargetCache);
			}
		}

		public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
		{
			if (renderTarget == null)
			{
				SetRenderTargets(null);
			}
			else
			{
				singleTargetCache[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
				SetRenderTargets(singleTargetCache);
			}
		}

		public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
		{
			// D3D11 requires our sampler state to be valid (i.e. not point to any of our new RTs)
			//  before we call SetRenderTargets. At this point FNA3D does not have a current copy
			//  of the managed sampler state, so we need to apply our current state now instead of
			//  before our next Clear or Draw operation.
			ApplySamplers();

			// Checking for redundant SetRenderTargets...
			if (renderTargets == null && renderTargetCount == 0)
			{
				return;
			}
			else if (renderTargets != null && renderTargets.Length == renderTargetCount)
			{
				bool isRedundant = true;
				for (int i = 0; i < renderTargets.Length; i += 1)
				{
					if (	renderTargets[i].RenderTarget != renderTargetBindings[i].RenderTarget ||
						renderTargets[i].CubeMapFace != renderTargetBindings[i].CubeMapFace	)
					{
						isRedundant = false;
						break;
					}
				}
				if (isRedundant)
				{
					return;
				}
			}

			int newWidth;
			int newHeight;
			RenderTargetUsage clearTarget;
			if (renderTargets == null || renderTargets.Length == 0)
			{
				FNA3D.FNA3D_SetRenderTargets(
					GLDevice,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					DepthFormat.None,
					(byte) (PresentationParameters.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0) /* lol c# */
				);

				// Set the viewport/scissor to the size of the backbuffer.
				newWidth = PresentationParameters.BackBufferWidth;
				newHeight = PresentationParameters.BackBufferHeight;
				clearTarget = PresentationParameters.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (int i = 0; i < renderTargetCount; i += 1)
				{
					FNA3D.FNA3D_ResolveTarget(GLDevice, ref nativeTargetBindings[i]);
				}
				Array.Clear(renderTargetBindings, 0, renderTargetBindings.Length);
				Array.Clear(nativeTargetBindings, 0, nativeTargetBindings.Length);
				renderTargetCount = 0;
			}
			else
			{
				IRenderTarget target = renderTargets[0].RenderTarget as IRenderTarget;
				unsafe
				{
					fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &nativeTargetBindingsNext[0])
					{
						PrepareRenderTargetBindings(rt, renderTargets);
						FNA3D.FNA3D_SetRenderTargets(
							GLDevice,
							rt,
							renderTargets.Length,
							target.DepthStencilBuffer,
							target.DepthStencilFormat,
							(byte) (target.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0) /* lol c# */
						);
					}
				}

				// Set the viewport/scissor to the size of the first render target.
				newWidth = target.Width;
				newHeight = target.Height;
				clearTarget = target.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (int i = 0; i < renderTargetCount; i += 1)
				{
					// We only need to resolve if the target is no longer bound.
					bool stillBound = false;
					for (int j = 0; j < renderTargets.Length; j += 1)
					{
						if (renderTargetBindings[i].RenderTarget == renderTargets[j].RenderTarget)
						{
							stillBound = true;
							break;
						}
					}
					if (stillBound)
					{
						continue;
					}
					FNA3D.FNA3D_ResolveTarget(GLDevice, ref nativeTargetBindings[i]);
				}
				Array.Clear(renderTargetBindings, 0, renderTargetBindings.Length);
				Array.Copy(renderTargets, renderTargetBindings, renderTargets.Length);
				Array.Clear(nativeTargetBindings, 0, nativeTargetBindings.Length);
				Array.Copy(nativeTargetBindingsNext, nativeTargetBindings, renderTargets.Length);
				renderTargetCount = renderTargets.Length;
			}

			// Apply new GL state, clear target if requested
			Viewport = new Viewport(0, 0, newWidth, newHeight);
			ScissorRectangle = new Rectangle(0, 0, newWidth, newHeight);
			if (clearTarget == RenderTargetUsage.DiscardContents)
			{
				Clear(
					ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
					DiscardColor,
					Viewport.MaxDepth,
					0
				);
			}
		}

		/// <summary>
		/// Returns a new array containing all of the render target(s) currently bound to the device.
		/// </summary>
		public RenderTargetBinding[] GetRenderTargets()
		{
			// Return a correctly sized copy our internal array.
			RenderTargetBinding[] bindings = new RenderTargetBinding[renderTargetCount];
			Array.Copy(renderTargetBindings, bindings, renderTargetCount);
			return bindings;
		}

		/// <summary>
		/// Copies the currently bound render target(s) into an output buffer (if provided), and returns the number of bound render targets.
		/// </summary>
		/// <param name="output">A buffer sized to contain all of the currently bound render targets, or null.</param>
		/// <returns>The number of render targets currently bound.</returns>
		public int GetRenderTargetsNoAllocEXT(RenderTargetBinding[] output)
		{
			if (output == null)
			{
				return renderTargetCount;
			}
			else if (output.Length != renderTargetCount)
			{
				throw new ArgumentException("Output buffer size incorrect");
			}
			Array.Copy(renderTargetBindings, output, renderTargetCount);
			return renderTargetCount;
		}

		#endregion

		#region Public Buffer Object Methods

		public void SetVertexBuffer(VertexBuffer vertexBuffer)
		{
			SetVertexBuffer(vertexBuffer, 0);
		}

		public void SetVertexBuffer(VertexBuffer vertexBuffer, int vertexOffset)
		{
			if (vertexBuffer == null)
			{
				if (vertexBufferCount == 0)
				{
					return;
				}
				for (int i = 0; i < vertexBufferCount; i += 1)
				{
					vertexBufferBindings[i] = VertexBufferBinding.None;
				}
				vertexBufferCount = 0;
				vertexBuffersUpdated = true;
				return;
			}

			if (	!ReferenceEquals(vertexBufferBindings[0].VertexBuffer, vertexBuffer) ||
				vertexBufferBindings[0].VertexOffset != vertexOffset	)
			{
				vertexBufferBindings[0] = new VertexBufferBinding(
					vertexBuffer,
					vertexOffset
				);
				vertexBuffersUpdated = true;
			}

			if (vertexBufferCount > 1)
			{
				for (int i = 1; i < vertexBufferCount; i += 1)
				{
					vertexBufferBindings[i] = VertexBufferBinding.None;
				}
				vertexBuffersUpdated = true;
			}

			vertexBufferCount = 1;
		}

		public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
		{
			if (vertexBuffers == null)
			{
				if (vertexBufferCount == 0)
				{
					return;
				}
				for (int j = 0; j < vertexBufferCount; j += 1)
				{
					vertexBufferBindings[j] = VertexBufferBinding.None;
				}
				vertexBufferCount = 0;
				vertexBuffersUpdated = true;
				return;
			}

			if (vertexBuffers.Length > vertexBufferBindings.Length)
			{
				throw new ArgumentOutOfRangeException(
					"vertexBuffers",
					String.Format(
						"Max Vertex Buffers supported is {0}",
						vertexBufferBindings.Length
					)
				);
			}

			int i = 0;
			while (i < vertexBuffers.Length)
			{
				if (	!ReferenceEquals(vertexBufferBindings[i].VertexBuffer, vertexBuffers[i].VertexBuffer) ||
					vertexBufferBindings[i].VertexOffset != vertexBuffers[i].VertexOffset ||
					vertexBufferBindings[i].InstanceFrequency != vertexBuffers[i].InstanceFrequency	)
				{
					vertexBufferBindings[i] = vertexBuffers[i];
					vertexBuffersUpdated = true;
				}
				i += 1;
			}
			if (vertexBuffers.Length < vertexBufferCount)
			{
				while (i < vertexBufferCount)
				{
					vertexBufferBindings[i] = VertexBufferBinding.None;
					i += 1;
				}
				vertexBuffersUpdated = true;
			}

			vertexBufferCount = vertexBuffers.Length;
		}

		public VertexBufferBinding[] GetVertexBuffers()
		{
			VertexBufferBinding[] result = new VertexBufferBinding[vertexBufferCount];
			Array.Copy(
				vertexBufferBindings,
				result,
				vertexBufferCount
			);
			return result;
		}

		#endregion

		#region DrawPrimitives: VertexBuffer, IndexBuffer

		/// <summary>
		/// Draw geometry by indexing into the vertex buffer.
		/// </summary>
		/// <param name="primitiveType">
		/// The type of primitives in the index buffer.
		/// </param>
		/// <param name="baseVertex">
		/// Used to offset the vertex range indexed from the vertex buffer.
		/// </param>
		/// <param name="minVertexIndex">
		/// A hint of the lowest vertex indexed relative to baseVertex.
		/// </param>
		/// <param name="numVertices">
		/// A hint of the maximum vertex indexed.
		/// </param>
		/// <param name="startIndex">
		/// The index within the index buffer to start drawing from.
		/// </param>
		/// <param name="primitiveCount">
		/// The number of primitives to render from the index buffer.
		/// </param>
		public void DrawIndexedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount
		) {
			ApplyState();

			PrepareVertexBindingArray(baseVertex);

			FNA3D.FNA3D_DrawIndexedPrimitives(
				GLDevice,
				primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				Indices.buffer,
				Indices.IndexElementSize
			);
		}

		public void DrawInstancedPrimitives(
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount
		) {
			// If this device doesn't have the support, just explode now before it's too late.
			if (FNA3D.FNA3D_SupportsHardwareInstancing(GLDevice) == 0)
			{
				throw new NoSuitableGraphicsDeviceException("Your hardware does not support hardware instancing!");
			}

			ApplyState();

			PrepareVertexBindingArray(baseVertex);

			FNA3D.FNA3D_DrawInstancedPrimitives(
				GLDevice,
				primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				instanceCount,
				Indices.buffer,
				Indices.IndexElementSize
			);
		}

		#endregion

		#region DrawPrimitives: VertexBuffer, No Indices

		public void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount
		) {
			ApplyState();

			PrepareVertexBindingArray(0);

			FNA3D.FNA3D_DrawPrimitives(
				GLDevice,
				primitiveType,
				vertexStart,
				primitiveCount
			);
		}

		#endregion

		#region DrawPrimitives: Vertex Arrays, Index Arrays

		public void DrawUserIndexedPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int numVertices,
			short[] indexData,
			int indexOffset,
			int primitiveCount
		) where T : struct, IVertexType {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
			GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				numVertices,
				vertexOffset,
				VertexDeclarationCache<T>.VertexDeclaration
			);
			PrepareUserIndexBuffer(
				ibHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				indexOffset,
				2
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();

			FNA3D.FNA3D_DrawIndexedPrimitives(
				GLDevice,
				primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				userIndexBuffer,
				IndexElementSize.SixteenBits
			);
		}

		public void DrawUserIndexedPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int numVertices,
			short[] indexData,
			int indexOffset,
			int primitiveCount,
			VertexDeclaration vertexDeclaration
		) where T : struct {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
			GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				numVertices,
				vertexOffset,
				vertexDeclaration
			);
			PrepareUserIndexBuffer(
				ibHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				indexOffset,
				2
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();

			FNA3D.FNA3D_DrawIndexedPrimitives(
				GLDevice,
				primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				userIndexBuffer,
				IndexElementSize.SixteenBits
			);
		}

		public void DrawUserIndexedPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int numVertices,
			int[] indexData,
			int indexOffset,
			int primitiveCount
		) where T : struct, IVertexType {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
			GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				numVertices,
				vertexOffset,
				VertexDeclarationCache<T>.VertexDeclaration
			);
			PrepareUserIndexBuffer(
				ibHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				indexOffset,
				4
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();

			FNA3D.FNA3D_DrawIndexedPrimitives(
				GLDevice,
				primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				userIndexBuffer,
				IndexElementSize.ThirtyTwoBits
			);
		}

		public void DrawUserIndexedPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int numVertices,
			int[] indexData,
			int indexOffset,
			int primitiveCount,
			VertexDeclaration vertexDeclaration
		) where T : struct {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
			GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				numVertices,
				vertexOffset,
				vertexDeclaration
			);
			PrepareUserIndexBuffer(
				ibHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				indexOffset,
				4
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();

			FNA3D.FNA3D_DrawIndexedPrimitives(
				GLDevice,
				primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				userIndexBuffer,
				IndexElementSize.ThirtyTwoBits
			);
		}

		#endregion

		#region DrawPrimitives: Vertex Arrays, No Indices

		public void DrawUserPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int primitiveCount
		) where T : struct, IVertexType {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				vertexOffset,
				VertexDeclarationCache<T>.VertexDeclaration
			);

			// Release the handles.
			vbHandle.Free();

			FNA3D.FNA3D_DrawPrimitives(
				GLDevice,
				primitiveType,
				0,
				primitiveCount
			);
		}

		public void DrawUserPrimitives<T>(
			PrimitiveType primitiveType,
			T[] vertexData,
			int vertexOffset,
			int primitiveCount,
			VertexDeclaration vertexDeclaration
		) where T : struct {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

			PrepareUserVertexBuffer(
				vbHandle.AddrOfPinnedObject(),
				PrimitiveVerts(primitiveType, primitiveCount),
				vertexOffset,
				vertexDeclaration
			);

			// Release the handles.
			vbHandle.Free();

			FNA3D.FNA3D_DrawPrimitives(
				GLDevice,
				primitiveType,
				0,
				primitiveCount
			);
		}

		#endregion

		#region FNA Extensions

		public void SetStringMarkerEXT(string text)
		{
			FNA3D.FNA3D_SetStringMarker(GLDevice, text);
		}

		#endregion

		#region Private State Flush Methods

		private void ApplyState()
		{
			// Update Blend/DepthStencil, if applicable
			if (currentBlend != nextBlend)
			{
				FNA3D.FNA3D_SetBlendState(
					GLDevice,
					ref nextBlend.state
				);
				currentBlend = nextBlend;
			}
			if (currentDepthStencil != nextDepthStencil)
			{
				FNA3D.FNA3D_SetDepthStencilState(
					GLDevice,
					ref nextDepthStencil.state
				);
				currentDepthStencil = nextDepthStencil;
			}

			// Always update RasterizerState, as it depends on other device states
			FNA3D.FNA3D_ApplyRasterizerState(
				GLDevice,
				ref RasterizerState.state
			);

			ApplySamplers();
		}

		private void ApplySamplers()
		{
			for (int sampler = 0; sampler < modifiedSamplers.Length; sampler += 1)
			{
				if (!modifiedSamplers[sampler])
				{
					continue;
				}

				modifiedSamplers[sampler] = false;

				FNA3D.FNA3D_VerifySampler(
					GLDevice,
					sampler,
					(Textures[sampler] != null) ?
						Textures[sampler].texture :
						IntPtr.Zero,
					ref SamplerStates[sampler].state
				);
			}

			for (int sampler = 0; sampler < modifiedVertexSamplers.Length; sampler += 1) 
			{
				if (!modifiedVertexSamplers[sampler])
				{
					continue;
				}

				modifiedVertexSamplers[sampler] = false;

				/* Believe it or not, this is actually how VertexTextures are
				 * stored in XNA4! Their D3D9 renderer just uses the last 4
				 * slots available in the device's sampler array. So that's what
				 * we get to do.
				 * -flibit
				 */
				FNA3D.FNA3D_VerifyVertexSampler(
					GLDevice,
					sampler,
					(VertexTextures[sampler] != null) ?
						VertexTextures[sampler].texture :
						IntPtr.Zero,
					ref VertexSamplerStates[sampler].state
				);
			}
		}

		private unsafe void PrepareVertexBindingArray(int baseVertex)
		{
			fixed (FNA3D.FNA3D_VertexBufferBinding* b = &nativeBufferBindings[0])
			{
				for (int i = 0; i < vertexBufferCount; i += 1)
				{
					VertexBuffer buffer = vertexBufferBindings[i].VertexBuffer;
					b[i].vertexBuffer = buffer.buffer;
					b[i].vertexDeclaration.vertexStride = buffer.VertexDeclaration.VertexStride;
					b[i].vertexDeclaration.elementCount = buffer.VertexDeclaration.elements.Length;
					b[i].vertexDeclaration.elements = buffer.VertexDeclaration.elementsPin;
					b[i].vertexOffset = vertexBufferBindings[i].VertexOffset;
					b[i].instanceFrequency = vertexBufferBindings[i].InstanceFrequency;
				}
				FNA3D.FNA3D_ApplyVertexBufferBindings(
					GLDevice,
					b,
					vertexBufferCount,
					(byte) (vertexBuffersUpdated ? 1 : 0),
					baseVertex
				);
			}
			vertexBuffersUpdated = false;
		}

		private unsafe void PrepareUserVertexBuffer(
			IntPtr vertexData,
			int numVertices,
			int vertexOffset,
			VertexDeclaration vertexDeclaration
		) {
			int len = numVertices * vertexDeclaration.VertexStride;
			int offset = vertexOffset * vertexDeclaration.VertexStride;
			vertexDeclaration.GraphicsDevice = this;

			if (len > userVertexBufferSize)
			{
				if (userVertexBuffer != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeVertexBuffer(
						GLDevice,
						userVertexBuffer
					);
				}

				userVertexBuffer = FNA3D.FNA3D_GenVertexBuffer(
					GLDevice,
					1,
					BufferUsage.WriteOnly,
					len
				);
				userVertexBufferSize = len;
			}

			FNA3D.FNA3D_SetVertexBufferData(
				GLDevice,
				userVertexBuffer,
				0,
				vertexData + offset,
				len,
				1,
				1,
				SetDataOptions.Discard
			);

			fixed (FNA3D.FNA3D_VertexBufferBinding* b = &nativeBufferBindings[0])
			{
				b->vertexBuffer = userVertexBuffer;
				b->vertexDeclaration.vertexStride = vertexDeclaration.VertexStride;
				b->vertexDeclaration.elementCount = vertexDeclaration.elements.Length;
				b->vertexDeclaration.elements = vertexDeclaration.elementsPin;
				b->vertexOffset = 0;
				b->instanceFrequency = 0;
				FNA3D.FNA3D_ApplyVertexBufferBindings(GLDevice, b, 1, 1, 0);
			}
			vertexBuffersUpdated = true;
		}

		private void PrepareUserIndexBuffer(
			IntPtr indexData,
			int numIndices,
			int indexOffset,
			int indexElementSizeInBytes
		) {
			int len = numIndices * indexElementSizeInBytes;
			if (len > userIndexBufferSize)
			{
				if (userIndexBuffer != IntPtr.Zero)
				{
					FNA3D.FNA3D_AddDisposeIndexBuffer(
						GLDevice,
						userIndexBuffer
					);
				}

				userIndexBuffer = FNA3D.FNA3D_GenIndexBuffer(
					GLDevice,
					1,
					BufferUsage.WriteOnly,
					len
				);
				userIndexBufferSize = len;
			}

			FNA3D.FNA3D_SetIndexBufferData(
				GLDevice,
				userIndexBuffer,
				0,
				indexData + (indexOffset * indexElementSizeInBytes),
				len,
				SetDataOptions.Discard
			);
		}

		/* Needed by VideoPlayer */
		internal static unsafe void PrepareRenderTargetBindings(
			FNA3D.FNA3D_RenderTargetBinding *b,
			RenderTargetBinding[] bindings
		) {
			for (int i = 0; i < bindings.Length; i += 1, b += 1)
			{
				Texture texture = bindings[i].RenderTarget;
				IRenderTarget rt = texture as IRenderTarget;
				if (texture is RenderTargetCube)
				{
					b->type = 1;
					b->data1 = rt.Width;
					b->data2 = (int) bindings[i].CubeMapFace;
				}
				else
				{
					b->type = 0;
					b->data1 = rt.Width;
					b->data2 = rt.Height;
				}
				b->levelCount = rt.LevelCount;
				b->multiSampleCount = rt.MultiSampleCount;
				b->texture = texture.texture;
				b->colorBuffer = rt.ColorBuffer;
			}
		}

		#endregion

		#region Private Static Methods

		private static int PrimitiveVerts(
			PrimitiveType primitiveType,
			int primitiveCount
		) {
			switch (primitiveType)
			{
				case PrimitiveType.TriangleList:
					return primitiveCount * 3;
				case PrimitiveType.TriangleStrip:
					return primitiveCount + 2;
				case PrimitiveType.LineList:
					return primitiveCount * 2;
				case PrimitiveType.LineStrip:
					return primitiveCount + 1;
				case PrimitiveType.PointListEXT:
					return primitiveCount;
				default:
					throw new InvalidOperationException(
						"Unrecognized primitive type!"
					);
			}
		}

		#endregion
	}
}
