#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
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
				return elements.Count;
			}
		}

		public EffectPass this[int index]
		{
			get
			{
				return elements[index];
			}
		}

		public EffectPass this[string name]
		{
			get
			{
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

		#endregion

		#region Internal Constructor

		internal EffectPassCollection(List<EffectPass> value)
		{
			elements = value;
		}

		#endregion

		#region Public Methods

		public List<EffectPass>.Enumerator GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion

		#region IEnumerator Methods

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		IEnumerator<EffectPass> System.Collections.Generic.IEnumerable<EffectPass>.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion
	}
}
