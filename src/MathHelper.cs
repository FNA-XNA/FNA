#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework
{
	/// <summary>
	/// Contains commonly used precalculated values and mathematical operations.
	/// </summary>
	public static class MathHelper
	{
		#region Public Constants

		/// <summary>
		/// Represents the mathematical constant e(2.71828175).
		/// </summary>
		public const float E = (float) Math.E;

		/// <summary>
		/// Represents the log base ten of e(0.4342945).
		/// </summary>
		public const float Log10E = 0.4342945f;

		/// <summary>
		/// Represents the log base two of e(1.442695).
		/// </summary>
		public const float Log2E = 1.442695f;

		/// <summary>
		/// Represents the value of pi(3.14159274).
		/// </summary>
		public const float Pi = (float) Math.PI;

		/// <summary>
		/// Represents the value of pi divided by two(1.57079637).
		/// </summary>
		public const float PiOver2 = (float) (Math.PI / 2.0);

		/// <summary>
		/// Represents the value of pi divided by four(0.7853982).
		/// </summary>
		public const float PiOver4 = (float) (Math.PI / 4.0);

		/// <summary>
		/// Represents the value of pi times two(6.28318548).
		/// </summary>
		public const float TwoPi = (float) (Math.PI * 2.0);

		#endregion

		#region Internal Static Readonly Fields

		internal static readonly float MachineEpsilonFloat = GetMachineEpsilonFloat();

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns the Cartesian coordinate for one axis of a point that is defined by a
		/// given triangle and two normalized barycentric (areal) coordinates.
		/// </summary>
		/// <param name="value1">
		/// The coordinate on one axis of vertex 1 of the defining triangle.
		/// </param>
		/// <param name="value2">
		/// The coordinate on the same axis of vertex 2 of the defining triangle.
		/// </param>
		/// <param name="value3">
		/// The coordinate on the same axis of vertex 3 of the defining triangle.
		/// </param>
		/// <param name="amount1">
		/// The normalized barycentric (areal) coordinate b2, equal to the weighting factor
		/// for vertex 2, the coordinate of which is specified in value2.
		/// </param>
		/// <param name="amount2">
		/// The normalized barycentric (areal) coordinate b3, equal to the weighting factor
		/// for vertex 3, the coordinate of which is specified in value3.
		/// </param>
		/// <returns>
		/// Cartesian coordinate of the specified point with respect to the axis being used.
		/// </returns>
		public static float Barycentric(
			float value1,
			float value2,
			float value3,
			float amount1,
			float amount2
		) {
			return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
		}

		/// <summary>
		/// Performs a Catmull-Rom interpolation using the specified positions.
		/// </summary>
		/// <param name="value1">The first position in the interpolation.</param>
		/// <param name="value2">The second position in the interpolation.</param>
		/// <param name="value3">The third position in the interpolation.</param>
		/// <param name="value4">The fourth position in the interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>A position that is the result of the Catmull-Rom interpolation.</returns>
		public static float CatmullRom(
			float value1,
			float value2,
			float value3,
			float value4,
			float amount
		) {
			/* Using formula from http://www.mvps.org/directx/articles/catmull/
			 * Internally using doubles not to lose precision.
			 */
			double amountSquared = amount * amount;
			double amountCubed = amountSquared * amount;
			return (float) (
				0.5 *
				(
					((2.0 * value2 + (value3 - value1) * amount) +
					((2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared) +
					(3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed)
				)
			);
		}

		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">
		/// The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c>
		/// will be returned.
		/// </param>
		/// <param name="max">
		/// The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c>
		/// will be returned.
		/// </param>
		/// <returns>The clamped value.</returns>
		public static float Clamp(float value, float min, float max)
		{
			// First we check to see if we're greater than the max.
			value = (value > max) ? max : value;

			// Then we check to see if we're less than the min.
			value = (value < min) ? min : value;

			// There's no check to see if min > max.
			return value;
		}

		/// <summary>
		/// Calculates the absolute value of the difference of two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <returns>Distance between the two values.</returns>
		public static float Distance(float value1, float value2)
		{
			return Math.Abs(value1 - value2);
		}

		/// <summary>
		/// Performs a Hermite spline interpolation.
		/// </summary>
		/// <param name="value1">Source position.</param>
		/// <param name="tangent1">Source tangent.</param>
		/// <param name="value2">Source position.</param>
		/// <param name="tangent2">Source tangent.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The result of the Hermite spline interpolation.</returns>
		public static float Hermite(
			float value1,
			float tangent1,
			float value2,
			float tangent2,
			float amount
		) {
			/* All transformed to double not to lose precision
			 * Otherwise, for high numbers of param:amount the result is NaN instead
			 * of Infinity.
			 */
			double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount;
			double result;
			double sCubed = s * s * s;
			double sSquared = s * s;

			if (WithinEpsilon(amount, 0f))
			{
				result = value1;
			}
			else if (WithinEpsilon(amount, 1f))
			{
				result = value2;
			}
			else
			{
				result = (
					((2 * v1 - 2 * v2 + t2 + t1) * sCubed) +
					((3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared) +
					(t1 * s) +
					v1
				);
			}

			return (float) result;
		}


		/// <summary>
		/// Linearly interpolates between two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <param name="amount">
		/// Value between 0 and 1 indicating the weight of value2.
		/// </param>
		/// <returns>Interpolated value.</returns>
		/// <remarks>
		/// This method performs the linear interpolation based on the following formula.
		/// <c>value1 + (value2 - value1) * amount</c>
		/// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will
		/// cause value2 to be returned.
		/// </remarks>
		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		/// <summary>
		/// Returns the greater of two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <returns>The greater value.</returns>
		public static float Max(float value1, float value2)
		{
			return value1 > value2 ? value1 : value2;
		}

		/// <summary>
		/// Returns the lesser of two values.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <returns>The lesser value.</returns>
		public static float Min(float value1, float value2)
		{
			return value1 < value2 ? value1 : value2;
		}

		/// <summary>
		/// Interpolates between two values using a cubic equation.
		/// </summary>
		/// <param name="value1">Source value.</param>
		/// <param name="value2">Source value.</param>
		/// <param name="amount">Weighting value.</param>
		/// <returns>Interpolated value.</returns>
		public static float SmoothStep(float value1, float value2, float amount)
		{
			/* It is expected that 0 < amount < 1.
			 * If amount < 0, return value1.
			 * If amount > 1, return value2.
			 */
			float result = MathHelper.Clamp(amount, 0f, 1f);
			result = MathHelper.Hermite(value1, 0f, value2, 0f, result);

			return result;
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="radians">The angle in radians.</param>
		/// <returns>The angle in degrees.</returns>
		/// <remarks>
		/// This method uses double precision internally, though it returns single float.
		/// Factor = 180 / pi
		/// </remarks>
		public static float ToDegrees(float radians)
		{
			return (float) (radians * 57.295779513082320876798154814105);
		}

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		/// <param name="degrees">The angle in degrees.</param>
		/// <returns>The angle in radians.</returns>
		/// <remarks>
		/// This method uses double precision internally, though it returns single float.
		/// Factor = pi / 180
		/// </remarks>
		public static float ToRadians(float degrees)
		{
			return (float) (degrees * 0.017453292519943295769236907684886);
		}

		/// <summary>
		/// Reduces a given angle to a value between pi and -pi.
		/// </summary>
		/// <param name="angle">The angle to reduce, in radians.</param>
		/// <returns>The new angle, in radians.</returns>
		public static float WrapAngle(float angle)
		{
			angle = (float) Math.IEEERemainder((double) angle, 6.2831854820251465);
			if (angle <= -3.14159274f)
			{
				angle += 6.28318548f;
			}
			else
			{
				if (angle > 3.14159274f)
				{
					angle -= 6.28318548f;
				}
			}
			return angle;
		}

		#endregion

		#region Internal Static Methods

		// FIXME: This could be an extension! ClampIntEXT? -flibit
		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">
		/// The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c>
		/// will be returned.
		/// </param>
		/// <param name="max">
		/// The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c>
		/// will be returned.
		/// </param>
		/// <returns>The clamped value.</returns>
		internal static int Clamp(int value, int min, int max)
		{
			value = (value > max) ? max : value;
			value = (value < min) ? min : value;
			return value;
		}

		internal static bool WithinEpsilon(float floatA, float floatB)
		{
			return Math.Abs(floatA - floatB) < MachineEpsilonFloat;
		}

		internal static int ClosestMSAAPower(int value)
		{
			/* Checking for the highest power of two _after_ than the given int:
			 * http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
			 * Take result, divide by 2, get the highest power of two _before_!
			 * -flibit
			 */
			if (value == 1)
			{
				// ... Except for 1, which is invalid for MSAA -flibit
				return 0;
			}
			int result = value - 1;
			result |= result >> 1;
			result |= result >> 2;
			result |= result >> 4;
			result |= result >> 8;
			result |= result >> 16;
			result += 1;
			if (result == value)
			{
				return result;
			}
			return result >> 1;
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Find the current machine's Epsilon for the float data type.
		/// (That is, the largest float, e,  where e == 0.0f is true.)
		/// </summary>
		private static float GetMachineEpsilonFloat()
		{
			float machineEpsilon = 1.0f;
			float comparison;

			/* Keep halving the working value of machineEpsilon until we get a number that
			 * when added to 1.0f will still evaluate as equal to 1.0f.
			 */
			do
			{
				machineEpsilon *= 0.5f;
				comparison = 1.0f + machineEpsilon;
			}
			while (comparison > 1.0f);

			return machineEpsilon;
		}

		#endregion
	}
}
