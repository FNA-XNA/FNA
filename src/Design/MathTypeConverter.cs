#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.ComponentModel;
#endregion

namespace Microsoft.Xna.Framework.Design
{
	public class MathTypeConverter : ExpandableObjectConverter
	{
		#region Protected Variables

		protected PropertyDescriptorCollection propertyDescriptions;

		protected bool supportStringConvert;

		#endregion

		#region Public Constructor

		public MathTypeConverter()
		{
			supportStringConvert = true;
		}

		#endregion

		#region Public Methods

		public override bool CanConvertFrom(
			ITypeDescriptorContext context,
			Type sourceType
		) {
			if (supportStringConvert && sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(
			ITypeDescriptorContext context,
			Type destinationType
		) {
			if (supportStringConvert && destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
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
	}
}
