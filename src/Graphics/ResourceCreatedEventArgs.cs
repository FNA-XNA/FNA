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

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class ResourceCreatedEventArgs : EventArgs
	{
		#region Public Properties

		/// <summary>
		/// The newly created resource object.
		/// </summary>
		public Object Resource
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal ResourceCreatedEventArgs(object resource)
		{
			Resource = resource;
		}

		#endregion
	}
}
