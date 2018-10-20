using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.Input.Touch
{
	internal static class GestureDetector
	{
		#region Private Static Variables

		// The ID of the active finger (usually the first finger)
		private static int activeFingerId = -1;

		// The position where the user first touched the screen
		private static Vector2 pressPosition;

		// The time when the user first touched the screen
		private static DateTime pressTimestamp;

		// The time when the user released all fingers from the screen
		private static DateTime releaseTimestamp;

		// The most recent time when the user moved the active finger
		private static DateTime moveTimestamp;

		// The current state of gesture detection
		private static GestureState state = GestureState.NONE;

		// A flag to cancel Taps if a double tap has just occurred
		private static bool justDoubleTapped = false;

		// The current velocity of the active finger
		private static Vector2 velocity;

		#endregion

		#region Private Constants

		/* How far (in pixels) the user can move their finger in a gesture
		 * before it counts as "moved". This prevents small, accidental
		 * finger movements from interfering with Hold and Tap gestures.
		 */
		private const int MOVE_THRESHOLD = 35;

		#endregion

		#region Private Enums

		// All possible states of Gesture detection.
		private enum GestureState
		{
			NONE,
			HOLDING,
			HELD,           /* Same as HOLDING, but after a Hold gesture has fired */
			JUST_TAPPED,
			DRAGGING_FREE,
			DRAGGING_H,
			DRAGGING_V,
			PINCHING
		};

		#endregion

		#region Internal Methods

		/* Called when SDL detects a FingerDown event.
		 * This detects the first active finger, prepares for
		 * Taps and Holds, and triggers Double Tap gestures.
		 */
		internal static void OnPressed(int fingerId, Vector2 touchPosition)
		{
			// Set the active finger if there isn't one already
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
			}
			else
			{
				// We don't care about other fingers
				return;
			}

			// Handle Double Tap gestures
			if (state == GestureState.JUST_TAPPED)
			{
				if (TouchPanel.IsGestureEnabled(GestureType.DoubleTap))
				{
					// Must tap again within 300ms of original tap's release
					TimeSpan timeSinceRelease = DateTime.Now - releaseTimestamp;
					if (timeSinceRelease <= TimeSpan.FromMilliseconds(300))
					{
						// If the new tap is close to the original tap
						float distance = (touchPosition - pressPosition).Length();
						if (distance <= MOVE_THRESHOLD)
						{
							// Double Tap!
							TouchPanel.gestures.Enqueue(new GestureSample(
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

			state = GestureState.HOLDING;
			pressPosition = touchPosition;
			pressTimestamp = DateTime.Now;
		}

		/* This is called when SDL detects a FingerUp event.
		 * It's responsible for resetting the active finger
		 * and firing Tap and Drag Complete gestures.
		 */
		internal static void OnReleased(int fingerId, Vector2 touchPosition)
		{
			// Did the user lift the active finger?
			if (fingerId == activeFingerId)
			{
				activeFingerId = -1;
			}

			// We're only interested in the very last finger to leave
			if (FNAPlatform.GetNumTouchFingers() > 0)
			{
				return;
			}

			// Set the timestamp
			releaseTimestamp = DateTime.Now;

			// Check for Tap gesture
			if (state == GestureState.HOLDING)
			{
				// Which Tap gestures are enabled?
				bool tapEnabled = TouchPanel.IsGestureEnabled(GestureType.Tap);
				bool dtapEnabled = TouchPanel.IsGestureEnabled(GestureType.DoubleTap);

				if (tapEnabled || dtapEnabled)
				{
					// How long did the user hold the touch?
					TimeSpan timeHeld = DateTime.Now - pressTimestamp;
					if (timeHeld < TimeSpan.FromSeconds(1))
					{
						// Don't register a Tap immediately after a Double Tap
						if (!justDoubleTapped)
						{
							if (tapEnabled)
							{
								// Tap!
								TouchPanel.gestures.Enqueue(new GestureSample(
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
							state = GestureState.JUST_TAPPED;
						}
					}
				}
			}

			// Reset this flag so we can catch Taps in the future
			justDoubleTapped = false;

			// Check for Drag Complete gesture
			bool wasDragging = (state == GestureState.DRAGGING_H ||
								state == GestureState.DRAGGING_V ||
								state == GestureState.DRAGGING_FREE);

			if (wasDragging)
			{
				if (TouchPanel.IsGestureEnabled(GestureType.DragComplete))
				{
					// Drag Complete!
					TouchPanel.gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.DragComplete,
						Vector2.Zero,
						Vector2.Zero,
						TimeSpan.FromTicks(DateTime.Now.Ticks)
					));
				}
			}

			// Reset the state if we're not anticipating a Double Tap
			if (state != GestureState.JUST_TAPPED)
			{
				state = GestureState.NONE;
			}

			// Handle Flicks
			if (TouchPanel.IsGestureEnabled(GestureType.Flick))
			{
				//TODO: If distance from original touch > MAX_THRESHOLD
				//TODO: And if length of flick is long enough
				Console.WriteLine(velocity);
				TouchPanel.gestures.Enqueue(new GestureSample(
					velocity,
					Vector2.Zero,
					GestureType.Flick,
					Vector2.Zero,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}

			// Cancel out any accumulated flick velocity
			velocity = Vector2.Zero;
		}

		/* This is called whenever SDL detects a FingerMotion event.
		 * Its primary purpose is to notice Drag gestures and cancel
		 * Hold/Tap gestures if the user moves their finger too much.
		 */
		internal static void OnMoved(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// If we lost the active finger
			if (activeFingerId == -1)
			{
				// This is our new active finger!
				activeFingerId = fingerId;
			}
			// If this finger is not the active one
			else if (fingerId != activeFingerId)
			{
				// Ignore the imposter!
				//TODO: Pinching goes here I think
				return;
			}

			// Determine which drag gestures are enabled
			bool hdrag = TouchPanel.IsGestureEnabled(GestureType.HorizontalDrag);
			bool vdrag = TouchPanel.IsGestureEnabled(GestureType.VerticalDrag);
			bool fdrag = TouchPanel.IsGestureEnabled(GestureType.FreeDrag);

			// Get the distance of the finger from its original position
			float distanceMoved = (touchPosition - pressPosition).Length();

			// Initialize Drags
			if (state == GestureState.HOLDING || state == GestureState.HELD)
			{
				if (distanceMoved > MOVE_THRESHOLD)
				{
					if (hdrag && (Math.Abs(delta.X) > Math.Abs(delta.Y)))
					{
						// Horizontal Drag!
						state = GestureState.DRAGGING_H;
					}
					else if (vdrag && (Math.Abs(delta.Y) > Math.Abs(delta.X)))
					{
						// Vertical Drag!
						state = GestureState.DRAGGING_V;
					}
					else if (fdrag)
					{
						// Free Drag!
						state = GestureState.DRAGGING_FREE;
					}
					else
					{
						// No drag...
						state = GestureState.NONE;
					}
				}
			}

			// Handle Dragging
			if (state == GestureState.DRAGGING_H && hdrag)
			{
				// Horizontal Dragging!
				TouchPanel.gestures.Enqueue(new GestureSample(
					new Vector2(delta.X, 0),
					Vector2.Zero,
					GestureType.HorizontalDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}
			else if (state == GestureState.DRAGGING_V && vdrag)
			{
				// Vertical Dragging!
				TouchPanel.gestures.Enqueue(new GestureSample(
					new Vector2(0, delta.Y),
					Vector2.Zero,
					GestureType.VerticalDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}
			else if (state == GestureState.DRAGGING_FREE && fdrag)
			{
				// Free Dragging!
				TouchPanel.gestures.Enqueue(new GestureSample(
					delta,
					Vector2.Zero,
					GestureType.FreeDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(DateTime.Now.Ticks)
				));
			}

			// Update the flick velocity
			DateTime now = DateTime.Now;
			TimeSpan dt = now - moveTimestamp;

			//TODO Flicking calculation

			moveTimestamp = now;
		}

		/* This is called at the beginning of each game frame to check for
		 * Hold gestures. Since SDL doesn't fire events for stationary touches,
		 * this is the only way to be sure we notice when one second has passed.
		 */
		internal static void OnUpdate(Vector2 touchPosition)
		{
			// Only proceed if the user is holding their finger still
			if (state != GestureState.HOLDING)
			{
				return;
			}

			if (TouchPanel.IsGestureEnabled(GestureType.Hold))
			{
				TimeSpan timeSincePress = DateTime.Now - pressTimestamp;
				if (timeSincePress >= TimeSpan.FromSeconds(1))
				{
					// Hold!
					TouchPanel.gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.Hold,
						touchPosition,
						Vector2.Zero,
						TimeSpan.FromTicks(DateTime.Now.Ticks)
					));

					state = GestureState.HELD;
				}
			}
		}

		#endregion
	}
}
