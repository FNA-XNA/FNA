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
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class PointConverter : MathTypeConverter
	{
		#region Public Constructor

		public PointConverter()
		{
			Type Point = typeof(Point);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Point.GetField("X")),
				new FieldPropertyDescriptor(Point.GetField("Y"))
			});
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
				StringListEnumerator<int> enumerator = new StringListEnumerator<int>(culture, s);
				return new Point(enumerator.Next(), enumerator.Next());
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			System.Globalization.CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is Point)
			{
				Point point;
				if (destinationType == typeof(string))
				{
					point = (Point) value;
					return ConvertToString(culture, point.X, point.Y);
				}
				else if (destinationType == typeof(InstanceDescriptor))
				{
					point = (Point) value;
					return new InstanceDescriptor(
						typeof(Point).GetConstructor(
							new Type[] { typeof(int), typeof(int) }
						),
						new int[] { point.X, point.Y }
					);
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			if (propertyValues == null)
			{
				throw new ArgumentNullException("propertyValues", "This method does not accept null for this parameter.");
			}
			return (object) new Point(
				(int) propertyValues["X"],
				(int) propertyValues["Y"]
			);
		}

		#endregion
	}
}
