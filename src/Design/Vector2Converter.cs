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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class Vector2Converter : MathTypeConverter
	{
		#region Public Constructor

		public Vector2Converter() : base()
		{
			// FIXME: Initialize propertyDescriptions... how? -flibit
		}

		#endregion

		#region Public Methods

		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value
		) {
			string s = value as string;
			if (s != null)
			{
				string[] v = s.Split(
					culture.NumberFormat.NumberGroupSeparator.ToCharArray()
				);
				return new Vector2(
					float.Parse(v[0], culture),
					float.Parse(v[1], culture)
				);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (destinationType == typeof(string))
			{
				Vector2 vec = (Vector2) value;
				return string.Join(
					culture.NumberFormat.NumberGroupSeparator,
					new string[]
					{
						vec.X.ToString(culture),
						vec.Y.ToString(culture)
					}
				);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			return (object) new Vector2(
				(float) propertyValues["X"],
				(float) propertyValues["Y"]
			);
		}

		#endregion
	}
}
