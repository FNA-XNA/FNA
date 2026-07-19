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
	public class MatrixConverter : MathTypeConverter
	{
		#region Public Constructor

		public MatrixConverter() : base()
		{
			// FIXME: Initialize propertyDescriptions... how? -flibit
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
			if (destinationType == typeof(InstanceDescriptor))
			{
				Matrix matrix = (Matrix) value;
				return new InstanceDescriptor(
					typeof(Matrix).GetConstructor(new Type[] {
						typeof(float), typeof(float), typeof(float), typeof(float),
						typeof(float), typeof(float), typeof(float), typeof(float),
						typeof(float), typeof(float), typeof(float), typeof(float),
						typeof(float), typeof(float), typeof(float), typeof(float)
					}),
					new object[] {
						matrix.M11, matrix.M12, matrix.M13, matrix.M14,
						matrix.M21, matrix.M22, matrix.M23, matrix.M24,
						matrix.M31, matrix.M32, matrix.M33, matrix.M34,
						matrix.M41, matrix.M42, matrix.M43, matrix.M44
					}
				);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			return (object) new Matrix(
				(float) propertyValues["M11"],
				(float) propertyValues["M12"],
				(float) propertyValues["M13"],
				(float) propertyValues["M14"],
				(float) propertyValues["M21"],
				(float) propertyValues["M22"],
				(float) propertyValues["M23"],
				(float) propertyValues["M24"],
				(float) propertyValues["M31"],
				(float) propertyValues["M32"],
				(float) propertyValues["M33"],
				(float) propertyValues["M34"],
				(float) propertyValues["M41"],
				(float) propertyValues["M42"],
				(float) propertyValues["M43"],
				(float) propertyValues["M44"]
			);
		}

		#endregion
	}
}
