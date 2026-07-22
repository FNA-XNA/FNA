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
	public class Vector3Converter : MathTypeConverter
	{
		#region Public Constructor

		public Vector3Converter()
		{
			Type Vector3 = typeof(Vector3);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Vector3.GetField("X")),
				new FieldPropertyDescriptor(Vector3.GetField("Y")),
				new FieldPropertyDescriptor(Vector3.GetField("Z"))
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
				return new Vector3(enumerator.Next(), enumerator.Next(), enumerator.Next());
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is Vector3)
			{
				if (destinationType == typeof(string))
				{
					Vector3 vec = (Vector3) value;
					return ConvertToString(culture, vec.X, vec.Y, vec.Z);
				}
				else if (destinationType == typeof(InstanceDescriptor))
				{
					Vector3 vector3 = (Vector3) value;
					return new InstanceDescriptor(
						typeof(Vector3).GetConstructor(
							new Type[] { typeof(float), typeof(float), typeof(float) }
						),
						new float[] { vector3.X, vector3.Y, vector3.Z }
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
			return (object) new Vector3(
				(float) propertyValues["X"],
				(float) propertyValues["Y"],
				(float) propertyValues["Z"]
			);
		}

		#endregion
	}
}
