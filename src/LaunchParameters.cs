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
			for (int i = 1; i < args.Length; i++)
			{
				string key = args[i].TrimStart(flags);
				string value = string.Empty;

				/* You can have multiple :, only the first matters */
				int valueOffset = key.IndexOf(":");
				if (valueOffset != -1)
				{
					value = key.Substring(valueOffset + 1);
					key = key.Substring(0, valueOffset);
				}
				/* All instances after the first are ignored */
				if (key.Length != 0 && !ContainsKey(key))
				{
					Add(
						key,
						value
					);
				}
			}
		}

		#endregion
	}
}
