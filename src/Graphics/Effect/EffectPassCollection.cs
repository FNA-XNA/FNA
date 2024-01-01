#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class EffectPassCollection : IEnumerable<EffectPass>, IEnumerable
	{
		#region Public Properties

		public int Count
		{
			get
			{
				if (elements == null)
				{
					return singleItem != null ? 1 : 0;
				}
				return elements.Count;
			}
		}

		public EffectPass this[int index]
		{
			get
			{
				if (elements != null)
				{
					return elements[index];
				}

				if (index != 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return singleItem;
			}
		}

		public EffectPass this[string name]
		{
			get
			{
				if (elements == null)
				{
					if (singleItem.Name.Equals(name))
					{
						return singleItem;
					}
					return null;
				}

				foreach (EffectPass elem in elements)
				{
					if (name.Equals(elem.Name))
					{
						return elem;
					}
				}
				return null; // FIXME: ArrayIndexOutOfBounds? -flibit
			}
		}

		#endregion

		#region Private Variables

		private List<EffectPass> elements;
		private EffectPass singleItem;

		#endregion

		#region Internal Constructor

		internal EffectPassCollection(List<EffectPass> value)
		{
			elements = value;
		}

		internal EffectPassCollection(EffectPass pass)
		{
			singleItem = pass;
		}

		#endregion

		#region Allocation Optimization

		internal List<EffectPass> GetList()
		{
			if (elements == null)
			{
				elements = new List<EffectPass>(1);
				elements.Add(singleItem);
			}
			return elements;
		}

		#endregion

		#region Public Methods

		public List<EffectPass>.Enumerator GetEnumerator()
		{
			return GetList().GetEnumerator();
		}

		#endregion

		#region IEnumerator Methods

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetList().GetEnumerator();
		}

		IEnumerator<EffectPass> System.Collections.Generic.IEnumerable<EffectPass>.GetEnumerator()
		{
			return GetList().GetEnumerator();
		}

		#endregion
	}
}
