#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class ArrayReader<T> : ContentTypeReader<T[]>
	{
		#region Private ContentTypeReader Instance

		ContentTypeReader elementReader;

		#endregion

		#region Public Constructor

		public ArrayReader()
		{
		}

		#endregion

		#region Protected Initialization Method

		protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			Type readerType = typeof(T);
			elementReader = manager.GetTypeReader(readerType);
		}

		#endregion

		#region Protected Read Method

		protected internal override T[] Read(ContentReader input, T[] existingInstance)
		{
			uint count = input.ReadUInt32();
			T[] array = existingInstance;
			if (array == null)
			{
				array = new T[count];
			}

			if (typeof(T).IsValueType)
			{
				for (uint i = 0; i < count; i += 1)
				{
					array[i] = input.ReadObject<T>(elementReader);
				}
			}
			else
			{
				for (uint i = 0; i < count; i += 1)
				{
					int readerType = input.Read7BitEncodedInt();
					if (readerType > 0)
					{
						array[i] = input.ReadObject<T>(
							input.TypeReaders[readerType - 1]
						);
					}
					else {
						array[i] = default(T);
					}
				}
			}
			return array;
		}

		#endregion
	}
}
