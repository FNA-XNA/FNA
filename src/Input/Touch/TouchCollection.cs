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
				if (touches == null)
				{
					return 0;
				}
				return touches.Length;
			}
		}

		public bool IsConnected
		{
			get;
			private set;
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
				if (index < 0 || index >= Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return touches[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		#endregion

		#region Private Variables

		private readonly TouchLocation[] touches;

		#endregion

		#region Public Constructor

		public TouchCollection(TouchLocation[] touches)
		{
			if (touches == null)
			{
				throw new ArgumentNullException("touches");
			}
			if (touches.Length > TouchPanel.MAX_TOUCHES)
			{
				throw new ArgumentOutOfRangeException("touches");
			}
			IsConnected = true;
			this.touches = new TouchLocation[touches.Length];
			touches.CopyTo(this.touches, 0);
		}

		#endregion

		#region Internal Constructor

		internal TouchCollection(TouchLocation[] touches, bool isConnected)
		{
			this.touches = touches;
			IsConnected = isConnected;
		}

		#endregion

		#region Public Methods

		/* Since the collection is always readonly, using any
		 * method that attempts to modify touches will result
		 * in a System.NotSupportedException at runtime.
		 */

		public void Add(TouchLocation item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(TouchLocation item)
		{
			if (touches == null)
			{
				return false;
			}
			return Array.IndexOf(touches, item) != -1;
		}

		public void CopyTo(TouchLocation[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0 || array.Length - arrayIndex < Count)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (touches == null)
			{
				return;
			}
			touches.CopyTo(array, arrayIndex);
		}

		public bool FindById(int id, out TouchLocation touchLocation)
		{
			if (touches != null)
			{
				foreach (TouchLocation touch in touches)
				{
					if (touch.Id == id)
					{
						touchLocation = touch;
						return true;
					}
				}
			}
			touchLocation = new TouchLocation();
			return false;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public int IndexOf(TouchLocation item)
		{
			if (touches == null)
			{
				return -1;
			}
			return Array.IndexOf(touches, item);
		}

		public void Insert(int index, TouchLocation item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(TouchLocation item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
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
