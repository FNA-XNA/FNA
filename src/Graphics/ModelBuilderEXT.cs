using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Grants ability to create a Model at the run-time
	/// </summary>
	public class ModelBuilderEXT
	{
		private class ModelMeshDesc
		{
			public string Name;
			public readonly List<ModelMeshPart> Parts = new List<ModelMeshPart>();
			public BoundingSphere BoundingSphere;
			public int? ParentBoneIndex;
		}

		private class ModelBoneDesc
		{
			public string Name;
			public int? ParentBoneIndex;
			public readonly HashSet<int> MeshesIndexes = new HashSet<int>();
			public readonly HashSet<int> ChildBonesIndexes = new HashSet<int>();
			public Matrix Transform;
		}

		private readonly List<ModelMeshDesc> _meshData = new List<ModelMeshDesc>();
		private readonly List<ModelBoneDesc> _boneData = new List<ModelBoneDesc>();

		/// <summary>
		/// Adds a model mesh
		/// </summary>
		/// <returns>Index of the new model mesh</returns>
		public int AddModelMesh()
		{
			var index = _meshData.Count;

			_meshData.Add(new ModelMeshDesc());

			return index;
		}

		/// <summary>
		/// Sets the name of the specified model mesh
		/// </summary>
		/// <param name="modelMeshIndex">Index of the model mesh</param>
		/// <param name="name">The name of the model mesh</param>
		public void SetModelMeshName(int modelMeshIndex, string name)
		{
			if (modelMeshIndex < 0 || modelMeshIndex >= _meshData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelMeshIndex));
			}

			_meshData[modelMeshIndex].Name = name;
		}

		/// <summary>
		/// Adds a mesh part to the model mesh
		/// </summary>
		/// <param name="modelMeshIndex">Index of the model mesh</param>
		/// <param name="indexBuffer">The index buffer for this mesh part</param>
		/// <param name="startIndex">The location in the index array at which to start reading vertices</param>
		/// <param name="vertexBuffer">The vertex buffer for this mesh part</param>
		/// <param name="vertexOffset">The offset (in vertices) from the top of vertex buffer</param>
		/// <param name="numVertices">The number of vertices used during a draw call</param>
		/// <param name="primitiveCount">The number of primitives to render</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void AddModelMeshPart(int modelMeshIndex,
			IndexBuffer indexBuffer, int startIndex,
			VertexBuffer vertexBuffer, int vertexOffset, int numVertices,
			int primitiveCount)
		{
			if (modelMeshIndex < 0 || modelMeshIndex >= _meshData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelMeshIndex));
			}

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

			_meshData[modelMeshIndex].Parts.Add(part);
		}

		/// <summary>
		/// Sets the bounding sphere for the specified model mesh.
		/// </summary>
		/// <param name="modelMeshIndex">Index of the model mesh</param>
		/// <param name="sphere">The bounding sphere</param>
		public void SetModelMeshBoundingSphere(int modelMeshIndex, BoundingSphere sphere)
		{
			if (modelMeshIndex < 0 || modelMeshIndex >= _meshData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelMeshIndex));
			}

			_meshData[modelMeshIndex].BoundingSphere = sphere;
		}

		/// <summary>
		/// Adds a new model bone
		/// </summary>
		/// <returns>Index of the new model bone</returns>
		public int AddModelBone()
		{
			var index = _boneData.Count;

			_boneData.Add(new ModelBoneDesc());

			return index;
		}

		/// <summary>
		/// Sets the name of the specified model bone
		/// </summary>
		/// <param name="modelBoneIndex">Index of the model bone</param>
		/// <param name="name">The name of the model bone</param>
		public void SetModelBoneName(int modelBoneIndex, string name)
		{
			if (modelBoneIndex < 0 || modelBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelBoneIndex));
			}

			_boneData[modelBoneIndex].Name = name;
		}

		/// <summary>
		/// Sets the transform of the specified model bone
		/// </summary>
		/// <param name="modelBoneIndex">Index of the model bone</param>
		/// <param name="transform">The transform of the model bone</param>
		public void SetModelBoneTransform(int modelBoneIndex, Matrix transform)
		{
			if (modelBoneIndex < 0 || modelBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelBoneIndex));
			}

			_boneData[modelBoneIndex].Transform = transform;
		}

		/// <summary>
		/// Adds the specified model mesh to the specified model bone
		/// </summary>
		/// <param name="modelBoneIndex">Index of the model bone</param>
		/// <param name="modelMeshIndex">Index of the model mesh</param>
		public void AddMeshToBone(int modelBoneIndex, int modelMeshIndex)
		{
			if (modelBoneIndex < 0 || modelBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelBoneIndex));
			}

			if (modelMeshIndex < 0 || modelMeshIndex >= _meshData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(modelMeshIndex));
			}

			_boneData[modelBoneIndex].MeshesIndexes.Add(modelMeshIndex);
			_meshData[modelMeshIndex].ParentBoneIndex = modelBoneIndex;
		}

		/// <summary>
		/// Adds a child model bone to the parent model bone
		/// </summary>
		/// <param name="parentBoneIndex">Index of the parent bone</param>
		/// <param name="childBoneIndex">Index of the child bone</param>
		public void AddBoneToBone(int parentBoneIndex, int childBoneIndex)
		{
			if (parentBoneIndex < 0 || parentBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(parentBoneIndex));
			}

			if (childBoneIndex < 0 || childBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(childBoneIndex));
			}

			_boneData[childBoneIndex].ParentBoneIndex = parentBoneIndex;
			_boneData[parentBoneIndex].ChildBonesIndexes.Add(childBoneIndex);
		}

		/// <summary>
		/// Creates the model
		/// </summary>
		/// <param name="device">Graphics Device</param>
		/// <param name="rootBoneIndex">Index of the root bone</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public Model Create(GraphicsDevice device, int rootBoneIndex)
		{
			if (rootBoneIndex < 0 || rootBoneIndex >= _boneData.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rootBoneIndex));
			}

			// Create meshes
			var meshes = new List<ModelMesh>();
			foreach (var desc in _meshData)
			{
				var mesh = new ModelMesh(device, desc.Parts)
				{
					Name = desc.Name,
					BoundingSphere = desc.BoundingSphere,
				};

				meshes.Add(mesh);
			}

			// Create bones
			var bones = new List<ModelBone>();
			for (var i = 0; i < _boneData.Count; ++i)
			{
				var desc = _boneData[i];
				var bone = new ModelBone
				{
					Index = i,
					Name = desc.Name,
					Transform = desc.Transform
				};

				foreach (var meshIndex in desc.MeshesIndexes)
				{
					var mesh = meshes[meshIndex];
					mesh.ParentBone = bone;
					bone.AddMesh(meshes[meshIndex]);
				}

				bones.Add(bone);
			}

			// Assign children
			for (var i = 0; i < _boneData.Count; ++i)
			{
				var desc = _boneData[i];
				var bone = bones[i];

				foreach (var childIndex in desc.ChildBonesIndexes)
				{
					var childBone = bones[childIndex];
					childBone.Parent = bone;
					bone.AddChild(childBone);
				}
			}

			// Create the model
			var model = new Model(device, bones, meshes)
			{
				Root = bones[rootBoneIndex]
			};

			return model;
		}

		/// <summary>
		/// Clears internal lists so the new model could be created
		/// </summary>
		public void Reset()
		{
			_meshData.Clear();
			_boneData.Clear();
		}
	}
}
