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

		#endregion

		#region Private Variables

		private byte[] buffer;
		private GCHandle pin;

		private RendererDetail[] rendererDetails;

		private readonly FAudio.FACTNotificationCallback xactNotificationFunc;
		private FAudio.FACTNotificationDescription notificationDesc;

		// If this isn't static, destructors gets confused like idiots
		private static readonly Dictionary<IntPtr, WeakReference> xactPtrs = new Dictionary<IntPtr, WeakReference>();

		#endregion

		#region Public Static Variables

		// STOP LEAKING YOUR XACT DATA, GOOD GRIEF PEOPLE
		public static bool ProgramExiting = false;

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
			xactNotificationFunc = OnXACTNotification;
			settings.fnNotificationCallback = Marshal.GetFunctionPointerForDelegate(
				xactNotificationFunc
			);

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

			// All XACT references have to go through here...
			notificationDesc = new FAudio.FACTNotificationDescription();
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

					FAudio.FACTAudioEngine_ShutDown(handle);
					pin.Free();
					buffer = null;
					rendererDetails = null;

					IsDisposed = true;
				}
			}
		}

		#endregion

		#region Internal Methods

		internal void RegisterWaveBank(
			IntPtr ptr,
			WeakReference reference
		) {
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
			notificationDesc.pWaveBank = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
			lock (xactPtrs)
			{
				xactPtrs.Add(ptr, reference);
			}
		}

		internal void RegisterSoundBank(
			IntPtr ptr,
			WeakReference reference
		) {
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
			notificationDesc.pSoundBank = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
			lock (xactPtrs)
			{
				xactPtrs.Add(ptr, reference);
			}
		}

		internal void RegisterCue(
			IntPtr ptr,
			WeakReference reference
		) {
			notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
			notificationDesc.pCue = ptr;
			FAudio.FACTAudioEngine_RegisterNotification(
				handle,
				ref notificationDesc
			);
			lock (xactPtrs)
			{
				xactPtrs.Add(ptr, reference);
			}
		}

		internal void UnregisterWaveBank(IntPtr ptr)
		{
			lock (xactPtrs)
			{
				if (!xactPtrs.ContainsKey(ptr))
				{
					return;
				}
				notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_WAVEBANKDESTROYED;
				notificationDesc.pWaveBank = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(
					handle,
					ref notificationDesc
				);
				xactPtrs.Remove(ptr);
			}
		}

		internal void UnregisterSoundBank(IntPtr ptr)
		{
			lock (xactPtrs)
			{
				if (!xactPtrs.ContainsKey(ptr))
				{
					return;
				}
				notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_SOUNDBANKDESTROYED;
				notificationDesc.pSoundBank = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(
					handle,
					ref notificationDesc
				);
				xactPtrs.Remove(ptr);
			}
		}

		internal void UnregisterCue(IntPtr ptr)
		{
			lock (xactPtrs)
			{
				if (!xactPtrs.ContainsKey(ptr))
				{
					return;
				}
				notificationDesc.type = FAudio.FACTNOTIFICATIONTYPE_CUEDESTROYED;
				notificationDesc.pCue = ptr;
				FAudio.FACTAudioEngine_UnRegisterNotification(
					handle,
					ref notificationDesc
				);
				xactPtrs.Remove(ptr);
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
	}
}
