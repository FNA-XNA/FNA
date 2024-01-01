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

		#region Public FNA Extension Properties

		public int FingerIdEXT
		{
			get;
			private set;
		}

		public int FingerId2EXT
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		public GestureSample(
			GestureType gestureType,
			TimeSpan timestamp,
			Vector2 position,
			Vector2 position2,
			Vector2 delta,
			Vector2 delta2
		) : this() {
			GestureType = gestureType;
			Timestamp = timestamp;
			Position = position;
			Position2 = position2;
			Delta = delta;
			Delta2 = delta2;

			// Oh well...
			FingerIdEXT = TouchPanel.NO_FINGER;
			FingerId2EXT = TouchPanel.NO_FINGER;
		}

		#endregion

		#region Internal Constructor

		internal GestureSample(
			GestureType gestureType,
			TimeSpan timestamp,
			Vector2 position,
			Vector2 position2,
			Vector2 delta,
			Vector2 delta2,
			int fingerId,
			int fingerId2
		) : this() {
			GestureType = gestureType;
			Timestamp = timestamp;
			Position = position;
			Position2 = position2;
			Delta = delta;
			Delta2 = delta2;
			FingerIdEXT = fingerId;
			FingerId2EXT = fingerId2;
		}

		#endregion
	}
}
