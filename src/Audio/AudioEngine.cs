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

		#endregion

		#region Private Variables

		private RendererDetail[] rendererDetails;

		private readonly FAudio.FACTNotificationCallback xactNotificationFunc;
		private FAudio.FACTNotificationDescription notificationDesc;

		private class IntPtrComparer : IEqualityComparer<IntPtr>
		{
			public bool Equals(IntPtr x, IntPtr y)
			{
				return x == y;
			}

			public int GetHashCode(IntPtr obj)
			{
				return obj.GetHashCode();
			}
		}

		private static readonly IntPtrComparer comparer = new IntPtrComparer();

		// If this isn't static, destructors gets confused like idiots
		private static readonly Dictionary<IntPtr, WeakReference> xactPtrs = new Dictionary<IntPtr, WeakReference>(comparer);

		#endregion

		#region Public Static Variables

		// STOP LEAKING YOUR XACT DATA, GOOD GRIEF PEOPLE
		internal static bool ProgramExiting = false;

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
			if (string.IsNullOrEmpty(settingsFile))
			{
				throw new ArgumentNullException("settingsFile", "This method does not accept null for this parameter.");
			}

			// Allocate (but don't initialize just yet!)
			FAudio.FACTCreateEngine(0, out handle);

			// Grab RendererDetails
			ushort rendererCount;
			FAudio.FACTAudioEngine_GetRendererCount(
				handle,
				out rendererCount
			);
			if (rendererCount == 0)
			{
				FAudio.FACTAudioEngine_Release(handle);
				throw new NoAudioHardwareException();
			}
			rendererDetails = new RendererDetail[rendererCount];
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
					rendererDetails[i].FriendlyName = new string((char*) details.displayName);
					rendererDetails[i].RendererId = new string((char*) details.rendererID);
				}
			}

			// Read entire file into memory, let FACT manage the pointer
			IntPtr bufferLen;
			IntPtr buffer = TitleContainer.ReadToPointer(settingsFile, out bufferLen);

			// Generate engine parameters
			FAudio.FACTRuntimeParameters settings = new FAudio.FACTRuntimeParameters();
			settings.pGlobalSettingsBuffer = buffer;
			settings.globalSettingsBufferSize = (uint) bufferLen;
			settings.globalSettingsFlags = FAudio.FACT_FLAG_MANAGEDATA;
			xactNotificationFunc = OnXACTNotification;
			settings.fnNotificationCallback = Marshal.GetFunctionPointerForDelegate(
				xactNotificationFunc
			);

			// Special parameters from constructor
			settings.lookAheadTime = (uint) lookAheadTime.Milliseconds;
			if (!string.IsNullOrEmpty(rendererId))
			{
				settings.pRendererID = Marshal.StringToHGlobalUni(rendererId);
			}

			// Init engine, finally
			uint ret = FAudio.FACTAudioEngine_Initialize(handle, ref settings);

			// Free the settings strings
			if (settings.pRendererID != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(settings.pRendererID);
			}

			if (ret == 0x8ac70007) // FACTENGINE_E_INVALIDDATA
			{
				FAudio.FACTAudioEngine_Release(handle);
				throw new ArgumentException("XACT could not load the data provided. Make sure you are using the correct version of the XACT tool.");
			}
			else if (ret != 0)
			{
				FAudio.FACTAudioEngine_Release(handle);
				throw new InvalidOperationException(
					"Engine initialization failed!"
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

			// All XACT references have to go through here...
			notificationDesc = new FAudio.FACTNotificationDescription();
			notificationDesc.flags = FAudio.FACT_FLAG_NOTIFICATION_PERSIST;
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
		}

		#endregion

		#region Static Constructor

		static AudioEngine()
		{
			AppDomain.CurrentDomain.ProcessExit += ProgramExit;
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
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name", "This method does not accept null for this parameter.");
			}

			AudioCategory category;
			category.index = FAudio.FACTAudioEngine_GetCategory(
				handle,
				name
			);

			if (category.index == FAudio.FACTCATEGORY_INVALID)
			{
				throw new InvalidOperationException("This resource could not be created.");
			}

			category.parent = this;
			category.name = name;
			return category;
		}

		public float GetGlobalVariable(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name", "This method does not accept null for this parameter.");
			}

			ushort variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new IndexOutOfRangeException("The specified variable index is invalid.");
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
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name", "This method does not accept null for this parameter.");
			}

			ushort variable = FAudio.FACTAudioEngine_GetGlobalVariableIndex(
				handle,
				name
			);

			if (variable == FAudio.FACTVARIABLEINDEX_INVALID)
			{
				throw new IndexOutOfRangeException("The specified variable index is invalid.");
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
					FAudio.FACTAudioEngine_ShutDown(handle);
					FAudio.FACTAudioEngine_Release(handle);
					rendererDetails = null;

					IsDisposed = true;
					if (disposing && Disposing != null)
					{
						Disposing(this, EventArgs.Empty);
					}
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void RegisterPointer(
			IntPtr ptr,
			WeakReference reference
		) {
			lock (xactPtrs)
			{
				xactPtrs[ptr] = reference;
			}
		}

		#endregion

		#region Private Methods

		[ObjCRuntime.MonoPInvokeCallback(typeof(FAudio.FACTNotificationCallback))]
		private static unsafe void OnXACTNotification(IntPtr notification)
		{
			WeakReference reference;
			FAudio.FACTNotification* not = (FAudio.FACTNotification*) notification;
			if (not->type == FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED)
			{
				IntPtr target = not->anon.waveBank.pWaveBank;
				lock (xactPtrs)
				{
					if (xactPtrs.TryGetValue(target, out reference))
					{
						if (reference.IsAlive)
						{
							(reference.Target as WaveBank).OnWaveBankDestroyed();
						}
					}
					xactPtrs.Remove(target);
				}
			}
			else if (not->type == FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED)
			{
				IntPtr target = not->anon.soundBank.pSoundBank;
				lock (xactPtrs)
				{
					if (xactPtrs.TryGetValue(target, out reference))
					{
						if (reference.IsAlive)
						{
							(reference.Target as SoundBank).OnSoundBankDestroyed();
						}
					}
					xactPtrs.Remove(target);
				}
			}
			else if (not->type == FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED)
			{
				IntPtr target = not->anon.cue.pCue;
				lock (xactPtrs)
				{
					if (xactPtrs.TryGetValue(target, out reference))
					{
						if (reference.IsAlive)
						{
							(reference.Target as Cue).OnCueDestroyed();
						}
					}
					xactPtrs.Remove(target);
				}
			}
		}

		#endregion

		#region Internal Static Methods

		internal static void ProgramExit(object sender, EventArgs e)
		{
			ProgramExiting = true;
		}

		#endregion
	}
}
