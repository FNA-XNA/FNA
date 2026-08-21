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
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	[Serializable]
	public struct RendererDetail
	{
		#region Public Properties

		public string FriendlyName
		{
			get;
			internal set;
		}

		public string RendererId
		{
			get;
			internal set;
		}

		#endregion

		#region Public Methods

		public override bool Equals(object obj)
		{
			return obj is RendererDetail && this == (RendererDetail) obj;
		}

		public override int GetHashCode()
		{
			return (string.IsNullOrEmpty(RendererId) ? 0 : RendererId.GetHashCode()) ^
				(string.IsNullOrEmpty(FriendlyName) ? 0 : FriendlyName.GetHashCode());
		}
		
		#endregion

		#region Public Static Operator Overloads

		public static bool operator==(RendererDetail left, RendererDetail right)
		{
			return left.FriendlyName == right.FriendlyName && left.RendererId == right.RendererId;
		}

		public static bool operator!=(RendererDetail left, RendererDetail right)
		{
			return !(left == right);
		}

		#endregion
	}
}
