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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Represents a collection of ModelMesh objects.
	/// </summary>
	public sealed class ModelMeshCollection : ReadOnlyCollection<ModelMesh>
	{
		#region Public Properties

		/// <summary>
		/// Retrieves a ModelMesh from the collection, given the name of the mesh.
		/// </summary>
		/// <param name="meshName">
		/// The name of the mesh to retrieve.
		/// </param>
		public ModelMesh this[string meshName]
		{
			get
			{
				ModelMesh ret;
				if (!this.TryGetValue(meshName, out ret))
				{
					throw new KeyNotFoundException();
				}
				return ret;
			}
		}

		#endregion

		#region Internal Constructor

		internal ModelMeshCollection(IList<ModelMesh> list) : base(list)
		{
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Finds a mesh with a given name if it exists in the collection.
		/// </summary>
		/// <param name="meshName">
		/// The name of the mesh to find.
		/// </param>
		/// <param name="value">
		/// [OutAttribute] The mesh named meshName, if found.
		/// </param>
		public bool TryGetValue(string meshName, out ModelMesh value)
		{
			if (string.IsNullOrEmpty(meshName))
			{
				throw new ArgumentNullException("meshName");
			}

			foreach (ModelMesh mesh in this)
			{
				if (string.Compare(mesh.Name, meshName, StringComparison.Ordinal) == 0)
				{
					value = mesh;
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
		public struct Enumerator : IEnumerator<ModelMesh>
		{
			private readonly ModelMeshCollection collection;
			private int position;

			internal Enumerator(ModelMeshCollection collection)
			{
				this.collection = collection;
				position = -1;
			}


			/// <summary>
			/// Gets the current element in the ModelMeshCollection.
			/// </summary>
			public ModelMesh Current
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
