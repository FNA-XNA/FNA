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
	public interface IDrawable
	{
		int DrawOrder
		{
			get;
		}

		bool Visible
		{
			get;
		}

		event EventHandler<EventArgs> DrawOrderChanged;
		event EventHandler<EventArgs> VisibleChanged;

		void Draw(GameTime gameTime);
	}
}
