#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// The collection of the <see cref="CurveKey"/> elements and a part of the <see cref="Curve"/> class.
	/// </summary>
	public class CurveKeyCollection : ICollection<CurveKey>, IEnumerable<CurveKey>, IEnumerable
	{
		#region Public Properties

		/// <summary>
		/// Returns the count of keys in this collection.
		/// </summary>
		public int Count
		{
			get
			{
				return innerlist.Count;
			}
		}

		/// <summary>
		/// Returns false because it is not a read-only collection.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Indexer.
		/// </summary>
		/// <param name="index">The index of key in this collection.</param>
		/// <returns><see cref="CurveKey"/> at <paramref name="index"/> position.</returns>
		public CurveKey this[int index]
		{
			get
			{
				return innerlist[index];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				if (MathHelper.WithinEpsilon(innerlist[index].Position, value.Position))
				{
					innerlist[index] = value;
				}
				else
				{
					innerlist.RemoveAt(index);
					innerlist.Add(value);
				}
			}
		}

		#endregion

		#region Private Fields

		private readonly List<CurveKey> innerlist;

		#endregion

		#region Private Constructors

		private CurveKeyCollection(List<CurveKey> innerlist)
		{
			this.innerlist = innerlist;
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates a new instance of <see cref="CurveKeyCollection"/> class.
		/// </summary>
		public CurveKeyCollection()
		{
			innerlist = new List<CurveKey>();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a key to this collection.
		/// </summary>
		/// <param name="item">New key for the collection.</param>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="item"/> is null.</exception>
		/// <remarks>The new key would be added respectively to a position of that key and the position of other keys.</remarks>
		public void Add(CurveKey item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			int i = innerlist.BinarySearch(item);
			if (i < 0)
			{
				/* ... otherwise, a negative number that is the bitwise complement
				 * of the index of the next element that is larger than item or, if there
				 * is no larger element, the bitwise complement of Count.
				 */
				i = ~i;
			}
			this.innerlist.Insert(i, item);
		}

		/// <summary>
		/// Removes all keys from this collection.
		/// </summary>
		public void Clear()
		{
			innerlist.Clear();
		}

		/// <summary>
		/// Creates a copy of this collection.
		/// </summary>
		/// <returns>A copy of this collection.</returns>
		public CurveKeyCollection Clone()
		{
			return new CurveKeyCollection(new List<CurveKey>(innerlist));
		}

		/// <summary>
		/// Determines whether this collection contains a specific key.
		/// </summary>
		/// <param name="item">The key to locate in this collection.</param>
		/// <returns><c>true</c> if the key is found; <c>false</c> otherwise.</returns>
		public bool Contains(CurveKey item)
		{
			return innerlist.Contains(item);
		}

		/// <summary>
		/// Copies the keys of this collection to an array, starting at the array index provided.
		/// </summary>
		/// <param name="array">Destination array where elements will be copied.</param>
		/// <param name="arrayIndex">The zero-based index in the array to start copying from.</param>
		public void CopyTo(CurveKey[] array, int arrayIndex)
		{
			innerlist.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator for the <see cref="CurveKeyCollection"/>.</returns>
		public IEnumerator<CurveKey> GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		/// <summary>
		/// Finds element in the collection and returns its index.
		/// </summary>
		/// <param name="item">Element for the search.</param>
		/// <returns>Index of the element; or -1 if item is not found.</returns>
		public int IndexOf(CurveKey item)
		{
			return innerlist.IndexOf(item);
		}

		/// <summary>
		/// Removes specific element.
		/// </summary>
		/// <param name="item">The element</param>
		/// <returns><c>true</c> if item is successfully removed; <c>false</c> otherwise. This method also returns <c>false</c> if item was not found.</returns>
		public bool Remove(CurveKey item)
		{
			return innerlist.Remove(item);
		}

		/// <summary>
		/// Removes element at the specified index.
		/// </summary>
		/// <param name="index">The index which element will be removed.</param>
		public void RemoveAt(int index)
		{
			innerlist.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		#endregion
	}
}
