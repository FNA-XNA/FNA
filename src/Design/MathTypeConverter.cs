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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Text;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class MathTypeConverter : ExpandableObjectConverter
	{
		#region Protected Variables

		protected PropertyDescriptorCollection propertyDescriptions;

		protected bool supportStringConvert = true;

		#endregion

		#region Public Methods

		public override bool CanConvertFrom(
			ITypeDescriptorContext context,
			Type sourceType
		) {
			return supportStringConvert && sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(
			ITypeDescriptorContext context,
			Type destinationType
		) {
			return destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
		}

		public override bool GetCreateInstanceSupported(
			ITypeDescriptorContext context
		) {
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(
			ITypeDescriptorContext context,
			Object value,
			Attribute[] attributes
		) {
			return propertyDescriptions;
		}

		public override bool GetPropertiesSupported(
			ITypeDescriptorContext context
		) {
			return true;
		}

		#endregion

		internal static string ConvertToString<T>(CultureInfo culture, params T[] array) where T : struct, IConvertible
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			string listSeparator = culture.TextInfo.ListSeparator + " ";
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				builder.Append(array[i].ToString(culture));
				builder.Append(listSeparator);
			}
			builder.Length -= listSeparator.Length;
			return builder.ToString();
		}
	}

	internal struct StringListEnumerator<T> where T : struct
	{
		private readonly CultureInfo culture;
		private readonly string text;
		private readonly string listSeparator;
		private readonly TypeConverter converter;
		private int start;
		internal StringListEnumerator(CultureInfo culture, string text)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			this.culture = culture;
			listSeparator = culture.TextInfo.ListSeparator;
			this.text = text.Trim();
			converter = TypeDescriptor.GetConverter(typeof(T));
			start = 0;
		}
		internal T Next()
		{
			int end = text.IndexOf(listSeparator, start);
			if (end == -1)
			{
				end = text.Length;
			}
			T result = (T) converter.ConvertFromString(null, culture, text.Substring(start, end - start));
			start = end + listSeparator.Length;
			return result;
		}
	}

	internal abstract class MemberPropertyDescriptor : PropertyDescriptor
	{
		private readonly MemberInfo member;

		public MemberPropertyDescriptor(MemberInfo member) : base(member.Name, (Attribute[]) member.GetCustomAttributes(typeof(Attribute), true))
		{
			this.member = member;
		}

		public override Type ComponentType
		{
			get
			{
				return member.DeclaringType;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override void ResetValue(object component) { }

		public override bool ShouldSerializeValue(object component)
		{
			return true;
		}
	}

	internal sealed class FieldPropertyDescriptor : MemberPropertyDescriptor
	{
		private readonly FieldInfo field;

		public FieldPropertyDescriptor(FieldInfo field) : base(field)
		{
			this.field = field;
		}

		public override Type PropertyType
		{
			get
			{
				return field.FieldType;
			}
		}

		public override object GetValue(object component)
		{
			return field.GetValue(component);
		}

		public override void SetValue(object component, object value)
		{
			field.SetValue(component, value);
		}
	}

	internal sealed class PropertyPropertyDescriptor : MemberPropertyDescriptor
	{
		private readonly PropertyInfo property;

		public PropertyPropertyDescriptor(PropertyInfo property) : base(property)
		{
			this.property = property;
		}

		public override Type PropertyType
		{
			get
			{
				return property.PropertyType;
			}
		}

		public override object GetValue(object component)
		{
			return property.GetValue(component, null);
		}

		public override void SetValue(object component, object value)
		{
			property.SetValue(component, value, null);
		}
	}
}
