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
	/// <summary>
	/// This is used to specify the type to use when deserializing this object at runtime.
	/// </summary>
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public sealed class ContentSerializerRuntimeTypeAttribute : Attribute
	{
		#region Public Properties

		/// <summary>
		/// The name of the type to use at runtime.
		/// </summary>
		public string RuntimeType
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		/// <summary>
		/// Creates an instance of the attribute.
		/// </summary>
		/// <param name="runtimeType">The name of the type to use at runtime.</param>
		public ContentSerializerRuntimeTypeAttribute(string runtimeType)
		{
			RuntimeType = runtimeType;
		}

		#endregion
	}
}
