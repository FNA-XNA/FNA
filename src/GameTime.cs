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

namespace Microsoft.Xna.Framework
{
	public class GameTime
	{
		#region Public Properties

		public TimeSpan TotalGameTime
		{
			get;
			internal set;
		}

		public TimeSpan ElapsedGameTime
		{
			get;
			internal set;
		}

		public bool IsRunningSlowly
		{
			get;
			internal set;
		}

		#endregion

		#region Public Constructors

		public GameTime()
		{
			TotalGameTime = TimeSpan.Zero;
			ElapsedGameTime = TimeSpan.Zero;
			IsRunningSlowly = false;
		}

		public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
		{
			TotalGameTime = totalGameTime;
			ElapsedGameTime = elapsedGameTime;
			IsRunningSlowly = false;
		}

		public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly)
		{
			TotalGameTime = totalRealTime;
			ElapsedGameTime = elapsedRealTime;
			IsRunningSlowly = isRunningSlowly;
		}

		#endregion
	}
}
