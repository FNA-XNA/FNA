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
#endregion

namespace Microsoft.Xna.Framework.Content
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ContentSerializerAttribute : Attribute
	{
		#region Public Properties

		public bool AllowNull
		{
			get;
			set;
		}

		/// <summary>
		/// Returns the overriden XML element name or the default "Item".
		/// </summary>
		public string CollectionItemName
		{
			get
			{
				// Return the default if unset.
				return collectionItemName ?? "Item";
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				collectionItemName = value;
			}
		}

		public string ElementName
		{
			get;
			set;
		}

		public bool FlattenContent
		{
			get;
			set;
		}

		/// <summary>
		/// Returns true if the default CollectionItemName value was overridden.
		/// </summary>
		public bool HasCollectionItemName
		{
			get
			{
				return collectionItemName != null;
			}
		}

		public bool Optional
		{
			get;
			set;
		}

		public bool SharedResource
		{
			get;
			set;
		}

		#endregion

		#region Private Variables

		private string collectionItemName;

		#endregion

		#region Public Constructor

		/// <summary>
		/// Creates an instance of the attribute.
		/// </summary>
		public ContentSerializerAttribute()
		{
			AllowNull = true;
		}

		#endregion

		#region Public Clone Method

		public ContentSerializerAttribute Clone()
		{
			return new ContentSerializerAttribute() {
				AllowNull = AllowNull,
				collectionItemName = collectionItemName,
				ElementName = ElementName,
				FlattenContent = FlattenContent,
				Optional = Optional,
				SharedResource = SharedResource
			};
		}

		#endregion
	}
}
