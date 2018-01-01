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
#endregion

namespace Microsoft.Xna.Framework.Input.Touch
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchlocation.aspx
	public struct TouchLocation : IEquatable<TouchLocation>
	{
		#region Public Properties

		public int Id
		{
			get;
			private set;
		}

		public Vector2 Position
		{
			get;
			private set;
		}

		public TouchLocationState State
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private Vector2 prevPosition;
		private TouchLocationState prevState;

		#endregion

		#region Public Constructors

		public TouchLocation(
			int id,
			TouchLocationState state,
			Vector2 position
		) : this() {
			Id = id;
			State = state;
			Position = position;
			prevPosition = Vector2.Zero;
			prevState = TouchLocationState.Invalid;
		}

		public TouchLocation(
			int id,
			TouchLocationState state,
			Vector2 position,
			TouchLocationState previousState,
			Vector2 previousPosition
		) : this() {
			Id = id;
			State = state;
			Position = position;
			prevState = previousState;
			prevPosition = previousPosition;
		}

		#endregion

		#region Public Methods

		public bool Equals(TouchLocation other)
		{
			return (	Id == other.Id &&
					Position == other.Position &&
					State == other.State &&
					prevPosition == other.prevPosition &&
					prevState == other.prevState	);
		}

		public override bool Equals(object obj)
		{
			return (obj is TouchLocation) && Equals((TouchLocation) obj);
		}

		public override int GetHashCode()
		{
			return Id; // FIXME: What is this really...?
		}

		public override string ToString()
		{
			return Id.ToString(); // FIXME: What is this really...?
		}

		public bool TryGetPreviousLocation(
			out TouchLocation previousLocation
		) {
			previousLocation = new TouchLocation(
				Id,
				prevState,
				prevPosition
			);
			return previousLocation.State != TouchLocationState.Invalid;
		}

		#endregion

		#region Public Static Operator Overloads

		public static bool operator==(
			TouchLocation value1,
			TouchLocation value2
		) {
			return value1.Equals(value2);
		}

		public static bool operator!=(
			TouchLocation value1,
			TouchLocation value2
		) {
			return !value1.Equals(value2);
		}

		#endregion
	}
}
