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

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct NormalizedByte4 : IPackedVector<uint>, IEquatable<NormalizedByte4>
	{
		#region Public Properties

		[CLSCompliant(false)]
		public uint PackedValue
		{
			get
			{
				return packedValue;
			}
			set
			{
				packedValue = value;
			}
		}

		#endregion

		#region Private Variables

		private uint packedValue;

		#endregion

		#region Public Constructors

		public NormalizedByte4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		public NormalizedByte4(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		#endregion

		#region Public Methods

		public Vector4 ToVector4()
		{
			return new Vector4(
				((sbyte) (packedValue & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 8) & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 16) & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 24) & 0xFF)) / 127.0f
			);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
		{
			return a.packedValue != b.packedValue;
		}

		public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
		{
			return a.packedValue == b.packedValue;
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedByte4) && Equals((NormalizedByte4) obj);
		}

		public bool Equals(NormalizedByte4 other)
		{
			return packedValue == other.packedValue;
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		#endregion

		#region Private Static Pack Method

		private static uint Pack(float x, float y, float z, float w)
		{
			return (
				((((uint) (MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) << 0) & 0x000000FF) |
				((((uint) (MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) << 8) & 0x0000FF00) |
				((((uint) (MathHelper.Clamp(z, -1.0f, 1.0f) * 127.0f)) << 16) & 0x00FF0000) |
				((((uint) (MathHelper.Clamp(w, -1.0f, 1.0f) * 127.0f)) << 24) & 0xFF000000)
			);
		}

		#endregion
	}
}
