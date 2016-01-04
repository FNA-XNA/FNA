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

namespace Microsoft.Xna.Framework
{
	public class GameComponentCollectionEventArgs : EventArgs
	{
		#region Public Properties

		public IGameComponent GameComponent
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructors

		public GameComponentCollectionEventArgs(IGameComponent gameComponent)
		{
			GameComponent = gameComponent;
		}

		#endregion
	}
}
