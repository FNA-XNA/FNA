using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.Input.Touch
{
	internal static class GestureDetector
	{
		#region Private Static Variables

		// The ID of the active finger
		private static int activeFingerId = -1;

		// The position where the user first touched the screen
		private static Vector2 pressPosition;

		// The time when the most recent active Press/Release occurred
		private static DateTime eventTimestamp;

		// The position of the active finger at the last Update tick
		private static Vector2 lastUpdatePosition;

		// The time of the most recent Update tick
		private static DateTime updateTimestamp;

		// The current state of gesture detection
		private static GestureState state = GestureState.NONE;

		// A flag to cancel Taps if a Double Tap has just occurred
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

		/* How fast the finger velocity must be to register as a Flick.
		 * This helps prevent accidental flicks when a drag or tap was
		 * intended.
		 */
		private const int MIN_FLICK_VELOCITY = 100;

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

		internal static bool IsGestureEnabled(GestureType gestureType)
		{
			return (TouchPanel.EnabledGestures & gestureType) != 0;
		}

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

			#region Double Tap Detection

			if (state == GestureState.JUST_TAPPED)
			{
				if (IsGestureEnabled(GestureType.DoubleTap))
				{
					// Must tap again within 300ms of original tap's release
					TimeSpan timeSinceRelease = DateTime.Now - eventTimestamp;
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
								TimeSpan.FromTicks(Environment.TickCount)
							));

							justDoubleTapped = true;
						}
					}
				}
			}

			#endregion

			state = GestureState.HOLDING;
			pressPosition = touchPosition;
			eventTimestamp = DateTime.Now;
		}

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

			#region Tap Detection

			if (state == GestureState.HOLDING)
			{
				// Which Tap gestures are enabled?
				bool tapEnabled = IsGestureEnabled(GestureType.Tap);
				bool dtapEnabled = IsGestureEnabled(GestureType.DoubleTap);

				if (tapEnabled || dtapEnabled)
				{
					// How long did the user hold the touch?
					TimeSpan timeHeld = DateTime.Now - eventTimestamp;
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
									TimeSpan.FromTicks(Environment.TickCount)
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

			#endregion

			#region Flick Detection

			if (IsGestureEnabled(GestureType.Flick))
			{
				float distanceFromPress = (touchPosition - pressPosition).Length();
				if (distanceFromPress > MOVE_THRESHOLD &&
					velocity.Length() >= MIN_FLICK_VELOCITY)
				{
					// Flick!
					TouchPanel.gestures.Enqueue(new GestureSample(
						velocity,
						Vector2.Zero,
						GestureType.Flick,
						Vector2.Zero,
						Vector2.Zero,
						TimeSpan.FromTicks(Environment.TickCount)
					));
				}

				// Reset velocity calculation variables
				velocity = Vector2.Zero;
				lastUpdatePosition = Vector2.Zero;
				updateTimestamp = DateTime.MinValue;
			}

			#endregion

			#region Drag Complete Detection

			if (IsGestureEnabled(GestureType.DragComplete))
			{
				bool wasDragging = (state == GestureState.DRAGGING_H ||
									state == GestureState.DRAGGING_V ||
									state == GestureState.DRAGGING_FREE);
				if (wasDragging)
				{
					// Drag Complete!
					TouchPanel.gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.DragComplete,
						Vector2.Zero,
						Vector2.Zero,
						TimeSpan.FromTicks(Environment.TickCount)
					));
				}
			}

			#endregion

			// Reset the state if we're not anticipating a Double Tap
			if (state != GestureState.JUST_TAPPED)
			{
				state = GestureState.NONE;
			}

			eventTimestamp = DateTime.Now;
		}

		internal static void OnMoved(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// Replace the active finger if we lost it
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
			}
			else if (fingerId != activeFingerId)
			{
				// Ignore the imposter!
				//TODO: Pinching goes here I think
				return;
			}

			#region Prepare for Dragging

			// Determine which drag gestures are enabled
			bool hdrag = IsGestureEnabled(GestureType.HorizontalDrag);
			bool vdrag = IsGestureEnabled(GestureType.VerticalDrag);
			bool fdrag = IsGestureEnabled(GestureType.FreeDrag);

			// Get the distance of the finger from its original position
			float distanceMoved = (touchPosition - pressPosition).Length();

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

			#endregion

			#region Drag Detection

			if (state == GestureState.DRAGGING_H && hdrag)
			{
				// Horizontal Dragging!
				TouchPanel.gestures.Enqueue(new GestureSample(
					new Vector2(delta.X, 0),
					Vector2.Zero,
					GestureType.HorizontalDrag,
					touchPosition,
					Vector2.Zero,
					TimeSpan.FromTicks(Environment.TickCount)
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
					TimeSpan.FromTicks(Environment.TickCount)
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
					TimeSpan.FromTicks(Environment.TickCount)
				));
			}

			#endregion
		}

		internal static void OnTick()
		{
			// Only proceed if the user has at least one finger on the screen
			if (TouchPanel.touches.Count == 0)
			{
				return;
			}

			TouchLocation touch = TouchPanel.touches[0];

			#region Flick Velocity Calculation

			if (IsGestureEnabled(GestureType.Flick))
			{
				// We need one frame to pass so we can calculate delta time
				if (updateTimestamp != DateTime.MinValue)
				{
					/* The calculation below is mostly taken from MonoGame.
					 * It accumulates velocity after running it through
					 * a low-pass filter to mitigate the effect of
					 * acceleration spikes. This works pretty well,
					 * but on rare occasions the velocity will still
					 * spike severely.
					 * 
					 * In practice this tends to be a non-issue, but
					 * if you *really* need to avoid any spikes, you
					 * may want to consider normalizing the delta
					 * reported in the GestureSample and then scaling it
					 * to min(actualVectorLength, preferredMaxLength).
					 * 
					 * -caleb
					 */

					float dt = (float)(DateTime.Now - updateTimestamp).TotalSeconds;
					Vector2 delta = touch.Position - lastUpdatePosition;
					Vector2 instVelocity = delta / (0.001f + dt);
					velocity += (instVelocity - velocity) * 0.45f;
				}

				lastUpdatePosition = touch.Position;
				updateTimestamp = DateTime.Now;
			}

			#endregion

			#region Hold Detection

			if (IsGestureEnabled(GestureType.Hold) && state == GestureState.HOLDING)
			{
				TimeSpan timeSincePress = DateTime.Now - eventTimestamp;
				if (timeSincePress >= TimeSpan.FromSeconds(1))
				{
					// Hold!
					TouchPanel.gestures.Enqueue(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.Hold,
						touch.Position,
						Vector2.Zero,
						TimeSpan.FromTicks(Environment.TickCount)
					));

					state = GestureState.HELD;
				}
			}

			#endregion
		}

		#endregion
	}
}
