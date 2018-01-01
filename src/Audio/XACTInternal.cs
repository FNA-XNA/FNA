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

namespace Microsoft.Xna.Framework.Audio
{
	internal static class XACTCalculator
	{
		public static double ParseDecibel(byte binaryValue)
		{
			/* FIXME: This magnificent beauty came from Mathematica!
			 * The byte values for all possible input dB values from the .xap are here:
			 * http://www.flibitijibibo.com/XACTVolume.txt
			 * Yes, this is actually what the XACT builder really does.
			 *
			 * Thanks to Kenny for plotting all that data.
			 * -flibit
			 */
			return (
				(3969.0 * Math.Log10(binaryValue / 28240.0)) + 8715.0
			);
		}

		// Note: This takes 'decibels * 100' as input!
		public static float CalculateAmplitudeRatio(double decibel)
		{
			return (float) Math.Pow(10, decibel / 2000.0);
		}

		// Interestingly, Reverb gains are stored as floats without the x100!
		public static float CalculateReverbAmplitudeRatio(float decibel)
		{
			return (float) Math.Pow(10, decibel / 20.0f);
		}
	}

	internal enum MaxInstanceBehavior : byte
	{
		Fail,
		Queue,
		ReplaceOldest,
		ReplaceQuietest,
		ReplaceLowestPriority
	}

	internal enum CrossfadeType : byte
	{
		Linear,
		Logarithmic,
		EqualPower
	}

	internal class Variable
	{
		public string Name
		{
			get;
			private set;
		}

		// Variable Accessibility
		public bool IsPublic
		{
			get;
			private set;
		}

		public bool IsReadOnly
		{
			get;
			private set;
		}

		public bool IsGlobal
		{
			get;
			private set;
		}

		public bool IsReserved
		{
			get;
			private set;
		}

		// Variable Value, Boundaries
		private float value;
		private float minValue;
		private float maxValue;

		public Variable(
			string name,
			bool varIsPublic,
			bool varIsReadOnly,
			bool varIsGlobal,
			bool varIsReserved,
			float varInitialValue,
			float varMinValue,
			float varMaxValue
		) {
			Name = name;
			IsPublic = varIsPublic;
			IsReadOnly = varIsReadOnly;
			IsGlobal = varIsGlobal;
			IsReserved = varIsReserved;
			value = varInitialValue;
			minValue = varMinValue;
			maxValue = varMaxValue;
		}

		public void SetValue(float newValue)
		{
			if (newValue < minValue)
			{
				value = minValue;
			}
			else if (newValue > maxValue)
			{
				value = maxValue;
			}
			else
			{
				value = newValue;
			}
		}

		public float GetValue()
		{
			return value;
		}

		public Variable Clone()
		{
			return new Variable(
				Name,
				IsPublic,
				IsReadOnly,
				IsGlobal,
				IsReserved,
				value,
				minValue,
				maxValue
			);
		}
	}

	internal enum RPCPointType : byte
	{
		Linear,
		Fast,
		Slow,
		SinCos
	}

	internal enum RPCParameter : ushort
	{
		Volume,
		Pitch,
		ReverbSend,
		FilterFrequency,
		FilterQFactor,
		NUM_PARAMETERS // If >=, DSP Parameter!
	}

	internal class RPCPoint
	{
		public float X
		{
			get;
			private set;
		}

		public float Y
		{
			get;
			private set;
		}

		public RPCPointType Type
		{
			get;
			private set;
		}

		public RPCPoint(float x, float y, RPCPointType type)
		{
			X = x;
			Y = y;
			Type = type;
		}
	}

	internal class RPC
	{
		// Parent Variable
		public string Variable
		{
			get;
			private set;
		}

		// RPC Parameter
		public RPCParameter Parameter
		{
			get;
			private set;
		}

		// RPC Curve Points
		private RPCPoint[] Points;

		public RPC(
			string rpcVariable,
			ushort rpcParameter,
			RPCPoint[] rpcPoints
		) {
			Variable = rpcVariable;
			Parameter = (RPCParameter) rpcParameter;
			Points = rpcPoints;
		}

		public RPCPoint LastPoint { get { return Points[Points.Length - 1]; } }

		public float CalculateRPC(float varInput)
		{
			// Min/Max
			if (varInput <= Points[0].X)
			{
				// Zero to first defined point
				return Points[0].Y;
			}
			if (varInput >= Points[Points.Length - 1].X)
			{
				// Last defined point to infinity
				return Points[Points.Length - 1].Y;
			}

			// Something between points... TODO: Non-linear curves
			float result = 0.0f;
			for (int i = 0; i < Points.Length - 1; i += 1)
			{
				// y = b
				result = Points[i].Y;
				if (varInput >= Points[i].X && varInput <= Points[i + 1].X)
				{
					// y += mx
					result +=
						((Points[i + 1].Y - Points[i].Y) /
						(Points[i + 1].X - Points[i].X)) *
							(varInput - Points[i].X);
					// Pre-algebra, rockin`!
					break;
				}
			}
			return result;
		}
	}

	internal class DSPParameter
	{
		public byte Type
		{
			get;
			private set;
		}

		public float Minimum
		{
			get;
			private set;
		}

		public float Maximum
		{
			get;
			private set;
		}

		private float INTERNAL_value;
		public float Value
		{
			get
			{
				return INTERNAL_value;
			}
			set
			{
				if (value < Minimum)
				{
					INTERNAL_value = Minimum;
				}
				else if (value > Maximum)
				{
					INTERNAL_value = Maximum;
				}
				else
				{
					INTERNAL_value = value;
				}
			}
		}
		public DSPParameter(byte type, float val, float min, float max)
		{
			Type = type;
			Minimum = min;
			Maximum = max;
			INTERNAL_value = val;
		}
	}

	internal class DSPPreset
	{
		public IALReverb Effect
		{
			get;
			private set;
		}

		public bool IsGlobal
		{
			get;
			private set;
		}

		public DSPParameter[] Parameters
		{
			get;
			private set;
		}

		public DSPPreset(
			bool global,
			DSPParameter[] parameters
		) {
			IsGlobal = global;
			Parameters = parameters;

			// FIXME: Did XACT ever go past Reverb? -flibit
			Effect = AudioDevice.GenReverb(Parameters);
		}

		public void Dispose()
		{
			AudioDevice.ALDevice.DeleteReverb(Effect);
		}

		public void SetParameter(int index, float value)
		{
			Parameters[index].Value = value;

			// Apply the value to the effect
			if (index == 0)
			{
				AudioDevice.ALDevice.SetReverbReflectionsDelay(Effect, Parameters[index].Value);
			}
			else if (index == 1)
			{
				AudioDevice.ALDevice.SetReverbDelay(Effect, Parameters[index].Value);
			}
			else if (index == 2)
			{
				AudioDevice.ALDevice.SetReverbPositionLeft(Effect, Parameters[index].Value);
			}
			else if (index == 3)
			{
				AudioDevice.ALDevice.SetReverbPositionRight(Effect, Parameters[index].Value);
			}
			else if (index == 4)
			{
				AudioDevice.ALDevice.SetReverbPositionLeftMatrix(Effect, Parameters[index].Value);
			}
			else if (index == 5)
			{
				AudioDevice.ALDevice.SetReverbPositionRightMatrix(Effect, Parameters[index].Value);
			}
			else if (index == 6)
			{
				AudioDevice.ALDevice.SetReverbEarlyDiffusion(Effect, Parameters[index].Value);
			}
			else if (index == 7)
			{
				AudioDevice.ALDevice.SetReverbLateDiffusion(Effect, Parameters[index].Value);
			}
			else if (index == 8)
			{
				AudioDevice.ALDevice.SetReverbLowEQGain(Effect, Parameters[index].Value);
			}
			else if (index == 9)
			{
				AudioDevice.ALDevice.SetReverbLowEQCutoff(Effect, Parameters[index].Value);
			}
			else if (index == 10)
			{
				AudioDevice.ALDevice.SetReverbHighEQGain(Effect, Parameters[index].Value);
			}
			else if (index == 11)
			{
				AudioDevice.ALDevice.SetReverbHighEQCutoff(Effect, Parameters[index].Value);
			}
			else if (index == 12)
			{
				AudioDevice.ALDevice.SetReverbRearDelay(Effect, Parameters[index].Value);
			}
			else if (index == 13)
			{
				AudioDevice.ALDevice.SetReverbRoomFilterFrequency(Effect, Parameters[index].Value);
			}
			else if (index == 14)
			{
				AudioDevice.ALDevice.SetReverbRoomFilterMain(Effect, Parameters[index].Value);
			}
			else if (index == 15)
			{
				AudioDevice.ALDevice.SetReverbRoomFilterHighFrequency(Effect, Parameters[index].Value);
			}
			else if (index == 16)
			{
				AudioDevice.ALDevice.SetReverbReflectionsGain(Effect, Parameters[index].Value);
			}
			else if (index == 17)
			{
				AudioDevice.ALDevice.SetReverbGain(Effect, Parameters[index].Value);
			}
			else if (index == 18)
			{
				AudioDevice.ALDevice.SetReverbDecayTime(Effect, Parameters[index].Value);
			}
			else if (index == 19)
			{
				AudioDevice.ALDevice.SetReverbDensity(Effect, Parameters[index].Value);
			}
			else if (index == 20)
			{
				AudioDevice.ALDevice.SetReverbRoomSize(Effect, Parameters[index].Value);
			}
			else if (index == 21)
			{
				AudioDevice.ALDevice.SetReverbWetDryMix(Effect, Parameters[index].Value);
			}
			else
			{
				throw new NotImplementedException("DSP parameter unhandled: " + index.ToString());
			}
		}
	}
}
