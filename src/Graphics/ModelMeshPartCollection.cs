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
	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
		#region Public Constructor

		public ModelMeshPartCollection(IList<ModelMeshPart> list) : base(list)
		{
		}

		#endregion

		#region Enumerator

		public new Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public struct Enumerator : IEnumerator<ModelMeshPart>
		{
			private readonly ModelMeshPartCollection collection;
			private int position;

			internal Enumerator(ModelMeshPartCollection collection)
			{
				this.collection = collection;
				position = -1;
			}


			/// <summary>
			/// Gets the current element in the ModelMeshCollection.
			/// </summary>
			public ModelMeshPart Current
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
