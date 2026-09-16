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
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexDeclaration : GraphicsResource
	{
		#region Public Properties

		public int VertexStride
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		internal VertexElement[] elements;
		internal IntPtr elementsPin;

		internal protected override bool IsHarmlessToLeakInstance
		{
			get
			{
				return true;
			}
		}

		#endregion

		#region Private Variables

		private GCHandle handle;

		#endregion

		#region Public Constructors

		public VertexDeclaration(
			params VertexElement[] elements
		) : this(GetVertexStride(elements), elements) {
		}

		public VertexDeclaration(
			int vertexStride,
			params VertexElement[] elements
		) {
			if ((elements == null) || (elements.Length == 0))
			{
				throw new ArgumentNullException("elements", "Elements cannot be empty");
			}

			this.elements = (VertexElement[]) elements.Clone();
			handle = GCHandle.Alloc(this.elements, GCHandleType.Pinned);
			elementsPin = handle.AddrOfPinnedObject();
			VertexStride = vertexStride;
		}

		#endregion

		#region Protected Dispose Method

		protected override void Dispose(bool disposing)
		{
			handle.Free();
			base.Dispose(disposing);
		}

		#endregion

		#region Public Methods

		public VertexElement[] GetVertexElements()
		{
			return (VertexElement[]) elements.Clone();
		}

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Returns the VertexDeclaration for Type.
		/// </summary>
		/// <param name="vertexType">A value type which implements the IVertexType interface.</param>
		/// <returns>The VertexDeclaration.</returns>
		/// <remarks>
		/// Prefer to use VertexDeclarationCache when the declaration lookup
		/// can be performed with a templated type.
		/// </remarks>
		internal static VertexDeclaration FromType(Type vertexType)
		{
			if (ReferenceEquals(vertexType, null))
			{
				throw new ArgumentNullException("vertexType", "This method does not accept null for this parameter.");
			}

			if (!vertexType.IsValueType)
			{
				throw new ArgumentException("Invalid vertex type. " + vertexType + " is not a value type.");
			}

			IVertexType type = Activator.CreateInstance(vertexType) as IVertexType;
			if (type == null)
			{
				throw new ArgumentException("Invalid vertex type. " + vertexType + " does not implement the IVertexType interface.");
			}

			VertexDeclaration vertexDeclaration = type.VertexDeclaration;
			if (vertexDeclaration == null)
			{
				throw new InvalidOperationException("Invalid vertex type. " + vertexType + " returned a null VertexDeclaration.");
			}
			if (Marshal.SizeOf(vertexType) != vertexDeclaration.VertexStride)
			{
				throw new InvalidOperationException("Invalid vertex type. The size of " + vertexType + " does not match the stride of its vertex declaration.");
			}
			return vertexDeclaration;
		}

		#endregion

		#region Private Static VertexElement Methods

		private static int GetVertexStride(VertexElement[] elements)
		{
			int max = 0;

			for (int i = 0; i < elements.Length; i += 1)
			{
				int start = elements[i].Offset + GetTypeSize(elements[i].VertexElementFormat);
				if (max < start)
				{
					max = start;
				}
			}

			return max;
		}

		private static int GetTypeSize(VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}
			return 0;
		}

		#endregion
	}
}
