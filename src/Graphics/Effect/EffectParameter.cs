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
			get;
			private set;
		}

		public EffectParameterCollection StructureMembers
		{
			get;
			private set;
		}

		public EffectAnnotationCollection Annotations
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		internal Texture texture;

		#endregion

		#region Private Variables

		private IntPtr values;

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
			EffectParameterCollection structureMembers,
			EffectAnnotationCollection annotations,
			IntPtr data
		) {
			Name = name;
			Semantic = semantic;
			RowCount = rowCount;
			ColumnCount = columnCount;
			if (elementCount > 0)
			{
				List<EffectParameter> elements = new List<EffectParameter>(elementCount);
				for (int i = 0; i < elementCount; i += 1)
				{
					// FIXME: Probably incomplete? -flibit
					elements.Add(new EffectParameter(
						null,
						null,
						rowCount,
						columnCount,
						0,
						ParameterClass,
						parameterType,
						null, // FIXME: See mojoshader_effects.c:readvalue -flibit
						null,
						new IntPtr(
							data.ToInt64() + (i * rowCount * columnCount)
						)
					));
				}
				Elements = new EffectParameterCollection(elements);
			}
			ParameterClass = parameterClass;
			ParameterType = parameterType;
			StructureMembers = structureMembers;
			Annotations = annotations;
			values = data;
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
				for (int i = 0; i < count; i += 1)
				{
					result[i] = resPtr[i] != 0;
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
			Marshal.Copy(values, result, 0, count);
			return result;
		}

		public Matrix GetValueMatrix()
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				float* resPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
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
				else if (ColumnCount == 3 && RowCount == 3)
				{
					return new Matrix(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						0.0f,
						resPtr[3],
						resPtr[4],
						resPtr[5],
						0.0f,
						resPtr[6],
						resPtr[7],
						resPtr[8],
						0.0f,
						0.0f,
						0.0f,
						0.0f,
						0.0f
					);
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					return new Matrix(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						0.0f,
						resPtr[3],
						resPtr[4],
						resPtr[5],
						0.0f,
						resPtr[6],
						resPtr[7],
						resPtr[8],
						0.0f,
						resPtr[9],
						resPtr[10],
						resPtr[11],
						0.0f
					);
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
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
						0.0f,
						0.0f,
						0.0f,
						0.0f
					);
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

		public Matrix[] GetValueMatrixArray(int count)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			Matrix[] result = new Matrix[count];
			unsafe
			{
				float* resPtr = (float*) values;
				int curOffset = 0;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 1],
							resPtr[curOffset + 2],
							resPtr[curOffset + 3],
							resPtr[curOffset + 4],
							resPtr[curOffset + 5],
							resPtr[curOffset + 6],
							resPtr[curOffset + 7],
							resPtr[curOffset + 8],
							resPtr[curOffset + 9],
							resPtr[curOffset + 10],
							resPtr[curOffset + 11],
							resPtr[curOffset + 12],
							resPtr[curOffset + 13],
							resPtr[curOffset + 14],
							resPtr[curOffset + 15]
						);
					}
					curOffset += 16;
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 1],
							resPtr[curOffset + 2],
							0.0f,
							resPtr[curOffset + 3],
							resPtr[curOffset + 4],
							resPtr[curOffset + 5],
							0.0f,
							resPtr[curOffset + 6],
							resPtr[curOffset + 7],
							resPtr[curOffset + 8],
							0.0f,
							0.0f,
							0.0f,
							0.0f,
							0.0f
						);
					}
					curOffset += 9;
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 1],
							resPtr[curOffset + 2],
							0.0f,
							resPtr[curOffset + 3],
							resPtr[curOffset + 4],
							resPtr[curOffset + 5],
							0.0f,
							resPtr[curOffset + 6],
							resPtr[curOffset + 7],
							resPtr[curOffset + 8],
							0.0f,
							resPtr[curOffset + 9],
							resPtr[curOffset + 10],
							resPtr[curOffset + 11],
							0.0f
						);
					}
					curOffset += 12;
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 1],
							resPtr[curOffset + 2],
							resPtr[curOffset + 3],
							resPtr[curOffset + 4],
							resPtr[curOffset + 5],
							resPtr[curOffset + 6],
							resPtr[curOffset + 7],
							resPtr[curOffset + 8],
							resPtr[curOffset + 9],
							resPtr[curOffset + 10],
							resPtr[curOffset + 11],
							0.0f,
							0.0f,
							0.0f,
							0.0f
						);
					}
					curOffset += 12;
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
			return result;
		}

		public Matrix GetValueMatrixTranspose()
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				float* resPtr = (float*) values;
				if (ColumnCount == 4 && RowCount == 4)
				{
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
				else if (ColumnCount == 3 && RowCount == 3)
				{
					return new Matrix(
						resPtr[0],
						resPtr[3],
						resPtr[6],
						0.0f,
						resPtr[1],
						resPtr[4],
						resPtr[7],
						0.0f,
						resPtr[2],
						resPtr[5],
						resPtr[8],
						0.0f,
						0.0f,
						0.0f,
						0.0f,
						0.0f
					);
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					return new Matrix(
						resPtr[0],
						resPtr[4],
						resPtr[8],
						0.0f,
						resPtr[1],
						resPtr[5],
						resPtr[9],
						0.0f,
						resPtr[2],
						resPtr[6],
						resPtr[10],
						0.0f,
						resPtr[3],
						resPtr[7],
						resPtr[11],
						0.0f
					);
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					return new Matrix(
						resPtr[0],
						resPtr[3],
						resPtr[6],
						resPtr[9],
						resPtr[1],
						resPtr[4],
						resPtr[7],
						resPtr[10],
						resPtr[2],
						resPtr[5],
						resPtr[8],
						resPtr[11],
						0.0f,
						0.0f,
						0.0f,
						0.0f
					);
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

		public Matrix[] GetValueMatrixTransposeArray(int count)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			Matrix[] result = new Matrix[count];
			unsafe
			{
				float* resPtr = (float*) values;
				int curOffset = 0;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset + 0],
							resPtr[curOffset + 4],
							resPtr[curOffset + 8],
							resPtr[curOffset + 12],
							resPtr[curOffset + 1],
							resPtr[curOffset + 5],
							resPtr[curOffset + 9],
							resPtr[curOffset + 13],
							resPtr[curOffset + 2],
							resPtr[curOffset + 6],
							resPtr[curOffset + 10],
							resPtr[curOffset + 14],
							resPtr[curOffset + 3],
							resPtr[curOffset + 7],
							resPtr[curOffset + 11],
							resPtr[curOffset + 15]
						);
						curOffset += 16;
					}
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 3],
							resPtr[curOffset + 6],
							0.0f,
							resPtr[curOffset + 1],
							resPtr[curOffset + 4],
							resPtr[curOffset + 7],
							0.0f,
							resPtr[curOffset + 2],
							resPtr[curOffset + 5],
							resPtr[curOffset + 8],
							0.0f,
							0.0f,
							0.0f,
							0.0f,
							0.0f
						);
						curOffset += 9;
					}
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 4],
							resPtr[curOffset + 8],
							0.0f,
							resPtr[curOffset + 1],
							resPtr[curOffset + 5],
							resPtr[curOffset + 9],
							0.0f,
							resPtr[curOffset + 2],
							resPtr[curOffset + 6],
							resPtr[curOffset + 10],
							0.0f,
							resPtr[curOffset + 3],
							resPtr[curOffset + 7],
							resPtr[curOffset + 11],
							0.0f
						);
						curOffset += 12;
					}
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < count; i += 1)
					{
						result[i] = new Matrix(
							resPtr[curOffset],
							resPtr[curOffset + 3],
							resPtr[curOffset + 6],
							resPtr[curOffset + 9],
							resPtr[curOffset + 1],
							resPtr[curOffset + 4],
							resPtr[curOffset + 7],
							resPtr[curOffset + 10],
							resPtr[curOffset + 2],
							resPtr[curOffset + 5],
							resPtr[curOffset + 8],
							resPtr[curOffset + 11],
							0.0f,
							0.0f,
							0.0f,
							0.0f
						);
						curOffset += 12;
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
			return result;
		}

		public Quaternion GetValueQuaternion()
		{
			// FIXME: Is this really a thing Effects do? -flibit
			throw new NotImplementedException("Quaternions?");
		}

		public Quaternion[] GetValueQuaternionArray(int count)
		{
			// FIXME: Is this really a thing Effects do? -flibit
			throw new NotImplementedException("Quaternions?");
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
			Marshal.Copy(values, result, 0, count);
			return result;
		}

		public string GetValueString()
		{
			/* FIXME: This requires digging into the effect->objects list.
			 * We've got the data, we just need to hook it up to FNA.
			 * -flibit
			 */
			throw new NotImplementedException("effect->objects[?]");
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
				int curOffset = 0;
				for (int i = 0; i < count; i += 1)
				{
					result[i] = new Vector2(
						resPtr[curOffset++],
						resPtr[curOffset++]
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
				int curOffset = 0;
				for (int i = 0; i < count; i += 1)
				{
					result[i] = new Vector3(
						resPtr[curOffset++],
						resPtr[curOffset++],
						resPtr[curOffset++]
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
				int curOffset = 0;
				for (int i = 0; i < count; i += 1)
				{
					result[i] = new Vector4(
						resPtr[curOffset++],
						resPtr[curOffset++],
						resPtr[curOffset++],
						resPtr[curOffset++]
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
				for (int i = 0; i < value.Length; i += 1)
				{
					// Ugh, this branch, stupid C#.
					dstPtr[i] = value[i] ? 1 : 0;
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
			Marshal.Copy(value, 0, values, value.Length);
		}

		public void SetValue(Matrix value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
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
					dstPtr[3] = value.M21;
					dstPtr[4] = value.M22;
					dstPtr[5] = value.M23;
					dstPtr[6] = value.M31;
					dstPtr[7] = value.M32;
					dstPtr[8] = value.M33;
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					dstPtr[0] = value.M11;
					dstPtr[1] = value.M12;
					dstPtr[2] = value.M13;
					dstPtr[3] = value.M21;
					dstPtr[4] = value.M22;
					dstPtr[5] = value.M23;
					dstPtr[6] = value.M31;
					dstPtr[7] = value.M32;
					dstPtr[8] = value.M33;
					dstPtr[9] = value.M41;
					dstPtr[10] = value.M42;
					dstPtr[11] = value.M43;
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
				int curOffset = 0;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M14;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M24;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M34;
						dstPtr[curOffset++] = value[i].M41;
						dstPtr[curOffset++] = value[i].M42;
						dstPtr[curOffset++] = value[i].M43;
						dstPtr[curOffset++] = value[i].M44;
					}
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M33;
					}
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M41;
						dstPtr[curOffset++] = value[i].M42;
						dstPtr[curOffset++] = value[i].M43;
					}
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M14;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M24;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M34;
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

		public void SetValueTranspose(Matrix value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
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
					dstPtr[3] = value.M12;
					dstPtr[4] = value.M22;
					dstPtr[5] = value.M32;
					dstPtr[6] = value.M13;
					dstPtr[7] = value.M23;
					dstPtr[8] = value.M33;
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
					dstPtr[3] = value.M12;
					dstPtr[4] = value.M22;
					dstPtr[5] = value.M32;
					dstPtr[6] = value.M13;
					dstPtr[7] = value.M23;
					dstPtr[8] = value.M33;
					dstPtr[9] = value.M14;
					dstPtr[10] = value.M24;
					dstPtr[11] = value.M34;
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
				int curOffset = 0;
				if (ColumnCount == 4 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M41;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M42;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M43;
						dstPtr[curOffset++] = value[i].M14;
						dstPtr[curOffset++] = value[i].M24;
						dstPtr[curOffset++] = value[i].M34;
						dstPtr[curOffset++] = value[i].M44;
					}
				}
				else if (ColumnCount == 3 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M33;
					}
				}
				else if (ColumnCount == 4 && RowCount == 3)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M41;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M42;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M43;
					}
				}
				else if (ColumnCount == 3 && RowCount == 4)
				{
					for (int i = 0; i < value.Length; i += 1)
					{
						dstPtr[curOffset++] = value[i].M11;
						dstPtr[curOffset++] = value[i].M21;
						dstPtr[curOffset++] = value[i].M31;
						dstPtr[curOffset++] = value[i].M12;
						dstPtr[curOffset++] = value[i].M22;
						dstPtr[curOffset++] = value[i].M32;
						dstPtr[curOffset++] = value[i].M13;
						dstPtr[curOffset++] = value[i].M23;
						dstPtr[curOffset++] = value[i].M33;
						dstPtr[curOffset++] = value[i].M14;
						dstPtr[curOffset++] = value[i].M24;
						dstPtr[curOffset++] = value[i].M34;
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
			// FIXME: Is this really a thing Effects do? -flibit
			throw new NotImplementedException("Quaternions?");
		}

		public void SetValue(Quaternion[] value)
		{
			// FIXME: Is this really a thing Effects do? -flibit
			throw new NotImplementedException("Quaternions?");
		}

		public void SetValue(float value)
		{
			unsafe
			{
				float* dstPtr = (float*) values;
				*dstPtr = value;
			}
		}

		public void SetValue(float[] value)
		{
			Marshal.Copy(value, 0, values, value.Length);
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
				int curOffset = 0;
				for (int i = 0; i < value.Length; i += 1)
				{
					dstPtr[curOffset++] = value[i].X;
					dstPtr[curOffset++] = value[i].Y;
				}
			}
		}

		public void SetValue(Vector3 value)
		{
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
				int curOffset = 0;
				for (int i = 0; i < value.Length; i += 1)
				{
					dstPtr[curOffset++] = value[i].X;
					dstPtr[curOffset++] = value[i].Y;
					dstPtr[curOffset++] = value[i].Z;
				}
			}
		}

		public void SetValue(Vector4 value)
		{
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
				int curOffset = 0;
				for (int i = 0; i < value.Length; i += 1)
				{
					dstPtr[curOffset++] = value[i].X;
					dstPtr[curOffset++] = value[i].Y;
					dstPtr[curOffset++] = value[i].Z;
					dstPtr[curOffset++] = value[i].W;
				}
			}
		}

		#endregion
	}
}
