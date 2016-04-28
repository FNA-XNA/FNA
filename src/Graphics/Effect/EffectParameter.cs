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

		internal IntPtr values;

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
				int curOffset = 0;
				List<EffectParameter> elements = new List<EffectParameter>(elementCount);
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
							memList.Add(new EffectParameter(
								structureMembers[j].Name,
								structureMembers[j].Semantic,
								structureMembers[j].RowCount,
								structureMembers[j].ColumnCount,
								memElems,
								structureMembers[j].ParameterClass,
								structureMembers[j].ParameterType,
								null, // FIXME: Nested structs! -flibit
								structureMembers[j].Annotations,
								new IntPtr(data.ToInt64() + curOffset)
							));
							int memSize = structureMembers[j].RowCount * 4;
							if (memElems > 0)
							{
								memSize *= memElems;
							}
							curOffset += memSize * 4;
						}
						elementMembers = new EffectParameterCollection(memList);
					}
					// FIXME: Probably incomplete? -flibit
					elements.Add(new EffectParameter(
						null,
						null,
						rowCount,
						columnCount,
						0,
						ParameterClass,
						parameterType,
						elementMembers,
						null,
						new IntPtr(
							data.ToInt64() + (i * rowCount * 16)
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
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = *resPtr != 0;
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
			unsafe
			{
				int* resPtr = (int*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = *resPtr;
				}
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
			unsafe
			{
				float* resPtr = (float*) values;
				for (int i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = *resPtr;
				}
			}
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
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					// Ugh, this branch, stupid C#.
					*dstPtr = value[i] ? 1 : 0;
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
			unsafe
			{
				int* dstPtr = (int*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					*dstPtr = value[i];
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
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
					dstPtr[3] = value[i].W;
				}
			}
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
			unsafe
			{
				float* dstPtr = (float*) values;
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					*dstPtr = value[i];
				}
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
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
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
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
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
				for (int i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
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
