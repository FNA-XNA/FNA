#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class DateTimeReader : ContentTypeReader<DateTime>
	{
		#region Internal Constructor

		internal DateTimeReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override DateTime Read(
			ContentReader input,
			DateTime existingInstance
		) {
			ulong value = input.ReadUInt64();
			ulong mask = (ulong) 3 << 62;
			long ticks = (long) (value & ~mask);
			DateTimeKind kind = (DateTimeKind) ((value >> 62) & 3);
			return new DateTime(ticks, kind);
		}

		#endregion
	}
}
