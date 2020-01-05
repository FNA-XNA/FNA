#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public static class GamePad
	{
		#region Internal Constants

		/* Based on the XInput constants */
		internal const float LeftDeadZone = 7849.0f / 32768.0f;
		internal const float RightDeadZone = 8689.0f / 32768.0f;
		internal const float TriggerThreshold = 30.0f / 255.0f;

		#endregion

		#region Internal Static Variables

		/* Determines how many controllers we should be tracking.
		 * Per XNA4 we track 4 by default, but if you want to track more you can
		 * do this by changing PlayerIndex.cs to include more index names.
		 * -flibit
		 */
		internal static readonly int GAMEPAD_COUNT = DetermineNumGamepads();

		private static int DetermineNumGamepads()
		{
			string numGamepadString = Environment.GetEnvironmentVariable(
				"FNA_GAMEPAD_NUM_GAMEPADS"
			);
			if (!String.IsNullOrEmpty(numGamepadString))
			{
				int numGamepads;
				if (int.TryParse(numGamepadString, out numGamepads))
				{
					if (numGamepads >= 0)
					{
						return numGamepads;
					}
				}
			}
			return Enum.GetNames(typeof(PlayerIndex)).Length;
		}

		#endregion

		#region Public GamePad API

		public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
		{
			return FNAPlatform.GetGamePadCapabilities((int) playerIndex);
		}

		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			return FNAPlatform.GetGamePadState(
				(int) playerIndex,
				GamePadDeadZone.IndependentAxes
			);
		}

		public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
		{
			return FNAPlatform.GetGamePadState(
				(int) playerIndex,
				deadZoneMode
			);
		}

		public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
		{
			return FNAPlatform.SetGamePadVibration(
				(int) playerIndex,
				leftMotor,
				rightMotor
			);
		}

		#endregion

		#region Public GamePad API, FNA Extensions

		public static string GetGUIDEXT(PlayerIndex playerIndex)
		{
			return FNAPlatform.GetGamePadGUID((int) playerIndex);
		}

		public static void SetLightBarEXT(PlayerIndex playerIndex, Color color)
		{
			FNAPlatform.SetGamePadLightBar((int) playerIndex, color);
		}

		#endregion

		#region Internal Static Methods

		internal static float ExcludeAxisDeadZone(float value, float deadZone)
		{
			if (value < -deadZone)
			{
				value += deadZone;
			}
			else if (value > deadZone)
			{
				value -= deadZone;
			}
			else
			{
				return 0.0f;
			}
			return value / (1.0f - deadZone);
		}

		#endregion
	}
}
