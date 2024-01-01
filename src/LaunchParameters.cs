#region License
/*
#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework
{
	public class LaunchParameters : Dictionary<string, string>
	{
		/* FIXME: This whole parser is one big assumption!
		 *
		 * I basically started with what MS programs usually accept as
		 * arguments, then threw a bunch of values at XNA to see what it
		 * accepted and what it didn't.
		 *
		 * Aside from what you see below, all I could rule out was that
		 * it doesn't let you do two args as one param, and '=' is not a
		 * valid value separator either. As an example, "-r:FNA.dll"
		 * will work, "-r FNA.dll" and "-r=FNA.dll" will not.
		 *
		 * The part that bothers me the most, however, is the flag
		 * indicator. It seems to let anything through as long as : is
		 * there, but it trims some special chars, and does so pretty
		 * broadly. You can do '-', "--", "---", etc! Lastly, in
		 * addition to the chars below, I also tried '+', which didn't
		 * work. I have no idea if there are any other chars to check.
		 *
		 * If anybody has an official standard, I'd like to see it!
		 * -flibit
		 */

		#region Private Static Variables

		private static readonly char[] flags = new char[]
		{
			'/', '-'
		};

		#endregion

		#region Public Constructor

		public LaunchParameters()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			foreach (string a in args)
			{
				string arg = a.TrimStart(flags);

				/* 1 for ':', 1 for key, 1 for value */
				if (arg.Length < 3)
				{
					continue;
				}

				/* You can have multiple :, only the first matters */
				int valueOffset = arg.IndexOf(":", 1, arg.Length - 2);
				if (valueOffset >= 0)
				{
					/* All instances after the first are ignored */
					string key = arg.Substring(0, valueOffset);
					if (!ContainsKey(key))
					{
						Add(
							key,
							arg.Substring(valueOffset + 1)
						);
					}
				}
			}
		}

		#endregion
	}
}
