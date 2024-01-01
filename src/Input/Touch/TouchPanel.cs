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
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchpanel.aspx
	public static class TouchPanel
	{
		#region Internal Constants

		// The maximum number of simultaneous touches allowed by XNA.
		internal const int MAX_TOUCHES = 8;

		// The value that represents the absence of a finger.
		internal const int NO_FINGER = -1;

		#endregion

		#region Public Static Properties

		public static int DisplayWidth
		{
			get;
			set;
		}

		public static int DisplayHeight
		{
			get;
			set;
		}

		public static DisplayOrientation DisplayOrientation
		{
			get;
			set;
		}

		public static GestureType EnabledGestures
		{
			get;
			set;
		}

		public static bool IsGestureAvailable
		{
			get
			{
				return gestures.Count > 0;
			}
		}

		public static IntPtr WindowHandle
		{
			get;
			set;
		}

		#endregion

		#region Internal Static Variables

		internal static bool TouchDeviceExists;

		#endregion

		#region Private Static Variables

		private static Queue<GestureSample> gestures = new Queue<GestureSample>();
		private static TouchLocation[] touches = new TouchLocation[MAX_TOUCHES];
		private static TouchLocation[] prevTouches = new TouchLocation[MAX_TOUCHES];
		private static List<TouchLocation> validTouches = new List<TouchLocation>();

		#endregion

		#region Public Static Methods

		public static TouchPanelCapabilities GetCapabilities()
		{
			return FNAPlatform.GetTouchCapabilities();
		}

		public static TouchCollection GetState()
		{
			validTouches.Clear();
			for (int i = 0; i < MAX_TOUCHES; i += 1)
			{
				if (touches[i].State != TouchLocationState.Invalid)
				{
					validTouches.Add(touches[i]);
				}
			}
			return new TouchCollection(validTouches.ToArray());
		}

		public static GestureSample ReadGesture()
		{
			if (gestures.Count == 0)
			{
				throw new InvalidOperationException();
			}

			return gestures.Dequeue();
		}

		#endregion

		#region Internal Static Methods

		internal static void EnqueueGesture(GestureSample gesture)
		{
			gestures.Enqueue(gesture);
		}

		internal static void INTERNAL_onTouchEvent(
			int fingerId,
			TouchLocationState state,
			float x,
			float y,
			float dx,
			float dy
		)
		{
			// Calculate the scaled touch position
			Vector2 touchPos = new Vector2(
				(float) Math.Round(x * DisplayWidth),
				(float) Math.Round(y * DisplayHeight)
			);

			// Notify the Gesture Detector about the event
			switch (state)
			{
				case TouchLocationState.Pressed:
					GestureDetector.OnPressed(fingerId, touchPos);
					break;

				case TouchLocationState.Moved:

					Vector2 delta = new Vector2(
						(float) Math.Round(dx * DisplayWidth),
						(float) Math.Round(dy * DisplayHeight)
					);

					GestureDetector.OnMoved(fingerId, touchPos, delta);

					break;

				case TouchLocationState.Released:
					GestureDetector.OnReleased(fingerId, touchPos);
					break;
			}
		}

		internal static void SetFinger(int index, int fingerId, Vector2 fingerPos)
		{
			if (fingerId == NO_FINGER)
			{
				// Was there a finger here before and the user just released it?
				if (prevTouches[index].State != TouchLocationState.Invalid
					&& prevTouches[index].State != TouchLocationState.Released)
				{
					touches[index] = new TouchLocation(
						prevTouches[index].Id,
						TouchLocationState.Released,
						prevTouches[index].Position,
						prevTouches[index].State,
						prevTouches[index].Position
					);
				}
				else
				{
					/* Nothing interesting here at all.
					 * Insert invalid data so this element
					 * is not included in GetState().
					 */
					touches[index] = new TouchLocation(
						NO_FINGER,
						TouchLocationState.Invalid,
						Vector2.Zero
					);
				}

				return;
			}

			// Is this a newly pressed finger?
			if (prevTouches[index].State == TouchLocationState.Invalid)
			{
				touches[index] = new TouchLocation(
					fingerId,
					TouchLocationState.Pressed,
					fingerPos
				);
			}
			else
			{
				// This finger was already down, so it's "moved"
				touches[index] = new TouchLocation(
					fingerId,
					TouchLocationState.Moved,
					fingerPos,
					prevTouches[index].State,
					prevTouches[index].Position
				);
			}
		}

		internal static void Update()
		{
			// Update Gesture Detector for time-sensitive gestures
			GestureDetector.OnUpdate();

			// Remember the last frame's touches
			touches.CopyTo(prevTouches, 0);

			// Get the latest finger data
			FNAPlatform.UpdateTouchPanelState();
		}

		#endregion
	}
}
