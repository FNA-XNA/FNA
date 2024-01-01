#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public class VisualizationData
	{
		#region Public Properties

		public ReadOnlyCollection<float> Frequencies
		{
			get
			{
				return new ReadOnlyCollection<float>(freq);
			}
		}

		public ReadOnlyCollection<float> Samples
		{
			get
			{
				return new ReadOnlyCollection<float>(samp);
			}
		}

		#endregion

		#region Internal Constants

		internal const int Size = 256;

		#endregion

		#region Internal Variables

		internal float[] freq;
		internal float[] samp;

		#endregion

		#region Public Constructor

		public VisualizationData()
		{
			freq = new float[Size];
			samp = new float[Size];
		}

		#endregion
	}
}
