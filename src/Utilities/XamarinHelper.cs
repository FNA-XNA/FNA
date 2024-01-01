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

// This is a dummy namespace needed for Xamarin iOS/tvOS AOT compilation
namespace ObjCRuntime
{
	[AttributeUsage(AttributeTargets.Method)]
	class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type t)
		{

		}
	}
}
