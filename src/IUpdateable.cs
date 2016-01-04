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
	public interface IUpdateable
	{
		#region Properties

		bool Enabled
		{
			get;
		}

		int UpdateOrder
		{
			get;
		}

		#endregion

		#region Events

		event EventHandler<EventArgs> EnabledChanged;
		event EventHandler<EventArgs> UpdateOrderChanged;

		#endregion

		#region Methods

		void Update(GameTime gameTime);

		#endregion
	}
}
