#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class DllMap
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetDefaultDllDirectories(int directoryFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern void AddDllDirectory(string lpPathName);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetDllDirectory(string lpPathName);

		const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

		public static void Initialize()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				try
				{
					SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
					AddDllDirectory(Path.Combine(
						AppDomain.CurrentDomain.BaseDirectory,
						Environment.Is64BitProcess ? "x64" : "x86"
					));
				}
				catch
				{
					// Pre-Windows 7, KB2533623
					SetDllDirectory(Path.Combine(
						AppDomain.CurrentDomain.BaseDirectory,
						Environment.Is64BitProcess ? "x64" : "x86"
					));
				}
			}
#if NETCOREAPP3_0
			/* .NET Core doesn't support dllmap, so we'll need
			 * to handle native library name resolution ourselves.
			 */
			DllMap.Register();
#endif
		}

#if NETCOREAPP3_0
		private static string OS;
		private static string CPU;
		private static Dictionary<string, string> MapDictionary;

		private static void Register()
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			NativeLibrary.SetDllImportResolver(assembly, MapAndLoad);

			// Get platform and CPU
			OS = GetCurrentPlatform();
			CPU = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

			// Read config XML and store details within MapDictionary
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
					// Oh well!
					return;
				}
			}

			MapDictionary = new Dictionary<string, string>();

			XElement root = XElement.Load(xmlPath);
			ParseXml(root);
		}

		// The callback which loads the mapped library in place of the original
		private static IntPtr MapAndLoad(
			string libraryName,
			Assembly assembly,
			DllImportSearchPath? dllImportSearchPath
		) {
			string mappedName;
			if (!MapDictionary.TryGetValue(libraryName, out mappedName))
			{
				mappedName = libraryName;
			}
			return NativeLibrary.Load(mappedName, assembly, dllImportSearchPath);
		}

		private static void ParseXml(XElement root)
		{
			foreach (XElement el in root.Elements("dllmap"))
			{
				// Ignore entries for other OSs
				if (!el.Attribute("os").ToString().Contains(OS))
				{
					continue;
				}

				XAttribute cpuAttribute = el.Attribute("cpu");
				if (cpuAttribute != null)
				{
					// Ignore entries for other CPUs
					if (!cpuAttribute.ToString().Contains(CPU))
					{
						continue;
					}
				}

				string oldLib = el.Attribute("dll").Value;
				string newLib = el.Attribute("target").Value;
				if (string.IsNullOrWhiteSpace(oldLib) || string.IsNullOrWhiteSpace(newLib))
				{
					continue;
				}

				// Don't allow duplicates
				if (MapDictionary.ContainsKey(oldLib))
				{
					continue;
				}

				MapDictionary.Add(oldLib, newLib);
			}
		}

		private static string GetCurrentPlatform()
		{
			string[] platformNames = new string[]
			{
				"LINUX",
				"OSX",
				"WINDOWS",
				"FREEBSD",
				"NETBSD",
				"OPENBSD"
			};

			OSPlatform[] platforms = new OSPlatform[platformNames.Length];
			for (int i = 0; i < platforms.Length; i += 1)
			{
				platforms[i] = OSPlatform.Create(platformNames[i]);
				if (RuntimeInformation.IsOSPlatform(platforms[i]))
				{
					return platformNames[i].ToLowerInvariant();
				}
			}

			// Uhhh, we'll just hope for the best! -caleb
			return "unknown";
		}
#endif
	}
}