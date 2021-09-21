#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#if NET

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Xml;
#endregion

namespace Microsoft.Xna.Framework
{
	internal static class NativeLibraryResolver
	{
		const string LibNameFAudio = "FAudio";
		const string LibNameFNA3D = "FNA3D";
		const string LibNameSDL2 = "SDL2";
		const string SDL2Version = "-2.0";
		const string LibNameTheorafile = "libtheorafile";

		private static IntPtr ResolveLibrary(
			string libraryName,
			Assembly assembly,
			DllImportSearchPath? dllImportSearchPath
		) {
			string libraryFileName = "";
			switch (libraryName)
			{
				case LibNameFAudio:
					libraryFileName = GetLibraryFileName(LibNameFAudio);
					break;
				case LibNameFNA3D:
					libraryFileName = GetLibraryFileName(LibNameFNA3D);
					break;
				case LibNameSDL2:
					libraryFileName = GetLibraryFileName(LibNameSDL2);
					break;
				case LibNameTheorafile:
					libraryFileName = GetLibraryFileName(LibNameTheorafile);
					break;
				default: return IntPtr.Zero;
			}

			IntPtr handle;
			bool success;

			string rootDirectory = AppContext.BaseDirectory;

			if (OperatingSystem.IsWindows())
			{
#if WINDOWS10_0_17763_0_OR_GREATER
				string arch = Environment.Is64BitProcess ? "win10-x64" : "win10-x86";
#else
				string arch = Environment.Is64BitProcess ? "win-x64" : "win-x86";
#endif
				var searchPaths = new[]
				{
					// This is where native libraries in our nupkg should end up
					Path.Combine(rootDirectory, "runtimes", arch, "native", libraryFileName),
					Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libraryFileName),
					Path.Combine(rootDirectory, libraryFileName)
				};

				foreach (var path in searchPaths)
				{
					success = NativeLibrary.TryLoad(path, out handle);
					if (success)
						return handle;
				}

				throw new FileLoadException($"Failed to load native library: {libraryName}!");
			}

			if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
			{
				string arch = OperatingSystem.IsMacOS() ? "osx-x64" : "linux-x64";

				var searchPaths = new[]
				{
					// This is where native libraries in our nupkg should end up
					Path.Combine(rootDirectory, "runtimes", arch, "native", libraryFileName),
					// The build output folder
					Path.Combine(rootDirectory, libraryFileName),
					Path.Combine("/usr/local/lib", libraryFileName),
					Path.Combine("/usr/lib", libraryFileName)
				};

				foreach (var path in searchPaths)
				{
					success = NativeLibrary.TryLoad(path, out handle);
					if (success)
						return handle;
				}

				throw new FileLoadException($"Failed to load native library: {libraryName}!");
			}

			throw new FileLoadException($"Failed to load native library: {libraryName}!");
		}

		private static string GetLibraryFileName(string libraryName)
		{
			string prefix = OperatingSystem.IsWindows() ? "" : "lib";
			string suffix = ".dll";
			if (OperatingSystem.IsWindows())
				suffix = ".dll";
			else if (OperatingSystem.IsMacOS())
				suffix = ".dylib";
			else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
				suffix = ".so";

			switch (libraryName)
			{
				case LibNameFAudio:
				case LibNameFNA3D:
					return prefix + libraryName + suffix;
				case LibNameSDL2:
				{
					if (OperatingSystem.IsWindows())
						return prefix + libraryName + suffix;
					else
						return prefix + libraryName + SDL2Version + suffix;
				}
				case LibNameTheorafile: // already prefixed
					return libraryName + suffix;
			}

			return "";
		}

		public static void Setup()
		{
			// Get the platform and architecture
			string cpu = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
			string wordsize = (IntPtr.Size * 8).ToString();

			// Set the resolver callback
#if !__IOS__ && !__TVOS__
			NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, ResolveLibrary);
#endif
		}
	}
}

#endif // NET
