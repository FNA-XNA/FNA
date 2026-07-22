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
	public class Vector4Converter : MathTypeConverter
	{
		#region Public Constructor

		public Vector4Converter()
		{
			Type Vector4 = typeof(Vector4);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Vector4.GetField("X")),
				new FieldPropertyDescriptor(Vector4.GetField("Y")),
				new FieldPropertyDescriptor(Vector4.GetField("Z")),
				new FieldPropertyDescriptor(Vector4.GetField("W"))
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
				return new Vector4(enumerator.Next(), enumerator.Next(), enumerator.Next(), enumerator.Next());
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is Vector4)
			{
				Vector4 vector;
				if (destinationType == typeof(string))
				{
					vector = (Vector4) value;
					return ConvertToString(culture, vector.X, vector.Y, vector.Z, vector.W);
				}
				else if (destinationType == typeof(InstanceDescriptor))
				{
					vector = (Vector4) value;
					return new InstanceDescriptor(
						typeof(Vector4).GetConstructor(
							new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }
						),
						new float[] { vector.X, vector.Y, vector.Z, vector.W }
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
			return (object) new Vector4(
				(float) propertyValues["X"],
				(float) propertyValues["Y"],
				(float) propertyValues["Z"],
				(float) propertyValues["W"]
			);
		}

		#endregion
	}
}
