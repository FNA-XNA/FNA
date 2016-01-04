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
	public sealed class EffectTechniqueCollection : IEnumerable<EffectTechnique>, IEnumerable
	{
		#region Public Properties

		public int Count
		{
			get
			{
				return elements.Count;
			}
		}

		public EffectTechnique this[int index]
		{
			get
			{
				return elements[index];
			}
		}

		public EffectTechnique this[string name]
		{
			get
			{
				foreach (EffectTechnique elem in elements)
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

		private List<EffectTechnique> elements;

		#endregion

		#region Internal Constructor

		internal EffectTechniqueCollection(List<EffectTechnique> value)
		{
			elements = value;
		}

		#endregion

		#region Public Methods

		public List<EffectTechnique>.Enumerator GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion

		#region IEnumerator Methods

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		IEnumerator<EffectTechnique> System.Collections.Generic.IEnumerable<EffectTechnique>.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion
	}
}
