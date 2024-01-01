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

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchcollection.aspx
	public struct TouchCollection : IList<TouchLocation>, ICollection<TouchLocation>, IEnumerable<TouchLocation>, IEnumerable
	{
		#region Public Properties

		public int Count
		{
			get
			{
				return touches.Count;
			}
		}

		public bool IsConnected
		{
			get
			{
				return TouchPanel.TouchDeviceExists;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public TouchLocation this[int index]
		{
			get
			{
				return touches[index];
			}
			set
			{
				// This will cause a runtime exception
				touches[index] = value;
			}
		}

		#endregion

		#region Private Variables

		private readonly List<TouchLocation> touches;

		#endregion

		#region Public Constructor

		public TouchCollection(TouchLocation[] touches)
		{
			this.touches = new List<TouchLocation>(touches);
		}

		#endregion

		#region Public Methods

		/* Since the collection is always readonly, using any
		 * method that attempts to modify touches will result
		 * in a System.NotSupportedException at runtime.
		 */

		public void Add(TouchLocation item)
		{
			touches.Add(item);
		}

		public void Clear()
		{
			touches.Clear();
		}

		public bool Contains(TouchLocation item)
		{
			return touches.Contains(item);
		}

		public void CopyTo(TouchLocation[] array, int arrayIndex)
		{
			touches.CopyTo(array, arrayIndex);
		}

		public bool FindById(int id, out TouchLocation touchLocation)
		{
			foreach (TouchLocation touch in touches)
			{
				if (touch.Id == id)
				{
					touchLocation = touch;
					return true;
				}
			}
			touchLocation = new TouchLocation(
				-1,
				TouchLocationState.Invalid,
				Vector2.Zero
			);
			return false;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public int IndexOf(TouchLocation item)
		{
			return touches.IndexOf(item);
		}

		public void Insert(int index, TouchLocation item)
		{
			touches.Insert(index, item);
		}

		public bool Remove(TouchLocation item)
		{
			return touches.Remove(item);
		}

		public void RemoveAt(int index)
		{
			touches.RemoveAt(index);
		}

		#endregion

		#region IEnumerator Methods

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<TouchLocation> System.Collections.Generic.IEnumerable<TouchLocation>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region Enumerator

		// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchcollection.enumerator.aspx
		public struct Enumerator : IEnumerator<TouchLocation>, IDisposable, IEnumerator
		{
			private TouchCollection collection;
			private int position;

			internal Enumerator(TouchCollection collection)
			{
				this.collection = collection;
				position = -1;
			}

			public TouchLocation Current
			{
				get
				{
					return collection[position];
				}
			}

			public bool MoveNext()
			{
				position += 1;
				return (position < collection.Count);
			}

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
