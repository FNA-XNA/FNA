#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Represents a set of bones associated with a model.
	/// </summary>
	public class ModelBoneCollection : ReadOnlyCollection<ModelBone>
	{
		#region Public Properties

		/// <summary>
		/// Retrieves a ModelBone from the collection, given the name of the bone.
		/// </summary>
		/// <param name="boneName">
		/// The name of the bone to retrieve.
		/// </param>
		public ModelBone this[string boneName]
		{
			get
			{
				ModelBone ret;
				if (TryGetValue(boneName, out ret))
				{
					return ret;
				}
				throw new KeyNotFoundException();
			}
		}

		#endregion

		#region Public Constructor

		public ModelBoneCollection(IList<ModelBone> list) : base(list)
		{
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Finds a bone with a given name if it exists in the collection.
		/// </summary>
		/// <param name="boneName">
		/// The name of the bone to find.
		/// </param>
		/// <param name="value">
		/// [OutAttribute] The bone named boneName, if found.
		/// </param>
		public bool TryGetValue(string boneName, out ModelBone value)
		{
			foreach (ModelBone bone in base.Items)
			{
				if (bone.Name == boneName)
				{
					value = bone;
					return true;
				}
			}
			value = null;
			return false;
		}

		#endregion

		#region Enumerator

		/// <summary>
		/// Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
		/// </summary>
		/// <returns></returns>
		public new Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Provides the ability to iterate through the bones in an ModelMeshCollection.
		/// </summary>
		public struct Enumerator : IEnumerator<ModelBone>
		{
			private readonly ModelBoneCollection collection;
			private int position;

			internal Enumerator(ModelBoneCollection collection)
			{
				this.collection = collection;
				position = -1;
			}


			/// <summary>
			/// Gets the current element in the ModelMeshCollection.
			/// </summary>
			public ModelBone Current
			{
				get
				{
					return collection[position];
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the ModelMeshCollection.
			/// </summary>
			public bool MoveNext()
			{
				position += 1;
				return (position < collection.Count);
			}

			/// <summary>
			/// Immediately releases the unmanaged resources used by this object.
			/// </summary>
			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					return collection[position];
				}
			}

			void IEnumerator.Reset()
			{
				position = -1;
			}
		}

		#endregion
	}
}
