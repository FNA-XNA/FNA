#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
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
	public sealed class EffectAnnotationCollection : IEnumerable<EffectAnnotation>, IEnumerable
	{
		#region Public Properties

		public int Count => elements.Count;

        public EffectAnnotation this[int index] => elements[index];

        public EffectAnnotation this[string name]
		{
			get
			{
				foreach (EffectAnnotation elem in elements)
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

		private List<EffectAnnotation> elements;

		#endregion

		#region Internal Constructor

		internal EffectAnnotationCollection(List<EffectAnnotation> value)
		{
			elements = value;
		}

		#endregion

		#region Public Methods

		public List<EffectAnnotation>.Enumerator GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion

		#region IEnumerator Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		IEnumerator<EffectAnnotation> IEnumerable<EffectAnnotation>.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion
	}
}
