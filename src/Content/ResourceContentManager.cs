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
using System.IO;
using System.Resources;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public class ResourceContentManager : ContentManager
	{
		#region Private ResourceManager Instance

		private ResourceManager resource;

		#endregion

		#region Public Constructor

		public ResourceContentManager(
			IServiceProvider servicesProvider,
			ResourceManager resource
		) : base(servicesProvider) {
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}
			this.resource = resource;
		}

		#endregion

		#region Protected OpenStream Method

		protected override Stream OpenStream(string assetName)
		{
			object obj = this.resource.GetObject(assetName);
			if (obj == null)
			{
				throw new ContentLoadException("Resource not found");
			}
			byte[] byteArrayObject = obj as byte[];
			if (byteArrayObject == null)
			{
				throw new ContentLoadException("Resource is not in binary format");
			}
			return new MemoryStream(byteArrayObject);
		}

		#endregion
	}
}
