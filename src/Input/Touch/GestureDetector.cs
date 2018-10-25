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
	internal static class GestureDetector
	{
		#region Private Static Variables

		// The ID of the active finger
		private static int activeFingerId = -1;

		// The current position of the active finger
		private static Vector2 activeFingerPosition;

		/* In XNA, if the Pinch gesture was disabled mid-pinch,
		 * it would still dispatch a PinchComplete gesture once *all*
		 * fingers were off the screen. (Not just the ones involved
		 * in the pinch.) Kinda weird, right?
		 * 
		 * This flag is used to mimic that behavior.
		 */
		private static bool callBelatedPinchComplete = false;

		// The time when the most recent active Press/Release occurred
		private static DateTime eventTimestamp;

		// The IDs of all fingers currently on the screen
		private static List<int> fingerIds = new List<int>();

		// A flag to cancel Taps if a Double Tap has just occurred
		private static bool justDoubleTapped = false;

		// The position of the active finger at the last Update tick
		private static Vector2 lastUpdatePosition;

		// The position where the user first touched the screen
		private static Vector2 pressPosition;

		// The ID of the second finger (used only for Pinching)
		private static int secondFingerId = -1;

		// The current position of the second finger (used only for Pinching)
		private static Vector2 secondFingerPosition;

		// The current state of gesture detection
		private static GestureState state = GestureState.NONE;

		// The time of the most recent Update tick
		private static DateTime updateTimestamp;

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

		internal static void OnPressed(int fingerId, Vector2 touchPosition)
		{
			fingerIds.Add(fingerId);

			if (state == GestureState.PINCHING)
			{
				// None of this method applies to active pinches
				return;
			}

			// Set the active finger if there isn't one already
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
				activeFingerPosition = touchPosition;
			}
			else
			{
				#region Pinch Initialization

				if (IsGestureEnabled(GestureType.Pinch))
				{
					// Initialize a Pinch
					secondFingerId = fingerId;
					secondFingerPosition = touchPosition;

					state = GestureState.PINCHING;
				}

				#endregion

				// No need to do anything more
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
							TouchPanel.EnqueueGesture(new GestureSample(
								Vector2.Zero,
								Vector2.Zero,
								GestureType.DoubleTap,
								touchPosition,
								Vector2.Zero,
								GetGestureTimestamp()
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
			fingerIds.Remove(fingerId);

			// Handle release events seperately for Pinch gestures
			if (state == GestureState.PINCHING)
			{
				OnReleased_Pinch(fingerId, touchPosition);
				return;
			}

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
								TouchPanel.EnqueueGesture(new GestureSample(
									Vector2.Zero,
									Vector2.Zero,
									GestureType.Tap,
									touchPosition,
									Vector2.Zero,
									GetGestureTimestamp()
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
				// Only flick if the finger is outside the threshold and moving fast
				float distanceFromPress = (touchPosition - pressPosition).Length();
				if (distanceFromPress > MOVE_THRESHOLD &&
					velocity.Length() >= MIN_FLICK_VELOCITY)
				{
					// Flick!
					TouchPanel.EnqueueGesture(new GestureSample(
						velocity,
						Vector2.Zero,
						GestureType.Flick,
						Vector2.Zero,
						Vector2.Zero,
						GetGestureTimestamp()
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
					TouchPanel.EnqueueGesture(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.DragComplete,
						Vector2.Zero,
						Vector2.Zero,
						GetGestureTimestamp()
					));
				}
			}

			#endregion

			#region Belated Pinch Complete Detection

			if (callBelatedPinchComplete && IsGestureEnabled(GestureType.PinchComplete))
			{
				TouchPanel.EnqueueGesture(new GestureSample(
					Vector2.Zero,
					Vector2.Zero,
					GestureType.PinchComplete,
					Vector2.Zero,
					Vector2.Zero,
					GetGestureTimestamp()
				));
			}
			callBelatedPinchComplete = false;

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
			// Handle move events separately for Pinch gestures
			if (state == GestureState.PINCHING)
			{
				OnMoved_Pinch(fingerId, touchPosition, delta);
				return;
			}

			// Replace the active finger if we lost it
			if (activeFingerId == -1)
			{
				activeFingerId = fingerId;
			}

			// If this finger isn't the active finger
			if (fingerId != activeFingerId)
			{
				// We don't care about it
				return;
			}

			// Update the position
			activeFingerPosition = touchPosition;

			#region Prepare for Dragging

			// Determine which drag gestures are enabled
			bool hdrag = IsGestureEnabled(GestureType.HorizontalDrag);
			bool vdrag = IsGestureEnabled(GestureType.VerticalDrag);
			bool fdrag = IsGestureEnabled(GestureType.FreeDrag);

			if (state == GestureState.HOLDING || state == GestureState.HELD)
			{
				// Prevent accidental drags
				float distanceFromPress = (touchPosition - pressPosition).Length();
				if (distanceFromPress > MOVE_THRESHOLD)
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
				TouchPanel.EnqueueGesture(new GestureSample(
					new Vector2(delta.X, 0),
					Vector2.Zero,
					GestureType.HorizontalDrag,
					touchPosition,
					Vector2.Zero,
					GetGestureTimestamp()
				));
			}
			else if (state == GestureState.DRAGGING_V && vdrag)
			{
				// Vertical Dragging!
				TouchPanel.EnqueueGesture(new GestureSample(
					new Vector2(0, delta.Y),
					Vector2.Zero,
					GestureType.VerticalDrag,
					touchPosition,
					Vector2.Zero,
					GetGestureTimestamp()
				));
			}
			else if (state == GestureState.DRAGGING_FREE && fdrag)
			{
				// Free Dragging!
				TouchPanel.EnqueueGesture(new GestureSample(
					delta,
					Vector2.Zero,
					GestureType.FreeDrag,
					touchPosition,
					Vector2.Zero,
					GetGestureTimestamp()
				));
			}

			#endregion

			#region Handle Disabled Drags

			/* Handle the case where the current drag type
			 * was disabled *while* the user was dragging.
			 */
			if ((state == GestureState.DRAGGING_H && !hdrag) ||
				(state == GestureState.DRAGGING_V && !vdrag) ||
				(state == GestureState.DRAGGING_FREE && !fdrag))
			{
				// Reset the state
				state = GestureState.HELD;
			}

			#endregion
		}

		internal static void OnTick()
		{
			if (state == GestureState.PINCHING)
			{
				/* Handle the case where the Pinch gesture
				 * was disabled *while* the user was pinching.
				 */
				if (!IsGestureEnabled(GestureType.Pinch))
				{
					state = GestureState.HELD;
					secondFingerId = -1;

					// Still might need to trigger a PinchComplete
					callBelatedPinchComplete = true;
				}

				// No pinches allowed in the rest of this method!
				return;
			}

			// Must have an active finger to proceed
			if (activeFingerId == -1)
			{
				return;
			}

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
					 * spike by an order of magnitude.
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
					Vector2 delta = activeFingerPosition - lastUpdatePosition;
					Vector2 instVelocity = delta / (0.001f + dt);
					velocity += (instVelocity - velocity) * 0.45f;
				}

				lastUpdatePosition = activeFingerPosition;
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
					TouchPanel.EnqueueGesture(new GestureSample(
						Vector2.Zero,
						Vector2.Zero,
						GestureType.Hold,
						activeFingerPosition,
						Vector2.Zero,
						GetGestureTimestamp()
					));

					state = GestureState.HELD;
				}
			}

			#endregion
		}

		#endregion

		#region Private Methods

		private static TimeSpan GetGestureTimestamp()
		{
			/* XNA calculates gesture timestamps from
			 * how long the device has been turned on.
			 */
			return TimeSpan.FromTicks(Environment.TickCount);
		}

		private static bool IsGestureEnabled(GestureType gestureType)
		{
			return (TouchPanel.EnabledGestures & gestureType) != 0;
		}

		/* The *_Pinch methods are separate from the standard event methods
		 * because they have to deal with multiple touches. It gets really
		 * messy and ugly if single-touch and multi-touch detection is all
		 * intermingled in the same methods.
		 */

		private static void OnReleased_Pinch(int fingerId, Vector2 touchPosition)
		{
			// We don't care about fingers that aren't part of the pinch
			if (fingerId != activeFingerId && fingerId != secondFingerId)
			{
				return;
			}

			if (IsGestureEnabled(GestureType.PinchComplete))
			{
				// Pinch Complete!
				TouchPanel.EnqueueGesture(new GestureSample(
					Vector2.Zero,
					Vector2.Zero,
					GestureType.PinchComplete,
					Vector2.Zero,
					Vector2.Zero,
					GetGestureTimestamp()
				));
			}

			// If we lost the active finger
			if (fingerId == activeFingerId)
			{
				// Then the second finger becomes the active finger
				activeFingerId = secondFingerId;
				activeFingerPosition = secondFingerPosition;
			}

			// Regardless, we no longer have a second finger
			secondFingerId = -1;

			// Attempt to replace our fallen comrade
			bool replacedSecondFinger = false;
			foreach (int id in fingerIds)
			{
				// Find a finger that's not already spoken for
				if (id != activeFingerId)
				{
					secondFingerId = id;
					replacedSecondFinger = true;
					break;
				}
			}

			if (!replacedSecondFinger)
			{
				// Aaaand we're back to a single touch
				state = GestureState.HELD;
			}
		}

		private static void OnMoved_Pinch(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// We only care if the finger moved is involved in the pinch
			if (fingerId != activeFingerId && fingerId != secondFingerId)
			{
				return;
			}

			/* In XNA, each Pinch gesture sample contained a delta
			 * for both fingers. It was somehow able to detect
			 * simultaneous deltas at an OS level. We don't have that
			 * luxury, so instead, each Pinch gesture will contain the
			 * delta information for just _one_ of the fingers.
			 * 
			 * In practice what this means is that you'll get twice as
			 * many Pinch gestures added to the queue (one sample for
			 * each finger). This doesn't matter too much, though,
			 * since the resulting behavior is identical to XNA.
			 * 
			 * -caleb
			 */

			if (fingerId == activeFingerId)
			{
				activeFingerPosition = touchPosition;
				TouchPanel.EnqueueGesture(new GestureSample(
					delta,
					Vector2.Zero,
					GestureType.Pinch,
					activeFingerPosition,
					secondFingerPosition,
					GetGestureTimestamp()
				));
			}
			else
			{
				secondFingerPosition = touchPosition;
				TouchPanel.EnqueueGesture(new GestureSample(
					Vector2.Zero,
					delta,
					GestureType.Pinch,
					activeFingerPosition,
					secondFingerPosition,
					GetGestureTimestamp()
				));
			}
		}

		#endregion
	}
}
