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
	public class BoundingSphereConverter : MathTypeConverter
	{
		#region Public Constructor

		public BoundingSphereConverter() : base()
		{
			Type BoundingSphere = typeof(BoundingSphere);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new FieldPropertyDescriptor(BoundingSphere.GetField("Center")),
				new FieldPropertyDescriptor(BoundingSphere.GetField("Radius"))
			});
			supportStringConvert = false;
		}

		#endregion

		#region Public Methods

		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value
		) {
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType
		) {
			if (value is BoundingSphere)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					BoundingSphere boundingSphere = (BoundingSphere) value;
					return new InstanceDescriptor(
						typeof(BoundingSphere).GetConstructor(
							new Type[] { typeof(Vector3), typeof(float) }
						),
						new object[] { boundingSphere.Center, boundingSphere.Radius }
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
			return (object) new BoundingSphere(
				(Vector3) propertyValues["Center"],
				(float) propertyValues["Radius"]
			);
		}

		#endregion
	}
}
