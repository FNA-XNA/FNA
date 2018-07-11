#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/dd940262.aspx
	public class AudioEngine : IDisposable
	{
		#region Public Constants

		public const int ContentVersion = 46;

		#endregion

		#region Public Properties

		public ReadOnlyCollection<RendererDetail> RendererDetails
		{
			get
			{
				return new ReadOnlyCollection<RendererDetail>(
					rendererDetails
				);
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Variables

		internal readonly IntPtr handle;
		internal readonly byte[] handle3D;
		internal readonly ushort channels;

		internal readonly object gcSync = new object();

		internal readonly List<WeakReference> sbList;
		internal readonly List<WeakReference> wbList;

		#endregion

		#region Private Variables

		private byte[] buffer;
		private GCHandle pin;

		private RendererDetail[] rendererDetails;

		#endregion

		#region Disposing Event

		public event EventHandler<EventArgs> Disposing;

		#endregion

		#region Public Constructors

		public AudioEngine(
			string settingsFile
		) : this(
			settingsFile,
			new TimeSpan(
				0, 0, 0, 0,
				(int) FAudio.FACT_ENGINE_LOOKAHEAD_DEFAULT
			),
			null
		) {
		}

		public AudioEngine(
			string settingsFile,
			TimeSpan lookAheadTime,
			string rendererId
		) {
			if (String.IsNullOrEmpty(settingsFile))
			{
				throw new ArgumentNullException("settingsFile");
			}

			// Read entire file into memory, pin buffer
			buffer = TitleContainer.ReadAllBytes(settingsFile);
			pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			// Generate engine parameters
			FAudio.FACTRuntimeParameters settings = new FAudio.FACTRuntimeParameters();
			settings.pGlobalSettingsBuffer = pin.AddrOfPinnedObject();
			settings.globalSettingsBufferSize = (uint) buffer.Length;

			// Special parameters from constructor
			settings.lookAheadTime = (uint) lookAheadTime.Milliseconds;
			if (!string.IsNullOrEmpty(rendererId))
			{
				// FIXME: wchar_t? -flibit
				settings.pRendererID = Marshal.StringToHGlobalAuto(rendererId);
			}

			// Init engine, finally
			FAudio.FACTCreateEngine(0, out handle);
			FAudio.FACTAudioEngine_Initialize(handle, ref settings);

			// Free the settings strings
			if (settings.pRendererID != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(settings.pRendererID);
			}

			// Grab RendererDetails
			ushort rendererCount;
			FAudio.FACTAudioEngine_GetRendererCount(
				handle,
				out rendererCount
			);
			if (rendererCount == 0)
			{
				Dispose();
				throw new NoAudioHardwareException();
			}
			rendererDetails = new RendererDetail[rendererCount];
			char[] displayName = new char[0xFF];
			char[] rendererID = new char[0xFF];
			for (ushort i = 0; i < rendererCount; i += 1)
			{
				FAudio.FACTRendererDetails details;
				FAudio.FACTAudioEngine_GetRendererDetails(
					handle,
					i,
					out details
				);
				unsafe
				{
					for (int j = 0; j < 0xFF; j += 1)
					{
						displayName[j] = (char) details.displayName[j];
						rendererID[j] = (char) details.rendererID[j];
					}
				}
				rendererDetails[i] = new RendererDetail(
					new string(displayName),
					new string(rendererID)
				);
			}

			// Init 3D audio
			handle3D = new byte[FAudio.F3DAUDIO_HANDLE_BYTESIZE];
			FAudio.FACT3DInitialize(
				handle,
				handle3D
			);

			// Grab channel count for DSP_SETTINGS
			FAudio.FAudioWaveFormatExtensible mixFormat;
			FAudio.FACTAudioEngine_GetFinalMixFormat(
				handle,
				out mixFormat
			);
			channels = mixFormat.Format.nChannels;

			/* We have to manage our XACT resources, lest we get the GC and the
			 * API thread fighting with one another... hoo boy.
			 */
			 sbList = new List<WeakReference>();
			 wbList = new List<WeakReference>();
		}

		#endregion

		#region Destructor

		~AudioEngine()
		{
			Dispose(false);
		}

		#endregion

		#region Public Dispose Methods

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Public Methods

		public AudioCategory GetCategory(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort category = FAudio.FACTAudioEngine_GetCategory(
				handle,
				name
			);

			if (category == FAudio.FACTCATEGORY_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid category name!"
				);
			}

			return new AudioCategory(this, category, name);
		}

		public float GetGlobalVariable(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid variable name!"
				);
			}

			float result;
			FAudio.FACTAudioEngine_GetGlobalVariable(
				handle,
				variable,
				out result
			);
			return result;
		}

		public void SetGlobalVariable(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			ushort variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new InvalidOperationException(
					"Invalid variable name!"
				);
			}

			FAudio.FACTAudioEngine_SetGlobalVariable(
				handle,
				variable,
				value
			);
		}

		public void Update()
		{
			FAudio.FACTAudioEngine_DoWork(handle);
		}

		#endregion

		#region Protected Methods

		protected virtual void Dispose(bool disposing)
		{
			lock (gcSync)
			{
				if (!IsDisposed)
				{
					if (Disposing != null)
					{
						Disposing.Invoke(this, null);
					}

					/* Deleting in this order, for safety:
					 * 1. Waves (deleted by Cues in native code)
					 * 2. Cues (deleted by SoundBank.Dispose)
					 * 3. SoundBanks, which should have no Cues now
					 * 4. WaveBanks, which should have no references now
					 */
					while (sbList.Count > 0)
					{
						if (sbList[0].Target != null)
						{
							((SoundBank) sbList[0].Target).Dispose();
						}
						else
						{
							sbList.RemoveAt(0);
						}
					}
					while (wbList.Count > 0)
					{
						if (wbList[0].Target != null)
						{
							((WaveBank) wbList[0].Target).Dispose();
						}
						else
						{
							wbList.RemoveAt(0);
						}
					}

					FAudio.FACTAudioEngine_ShutDown(handle);
					pin.Free();
					buffer = null;
					rendererDetails = null;

					IsDisposed = true;
				}
			}
		}

		#endregion
	}
}
