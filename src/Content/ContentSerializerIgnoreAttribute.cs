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
	/* http://msdn.microsoft.com/en-us/library/bb195465.aspx
	 * The class definition on msdn site shows: [AttributeUsageAttribute(384)]
	 * The following code var ff = (AttributeTargets)384; shows that ff is Field | Property
	 * so that is what we use.
	 */
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class ContentSerializerIgnoreAttribute : Attribute
	{
	}
}
