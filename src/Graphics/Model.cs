#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// A basic 3D model with per mesh parent bones.
	/// </summary>
	public class Model
	{
		#region Public Properties

		/// <summary>
		/// A collection of <see cref="ModelBone"/> objects which describe how each mesh in the
		/// mesh collection for this model relates to its parent mesh.
		/// </summary>
		public ModelBoneCollection Bones
		{
			get;
			private set;
		}

		/// <summary>
		/// A collection of <see cref="ModelMesh"/> objects which compose the model. Each <see cref="ModelMesh"/>
		/// in a model may be moved independently and may be composed of multiple materials
		/// identified as <see cref="ModelMeshPart"/> objects.
		/// </summary>
		public ModelMeshCollection Meshes
		{
			get;
			private set;
		}

		/// <summary>
		/// Root bone for this model.
		/// </summary>
		public ModelBone Root
		{
			get;
			internal set;
		}

		/// <summary>
		/// Custom attached object.
		/// <remarks>
		/// Skinning data is example of attached object for model.
		/// </remarks>
		/// </summary>
		public object Tag
		{
			get;
			set;
		}

		#endregion

		#region Private Static Variables

		private static Matrix[] sharedDrawBoneMatrices;

		#endregion

		#region Internal Constructor

		/// <summary>
		/// Constructs a model.
		/// </summary>
		/// <param name="graphicsDevice">A valid reference to <see cref="GraphicsDevice"/>.</param>
		/// <param name="bones">The collection of bones.</param>
		/// <param name="meshes">The collection of meshes.</param>
		internal Model(GraphicsDevice graphicsDevice, List<ModelBone> bones, List<ModelMesh> meshes)
		{
			Bones = new ModelBoneCollection(bones);
			Meshes = new ModelMeshCollection(meshes);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Draws the model meshes.
		/// </summary>
		/// <param name="world">The world transform.</param>
		/// <param name="view">The view transform.</param>
		/// <param name="projection">The projection transform.</param>
		public void Draw(Matrix world, Matrix view, Matrix projection)
		{
			int boneCount = Bones.Count;

			if (sharedDrawBoneMatrices == null ||
				sharedDrawBoneMatrices.Length < boneCount)
			{
				sharedDrawBoneMatrices = new Matrix[boneCount];
			}

			// Look up combined bone matrices for the entire model.
			CopyAbsoluteBoneTransformsTo(sharedDrawBoneMatrices);

			// Draw the model.
			foreach (ModelMesh mesh in Meshes)
			{
				foreach (Effect effect in mesh.Effects)
				{
					IEffectMatrices effectMatricies = effect as IEffectMatrices;
					if (effectMatricies == null)
					{
						throw new InvalidOperationException();
					}
					effectMatricies.World = sharedDrawBoneMatrices[mesh.ParentBone.Index] * world;
					effectMatricies.View = view;
					effectMatricies.Projection = projection;
				}

				mesh.Draw();
			}
		}

		/// <summary>
		/// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
			{
				throw new ArgumentNullException("destinationBoneTransforms");
			}
			if (destinationBoneTransforms.Length < Bones.Count)
			{
				throw new ArgumentOutOfRangeException("destinationBoneTransforms");
			}

			int count = Bones.Count;
			for (int index1 = 0; index1 < count; index1 += 1)
			{
				ModelBone modelBone = Bones[index1];
				if (modelBone.Parent == null)
				{
					destinationBoneTransforms[index1] = modelBone.Transform;
				}
				else
				{
					int index2 = modelBone.Parent.Index;
					Matrix modelBoneTransform = modelBone.Transform;
					Matrix.Multiply(
						ref modelBoneTransform,
						ref destinationBoneTransforms[index2],
						out destinationBoneTransforms[index1]
					);
				}
			}
		}

		/// <summary>
		/// Copies bone transforms relative to <see cref="Model.Root"/> bone from a given array to this model.
		/// </summary>
		/// <param name="sourceBoneTransforms">The array of prepared bone transform data.</param>
		public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms)
		{
			if (sourceBoneTransforms == null)
			{
				throw new ArgumentNullException("sourceBoneTransforms");
			}
			if (sourceBoneTransforms.Length < Bones.Count)
			{
				throw new ArgumentOutOfRangeException("sourceBoneTransforms");
			}
			for (int i = 0; i < sourceBoneTransforms.Length; i += 1)
			{
				Bones[i].Transform = sourceBoneTransforms[i];
			}
		}

		/// <summary>
		/// Copies bone transforms relative to <see cref="Model.Root"/> bone from this model to a given array.
		/// </summary>
		/// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
		public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
			{
				throw new ArgumentNullException("destinationBoneTransforms");
			}
			if (destinationBoneTransforms.Length < Bones.Count)
			{
				throw new ArgumentOutOfRangeException("destinationBoneTransforms");
			}
			for (int i = 0; i < destinationBoneTransforms.Length; i += 1)
			{
				destinationBoneTransforms[i] = Bones[i].Transform;
			}
		}

		#endregion

		#region Internal Methods

		internal void BuildHierarchy()
		{
			Matrix globalScale = Matrix.CreateScale(0.01f);

			foreach (ModelBone node in Root.Children)
			{
				BuildHierarchy(node, Root.Transform * globalScale, 0);
			}
		}

		#endregion

		#region Private Methods

		private void BuildHierarchy(ModelBone node, Matrix parentTransform, int level)
		{
			node.ModelTransform = node.Transform * parentTransform;
			
			foreach (ModelBone child in node.Children)
			{
				BuildHierarchy(child, node.ModelTransform, level + 1);
			}
		}

		#endregion
	}
}
