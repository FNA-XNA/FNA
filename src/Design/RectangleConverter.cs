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
	public class RectangleConverter : MathTypeConverter
	{
		#region Public Constructor

		public RectangleConverter()
		{
			Type Rectangle = typeof(Rectangle);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Rectangle.GetField("X")),
				new FieldPropertyDescriptor(Rectangle.GetField("Y")),
				new FieldPropertyDescriptor(Rectangle.GetField("Width")),
				new FieldPropertyDescriptor(Rectangle.GetField("Height"))
			});
			supportStringConvert = false;
		}

		#endregion

		#region Public Methods

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is Rectangle)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					Rectangle rectangle = (Rectangle) value;
					return new InstanceDescriptor(
						typeof(Rectangle).GetConstructor(
							new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }
						),
						new int[] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height }
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
				throw new ArgumentNullException("propertyValues");
			}
			return (object) new Rectangle(
				(int) propertyValues["X"],
				(int) propertyValues["Y"],
				(int) propertyValues["Width"],
				(int) propertyValues["Height"]
			);
		}

		#endregion
	}
}
