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
			UInt64 value = input.ReadUInt64();
			UInt64 mask = (UInt64) 3 << 62;
			long ticks = (long) (value & ~mask);
			DateTimeKind kind = (DateTimeKind) ((value >> 62) & 3);
			return new DateTime(ticks, kind);
		}

		#endregion
	}
}
