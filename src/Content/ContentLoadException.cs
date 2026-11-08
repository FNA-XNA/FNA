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
using System.Runtime.Serialization;

#endregion

namespace Microsoft.Xna.Framework.Content
{
	[Serializable]
	public class ContentLoadException : Exception
	{
		#region Public Constructors

		public ContentLoadException() : base()
		{
		}

		public ContentLoadException(string message) : base(message)
		{
		}

		public ContentLoadException(string message, Exception innerException) : base(message,innerException)
		{
		}

		#endregion

		#region Protected Constructors
#pragma warning disable SYSLIB0051
		protected ContentLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#pragma warning restore SYSLIB0051
		#endregion
	}
}

