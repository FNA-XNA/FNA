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
using System.Reflection;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class BoundingBoxConverter : MathTypeConverter
	{
		#region Public Constructor

		public BoundingBoxConverter() : base()
		{
			// FIXME: Initialize propertyDescriptions... how? -flibit
			supportStringConvert = false;
		}

		#endregion

		#region Public Methods

		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value
		) {
			// FIXME: This method exists in the spec, but... why?! -flibit
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (destinationType == typeof(InstanceDescriptor))
			{
				BoundingBox boundingBox = (BoundingBox) value;
				ConstructorInfo constructor = typeof(BoundingBox).GetConstructor(
					new Type[] { typeof(Vector3), typeof(Vector3) }
				);
				return new InstanceDescriptor(constructor,
					new object[] { boundingBox.Min, boundingBox.Max }
				);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			return (object) new BoundingBox(
				(Vector3) propertyValues["Min"],
				(Vector3) propertyValues["Max"]
			);
		}

		#endregion
	}
}
