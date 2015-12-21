#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class SamplerStateCollection
	{
		#region Public Array Access Property

		public SamplerState this[int index]
		{
			get
			{
				return samplers[index];
			}
			set
			{
				samplers[index] = value;
				if (!modifiedSamplers.Contains(index))
				{
					modifiedSamplers.Enqueue(index);
				}
			}
		}

		#endregion

		#region Private Variables

		private readonly SamplerState[] samplers;
		private readonly Queue<int> modifiedSamplers;

		#endregion

		#region Internal Constructor

		internal SamplerStateCollection(
			int slots,
			Queue<int> modSamplers
		) {
			samplers = new SamplerState[slots];
			modifiedSamplers = modSamplers;
			for (int i = 0; i < samplers.Length; i += 1)
			{
				samplers[i] = SamplerState.LinearWrap;
			}
		}

		#endregion
	}
}
