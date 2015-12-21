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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class EffectAnnotation
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

		#endregion

		#region Private Variables

		IntPtr values;

		#endregion

		#region Internal Constructor

		internal EffectAnnotation(
			string name,
			string semantic,
			int rowCount,
			int columnCount,
			EffectParameterClass parameterClass,
			EffectParameterType parameterType,
			IntPtr data
		) {
			Name = name;
			Semantic = semantic;
			RowCount = rowCount;
			ColumnCount = columnCount;
			ParameterClass = parameterClass;
			ParameterType = parameterType;
			values = data;
		}

		#endregion

		#region Public Methods

		public bool GetValueBoolean()
		{
			unsafe
			{
				// Values are always 4 bytes, so we get to do this. -flibit
				int* resPtr = (int*) values;
				return *resPtr != 0;
			}
		}

		public int GetValueInt32()
		{
			unsafe
			{
				int* resPtr = (int*) values;
				return *resPtr;
			}
		}

		public Matrix GetValueMatrix()
		{
			// FIXME: This assumes 4x4! -flibit
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

		public float GetValueSingle()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return *resPtr;
			}
		}

		public string GetValueString()
		{
			/* FIXME: This requires digging into the effect->objects list.
			 * We've got the data, we just need to hook it up to FNA.
			 * -flibit
			 */
			throw new NotImplementedException("effect->objects[?]");
		}

		public Vector2 GetValueVector2()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Vector2(resPtr[0], resPtr[1]);
			}
		}

		public Vector3 GetValueVector3()
		{
			unsafe
			{
				float* resPtr = (float*) values;
				return new Vector3(resPtr[0], resPtr[1], resPtr[2]);
			}
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

		#endregion
	}
}
