#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
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
	public struct GamePadThumbSticks
	{
		#region Public Properties

		public Vector2 Left
		{
			get
			{
				return left;
			}
		}
		public Vector2 Right
		{
			get
			{
				return right;
			}
		}

		#endregion

		#region Private Variables

		private Vector2 left;
		private Vector2 right;

		#endregion

		#region Public Constructor

		public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition)
		{
			left = leftPosition;
			right = rightPosition;
			ApplySquareClamp();
		}

		#endregion

		#region Internal Constructor

		internal GamePadThumbSticks(
			Vector2 leftPosition,
			Vector2 rightPosition,
			GamePadDeadZone deadZoneMode
		) {
			/* XNA applies dead zones before rounding/clamping values.
			 * The public constructor does not allow this because the
			 * dead zone must be known first.
			 */
			left = leftPosition;
			right = rightPosition;
			ApplyDeadZone(deadZoneMode);
			if (deadZoneMode == GamePadDeadZone.Circular)
			{
				ApplyCircularClamp();
			}
			else
			{
				ApplySquareClamp();
			}
		}

		#endregion

		#region Private Methods

		private void ApplyDeadZone(GamePadDeadZone dz)
		{
			switch (dz)
			{
				case GamePadDeadZone.None:
					break;
				case GamePadDeadZone.IndependentAxes:
					left.X = GamePad.ExcludeAxisDeadZone(left.X, GamePad.LeftDeadZone);
					left.Y = GamePad.ExcludeAxisDeadZone(left.Y, GamePad.LeftDeadZone);
					right.X = GamePad.ExcludeAxisDeadZone(right.X, GamePad.RightDeadZone);
					right.Y = GamePad.ExcludeAxisDeadZone(right.Y, GamePad.RightDeadZone);
					break;
				case GamePadDeadZone.Circular:
					left = ExcludeCircularDeadZone(left, GamePad.LeftDeadZone);
					right = ExcludeCircularDeadZone(right, GamePad.RightDeadZone);
					break;
			}
		}

		private void ApplySquareClamp()
		{
			left.X = MathHelper.Clamp(left.X, -1.0f, 1.0f);
			left.Y = MathHelper.Clamp(left.Y, -1.0f, 1.0f);
			right.X = MathHelper.Clamp(right.X, -1.0f, 1.0f);
			right.Y = MathHelper.Clamp(right.Y, -1.0f, 1.0f);
		}

		private void ApplyCircularClamp()
		{
			if (left.LengthSquared() > 1.0f)
			{
				left.Normalize();
			}
			if (right.LengthSquared() > 1.0f)
			{
				right.Normalize();
			}
		}

		#endregion

		#region Private Static Methods

		private static Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
		{
			float originalLength = value.Length();
			if (originalLength <= deadZone)
			{
				return Vector2.Zero;
			}
			float newLength = (originalLength - deadZone) / (1.0f - deadZone);
			return value * (newLength / originalLength);
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadThumbSticks"/>
		/// are equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator ==(GamePadThumbSticks left, GamePadThumbSticks right)
		{
			return (left.left == right.left) && (left.right == right.right);
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadThumbSticks"/>
		/// are not equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator !=(GamePadThumbSticks left, GamePadThumbSticks right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a <see cref="GamePadThumbSticks"/> and has the
		/// same value as this instance; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is GamePadThumbSticks) && (this == (GamePadThumbSticks) obj);
		}

		public override int GetHashCode()
		{
			return this.Left.GetHashCode() + 37 * this.Right.GetHashCode();
		}

		#endregion
	}
}
