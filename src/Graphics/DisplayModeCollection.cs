#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion License

#region Using Statements
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class DisplayModeCollection : IEnumerable<DisplayMode>, IEnumerable
	{
		#region Public Properties

		public IEnumerable<DisplayMode> this[SurfaceFormat format]
		{
			get
			{
				List<DisplayMode> list = new List<DisplayMode>();
				foreach (DisplayMode mode in this.modes)
				{
					if (mode.Format == format)
					{
						list.Add(mode);
					}
				}
				return list;
			}
		}

		#endregion

		#region Private Variables

		private readonly List<DisplayMode> modes;

		#endregion

		#region Public Constructor

		public DisplayModeCollection(List<DisplayMode> setmodes)
		{
			modes = setmodes;
		}

		#endregion

		#region Public Methods

		public IEnumerator<DisplayMode> GetEnumerator()
		{
			return modes.GetEnumerator();
		}

		#endregion

		#region Private Methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return modes.GetEnumerator();
		}

		#endregion
	}
}
