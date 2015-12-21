#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadTriggers
	{
		#region Public Properties

		public float Left
		{
			get
			{
				return left;
			}
			internal set
			{
				left = MathHelper.Clamp(value, 0f, 1f);
			}
		}
		public float Right
		{
			get
			{
				return right;
			}
			internal set
			{
				right = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		#endregion

		#region Private Variables

		private float left;
		private float right;

		#endregion

		#region Public Constructor

		public GamePadTriggers(float leftTrigger, float rightTrigger):this()
		{
			Left = leftTrigger;
			Right = rightTrigger;
		}

		#endregion

		#region Public Static Operators and Override Methods

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadTriggers"/> are
		/// equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator ==(GamePadTriggers left, GamePadTriggers right)
		{
			return (	(MathHelper.WithinEpsilon(left.left, right.left)) &&
					(MathHelper.WithinEpsilon(left.right, right.right))	);
		}

		/// <summary>
		/// Determines whether two specified instances of <see cref="GamePadTriggers"/> are
		/// not equal.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns>
		/// True if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise, false.
		/// </returns>
		public static bool operator !=(GamePadTriggers left, GamePadTriggers right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a <see cref="GamePadTriggers"/> and has the
		/// same value as this instance; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return (obj is GamePadTriggers) && (this == (GamePadTriggers) obj);
		}

		public override int GetHashCode ()
		{
			return this.Left.GetHashCode() + this.Right.GetHashCode();
		}

		#endregion
	}
}
