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
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class NullableReader<T> : ContentTypeReader<T?> where T : struct
	{
		#region Private ContentTypeReader Instance

		ContentTypeReader elementReader;

		#endregion

		#region Internal Constructor

		internal NullableReader()
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

		protected internal override T? Read(ContentReader input, T? existingInstance)
		{
			if (input.ReadBoolean())
			{
				return input.ReadObject<T>(elementReader);
			}
			return null;
		}

		#endregion
	}
}
