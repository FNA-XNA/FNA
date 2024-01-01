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

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class EffectPass
	{
		#region Public Properties

		public string Name
		{
			get;
			private set;
		}

		public EffectAnnotationCollection Annotations
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private Effect parentEffect;
		private IntPtr parentTechnique;
		private uint pass;

		#endregion

		#region Internal Constructor

		internal EffectPass(
			string name,
			EffectAnnotationCollection annotations,
			Effect parent,
			IntPtr technique,
			uint passIndex
		) {
			Name = name;
			Annotations = annotations;
			parentEffect = parent;
			parentTechnique = technique;
			pass = passIndex;
		}

		#endregion

		#region Public Methods

		public void Apply()
		{
			if (parentTechnique != parentEffect.CurrentTechnique.TechniquePointer)
			{
				throw new InvalidOperationException(
					"Applied a pass not in the current technique!"
				);
			}
			parentEffect.OnApply();
			parentEffect.INTERNAL_applyEffect(pass);
		}

		#endregion
	}
}
