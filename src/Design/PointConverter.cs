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
	public class PointConverter : MathTypeConverter
	{
		#region Public Constructor

		public PointConverter() : base()
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
				return new Point(
					int.Parse(v[0], culture),
					int.Parse(v[1], culture)
				);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			System.Globalization.CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (destinationType == typeof(string))
			{
				Point pt = (Point) value;
				return string.Join(
					culture.NumberFormat.NumberGroupSeparator,
					new string[]
					{
						pt.X.ToString(culture),
						pt.Y.ToString(culture)
					}
				);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			return (object) new Point(
				(int) propertyValues["X"],
				(int) propertyValues["Y"]
			);
		}

		#endregion
	}
}
