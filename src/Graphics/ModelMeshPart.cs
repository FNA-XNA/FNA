#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class ModelMeshPart
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the material Effect for this mesh part.
		/// </summary>
		public Effect Effect
		{
			get
			{
				return INTERNAL_effect;
			}
			set
			{
				if (value == INTERNAL_effect)
				{
					return;
				}

				if (INTERNAL_effect != null)
				{
					// First check to see any other parts are also using this effect.
					bool removeEffect = true;
					foreach (ModelMeshPart part in parent.MeshParts)
					{
						if (part != this && part.INTERNAL_effect == INTERNAL_effect)
						{
							removeEffect = false;
							break;
						}
					}

					if (removeEffect)
					{
						parent.Effects.Remove(INTERNAL_effect);
					}
				}

				// Set the new effect.
				INTERNAL_effect = value;
				parent.Effects.Add(value);
			}
		}

		/// <summary>
		/// Gets the index buffer for this mesh part.
		/// </summary>
		public IndexBuffer IndexBuffer
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the number of vertices used during a draw call.
		/// </summary>
		public int NumVertices
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the number of primitives to render.
		/// </summary>
		public int PrimitiveCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the location in the index array at which to start reading vertices.
		/// </summary>
		public int StartIndex
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets or sets an object identifying this model mesh part.
		/// </summary>
		public object Tag
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the vertex buffer for this mesh part.
		/// </summary>
		public VertexBuffer VertexBuffer
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the offset (in vertices) from the top of vertex buffer.
		/// </summary>
		public int VertexOffset
		{
			get;
			internal set;
		}

		#endregion

		#region Internal Variables

		internal ModelMesh parent;

		#endregion

		#region Private Variables

		private Effect INTERNAL_effect;

		#endregion
	}
}
