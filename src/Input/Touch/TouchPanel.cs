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

		/* Touch Variables */
		private static Queue<TouchLocation> toProcess = new Queue<TouchLocation>();
		private static List<TouchLocation> toReleaseNextFrame = new List<TouchLocation>();

		/* Gesture Variables */
		private static DateTime gTouchDownTime;
		private static DateTime gReleaseTime;
		private static Vector2 gTouchDownPosition;
		private static int activeFingerId = -1;
		private static bool justDoubleTapped = false;
		private static GestureState gState = GestureState.NONE;
		
		#endregion

		#region Private Constants

		private const int MAX_TOUCHES = 8;
		private const int MOVE_THRESHOLD = 35;

		private enum GestureState
		{
			NONE,
			HOLDING,
			HELD,
			JUST_TAPPED,
			DRAGGING_FREE,
			DRAGGING_H,
			DRAGGING_V,
			PINCHING
		};

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

		internal static void INTERNAL_onTouchEvent(
			int fingerId,
			TouchLocationState state,
			float x,
			float y,
			float dx,
			float dy
		) {
			// Get the touch position
			Vector2 touchPos = new Vector2(
				(float) Math.Round(x * DisplayWidth),
				(float) Math.Round(y * DisplayHeight)
			);

			// Process the touch on the next frame
			toProcess.Enqueue(new TouchLocation(
				fingerId,
				state,
				touchPos
			));

			// Use it for gesture detection
			switch (state)
			{
				case TouchLocationState.Pressed:
					CalculateGesture_FingerDown(fingerId, touchPos);
					break;

				case TouchLocationState.Moved:
					Vector2 delta = new Vector2(
						(float) Math.Round(dx * DisplayWidth),
						(float) Math.Round(dy * DisplayHeight)
					);
					CalculateGesture_FingerMoved(fingerId, touchPos, delta);
					break;

				case TouchLocationState.Released:
					CalculateGesture_FingerUp(fingerId, touchPos);
					break;
			}
			
		}

		internal static void UpdateTouches()
		{
			// Remove all touches that were released last frame
			touches.RemoveAll(touch => touch.State == TouchLocationState.Released);

			// Check for Hold gesture
			if (touches.Count > 0)
			{
				CalculateGesture_OnUpdate(touches[0].Position);
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

		#region Private Methods

		/* Called when SDL detects a FingerDown event.
		 * This detects the first active finger, prepares for
		 * Taps and Holds, and triggers Double Tap gestures.
		 */
		private static void CalculateGesture_FingerDown(int fingerId, Vector2 touchPosition)
		{
			// Set the active finger if there isn't one already
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
			}
			else if (fingerId != activeFingerId)
			{
				// Ignore the new finger for now
				return;
			}

			// Handle Double Tap gestures
			if (gState == GestureState.JUST_TAPPED)
			{
				if ((EnabledGestures & GestureType.DoubleTap) != 0)
				{
					// Must tap again within 300ms of original tap
					TimeSpan timeBetweenTaps = DateTime.Now.Subtract(gReleaseTime);
					if (timeBetweenTaps <= TimeSpan.FromMilliseconds(300))
					{
						// If the new tap is close to the original tap
						float distance = (touchPosition - gTouchDownPosition).Length();
						if (distance <= MOVE_THRESHOLD)
						{
							// Double Tap!
							gestures.Enqueue(new GestureSample(
								Vector2.Zero,
								Vector2.Zero,
								GestureType.DoubleTap,
								touchPosition,
								Vector2.Zero,
								TimeSpan.FromTicks(DateTime.Now.Ticks)
							));

							justDoubleTapped = true;
						}
					}
				}
			}

			// Prepare for a potential Tap or Hold gesture
			gState = GestureState.HOLDING;

			// Store the time and position the user touched down
			gTouchDownTime = DateTime.Now;
			gTouchDownPosition = touchPosition;
		}

		/* This is called when SDL detects a FingerUp event.
		 * It's responsible for resetting the active finger
		 * and firing Tap and Drag Complete gestures.
		 */
		private static void CalculateGesture_FingerUp(int fingerId, Vector2 touchPosition)
		{
			// Reset the active finger if the user lifted it
			if (fingerId == activeFingerId)
			{
				activeFingerId = -1;
			}

			// We're only interested in the very last finger to leave
			if (FNAPlatform.GetNumTouchFingers() > 0)
			{
				return;
			}

			// Check for Tap gesture
			if (gState == GestureState.HOLDING)
			{
				// Which Taps are enabled?
				bool tapEnabled = (EnabledGestures & GestureType.Tap) != 0;
				bool dtapEnabled = (EnabledGestures & GestureType.DoubleTap) != 0;

				if (tapEnabled || dtapEnabled)
				{
					// Must lift finger within 1 second of touching down to tap
					TimeSpan timeHeld = DateTime.Now.Subtract(gTouchDownTime);
					if (timeHeld < TimeSpan.FromMilliseconds(1000))
					{
						// Don't register a Tap immediately after a Double Tap
						if (!justDoubleTapped)
						{
							if (tapEnabled)
							{
								// Tap!
								gestures.Enqueue(new GestureSample(
									Vector2.Zero,
									Vector2.Zero,
									GestureType.Tap,
									touchPosition,
									Vector2.Zero,
									TimeSpan.FromTicks(DateTime.Now.Ticks)
								));
							}

							/* Even if Tap isn't enabled, we still
							* need this for Double Tap detection.
							*/
							gState = GestureState.JUST_TAPPED;
						}
					}
				}
			}
			else if (gState == GestureState.DRAGGING_H ||
					 gState == GestureState.DRAGGING_V ||
					 gState == GestureState.DRAGGING_FREE
			) {
				if ((EnabledGestures & GestureType.DragComplete) != 0)
				{
					// Drag Complete!
					gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.DragComplete,
						Vector2.Zero,
						Vector2.Zero,
						TimeSpan.FromTicks(DateTime.Now.Ticks)
					));
				}
			}

			// Reset the state if the user didn't just tap
			if (gState != GestureState.JUST_TAPPED)
			{
				gState = GestureState.NONE;
			}

			// Reset double tap flag so we can register taps again
			justDoubleTapped = false;

			// Store the time the finger was released
			gReleaseTime = DateTime.Now;
		}

		/* This is called whenever SDL detects a FingerMotion event.
		 * Its primary purpose is to notice Drag gestures and cancel
		 * Hold/Tap gestures if the user moves their finger too much.
		 */
		private static void CalculateGesture_FingerMoved(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// Replace the active finger with this one if needed
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
			}
			else if (fingerId != activeFingerId)
			{
				// Ignore any other finger
				return;
			}

			// Determine which drag gestures are enabled
			bool hdrag = (EnabledGestures & GestureType.HorizontalDrag) != 0;
			bool vdrag = (EnabledGestures & GestureType.VerticalDrag) != 0;
			bool fdrag = (EnabledGestures & GestureType.FreeDrag) != 0;

			// Check for drag initialization
			if (gState == GestureState.HOLDING || gState == GestureState.HELD)
			{
				// If the finger moved outside the threshold distance
				float distanceMoved = (touchPosition - gTouchDownPosition).Length();
				if (distanceMoved > MOVE_THRESHOLD)
				{
					// All right, which drag are we going with?
					if (hdrag && (Math.Abs(delta.X) > Math.Abs(delta.Y)))
					{
						// Horizontal Drag!
						gState = GestureState.DRAGGING_H;
					}
					else if (vdrag && (Math.Abs(delta.Y) > Math.Abs(delta.X)))
					{
						// Vertical Drag!
						gState = GestureState.DRAGGING_V;
					}
					else if (fdrag)
					{
						// Free Drag!
						gState = GestureState.DRAGGING_FREE;
					}
					else
					{
						// No drag...
						gState = GestureState.NONE;
					}
				}
			}

			// Handle Dragging
			if (gState == GestureState.DRAGGING_H && hdrag)
			{
				// Horizontal Dragging!
				gestures.Enqueue(new GestureSample(
					new Vector2(delta.X, 0),
					Vector2.Zero,
					GestureType.HorizontalDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}
			else if (gState == GestureState.DRAGGING_V && vdrag)
			{
				// Vertical Dragging!
				gestures.Enqueue(new GestureSample(
					new Vector2(0, delta.Y),
					Vector2.Zero,
					GestureType.VerticalDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}
			else if (gState == GestureState.DRAGGING_FREE && fdrag)
			{
				// Free Dragging!
				gestures.Enqueue(new GestureSample(
					delta,
					Vector2.Zero,
					GestureType.FreeDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}

			//TODO: Calculate touch velocity for flicking
		}

		/* This is called at the beginning of each game frame to check for
		 * Hold gestures. Since SDL doesn't fire events for stationary touches,
		 * this is the only way to be sure we notice when one second has passed.
		 */
		private static void CalculateGesture_OnUpdate(Vector2 touchPosition)
		{
			// Is the user holding their finger still?
			if (gState != GestureState.HOLDING)
			{
				return;
			}

			// Are Hold gestures enabled?
			if ((EnabledGestures & GestureType.Hold) != 0)
			{
				TimeSpan timeSinceTouchDown = DateTime.Now.Subtract(gTouchDownTime);
				if (timeSinceTouchDown >= TimeSpan.FromMilliseconds(1000))
				{
					// Hold!
					gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.Hold,
						touchPosition,
						Vector2.Zero,
						TimeSpan.FromTicks(DateTime.Now.Ticks)
					));

					gState = GestureState.HELD;
				}
			}
		}

		#endregion
	}
}
