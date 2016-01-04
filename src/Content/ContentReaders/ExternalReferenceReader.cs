#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Content
{
	/// <summary>
	/// External reference reader, provided for compatibility with XNA Framework built content
	/// </summary>
	internal class ExternalReferenceReader : ContentTypeReader
	{
		#region Public Constructor

		public ExternalReferenceReader() : base(typeof(object))
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override object Read(
			ContentReader input,
			object existingInstance
		) {
			return input.ReadExternalReference<object>();
		}

		#endregion
	}
}
