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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public struct VertexBufferBinding
	{
		#region Public Properties

		/// <summary>
		/// Gets the instance frequency. A value of 0 means no instancing.
		/// </summary>
		public int InstanceFrequency
		{
			get
			{
				return instanceFrequency;
			}
		}

		/// <summary>
		/// Gets the vertex buffer.
		/// </summary>
		public VertexBuffer VertexBuffer
		{
			get
			{
				return vertexBuffer;
			}
		}

		/// <summary>
		/// Gets the offset in bytes from the beginning of the vertex buffer to the first vertex to use.
		/// </summary>
		public int VertexOffset
		{
			get
			{
				return vertexOffset;
			}
		}

		#endregion

		#region Private Variables

		private VertexBuffer vertexBuffer;
		private int vertexOffset;
		private int instanceFrequency;

		#endregion

		#region Internal Static Variables

		/// <summary>
		/// A null vertex buffer binding for unused vertex buffer slots.
		/// </summary>
		internal static readonly VertexBufferBinding None = new VertexBufferBinding();

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates an instance of VertexBufferBinding.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer to bind.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer)
		{
			if (vertexBuffer == null)
			{
				throw new ArgumentNullException("vertexBuffer", "This method does not accept null for this parameter.");
			}
			this.vertexBuffer = vertexBuffer;
			vertexOffset = 0;
			instanceFrequency = 0;
		}

		/// <summary>
		/// Creates an instance of VertexBufferBinding.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer to bind.</param>
		/// <param name="vertexOffset">The offset in bytes to the first vertex to use.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset)
		{
			if (vertexBuffer == null)
			{
				throw new ArgumentNullException("vertexBuffer", "This method does not accept null for this parameter.");
			}
			if (unchecked((uint) vertexOffset >= (uint) vertexBuffer.VertexCount))
			{
				throw new ArgumentOutOfRangeException("vertexOffset");
			}
			this.vertexBuffer = vertexBuffer;
			this.vertexOffset = vertexOffset;
			instanceFrequency = 0;
		}

		/// <summary>
		/// Creates an instance of VertexBufferBinding.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer to bind.</param>
		/// <param name="vertexOffset">The offset in bytes to the first vertex to use.</param>
		/// <param name="instanceFrequency">Number of instances to draw for each draw call. Use 0 if not using instanced drawing.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset, int instanceFrequency)
		{
			if (vertexBuffer == null)
			{
				throw new ArgumentNullException("vertexBuffer", "This method does not accept null for this parameter.");
			}
			if (unchecked((uint) vertexOffset >= (uint) vertexBuffer.VertexCount))
			{
				throw new ArgumentOutOfRangeException("vertexOffset");
			}
			if (instanceFrequency < 0)
			{
				throw new ArgumentOutOfRangeException("instanceFrequency");
			}
			this.vertexBuffer = vertexBuffer;
			this.vertexOffset = vertexOffset;
			this.instanceFrequency = instanceFrequency;
		}

		#endregion

		#region Implicit Operators

		public static implicit operator VertexBufferBinding(VertexBuffer buffer)
		{
			/* Unlike the constructors, we have to be ready for buffer to be null
			 * due to weird left-hand assignment shenanigans. The one that tripped
			 * me was SetVertexBuffer calling this for some odd reason.
			 * -flibit
			 */
			VertexBufferBinding result;
			result.vertexBuffer = buffer;
			result.vertexOffset = 0;
			result.instanceFrequency = 0;
			return result;
		}

		#endregion
	}
}
