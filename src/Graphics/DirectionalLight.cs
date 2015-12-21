#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class DirectionalLight
	{
		#region Public Properties

		private Vector3 INTERNAL_diffuseColor;
		public Vector3 DiffuseColor
		{
			get
			{
				return INTERNAL_diffuseColor;
			}
			set
			{
				INTERNAL_diffuseColor = value;
				if (Enabled && diffuseColorParameter != null)
				{
					diffuseColorParameter.SetValue(INTERNAL_diffuseColor);
				}
			}
		}

		private Vector3 INTERNAL_direction;
		public Vector3 Direction
		{
			get
			{
				return INTERNAL_direction;
			}
			set
			{
				INTERNAL_direction = value;
				if (directionParameter != null)
				{
					directionParameter.SetValue(INTERNAL_direction);
				}
			}
		}

		private Vector3 INTERNAL_specularColor;
		public Vector3 SpecularColor
		{
			get
			{
				return INTERNAL_specularColor;
			}
			set
			{
				INTERNAL_specularColor = value;
				if (Enabled && specularColorParameter != null)
				{
					specularColorParameter.SetValue(INTERNAL_specularColor);
				}
			}
		}

		private bool INTERNAL_enabled;
		public bool Enabled
		{
			get
			{
				return INTERNAL_enabled;
			}
			set
			{
				if (INTERNAL_enabled != value)
				{
					INTERNAL_enabled = value;
					if (INTERNAL_enabled)
					{
						if (diffuseColorParameter != null)
						{
							diffuseColorParameter.SetValue(DiffuseColor);
						}
						if (specularColorParameter != null)
						{
							specularColorParameter.SetValue(SpecularColor);
						}
					}
					else
					{
						if (diffuseColorParameter != null)
						{
							diffuseColorParameter.SetValue(Vector3.Zero);
						}
						if (specularColorParameter != null)
						{
							specularColorParameter.SetValue(Vector3.Zero);
						}
					}
				}

			}
		}

		#endregion

		#region Internal Variables

		internal EffectParameter diffuseColorParameter;
		internal EffectParameter directionParameter;
		internal EffectParameter specularColorParameter;

		#endregion

		#region Public Constructor

		public DirectionalLight(
			EffectParameter directionParameter,
			EffectParameter diffuseColorParameter,
			EffectParameter specularColorParameter,
			DirectionalLight cloneSource
		) {
			this.diffuseColorParameter = diffuseColorParameter;
			this.directionParameter = directionParameter;
			this.specularColorParameter = specularColorParameter;
			if (cloneSource != null)
			{
				DiffuseColor = cloneSource.DiffuseColor;
				Direction = cloneSource.Direction;
				SpecularColor = cloneSource.SpecularColor;
				Enabled = cloneSource.Enabled;
			}
		}

		#endregion
	}
}
