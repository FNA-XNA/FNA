#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
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
	internal class TimeSpanReader : ContentTypeReader<TimeSpan>
	{
		#region Internal Constructor

		internal TimeSpanReader()
		{
		}

		#endregion

		#region Protected Read Method

		protected internal override TimeSpan Read(
			ContentReader input,
			TimeSpan existingInstance
		) {
			/* Could not find any information on this really but from
			 * all the searching it looks like the constructor of number
			 * of ticks is long so I have placed that here for now.
			 * long is a Int64 so we read with 64
			 * <Duration>PT2S</Duration>
			 */
			return new TimeSpan(input.ReadInt64());
		}

		#endregion
	}
}
