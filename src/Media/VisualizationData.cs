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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public class VisualizationData
	{
		#region Public Properties

		public ReadOnlyCollection<float> Frequencies
		{
			get;
			private set;
		}

		public ReadOnlyCollection<float> Samples
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constants

		internal const int Size = 256;

		#endregion

		#region Private Variables

		private List<float> freqList;
		private List<float> sampList;

		#endregion

		#region Public Constructor

		public VisualizationData()
		{
			freqList = new List<float>(Size);
			sampList = new List<float>(Size);
			freqList.AddRange(new float[Size]);
			sampList.AddRange(new float[Size]);
			Frequencies = new ReadOnlyCollection<float>(freqList);
			Samples = new ReadOnlyCollection<float>(sampList);
		}

		#endregion

		#region Internal Methods

		internal void CalculateData(Song curSong)
		{
			float[] samples = curSong.GetSamples();

			for (int i = 0; i < Size; i += 1)
			{
				sampList[i] = samples[i * curSong.chunkStep];
				freqList[i] = 0.0f; // TODO: Frequencies -flibit
			}
		}

		#endregion
	}
}
