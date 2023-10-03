using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics {
    internal struct GraphicsResourceHandles
	{
		public IntPtr effect;
		public IntPtr indexBuffer;
		public IntPtr query;
		public IntPtr renderbuffer1, renderbuffer2;
		public IntPtr texture;
		public IntPtr vertexBuffer;
		// Untyped data that needs to be released using Free (i.e. Effect.stateChangesPtr)
		public IntPtr mallocedPointer;

		public void Dispose (GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}

			if (effect != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeEffect(
					device.GLDevice,
					effect
				);
			}

			if (indexBuffer != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeIndexBuffer(
					device.GLDevice,
					indexBuffer
				);
			}

			if (query != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeQuery(
					device.GLDevice,
					query
				);
			}

			if (renderbuffer1 != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeRenderbuffer(
					device.GLDevice,
					renderbuffer1
				);
			}

			if (renderbuffer2 != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeRenderbuffer(
					device.GLDevice,
					renderbuffer2
				);
			}

			if (texture != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeTexture(
					device.GLDevice,
					texture
				);
			}

			if (vertexBuffer != IntPtr.Zero)
			{
				FNA3D.FNA3D_AddDisposeVertexBuffer(
					device.GLDevice,
					vertexBuffer
				);
			}

			if (mallocedPointer != IntPtr.Zero)
			{
				FNAPlatform.Free(mallocedPointer);
			}

			// Zero out all our fields to prevent a double-free
			this = default;
		}
    }
}
