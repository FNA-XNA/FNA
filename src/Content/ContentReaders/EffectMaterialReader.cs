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
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	internal class EffectMaterialReader : ContentTypeReader<EffectMaterial>
	{
		#region Protected Read Method

		protected internal override EffectMaterial Read(
			ContentReader input,
			EffectMaterial existingInstance
		) {
			Effect effect = input.ReadExternalReference<Effect>();
			EffectMaterial effectMaterial = new EffectMaterial(effect);
			Dictionary<string, object> dict = input.ReadObject<Dictionary<string, object>>();
			foreach (KeyValuePair<string, object> item in dict) {
				EffectParameter parameter = effectMaterial.Parameters[item.Key];
				if (parameter != null)
				{
					Type itemType = item.Value.GetType();
					if (typeof(Texture).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Texture) item.Value);
					}
					else if (typeof(int).IsAssignableFrom(itemType))
					{
						parameter.SetValue((int) item.Value);
					}
					else if (typeof(bool).IsAssignableFrom(itemType))
					{
						parameter.SetValue((bool) item.Value);
					}
					else if (typeof(float).IsAssignableFrom(itemType))
					{
						parameter.SetValue((float) item.Value);
					}
					else if (typeof(float[]).IsAssignableFrom(itemType))
					{
						parameter.SetValue((float[]) item.Value);
					}
					else if (typeof(Vector2).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector2) item.Value);
					}
					else if (typeof(Vector2[]).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector2[]) item.Value);
					}
					else if (typeof(Vector3).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector3) item.Value);
					}
					else if (typeof(Vector3[]).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector3[]) item.Value);
					}
					else if (typeof(Vector4).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector4) item.Value);
					}
					else if (typeof(Vector4[]).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Vector4[]) item.Value);
					}
					else if (typeof(Matrix).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Matrix) item.Value);
					}
					else if (typeof(Matrix[]).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Matrix[]) item.Value);
					}
					else if (typeof(Quaternion).IsAssignableFrom(itemType))
					{
						parameter.SetValue((Quaternion) item.Value);
					}
					else
					{
						throw new NotSupportedException("Parameter type is not supported");
					}
				}
				else
				{
					Debug.WriteLine("No parameter " + item.Key);
				}
			}
			return effectMaterial;
		}

		#endregion
	}
}
