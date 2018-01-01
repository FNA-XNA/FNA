#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.gesturesample.aspx
	public struct GestureSample
	{
		#region Public Properties

		public Vector2 Delta
		{
			get;
			private set;
		}

		public Vector2 Delta2
		{
			get;
			private set;
		}

		public GestureType GestureType
		{
			get;
			private set;
		}

		public Vector2 Position
		{
			get;
			private set;
		}

		public Vector2 Position2
		{
			get;
			private set;
		}

		public TimeSpan Timestamp
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal GestureSample(
			Vector2 delta,
			Vector2 delta2,
			GestureType gestureType,
			Vector2 position,
			Vector2 position2,
			TimeSpan timestamp
		) : this() {
			Delta = delta;
			Delta2 = delta2;
			GestureType = gestureType;
			Position = position;
			Position2 = position2;
			Timestamp = timestamp;
		}

		#endregion
	}
}
