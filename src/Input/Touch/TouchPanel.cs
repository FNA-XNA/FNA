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
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchpanel.aspx
	public static class TouchPanel
	{
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

		internal static List<TouchLocation> touches = new List<TouchLocation>(MAX_TOUCHES);
		internal static Queue<GestureSample> gestures = new Queue<GestureSample>();

		#endregion

		#region Private Static Variables

		private static Queue<TouchLocation> toProcess = new Queue<TouchLocation>();
		private static List<TouchLocation> toReleaseNextFrame = new List<TouchLocation>();

		private static DateTime touchDownTime;
		private static DateTime touchUpTime;
		private static Vector2 touchDownPosition;

		private enum GestureState
		{
			NONE,
			DOUBLETAP,
			HOLD
		};
		private static GestureState anticipatedGesture = GestureState.NONE;

		private static bool justDoubleTapped = false;

		#endregion

		#region Private Constants

		private const int MAX_TOUCHES = 8;
		private const int JITTER_THRESHOLD = 50;

		#endregion

		#region Public Static Methods

		public static TouchPanelCapabilities GetCapabilities()
		{
			return FNAPlatform.GetTouchCapabilities();
		}

		public static TouchCollection GetState()
		{
			return new TouchCollection(touches.ToArray());
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

		#region Internal Methods

		internal static void INTERNAL_onFingerDown(
			int fingerId,
			float x,
			float y
		) {
			toProcess.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Pressed,
				new Vector2(
					(float) Math.Round(x * DisplayWidth),
					(float) Math.Round(y * DisplayHeight)
				)
			));

			/* Calculate Gestures */

			// Is this the first finger on the screen?
			if (FNAPlatform.GetNumTouchFingers() <= 1)
			{
				Vector2 pos = new Vector2(
					(float)Math.Round(x * DisplayWidth),
					(float)Math.Round(y * DisplayHeight)
				);
				touchDownTime = DateTime.Now;

				if (anticipatedGesture == GestureState.DOUBLETAP)
				{
					if (touchDownTime.Subtract(touchUpTime) <= TimeSpan.FromMilliseconds(300))
					{
						if ((pos - touchDownPosition).Length() <= JITTER_THRESHOLD)
						{
							Console.WriteLine("DOUBLE TAP");
							justDoubleTapped = true;
						}
					}

					anticipatedGesture = GestureState.NONE;
				}
				else
				{
					anticipatedGesture = GestureState.HOLD;
				}

				touchDownPosition = pos;
			}
		}

		internal static void INTERNAL_onFingerUp(
			int fingerId,
			float x,
			float y
		) {
			toProcess.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Released,
				new Vector2(
					(float) Math.Round(x * DisplayWidth),
					(float) Math.Round(y * DisplayHeight)
				)
			));

			/* Calculate Gestures */

			// Was this the last finger to lift?
			if (FNAPlatform.GetNumTouchFingers() == 0)
			{
				bool didTap = false;

				touchUpTime = DateTime.Now;

				if (touchUpTime.Subtract(touchDownTime) <= TimeSpan.FromMilliseconds(1000))
				{
					Vector2 touchUpPosition = new Vector2(
						(float)Math.Round(x * DisplayWidth),
						(float)Math.Round(y * DisplayHeight)
					);

					if ((touchDownPosition - touchUpPosition).Length() <= JITTER_THRESHOLD)
					{
						if (!justDoubleTapped)
						{
							Console.WriteLine("TAP");
							didTap = true;
						}
					}
				}

				anticipatedGesture = GestureState.NONE;
				justDoubleTapped = false;

				if (didTap)
				{
					anticipatedGesture = GestureState.DOUBLETAP;
				}
			}
		}

		internal static void INTERNAL_onFingerMotion(
			int fingerId,
			float x,
			float y,
			float dx,
			float dy
		) {
			toProcess.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Moved,
				new Vector2(
					(float) Math.Round(x * DisplayWidth),
					(float) Math.Round(y * DisplayHeight)
				)
			));
		}

		internal static void UpdateTouches()
		{
			// Remove all touches that were released last frame
			touches.RemoveAll(touch => touch.State == TouchLocationState.Released);

			// Check for Hold gesture
			if (anticipatedGesture == GestureState.HOLD && touches.Count > 0)
			{
				if (DateTime.Now.Subtract(touchDownTime) >= TimeSpan.FromMilliseconds(1000))
				{
					if ((touchDownPosition - touches[0].Position).Length() <= JITTER_THRESHOLD)
					{
						Console.WriteLine("HOLD");
					}

					anticipatedGesture = GestureState.NONE;
				}
			}

			// Save touch states and positions for future reference
			List<TouchLocation> prevTouches = new List<TouchLocation>(touches);

			// Change formerly Pressed touches to Moved
			for (int i = 0; i < touches.Count; i += 1)
			{
				if (touches[i].State == TouchLocationState.Pressed)
				{
					touches[i] = new TouchLocation(
						touches[i].Id,
						TouchLocationState.Moved,
						touches[i].Position,
						prevTouches[i].State,
						prevTouches[i].Position
					);
				}
			}

			// Change formerly Pressed touches to Released if needed
			foreach (TouchLocation rtouch in toReleaseNextFrame)
			{
				for (int i = 0; i < touches.Count; i += 1)
				{
					if (touches[i].Id == rtouch.Id)
					{
						touches[i] = new TouchLocation(
							touches[i].Id,
							TouchLocationState.Released,
							touches[i].Position,
							prevTouches[i].State,
							prevTouches[i].Position
						);
					}
				}
			}
			toReleaseNextFrame.Clear();

			// Process all new touch events
			while (toProcess.Count > 0)
			{
				TouchLocation touch = toProcess.Dequeue();

				// Add a new (Pressed) touch if we have room
				if (touch.State == TouchLocationState.Pressed
					&& touches.Count < MAX_TOUCHES)
				{
					touches.Add(touch);
				}
				else
				{
					// Update touches that were already registered
					for (int i = 0; i < touches.Count; i += 1)
					{
						if (touches[i].Id == touch.Id)
						{
							// If this is a newly Pressed touch
							if (touches[i].State == TouchLocationState.Pressed)
							{
								if (touch.State == TouchLocationState.Released)
								{
									// Mark it for a Released state next frame
									if (!toReleaseNextFrame.Contains(touches[i]))
									{
										toReleaseNextFrame.Add(touches[i]);
									}
								}
							}
							else
							{
								// Update the existing touch with new data
								touches[i] = new TouchLocation(
									touches[i].Id,
									touch.State,
									touch.Position,
									prevTouches[i].State,
									prevTouches[i].Position
								);
							}

							// We found the touch we were looking for...
							break;
						}
					}
				}
			}
		}

		#endregion
	}
}
