#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#if NET5_0

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Xml;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class FNADllMap
	{
		#region Private Static Variables

		private static Dictionary<string, string> mapDictionary
			= new Dictionary<string, string>();

		#endregion

		#region Private Static Methods

		private static string GetPlatformName()
		{
			if (OperatingSystem.IsWindows())
			{
				return "windows";
			}
			else if (OperatingSystem.IsMacOS())
			{
				return  "osx";
			}
			else if (OperatingSystem.IsLinux())
			{
				return "linux";
			}
			else if (OperatingSystem.IsFreeBSD())
			{
				return "freebsd";
			}
			else
			{
				// Maybe this platform statically links?
				return "unknown";
			}
		}

		#endregion

		#region DllImportResolver Callback Method

		private static IntPtr MapAndLoad(
			string libraryName,
			Assembly assembly,
			DllImportSearchPath? dllImportSearchPath
		) {
			string mappedName;
			if (!mapDictionary.TryGetValue(libraryName, out mappedName))
			{
				mappedName = libraryName;
			}
			return NativeLibrary.Load(mappedName, assembly, dllImportSearchPath);
		}

		#endregion

		#region Module Initializer

		[ModuleInitializer]
		public static void Init()
		{
			// Get the platform and architecture
			string os = GetPlatformName();
			string cpu = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

			// Locate the config XML
			Assembly assembly = Assembly.GetCallingAssembly();
			string xmlPath = Path.Combine(
				Path.GetDirectoryName(assembly.Location),
				Path.GetFileNameWithoutExtension(assembly.Location) + ".dll.config"
			);
			if (!File.Exists(xmlPath))
			{
				// Maybe it's called app.config?
				xmlPath = Path.Combine(
					Path.GetDirectoryName(assembly.Location),
					"app.config"
				);
				if (!File.Exists(xmlPath))
				{
					// Let's hope for the best...
					return;
				}
			}

			// Parse the XML into a mapping dictionary
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlPath);

			foreach (XmlNode node in xmlDoc.GetElementsByTagName("dllmap"))
			{
				// Ignore entries for other OSs
				if (!node.Attributes["os"].Value.Contains(os))
				{
					continue;
				}

				// Ignore entries for other CPUs
				XmlAttribute cpuAttribute = node.Attributes["cpu"];
				if (cpuAttribute != null && !cpuAttribute.Value.Contains(cpu))
				{
					continue;
				}

				// Find a mapping
				string oldLib = node.Attributes["dll"].Value;
				string newLib = node.Attributes["target"].Value;
				if (string.IsNullOrWhiteSpace(oldLib) || string.IsNullOrWhiteSpace(newLib))
				{
					continue;
				}

				// Don't allow duplicates
				if (mapDictionary.ContainsKey(oldLib))
				{
					continue;
				}

				mapDictionary.Add(oldLib, newLib);
			}

			// Set the resolver callback
			NativeLibrary.SetDllImportResolver(assembly, MapAndLoad);
		}

		#endregion
	}
}

#endif
