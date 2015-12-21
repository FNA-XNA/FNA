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
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public abstract class ContentTypeReader
	{
		#region Public Properties

		public virtual bool CanDeserializeIntoExistingObject
		{
			get
			{
				return false;
			}
		}

		public Type TargetType
		{
			get
			{
				return this.targetType;
			}
		}

		public virtual int TypeVersion
		{
			// The default version (unless overridden) is zero
			get
			{
				return 0;
			}
		}

		#endregion

		#region Private Member Variables

		private Type targetType;

		#endregion

		#region Protected Constructors

		protected ContentTypeReader(Type targetType)
		{
			this.targetType = targetType;
		}

		#endregion

		#region Protected Internal Filename Normalizer Method

		protected internal static string Normalize(string fileName, string[] extensions)
		{
			if (File.Exists(fileName))
			{
				return fileName;
			}

			foreach (string ext in extensions)
			{
				// Concatenate the file name with valid extensions.
				string fileNamePlusExt = fileName + ext;
				if (File.Exists(fileNamePlusExt))
				{
					return fileNamePlusExt;
				}
			}
			return null;
		}

		#endregion

		#region Protected Initialization Method

		protected internal virtual void Initialize(ContentTypeReaderManager manager)
		{
			// Do nothing. Are we supposed to add ourselves to the manager?
		}

		#endregion

		#region Protected Read Method

		protected internal abstract object Read(ContentReader input, object existingInstance);

		#endregion
	}

	public abstract class ContentTypeReader<T> : ContentTypeReader
	{
		#region Protected Constructor

		protected ContentTypeReader() : base(typeof(T))
		{
		}

		#endregion

		#region Protected Read Methods

		/// <summary>
		/// Reads an object from the input stream.
		/// </summary>
		/// <param name="input">The input stream.</param>
		/// <param name="existingInstance">
		/// Existing instance of an object to receive the data, or null if a new object
		/// instance should be created.
		/// </param>
		protected internal override object Read(ContentReader input, object existingInstance)
		{
			if (existingInstance == null)
			{
				return this.Read(input, default(T));
			}
			else
			{
				return this.Read(input, (T) existingInstance);
			}
		}

		protected internal abstract T Read(ContentReader input, T existingInstance);

		#endregion
	}
}
