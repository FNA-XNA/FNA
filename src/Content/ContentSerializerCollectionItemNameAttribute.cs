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
	/// This is used to specify the XML element name to use for each item in a collection.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ContentSerializerCollectionItemNameAttribute : Attribute
	{
		#region Public Properties

		/// <summary>
		/// The XML element name to use for each item in the collection.
		/// </summary>
		public string CollectionItemName
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		/// <summary>
		/// Creates an instance of the attribute.
		/// </summary>
		/// <param name="collectionItemName">The XML element name to use for each item in the collection.</param>
		public ContentSerializerCollectionItemNameAttribute(string collectionItemName)
		{
			CollectionItemName = collectionItemName;
		}

		#endregion
	}
}

