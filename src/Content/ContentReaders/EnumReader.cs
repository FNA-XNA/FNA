#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
	internal class EnumReader<T> : ContentTypeReader<T>
	{
		#region Private ContentTypeReader Instance

		ContentTypeReader elementReader;

		#endregion

		#region Public Constructor

		public EnumReader()
		{
		}

		#endregion

		#region Protected Initialization Method

		protected internal override void Initialize(ContentTypeReaderManager manager)
		{
			Type readerType = Enum.GetUnderlyingType(typeof(T));
			elementReader = manager.GetTypeReader(readerType);
		}

		#endregion

		#region Protected Read Method

		protected internal override T Read(ContentReader input, T existingInstance)
		{
			return input.ReadRawObject<T>(elementReader);
		}

		#endregion
	}
}
