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

		private static Queue<TouchLocation> touchEvents = new Queue<TouchLocation>();
		private static List<TouchLocation> touchesToRelease = new List<TouchLocation>();

		#endregion

		#region Private Constants

		// The maximum number of simultaneous touches allowed by XNA.
		private const int MAX_TOUCHES = 8;

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

		#region Internal Static Methods

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

			// Add the event to the queue
			touchEvents.Enqueue(new TouchLocation(
				fingerId,
				state,
				touchPos
			));

			// Use the event for gesture detection
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

		internal static bool IsGestureEnabled(GestureType gestureType)
		{
			return (EnabledGestures & gestureType) != 0;
		}

		/* This runs at the beginning of each game frame
		 * to assemble and update the list of active touches.
		 */
		internal static void UpdateTouches()
		{
			// Remove all touches that were released last frame
			touches.RemoveAll(touch => touch.State == TouchLocationState.Released);

			// Check for Hold gesture
			if (touches.Count > 0)
			{
				GestureDetector.OnUpdate(touches[0].Position);
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
			foreach (TouchLocation rtouch in touchesToRelease)
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
			touchesToRelease.Clear();

			// Process all new touch events
			while (touchEvents.Count > 0)
			{
				TouchLocation touch = touchEvents.Dequeue();

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
									if (!touchesToRelease.Contains(touches[i]))
									{
										touchesToRelease.Add(touches[i]);
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
