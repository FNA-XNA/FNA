#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
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
				modifiedSamplers[index] = true;
			}
		}

		#endregion

		#region Private Variables

		private readonly SamplerState[] samplers;
		private readonly bool[] modifiedSamplers;

		#endregion

		#region Internal Constructor

		internal SamplerStateCollection(
			int slots,
			bool[] modSamplers
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
