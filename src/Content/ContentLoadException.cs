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
	}
}

