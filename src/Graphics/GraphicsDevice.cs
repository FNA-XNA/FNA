#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
					return new DisplayMode(
						GLDevice.Backbuffer.Width,
						GLDevice.Backbuffer.Height,
						SurfaceFormat.Color
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
				GLDevice.SetScissorRect(
					value,
					RenderTargetCount > 0
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
				GLDevice.SetViewport(
					value,
					RenderTargetCount > 0
				);
			}
		}

		public Color BlendFactor
		{
			get
			{
				return GLDevice.BlendFactor;
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				GLDevice.BlendFactor = value;
			}
		}

		public int MultiSampleMask
		{
			get
			{
				return GLDevice.MultiSampleMask;
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				GLDevice.MultiSampleMask = value;
			}
		}

		public int ReferenceStencil
		{
			get
			{
				return GLDevice.ReferenceStencil;
			}
			set
			{
				/* FIXME: Does this affect the value found in
				 * DepthStencilState?
				 * -flibit
				 */
				GLDevice.ReferenceStencil = value;
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

		#region Internal RenderTarget Properties

		internal int RenderTargetCount
		{
			get;
			private set;
		}

		#endregion

		#region Internal GL Device

		internal readonly IGLDevice GLDevice;

		#endregion

		#region Private State Shadowing Variables

		private BlendState currentBlend;
		private BlendState nextBlend;
		private DepthStencilState currentDepthStencil;
		private DepthStencilState nextDepthStencil;

		#endregion

		#region Private Vertex Sampler Offset Variable

		private int vertexSamplerStart;

		#endregion

		#region Internal Sampler Change Queue

		private readonly Queue<int> modifiedSamplers = new Queue<int>();
		private readonly Queue<int> modifiedVertexSamplers = new Queue<int>();

		#endregion

		#region Private Disposal Variables

		/* Use WeakReference for the global resources list as we do not
		 * know when a resource may be disposed and collected. We do not
		 * want to prevent a resource from being collected by holding a
		 * strong reference to it in this list.
		 */
		private readonly List<WeakReference> resources = new List<WeakReference>();
		private readonly object resourcesLock = new object();

		#endregion

		#region Private Clear Variables

		/* On Intel Integrated graphics, there is a fast hw unit for doing
		 * clears to colors where all components are either 0 or 255.
		 * Despite XNA4 using Purple here, we use black (in Release) to avoid
		 * performance warnings on Intel/Mesa.
		 * -sulix
		 */
#if DEBUG
		private static readonly Vector4 DiscardColor = new Color(68, 34, 136, 255).ToVector4();
#else
		private static readonly Vector4 DiscardColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
#endif

		#endregion

		#region Private RenderTarget Variables

		// 4, per XNA4 HiDef spec
		private readonly RenderTargetBinding[] renderTargetBindings = new RenderTargetBinding[4];

		#endregion

		#region Private Buffer Object Variables

		// 16, per XNA4 HiDef spec
		private VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[16];
		private int vertexBufferCount = 0;
		private bool vertexBuffersUpdated = false;

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
		internal void OnResourceCreated()
		{
			if (ResourceCreated != null)
			{
				ResourceCreated(this, (ResourceCreatedEventArgs) EventArgs.Empty);
			}
		}

		// TODO: Hook this up to GraphicsResource
		internal void OnResourceDestroyed()
		{
			if (ResourceDestroyed != null)
			{
				ResourceDestroyed(this, (ResourceDestroyedEventArgs) EventArgs.Empty);
			}
		}

		#endregion

		#region Constructor, Deconstructor, Dispose Methods

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

			// Set up the OpenGL Device. Loads entry points.
			GLDevice = new OpenGLDevice(PresentationParameters);

			// Force set the default render states.
			BlendState = BlendState.Opaque;
			DepthStencilState = DepthStencilState.Default;
			RasterizerState = RasterizerState.CullCounterClockwise;

			// Initialize the Texture/Sampler state containers
			int maxTextures = Math.Min(GLDevice.MaxTextureSlots, 16); // Per XNA4 spec
			int maxVertexTextures = MathHelper.Clamp(GLDevice.MaxTextureSlots - 16, 0, 4); // Per XNA4 HiDef spec
			vertexSamplerStart = GLDevice.MaxTextureSlots - maxVertexTextures;
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
						foreach (WeakReference resource in resources.ToArray())
						{
							object target = resource.Target;
							if (target != null)
							{
								(target as IDisposable).Dispose();
							}
						}
						resources.Clear();
					}

					// Dispose of the GL Device/Context
					GLDevice.Dispose();
				}

				IsDisposed = true;
			}
		}

		#endregion

		#region Internal Resource Management Methods

		internal void AddResourceReference(WeakReference resourceReference)
		{
			lock (resourcesLock)
			{
				resources.Add(resourceReference);
			}
		}

		internal void RemoveResourceReference(WeakReference resourceReference)
		{
			lock (resourcesLock)
			{
				resources.Remove(resourceReference);
			}
		}

		#endregion

		#region Public Present Method

		public void Present()
		{
			GLDevice.SwapBuffers(
				null,
				null,
				PresentationParameters.DeviceWindowHandle
			);
		}

		public void Present(
			Rectangle? sourceRectangle,
			Rectangle? destinationRectangle,
			IntPtr overrideWindowHandle
		) {
			GLDevice.SwapBuffers(
				sourceRectangle,
				destinationRectangle,
				overrideWindowHandle
			);
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

			// Set the new PresentationParameters first.
			PresentationParameters = presentationParameters;
			PresentationParameters.MultiSampleCount = Math.Min(
				MathHelper.ClosestMSAAPower(
					PresentationParameters.MultiSampleCount
				),
				GLDevice.MaxMultiSampleCount
			);

			/* Reset the backbuffer first, before doing anything else.
			 * The GLDevice needs to know what we're up to right away.
			 * -flibit
			 */
			GLDevice.ResetBackbuffer(
				PresentationParameters,
				RenderTargetCount > 0
			);

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

			// FIXME: This should probably mean something. -flibit
			Adapter = graphicsAdapter;

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
			if (RenderTargetCount == 0)
			{
				/* FIXME: PresentationParameters.DepthStencilFormat is probably
				 * a more accurate value here, but the Backbuffer may disagree.
				 * -flibit
				 */
				dsFormat = GLDevice.Backbuffer.DepthFormat;
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
			GLDevice.Clear(
				options,
				color,
				depth,
				stencil
			);
		}

		#endregion

		#region Public Backbuffer Methods

		public void GetBackBufferData<T>(T[] data) where T : struct
		{
			GLDevice.ReadBackbuffer(data, 0, data.Length, null);
		}

		public void GetBackBufferData<T>(
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GLDevice.ReadBackbuffer(data, startIndex, elementCount, null);
		}

		public void GetBackBufferData<T>(
			Rectangle? rect,
			T[] data,
			int startIndex,
			int elementCount
		) where T : struct {
			GLDevice.ReadBackbuffer(data, startIndex, elementCount, rect);
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
				SetRenderTargets(new RenderTargetBinding(renderTarget));
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
				SetRenderTargets(new RenderTargetBinding(renderTarget, cubeMapFace));
			}
		}

		public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
		{
			// Checking for redundant SetRenderTargets...
			if (renderTargets == null && RenderTargetCount == 0)
			{
				return;
			}
			else if (renderTargets != null && renderTargets.Length == RenderTargetCount)
			{
				bool isRedundant = true;
				for (int i = 0; i < renderTargets.Length; i += 1)
				{
					if (	renderTargets[i].RenderTarget != renderTargetBindings[i].RenderTarget ||
						renderTargets[i].CubeMapFace != renderTargetBindings[i].CubeMapFace	)
					{
						isRedundant = false;
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
				GLDevice.SetRenderTargets(null, null, DepthFormat.None);

				// Set the viewport/scissor to the size of the backbuffer.
				newWidth = PresentationParameters.BackBufferWidth;
				newHeight = PresentationParameters.BackBufferHeight;
				clearTarget = PresentationParameters.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (int i = 0; i < RenderTargetCount; i += 1)
				{
					GLDevice.ResolveTarget(renderTargetBindings[i]);
				}
				Array.Clear(renderTargetBindings, 0, renderTargetBindings.Length);
				RenderTargetCount = 0;
			}
			else
			{
				IRenderTarget target = renderTargets[0].RenderTarget as IRenderTarget;
				GLDevice.SetRenderTargets(
					renderTargets,
					target.DepthStencilBuffer,
					target.DepthStencilFormat
				);

				// Set the viewport/scissor to the size of the first render target.
				newWidth = target.Width;
				newHeight = target.Height;
				clearTarget = target.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (int i = 0; i < RenderTargetCount; i += 1)
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
					GLDevice.ResolveTarget(renderTargetBindings[i]);
				}
				Array.Clear(renderTargetBindings, 0, renderTargetBindings.Length);
				Array.Copy(renderTargets, renderTargetBindings, renderTargets.Length);
				RenderTargetCount = renderTargets.Length;
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

		public RenderTargetBinding[] GetRenderTargets()
		{
			// Return a correctly sized copy our internal array.
			RenderTargetBinding[] bindings = new RenderTargetBinding[RenderTargetCount];
			Array.Copy(renderTargetBindings, bindings, RenderTargetCount);
			return bindings;
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

			// Set up the vertex buffers
			GLDevice.ApplyVertexAttributes(
				vertexBufferBindings,
				vertexBufferCount,
				vertexBuffersUpdated,
				baseVertex
			);
			vertexBuffersUpdated = false;

			GLDevice.DrawIndexedPrimitives(
				primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				Indices
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
			if (!GLDevice.SupportsHardwareInstancing)
			{
				throw new NoSuitableGraphicsDeviceException("Your hardware does not support hardware instancing!");
			}

			ApplyState();

			// Set up the vertex buffers
			GLDevice.ApplyVertexAttributes(
				vertexBufferBindings,
				vertexBufferCount,
				vertexBuffersUpdated,
				baseVertex
			);
			vertexBuffersUpdated = false;

			GLDevice.DrawInstancedPrimitives(
				primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				instanceCount,
				Indices
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

			// Set up the vertex buffers
			GLDevice.ApplyVertexAttributes(
				vertexBufferBindings,
				vertexBufferCount,
				vertexBuffersUpdated,
				0
			);
			vertexBuffersUpdated = false;

			GLDevice.DrawPrimitives(
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
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();
			IntPtr ibPtr = ibHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				vertexOffset
			);

			GLDevice.DrawUserIndexedPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				numVertices,
				ibPtr,
				indexOffset,
				IndexElementSize.SixteenBits,
				primitiveCount
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();
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
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();
			IntPtr ibPtr = ibHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				vertexOffset
			);

			GLDevice.DrawUserIndexedPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				numVertices,
				ibPtr,
				indexOffset,
				IndexElementSize.SixteenBits,
				primitiveCount
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();
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
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();
			IntPtr ibPtr = ibHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				vertexOffset
			);

			GLDevice.DrawUserIndexedPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				numVertices,
				ibPtr,
				indexOffset,
				IndexElementSize.ThirtyTwoBits,
				primitiveCount
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();
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
		) where T : struct, IVertexType {
			ApplyState();

			// Pin the buffers.
			GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
			GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();
			IntPtr ibPtr = ibHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				vertexOffset
			);

			GLDevice.DrawUserIndexedPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				numVertices,
				ibPtr,
				indexOffset,
				IndexElementSize.ThirtyTwoBits,
				primitiveCount
			);

			// Release the handles.
			ibHandle.Free();
			vbHandle.Free();
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
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			VertexDeclaration vertexDeclaration = VertexDeclarationCache<T>.VertexDeclaration;
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				0
			);

			GLDevice.DrawUserPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				primitiveCount
			);

			// Release the handles.
			vbHandle.Free();
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
			IntPtr vbPtr = vbHandle.AddrOfPinnedObject();

			// Setup the vertex declaration to point at the vertex data.
			vertexDeclaration.GraphicsDevice = this;
			GLDevice.ApplyVertexAttributes(
				vertexDeclaration,
				vbPtr,
				0
			);

			GLDevice.DrawUserPrimitives(
				primitiveType,
				vbPtr,
				vertexOffset,
				primitiveCount
			);

			// Release the handles.
			vbHandle.Free();
		}

		#endregion

		#region FNA Extensions

		public void SetStringMarkerEXT(string text)
		{
			GLDevice.SetStringMarker(text);
		}

		#endregion

		#region Private State Flush Methods

		private void ApplyState()
		{
			// Update Blend/DepthStencil, if applicable
			if (currentBlend != nextBlend)
			{
				GLDevice.SetBlendState(nextBlend);
				currentBlend = nextBlend;
			}
			if (currentDepthStencil != nextDepthStencil)
			{
				GLDevice.SetDepthStencilState(nextDepthStencil);
				currentDepthStencil = nextDepthStencil;
			}

			// Always update RasterizerState, as it depends on other device states
			GLDevice.ApplyRasterizerState(
				RasterizerState,
				RenderTargetCount > 0
			);

			while (modifiedSamplers.Count > 0)
			{
				int sampler = modifiedSamplers.Dequeue();
				GLDevice.VerifySampler(
					sampler,
					Textures[sampler],
					SamplerStates[sampler]
				);
			}
			while (modifiedVertexSamplers.Count > 0)
			{
				/* Believe it or not, this is actually how VertexTextures are
				 * stored in XNA4! Their D3D9 renderer just uses the last 4
				 * slots available in the device's sampler array. So that's what
				 * we get to do.
				 * -flibit
				 */
				int sampler = modifiedVertexSamplers.Dequeue();
				GLDevice.VerifySampler(
					vertexSamplerStart + sampler,
					VertexTextures[sampler],
					VertexSamplerStates[sampler]
				);
			}
		}

		#endregion
	}
}
