#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class EffectParameter
	{
		#region Public Properties

		public string Name
		{
			get;
			private set;
		}

		public string Semantic
		{
			get;
			private set;
		}

		public int RowCount
		{
			get;
			private set;
		}

		public int ColumnCount
		{
			get;
			private set;
		}

		public EffectParameterClass ParameterClass
		{
			get;
			private set;
		}

		public EffectParameterType ParameterType
		{
			get;
			private set;
		}

		public EffectParameterCollection Elements
		{
			get
			{
				if ((elementCount > 0) && (elements == null))
				{
					BuildElementList();
				}
				return elements;
			}
		}

		public EffectParameterCollection StructureMembers
		{
			get
			{
				if ((mojoType != IntPtr.Zero) && (members == null))
				{
					BuildMemberList();
				}
				return members;
			}
		}

		public EffectAnnotationCollection Annotations
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		internal Texture texture;
		internal string cachedString = string.Empty;

		internal IntPtr values;
		internal uint valuesSizeBytes;

		internal IntPtr mojoType;

		internal int elementCount;
		internal EffectParameterCollection elements;
		internal EffectParameterCollection members;
		#endregion

		#region Private Variables

		// Ugly as all heck, but I had to do it for structures. - MrSoup678
		private Effect outer;

		#endregion

		#region Internal Constructor

		internal EffectParameter(
			string name,
			string semantic,
			int rowCount,
			int columnCount,
			int elementCount,
			EffectParameterClass parameterClass,
			EffectParameterType parameterType,
			IntPtr mojoType,
			EffectAnnotationCollection annotations,
			IntPtr data,
			uint dataSizeBytes,
			Effect effect
		) {
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException("data");
			}

			Name = name;
			Semantic = semantic ?? string.Empty;
			RowCount = rowCount;
			ColumnCount = columnCount;
			this.elementCount = elementCount;
			ParameterClass = parameterClass;
			ParameterType = parameterType;
			this.mojoType = mojoType;
			Annotations = annotations;
			values = data;
			valuesSizeBytes = dataSizeBytes;
			outer = effect;
		}

		internal EffectParameter(
			string name,
			string semantic,
			int rowCount,
			int columnCount,
			int elementCount,
			EffectParameterClass parameterClass,
			EffectParameterType parameterType,
			EffectParameterCollection structureMembers,
			EffectAnnotationCollection annotations,
			IntPtr data,
			uint dataSizeBytes,
			Effect effect
		) {
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException("data");
			}

			Name = name;
			Semantic = semantic ?? string.Empty;
			RowCount = rowCount;
			ColumnCount = columnCount;
			this.elementCount = elementCount;
			ParameterClass = parameterClass;
			ParameterType = parameterType;
			members = structureMembers;
			Annotations = annotations;
			values = data;
			valuesSizeBytes = dataSizeBytes;
			outer = effect;
		}

		#endregion

		#region Allocation Optimizations

		internal void BuildMemberList()
		{
			members = Effect.INTERNAL_readEffectParameterStructureMembers(this, mojoType, outer);
		}

		internal void BuildElementList()
		{
			if (elementCount > 0)
			{
				int curOffset = 0;
				List<EffectParameter> elements = new List<EffectParameter>(elementCount);
				EffectParameterCollection structureMembers = StructureMembers;
				for (int i = 0; i < elementCount; i += 1)
				{
					EffectParameterCollection elementMembers = null;
					if (structureMembers != null)
					{
						List<EffectParameter> memList = new List<EffectParameter>();
						for (int j = 0; j < structureMembers.Count; j += 1)
						{
							int memElems = 0;
							if (structureMembers[j].Elements != null)
							{
								memElems = structureMembers[j].Elements.Count;
							}
							int memSize = structureMembers[j].RowCount * 4;
							if (memElems > 0)
							{
								memSize *= memElems;
							}
							memList.Add(new EffectParameter(
								structureMembers[j].Name,
								structureMembers[j].Semantic,
								structureMembers[j].RowCount,
								structureMembers[j].ColumnCount,
								memElems,
								structureMembers[j].ParameterClass,
								structureMembers[j].ParameterType,
								IntPtr.Zero, // FIXME: Nested structs! -flibit
								structureMembers[j].Annotations,
								new IntPtr(values.ToInt64() + curOffset),
								(uint) memSize * 4,
								outer
							));
							curOffset += memSize * 4;
						}
						elementMembers = new EffectParameterCollection(memList);
					}
					// FIXME: Probably incomplete? -flibit
					elements.Add(new EffectParameter(
						null,
						null,
						RowCount,
						ColumnCount,
						0,
						ParameterClass,
						ParameterType,
						elementMembers,
						null,
						new IntPtr(
							values.ToInt64() + (i * RowCount * 16)
						),
						// FIXME: Not obvious to me how to compute this -kg
						0,
						outer
					));
				}
				this.elements = new EffectParameterCollection(elements);
			}
		}

		#endregion

		#region Public Get Methods

		public bool GetValueBoolean()
		{
			unsafe
			{
				// Values are always 4 bytes, so we get to do this. -flibit
				int* resPtr = (int*) values;
				return *resPtr != 0;
			}
		}

		public bool[] GetValueBooleanArray(int count)
		{
			bool[] result = new bool[count];
			unsafe
			{
				int* resPtr = (int*) values;
				for (int i = 0; i < result.Length; resPtr += 4)
				{
					for (int j = 0; j < ColumnCount; j += 1, i += 1)
					{
						result[i] = *(resPtr + j) != 0;
					}
				}
			}
			return result;
		}

		public int GetValueInt32()
		{
			unsafe
			{
				int* resPtr = (int*) values;
				return *resPtr;
			}
		}

		public int[] GetValueInt32Array(int count)
		{
			int[] result = new int[count];
			for (int i = 0, j = 0; i < result.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(values + j, result, i, ColumnCount);
			}
			return result;
		}

		public Matrix GetValueMatrixTranspose()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Matrix(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3],
					resPtr[4],
					resPtr[5],
					resPtr[6],
					resPtr[7],
					resPtr[8],
					resPtr[9],
					resPtr[10],
					resPtr[11],
					resPtr[12],
					resPtr[13],
					resPtr[14],
					resPtr[15]
				);
			}
		}

		public Matrix[] GetValueMatrixTransposeArray(int count)
		{
			Matrix[] result = new Matrix[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 16)
				{
					result[i] = new Matrix(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3],
						resPtr[4],
						resPtr[5],
						resPtr[6],
						resPtr[7],
						resPtr[8],
						resPtr[9],
						resPtr[10],
						resPtr[11],
						resPtr[12],
						resPtr[13],
						resPtr[14],
						resPtr[15]
					);
				}
			}
			return result;
		}

		public Matrix GetValueMatrix()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Matrix(
					resPtr[0],
					resPtr[4],
					resPtr[8],
					resPtr[12],
					resPtr[1],
					resPtr[5],
					resPtr[9],
					resPtr[13],
					resPtr[2],
					resPtr[6],
					resPtr[10],
					resPtr[14],
					resPtr[3],
					resPtr[7],
					resPtr[11],
					resPtr[15]
				);
			}
		}

		public Matrix[] GetValueMatrixArray(int count)
		{
			Matrix[] result = new Matrix[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 16)
				{
					result[i] = new Matrix(
						resPtr[0],
						resPtr[4],
						resPtr[8],
						resPtr[12],
						resPtr[1],
						resPtr[5],
						resPtr[9],
						resPtr[13],
						resPtr[2],
						resPtr[6],
						resPtr[10],
						resPtr[14],
						resPtr[3],
						resPtr[7],
						resPtr[11],
						resPtr[15]
					);
				}
			}
			return result;
		}

		public Quaternion GetValueQuaternion()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Quaternion(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3]
				);
			}
		}

		public Quaternion[] GetValueQuaternionArray(int count)
		{
			Quaternion[] result = new Quaternion[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Quaternion(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3]
					);
				}
			}
			return result;
		}

		public float GetValueSingle()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return *resPtr;
			}
		}

		public float[] GetValueSingleArray(int count)
		{
			float[] result = new float[count];
			for (int i = 0, j = 0; i < result.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(values + j, result, i, ColumnCount);
			}
			return result;
		}

		public string GetValueString()
		{
			return cachedString;
		}

		public Texture2D GetValueTexture2D()
		{
			return (Texture2D) texture;
		}

		public Texture3D GetValueTexture3D()
		{
			return (Texture3D) texture;
		}

		public TextureCube GetValueTextureCube()
		{
			return (TextureCube) texture;
		}

		public Vector2 GetValueVector2()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Vector2(resPtr[0], resPtr[1]);
			}
		}

		public Vector2[] GetValueVector2Array(int count)
		{
			Vector2[] result = new Vector2[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector2(
						resPtr[0],
						resPtr[1]
					);
				}
			}
			return result;
		}

		public Vector3 GetValueVector3()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Vector3(resPtr[0], resPtr[1], resPtr[2]);
			}
		}

		public Vector3[] GetValueVector3Array(int count)
		{
			Vector3[] result = new Vector3[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector3(
						resPtr[0],
						resPtr[1],
						resPtr[2]
					);
				}
			}
			return result;
		}

		public Vector4 GetValueVector4()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Vector4(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3]
				);
			}
		}

		public Vector4[] GetValueVector4Array(int count)
		{
			Vector4[] result = new Vector4[count];
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector4(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3]
					);
				}
			}
			return result;
		}

		#endregion

		#region Public Set Methods

		public void SetValue(bool value)
		{
			unsafe
			{
				int* dstPtr = (int*) values;
				// Ugh, this branch, stupid C#.
				*dstPtr = value ? 1 : 0;
			}
		}

		public void SetValue(bool[] value)
		{
			unsafe
			{
				int* dstPtr = (int*) values;
				for (int i = 0; i < value.Length; dstPtr += 4)
				{
					for (int j = 0; j < ColumnCount; j += 1, i += 1)
					{
						// Ugh, this branch, stupid C#.
						*(dstPtr + j) = value[i] ? 1 : 0;
					}
				}
			}
		}

		public void SetValue(int value)
		{
			if (ParameterType == EffectParameterType.Single)
			{
				unsafe
				{
					float *dstPtr = (float*) values;
					*dstPtr = (float) value;
				}
			}
			else
			{
				unsafe
				{
					int* dstPtr = (int*) values;
					*dstPtr = value;
				}
			}
		}

		public void SetValue(int[] value)
		{
			for (int i = 0, j = 0; i < value.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(value, i, values + j, ColumnCount);
			}
		}

		public void SetValueTranspose(Matrix value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[2] = value.M13;
					dstPtr[3] = value.M14;
					dstPtr[4] = value.M21;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M23;
					dstPtr[7] = value.M24;
					dstPtr[8] = value.M31;
					dstPtr[9] = value.M32;
					dstPtr[10] = value.M33;
					dstPtr[11] = value.M34;
					dstPtr[12] = value.M41;
					dstPtr[13] = value.M42;
					dstPtr[14] = value.M43;
					dstPtr[15] = value.M44;
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[2] = value.M13;
					dstPtr[4] = value.M21;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M23;
					dstPtr[8] = value.M31;
					dstPtr[9] = value.M32;
					dstPtr[10] = value.M33;
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[2] = value.M13;
					dstPtr[4] = value.M21;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M23;
					dstPtr[8] = value.M31;
					dstPtr[9] = value.M32;
					dstPtr[10] = value.M33;
					dstPtr[12] = value.M41;
					dstPtr[13] = value.M42;
					dstPtr[14] = value.M43;
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[2] = value.M13;
					dstPtr[3] = value.M14;
					dstPtr[4] = value.M21;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M23;
					dstPtr[7] = value.M24;
					dstPtr[8] = value.M31;
					dstPtr[9] = value.M32;
					dstPtr[10] = value.M33;
					dstPtr[11] = value.M34;
				}
				else if (ColumnCount == 2 && RowCount == 2)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[4] = value.M21;
					dstPtr[5] = value.M22;
				}
				else
				{
					throw new NotImplementedException(
						"Matrix Size: " +
						RowCount.ToString() + " " +
						ColumnCount.ToString()
					);
				}
			}
		}

		public void SetValueTranspose(Matrix[] value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				float* dstPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 16)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M12;
						dstPtr[2] = value[i].M13;
						dstPtr[3] = value[i].M14;
						dstPtr[4] = value[i].M21;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M23;
						dstPtr[7] = value[i].M24;
						dstPtr[8] = value[i].M31;
						dstPtr[9] = value[i].M32;
						dstPtr[10] = value[i].M33;
						dstPtr[11] = value[i].M34;
						dstPtr[12] = value[i].M41;
						dstPtr[13] = value[i].M42;
						dstPtr[14] = value[i].M43;
						dstPtr[15] = value[i].M44;
					}
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 12)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M12;
						dstPtr[2] = value[i].M13;
						dstPtr[4] = value[i].M21;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M23;
						dstPtr[8] = value[i].M31;
						dstPtr[9] = value[i].M32;
						dstPtr[10] = value[i].M33;
					}
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 16)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M12;
						dstPtr[2] = value[i].M13;
						dstPtr[4] = value[i].M21;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M23;
						dstPtr[8] = value[i].M31;
						dstPtr[9] = value[i].M32;
						dstPtr[10] = value[i].M33;
						dstPtr[12] = value[i].M41;
						dstPtr[13] = value[i].M42;
						dstPtr[14] = value[i].M43;
					}
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 12)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M12;
						dstPtr[2] = value[i].M13;
						dstPtr[3] = value[i].M14;
						dstPtr[4] = value[i].M21;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M23;
						dstPtr[7] = value[i].M24;
						dstPtr[8] = value[i].M31;
						dstPtr[9] = value[i].M32;
						dstPtr[10] = value[i].M33;
						dstPtr[11] = value[i].M34;
					}
				}
				else if (ColumnCount == 2 && RowCount == 2)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 8)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M12;
						dstPtr[4] = value[i].M21;
						dstPtr[5] = value[i].M22;
					}
				}
				else
				{
					throw new NotImplementedException(
						"Matrix Size: " +
						RowCount.ToString() + " " +
						ColumnCount.ToString()
					);
				}
			}
		}

		public void SetValue(Matrix value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M21;
					dstPtr[2] = value.M31;
					dstPtr[3] = value.M41;
					dstPtr[4] = value.M12;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M32;
					dstPtr[7] = value.M42;
					dstPtr[8] = value.M13;
					dstPtr[9] = value.M23;
					dstPtr[10] = value.M33;
					dstPtr[11] = value.M43;
					dstPtr[12] = value.M14;
					dstPtr[13] = value.M24;
					dstPtr[14] = value.M34;
					dstPtr[15] = value.M44;
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M21;
					dstPtr[2] = value.M31;
					dstPtr[4] = value.M12;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M32;
					dstPtr[8] = value.M13;
					dstPtr[9] = value.M23;
					dstPtr[10] = value.M33;
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M21;
					dstPtr[2] = value.M31;
					dstPtr[3] = value.M41;
					dstPtr[4] = value.M12;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M32;
					dstPtr[7] = value.M42;
					dstPtr[8] = value.M13;
					dstPtr[9] = value.M23;
					dstPtr[10] = value.M33;
					dstPtr[11] = value.M43;
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M21;
					dstPtr[2] = value.M31;
					dstPtr[4] = value.M12;
					dstPtr[5] = value.M22;
					dstPtr[6] = value.M32;
					dstPtr[8] = value.M13;
					dstPtr[9] = value.M23;
					dstPtr[10] = value.M33;
					dstPtr[12] = value.M14;
					dstPtr[13] = value.M24;
					dstPtr[14] = value.M34;
				}
				else if (ColumnCount == 2 && RowCount == 2)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M21;
					dstPtr[4] = value.M12;
					dstPtr[5] = value.M22;
				}
				else
				{
					throw new NotImplementedException(
						"Matrix Size: " +
						RowCount.ToString() + " " +
						ColumnCount.ToString()
					);
				}
			}
		}

		public void SetValue(Matrix[] value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				float* dstPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 16)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M21;
						dstPtr[2] = value[i].M31;
						dstPtr[3] = value[i].M41;
						dstPtr[4] = value[i].M12;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M32;
						dstPtr[7] = value[i].M42;
						dstPtr[8] = value[i].M13;
						dstPtr[9] = value[i].M23;
						dstPtr[10] = value[i].M33;
						dstPtr[11] = value[i].M43;
						dstPtr[12] = value[i].M14;
						dstPtr[13] = value[i].M24;
						dstPtr[14] = value[i].M34;
						dstPtr[15] = value[i].M44;
					}
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 12)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M21;
						dstPtr[2] = value[i].M31;
						dstPtr[4] = value[i].M12;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M32;
						dstPtr[8] = value[i].M13;
						dstPtr[9] = value[i].M23;
						dstPtr[10] = value[i].M33;
					}
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 12)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M21;
						dstPtr[2] = value[i].M31;
						dstPtr[3] = value[i].M41;
						dstPtr[4] = value[i].M12;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M32;
						dstPtr[7] = value[i].M42;
						dstPtr[8] = value[i].M13;
						dstPtr[9] = value[i].M23;
						dstPtr[10] = value[i].M33;
						dstPtr[11] = value[i].M43;
					}
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 16)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M21;
						dstPtr[2] = value[i].M31;
						dstPtr[4] = value[i].M12;
						dstPtr[5] = value[i].M22;
						dstPtr[6] = value[i].M32;
						dstPtr[8] = value[i].M13;
						dstPtr[9] = value[i].M23;
						dstPtr[10] = value[i].M33;
						dstPtr[12] = value[i].M14;
						dstPtr[13] = value[i].M24;
						dstPtr[14] = value[i].M34;
					}
				}
				else if (ColumnCount == 2 && RowCount == 2)
				{
					for (int i = 0; i < value.Length; i += 1, dstPtr += 8)
					{
#if DEBUG
						value[i].CheckForNaNs();
#endif
						dstPtr[0] = value[i].M11;
						dstPtr[1] = value[i].M21;
						dstPtr[4] = value[i].M12;
						dstPtr[5] = value[i].M22;
					}
				}
				else
				{
					throw new NotImplementedException(
						"Matrix Size: " +
						RowCount.ToString() + " " +
						ColumnCount.ToString()
					);
				}
			}
		}

		public void SetValue(Quaternion value)
		{
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
				dstPtr[3] = value.W;
			}
		}

		public void SetValue(Quaternion[] value)
		{
			unsafe
			{
				float* dstPtr = (float*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
#if DEBUG
					value[i].CheckForNaNs();
#endif
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
					dstPtr[3] = value[i].W;
				}
			}
		}

		public void SetValue(float value)
		{
#if DEBUG
			if (float.IsNaN(value))
			{
				throw new InvalidOperationException("Effect parameter is NaN!");
			}
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				*dstPtr = value;
			}
		}

		public void SetValue(float[] value)
		{
#if DEBUG
			foreach (float f in value)
			{
				if (float.IsNaN(f))
				{
					throw new InvalidOperationException("Effect parameter contains NaN!");
				}
			}
#endif
			for (int i = 0, j = 0; i < value.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(value, i, values + j, ColumnCount);
			}
		}

		public void SetValue(string value)
		{
			/* FIXME: This requires digging into the effect->objects list.
			 * We've got the data, we just need to hook it up to FNA.
			 * -flibit
			 */
			throw new NotImplementedException("effect->objects[?]");
		}

		public void SetValue(Texture value)
		{
			texture = value;
		}

		public void SetValue(Vector2 value)
		{
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
			}
		}

		public void SetValue(Vector2[] value)
		{
			unsafe
			{
				float* dstPtr = (float*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
#if DEBUG
					value[i].CheckForNaNs();
#endif
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
				}
			}
		}

		public void SetValue(Vector3 value)
		{
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
			}
		}

		public void SetValue(Vector3[] value)
		{
			unsafe
			{
				float* dstPtr = (float*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
#if DEBUG
					value[i].CheckForNaNs();
#endif
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
				}
			}
		}

		public void SetValue(Vector4 value)
		{
#if DEBUG
			value.CheckForNaNs();
#endif
			unsafe
			{
				float* dstPtr = (float*) values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
				dstPtr[3] = value.W;
			}
		}

		public void SetValue(Vector4[] value)
		{
			unsafe
			{
				float* dstPtr = (float*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
#if DEBUG
					value[i].CheckForNaNs();
#endif
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
					dstPtr[3] = value[i].W;
				}
			}
		}

		#endregion
	}
}
