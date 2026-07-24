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
	public class QuaternionConverter : MathTypeConverter
	{
		#region Public Constructor

		public QuaternionConverter()
		{
			Type Quaternion = typeof(Quaternion);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Quaternion.GetField("X")),
				new FieldPropertyDescriptor(Quaternion.GetField("Y")),
				new FieldPropertyDescriptor(Quaternion.GetField("Z")),
				new FieldPropertyDescriptor(Quaternion.GetField("W"))
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
				StringListEnumerator<float> enumerator = new StringListEnumerator<float>(culture, s);
				return new Quaternion(enumerator.Next(), enumerator.Next(), enumerator.Next(), enumerator.Next());
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is Quaternion)
			{
				Quaternion quaternion;
				if (destinationType == typeof(string))
				{
					quaternion = (Quaternion) value;
					return ConvertToString(culture, quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
				}
				else if (destinationType == typeof(InstanceDescriptor))
				{
					quaternion = (Quaternion) value;
					return new InstanceDescriptor(
						typeof(Quaternion).GetConstructor(
							new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }
						),
						new float[] { quaternion.X, quaternion.Y, quaternion.Z, quaternion.W }
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
			return (object) new Quaternion(
				(float) propertyValues["X"],
				(float) propertyValues["Y"],
				(float) propertyValues["Z"],
				(float) propertyValues["W"]
			);
		}

		#endregion
	}
}
