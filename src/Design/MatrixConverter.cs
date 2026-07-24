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
	public class MatrixConverter : MathTypeConverter
	{
		#region Public Constructor

		public MatrixConverter()
		{
			Type Matrix = typeof(Matrix);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(Matrix.GetField("M11")),
				new FieldPropertyDescriptor(Matrix.GetField("M12")),
				new FieldPropertyDescriptor(Matrix.GetField("M13")),
				new FieldPropertyDescriptor(Matrix.GetField("M14")),
				new FieldPropertyDescriptor(Matrix.GetField("M21")),
				new FieldPropertyDescriptor(Matrix.GetField("M22")),
				new FieldPropertyDescriptor(Matrix.GetField("M23")),
				new FieldPropertyDescriptor(Matrix.GetField("M24")),
				new FieldPropertyDescriptor(Matrix.GetField("M31")),
				new FieldPropertyDescriptor(Matrix.GetField("M32")),
				new FieldPropertyDescriptor(Matrix.GetField("M33")),
				new FieldPropertyDescriptor(Matrix.GetField("M34")),
				new FieldPropertyDescriptor(Matrix.GetField("M41")),
				new FieldPropertyDescriptor(Matrix.GetField("M42")),
				new FieldPropertyDescriptor(Matrix.GetField("M43")),
				new FieldPropertyDescriptor(Matrix.GetField("M44"))
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
			if (value is Matrix)
			{
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
						new float[] {
							matrix.M11, matrix.M12, matrix.M13, matrix.M14,
							matrix.M21, matrix.M22, matrix.M23, matrix.M24,
							matrix.M31, matrix.M32, matrix.M33, matrix.M34,
							matrix.M41, matrix.M42, matrix.M43, matrix.M44
						}
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
