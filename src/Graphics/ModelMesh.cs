#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Represents a mesh that is part of a Model.
	/// </summary>
	public sealed class ModelMesh
	{
		#region Public Properties

		/// <summary>
		/// Gets the BoundingSphere that contains this mesh.
		/// </summary>
		public BoundingSphere BoundingSphere
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a collection of effects associated with this mesh.
		/// </summary>
		public ModelEffectCollection Effects
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ModelMeshPart objects that make up this mesh. Each part of a mesh
		/// is composed of a set of primitives that share the same material.
		/// </summary>
		public ModelMeshPartCollection MeshParts
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of this mesh.
		/// </summary>
		public string Name
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the parent bone for this mesh. The parent bone of a mesh contains a
		/// transformation matrix that describes how the mesh is located relative to
		/// any parent meshes in a model.
		/// </summary>
		public ModelBone ParentBone
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets or sets an object identifying this mesh.
		/// </summary>
		public object Tag
		{
			get;
			set;
		}

		#endregion

		#region Private Variables

		private GraphicsDevice graphicsDevice;

		#endregion

		#region Public Constructors

		public ModelMesh(GraphicsDevice graphicsDevice, List<ModelMeshPart> parts)
		{
			// TODO: Complete member initialization
			this.graphicsDevice = graphicsDevice;

			MeshParts = new ModelMeshPartCollection(parts);

			foreach (ModelMeshPart part in parts)
			{
				part.parent = this;
			}

			Effects = new ModelEffectCollection();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Draws all of the ModelMeshPart objects in this mesh, using their current
		/// Effect settings.
		/// </summary>
		public void Draw()
		{
			foreach (ModelMeshPart part in MeshParts)
			{
				Effect effect = part.Effect;

				if (part.PrimitiveCount > 0)
				{
					graphicsDevice.SetVertexBuffer(part.VertexBuffer);
					graphicsDevice.Indices = part.IndexBuffer;

					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						graphicsDevice.DrawIndexedPrimitives(
							PrimitiveType.TriangleList,
							part.VertexOffset,
							0,
							part.NumVertices,
							part.StartIndex,
							part.PrimitiveCount
						);
					}
				}
			}
		}

		#endregion
	}
}
