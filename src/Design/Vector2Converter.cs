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
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class Vector2Converter : MathTypeConverter
	{
		#region Public Constructor

		public Vector2Converter()
		{
			Type Vector2 = typeof(Vector2);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Vector2.GetField("X")),
				new FieldPropertyDescriptor(Vector2.GetField("Y"))
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
				return new Vector2(enumerator.Next(), enumerator.Next());
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
				return ConvertToString(culture, vec.X, vec.Y);
			}
			else if (destinationType == typeof(InstanceDescriptor))
			{
				Vector2 vector2 = (Vector2) value;
				return new InstanceDescriptor(
					typeof(Vector2).GetConstructor(
						new Type[] { typeof(float), typeof(float) }
					),
					new float[] { vector2.X, vector2.Y }
				);
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
			return (object) new Vector2(
				(float) propertyValues["X"],
				(float) propertyValues["Y"]
			);
		}

		#endregion
	}
}
