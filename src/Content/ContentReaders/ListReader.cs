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
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class ListReader<T> : ContentTypeReader<List<T>>
	{
		#region Public Properties

		public override bool CanDeserializeIntoExistingObject
		{
			get
			{
				return true;
			}
		}

		#endregion

		#region Private ContentTypeReader Instance

		ContentTypeReader elementReader;

		#endregion

		#region Public Constructor

		public ListReader()
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

		protected internal override List<T> Read(
			ContentReader input,
			List<T> existingInstance
		) {
			int count = input.ReadInt32();
			List<T> list = existingInstance;
			if (list == null)
			{
				list = new List<T>(count);
			}
			for (int i = 0; i < count; i += 1)
			{
				Type objectType = typeof(T);
				if (objectType.IsValueType)
				{
					list.Add(input.ReadObject<T>(elementReader));
				}
				else
				{
					int readerType = input.Read7BitEncodedInt();
					list.Add((readerType > 0) ? input.ReadObject<T>(input.TypeReaders[readerType - 1]) : default(T));
				}
			}
			return list;
		}

		#endregion
	}
}
