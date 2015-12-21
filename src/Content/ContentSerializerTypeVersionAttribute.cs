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
	/// <summary>
	/// This is used to specify the version when deserializing this object at runtime.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class ContentSerializerTypeVersionAttribute : Attribute
	{
		#region Public Properties

		/// <summary>
		/// The version passed to the type at runtime.
		/// </summary>
		public int TypeVersion
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		/// <summary>
		/// Creates an instance of the attribute.
		/// </summary>
		/// <param name="typeVersion">The version passed to the type at runtime.</param>
		public ContentSerializerTypeVersionAttribute(int typeVersion)
		{
			TypeVersion = typeVersion;
		}

		#endregion
	}
}
