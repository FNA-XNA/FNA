#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public struct VertexElement
	{
		#region Public Properties

		public int Offset
		{
			get;
			set;
		}

		public VertexElementFormat VertexElementFormat
		{
			get;
			set;
		}

		public VertexElementUsage VertexElementUsage
		{
			get;
			set;
		}

		public int UsageIndex
		{
			get;
			set;
		}

		#endregion

		#region Public Constructor

		public VertexElement(
			int offset,
			VertexElementFormat elementFormat,
			VertexElementUsage elementUsage,
			int usageIndex
		) : this() {
			Offset = offset;
			UsageIndex = usageIndex;
			VertexElementFormat = elementFormat;
			VertexElementUsage = elementUsage;
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override int GetHashCode()
		{
			// TODO: Fix hashes
			return 0;
		}

		public override string ToString()
		{
			return (
				"{{Offset:" + Offset.ToString() +
				" Format:" + VertexElementFormat.ToString() +
				" Usage:" + VertexElementUsage.ToString() +
				" UsageIndex: " + UsageIndex.ToString() +
				"}}"
			);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != base.GetType())
			{
				return false;
			}
			return (this == ((VertexElement) obj));
		}

		public static bool operator ==(VertexElement left, VertexElement right)
		{
			return (	(left.Offset == right.Offset) &&
					(left.UsageIndex == right.UsageIndex) &&
					(left.VertexElementUsage == right.VertexElementUsage) &&
					(left.VertexElementFormat == right.VertexElementFormat)	);
		}

		public static bool operator !=(VertexElement left, VertexElement right)
		{
			return !(left == right);
		}

		#endregion
	}
}
