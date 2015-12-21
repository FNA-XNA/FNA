#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
	/// Represents a collection of effects associated with a model.
	/// </summary>
	public sealed class ModelEffectCollection : ReadOnlyCollection<Effect>
	{
		#region Public Constructor

		public ModelEffectCollection(IList<Effect> list) : base(list)
		{
		}

		#endregion

		#region Internal Constructor

		internal ModelEffectCollection() : base(new List<Effect>())
		{
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a ModelEffectCollection.Enumerator that can iterate through a ModelEffectCollection.
		/// </summary>
		public new ModelEffectCollection.Enumerator GetEnumerator()
		{
			return new ModelEffectCollection.Enumerator((List<Effect>) Items);
		}

		#endregion

		#region Internal Methods

		// ModelMeshPart needs to be able to add to ModelMesh's effects list
		internal void Add(Effect item)
		{
			Items.Add(item);
		}

		internal void Remove(Effect item)
		{
			Items.Remove(item);
		}

		#endregion

		#region Public Enumerator struct

		/// <summary>
		/// Provides the ability to iterate through the bones in an ModelEffectCollection.
		/// </summary>
		public struct Enumerator : IEnumerator<Effect>, IDisposable, IEnumerator
		{
			/// <summary>
			/// Gets the current element in the ModelEffectCollection.
			/// </summary>
			public Effect Current
			{
				get
				{
					return enumerator.Current;
				}
			}

			List<Effect>.Enumerator enumerator;
			bool disposed;

			internal Enumerator(List<Effect> list)
			{
				enumerator = list.GetEnumerator();
				disposed = false;
			}

			/// <summary>
			/// Immediately releases the unmanaged resources used by this object.
			/// </summary>
			public void Dispose()
			{
				if (!disposed)
				{
					enumerator.Dispose();
					disposed = true;
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the ModelEffectCollection.
			/// </summary>
			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			void IEnumerator.Reset()
			{
				IEnumerator resetEnumerator = enumerator;
				resetEnumerator.Reset();
				enumerator = (List<Effect>.Enumerator) resetEnumerator;
			}

		}

		#endregion
	}
}
