#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework
{
	public class GameServiceContainer : IServiceProvider
	{
		#region Private Fields

		readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

		#endregion

		#region Public Methods

		public void AddService(Type type, object provider)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type", "The service type cannot be null.");
			}
			if (provider == null)
			{
				throw new ArgumentNullException("provider", "The service provider instance cannot be null.");
			}
			if (!type.IsAssignableFrom(provider.GetType()))
			{
				throw new ArgumentException(
					"Service provider object of type " + provider.GetType().FullName +
					" must be assignable to service type " + type.GetType().FullName + "."
				);
			}

			services.Add(type, provider);
		}

		public object GetService(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type", "The service type cannot be null.");
			}

			object service;
			if (services.TryGetValue(type, out service))
			{
				return service;
			}

			return null;
		}

		public void RemoveService(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type", "The service type cannot be null.");
			}

			services.Remove(type);
		}

		#endregion
	}
}
