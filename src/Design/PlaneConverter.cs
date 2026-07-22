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
	public class PlaneConverter : MathTypeConverter
	{
		#region Public Constructor

		public PlaneConverter()
		{
			Type Plane = typeof(Plane);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Plane.GetField("Normal")),
				new FieldPropertyDescriptor(Plane.GetField("D"))
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
			if (value is Plane)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					Plane plane = (Plane) value;
					return new InstanceDescriptor(
						typeof(Plane).GetConstructor(
							new Type[] { typeof(Vector3), typeof(float) }
						),
						new object[] { plane.Normal, plane.D }
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
			return (object) new Plane(
				(Vector3) propertyValues["Normal"],
				(float) propertyValues["D"]
			);
		}

		#endregion
	}
}
