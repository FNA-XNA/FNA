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

		internal static Queue<TouchLocation> detectedTouches = new Queue<TouchLocation>();

		#endregion

		#region Private Constants

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

		#region Internal Methods

		internal static void INTERNAL_onFingerDown(
			int fingerId,
			float x,
			float y,
			uint timestamp
		) {
			detectedTouches.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Pressed,
				new Vector2(
					x * DisplayWidth,
					y * DisplayHeight
				)
			));
		}

		internal static void INTERNAL_onFingerUp(
			int fingerId,
			float x,
			float y,
			uint timestamp
		) {
			detectedTouches.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Released,
				new Vector2(
					x * DisplayWidth,
					y * DisplayHeight
				)
			));
		}

		internal static void INTERNAL_onFingerMotion(
			int fingerId,
			float x,
			float y,
			float dx,
			float dy,
			uint timestamp
		) {
			detectedTouches.Enqueue(new TouchLocation(
				fingerId,
				TouchLocationState.Moved,
				new Vector2(
					x * DisplayWidth,
					y * DisplayHeight
				)
			));
		}

		internal static void UpdateTouches()
		{
			// Update all previously Pressed touches to become Moved
			for (int i = 0; i < touches.Count; i += 1)
			{
				if (touches[i].State == TouchLocationState.Pressed)
				{
					touches[i] = new TouchLocation(
						touches[i].Id,
						TouchLocationState.Moved,
						touches[i].Position,
						TouchLocationState.Pressed,
						touches[i].Position
					);
				}
			}

			// Remove all previously Released touches
			touches.RemoveAll(touch => touch.State == TouchLocationState.Released);

			// Process new touches
			while (detectedTouches.Count > 0)
			{
				// Get the next available detected touch location
				TouchLocation dtouch = detectedTouches.Dequeue();

				/* If it's a new touch (has the Pressed state)
				 * and we have room for it, add it to the list.
				 */
				if (dtouch.State == TouchLocationState.Pressed
					&& touches.Count < MAX_TOUCHES)
				{
					touches.Add(dtouch);
				}
				else
				{
					// Update existing touches
					for (int i = 0; i < touches.Count; i += 1)
					{
						if (dtouch.Id == touches[i].Id)
						{
							/* Don't change states from Pressed to Moved unless we do it manually.
							 * Otherwise the touch will never be registered as Pressed.
							 */
							if (!(touches[i].State == TouchLocationState.Pressed
								   && dtouch.State == TouchLocationState.Moved))
							{
								touches[i] = new TouchLocation(
									dtouch.Id,
									dtouch.State,
									dtouch.Position,
									touches[i].State,
									touches[i].Position
								);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
