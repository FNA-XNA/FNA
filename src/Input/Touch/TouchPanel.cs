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
		private static GestureState gState = GestureState.NONE;
		
		#endregion

		#region Private Constants

		private const int MAX_TOUCHES = 8;
		private const int MOVE_THRESHOLD = 35;

		private enum GestureState
		{
			NONE,
			HOLDING,
			JUST_TAPPED,
			JUST_DOUBLETAPPED,
			DRAGGING,
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
					CalculateGesture_FingerDown(touchPos);
					break;

				case TouchLocationState.Moved:
					Vector2 delta = new Vector2(
						(float) Math.Round(dx * DisplayWidth),
						(float) Math.Round(dy * DisplayHeight)
					);
					CalculateGesture_FingerMoved(touchPos, delta);
					break;

				case TouchLocationState.Released:
					CalculateGesture_FingerUp(touchPos);
					break;
			}
			
		}

		internal static void CalculateGesture_FingerDown(Vector2 touchPosition)
		{
			// Is this the first finger on the screen?
			if (FNAPlatform.GetNumTouchFingers() <= 1)
			{
				if (gState == GestureState.JUST_TAPPED)
				{
					// Handle Double Tap gestures (if they're enabled)
					if ((EnabledGestures & GestureType.DoubleTap) != 0)
					{
						TimeSpan timeBetweenTaps = DateTime.Now.Subtract(gReleaseTime);
						if (timeBetweenTaps <= TimeSpan.FromMilliseconds(300))
						{
							float distance = (touchPosition - gTouchDownPosition).Length();
							if (distance <= MOVE_THRESHOLD)
							{
								Console.WriteLine("DOUBLE TAP");
								gState = GestureState.JUST_DOUBLETAPPED;
							}
						}
					}
				}

				if (gState != GestureState.JUST_DOUBLETAPPED)
				{
					gState = GestureState.HOLDING;
				}

				// Store the time and position the user touched down
				gTouchDownTime = DateTime.Now;
				gTouchDownPosition = touchPosition;
			}
		}

		internal static void CalculateGesture_FingerUp(Vector2 touchPosition)
		{
			// Was this the last finger to lift?
			if (FNAPlatform.GetNumTouchFingers() == 0)
			{
				if (gState == GestureState.HOLDING)
				{
					// Handle Tap gestures
					bool tapEnabled = (EnabledGestures & GestureType.Tap) != 0;
					bool dtapEnabled = (EnabledGestures & GestureType.DoubleTap) != 0;

					if (tapEnabled || dtapEnabled)
					{
						TimeSpan timeHeld = DateTime.Now.Subtract(gTouchDownTime);
						if (timeHeld < TimeSpan.FromMilliseconds(1000))
						{
							// Don't register a Tap immediately after a Double Tap
							if (gState != GestureState.JUST_DOUBLETAPPED)
							{
								if (tapEnabled)
								{
									Console.WriteLine("TAP");
								}

								/* Even if Tap isn't enabled, we still
								 * need this for Double Tap detection.
								 */
								gState = GestureState.JUST_TAPPED;
							}
						}
					}
				}

				if (gState != GestureState.JUST_TAPPED)
				{
					gState = GestureState.NONE;
				}

				// Store the time the finger was released
				gReleaseTime = DateTime.Now;
			}
		}

		internal static void CalculateGesture_FingerMoved(
			Vector2 touchPosition,
			Vector2 delta
		) {
			if (gState == GestureState.HOLDING)
			{
				float distanceMoved = (touchPosition - gTouchDownPosition).Length();
				if (distanceMoved > MOVE_THRESHOLD)
				{
					// Moved too far away to be a Hold...is it a drag?

					bool hdrag = (EnabledGestures & GestureType.HorizontalDrag) != 0;
					bool vdrag = (EnabledGestures & GestureType.VerticalDrag) != 0;
					bool fdrag = (EnabledGestures & GestureType.FreeDrag) != 0;

					if (hdrag && Math.Abs(delta.X) > Math.Abs(delta.Y))
					{
						gState = GestureState.DRAGGING_H;
						Console.WriteLine("DRAGGING HORIZONTALLY");
					}
					else if (vdrag && Math.Abs(delta.Y) > Math.Abs(delta.X))
					{
						gState = GestureState.DRAGGING_V;
						Console.WriteLine("DRAGGING VERTICALLY");
					}
					else
					{
						gState = GestureState.DRAGGING;
						
						if (fdrag) //TODO: Is this right?
						{
							Console.WriteLine("DRAGGING FREE");
						}
					}
				}
			}
		}

		internal static void CalculateGesture_OnUpdate(Vector2 touchPosition)
		{
			if (gState == GestureState.HOLDING)
			{
				// Handle Hold gestures (if they're enabled)
				if ((EnabledGestures & GestureType.Hold) != 0)
				{
					TimeSpan timeSinceTouchDown = DateTime.Now.Subtract(gTouchDownTime);
					if (timeSinceTouchDown >= TimeSpan.FromMilliseconds(1000))
					{
						Console.WriteLine("HOLD");
						gState = GestureState.NONE;
					}
				}
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
	}
}
