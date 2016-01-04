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

		Dictionary<Type, object> services;

		#endregion

		#region Public Constructors

		public GameServiceContainer()
		{
			services = new Dictionary<Type, object>();
		}

		#endregion

		#region Public Methods

		public void AddService(Type type, object provider)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			if (!type.IsAssignableFrom(provider.GetType()))
			{
				throw new ArgumentException(
					"The provider does not match the specified service type!"
				);
			}

			services.Add(type, provider);
		}

		public object GetService(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
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
				throw new ArgumentNullException("type");
			}

			services.Remove(type);
		}

		#endregion
	}
}
