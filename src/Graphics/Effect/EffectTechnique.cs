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
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class EffectTechnique
	{
		#region Public Properties

		public string Name
		{
			get;
			private set;
		}

		public EffectPassCollection Passes
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

		#region Internal Properties

		internal IntPtr TechniquePointer
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal EffectTechnique(
			string name,
			IntPtr pointer,
			EffectPassCollection passes,
			EffectAnnotationCollection annotations
		) {
			Name = name;
			Passes = passes;
			Annotations = annotations;
			TechniquePointer = pointer;
		}

		#endregion
	}
}
