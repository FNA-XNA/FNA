using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Model mesh descriptor
	/// </summary>
	public class ModelMeshDescEXT
	{
		/// <summary>
		/// Name of the model mesh
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Parts of the model mesh
		/// </summary>

		/// <summary>
		/// Bounding Sphere of the model mesh
		/// </summary>
		public BoundingSphere BoundingSphere { get; set; }

		/// <summary>
		/// Parts of the model mesh
		/// </summary>
		public readonly List<ModelMeshPart> Parts = new List<ModelMeshPart>();

		internal int Index { get; set; }

		/// <summary>
		/// Adds a mesh part to the model mesh
		/// </summary>
		/// <param name="indexBuffer">The index buffer for this mesh part</param>
		/// <param name="startIndex">The location in the index array at which to start reading vertices</param>
		/// <param name="vertexBuffer">The vertex buffer for this mesh part</param>
		/// <param name="vertexOffset">The offset (in vertices) from the top of vertex buffer</param>
		/// <param name="numVertices">The number of vertices used during a draw call</param>
		/// <param name="primitiveCount">The number of primitives to render</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void AddModelMeshPart(IndexBuffer indexBuffer, int startIndex,
			VertexBuffer vertexBuffer, int vertexOffset, int numVertices,
			int primitiveCount)
		{
			if (indexBuffer == null)
			{
				throw new ArgumentNullException(nameof(indexBuffer));
			}

			if (startIndex < 0 || startIndex >= indexBuffer.IndexCount)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (vertexBuffer == null)
			{
				throw new ArgumentNullException(nameof(vertexBuffer));
			}

			if (vertexOffset < 0 || vertexOffset >= vertexBuffer.VertexCount)
			{
				throw new ArgumentOutOfRangeException(nameof(vertexOffset));
			}

			if (numVertices <= 0 || numVertices > vertexBuffer.VertexCount)
			{
				throw new ArgumentOutOfRangeException(nameof(numVertices));
			}

			if (primitiveCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(primitiveCount));
			}

			var part = new ModelMeshPart()
			{
				IndexBuffer = indexBuffer,
				StartIndex = startIndex,
				VertexBuffer = vertexBuffer,
				VertexOffset = vertexOffset,
				NumVertices = numVertices,
				PrimitiveCount = primitiveCount
			};

			Parts.Add(part);
		}
	}

	/// <summary>
	/// Model bone descriptor
	/// </summary>
	public class ModelBoneDescEXT
	{
		/// <summary>
		/// Name of the model bone
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Transform of the model bone
		/// </summary>
		public Matrix Transform { get; set; }

		/// <summary>
		/// Meshes of the model bone
		/// </summary>
		public readonly List<ModelMeshDescEXT> Meshes = new List<ModelMeshDescEXT>();

		/// <summary>
		/// Children of the model bone
		/// </summary>
		public readonly List<ModelBoneDescEXT> Children = new List<ModelBoneDescEXT>();

		internal int Index { get; set; }
	}

	/// <summary>
	/// Grants ability to create a Model at the run-time
	/// </summary>
	public static class ModelBuilderEXT
	{
		/// <summary>
		/// Creates the model
		/// </summary>
		/// <param name="device">Graphics Device</param>
		/// <param name="meshes">Meshes of the model</param>
		/// <param name="bones">Bones of the model</param>
		/// <param name="root">Root bone of the model</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Model Create(GraphicsDevice device, List<ModelMeshDescEXT> meshes, List<ModelBoneDescEXT> bones, ModelBoneDescEXT root)
		{
			if (meshes == null)
			{
				throw new ArgumentNullException(nameof(meshes));
			}

			if (bones == null)
			{
				throw new ArgumentNullException(nameof(bones));
			}

			if (root == null)
			{
				throw new ArgumentNullException(nameof(root));
			}

			// Assign indexes
			for (var i = 0; i < meshes.Count; ++i)
			{
				meshes[i].Index = i;
			}

			for (var i = 0; i < bones.Count; ++i)
			{
				bones[i].Index = i;
			}

			// Create meshes
			var modelMeshes = new List<ModelMesh>();
			foreach (var desc in meshes)
			{
				var modelMesh = new ModelMesh(device, desc.Parts)
				{
					Name = desc.Name,
					BoundingSphere = desc.BoundingSphere,
				};

				modelMeshes.Add(modelMesh);
			}

			// Create bones
			var modelBones = new List<ModelBone>();
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = new ModelBone
				{
					Index = i,
					Name = desc.Name,
					Transform = desc.Transform
				};

				foreach (var mesh in desc.Meshes)
				{
					var modelMesh = modelMeshes[mesh.Index];
					modelMesh.ParentBone = bone;
					bone.AddMesh(modelMesh);
				}

				modelBones.Add(bone);
			}

			// Assign children
			for (var i = 0; i < bones.Count; ++i)
			{
				var desc = bones[i];
				var bone = modelBones[i];

				foreach (var child in desc.Children)
				{
					var childBone = modelBones[child.Index];
					childBone.Parent = bone;
					bone.AddChild(childBone);
				}
			}

			// Create the model
			var model = new Model(device, modelBones, modelMeshes)
			{
				Root = modelBones[root.Index]
			};

			return model;
		}
	}
}
