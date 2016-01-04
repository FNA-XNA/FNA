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
			private set;
		}

		public string RendererId
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal RendererDetail(string name, string id) : this()
		{
			FriendlyName = name;
			RendererId = id;
		}

		#endregion

		#region Public Methods

		public override bool Equals(object obj)
		{
			return (	(obj is RendererDetail) &&
					RendererId.Equals(((RendererDetail) obj).RendererId)	);
		}

		public override int GetHashCode()
		{
			return RendererId.GetHashCode();
		}

		public override string ToString()
		{
			return FriendlyName;
		}
		
		#endregion

		#region Public Static Operator Overloads

		public static bool operator==(RendererDetail left, RendererDetail right)
		{
			return left.RendererId.Equals(right.RendererId);
		}

		public static bool operator!=(RendererDetail left, RendererDetail right)
		{
			return !left.RendererId.Equals(right.RendererId);
		}

		#endregion
	}
}
