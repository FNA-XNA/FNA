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
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audiocategory.aspx
	public struct AudioCategory : IEquatable<AudioCategory>
	{
		#region Public Properties

		public string Name
		{
			get
			{
				return name;
			}
		}

		#endregion

		#region Internal Variables

		internal AudioEngine parent;
		internal ushort index;
		internal string name;

		#endregion

		#region Public Methods

		public void Pause()
		{
			lock (parent.gcSync)
			{
				if (parent.IsDisposed)
				{
					throw new ArgumentException();
				}
				FAudio.FACTAudioEngine_Pause(parent.handle, index, 1);
			}
		}

		public void Resume()
		{
			lock (parent.gcSync)
			{
				if (parent.IsDisposed)
				{
					throw new ArgumentException();
				}
				FAudio.FACTAudioEngine_Pause(parent.handle, index, 0);
			}
		}

		public void SetVolume(float volume)
		{
			if (volume < FAudio.FACTVOLUME_MIN)
			{
				throw new ArgumentException("Volume must be a positive float value.");
			}
			if (volume > FAudio.FACTVOLUME_MAX)
			{
				throw new ArgumentException();
			}
			lock (parent.gcSync)
			{
				if (parent.IsDisposed)
				{
					throw new ArgumentException();
				}
				FAudio.FACTAudioEngine_SetVolume(parent.handle, index, volume);
			}
		}

		public void Stop(AudioStopOptions options)
		{
			if (unchecked((uint) options) > 1)
			{
				throw new ArgumentException();
			}
			lock (parent.gcSync)
			{
				if (parent.IsDisposed)
				{
					throw new ArgumentException();
				}
				FAudio.FACTAudioEngine_Stop(
					parent.handle,
					index,
					(uint) options
				);
			}
		}

		public override int GetHashCode()
		{
			int hashcode = index;
			if (parent != null)
			{
				hashcode ^= parent.GetHashCode();
			}
			return hashcode;
		}

		public bool Equals(AudioCategory other)
		{
			return other.parent == parent && other.index == index;
		}

		public override bool Equals(object obj)
		{
			return obj is AudioCategory && Equals((AudioCategory) obj);
		}

		public static bool operator ==(
			AudioCategory value1,
			AudioCategory value2
		) {
			return value1.Equals(value2);
		}

		public static bool operator !=(
			AudioCategory value1,
			AudioCategory value2
		) {
			return !(value1.Equals(value2));
		}

		public override string ToString()
		{
			return name ?? string.Empty;
		}

		#endregion
	}
}
