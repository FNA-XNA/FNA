#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/dd940262.aspx
	public class AudioEngine : IDisposable
	{
		#region Public Constants

		public const int ContentVersion = 46;

		#endregion

		#region Public Properties

		public ReadOnlyCollection<RendererDetail> RendererDetails
		{
			get
			{
				return AudioDevice.Renderers;
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private Dictionary<string, WaveBank> INTERNAL_waveBanks;

		private List<AudioCategory> INTERNAL_categories;
		private List<Variable> INTERNAL_variables;
		private Dictionary<long, RPC> INTERNAL_RPCs;
		private List<DSPParameter> INTERNAL_dspParameters;
		private Dictionary<long, DSPPreset> INTERNAL_dspPresets;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructors

		public AudioEngine(string settingsFile)
		{
			if (String.IsNullOrEmpty(settingsFile))
			{
				throw new ArgumentNullException("settingsFile");
			}

			using (Stream stream = TitleContainer.OpenStream(settingsFile))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				// Check the file header. Should be 'XGSF'
				if (reader.ReadUInt32() != 0x46534758)
				{
					throw new ArgumentException("XGSF format not recognized!");
				}

				// Check the Content and Tool versions
				if (reader.ReadUInt16() != ContentVersion)
				{
					throw new ArgumentException("XGSF Content version!");
				}
				if (reader.ReadUInt16() != 42)
				{
					throw new ArgumentException("XGSF Tool version!");
				}

				// Unknown value
				reader.ReadUInt16();

				// Last Modified, Unused
				reader.ReadUInt64();

				// XACT Version, Unused
				reader.ReadByte();

				// Number of AudioCategories
				ushort numCategories = reader.ReadUInt16();

				// Number of XACT Variables
				ushort numVariables = reader.ReadUInt16();

				// KEY#1 Length
				/*ushort numKeyOne =*/ reader.ReadUInt16();

				// KEY#2 Length
				/*ushort numKeyTwo =*/ reader.ReadUInt16();

				// Number of RPC Variables
				ushort numRPCs = reader.ReadUInt16();

				// Number of DSP Presets/Parameters
				ushort numDSPPresets = reader.ReadUInt16();
				ushort numDSPParameters = reader.ReadUInt16();

				// Category Offset in XGS File
				uint categoryOffset = reader.ReadUInt32();

				// Variable Offset in XGS File
				uint variableOffset = reader.ReadUInt32();

				// KEY#1 Offset
				/*uint keyOneOffset =*/ reader.ReadUInt32();

				// Category Name Index Offset, unused
				reader.ReadUInt32();

				// KEY#2 Offset
				/*uint keyTwoOffset =*/ reader.ReadUInt32();

				// Variable Name Index Offset, unused
				reader.ReadUInt32();

				// Category Name Offset in XGS File
				uint categoryNameOffset = reader.ReadUInt32();

				// Variable Name Offset in XGS File
				uint variableNameOffset = reader.ReadUInt32();

				// RPC Variable Offset in XGS File
				uint rpcOffset = reader.ReadUInt32();

				// DSP Preset/Parameter Offsets in XGS File
				uint dspPresetOffset = reader.ReadUInt32();
				uint dspParameterOffset = reader.ReadUInt32();

				/* Unknown table #1
				reader.BaseStream.Seek(keyOneOffset, SeekOrigin.Begin);
				for (int i = 0; i < numKeyOne; i += 1)
				{
					// Appears to consistently be 16 shorts?
					System.Console.WriteLine(reader.ReadInt16());
				}
				/* OhGodNo
				 *  1, -1,  4, -1,
				 *  3, -1, -1,  7,
				 * -1,  2,  5, -1,
				 *  6,  0, -1, -1
				 *
				 * Naddachance
				 *  1, -1,  4, -1,
				 *  5, -1, -1, -1,
				 * -1,  2, -1, -1,
				 *  3,  0, -1, -1
				 *
				 * TFA
				 *  1, -1, -1, -1,
				 * -1, -1, -1, -1,
				 * -1,  2, -1, -1,
				 * -1, -0, -1, -1
				 */

				/* Unknown table #2
				reader.BaseStream.Seek(keyTwoOffset, SeekOrigin.Begin);
				for (int i = 0; i < numKeyTwo; i += 1)
				{
					// Appears to be between 16-20 shorts?
					System.Console.WriteLine(reader.ReadInt16());
				}
				/* OhGodNo
				 *  2,  7,  1, -1,
				 * -1, 10, 19, -1,
				 *  11, 3, -1, -1,
				 *  8, -1, 14,  5,
				 * 12,  0,  4,  6
				 *
				 * Naddachance
				 *  2,  3, -1, -1,
				 *  9, -1,  7, -1,
				 * 10,  0,  1,  5,
				 * -1, -1, -1, -1
				 *
				 * TFA
				 *  2,  3, -1, -1,
				 * -1, -1, -1, -1,
				 * -1,  0,  1,  5,
				 * -1, -1, -1, -1
				 */

				// Obtain the Audio Category Names
				reader.BaseStream.Seek(categoryNameOffset, SeekOrigin.Begin);
				string[] categoryNames = new string[numCategories];
				for (int i = 0; i < numCategories; i += 1)
				{
					List<char> builtString = new List<char>();
					while (reader.PeekChar() != 0)
					{
						builtString.Add(reader.ReadChar());
					}
					reader.ReadChar(); // Null terminator
					categoryNames[i] = new string(builtString.ToArray());
				}

				// Obtain the Audio Categories
				reader.BaseStream.Seek(categoryOffset, SeekOrigin.Begin);
				INTERNAL_categories = new List<AudioCategory>();
				for (int i = 0; i < numCategories; i += 1)
				{
					// Maximum instances
					byte maxInstances = reader.ReadByte();

					// Fade In/Out
					ushort fadeInMS = reader.ReadUInt16();
					ushort fadeOutMS = reader.ReadUInt16();

					// Instance Behavior Flags
					byte instanceFlags = reader.ReadByte();
					int fadeType = instanceFlags & 0x07;
					int maxBehavior = instanceFlags >> 3;

					// Unknown value
					reader.ReadUInt16();

					// Volume
					float volume = XACTCalculator.CalculateVolume(reader.ReadByte());

					// Visibility Flags, unused
					reader.ReadByte();

					// Add to the engine list
					INTERNAL_categories.Add(
						new AudioCategory(
							categoryNames[i],
							volume,
							maxInstances,
							maxBehavior,
							fadeInMS,
							fadeOutMS,
							fadeType
						)
					);
				}

				// Obtain the Variable Names
				reader.BaseStream.Seek(variableNameOffset, SeekOrigin.Begin);
				string[] variableNames = new string[numVariables];
				for (int i = 0; i < numVariables; i += 1)
				{
					List<char> builtString = new List<char>();
					while (reader.PeekChar() != 0)
					{
						builtString.Add(reader.ReadChar());
					}
					reader.ReadChar(); // Null terminator
					variableNames[i] = new string(builtString.ToArray());
				}

				// Obtain the Variables
				reader.BaseStream.Seek(variableOffset, SeekOrigin.Begin);
				INTERNAL_variables = new List<Variable>();
				for (int i = 0; i < numVariables; i += 1)
				{
					// Variable Accessibility (See Variable constructor)
					byte varFlags = reader.ReadByte();

					// Variable Value, Boundaries
					float initialValue =	reader.ReadSingle();
					float minValue =	reader.ReadSingle();
					float maxValue =	reader.ReadSingle();

					// Add to the engine list
					INTERNAL_variables.Add(
						new Variable(
							variableNames[i],
							(varFlags & 0x01) != 0,
							(varFlags & 0x02) != 0,
							(varFlags & 0x04) == 0,
							(varFlags & 0x08) != 0,
							initialValue,
							minValue,
							maxValue
						)
					);
				}

				// Obtain the RPC Curves
				reader.BaseStream.Seek(rpcOffset, SeekOrigin.Begin);
				INTERNAL_RPCs = new Dictionary<long, RPC>();
				for (int i = 0; i < numRPCs; i += 1)
				{
					// RPC "Code", used by the SoundBanks
					long rpcCode = reader.BaseStream.Position;

					// RPC Variable
					ushort rpcVariable = reader.ReadUInt16();

					// Number of RPC Curve Points
					byte numPoints = reader.ReadByte();

					// RPC Parameter
					ushort rpcParameter = reader.ReadUInt16();

					// RPC Curve Points
					RPCPoint[] rpcPoints = new RPCPoint[numPoints];
					for (byte j = 0; j < numPoints; j += 1)
					{
						float x = reader.ReadSingle();
						float y = reader.ReadSingle();
						byte type = reader.ReadByte();
						rpcPoints[j] = new RPCPoint(
							x, y,
							(RPCPointType) type
						);
					}

					// Add to the engine list
					INTERNAL_RPCs.Add(
						rpcCode,
						new RPC(
							INTERNAL_variables[rpcVariable].Name,
							rpcParameter,
							rpcPoints
						)
					);
				}

				// Obtain the DSP Parameters
				reader.BaseStream.Seek(dspParameterOffset, SeekOrigin.Begin);
				INTERNAL_dspParameters = new List<DSPParameter>();
				for (int i = 0; i < numDSPParameters; i += 1)
				{
					// Effect Parameter Type
					byte type = reader.ReadByte();

					// Effect value, boundaries
					float value = reader.ReadSingle();
					float minVal = reader.ReadSingle();
					float maxVal = reader.ReadSingle();

					// Unknown value
					reader.ReadUInt16();

					// Add to Parameter list
					INTERNAL_dspParameters.Add(
						new DSPParameter(
							type,
							value,
							minVal,
							maxVal
						)
					);
				}

				// Obtain the DSP Presets
				reader.BaseStream.Seek(dspPresetOffset, SeekOrigin.Begin);
				INTERNAL_dspPresets = new Dictionary<long, DSPPreset>();
				int total = 0;
				for (int i = 0; i < numDSPPresets; i += 1)
				{
					// DSP "Code", used by the SoundBanks
					long dspCode = reader.BaseStream.Position;

					// Preset Accessibility
					bool global = (reader.ReadByte() == 1);

					// Number of preset parameters
					uint numParams = reader.ReadUInt32();

					// Obtain DSP Parameters
					DSPParameter[] parameters = new DSPParameter[numParams];
					for (uint j = 0; j < numParams; j += 1)
					{
						parameters[j] = INTERNAL_dspParameters[total];
						total += 1;
					}

					// Add to DSP Preset list
					INTERNAL_dspPresets.Add(
						dspCode,
						new DSPPreset(
							global,
							parameters
						)
					);
				}
			}

			// Create the WaveBank Dictionary
			INTERNAL_waveBanks = new Dictionary<string, WaveBank>();

			// Finally.
			IsDisposed = false;
		}

		public AudioEngine(
			string settingsFile,
			TimeSpan lookAheadTime,
			string rendererId
		) {
			/* TODO: May require either resetting the ALDevice,
			 * or adding a second AL device/context for this engine.
			 * -flibit
			 */
			throw new NotSupportedException();
		}

		#endregion

		#region Destructor

		~AudioEngine()
		{
			Dispose();
		}

		#endregion

		#region Public Dispose Methods

		public void Dispose()
		{
			if (!IsDisposed)
			{
				if (Disposing != null)
				{
					Disposing.Invoke(this, null);
				}
				foreach (AudioCategory curCategory in INTERNAL_categories)
				{
					curCategory.Stop(AudioStopOptions.Immediate);
				}
				INTERNAL_categories.Clear();
				foreach (KeyValuePair<long, DSPPreset> curDSP in INTERNAL_dspPresets)
				{
					curDSP.Value.Dispose();
				}
				INTERNAL_dspPresets.Clear();
				INTERNAL_dspParameters.Clear();
				INTERNAL_variables.Clear();
				INTERNAL_RPCs.Clear();
				IsDisposed = true;
			}
		}

		#endregion

		#region Public Methods

		public AudioCategory GetCategory(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			for (int i = 0; i < INTERNAL_categories.Count; i += 1)
			{
				if (INTERNAL_categories[i].Name.Equals(name))
				{
					return INTERNAL_categories[i];
				}
			}
			throw new InvalidOperationException("Category not found!");
		}

		public float GetGlobalVariable(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			for (int i = 0; i < INTERNAL_variables.Count; i += 1)
			{
				if (name.Equals(INTERNAL_variables[i].Name))
				{
					if (!INTERNAL_variables[i].IsGlobal)
					{
						throw new InvalidOperationException("Variable not global!");
					}
					return INTERNAL_variables[i].GetValue();
				}
			}
			throw new InvalidOperationException("Variable not found!");
		}

		public void SetGlobalVariable(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			for (int i = 0; i < INTERNAL_variables.Count; i += 1)
			{
				if (name.Equals(INTERNAL_variables[i].Name))
				{
					if (!INTERNAL_variables[i].IsGlobal)
					{
						throw new InvalidOperationException("Variable not global!");
					}
					INTERNAL_variables[i].SetValue(value);
					return; // We made it!
				}
			}
			throw new InvalidOperationException("Variable not found!");
		}

		public void Update()
		{
			// Update Global RPCs
			foreach (RPC curRPC in INTERNAL_RPCs.Values)
			if (curRPC.Parameter >= RPCParameter.NUM_PARAMETERS)
			foreach (Variable curVar in INTERNAL_variables)
			if (curVar.Name.Equals(curRPC.Variable) && curVar.IsGlobal)
			foreach (DSPPreset curDSP in INTERNAL_dspPresets.Values)
			{
				/* FIXME: This affects all DSP presets!
				 * What if there's more than one?
				 * -flibit
				 */
				curDSP.SetParameter(
					(int) curRPC.Parameter - (int) RPCParameter.NUM_PARAMETERS,
					curRPC.CalculateRPC(GetGlobalVariable(curVar.Name))
				);
			}

			// Apply all DSP changes once they have been made
			foreach (DSPPreset curDSP in INTERNAL_dspPresets.Values)
			{
				AudioDevice.ALDevice.CommitReverbChanges(curDSP.Effect);
			}

			// Update Cues
			foreach (AudioCategory curCategory in INTERNAL_categories)
			{
				curCategory.INTERNAL_update();
			}
		}

		#endregion

		#region Internal Methods

		internal void INTERNAL_addWaveBank(string name, WaveBank waveBank)
		{
			INTERNAL_waveBanks.Add(name, waveBank);
		}

		internal void INTERNAL_removeWaveBank(string name)
		{
			INTERNAL_waveBanks.Remove(name);
		}

		internal SoundEffect INTERNAL_getWaveBankTrack(string name, ushort track)
		{
			return INTERNAL_waveBanks[name].INTERNAL_getTrack(track);
		}

		internal string INTERNAL_getVariableName(ushort index)
		{
			return INTERNAL_variables[index].Name;
		}

		internal RPC INTERNAL_getRPC(uint code)
		{
			return INTERNAL_RPCs[code];
		}

		internal IALReverb INTERNAL_getDSP(uint code)
		{
			return INTERNAL_dspPresets[code].Effect;
		}

		internal AudioCategory INTERNAL_initCue(Cue newCue, ushort category)
		{
			List<Variable> cueVariables = new List<Variable>();
			foreach (Variable curVar in INTERNAL_variables)
			{
				if (!curVar.IsGlobal)
				{
					cueVariables.Add(curVar.Clone());
				}
			}
			newCue.INTERNAL_genVariables(cueVariables);
			return INTERNAL_categories[category];
		}

		internal bool INTERNAL_isGlobalVariable(string name)
		{
			// FIXME: Any way to speed this up? -flibit
			foreach (Variable curVar in INTERNAL_variables)
			{
				if (name.Equals(curVar.Name))
				{
					return curVar.IsGlobal;
				}
			}

			// Variable doesn't even exist here...!
			return false;
		}

		#endregion
	}
}
