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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal static class ContentExtensions
	{
		#region Public Static Constructor Extractor Method

		public static ConstructorInfo GetDefaultConstructor(this Type type)
		{
			BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			return type.GetConstructor(attrs, null, new Type[0], null);
		}

		#endregion

		#region Public Static Property Extractor Method

		public static PropertyInfo[] GetAllProperties(this Type type)
		{

			/* Sometimes, overridden properties of abstract classes can show up even with
			 * BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
			 * all properties in this list are defined in this class by comparing
			 * its get method with that of its base class. If they're the same
			 * Then it's an overridden property.
			 */
			const BindingFlags attrs = (
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.DeclaredOnly
			);
			List<PropertyInfo> allProps = type.GetProperties(attrs).ToList();
			PropertyInfo[] props = allProps.FindAll(
				p => p.GetGetMethod(true) != null && p.GetGetMethod(true) == p.GetGetMethod(true).GetBaseDefinition()
			).ToArray();
			return props;
		}

		#endregion

		#region Public Static Field Extractor Method

		public static FieldInfo[] GetAllFields(this Type type)
		{
			BindingFlags attrs = (
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.DeclaredOnly
			);
			return type.GetFields(attrs);
		}

		#endregion
	}
}
