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
	public class ColorConverter : MathTypeConverter
	{
		#region Public Constructor

		public ColorConverter() : base()
		{
			Type Color = typeof(Color);
			propertyDescriptions = new PropertyDescriptorCollection(new PropertyDescriptor[] {
				new PropertyPropertyDescriptor(Color.GetProperty("R")),
				new PropertyPropertyDescriptor(Color.GetProperty("G")),
				new PropertyPropertyDescriptor(Color.GetProperty("B")),
				new PropertyPropertyDescriptor(Color.GetProperty("A"))
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
				string[] v = s.Split(
					culture.TextInfo.ListSeparator.ToCharArray()
				);
				return new Color(
					int.Parse(v[0], culture),
					int.Parse(v[1], culture),
					int.Parse(v[2], culture),
					int.Parse(v[3], culture)
				);
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
				Color src = (Color) value;
				return string.Join(
					culture.TextInfo.ListSeparator + " ",
					new string[]
					{
						src.R.ToString(culture),
						src.G.ToString(culture),
						src.B.ToString(culture),
						src.A.ToString(culture)
					}
				);
			}
			else if (destinationType == typeof(InstanceDescriptor))
			{
				Color color = (Color) value;
				return new InstanceDescriptor(
					typeof(Color).GetConstructor(
						new Type[] { typeof(byte), typeof(byte), typeof(byte), typeof(byte) }
					),
					new byte[] { color.R, color.G, color.B, color.A }
				);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues
		) {
			return (object) new Color(
				(int) propertyValues["R"],
				(int) propertyValues["G"],
				(int) propertyValues["B"],
				(int) propertyValues["A"]
			);
		}

		#endregion
	}
}
